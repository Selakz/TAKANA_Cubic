#nullable enable

using System;
using System.Collections.Generic;
using EditorPlugin.PuerTS;
using Puerts;
using T3Framework.Runtime.Log;

namespace EditorPlugin.PluginSystem
{
	public enum PluginState
	{
		Unloaded,
		Loaded,
		Error
	}

	public interface IPluginInstance
	{
		public PluginManifest Manifest { get; }

		public IReadOnlyDictionary<string, PluginParamWrapper> Params { get; }

		public void Execute();
	}

	public sealed class PluginInstance : IDisposable, IPluginInstance
	{
		// Serializable and Public
		public PluginManifest Manifest { get; }

		public string DirectoryPath { get; }

		public PluginState State { get; private set; }

		public IReadOnlyDictionary<string, PluginParamWrapper> Params { get; }

		public PluginRuntimeEnv? Env { get; private set; }

		// Private
		private readonly string? librariesDirectory;
		private readonly Action<PluginRuntimeEnv>? bridgeInit;
		private ScriptObject? entryObject;

		// Defined Functions
		public PluginInstance(
			PluginManifest manifest, string directoryPath, string? librariesDirectory = null,
			Action<PluginRuntimeEnv>? bridgeInit = null)
		{
			Manifest = manifest;
			DirectoryPath = directoryPath;
			this.librariesDirectory = librariesDirectory;
			this.bridgeInit = bridgeInit;

			var dict = new Dictionary<string, PluginParamWrapper>();
			foreach (var param in manifest.Params) dict[param.Id] = new PluginParamWrapper(param.Type, this);
			Params = dict;
		}

		public void EnsureLoaded()
		{
			if (State == PluginState.Loaded) return;

			try
			{
				var env = Env;
				if (env is null)
				{
					env = new PluginRuntimeEnv(DirectoryPath, librariesDirectory);
					Env = env;
					bridgeInit?.Invoke(env);
				}

				InitializeParams(env);
				entryObject = env.ExecuteModule(Manifest.JsEntry);
				State = PluginState.Loaded;
			}
			catch
			{
				State = PluginState.Error;
				Env?.Dispose();
				Env = null;
				throw;
			}
		}

		private void InitializeParams(PluginRuntimeEnv env)
		{
			var ids = new string[Params.Count];
			var wrappers = new IWrapper<object>[Params.Count];
			int index = 0;
			foreach (var pair in Params)
			{
				ids[index] = pair.Key;
				wrappers[index] = pair.Value;
				index++;
			}

			env.BridgeObject.Get<Action<object, object>>("__params_init")(ids, wrappers);
		}

		public void Execute()
		{
			try
			{
				EnsureLoaded();
				if (State != PluginState.Loaded) return;
				entryObject!.Get<ScriptObject>("default").Get<Action>("execute")();
			}
			catch (Exception e)
			{
				T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|Execute", T3LogType.Error);
				T3Logger.Log("MessageRaw", $"{e.Message}\n{e.StackTrace}");
			}
		}

		public void Tick()
		{
			try
			{
				if (State == PluginState.Loaded) Env?.Tick();
			}
			catch
			{
				T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|Tick", T3LogType.Error);
				// No MessageBox to avoid blocking the editor
			}
		}

		public void Dispose()
		{
			State = PluginState.Unloaded;
			foreach (var param in Params) param.Value.Dispose();
			Env?.Dispose();
			Env = null;
		}
	}
}