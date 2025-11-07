#nullable enable

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace T3Framework.Runtime.WebRequest
{
	public delegate void ProgressHandler(long totalBytes, long downloadedBytes, float progressPercentage);

	public class WebRequestManager : MonoBehaviour
	{
		// Serializable and Public
		public static WebRequestManager Instance { get; private set; } = default!;

		public HttpClient? Client { get; private set; }

		public int BufferSize { get; set; } = 1024 * 128;

		// Defined Functions
		/// <summary> If result is null, there's network failure or deserialization failure. </summary>
		/// <returns> <see cref="TResponse"/> using Newtonsoft.Json to deserialize. </returns>
		public async UniTask<TResponse?> GetWithStructureResponseAsync<TQuery, TResponse>
			(string url, TQuery query) where TQuery : IQueryParam where TResponse : struct
		{
			if (Client is null) return default;

			UriBuilder uriBuilder = new(url) { Query = query.Query };
			try
			{
				using var response = await Client.GetAsync(uriBuilder.Uri);
				if (response.IsSuccessStatusCode)
				{
					Debug.Log($"Request success: {url}");
					var json = await response.Content.ReadAsStringAsync();
					var result = JsonConvert.DeserializeObject<TResponse>(json);
					return result;
				}
				else
				{
					Debug.Log($"Request failed: {url}");
				}
			}
			catch (Exception e)
			{
				Debug.Log($"Request error: {e}");
				return default;
			}

			return default;
		}

		public async UniTask<bool> GetWithFileResponseAndSaveAsync<TQuery>
		(string url, TQuery query, string savePath,
			ProgressHandler? onProgress = null,
			CancellationToken? token = null) where TQuery : IQueryParam
		{
			if (Client is null) return false;

			FileStream? fileStream = null;
			try
			{
				var directory = Path.GetDirectoryName(savePath);
				if (string.IsNullOrEmpty(directory)) return false;
				if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

				UriBuilder uriBuilder = new(url) { Query = query.Query };
				using var response = token is null
					? await Client.GetAsync(uriBuilder.Uri, HttpCompletionOption.ResponseHeadersRead)
					: await Client.GetAsync(uriBuilder.Uri, HttpCompletionOption.ResponseHeadersRead, token.Value);
				if (!response.IsSuccessStatusCode) return false;

				long? totalSize = response.Content.Headers.ContentLength;
				fileStream = new(savePath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);
				await using var contentStream = await response.Content.ReadAsStreamAsync();

				var buffer = new byte[BufferSize];
				long downloadedBytes = 0;
				int bytesRead;

				while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
				{
					await fileStream.WriteAsync(buffer, 0, bytesRead);
					downloadedBytes += bytesRead;

					if (totalSize.HasValue)
					{
						onProgress?.Invoke(totalSize.Value, downloadedBytes, (float)downloadedBytes / totalSize.Value);
					}
				}

				await fileStream.FlushAsync();
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				// ReSharper disable once MethodHasAsyncOverload
				fileStream?.Dispose();
			}
		}

		// System Functions
		void OnEnable()
		{
			Client = new HttpClient();
			Instance = this;
		}
	}
}