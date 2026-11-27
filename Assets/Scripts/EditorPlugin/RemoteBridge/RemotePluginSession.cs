#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using EditorPlugin.PluginSystem;
using EditorPlugin.PuerTS;
using EditorPlugin.Shared;
using MusicGame.ChartEditor.Command;
using Newtonsoft.Json.Linq;
using Puerts;

namespace EditorPlugin.RemoteBridge
{
	public sealed class RemotePluginSession : IDisposable
	{
		private sealed class PendingExecution
		{
			public RemoteExecutionSink Sink { get; }

			public TaskCompletionSource<RemoteExecuteResponse> Completion { get; }

			public Stopwatch Stopwatch { get; }

			public int TimeoutMs { get; }

			public int FlushesBefore { get; }

			public PendingExecution(
				RemoteExecutionSink sink,
				TaskCompletionSource<RemoteExecuteResponse> completion,
				int timeoutMs,
				int flushesBefore)
			{
				Sink = sink;
				Completion = completion;
				Stopwatch = Stopwatch.StartNew();
				TimeoutMs = timeoutMs;
				FlushesBefore = flushesBefore;
			}
		}

		private readonly PluginRuntimeEnv env;
		private readonly T3CSharpApi api;
		private readonly CommandManager commandManager;
		private PendingExecution? activeExecution;
		private bool disposed;

		private RemotePluginSession(PluginRuntimeEnv env, T3CSharpApi api, CommandManager commandManager)
		{
			this.env = env;
			this.api = api;
			this.commandManager = commandManager;
		}

		public static RemotePluginSession? TryCreate(
			string workspaceDirectory,
			string? librariesDirectory,
			IBridgeBootstrapService bridgeBootstrap,
			CommandManager commandManager)
		{
			var env = new PluginRuntimeEnv(workspaceDirectory, librariesDirectory);
			var api = bridgeBootstrap.Initialize(env);
			if (api is null)
			{
				env.Dispose();
				return null;
			}

			return new RemotePluginSession(env, api, commandManager);
		}

		public Task<RemoteExecuteResponse> ExecuteModuleAsync(string moduleSpecifier, int timeoutMs)
		{
			if (disposed)
			{
				return Task.FromResult(new RemoteExecuteResponse
				{
					Success = false,
					Stage = "level_changed",
					Error = "Remote plugin session was disposed."
				});
			}

			if (activeExecution is not null)
			{
				return Task.FromResult(new RemoteExecuteResponse
				{
					Success = false,
					Stage = "runtime_error",
					Error = "Another script execution is already in progress."
				});
			}

			var tcs = new TaskCompletionSource<RemoteExecuteResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
			int flushesBefore = api.staging.FlushCount;
			PendingExecution? pending = null;

			var sink = new RemoteExecutionSink(
				(hasValue, jsonValue) => CompleteResolved(pending!, hasValue, jsonValue),
				(message, stack) => CompleteRejected(pending!, message, stack));
			pending = new PendingExecution(sink, tcs, timeoutMs, flushesBefore);
			activeExecution = pending;

			try
			{
				ScriptObject module = env.ExecuteModule(moduleSpecifier);
				var run = module.Get<Action<object>>("default");
				if (run is null)
				{
					CompleteRejected(pending, "Compiled module did not export a default runner function.", null);
					return tcs.Task;
				}

				run.Invoke(sink);
				env.Tick();
			}
			catch (Exception e)
			{
				if (sink.TryForceSettle())
				{
					CompleteRejected(pending, e.Message, e.StackTrace);
				}
			}

			return tcs.Task;
		}

		public void Tick()
		{
			if (disposed) return;

			try
			{
				env.Tick();
			}
			catch (Exception e)
			{
				if (activeExecution is { } running && running.Sink.TryForceSettle())
				{
					CompleteRejected(running, e.Message, e.StackTrace);
				}
			}

			if (activeExecution is { } current && current.Stopwatch.ElapsedMilliseconds >= current.TimeoutMs)
			{
				if (current.Sink.TryForceSettle())
				{
					CompleteTimedOut(current);
				}
			}
		}

		public void Dispose()
		{
			if (disposed) return;
			disposed = true;

			if (activeExecution is { } current)
			{
				activeExecution = null;
				current.Sink.TryForceSettle();
				if (api.staging.hasPending) api.staging.Discard();
				current.Completion.TrySetResult(new RemoteExecuteResponse
				{
					Success = false,
					Stage = "level_changed",
					Logs = new List<RemoteLogEntry>(current.Sink.Logs),
					Error = "Execution aborted because the remote session was reset or the level changed."
				});
			}

			env.Dispose();
		}

		private void CompleteResolved(PendingExecution execution, bool hasValue, string? jsonValue)
		{
			if (ReferenceEquals(activeExecution, execution)) activeExecution = null;

			bool autoCommitted = false;
			if (api.staging.hasPending)
			{
				api.staging.commit();
				autoCommitted = true;
			}

			bool committed = api.staging.FlushCount > execution.FlushesBefore;
			JToken? parsedResult = null;
			if (hasValue && jsonValue is not null)
			{
				try
				{
					parsedResult = JToken.Parse(jsonValue);
				}
				catch
				{
					parsedResult = JValue.CreateString(jsonValue);
				}
			}

			execution.Completion.TrySetResult(new RemoteExecuteResponse
			{
				Success = true,
				Stage = "completed",
				HasResult = hasValue,
				Result = parsedResult,
				Committed = committed,
				AutoCommitted = autoCommitted,
				Revision = commandManager.Revision,
				Logs = new List<RemoteLogEntry>(execution.Sink.Logs)
			});
		}

		private void CompleteRejected(PendingExecution execution, string message, string? stack)
		{
			if (ReferenceEquals(activeExecution, execution)) activeExecution = null;

			if (api.staging.hasPending) api.staging.Discard();
			bool committed = api.staging.FlushCount > execution.FlushesBefore;

			execution.Completion.TrySetResult(new RemoteExecuteResponse
			{
				Success = false,
				Stage = "runtime_error",
				Committed = committed,
				AutoCommitted = false,
				Revision = commandManager.Revision,
				Logs = new List<RemoteLogEntry>(execution.Sink.Logs),
				Error = message,
				Stack = stack
			});
		}

		private void CompleteTimedOut(PendingExecution execution)
		{
			if (ReferenceEquals(activeExecution, execution)) activeExecution = null;

			if (api.staging.hasPending) api.staging.Discard();
			bool committed = api.staging.FlushCount > execution.FlushesBefore;

			execution.Completion.TrySetResult(new RemoteExecuteResponse
			{
				Success = false,
				Stage = "timeout",
				Committed = committed,
				AutoCommitted = false,
				Revision = commandManager.Revision,
				Logs = new List<RemoteLogEntry>(execution.Sink.Logs),
				Error = $"Script execution timed out after {execution.TimeoutMs} ms."
			});
		}
	}
}
