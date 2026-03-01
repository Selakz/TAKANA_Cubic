#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime.ECS;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace T3Framework.Runtime.VContainer
{
	public static class T3VContainerExtensions
	{
		public static RegistrationBuilder RegisterNotifiableProperty<T>(
			this IContainerBuilder builder, T initialValue, Lifetime lifetime = Lifetime.Singleton)
		{
			return builder.Register<NotifiableProperty<T>>(lifetime).WithParameter("initialValue", initialValue);
		}

		public static RegistrationBuilder RegisterViewPool<T>(
			this IContainerBuilder builder,
			PrefabObject prefab,
			Transform defaultTransform,
			Lifetime lifetime = Lifetime.Singleton) where T : IComponent
		{
			return builder.Register<ViewPool<T>>(lifetime)
				.WithParameter("prefab", prefab)
				.WithParameter("defaultTransform", defaultTransform)
				.As<IViewPool<T>>();
		}
		
		public static RegistrationBuilder RegisterViewPool<T>(
			this IContainerBuilder builder,
			ViewPoolInstaller installer,
			Lifetime lifetime = Lifetime.Singleton) where T : IComponent
		{
			return installer.Register<ViewPool<T>, T>(builder, lifetime);
		}
		
		public static RegistrationBuilder RegisterClassViewPool<T, TClass>(
			this IContainerBuilder builder,
			Dictionary<TClass, PrefabObject> prefabs,
			Transform defaultTransform,
			IClassifier<TClass> classifier,
			Lifetime lifetime = Lifetime.Singleton) where T : IComponent
		{
			return builder.Register<ViewPool<T, TClass>>(lifetime)
				.WithParameter("prefabs", prefabs)
				.WithParameter("defaultTransform", defaultTransform)
				.WithParameter("classifier", classifier)
				.As<IViewPool<T>>();
		}
		
		public static RegistrationBuilder RegisterClassViewPool<T, TClass>(
			this IContainerBuilder builder,
			ClassViewPoolInstaller<TClass> installer,
			IClassifier<TClass> classifier,
			Lifetime lifetime = Lifetime.Singleton) where T : IComponent
		{
			return installer.Register<ViewPool<T, TClass>, T>(builder, lifetime, classifier);
		}
	}
}