# 场景、Prefab 与脚本依赖审计

审计日期：2026-10-30  
方法：静态解析 Unity 序列化 GUID 引用，并定向扫描 C# 动态加载、场景切换、反射和字符串路径

## 1. 审计方法

本次审计建立了以下关系：

1. 从 `Assets/**/*.meta` 建立 3,157 个 GUID 到资源路径的映射。
2. 解析 1,110 个文本型 Unity/Shader/C# 文件的 GUID 引用边。
3. 以 `Assets/Scenes/Combat/Level1_CircularArena_Combat.unity` 为根计算完整依赖闭包。
4. 以当前 Build Settings 中的三个场景、PC URP Asset、URP Global Settings、GraphicsSettings 引用的内部阴影 Shader，以及 TMP 核心 `Resources` 为额外结构根。
5. 扫描全部 C# 中的 `Resources.Load`、`SceneManager.LoadScene*`、`AssetDatabase.LoadAssetAtPath`、`Shader.Find`、反射、运行时 Instantiate 与字符串路径。
6. 扫描全部 Shader 的 `#include` 文件依赖，避免 GUID 图漏掉文件名依赖。

结果：

- 目标场景直接引用 236 个唯一资源。
- 目标场景完整闭包为 1,506 个资源。
- 加上当前结构和 TMP Resources 后为 1,751 个 KEEP。
- 当前结构之外为 1,038 个资源。
- 其中只有 145 个低风险示例/教程资源进入当前 REMOVE 候选；893 个进入 REVIEW。

## 2. 场景依赖

### 2.1 逐入口依赖规模

| 场景或入口 | 是否在目标闭包 | 唯一于目标的依赖数 | 唯一依赖大小 MB | 判断 |
|---|---:|---:|---:|---|
| `Assets/Scenes/Combat/Level1_CircularArena_Combat.unity` | 是 | 1,506（完整闭包） | 1,482.85 | 目标 KEEP |
| `Assets/3DGamekit/Scenes/GamePlay/Level1.unity` | 否 | 1 | 38.51 | 仅场景文件本身唯一；依赖基本与目标共享 |
| `Assets/3DGamekit/Scenes/GamePlay/Level2.unity` | 否 | 206 | 212.48 | REVIEW；当前可经 Build Settings 与目标场景字符串切关 |
| `Assets/3DGamekit/Scenes/Start.unity` | 否 | 32 | 25.05 | REVIEW；当前 Build Settings 第一入口 |
| `Assets/3DGamekit/Scenes/UI/Loading.unity` | 否 | 7 | 0.05 | REVIEW；旧场景管理链仍可达 |
| `Assets/3DGamekit/Scenes/_TemplateScene.unity` | 否 | 6 | 6.31 | REVIEW；Editor 场景创建模板 |
| `Assets/Scenes/SampleScene.unity` | 否 | 4 | 0.12 | REMOVE 候选 |
| `Assets/TextMesh Pro/Examples & Extras/` | 否 | 136 | 5.53 | REMOVE 候选 |

### 2.2 全部场景核对

工程共有 40 个 `.unity` 文件，全部已纳入扫描：

- 目标场景：1 个。
- Game Kit 主场景和模板：`Start`、`Level1`、`Level2`、`Loading`、`_TemplateScene`。
- URP 模板场景：`SampleScene`。
- TextMesh Pro 示例场景：31 个，包括文本排版、样式、链接、遮蔽、输入框、Shader、Benchmark 等。
- `_Recovery`：2 个编辑器恢复场景。

当前 Build Settings 仍指向 `Start`、`Level1`、`Level2`，目标战斗场景未加入。直接修改 Build Settings 属于第二阶段，本阶段未修改。

## 3. Prefab 依赖

工程共有 219 个 Prefab 文件：

