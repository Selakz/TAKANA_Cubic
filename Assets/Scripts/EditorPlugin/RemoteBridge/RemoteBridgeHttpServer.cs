#nullable enable

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace EditorPlugin.RemoteBridge
{
	public readonly struct RemoteHttpResponse
	{
		public int StatusCode { get; }

		public object Body { get; }

		public RemoteHttpResponse(int statusCode, object body)
		{
			StatusCode = statusCode;
			Body = body;
		}
	}

	public sealed class RemoteBridgeHttpServer : IDisposable
	{
		private const int MaxRequestBodyBytes = 2 * 1024 * 1024;

		private readonly HttpListener listener = new();
		private readonly CancellationTokenSource cts = new();
		private readonly Func<CancellationToken, Task<RemoteHttpResponse>> handleHealth;
		private readonly Func<CancellationToken, Task<RemoteHttpResponse>> handleLevel;
		private readonly Func<string, CancellationToken, Task<RemoteHttpResponse>> handleExecute;
		private readonly Func<string, CancellationToken, Task<RemoteHttpResponse>> handleUndo;
		private readonly Func<string, CancellationToken, Task<RemoteHttpResponse>> handleRedo;
		private readonly Func<CancellationToken, Task<RemoteHttpResponse>> handleReset;
		private bool disposed;

		public int Port { get; }

		public RemoteBridgeHttpServer(
			int port,
			Func<CancellationToken, Task<RemoteHttpResponse>> handleHealth,
			Func<CancellationToken, Task<RemoteHttpResponse>> handleLevel,
			Func<string, CancellationToken, Task<RemoteHttpResponse>> handleExecute,
			Func<string, CancellationToken, Task<RemoteHttpResponse>> handleUndo,
			Func<string, CancellationToken, Task<RemoteHttpResponse>> handleRedo,
			Func<CancellationToken, Task<RemoteHttpResponse>> handleReset)
		{
			Port = port;
			this.handleHealth = handleHealth;
			this.handleLevel = handleLevel;
			this.handleExecute = handleExecute;
			this.handleUndo = handleUndo;
			this.handleRedo = handleRedo;
			this.handleReset = handleReset;

			listener.Prefixes.Add($"http://127.0.0.1:{port}/");
			listener.Start();
			_ = Task.Run(() => AcceptLoopAsync(cts.Token));
		}

		public void Dispose()
		{
			if (disposed) return;
			disposed = true;
			cts.Cancel();
			try
			{
				listener.Stop();
			}
			catch
			{
				// Ignore listener shutdown races.
			}

			try
			{
				listener.Close();
			}
			catch
			{
				// Ignore listener close races.
			}

			cts.Dispose();
		}

		private async Task AcceptLoopAsync(CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				HttpListenerContext context;
				try
				{
					context = await listener.GetContextAsync().ConfigureAwait(false);
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				catch (HttpListenerException)
				{
					if (ct.IsCancellationRequested || !listener.IsListening) break;
					continue;
				}
				catch (Exception e)
				{
					if (ct.IsCancellationRequested) break;
					Debug.LogError($"[RemoteBridge] Accept loop error: {e}");
					continue;
				}

				_ = Task.Run(() => HandleContextAsync(context, ct), ct);
			}
		}

		private async Task HandleContextAsync(HttpListenerContext context, CancellationToken ct)
		{
			RemoteHttpResponse response;
			try
			{
				string path = (context.Request.Url?.AbsolutePath ?? "/").TrimEnd('/');
				if (string.IsNullOrEmpty(path)) path = "/";
				string method = context.Request.HttpMethod.ToUpperInvariant();

				response = path switch
				{
					"/health" => method == "GET"
						? await handleHealth(ct).ConfigureAwait(false)
						: MethodNotAllowed("GET"),
					"/level" => method == "GET"
						? await handleLevel(ct).ConfigureAwait(false)
						: MethodNotAllowed("GET"),
					"/execute" => method == "POST"
						? await HandlePostBodyRequestAsync(context.Request, handleExecute, ct).ConfigureAwait(false)
						: MethodNotAllowed("POST"),
					"/undo" => method == "POST"
						? await HandlePostBodyRequestAsync(context.Request, handleUndo, ct).ConfigureAwait(false)
						: MethodNotAllowed("POST"),
					"/redo" => method == "POST"
						? await HandlePostBodyRequestAsync(context.Request, handleRedo, ct).ConfigureAwait(false)
						: MethodNotAllowed("POST"),
					"/reset" => method == "POST"
						? await handleReset(ct).ConfigureAwait(false)
						: MethodNotAllowed("POST"),
					_ => new RemoteHttpResponse(404, new RemoteErrorResponse { Error = $"Unknown endpoint: {path}" })
				};
			}
			catch (OperationCanceledException)
			{
				response = new RemoteHttpResponse(503, new RemoteErrorResponse { Error = "Server is shutting down." });
			}
			catch (Exception e)
			{
				Debug.LogError($"[RemoteBridge] Request failed: {e}");
				response = new RemoteHttpResponse(500, new RemoteErrorResponse { Error = e.Message });
			}

			await WriteJsonResponseAsync(context.Response, response).ConfigureAwait(false);

			static RemoteHttpResponse MethodNotAllowed(string allowedMethod) =>
				new(405, new RemoteErrorResponse { Error = $"Method not allowed. Use {allowedMethod}." });

			async Task<RemoteHttpResponse> HandlePostBodyRequestAsync(
				HttpListenerRequest request,
				Func<string, CancellationToken, Task<RemoteHttpResponse>> handler,
				CancellationToken token)
			{
				if (request.ContentLength64 > MaxRequestBodyBytes)
				{
					return new RemoteHttpResponse(413,
						new RemoteErrorResponse { Error = $"Request body exceeds {MaxRequestBodyBytes} bytes." });
				}

				var (ok, body, readError) = await ReadBodyUtf8Async(request.InputStream, MaxRequestBodyBytes, token)
					.ConfigureAwait(false);
				if (!ok)
				{
					return new RemoteHttpResponse(413, new RemoteErrorResponse { Error = readError! });
				}

				return await handler(body, token).ConfigureAwait(false);
			}

			static async Task<(bool Ok, string Body, string? Error)> ReadBodyUtf8Async(
				Stream stream, int maxBytes, CancellationToken token)
			{
				using var ms = new MemoryStream();
				byte[] buffer = new byte[8192];
				int total = 0;
				while (true)
				{
					int read = await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
					if (read <= 0) break;
					total += read;
					if (total > maxBytes)
					{
						return (false, string.Empty, $"Request body exceeds {maxBytes} bytes.");
					}

					ms.Write(buffer, 0, read);
				}

				return (true, Encoding.UTF8.GetString(ms.ToArray()), null);
			}

			static async Task WriteJsonResponseAsync(HttpListenerResponse httpResponse, RemoteHttpResponse payload)
			{
				try
				{
					byte[] bytes = Encoding.UTF8.GetBytes(RemoteBridgeSerializer.Serialize(payload.Body));
					httpResponse.StatusCode = payload.StatusCode;
					httpResponse.ContentType = "application/json; charset=utf-8";
					httpResponse.ContentEncoding = Encoding.UTF8;
					httpResponse.ContentLength64 = bytes.Length;
					await httpResponse.OutputStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
					httpResponse.OutputStream.Close();
				}
				catch
				{
					// Client disconnected before reading the response.
				}
			}
		}
	}
}
