# CircularArea 1v1 格斗项目资源盘点

审计日期：2026-10-30  
工程根目录：`E:\Unity Engine\AssetImportSandbox_InternationalURP`  
审计阶段：第一阶段只审计，不删除、不移动、不改写 Unity 资源  
审计性质：静态文件与 Unity 序列化 GUID 依赖审计；未执行 Editor 编译、Play Mode 或 Player 构建

## 1. 结论摘要

当前工程仍然是一个带完整 3D Game Kit 示例内容和 TextMesh Pro 示例内容的资源工程。目标战斗场景虽然存在，但它只是原始 `Level1` 的完整副本，当前尚未变成精简的 1v1 格斗工程。

| 指标 | 结果 |
|---|---:|
| Unity Editor | `6000.3.27f1` |
| 渲染管线 | URP `17.3.0` |
| Input System | `1.20.1` |
| `activeInputHandler` | `2`（当前配置同时启用 Input Manager 与 Input System） |
| `Assets` 全部文件 | 5,946 |
| `.meta` 文件 | 3,157 |
| 非 `.meta` 资源文件 | 2,789 |
| `Assets` 总大小 | 2,622.63 MB |
| 正式战斗场景 | 1 个 |
| 工程内全部 `.unity` 场景 | 40 个 |
| Prefab 文件 | 219 个 |
| C# 文件 | 308 个 |

目标场景的 GUID 依赖闭包和当前项目结构依赖闭包如下：

| 集合 | 文件数 | 大小 | 含义 |
|---|---:|---:|---|
| 目标战斗场景直接引用 | 236 | 约 280 MB | 场景文件直接序列化的唯一资源 |
| 目标战斗场景完整静态闭包 | 1,506 | 1,482.85 MB | 继续沿 Prefab、材质、Animator、Shader、音频等 GUID 链得到的依赖 |
| 当前结构 KEEP 集 | 1,751 | 1,743.45 MB | 目标场景闭包，加上 Build Settings、URP 配置和 TMP 核心 Resources |
| 当前结构 KEEP 集之外 | 1,038 | 879.19 MB | 静态上没有被当前工程入口链直接引用 |

“KEEP 集之外”不等于可删除。其中只有 145 个文件属于低风险示例/教程候选；其余 893 个文件必须标记 `REVIEW`，原因见 `ASSET_KEEP_REMOVE_QUARANTINE.md`。

## 2. 工程与配置

| 项目 | 实际值 | 状态 |
|---|---|---|
| Unity 版本 | `6000.3.27f1` | 符合固定目标，不得改动 |
| URP | `com.unity.render-pipelines.universal 17.3.0` | 符合目标 |
| PC Render Pipeline | `Assets/Settings/PC_RPAsset.asset` | 被 `GraphicsSettings.asset` 与 `QualitySettings.asset` 引用 |
| 当前 Quality | `Fantastic`，索引 `2` | 保持现状，不在本阶段调整 |
| Color Space | Linear | 保持现状 |
| Input System | `com.unity.inputsystem 1.20.1` | 已安装 |
| Input Handler | 两套输入均启用 | 清理旧 `PlayerInput` 前不能只关闭旧输入设置 |
| Build 目标 | Standalone Windows 64 位 | 与当前原型目标一致 |

主要包包括 URP、Input System、Cinemachine、AI Navigation、ProBuilder、Polybrush、Timeline、Visual Scripting、Splines、UGUI、TextMeshPro、Test Framework。没有 Addressables 包。

## 3. 场景盘点

### 3.1 目标战斗场景

目标文件：

`Assets/Scenes/Combat/Level1_CircularArena_Combat.unity`

| 指标 | 结果 |
|---|---:|
| 场景文件大小 | 40,378,691 bytes |
| YAML 行数 | 1,062,179 |
| 本地 GameObject 块 | 394 |
| Transform 块 | 9,644 |
| MonoBehaviour 块 | 106 |
| PrefabInstance 块 | 9,380 |

目标场景与原 `Assets/3DGamekit/Scenes/GamePlay/Level1.unity` 的静态差异只有 `CameraRig` 的位置和旋转。也就是说，它仍完整保留 Level 1 的示例关卡对象、人物、敌人、旧玩法脚本和 UI。

已确认的关键内容：

| 内容 | 当前情况 |
|---|---|
| 环境 | `Environment/Structures/CircularArea` 存在，局部位置约为 `(-120, 21, -43)` |
| 玩家 | 1 个 Ellen 实例，Prefab 为 `Assets/3DGamekit/Prefabs/Characters/Ellen/Ellen.prefab` |
| 主要敌人类 | 26 个 Chomper 实例 |
| 其他敌人 | 1 个 Grenadier，1 个 EelDog |
| 相机 | 1 个 `CameraRig`，位置为约 `(22.77, -6.99, 62.06)` |
| 音频控制 | 1 个 `Level01Audio.prefab` 实例 |
| 场景入口 | 尚未加入 Build Settings |

