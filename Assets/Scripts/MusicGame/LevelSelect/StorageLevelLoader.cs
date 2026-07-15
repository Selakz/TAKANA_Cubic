#nullable enable

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;

namespace MusicGame.LevelSelect
{
	public class StorageLevelLoader : HierarchySystem<StorageLevelLoader>
	{
		[Inject] private IDataset<LevelComponent<GameplayPreference>> levelDataset = default!;
		[Inject] private IDataset<PackInfo> packDataset = default!;

		public static async UniTask<List<RawLevelInfo<GameplayPreference>>> LoadLevels(string storagePath)
		{
			List<RawLevelInfo<GameplayPreference>> levels = new();
			if (!Directory.Exists(storagePath)) return levels;
			foreach (var folder in Directory.GetDirectories(storagePath))
			{
				var levelInfo = await RawLevelInfo<GameplayPreference>.FromFolder(folder);
				Debug.Log($"Try load level {folder} and {(levelInfo is null ? "failed" : "succeeded")}");
				if (levelInfo is not null) levels.Add(levelInfo);
			}

			return levels;
		}

		public static async UniTask<List<PackInfo>> LoadPacks()
		{
			List<PackInfo> packs = new();
			var packsPath = Path.Combine(Application.persistentDataPath, "Packs");
			if (!Directory.Exists(packsPath)) return packs;

			foreach (var packFolder in Directory.GetDirectories(packsPath))
			{
				var packInfoPath = Path.Combine(packFolder, "packinfo.yaml");
				if (File.Exists(packInfoPath))
				{
					var songInfo = await ISetting<SongInfo>.LoadAsync(packInfoPath);
					packs.Add(new PackInfo
					{
						Id = songInfo.Id,
						Title = songInfo.Title,
						Description = songInfo.Description,
					});
				}
			}

			return packs;
		}

		public static async UniTask UnzipLevels(string storagePath)
		{
			if (!Directory.Exists(storagePath))
			{
				Debug.LogError($"Storage path {storagePath} does not exist");
				return;
			}

			foreach (var file in Directory.GetFiles(storagePath))
			{
				var fileInfo = new FileInfo(file);
				if (fileInfo.Extension != ".t3pkg") continue;

				var levelDirectory = fileInfo.FullName.Replace(fileInfo.Extension, string.Empty);
				try
				{
					await Task.Run(() => ZipFile.ExtractToDirectory(fileInfo.FullName, levelDirectory));
				}
				catch
				{
					Debug.LogError($"Failed to extract file {fileInfo.FullName}");
					continue;
				}

				try
				{
					File.Delete(fileInfo.FullName);
				}
				catch
				{
					Debug.LogError($"Failed to delete {fileInfo.FullName}");
				}
			}
		}

		public static async UniTask UnzipPacks(string storagePath)
		{
			if (!Directory.Exists(storagePath)) return;

			var packsPath = Path.Combine(Application.persistentDataPath, "Packs");
			foreach (var file in Directory.GetFiles(storagePath))
			{
				var fileInfo = new FileInfo(file);
				if (fileInfo.Extension != ".t3bundle") continue;

				var bundleName = Path.GetFileNameWithoutExtension(fileInfo.Name);
				var packDirectory = Path.Combine(packsPath, bundleName);
				try
				{
					await Task.Run(() => ZipFile.ExtractToDirectory(fileInfo.FullName, packDirectory));
				}
				catch
				{
					Debug.LogError($"Failed to extract bundle {fileInfo.FullName}");
					continue;
				}

				foreach (var innerFile in Directory.GetFiles(packDirectory))
				{
					var innerInfo = new FileInfo(innerFile);
					if (innerInfo.Extension != ".t3pkg") continue;

					var targetT3Pkg = Path.Combine(storagePath, innerInfo.Name);
					try
					{
						File.Move(innerInfo.FullName, targetT3Pkg);
					}
					catch
					{
						Debug.LogError($"Failed to move t3pkg {innerInfo.FullName}");
					}
				}

				foreach (var innerDir in Directory.GetDirectories(packDirectory))
				{
					var dirName = Path.GetFileName(innerDir);
					var targetDir = Path.Combine(storagePath, dirName);
					try
					{
						Directory.Move(innerDir, targetDir);
					}
					catch
					{
						Debug.LogError($"Failed to move folder {innerDir}");
					}
				}

				try
				{
					File.Delete(fileInfo.FullName);
				}
				catch
				{
					Debug.LogError($"Failed to delete bundle {fileInfo.FullName}");
				}
			}
		}

		async void Start()
		{
			var storagePath = ISingleton<LevelSetting>.Instance.StoragePath.Value;
			Directory.CreateDirectory(storagePath);
			var packsPath = Path.Combine(Application.persistentDataPath, "Packs");
			Directory.CreateDirectory(packsPath);
			await UnzipPacks(storagePath);
			await UnzipLevels(storagePath);
			var levels = await LoadLevels(storagePath);
			var packs = await LoadPacks();
			foreach (var level in levels) levelDataset.Add(new(level));
			packDataset.Add(PackInfo.All);
			packDataset.Add(PackInfo.Single);
			foreach (var pack in packs) packDataset.Add(pack);
		}
	}
}