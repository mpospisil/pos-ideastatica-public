﻿using System;

namespace IdeaStatiCa.Plugin.Grpc.Reflection
{
	public interface IMethodInvoker
	{
		/// <summary>
		/// Invokes remote method over Grpc via reflection.
		/// </summary>
		/// <typeparam name="T">Type to which the result will be converted.</typeparam>
		/// <param name="methodName">Name of the method to invoke.</param>
		/// <param name="returnType">Type of the value which is return by the method</param>
		/// <param name="arguments">Arguments with which the method will be called.</param>
		/// <returns></returns>
		T InvokeMethod<T>(string methodName, Type returnType, params object[] arguments);
	}
}
