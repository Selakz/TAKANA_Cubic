#nullable enable

using VContainer;
using VContainer.Unity;

namespace T3Framework.Runtime.VContainer
{
	/// <summary> Used in hierarchy for <see cref="HierarchyInstaller"/> to find. </summary>
	public interface ISelfInstaller
	{
		public void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterComponent(this);
		}
	}
}