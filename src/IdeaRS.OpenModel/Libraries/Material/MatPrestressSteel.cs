﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IdeaRS.OpenModel.Material
{
	/// <summary>
	/// Material prestressing steel base class
	/// </summary>
	[XmlInclude(typeof(MatPrestressSteelEc2))]
	[XmlInclude(typeof(MatPrestressSteelSIA))]
	public abstract class MatPrestressSteel : Material
	{

		/// <summary>
		/// Diameter
		/// </summary>
		public double Diameter { get; set; }

		/// <summary>
		/// Area
		/// </summary>
		public double Area { get; set; }

		/// <summary>
		/// number of wires in strand
		/// </summary>
		public int NumberOfWires { get; set; }

		/// <summary>
		/// Equivalent diameter
		/// </summary>
		public double EquivalentDiameter { get; set; }

	}
}
