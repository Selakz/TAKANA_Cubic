#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace T3Framework.Static.Setting
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DropdownOptionsAttribute : Attribute
	{
		private readonly object[] options;

		public IEnumerable<T> GetOptions<T>() => options.OfType<T>();

		public DropdownOptionsAttribute(params object[] options)
		{
			this.options = options;
		}
	}
}