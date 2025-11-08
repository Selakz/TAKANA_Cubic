#nullable enable

using T3Framework.Runtime.Event;

namespace MusicGame.Utility.AutoUpdate
{
	public struct DownloadProgress
	{
		public long TotalBytes { get; set; }

		public long DownloadedBytes { get; set; }

		public float ProgressPercentage { get; set; }

		public DownloadProgress(long totalBytes, long downloadedBytes, float progressPercentage)
		{
			TotalBytes = totalBytes;
			DownloadedBytes = downloadedBytes;
			ProgressPercentage = progressPercentage;
		}
	}

	public class DownloadProgressDataContainer : NotifiableDataContainer<DownloadProgress>
	{
		public override DownloadProgress InitialValue => new(0, 0, 0);
	}
}