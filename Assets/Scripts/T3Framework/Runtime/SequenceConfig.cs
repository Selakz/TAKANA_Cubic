#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;

namespace T3Framework.Runtime
{
	[Serializable]
	public class SequencePriority
	{
		[SerializeField] private SequenceConfig config = default!;
		[SerializeField] private string id = default!;

		public int Value => config.Priorities[id];

		public static implicit operator int(SequencePriority priority) => priority.Value;
	}

	[CreateAssetMenu(fileName = "SequenceConfig", menuName = "T3FrameworkConfig/SequenceConfig", order = 0)]
	public class SequenceConfig : ScriptableObject
	{
		[SerializeField] private InspectorDictionary<string, int> priorities = new();

		public Dictionary<string, int> Priorities => priorities.Value;

		[ContextMenu("Sort Ascending")]
		public void SortAscend()
		{
			priorities.SortAscendByValue();
		}

		[ContextMenu("Sort Descending")]
		public void SortDescend()
		{
			priorities.SortDescendByValue();
		}
	}
}