| 集合 | 数量 | 说明 |
|---|---:|---|
| 当前 KEEP 集 | 154 | 目标场景 123 个，加上旧 Build 场景/结构入口新增可达的 Prefab |
| 当前结构之外 | 65 | 未进入目标、Build Settings、URP 配置的静态闭包 |
| 目标场景 PrefabInstance 实例块 | 9,380 | 场景实例数量，不是唯一 Prefab 数量 |

目标场景中的代表性 Prefab 实例：

| Prefab | 实例数 |
|---|---:|
| `Ellen.prefab` | 1 |
| `Chomper.prefab` | 26 |
| `Grenadier.prefab` | 1 |
| `EelDogs.prefab` | 1 |
| `CameraRig.prefab` | 1 |
| `Level01Audio.prefab` | 1 |

数量最多的环境依赖主要是 `GroundCover01`、`GroundCover03`、`SmallRock02`、`SmallRock03`、`GroundCover02`、植被、墙面和圆形地砖 Prefab。这些 Prefab 的源资产必须保留，即使目标场景只需其中部分实例。

当前结构之外仍有 65 个 Prefab，不能直接删除的原因包括：

- UUID 链不覆盖 C# 类型引用和编辑器代码加载；
- 可能被旧场景的对象字段加载；
- Shader、材质和 VFX 存在二级依赖；
- 部分 Prefab 可能是后续竞技场编辑的备用环境模块；
- 没有 Editor 的 prefab missing-reference 检查。

## 4. C# 依赖和动态路径

工程共有 308 个 C# 文件：

| 集合 | 数量 | 当前判断 |
|---|---:|---|
| 目标场景闭包内 | 83 | KEEP |
| 当前结构 KEEP 集内 | 101 | KEEP |
| 当前结构之外 | 207 | REVIEW |

目标场景直接引用的脚本为 25 个，其余通过 Prefab、Animator 和 ScriptableObject 间接加入闭包。

### 4.1 Resources 加载

已发现的运行时代码路径：

| 脚本 | 加载路径 | 影响 |
|---|---|---|
| `Assets/3DGamekit/Scripts/Game/Localization/Translator.cs` | `Resources.Load<Translator>("Translator")` | 当前工程没有找到对应的 `Resources/Translator` 资源；旧本地化默认实例路径需要 Editor 验证 |
| `Assets/3DGamekit/Packages/SceneManagement/Runtime/ScreenFader.cs` | `Resources.Load<ScreenFader>("ScreenFader")` | `ScreenFader.prefab` 当前位于普通 Prefab 目录而非 Resources；旧路径是否实际执行需验证 |
| TextMesh Pro 示例脚本 | `Resources.Load<TMP_FontAsset>` 等 | 不影响目标场景时可由示例删除候选一并处理 |

`Assets/TextMesh Pro/Resources/` 虽未通过普通 GUID 链到达，但按 TMP 约定必须视为 KEEP。

没有发现 Addressables、AssetBundle、StreamingAssets 或 AssetReference 使用。

### 4.2 场景字符串

| 来源 | 值 | 结论 |
|---|---|---|
| 目标战斗场景中的 `TransitionPoint` | `Level2` | Level2 当前不能删除 |
| `Assets/3DGamekit/Prefabs/SceneControl/TransitionStart.prefab` | `Start` | Start 当前不能删除 |
| `SceneController.Transition` | 使用 `TransitionPoint.newSceneName` 调 `SceneManager.LoadSceneAsync` | 删除场景前必须清理所有旧 TransitionPoint |
| `SceneReloaderBehaviour` | 使用当前场景 `buildIndex` | 旧 Timeline 可能触发重载 |

### 4.3 Editor 与反射

