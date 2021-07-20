﻿using IdeaStatiCa.BimApi;

namespace IdeaStatiCa.BimImporter
{
	/// <summary>
	/// Stores and manages mapping between <see cref="IIdeaObject.Id"/> and <see cref="IdeaRS.OpenModel.OpenElementId.Id"/>.
	/// </summary>
	public interface IProject
	{
		/// <summary>
		/// Returns IOM id for given <paramref name="bimId"/>.
		/// </summary>
		/// <param name="bimId">BimApi id</param>
		/// <returns>IOM id</returns>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Throws when no mapping
		/// for given <paramref name="bimId"/> exists.</exception>
		int GetIomId(string bimApiId);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="iomId"></param>
		/// <returns></returns>
		string GetBimApiId(int iomId);

		/// <summary>
		/// Returns BimApi object for given IOM <paramref name="id"/>.
		/// </summary>
		/// <param name="id">IOM id</param>
		/// <returns><see cref="IIdeaObject"/> object</returns>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Throws when no mapping
		/// for given <paramref name="id"/> exists.</exception>
		IIdeaObject GetBimObject(int id);

		/// <summary>
		/// For given BimApi object returns IOM id or assigns a new one and returns it.
		/// </summary>
		/// <param name="obj">BimApi object</param>
		/// <returns>IOM id</returns>
		int GetIomId(IIdeaObject obj);

		IIdeaPersistenceToken GetPersistenceToken(int iomId);
	}
}