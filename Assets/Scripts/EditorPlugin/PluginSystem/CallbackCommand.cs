#nullable enable

using System;
using MusicGame.ChartEditor.Command;
using T3Framework.Runtime.Log;

namespace EditorPlugin.PluginSystem
{
	/// <summary>
	/// Invokes a callback right after the commands it is batched with, and at most once,
	/// so that undo/redo never replays plugin callbacks.
	/// </summary>
	public sealed class CallbackCommand : ICommand
	{
		public string Name => "Plugin callback";

		private readonly Action callback;
		private bool hasInvoked;

		public CallbackCommand(Action callback)
		{
			this.callback = callback;
		}

		public void Do()
		{
			if (hasInvoked) return;
			hasInvoked = true;
			SafeInvoke(callback);
		}

		public void Undo()
		{
		}

		/// <summary> Runs a callback coming from a plugin, reporting its exceptions as plugin internal errors. </summary>
		public static void SafeInvoke(Action callback)
		{
			try
			{
				callback.Invoke();
			}
			catch (Exception e)
			{
				T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|callback", T3LogType.Error);
				T3Logger.Log("MessageRaw", $"{e.Message}\n{e.StackTrace}");
			}
		}
	}
}
