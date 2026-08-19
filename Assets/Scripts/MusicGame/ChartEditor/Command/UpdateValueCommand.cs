#nullable enable

using System;
using UnityEngine;

namespace MusicGame.ChartEditor.Command
{
	public class UpdateValueCommand<T> : ICommand
	{
		public string Name => "Update Value";

		private readonly Func<T> getter;
		private readonly Action<T> setter;
		private readonly T value;
		private T? oldValue;
		private bool hasDone = false;

		public UpdateValueCommand(Func<T> getter, Action<T> setter, T value)
		{
			this.getter = getter;
			this.setter = setter;
			this.value = value;
		}

		public void Do()
		{
			oldValue = getter();
			setter(value);
			hasDone = true;
		}

		public void Undo()
		{
			if (!hasDone)
			{
				Debug.LogError("UpdateValueCommand.Undo: command has not been done yet.");
				return;
			}

			setter(oldValue!);
			oldValue = default;
			hasDone = false;
		}
	}
}