#nullable enable

using System;

namespace T3Framework.Runtime.ECS
{
	public interface IClassifier<TClass>
	{
		TClass? Classify(IComponent component);

		bool IsOfType(IComponent component, TClass type);

		bool IsSubType(TClass subType, TClass type);
	}

	public abstract class SpecificClassifier<TModel, TClass> : IClassifier<TClass>
	{
		public abstract TClass? Classify(IComponent<TModel> component);

		public abstract TClass? Default { get; }

		public TClass? Classify(IComponent component) =>
			component is IComponent<TModel> modelComponent ? Classify(modelComponent) : Default;

		public abstract bool IsOfType(IComponent component, TClass type);

		public abstract bool IsSubType(TClass subType, TClass type);
	}

	public class TypeClassifier<TModel> : SpecificClassifier<TModel, Type>
	{
		public override Type? Default => null;


		public override Type? Classify(IComponent<TModel> component) => component.Model?.GetType();

		public override bool IsOfType(IComponent component, Type type)
		{
			if (component is not IComponent<TModel> modelComponent) return false;
			return modelComponent.Model?.GetType() == type;
		}

		public override bool IsSubType(Type subType, Type type) => subType == type;
	}

	public class BoolClassifier<TModel> : SpecificClassifier<TModel, bool>
	{
		public override bool Default => false;


		public Predicate<TModel> Predicate { get; }

		public BoolClassifier(Predicate<TModel> predicate) => Predicate = predicate;

		public override bool Classify(IComponent<TModel> component) => Predicate(component.Model);

		public override bool IsOfType(IComponent component, bool type)
		{
			if (component is not IComponent<TModel> modelComponent) return !type;
			return Predicate(modelComponent.Model) == type;
		}

		public override bool IsSubType(bool subType, bool type) => subType == type;
	}
}