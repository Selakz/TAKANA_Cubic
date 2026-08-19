#nullable enable

using System;
using System.Collections.Generic;
using Puerts;

namespace EditorPlugin.PuerTS
{
	public sealed class PluginRuntimeEnv : IDisposable
	{
		public string RootDirectory { get; }

		public ScriptObject BridgeObject { get; private set; }

#pragma warning disable CS0618
		private readonly JsEnv env;
#pragma warning restore CS0618

		private readonly List<IDisposable> disposables = new();

		public PluginRuntimeEnv(string rootDirectory, string? librariesDirectory = null)
		{
			RootDirectory = rootDirectory;
			var loader = new PluginRuntimeLoader(rootDirectory, librariesDirectory);
#pragma warning disable CS0618
			env = new JsEnv(loader, -1, BackendType.QuickJS, IntPtr.Zero, IntPtr.Zero);
#pragma warning restore CS0618

			BridgeObject = env.ExecuteModule("EditorPlugin/bridge.mjs");
		}

		public ScriptObject ExecuteModule(string specifier)
		{
#pragma warning disable CS0618
			return env.ExecuteModule(specifier);
#pragma warning restore CS0618
		}

		public void Tick() => env.Tick();

		public void AddDisposable(IDisposable disposable) => disposables.Add(disposable);

		public void Dispose()
		{
			foreach (var disposable in disposables) disposable.Dispose();
			disposables.Clear();
			env.Dispose();
		}
	}
}