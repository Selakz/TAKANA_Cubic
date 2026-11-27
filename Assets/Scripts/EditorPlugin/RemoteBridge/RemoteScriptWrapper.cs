#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace EditorPlugin.RemoteBridge
{
	public static class RemoteScriptWrapper
	{
		public static string WrapTypeScript(string userCode)
		{
			var (imports, body) = SplitLeadingImports(userCode);
			var sb = new StringBuilder();
			if (!string.IsNullOrEmpty(imports))
			{
				sb.AppendLine(imports);
			}

			sb.AppendLine("interface __RemoteSink {");
			sb.AppendLine("  onResolve(hasValue: boolean, jsonValue: string | null): void;");
			sb.AppendLine("  onReject(message: string, stack: string | null): void;");
			sb.AppendLine("  onLog(level: string, message: string): void;");
			sb.AppendLine("}");
			sb.AppendLine("export default async function __remote_run(__sink: __RemoteSink): Promise<void> {");
			sb.AppendLine("  const __g: Record<string, any> = Function(\"return this\")();");
			sb.AppendLine("  const __origConsole = __g.console;");
			sb.AppendLine("  const __capture = (level: string) => (...args: unknown[]): void => {");
			sb.AppendLine("    const text = args.map((item: unknown): string => {");
			sb.AppendLine("      if (typeof item === \"string\") return item;");
			sb.AppendLine("      try {");
			sb.AppendLine("        const serialized = JSON.stringify(item);");
			sb.AppendLine("        return serialized !== undefined ? serialized : String(item);");
			sb.AppendLine("      } catch {");
			sb.AppendLine("        return String(item);");
			sb.AppendLine("      }");
			sb.AppendLine("    }).join(\" \");");
			sb.AppendLine("    __sink.onLog(level, text);");
			sb.AppendLine("  };");
			sb.AppendLine("  __g.console = {");
			sb.AppendLine("    log: __capture(\"log\"),");
			sb.AppendLine("    info: __capture(\"info\"),");
			sb.AppendLine("    warn: __capture(\"warn\"),");
			sb.AppendLine("    error: __capture(\"error\"),");
			sb.AppendLine("  };");
			sb.AppendLine("  try {");
			sb.AppendLine("    const __result: unknown = await (async (): Promise<unknown> => {");
			sb.AppendLine(body);
			sb.AppendLine("    })();");
			sb.AppendLine("    if (__result === undefined) {");
			sb.AppendLine("      __sink.onResolve(false, null);");
			sb.AppendLine("    } else {");
			sb.AppendLine("      let __json: string | null = null;");
			sb.AppendLine("      try {");
			sb.AppendLine("        const __serialized = JSON.stringify(__result);");
			sb.AppendLine("        __json = __serialized !== undefined ? __serialized : JSON.stringify(String(__result));");
			sb.AppendLine("      } catch {");
			sb.AppendLine("        __json = JSON.stringify(String(__result));");
			sb.AppendLine("      }");
			sb.AppendLine("      __sink.onResolve(true, __json);");
			sb.AppendLine("    }");
			sb.AppendLine("  } catch (__err: unknown) {");
			sb.AppendLine("    const __message = __err instanceof Error ? __err.message : String(__err);");
			sb.AppendLine("    const __stack = __err instanceof Error && typeof __err.stack === \"string\" ? __err.stack : null;");
			sb.AppendLine("    __sink.onReject(__message, __stack);");
			sb.AppendLine("  } finally {");
			sb.AppendLine("    __g.console = __origConsole;");
			sb.AppendLine("  }");
			sb.AppendLine("}");
			return sb.ToString();
		}

		public static string WrapJavaScript(string userCode)
		{
			var (imports, body) = SplitLeadingImports(userCode);
			var sb = new StringBuilder();
			if (!string.IsNullOrEmpty(imports))
			{
				sb.AppendLine(imports);
			}

			sb.AppendLine("export default async function __remote_run(__sink) {");
			sb.AppendLine("  const __g = Function(\"return this\")();");
			sb.AppendLine("  const __origConsole = __g.console;");
			sb.AppendLine("  const __capture = (level) => (...args) => {");
			sb.AppendLine("    const text = args.map((item) => {");
			sb.AppendLine("      if (typeof item === \"string\") return item;");
			sb.AppendLine("      try {");
			sb.AppendLine("        const serialized = JSON.stringify(item);");
			sb.AppendLine("        return serialized !== undefined ? serialized : String(item);");
			sb.AppendLine("      } catch {");
			sb.AppendLine("        return String(item);");
			sb.AppendLine("      }");
			sb.AppendLine("    }).join(\" \");");
			sb.AppendLine("    __sink.onLog(level, text);");
			sb.AppendLine("  };");
			sb.AppendLine("  __g.console = {");
			sb.AppendLine("    log: __capture(\"log\"),");
			sb.AppendLine("    info: __capture(\"info\"),");
			sb.AppendLine("    warn: __capture(\"warn\"),");
			sb.AppendLine("    error: __capture(\"error\"),");
			sb.AppendLine("  };");
			sb.AppendLine("  try {");
			sb.AppendLine("    const __result = await (async () => {");
			sb.AppendLine(body);
			sb.AppendLine("    })();");
			sb.AppendLine("    if (__result === undefined) {");
			sb.AppendLine("      __sink.onResolve(false, null);");
			sb.AppendLine("    } else {");
			sb.AppendLine("      let __json = null;");
			sb.AppendLine("      try {");
			sb.AppendLine("        const __serialized = JSON.stringify(__result);");
			sb.AppendLine("        __json = __serialized !== undefined ? __serialized : JSON.stringify(String(__result));");
			sb.AppendLine("      } catch {");
			sb.AppendLine("        __json = JSON.stringify(String(__result));");
			sb.AppendLine("      }");
			sb.AppendLine("      __sink.onResolve(true, __json);");
			sb.AppendLine("    }");
			sb.AppendLine("  } catch (__err) {");
			sb.AppendLine("    const __message = __err instanceof Error ? __err.message : String(__err);");
			sb.AppendLine("    const __stack = __err instanceof Error && typeof __err.stack === \"string\" ? __err.stack : null;");
			sb.AppendLine("    __sink.onReject(__message, __stack);");
			sb.AppendLine("  } finally {");
			sb.AppendLine("    __g.console = __origConsole;");
			sb.AppendLine("  }");
			sb.AppendLine("}");
			return sb.ToString();
		}

		private static (string Imports, string Body) SplitLeadingImports(string userCode)
		{
			string normalized = userCode.Replace("\r\n", "\n").Replace('\r', '\n');
			string[] lines = normalized.Split('\n');
			var importLines = new List<string>();
			int index = 0;
			bool insideMultilineImport = false;

			while (index < lines.Length)
			{
				string line = lines[index];
				string trimmed = line.Trim();

				if (insideMultilineImport)
				{
					importLines.Add(line);
					if (IsImportTerminated(trimmed)) insideMultilineImport = false;
					index++;
					continue;
				}

				if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//", StringComparison.Ordinal))
				{
					index++;
					continue;
				}

				if (IsImportStart(trimmed))
				{
					importLines.Add(line);
					if (!IsImportTerminated(trimmed)) insideMultilineImport = true;
					index++;
					continue;
				}

				break;
			}

			if (importLines.Count == 0) return (string.Empty, normalized);

			string imports = string.Join("\n", importLines);
			string body = index < lines.Length ? string.Join("\n", lines, index, lines.Length - index) : string.Empty;
			return (imports, body);

			static bool IsImportStart(string trimmedLine) =>
				trimmedLine.StartsWith("import ", StringComparison.Ordinal) ||
				trimmedLine.StartsWith("import{", StringComparison.Ordinal) ||
				trimmedLine.StartsWith("import*", StringComparison.Ordinal) ||
				trimmedLine.StartsWith("import\"", StringComparison.Ordinal) ||
				trimmedLine.StartsWith("import'", StringComparison.Ordinal);

			static bool IsImportTerminated(string trimmedLine)
			{
				if (trimmedLine.EndsWith(";", StringComparison.Ordinal)) return true;
				if (trimmedLine.Contains(" from ", StringComparison.Ordinal) &&
				    (trimmedLine.EndsWith("\"", StringComparison.Ordinal) ||
				     trimmedLine.EndsWith("'", StringComparison.Ordinal)))
				{
					return true;
				}

				return (trimmedLine.StartsWith("import\"", StringComparison.Ordinal) ||
				        trimmedLine.StartsWith("import'", StringComparison.Ordinal) ||
				        trimmedLine.StartsWith("import \"", StringComparison.Ordinal) ||
				        trimmedLine.StartsWith("import '", StringComparison.Ordinal)) &&
				       (trimmedLine.EndsWith("\"", StringComparison.Ordinal) ||
				        trimmedLine.EndsWith("'", StringComparison.Ordinal));
			}
		}
	}
}
