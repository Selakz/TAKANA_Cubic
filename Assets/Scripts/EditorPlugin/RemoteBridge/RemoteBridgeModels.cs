#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Serialization.Json;

namespace EditorPlugin.RemoteBridge
{
	public enum RemoteBridgeState
	{
		Stopped,
		Listening,
		Error
	}

	public readonly struct RemoteBridgeStatus : IEquatable<RemoteBridgeStatus>
	{
		public RemoteBridgeState State { get; }

		public int Port { get; }

		public string? ErrorMessage { get; }

		public RemoteBridgeStatus(RemoteBridgeState state, int port, string? errorMessage = null)
		{
			State = state;
			Port = port;
			ErrorMessage = errorMessage;
		}

		public bool Equals(RemoteBridgeStatus other) =>
			State == other.State && Port == other.Port && ErrorMessage == other.ErrorMessage;

		public override bool Equals(object? obj) => obj is RemoteBridgeStatus other && Equals(other);

		public override int GetHashCode() => HashCode.Combine((int)State, Port, ErrorMessage);
	}

	public static class RemoteBridgeSerializer
	{
		public static JsonSerializerSettings Settings { get; } = new()
		{
			Converters = { new I18NStringJsonConverter() },
			ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
		};

		public static string Serialize(object value) => JsonConvert.SerializeObject(value, Settings);

		public static T? Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, Settings);
	}

	public sealed class RemoteHealthResponse
	{
		public bool Ok { get; set; }

		public string AppVersion { get; set; } = string.Empty;

		public string? ApiVersion { get; set; }

		public bool HasLevel { get; set; }

		public bool TscAvailable { get; set; }
	}

	public sealed class RemoteLevelResponse
	{
		public bool HasLevel { get; set; }

		public string AppVersion { get; set; } = string.Empty;

		public string? ApiVersion { get; set; }

		public RemoteLevelPayload? Level { get; set; }
	}

	public sealed class RemoteLevelPayload
	{
		public string LevelPath { get; set; } = string.Empty;

		public int Difficulty { get; set; }

		public string OpenedAt { get; set; } = string.Empty;

		public long OpenedAtUnixMs { get; set; }

		public int Revision { get; set; }

		public int AudioLengthMilli { get; set; }

		public int ChartTimeMilli { get; set; }

		public RemoteSongInfoPayload SongInfo { get; set; } = new();
	}

	public sealed class RemoteSongInfoPayload
	{
		public string Id { get; set; } = string.Empty;

		public string PackId { get; set; } = string.Empty;

		public I18NString Title { get; set; } = new();

		public I18NString Composer { get; set; } = new();

		public I18NString Illustrator { get; set; } = new();

		public string BpmDisplay { get; set; } = string.Empty;

		public I18NString Description { get; set; } = new();

		public RemoteDifficultyInfoPayload? DifficultyInfo { get; set; }

		public Dictionary<int, RemoteDifficultyInfoPayload> Difficulties { get; set; } = new();
	}

	public sealed class RemoteDifficultyInfoPayload
	{
		public string LevelDisplay { get; set; } = string.Empty;

		public I18NString Charter { get; set; } = new();

		public bool IsCustom { get; set; }

		public string Name { get; set; } = string.Empty;
	}

	public sealed class RemoteExecuteRequest
	{
		public string? Code { get; set; }

		public string? Language { get; set; }

		public int? TimeoutMs { get; set; }

		public int? ExpectedRevision { get; set; }
	}

	public sealed class RemoteHistoryRequest
	{
		public int? ExpectedRevision { get; set; }

		public int? Steps { get; set; }
	}

	public sealed class RemoteHistoryResponse
	{
		public bool Success { get; set; }

		public string Stage { get; set; } = string.Empty;

		public int AppliedSteps { get; set; }

		public int? RevisionBefore { get; set; }

		public int? RevisionAfter { get; set; }

		public string? Error { get; set; }
	}

	public sealed class RemoteExecuteResponse
	{
		public bool Success { get; set; }

		public string Stage { get; set; } = string.Empty;

		public JToken? Result { get; set; }

		public bool HasResult { get; set; }

		public bool Committed { get; set; }

		public bool AutoCommitted { get; set; }

		public int? Revision { get; set; }

		public List<RemoteLogEntry> Logs { get; set; } = new();

		public string? Error { get; set; }

		public string? Stack { get; set; }
	}

	public sealed class RemoteLogEntry
	{
		public string Level { get; set; } = string.Empty;

		public string Message { get; set; } = string.Empty;
	}

	public sealed class RemoteResetResponse
	{
		public bool Ok { get; set; }

		public bool HasLevel { get; set; }
	}

	public sealed class RemoteErrorResponse
	{
		public string Error { get; set; } = string.Empty;
	}
}
