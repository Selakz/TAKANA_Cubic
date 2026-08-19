#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Debug = UnityEngine.Debug;

namespace EditorPlugin.PluginSystem
{
	public sealed class PluginAutoCompiler
	{
		private const string TscCommand = "tsc.cmd";

		private static readonly string[] excludedDirectories = { "node_modules", "dist" };

		private readonly string tsconfigPath;
		private readonly string typesDirectory;

		public PluginAutoCompiler(string sharedConfigDirectory)
		{
			tsconfigPath = Path.Combine(sharedConfigDirectory, "tsconfig.json");
			typesDirectory = Path.Combine(sharedConfigDirectory, "types");
		}

		public static bool NeedsCompile(string pluginDirectory, string jsEntry)
		{
			string jsEntryPath = Path.Combine(pluginDirectory, jsEntry);
			if (!File.Exists(jsEntryPath)) return true;

			DateTime jsTime = File.GetLastWriteTimeUtc(jsEntryPath);
			return EnumerateTsFiles(pluginDirectory).Any(tsFile => File.GetLastWriteTimeUtc(tsFile) > jsTime);
		}

		public static bool IsTscAvailable()
		{
			try
			{
				using Process process = CreateProcess("-v", null);
				process.Start();
				process.WaitForExit();
				return process.ExitCode == 0;
			}
			catch
			{
				return false;
			}
		}

		public bool Compile(string pluginDirectory, string tsEntry, string jsEntry)
		{
			string distDirectory = Path.Combine(pluginDirectory, "dist");
			string tsEntryPath = Path.Combine(pluginDirectory, tsEntry);
			string jsEntryPath = Path.Combine(pluginDirectory, jsEntry);

			List<string> args = ReadCompilerArguments();
			args.Add("--outDir");
			args.Add(distDirectory);
			args.Add("--rootDir");
			args.Add(pluginDirectory);
			args.Add(tsEntryPath);
			args.AddRange(Directory.GetFiles(typesDirectory, "*.d.ts", SearchOption.AllDirectories));

			bool success;
			try
			{
				var buildArguments = BuildArguments(args);
				Debug.Log(buildArguments);
				using Process process = CreateProcess(buildArguments, pluginDirectory);
				process.Start();
				process.WaitForExit();
				success = process.ExitCode == 0 && File.Exists(jsEntryPath);
			}
			catch
			{
				success = false;
				Debug.LogWarning($"compilation failed due to exception: {pluginDirectory}");
			}

			if (!success) DeleteDirectory(distDirectory);
			return success;
		}

		private static IEnumerable<string> EnumerateTsFiles(string pluginDirectory)
		{
			return Directory.GetFiles(pluginDirectory, "*.ts", SearchOption.AllDirectories)
				.Where(file => !IsExcluded(pluginDirectory, file));
		}

		private static bool IsExcluded(string pluginDirectory, string file)
		{
			string relative = Path.GetRelativePath(pluginDirectory, file);
			return (from segment in relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				from excluded in excludedDirectories
				where segment == excluded
				select segment).Any();
		}

		private List<string> ReadCompilerArguments()
		{
			List<string> args = new();
			if (!File.Exists(tsconfigPath)) return args;

			JObject root = JObject.Parse(File.ReadAllText(tsconfigPath));
			if (root["compilerOptions"] is not JObject options) return args;

			args.Add("--ignoreConfig");
			foreach (var pair in options)
			{
				if (pair.Value is null) continue;
				if (pair.Value.Type == JTokenType.Boolean)
				{
					if ((bool)pair.Value) args.Add($"--{pair.Key}");
					continue;
				}

				args.Add($"--{pair.Key}");
				args.Add(pair.Value.ToString());
			}

			return args;
		}

		private static string BuildArguments(IReadOnlyList<string> args)
		{
			string[] quoted = new string[args.Count];
			for (int i = 0; i < args.Count; i++)
			{
				string arg = args[i];
				quoted[i] = arg.Contains(' ') && !arg.StartsWith('"') ? $"\"{arg}\"" : arg;
			}

			return string.Join(" ", quoted);
		}

		private static Process CreateProcess(string arguments, string? workingDirectory)
		{
			ProcessStartInfo startInfo = new()
			{
				FileName = TscCommand,
				Arguments = arguments,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardError = true,
			};
			if (workingDirectory is not null) startInfo.WorkingDirectory = workingDirectory;

			return new Process { StartInfo = startInfo };
		}

		private static void DeleteDirectory(string directory)
		{
			if (Directory.Exists(directory)) Directory.Delete(directory, true);
		}
	}
}