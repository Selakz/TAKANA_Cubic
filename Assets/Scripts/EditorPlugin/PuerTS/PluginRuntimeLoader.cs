#nullable enable

using System.IO;
using Puerts;
using UnityEngine;

namespace EditorPlugin.PuerTS
{
	public class PluginRuntimeLoader : ILoader, IModuleChecker, IResolvableLoader
	{
		private readonly string rootDirectory;
		private readonly string? librariesDirectory;

		public PluginRuntimeLoader(string rootDirectory, string? librariesDirectory = null)
		{
			this.rootDirectory = rootDirectory;
			this.librariesDirectory = librariesDirectory;
		}

		public bool FileExists(string filepath)
		{
			if (File.Exists(GetFileSystemPath(filepath))) return true;

			return Resources.Load<TextAsset>(ResourcesPathOf(filepath)) != null;
		}

        public string ReadFile(string filepath, out string debugpath)
        {
            debugpath = filepath;

            string fsPath = GetFileSystemPath(filepath);
            if (File.Exists(fsPath)) return File.ReadAllText(fsPath);

            TextAsset? asset = Resources.Load<TextAsset>(ResourcesPathOf(filepath));
            return asset != null ? asset.text : null!;
        }

		public bool IsESM(string filepath) => !filepath.EndsWith(".cjs");

		public string Resolve(string specifier, string referrer)
		{
			if (specifier.StartsWith("./") || specifier.StartsWith("../"))
			{
				string? referrerDirectory = Path.GetDirectoryName(referrer.Replace('/', Path.DirectorySeparatorChar));
				if (string.IsNullOrEmpty(referrerDirectory)) return specifier;
				return Path.Combine(referrerDirectory, specifier).Replace('\\', '/');
			}

			if (specifier.StartsWith("/")) return specifier;

			if (librariesDirectory == null) return specifier;

			string basePath = Path.GetFullPath(Path.Combine(librariesDirectory, specifier));
			string? resolved = TryResolveExtension(basePath);
			if (resolved == null) return specifier;

			return Path.GetRelativePath(rootDirectory, resolved).Replace('\\', '/');
		}

		private string GetFileSystemPath(string filepath) =>
			Path.Combine(rootDirectory, filepath.Replace('/', Path.DirectorySeparatorChar));

		private static string? TryResolveExtension(string basePath)
		{
			foreach (string ext in s_extensions)
			{
				if (File.Exists(basePath + ext)) return basePath + ext;
			}

			foreach (string ext in s_extensions)
			{
				string indexPath = Path.Combine(basePath, "index" + ext);
				if (File.Exists(indexPath)) return indexPath;
			}

			return null;
		}

		private static readonly string[] s_extensions = { ".ts", ".js", ".mjs" };

		private static string ResourcesPathOf(string filepath)
		{
			if (filepath.EndsWith(".cjs") || filepath.EndsWith(".mjs"))
				return filepath[..^4];
			return filepath;
		}
	}
}