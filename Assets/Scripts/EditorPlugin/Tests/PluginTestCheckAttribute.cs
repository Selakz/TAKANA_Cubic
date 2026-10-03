#nullable enable

using System;

namespace EditorPlugin.Tests
{
	/// <summary>
	/// Marks a static method as the C# side check of one scenario of the test plugin. The method must be a
	/// <c>static void</c> method taking a single <see cref="PluginTestCheckContext" />.
	/// See <see cref="PluginSystemChecks" /> for how the marked methods are collected.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
	public sealed class PluginTestCheckAttribute : Attribute
	{
		/// <summary> Name of the scenario, as declared by harness.scenario in the test plugin. </summary>
		public string Scenario { get; }

		public PluginTestCheckAttribute(string scenario) => Scenario = scenario;
	}
}
