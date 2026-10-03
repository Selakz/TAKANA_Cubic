#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace EditorPlugin.Tests
{
	/// <summary>
	/// Aggregation point of the C# side checks. The checks themselves live in <c>EditorPlugin/Tests/Cases</c>, one
	/// file per case file of the test plugin, and are discovered by scanning this assembly for methods marked with
	/// <see cref="PluginTestCheckAttribute" />, so a new case file never requires editing this one.
	/// A scenario without a C# check is allowed: the TypeScript assertions may already cover it.
	/// </summary>
	public static class PluginSystemChecks
	{
		private static readonly List<string> errors = new();
		private static readonly Dictionary<string, Action<PluginTestCheckContext>> byScenario = Discover();

		/// <summary> C# checks by scenario name. </summary>
		public static IReadOnlyDictionary<string, Action<PluginTestCheckContext>> ByScenario => byScenario;

		/// <summary> Problems found while discovering the checks; the runner reports them as run errors. </summary>
		public static IReadOnlyList<string> Errors => errors;

		private static Dictionary<string, Action<PluginTestCheckContext>> Discover()
		{
			var result = new Dictionary<string, Action<PluginTestCheckContext>>();
			Type[] types;
			try
			{
				types = typeof(PluginSystemChecks).Assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				errors.Add($"failed to scan the assembly for C# checks: {e.Message}");
				return result;
			}

			foreach (var type in types)
			{
				foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
				                                       BindingFlags.Static | BindingFlags.Instance |
				                                       BindingFlags.DeclaredOnly))
				{
					foreach (var attribute in method.GetCustomAttributes<PluginTestCheckAttribute>())
					{
						Register(result, type, method, attribute.Scenario);
					}
				}
			}

			return result;
		}

		private static void Register(Dictionary<string, Action<PluginTestCheckContext>> result, Type type,
			MethodInfo method, string scenario)
		{
			var parameters = method.GetParameters();
			if (!method.IsStatic || method.ReturnType != typeof(void) || method.IsGenericMethodDefinition ||
			    parameters.Length != 1 || parameters[0].ParameterType != typeof(PluginTestCheckContext))
			{
				errors.Add($"PluginTestCheck on {type.Name}.{method.Name} must be a static void method taking a " +
				           $"single {nameof(PluginTestCheckContext)}");
				return;
			}

			if (result.ContainsKey(scenario))
			{
				errors.Add($"duplicated C# check for scenario {scenario} ({type.Name}.{method.Name})");
				return;
			}

			try
			{
				result[scenario] = (Action<PluginTestCheckContext>)method.CreateDelegate(
					typeof(Action<PluginTestCheckContext>));
			}
			catch (Exception e)
			{
				errors.Add($"failed to bind the C# check {type.Name}.{method.Name}: {e.Message}");
			}
		}
	}
}
