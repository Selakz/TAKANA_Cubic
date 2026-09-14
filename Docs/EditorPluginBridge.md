# EditorPlugin Bridge — PuerTS 插件系统

## 概述

基于 PuerTS (QuickJS 后端) 实现的热重载插件系统。用户编写 TypeScript 代码实现编辑器插件。

- **技术栈**: PuerTS 3.0.2, QuickJS, TypeScript (编译为 .mjs)
- **后端**: QuickJS (轻量, ~3MB)
- **目标**: 用户可在 `persistentDataPath/Plugins/<插件名>/main.ts` 目录下编写 TS 插件

目前所有内部ts代码全部编译为单个mjs文件，Agent只需专注于ts代码编写，无需关心与mjs相关的任何工作，如编译mjs的过程、判断mjs是否与现有ts代码同步等。换言之，Agent任何时候都无需读取mjs文件。

目前该功能还未上线，不用关心重构的兼容性问题。

## 架构分层

### C# 侧 — EditorPlugin 程序集

以下是部分代码文件介绍，可能不包含目录下的所有文件：

```
Assets/Scripts/EditorPlugin/
├── PuerTS/                          # Layer 1: PuerTS 基础设施
│   ├── PluginRuntimeLoader.cs       # ILoader + IModuleChecker + IResolvableLoader
│   └── PluginRuntimeEnv.cs          # JsEnv 封装, 构造时加载 bridge.mjs
├── PluginSystem/                    # Layer 2: 插件生命周期管理
│   ├── PluginManifest.cs            # 插件清单数据
│   ├── PluginInstance.cs            # 单个插件实例, 公开 Env 属性
│   ├── T3BridgeBootstrapService.cs  # T3模式专属: T3 桥接初始化
│   ├── PropertyWrapper.cs           # 泛型属性包装, 暂存 → Command
│   └── StagingRegistry.cs           # 收集脏属性 → BatchCommand
├── EditorIntegration/               # Layer 3: 编辑器集成
│   ├── UI/                          # UI 组件
│   ├── EditorPluginInstaller.cs     # 安装器, 注册插件数据集
│   ├── PluginComponent.cs           # 插件组件, 存储插件实例
│   ├── PluginManageService.cs       # HierarchySystem Service, 管理插件加载/卸载
└── Shared/                            # 存放ts侧接受的C#侧 Api对应的声明

```

### TypeScript 侧 — 桥接代码

```
ts/
├── main.ts                  # 通用模块: 仅注册 T3Time, 导出空 __bridge_init
├── model.ts                 # 通用类型: T3Time, Wrapper<T>, T3TimeWrapper, ComponentModel/Snapshot
└── t3/
    ├── main.ts              # T3 模块: 注册 HitType/HitModel/HoldModel/getT3Context, 导出 __bridge_init
    ├── t3notes.ts           # T3 音符类型: HitModel/Snapshot, HoldModel/Snapshot
    ├── t3chart.ts           # T3 谱面: ChartSnapshot
    ├── t3context.ts         # T3 上下文: T3Context, getT3Context()
    └── global.d.ts          # 编译辅助: 声明 globalThis 变量
```

## 插件生命周期

```
PluginManageService (HierarchySystem, 挂载在场景中)
  ├─ [Inject] NotifiableProperty<LevelInfo?>  ← 当前关卡
  ├─ [Inject] CommandManager                   ← 编辑器命令管理器
  │
  ├─ Refresh()
  │   ├─ 扫描 persistentDataPath/Plugins/ 下每个子目录
  │   ├─ 读取 manifest.json → PluginManifest
  │   ├─ new PluginInstance(manifest, dir)
  │   │   ├─ new PluginRuntimeEnv(dir) → 创建 JsEnv, 加载 bridge.mjs (通用)
  │   │   └─ env.ExecuteModule(main.ts) → 加载用户插件
  │   └─ T3BridgeBootstrap.Initialize(env, chart, cmd)
  │        └─ env.BridgeObject.__t3_bridge_init(api) → 注册 getT3Context
  │
  └─ LevelInfo 变化时 → Refresh
```

## 三种对象类别

| 类别 | 命名后缀 | 修改行为 | 来源 | 生命周期 |
|---|---|---|---|---|
| **Snapshot** | `*Snapshot` | 暂存, commit() 提交 | C# 已有实例的包装 | 跟随 C# 实例 |
| **Model** | `*Model` | 立即执行 | TS 中 `new XxxModel()` 创建, 或 `snapshot.getModel()` 获取副本 | 独立于关卡, 直到 `addNote()` 等操作注入 |
| **Wrapper** | `Wrapper<T>` / `*Wrapper` | 暂存, commit() 提交 | 对 Snapshot 属性的包装 | 跟随所属 Snapshot |

### 修改路径

