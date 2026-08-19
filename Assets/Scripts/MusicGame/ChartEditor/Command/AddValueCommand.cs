#nullable enable

using System;
using UnityEngine;

namespace MusicGame.ChartEditor.Command
{
	public class AddValueCommand<T> : ICommand
	{
		public string Name => "Add Value";

		private readonly Func<T> getter;
		private readonly Action<T> setter;
		private readonly T addition;
		private readonly Func<T, T, T> addFunc;
		private T? oldValue;
		private bool hasDone = false;

		public AddValueCommand(Func<T> getter, Action<T> setter, T addition, Func<T, T, T> addFunc)
		{
			this.getter = getter;
			this.setter = setter;
			this.addition = addition;
			this.addFunc = addFunc;
		}

		public void Do()
		{
			oldValue = getter();
			setter(addFunc(oldValue!, addition));
			hasDone = true;
		}

		public void Undo()
		{
			if (!hasDone)
			{
				Debug.LogError("AddValueCommand.Undo: command has not been done yet.");
				return;
			}

			setter(oldValue!);
			oldValue = default;
			hasDone = false;
		}
	}
}