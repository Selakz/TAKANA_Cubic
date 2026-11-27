#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EditorPlugin.EditorIntegration;
using EditorPlugin.PluginSystem;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace EditorPlugin.RemoteBridge
{
	public interface IRemoteBridgeService
	{
		public NotifiableProperty<RemoteBridgeStatus> Status { get; }
	}

	public class RemoteBridgeService : HierarchySystem<RemoteBridgeService>, IRemoteBridgeService
	{
		// Serializable and Public
		public override bool AsImplementedInterfaces => true;

		public NotifiableProperty<RemoteBridgeStatus> Status { get; } =
			new(new RemoteBridgeStatus(RemoteBridgeState.Stopped, 0));

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, OnLevelChanged),
			new PropertyRegistrar<bool>(ISingleton<EditorSetting>.Instance.RemoteBridgeEnabled, _ => SyncServerState()),
			new PropertyRegistrar<int>(ISingleton<EditorSetting>.Instance.RemoteBridgePort, _ => SyncServerState())
		};

		// Private
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private CommandManager commandManager = default!;
		[Inject] private IGameAudioPlayer music = default!;
		[Inject] private IBridgeBootstrapService bridgeBootstrap = default!;
		[Inject] private IPluginManageService pluginManageService = default!;

		private const int DefaultTimeoutMs = 15000;
		private const int MinTimeoutMs = 500;
		private const int MaxTimeoutMs = 60000;
		private const int MaxHistorySteps = 50;

		private readonly ConcurrentQueue<Action> mainThreadQueue = new();
		private readonly SemaphoreSlim executionLock = new(1, 1);
		private readonly PluginAutoCompiler autoCompiler = new(PluginSystemPaths.SharedConfigDirectory);

		private RemoteBridgeHttpServer? httpServer;
		private RemotePluginSession? remoteSession;
		private DateTimeOffset? levelOpenedAt;
		private int levelGeneration;
		private int executionSequence;
		private string workspaceDirectory = string.Empty;
		private string appVersion = string.Empty;
		private bool? cachedTscAvailable;

		// Defined Functions
		private void OnLevelChanged(LevelInfo? info)
		{
			levelGeneration++;
			remoteSession?.Dispose();
			remoteSession = null;
			levelOpenedAt = info is null ? null : DateTimeOffset.UtcNow;
		}

		private void SyncServerState()
		{
			var setting = ISingleton<EditorSetting>.Instance;
			bool shouldRun = setting.RemoteBridgeEnabled.Value;
			int port = Mathf.Clamp(setting.RemoteBridgePort.Value, 1024, 65535);

			if (!shouldRun)
			{
				StopServer(port);
				return;
			}

			if (httpServer is not null && httpServer.Port == port) return;

			StopServer(port);
			try
			{
				httpServer = new RemoteBridgeHttpServer(
					port,
					HandleHealthAsync,
					HandleLevelAsync,
					HandleExecuteAsync,
					HandleUndoAsync,
					HandleRedoAsync,
					HandleResetAsync);
				Status.Value = new RemoteBridgeStatus(RemoteBridgeState.Listening, port);
				Debug.Log($"[RemoteBridge] Listening on http://127.0.0.1:{port}/");
			}
			catch (Exception e)
			{
				Status.Value = new RemoteBridgeStatus(RemoteBridgeState.Error, port, e.Message);
				Debug.LogError($"[RemoteBridge] Failed to listen on 127.0.0.1:{port}: {e.Message}");
			}
		}

		private void StopServer(int port)
		{
			if (httpServer is not null)
			{
				httpServer.Dispose();
				httpServer = null;
				Debug.Log("[RemoteBridge] HTTP server stopped.");
			}

			Status.Value = new RemoteBridgeStatus(RemoteBridgeState.Stopped, port);
		}

		private Task<RemoteHttpResponse> HandleHealthAsync(CancellationToken ct)
		{
			bool tscAvailable = cachedTscAvailable ??= PluginAutoCompiler.IsTscAvailable();
			return RunOnMainThreadAsync(() => new RemoteHttpResponse(200, new RemoteHealthResponse
			{
				Ok = true,
				AppVersion = appVersion,
				ApiVersion = pluginManageService.ApiVersion?.ToString(),
				HasLevel = levelInfo.Value is not null,
				TscAvailable = tscAvailable
			}), ct);
		}

		private Task<RemoteHttpResponse> HandleLevelAsync(CancellationToken ct)
		{
			return RunOnMainThreadAsync(() =>
			{
				var response = new RemoteLevelResponse
				{
					HasLevel = levelInfo.Value is not null,
					AppVersion = appVersion,
					ApiVersion = pluginManageService.ApiVersion?.ToString()
				};

				if (levelInfo.Value is { } info)
				{
					var opened = levelOpenedAt ?? DateTimeOffset.UtcNow;
					var difficulties = new Dictionary<int, RemoteDifficultyInfoPayload>();
					foreach (var pair in info.SongInfo.Difficulties)
					{
						difficulties[pair.Key] = ToDifficultyPayload(pair.Value);
					}

					difficulties.TryGetValue(info.Difficulty, out var currentDifficulty);
					response.Level = new RemoteLevelPayload
					{
						LevelPath = info.LevelPath,
						Difficulty = info.Difficulty,
						OpenedAt = opened.ToString("O"),
						OpenedAtUnixMs = opened.ToUnixTimeMilliseconds(),
						Revision = commandManager.Revision,
						AudioLengthMilli = music.AudioLength.Milli,
						ChartTimeMilli = music.ChartTime.Milli,
						SongInfo = new RemoteSongInfoPayload
						{
							Id = info.SongInfo.Id,
							PackId = info.SongInfo.PackId,
							Title = info.SongInfo.Title,
							Composer = info.SongInfo.Composer,
							Illustrator = info.SongInfo.Illustrator,
							BpmDisplay = info.SongInfo.BpmDisplay,
							Description = info.SongInfo.Description,
							DifficultyInfo = currentDifficulty,
							Difficulties = difficulties
						}
					};
				}

				return new RemoteHttpResponse(200, response);

				static RemoteDifficultyInfoPayload ToDifficultyPayload(DifficultyInfo source) => new()
				{
					LevelDisplay = source.LevelDisplay,
					Charter = source.Charter,
					IsCustom = source.IsCustom,
					Name = source.Name
				};
			}, ct);
		}

		private async Task<RemoteHttpResponse> HandleResetAsync(CancellationToken ct)
		{
			await executionLock.WaitAsync(ct).ConfigureAwait(false);
			try
			{
				return await RunOnMainThreadAsync(() =>
				{
					remoteSession?.Dispose();
					remoteSession = null;
					return new RemoteHttpResponse(200, new RemoteResetResponse
					{
						Ok = true,
						HasLevel = levelInfo.Value is not null
					});
				}, ct).ConfigureAwait(false);
			}
			finally
			{
				executionLock.Release();
			}
		}

		private Task<RemoteHttpResponse> HandleUndoAsync(string rawBody, CancellationToken ct) =>
			HandleHistoryAsync(rawBody, true, ct);

		private Task<RemoteHttpResponse> HandleRedoAsync(string rawBody, CancellationToken ct) =>
			HandleHistoryAsync(rawBody, false, ct);

		private async Task<RemoteHttpResponse> HandleHistoryAsync(string rawBody, bool isUndo, CancellationToken ct)
		{
			RemoteHistoryRequest request;
			if (string.IsNullOrWhiteSpace(rawBody))
			{
				request = new RemoteHistoryRequest();
			}
			else
			{
				try
				{
					request = RemoteBridgeSerializer.Deserialize<RemoteHistoryRequest>(rawBody) ?? new();
				}
				catch (Exception e)
				{
					return new RemoteHttpResponse(400, new RemoteErrorResponse { Error = $"Invalid JSON body: {e.Message}" });
				}
			}

			int steps = request.Steps ?? 1;
			if (steps < 1)
			{
				return new RemoteHttpResponse(400, new RemoteErrorResponse { Error = "Field 'steps' must be >= 1." });
			}

			steps = Mathf.Min(steps, MaxHistorySteps);

			await executionLock.WaitAsync(ct).ConfigureAwait(false);
			try
			{
				return await RunOnMainThreadAsync(() =>
				{
					if (levelInfo.Value is null)
					{
						return new RemoteHttpResponse(409, new RemoteHistoryResponse
						{
							Success = false,
							Stage = "no_level",
							Error = "No level is currently loaded in the editor."
						});
					}

					int revisionBefore = commandManager.Revision;
					if (request.ExpectedRevision is { } expected && revisionBefore != expected)
					{
						return new RemoteHttpResponse(409, new RemoteHistoryResponse
						{
							Success = false,
							Stage = "revision_mismatch",
							AppliedSteps = 0,
							RevisionBefore = revisionBefore,
							RevisionAfter = revisionBefore,
							Error = $"Expected revision {expected}, but current revision is {revisionBefore}."
						});
					}

					int appliedSteps = 0;
					bool stepTriggered = false;
					Action<ICommand> onStep = _ => stepTriggered = true;

					if (isUndo) commandManager.OnUndo += onStep;
					else commandManager.OnRedo += onStep;
					try
					{
						for (int i = 0; i < steps; i++)
						{
							stepTriggered = false;
							if (isUndo) commandManager.Undo();
							else commandManager.Redo();
							if (!stepTriggered) break;
							appliedSteps++;
						}
					}
					finally
					{
						if (isUndo) commandManager.OnUndo -= onStep;
						else commandManager.OnRedo -= onStep;
					}

					int revisionAfter = commandManager.Revision;
					if (appliedSteps == 0)
					{
						return new RemoteHttpResponse(409, new RemoteHistoryResponse
						{
							Success = false,
							Stage = isUndo ? "nothing_to_undo" : "nothing_to_redo",
							AppliedSteps = 0,
							RevisionBefore = revisionBefore,
							RevisionAfter = revisionAfter,
							Error = isUndo ? "Nothing to undo." : "Nothing to redo."
						});
					}

					return new RemoteHttpResponse(200, new RemoteHistoryResponse
					{
						Success = true,
						Stage = "completed",
						AppliedSteps = appliedSteps,
						RevisionBefore = revisionBefore,
						RevisionAfter = revisionAfter
					});
				}, ct).ConfigureAwait(false);
			}
			finally
			{
				executionLock.Release();
			}
		}

		private async Task<RemoteHttpResponse> HandleExecuteAsync(string rawBody, CancellationToken ct)
		{
			RemoteExecuteRequest? request;
			try
			{
				request = RemoteBridgeSerializer.Deserialize<RemoteExecuteRequest>(rawBody);
			}
			catch (Exception e)
			{
				return new RemoteHttpResponse(400, new RemoteErrorResponse { Error = $"Invalid JSON body: {e.Message}" });
			}

			if (request is null || string.IsNullOrWhiteSpace(request.Code))
			{
				return new RemoteHttpResponse(400, new RemoteErrorResponse { Error = "Field 'code' is required." });
			}

			if (!TryParseLanguage(request.Language, out bool isTypeScript))
			{
				return new RemoteHttpResponse(400, new RemoteErrorResponse
				{
					Error = $"Unsupported language '{request.Language}'. Expected 'ts' or 'js'."
				});
			}

			int timeoutMs = Mathf.Clamp(request.TimeoutMs ?? DefaultTimeoutMs, MinTimeoutMs, MaxTimeoutMs);

			await executionLock.WaitAsync(ct).ConfigureAwait(false);
			try
			{
				var (hasLevel, capturedGeneration, currentRevision) = await RunOnMainThreadAsync(
					() => (levelInfo.Value is not null, levelGeneration, commandManager.Revision), ct)
					.ConfigureAwait(false);
				if (!hasLevel)
				{
					return new RemoteHttpResponse(409, new RemoteExecuteResponse
					{
						Success = false,
						Stage = "no_level",
						Error = "No level is currently loaded in the editor."
					});
				}

				if (request.ExpectedRevision is { } expectedBeforeCompile && currentRevision != expectedBeforeCompile)
				{
					return new RemoteHttpResponse(409, new RemoteExecuteResponse
					{
						Success = false,
						Stage = "revision_mismatch",
						Revision = currentRevision,
						Error = $"Expected revision {expectedBeforeCompile}, but current revision is {currentRevision}."
					});
				}

				Directory.CreateDirectory(workspaceDirectory);
				int runId = Interlocked.Increment(ref executionSequence);
				string? sourceFilePath = null;
				string? compiledFilePath = null;
				string moduleSpecifier;

				try
				{
					if (isTypeScript)
					{
						bool tscOk = cachedTscAvailable ??= PluginAutoCompiler.IsTscAvailable();
						if (!tscOk)
						{
							return new RemoteHttpResponse(200, new RemoteExecuteResponse
							{
								Success = false,
								Stage = "compile_error",
								Revision = currentRevision,
								Error = "TypeScript compiler (tsc.cmd) is not available in PATH."
							});
						}

						string tsFileName = $"run_{runId}.ts";
						string jsRelativePath = $"dist/run_{runId}.js";
						sourceFilePath = Path.Combine(workspaceDirectory, tsFileName);
						compiledFilePath = Path.Combine(workspaceDirectory, "dist", $"run_{runId}.js");
						File.WriteAllText(sourceFilePath, RemoteScriptWrapper.WrapTypeScript(request.Code!));

						bool compiled = autoCompiler.Compile(workspaceDirectory, tsFileName, jsRelativePath, out string diagnostics);
						if (!compiled)
						{
							return new RemoteHttpResponse(200, new RemoteExecuteResponse
							{
								Success = false,
								Stage = "compile_error",
								Revision = currentRevision,
								Error = string.IsNullOrWhiteSpace(diagnostics)
									? "TypeScript compilation failed."
									: diagnostics
							});
						}

						moduleSpecifier = jsRelativePath;
					}
					else
					{
						string mjsFileName = $"run_{runId}.mjs";
						sourceFilePath = Path.Combine(workspaceDirectory, mjsFileName);
						File.WriteAllText(sourceFilePath, RemoteScriptWrapper.WrapJavaScript(request.Code!));
						moduleSpecifier = mjsFileName;
					}

					var executionTask = await RunOnMainThreadAsync(() =>
					{
						if (levelInfo.Value is null)
						{
							return Task.FromResult(new RemoteExecuteResponse
							{
								Success = false,
								Stage = "no_level",
								Error = "No level is currently loaded in the editor."
							});
						}

						if (levelGeneration != capturedGeneration)
						{
							return Task.FromResult(new RemoteExecuteResponse
							{
								Success = false,
								Stage = "level_changed",
								Error = "The open level changed while preparing the script."
							});
						}

						if (request.ExpectedRevision is { } expectedBeforeRun && commandManager.Revision != expectedBeforeRun)
						{
							int latestRevision = commandManager.Revision;
							return Task.FromResult(new RemoteExecuteResponse
							{
								Success = false,
								Stage = "revision_mismatch",
								Revision = latestRevision,
								Error = $"Expected revision {expectedBeforeRun}, but current revision is {latestRevision}."
							});
						}

						if (remoteSession is null)
						{
							string librariesDir = Path.Combine(ISingleton<EditorSetting>.Instance.PluginPath, "libraries");
							string? libraries = Directory.Exists(librariesDir) ? librariesDir : null;
							remoteSession = RemotePluginSession.TryCreate(
								workspaceDirectory, libraries, bridgeBootstrap, commandManager);
							if (remoteSession is null)
							{
								return Task.FromResult(new RemoteExecuteResponse
								{
									Success = false,
									Stage = "no_level",
									Error = "Failed to initialize bridge for the current level."
								});
							}
						}

						return remoteSession.ExecuteModuleAsync(moduleSpecifier, timeoutMs);
					}, ct).ConfigureAwait(false);

					RemoteExecuteResponse result = await executionTask.ConfigureAwait(false);
					int statusCode = result.Stage is "no_level" or "revision_mismatch" ? 409 : 200;
					return new RemoteHttpResponse(statusCode, result);
				}
				finally
				{
					TryDeleteFile(sourceFilePath);
					TryDeleteFile(compiledFilePath);
				}
			}
			finally
			{
				executionLock.Release();
			}

			static bool TryParseLanguage(string? language, out bool isTs)
			{
				if (string.IsNullOrWhiteSpace(language))
				{
					isTs = true;
					return true;
				}

				switch (language!.Trim().ToLowerInvariant())
				{
					case "ts":
					case "typescript":
						isTs = true;
						return true;
					case "js":
					case "javascript":
						isTs = false;
						return true;
					default:
						isTs = false;
						return false;
				}
			}

			static void TryDeleteFile(string? path)
			{
				if (string.IsNullOrEmpty(path)) return;
				try
				{
					if (File.Exists(path)) File.Delete(path);
				}
				catch
				{
					// Ignore cleanup errors on temporary execution files.
				}
			}
		}

		private Task<T> RunOnMainThreadAsync<T>(Func<T> action, CancellationToken ct)
		{
			var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
			if (ct.IsCancellationRequested)
			{
				tcs.TrySetCanceled(ct);
				return tcs.Task;
			}

			CancellationTokenRegistration registration = ct.Register(() => tcs.TrySetCanceled(ct));
			mainThreadQueue.Enqueue(() =>
			{
				registration.Dispose();
				if (tcs.Task.IsCanceled) return;
				try
				{
					tcs.TrySetResult(action.Invoke());
				}
				catch (Exception e)
				{
					tcs.TrySetException(e);
				}
			});
			return tcs.Task;
		}

		private void CleanupWorkspace()
		{
			if (string.IsNullOrEmpty(workspaceDirectory)) return;
			try
			{
				if (Directory.Exists(workspaceDirectory)) Directory.Delete(workspaceDirectory, true);
			}
			catch
			{
				// Ignore temporary workspace cleanup errors.
			}
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			appVersion = Application.version;
			workspaceDirectory = Path.Combine(Application.temporaryCachePath, "EditorPluginRemote");
			CleanupWorkspace();
		}

		void Update()
		{
			while (mainThreadQueue.TryDequeue(out var work))
			{
				work.Invoke();
			}

			remoteSession?.Tick();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			StopServer(ISingleton<EditorSetting>.Instance.RemoteBridgePort.Value);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			StopServer(ISingleton<EditorSetting>.Instance.RemoteBridgePort.Value);
			remoteSession?.Dispose();
			remoteSession = null;
			CleanupWorkspace();
			executionLock.Dispose();
		}
	}
}