- **Snapshot/Wrapper**: `note.hitType.value = X` → PropertyWrapper 暂存 → commit() → BatchCommand
- **Model**: `model.hitType = X` → 直接修改 JS 对象, 不影响 C#
- **getModel()**: 返回独立副本, 修改不影响 Snapshot

## 数据流: PropertyWrapper → Command

```
用户 TS: note.hitType.value = HitType.Slide
                    │
                    ▼ (PuerTS 编组, 调用 C# setter)
C# PropertyWrapper<int>.set_value(Slide)
  ├─ 首次 dirty → getter() 读旧值
  ├─ registry.Add(component, apply, revert)
  └─ stagedValue = Slide, dirty = true

用户 TS: ctx.commit()
                    │
                    ▼
BridgeBootstrap → api.stagingFlush()
                    │
                    ▼
StagingRegistry.Flush()
  ├─ 遍历 dirty entries
  ├─ new UpdateComponentCommand(component, apply, revert) × N
  ├─ new BatchCommand(commands, "Plugin edit")
  └─ commandManager.Add(batchCommand)  → 可撤销
```

## 模式扩展机制

插件系统预期支持不同的游戏模式, 每种模式基于相同的ChartInfo框架，但有不同的隐含预设和独立的类型定义与桥接代码。

### 当前: T3 模式

- C# 端: `T3BridgeBootstrap.cs` (含 `BuildNoteSnapshot` 中的 Hit/Hold switch)
- TS 端: `ts/t3/` 下的所有文件
- 用户接口: `.d.ts` 位于 `StreamingAssets/EditorPlugin/types/t3/`

### 通用接口 (所有模式共享)

- `.d.ts`: `StreamingAssets/EditorPlugin/types/model.d.ts`
  - `T3Time`, `Color`, `Wrapper<T>`, `ComponentModel`, `ComponentSnapshot`
- TS 桥接: `ts/model.ts`, `ts/main.ts`

### 新增模式的步骤

1. 在 `ts/` 下新建 `<模式名>/` 目录
2. 实现模式专属的 Model/Snapshot class（参照 `ts/t3/`）
3. 创建 `<模式名>/main.ts` 注册 globals, 导出 `__xxx_bridge_init`
4. C# 端新增对应的 `XxxBridgeBootstrap.cs`
5. 创建 `StreamingAssets/EditorPlugin/types/<模式名>/` 下的 `.d.ts`
6. `PluginManageService` 根据当前模式调用对应的 Bootstrap

## 目前的类型映射 (C# ↔ TS)

| C# 类型 | TS 表示 | 说明 |
|---|---|---|
| `ChartComponent` (Model is Hit) | `HitSnapshot` — class | `HitType` 属性为 `Wrapper<number>` (枚举值) |
| `ChartComponent` (Model is Hold) | `HoldSnapshot` — class | 含 `timeJudge` 和 `timeEnd` 两个时间 wrapper |
| `Hit` (C# Model) | `HitModel` — class | 纯数据, 可 new |
| `Hold` (C# Model) | `HoldModel` — class | 纯数据, 可 new |
| `HitType` 枚举 | `HitType { Tap, Slide }` | 运行时为 `{Tap:0, Slide:1, 0:"Tap", 1:"Slide"}` |
| `T3Time` (struct) | `T3Time` — class, 底层存 `int milli` | C# ↔ TS 通过 `T3TimeWrapper` 转换 |
| `PropertyWrapper<T>` | `Wrapper<T>` — interface | C# 类, PuerTS 自动编组为 JS 对象 |

### 关键: `T3TimeWrapper`

因为 C# 的 `PropertyWrapper` 底层存 `int` (毫秒), 而用户接口声明为 `Wrapper<T3Time>`, 桥接层通过 `T3TimeWrapper` 做转换:

```
TS 写: wrapper.value = new T3Time(2000)  →  T3TimeWrapper.set  →  inner.value = 2000
TS 读: wrapper.value                     →  T3TimeWrapper.get  →  new T3Time(inner.value)
```

## 暂存模式 (Staging)

当前采用 **每属性首次 dirty 时注册 apply/revert, 多次修改 coalescing**:

```
首次 set:   dirty = false → getter() 读旧值 → registry.Add(apply, revert) → dirty = true
后续 set:   dirty = true  → 仅更新 stagedValue (apply 指向最新值, revert 保持最旧值)
commit():   Flush() → 每个 dirty wrapper 产出一个 UpdateComponentCommand
```

## 相关文件路径

| 类型 | 路径 |
|---|---|
| C# 桥接 | `Assets/Scripts/EditorPlugin/` |
| TS 桥接源码 | `Assets/Scripts/EditorPlugin/PluginSystem/ts/` |
| 编译产物 | `Assets/Resources/EditorPlugin/bridge.mjs` |
| 用户类型声明 | `Assets/StreamingAssets/EditorPlugin/types/` |
| 插件目录 | `Application.persistentDataPath/Plugins/<plugin>/main.ts` |
