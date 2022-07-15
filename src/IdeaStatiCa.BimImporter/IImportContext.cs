﻿using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Connection;
using IdeaRS.OpenModel.Result;
using IdeaStatiCa.BimApi;
using IdeaStatiCa.BimImporter.BimItems;

namespace IdeaStatiCa.BimImporter
{
	internal interface IImportContext
	{
		OpenModel OpenModel { get; }

		OpenModelResult OpenModelResult { get; }

		BimImporterConfiguration Configuration { get; }

		ReferenceElement Import(IIdeaObject obj);

		object ImportConnectionItem(IIdeaObject obj, ConnectionData connectionData);

		void ImportBimItem(IBimItem bimItem);
	}
}