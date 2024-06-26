﻿using FluentAssertions;
using IdeaRS.OpenModel;
using IdeaRS.OpenModel.Model;
using IdeaStatiCa.BimApi;
using IdeaStatiCa.BimImporter.Importers;
using IdeaStatiCa.BimImporter.Tests.Helpers;
using IdeaStatiCa.Plugin;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace IdeaStatiCa.BimImporter.Tests.Importers
{
	[TestFixture]
	public class MemberImporterTest
	{
		[Test]
		public void MemberImport()
		{
			// Setup
			ImportContextBuilder ctxBuilder = new ImportContextBuilder();
			(var segment1, _) = ctxBuilder.Add<IIdeaSegment3D>();
			(var segment2, _) = ctxBuilder.Add<IIdeaSegment3D>();

			var node1 = MockHelper.CreateNode(0, 0, 0);
			var node2 = MockHelper.CreateNode(1, 1, 1);
			var node3 = MockHelper.CreateNode(2, 2, 2);

			segment1.StartNode.Returns(node1);
			segment1.EndNode.Returns(node2);
			segment2.StartNode.Returns(node2);
			segment2.EndNode.Returns(node3);

			var element1 = MockHelper.CreateElement("elm1", segment1);
			var refElement1 = ctxBuilder.Add(element1);
			var element2 = MockHelper.CreateElement("elm2", segment2);
			var refElement2 = ctxBuilder.Add(element2);

			IIdeaMember1D member = Substitute.For<IIdeaMember1D>();
			member.Id.Returns("member1");
			member.Name.Returns("member1");
			member.Type.Returns(Member1DType.Beam);
			member.Elements.Returns(new List<IIdeaElement1D>() {
				element1, element2
			});

			MemberImporter memberImporter = new MemberImporter(new NullLogger());

			// Tested method
			OpenElementId iomObject = memberImporter.Import(ctxBuilder.Context, member);

			// Assert
			Assert.That(iomObject, Is.InstanceOf<Member1D>());
			Member1D iomMember = (Member1D)iomObject;

			Assert.That(iomMember.Name, Is.EqualTo("member1"));
			Assert.That(iomMember.Member1DType, Is.EqualTo(Member1DType.Beam));
			Assert.That(iomMember.Elements1D, Is.EquivalentTo(new[] { refElement1, refElement2 }));
		}

		[Test]
		public void MemberImport_WhenElementsIsEmpty_ThrowsConstraintException()
		{
			// Setup
			ImportContext ctx = new ImportContext(null, null, null, new NullLogger(),
				new BimImporterConfiguration());

			IIdeaMember1D member = Substitute.For<IIdeaMember1D>();
			member.Id.Returns("member1");
			member.Elements.Returns(new List<IIdeaElement1D>());

			MemberImporter memberImporter = new MemberImporter(new NullLogger());

			// Tested method
			Assert.That(() => memberImporter.Import(ctx, member), Throws.TypeOf<ConstraintException>());
		}

		[Test]
		public void MemberImport_WhenStartAndEndNodeOfTwoConsecutiveElementsArentEqual_ThrowsConstraintException()
		{
			// Setup
			ImportContextBuilder ctxBuilder = new ImportContextBuilder();
			(var segment1, _) = ctxBuilder.Add<IIdeaSegment3D>();
			(var segment2, _) = ctxBuilder.Add<IIdeaSegment3D>();

			var node1 = MockHelper.CreateNode(0, 0, 0);
			var node2 = MockHelper.CreateNode(1, 1, 1);
			var node3 = MockHelper.CreateNode(2, 2, 2);
			var node4 = MockHelper.CreateNode(3, 3, 3);

			segment1.StartNode.Returns(node1);
			segment1.EndNode.Returns(node2);
			segment2.StartNode.Returns(node3);
			segment2.EndNode.Returns(node4);

			var element1 = MockHelper.CreateElement("elm1", segment1);
			ctxBuilder.Add(element1);
			var element2 = MockHelper.CreateElement("elm2", segment2);
			ctxBuilder.Add(element2);

			IIdeaMember1D member = Substitute.For<IIdeaMember1D>();
			member.Id.Returns("member1");
			member.Elements.Returns(new List<IIdeaElement1D>() {
				element1, element2
			});

			MemberImporter memberImporter = new MemberImporter(new NullLogger());

			// Tested method
			Assert.That(() => memberImporter.Import(ctxBuilder.Context, member), Throws.TypeOf<ConstraintException>());
		}

		[Test]
		public void MemberImport_WhenAnElementIsDuplicated_ThrowsConstraintException()
		{
			// Setup
			ImportContextBuilder ctxBuilder = new ImportContextBuilder();
			(var segment1, _) = ctxBuilder.Add<IIdeaSegment3D>();

			var node1 = MockHelper.CreateNode(0, 0, 0);
			var node2 = MockHelper.CreateNode(1, 1, 1);

			segment1.StartNode.Returns(node1);
			segment1.EndNode.Returns(node2);

			var element1 = MockHelper.CreateElement("elm1", segment1);
			ctxBuilder.Add(element1);

			IIdeaMember1D member = Substitute.For<IIdeaMember1D>();
			member.Id.Returns("member1");
			member.Elements.Returns(new List<IIdeaElement1D>() {
				element1, element1
			});

			MemberImporter memberImporter = new MemberImporter(new NullLogger());

			// Tested method
			Assert.That(() => memberImporter.Import(ctxBuilder.Context, member), Throws.TypeOf<ConstraintException>());
		}

		[Test]
		public void MemberImport_WhenCrossSectionIsNull_ThenUsesCrossSectionFromFirstElement()
		{
			// Setup
			ImportContextBuilder ctxBuilder = new ImportContextBuilder();
			(var css, var refCss) = ctxBuilder.Add<IIdeaCrossSection>();

			var element = MockHelper.CreateElement();
#pragma warning disable CS0618 // Type or member is obsolete
			element.StartCrossSection.Returns(css);
#pragma warning restore CS0618 // Type or member is obsolete
			ctxBuilder.Add(element);

			IIdeaMember1D member = Substitute.For<IIdeaMember1D>();
			member.Id.Returns("member1");
			member.Elements.Returns(new List<IIdeaElement1D>() {
				element
			});
			member.CrossSection.Returns((IIdeaCrossSection)null);

			MemberImporter memberImporter = new MemberImporter(new NullLogger());

			// Tested method
			var memberIom = (Member1D)memberImporter.Import(ctxBuilder.Context, member);
			memberIom.CrossSection.Should().BeSameAs(refCss);
		}

		[Test]
		public void MemberImport_WhenMemberAndElementHasCrossSection_ThenUseCrossSectionFromMember()
		{
			// Setup
			ImportContextBuilder ctxBuilder = new ImportContextBuilder();
			(var css1, var refCss1) = ctxBuilder.Add<IIdeaCrossSection>();
			(var css2, var refCss2) = ctxBuilder.Add<IIdeaCrossSection>();

			var element = MockHelper.CreateElement();
#pragma warning disable CS0618 // Type or member is obsolete
			element.StartCrossSection.Returns(css1);
#pragma warning restore CS0618 // Type or member is obsolete
			ctxBuilder.Add(element);

			IIdeaMember1D member = Substitute.For<IIdeaMember1D>();
			member.Id.Returns("member1");
			member.Elements.Returns(new List<IIdeaElement1D>() {
				element
			});
			member.CrossSection.Returns(css2);

			MemberImporter memberImporter = new MemberImporter(new NullLogger());

			// Tested method
			var memberIom = (Member1D)memberImporter.Import(ctxBuilder.Context, member);
			memberIom.CrossSection.Should().BeSameAs(refCss2);
		}

		[Test]
		public void MemberImport_WhenNoCrossSectionIsSet_ThrowsContraintException()
		{
			// Setup
			ImportContextBuilder ctxBuilder = new ImportContextBuilder();

			var element = MockHelper.CreateElement();
#pragma warning disable CS0618 // Type or member is obsolete
			element.StartCrossSection.Returns((IIdeaCrossSection)null);
#pragma warning restore CS0618 // Type or member is obsolete
			ctxBuilder.Add(element);

			IIdeaMember1D member = Substitute.For<IIdeaMember1D>();
			member.Id.Returns("member1");
			member.Elements.Returns(new List<IIdeaElement1D>() {
				element
			});
			member.CrossSection.Returns((IIdeaCrossSection)null);

			MemberImporter memberImporter = new MemberImporter(new NullLogger());

			// Tested method
			memberImporter.Invoking(x => x.Import(ctxBuilder.Context, member))
				.Should().Throw<ConstraintException>();
		}
	}
}