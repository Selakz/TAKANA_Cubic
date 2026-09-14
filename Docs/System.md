所有系统均需要继承类`HierarchySystem<T>`，系统分为三类：
- Installer：通常每个模块或子模块只有一个Installer，用于注册该模块通用的数据，但不用注册任何业务系统或服务。
- System：用于实现具体业务逻辑的系统。
- Service：用于提供模块内的复用逻辑的服务。

所有系统都可以实时被禁用或启用，因此无需担心实现相似功能的系统相互干扰的问题。

## System和Service的注意事项

### 区别

- 二者的代码结构大致相同，区别在于System不应有任何非调试目的的公开属性或方法，而Service主要提供公开属性或方法。
- 每个Service应当实现一个接口，便于后续进行服务替换，如下所示。
```csharp
public interface I[ClassName]Service{}

public class [ClassName]Service : HierarchySystem<[ClassName]Service>, I[ClassName]Service{
    // Serializable and Public
	public override bool AsImplementedInterfaces => true;
    
    /* 服务的具体业务代码内容 */
}
```

### 代码顺序

以下是一个System和Service的代码模板。注意其中的行注释均为固定内容，不可省略或修改，用于划分代码模块。每个模块都并非必需，当对应的模块存在时，就应该保留对应的行注释。
```csharp
#nullable enable

using ...;

namespace ...
{
    public class [ClassName]System : HierarchySystem<[ClassName]System>
    {
        // Serializable and Public
        [SerializeField] public [Type] data1 { get; set; } = default!;
        [SerializeField] private [Type] data2 = default!;
        
        // Event Registrars
        /* 用于控制自身启用与否的事件注册 */
        protected override IEventRegistrar[] EnterRegistrars => [
            new XXXRegistrar(),
        ]
        
        /* 用于实现业务逻辑的事件注册 */
        protected override IEventRegistrar[] EnableRegistrars => [
            new XXXRegistrar(),
        ]
        
        // Private
        [Inject] private readonly [Type] data3 = default!;
        
        private [Type]? data4;
        
        // Static
        private static readonly [Type] data5 = default!;
        
        // Defined Functions
        private void DoSomething(){}
        
        // System Functions
        public override void Awake(){}
    }
}
```

### 事件注册

系统的事件注册优先使用Registrar，而不是直接在OnEnable和OnDisable中处理。例如，监听某个按钮和某个NotifiableProperty的系统的代码如下所示。
```csharp
protected override IEventRegistrar[] EnableRegistrars => [
    new ButtonRegistrar(button, () => DoSomething()),
    new PropertyRegistrar(notifiableProperty, value => DoSomething(value))
]
```

优先使用Lambda表达式定义Registrar中的回调，除非需要复用或者内容过长。

如果要订阅的事件没有对应的Registrar，优先考虑先实现一个对应的Registrar，见[](Registrar.md)。

关于数据处理相关内容，见[](DatasetAndViewPool.md)
