﻿using IdeaStatica.BimApiLink.Importers;
using IdeaStatica.BimApiLink.Persistence;
using IdeaStatica.BimApiLink.Plugin;
using IdeaStatiCa.BimImporter;
using IdeaStatiCa.BimImporter.Persistence;
using IdeaStatiCa.BimImporter.Results;
using IdeaStatiCa.Plugin;

namespace IdeaStatica.BimApiLink
{
	public abstract class BimLink
	{
		protected string ApplicationName { get; }

		private string? _ideaStatiCaPath;
		private IPluginLogger? _pluginLogger;
		private GrpcBimHostingFactory? _bimHosting;
		private ResultsImportersConfiguration? _resultsImportersConfiguration;
		private BimImporterConfiguration? _bimImporterConfiguration;
		private readonly string _projectPath;

		private readonly ImportersConfiguration _importersConfiguration = new();

		public static BimLink Create(string applicationName, string checkbotProjectPath) => new FeaBimLink(applicationName, checkbotProjectPath);

		protected BimLink(string applicationName, string projectPath)
		{
			ApplicationName = applicationName;
			_projectPath = projectPath;
		}

		public BimLink WithIdeaStatiCa(string path)
		{
			_ideaStatiCaPath = path;
			return this;
		}

		public BimLink WithLogger(IPluginLogger pluginLogger)
		{
			_pluginLogger = pluginLogger;
			return this;
		}

		public BimLink WithImporters(Action<ImportersConfiguration> func)
		{
			func(_importersConfiguration);
			return this;
		}

		public BimLink WithResultsImporters(Action<ResultsImportersConfiguration> func)
		{
			ResultsImportersConfiguration conf = new();
			func(conf);
			_resultsImportersConfiguration = conf;

			return this;
		}

		public BimLink WithBimImporterConfiguration(BimImporterConfiguration configuration)
		{
			_bimImporterConfiguration = configuration;
			return this;
		}

		public IProgressMessaging InitHostingClient(IPluginLogger pluginLogger)
		{
			_bimHosting ??= new GrpcBimHostingFactory();

			return _bimHosting.InitGrpcClient(pluginLogger);
		}

		public Task Run(IFeaModel feaModel)
		{
			ImporterDispatcher importerDispatcher = new(_importersConfiguration.Manager);

			IPluginLogger pluginLogger = _pluginLogger ?? new NullLogger();
			IBimResultsProvider resultsProvider = _resultsImportersConfiguration?.ResultsProvider ?? new DefaultResultsProvider();
			BimImporterConfiguration bimImporterConfiguration = _bimImporterConfiguration ?? new BimImporterConfiguration();

			IApplicationBIM applicationBIM = Create(
				pluginLogger,
				importerDispatcher,
				_projectPath,
				bimImporterConfiguration,
				InitHostingClient(pluginLogger),
				resultsProvider,
				feaModel);

			PluginFactory pluginFactory = new(
				applicationBIM,
				ApplicationName,
				_ideaStatiCaPath);

			if (_bimHosting is null)
			{
				InitHostingClient(pluginLogger);
			}

			IBIMPluginHosting pluginHosting = _bimHosting.Create(pluginFactory, pluginLogger);

			string pid = Environment.ProcessId.ToString();
			return pluginHosting.RunAsync(pid, _projectPath);
		}

		protected abstract IApplicationBIM Create(
			IPluginLogger logger,
			IBimApiImporter bimApiImporter,
			string projectPath,
			BimImporterConfiguration bimImporterConfiguration,
			IProgressMessaging remoteApp,
			IBimResultsProvider resultsProvider,
			IFeaModel feaModel);
	}

	internal class FeaBimLink : BimLink
	{
		public FeaBimLink(string applicationName, string projectPath) : base(applicationName, projectPath)
		{
		}

		protected override IApplicationBIM Create(
			IPluginLogger logger,
			IBimApiImporter bimApiImporter,
			string projectPath,
			BimImporterConfiguration bimImporterConfiguration,
			IProgressMessaging remoteApp,
			IBimResultsProvider resultsProvider,
			IFeaModel feaModel)
		{
			JsonPersistence jsonPersistence = new();
			JsonProjectStorage projectStorage = new(jsonPersistence, projectPath);
			Project project = new(logger, jsonPersistence);
			ProjectAdapter projectAdapter = new(project, bimApiImporter);
			FeaModelAdapter feaModelAdapter = new(bimApiImporter, feaModel);
			IBimImporter bimImporter = BimImporter.Create(
				feaModelAdapter,
				projectAdapter,
				logger,
				null,
				bimImporterConfiguration,
				remoteApp,
				resultsProvider);

			return new FeaApplication(ApplicationName, projectAdapter, projectStorage, bimImporter, bimApiImporter);
		}
	}
}