### 3.2 当前 Build Settings

`ProjectSettings/EditorBuildSettings.asset` 当前只包含：

| 顺序 | 场景 | 状态 |
|---:|---|---|
| 1 | `Assets/3DGamekit/Scenes/Start.unity` | 旧示例入口 |
| 2 | `Assets/3DGamekit/Scenes/GamePlay/Level1.unity` | 原始关卡 |
| 3 | `Assets/3DGamekit/Scenes/GamePlay/Level2.unity` | 旧示例关卡 |
| — | `Assets/Scenes/Combat/Level1_CircularArena_Combat.unity` | **未加入 Build Settings** |

此外，目标场景中存在一个 `TransitionPoint`，其 `newSceneName` 为 `Level2`。因此在移除该切关对象之前，Level2 仍是运行时可到达路径，不能仅因为“不是目标场景”就删除。

### 3.3 全部场景

静态扫描覆盖了全部 40 个 `.unity` 文件。按项目角色归类如下：

| 场景组 | 数量 | 当前判断 |
|---|---:|---|
| 目标战斗场景 | 1 | KEEP |
| 原 Level1、Start、Level2、Loading | 4 | 当前结构 KEEP；最终精简前需重构入口与切关 |
| `_TemplateScene` | 1 | REVIEW |
| `Assets/Scenes/SampleScene.unity` | 1 | 低风险 REMOVE 候选 |
| TextMesh Pro 示例场景 | 31 | 低风险 REMOVE 候选 |
| `Assets/_Recovery` 场景 | 2 | REVIEW，先确认真实自动恢复是否仍需保留 |

全部场景及唯一依赖规模的详细结果见 `SCENE_AND_SCRIPT_DEPENDENCIES.md`。

## 4. 目标场景依赖类型

以下数量是目标场景完整静态闭包中的唯一资源文件数，不是场景实例数：

| 类型 | 数量 | 大小 MB |
|---|---:|---:|
| OGG 音频 | 411 | 24.19 |
| TIF 纹理 | 372 | 934.66 |
| FBX 模型/动画源 | 180 | 166.67 |
| Material | 141 | 0.61 |
| Prefab | 123 | 7.66 |
| PNG | 95 | 52.53 |
| C# | 83 | 0.19 |
| AnimationClip | 30 | 0.59 |
| Shader Graph | 21 | 4.00 |
| Animator Controller | 16 | 0.19 |
| ScriptableObject/Asset | 14 | 245.40 |
| Playable | 3 | 0.04 |
| Shader 文本 | 2 | 接近 0 |
| Scene | 1 | 38.51 |

直接由目标场景文件引用、但不包含 Prefab 内部后续依赖的唯一资源为 236 个。这个差额说明 Prefab、动画、材质与 ScriptableObject 的间接链不能遗漏。

## 5. 工作区与文档

| 目录/文件 | 内容 |
|---|---|
| `Assets/3DGamekit` | 完整 Game Kit 资源、场景、脚本、嵌入包、音频、Prefab、文档 |
| `Assets/TextMesh Pro` | TMP 运行资源、Shader，以及大量示例/教程内容 |
| `Assets/Scenes/Combat` | 当前唯一正式战斗副本 |
| `Assets/_Recovery` | 2 个编辑器恢复场景，其中一个约 38.51 MB |
| `Docs/ProjectReference` | V2.0 场景、战斗系统和执行工作流设计 |
| `Packages` | `manifest.json` 与 `packages-lock.json` |

已有设计基线要求保留完整 Level1 的场景对象，并通过 `__CombatOverlay` 接入新战斗逻辑。当前工程中尚未发现 `__CombatOverlay`、`ArenaModeController`、`CombatResolver` 或新战斗脚本。

## 6. 审计限制

本次没有启动或切换到当前工程的 Unity Editor，因此没有执行 Unity `AssetDatabase` 实测、场景打开、Animator 播放、Console 编译检查、Play Mode 或 Player Build。当前已连接的 Unity Editor 属于另一个工程 `MotionFightPrototype`，不能用于本工程验证。

静态 GUID 审计能可靠覆盖直接序列化引用和大多数 Prefab/材质/动画链，但不能完全覆盖：

- Shader `#include` 的文件路径依赖；
- `Resources` 约定加载；
- 非常规反射；
- 编辑器菜单生成的资源；
- LightingData、ReflectionProbe、NavMeshData 的 Editor 内部关联；
- 运行时以字符串指定场景或资源名的路径。

因此本阶段没有执行任何删除。
