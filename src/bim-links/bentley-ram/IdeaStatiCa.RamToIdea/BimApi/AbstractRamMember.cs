﻿using IdeaRS.OpenModel.Geometry3D;
using IdeaRS.OpenModel.Model;
using IdeaStatiCa.BimApi;
using IdeaStatiCa.BimApi.Results;
using IdeaStatiCa.RamToIdea.Factories;
using IdeaStatiCa.RamToIdea.Geometry;
using IdeaStatiCa.RamToIdea.Model;
using IdeaStatiCa.RamToIdea.Sections;
using IdeaStatiCa.RamToIdea.Utilities;
using RAMDATAACCESSLib;
using System;
using System.Collections.Generic;

namespace IdeaStatiCa.RamToIdea.BimApi
{
	/// <inheritdoc cref="IIdeaMember1D"/>
	internal abstract class AbstractRamMember : IIdeaMember1D
	{
		public Member1DType Type
		{
			get
			{
				switch (MemberType)
				{
					case MemberType.Beam:
					case MemberType.VerticalBrace:
					case MemberType.HorizontalBrace:
						return Member1DType.Beam;

					case MemberType.Column:
						return Member1DType.Column;
				}

				throw new NotImplementedException();
			}
		}

		private readonly Lazy<List<IIdeaElement1D>> _elements;
		public List<IIdeaElement1D> Elements => _elements.Value;

		public string Id => $"member-{UID}";

		public string Name
		{
			get
			{
				if (Properties.Story == 0)
				{
					return Properties.Label.ToString();
				}

				return $"{_objectFactory.GetStory(Properties.Story).lLevel}-{Properties.Label}";
			}
		}

		public IIdeaPersistenceToken Token => null;

		public abstract int UID { get; }

		public abstract MemberType MemberType { get; }

		protected abstract RamMemberProperties Properties { get; }

		protected IResultsFactory ResultsFactory { get; }

		private Line _line;
		private readonly IObjectFactory _objectFactory;
		private readonly ISectionFactory _sectionProvider;
		private readonly IGeometry _geometry;
		private readonly ISegmentFactory _segmentFactory;

		public AbstractRamMember(IObjectFactory objectFactory, ISectionFactory sectionProvider, IResultsFactory resultsFactory, IGeometry geometry, ISegmentFactory segmentFactory)
		{
			_objectFactory = objectFactory;
			ResultsFactory = resultsFactory;
			_sectionProvider = sectionProvider;
			_geometry = geometry;
			_segmentFactory = segmentFactory;

			_elements = new Lazy<List<IIdeaElement1D>>(CreateElements);
		}

		protected void Init()
		{
			_line = CreateLine();
		}

		public abstract IEnumerable<IIdeaResult> GetResults();

		private List<IIdeaElement1D> CreateElements()
		{
			IRamSection section = _sectionProvider.GetSection(Properties);

			IdeaVector3D offset;
			if (MemberType == MemberType.Beam)
			{
				offset = new IdeaVector3D(0, 0, -section.Height / 2);
			}
			else
			{
				offset = new IdeaVector3D(0, 0, 0);
			}

			List<IIdeaElement1D> elements = new List<IIdeaElement1D>();

			foreach (RamLineSegment3D segment in _segmentFactory.CreateSegments(_line))
			{
				var cs = (CoordSystemByVector)segment.LocalCoordinateSystem;
				var offset2 = new IdeaVector3D(cs.VecX.Z * offset.Z, cs.VecY.Z * offset.Z, cs.VecZ.Z * offset.Z);

				RamElement1D element = new RamElement1D()
				{
					Segment = segment,
					StartCrossSection = section,
					EndCrossSection = section,
					RotationRx = Properties.Rotation.DegreesToRadians(),
					EccentricityBegin = offset2,
					EccentricityEnd = offset2
				};

				elements.Add(element);
			}

			return elements;
		}

		protected virtual Line CreateLine()
		{
			(SCoordinate start, SCoordinate end) = GetStartEndCoordinates();
			return _geometry.CreateLine(start, end, Properties.CanBeSubdivided);
		}

		protected abstract (SCoordinate, SCoordinate) GetStartEndCoordinates();
	}
}