dataset存储数据，viewPool决定数据对应的视图是否出现在世界中。

以下是一个最简单的实现：每当`trackDataset`里发生组件添加或移除时，就添加或移除decoratorPool里对应的视图。此代码仅为演示，实际业务中如果viewPool与dataset完全同步，请使用更简洁的[](../Assets/Scripts/T3Framework/Runtime/ECS/AutoViewPoolRegistrar.cs)

```csharp
public class TrackDecoratorSystem : HierarchySystem<TrackDecoratorSystem>
{
    // Event Registrars
    protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
    {
        new DatasetLifetimeRegistrar<ChartComponent>(trackDataset,
            component => new CustomRegistrar(
                () => decoratorPool.Add(component),
                () => decoratorPool.Remove(component)), true)
    };

    // Private
    [Inject] private IDataset<ChartComponent> trackDataset = default!;
    [Inject] private IViewPool<ChartComponent> decoratorPool = default!;
}
```