| 脚本 | 动态行为 | 风险级别 |
|---|---|---|
| `NewSceneCreator.cs` | `AssetDatabase.FindAssets("_TemplateScene")`，从 Assets 路径加载 Prefab | `_TemplateScene` 和新场景创建工具必须 REVIEW |
| `PackageManagerChecker.cs` | 扫描 `PackageImportList.txt` | 影响旧包导入工作流 |
| `TimelinePlayableWizard.cs` | 程序集类型枚举与代码生成 | 不能按文件 GUID 判断 Editor 工具依赖 |
| `ReadmeEditor.cs` | Editor 反射加载 `TutorialInfo/Layout.wlt` | TutorialInfo 属低风险示例内容，但需确认不再打开 URP 帮助页 |
| `AutoSetQualitySetting.cs` | Editor 反射访问 GameView 并写 Library 标记 | 不属于目标运行时，但属于 Editor 工具，需 REVIEW |
| `GaussianBlurRenderFeature.cs` | `Shader.Find("Hidden/Universal/GaussianBlur")` | 对应 Shader 虽不在目标闭包，也不能按普通未引用纹理处理 |

### 4.4 Instantiate

保留脚本中已确认的运行时 Prefab 创建路径包括：

- HUD 血量图标实例化；
- 武器命中粒子和爆炸 VFX；
- Ragdoll 替换；
- ObjectPooler；
- 场景管理和本地化默认实例；
- Timeline/WorldBuilding Editor 生成对象。

这些 Prefab 字段多数为 Unity 对象引用，静态 GUID 图可识别；但 C# 编译层面的类型引用无法由 GUID 图表示，所以 P 级删除前必须做编译与 Editor 菜单测试。

## 5. 目标场景中的旧玩法组件

以下系统仍存在于目标场景或目标场景的 Prefab 依赖链中。它们不是“直接文件引用为零即可删除”的候选。

| 系统 | 证据 | 当前状态 | 后续处理 |
|---|---|---|---|
| 原玩家控制 | `PlayerController.cs`、`PlayerInput.cs` 各被目标场景直接引用 | KEEP | 新输入/移动系统完成后精确禁用 |
| 旧伤害系统 | `Damageable.cs` 出现在目标场景、Ellen、Chomper、Grenadier 和 DestructibleBox Prefab 链 | KEEP | 新 `CombatResolver` 成为唯一伤害权威后再禁用 |
| 旧 Chomper AI | 26 个 Chomper 实例，`ChomperBehavior` 与多个 SceneLinked SMB | KEEP | 选定一只对手并实现新 AI 后禁用其余旧行为 |
| Grenadier/EelDog | 目标场景各有实例 | KEEP | 非参战角色保留外观，禁用旧 AI/攻击 |
| 检查点 | 目标场景包含 1 个 `CheckpointGroup` 和 5 个 `Checkpoint` 序列化引用 | KEEP | 用战斗重开逻辑替代后再移除 |
| 关卡切换 | 1 个 `TransitionPoint`，目标为 Level2 | KEEP | 必须改为单场景重开或新流程后再删 |
| 旧 HUD | 1 个 `HealthUI`；`DialogueCanvasController` 也在目标闭包 | KEEP | 新 HUD 验证后替换 |
| 死亡区/陷阱 | 当前 KEEP 闭包包含 `DeathVolume.prefab` | KEEP | 1v1 场地边界确认后处理 |
| Timeline/机关/解谜对象 | 场景和 Prefab 链仍保留大量旧对象 | KEEP | 逐对象审计，不按目录删除 |

## 6. 需要的 Editor 复核

第二阶段打开当前工程后，必须补充以下 Unity 原生验证：

1. Console 无编译错误。
2. 目标场景无 Missing Script、Missing Prefab 和粉色材质。
3. `AssetDatabase.GetDependencies(targetScene, true)` 与本次静态闭包比对。
4. 逐个 Animator Controller 检查状态中引用的 Clip 和 StateMachineBehaviour。
5. 检查 Lighting Data、Reflection Probe、Light Probe Group 与 NavMesh 的 Editor 关联。
6. 扫描 Prefab Stage 和所有 Editor 菜单入口。
7. 对 `Resources`、`StreamingAssets`、Addressables 实际目录再做一次 Unity 原生核对。
8. 运行目标场景、暂停/恢复、重开和 Windows Player 构建。
