#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;

namespace MusicGame.Utility.AutoUpdate
{
	public class ResponseFileWatcher
	{
		private FileSystemWatcher? watcher;
		private readonly string statusFilePath;

		public ResponseFileWatcher(string statusFilePath)
		{
			this.statusFilePath = statusFilePath;
		}

		public bool StartListening(Action<string?> onStatusChanged)
		{
			try
			{
				if (watcher != null) StopListening();

				watcher = new FileSystemWatcher
				{
					Path = Path.GetDirectoryName(statusFilePath),
					Filter = Path.GetFileName(statusFilePath),
					NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
					EnableRaisingEvents = true,
					InternalBufferSize = 1024,
					IncludeSubdirectories = false
				};

				watcher.Created += async (_, _) =>
				{
					await Task.Delay(10);
					var result = ReadStatusFile();
					onStatusChanged.Invoke(result);
				};

				watcher.Changed += async (_, _) =>
				{
					await Task.Delay(10);
					var result = ReadStatusFile();
					onStatusChanged.Invoke(result);
				};

				return true;
			}
			catch
			{
				return false;
			}
		}

		public void StopListening()
		{
			if (watcher != null)
			{
				watcher.EnableRaisingEvents = false;
				watcher.Dispose();
				watcher = null;
			}
		}

		private string? ReadStatusFile()
		{
			try
			{
				using FileStream fs = new(statusFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using StreamReader reader = new(fs);
				return reader.ReadToEnd().Trim();
			}
			catch
			{
				return null;
			}
		}

		public void ClearStatusFile()
		{
			if (File.Exists(statusFilePath))
			{
				try
				{
					File.Delete(statusFilePath);
				}
				catch
				{
					//
				}
			}
		}
	}
}