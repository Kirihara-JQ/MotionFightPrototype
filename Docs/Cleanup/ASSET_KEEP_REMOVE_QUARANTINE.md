# 资源保留、删除候选与隔离清单

审计日期：2026-10-30  
状态：执行受阻；未删除、未移动、未修改 `.meta`

## 0. 执行状态（2026-10-30）

本次清理在删除开始前停止，原因是完整工程的可回退 Git 基线无法确认：

- 当前工程 `E:\Unity Engine\AssetImportSandbox_InternationalURP` 不是 Git 仓库。
- 同级目录 `E:\Unity Engine\MotionFightPrototype` 配置了指定远程仓库，但其工作区是另一套缺少战斗场景的工程。
- 该 Git 工作区共有 21 项未提交变化，不能作为当前完整工程的干净回退点。
- 因此本清单中的所有 `REMOVE` 项目均保持原状，没有进入删除操作。

## 1. 分类口径

- `KEEP`：目标战斗场景直接或间接引用，或当前 Build Settings、URP 配置、TMP 约定加载所必需。
- `REMOVE`：当前入口链中没有直接或间接引用，且未发现字符串加载、Resources 动态加载或 Editor 路径依赖；仍需在第二阶段通过 Unity Editor 导入、编译、打开场景和构建验证后删除。
- `REVIEW`：静态上未进入当前入口闭包，但存在脚本编译、Shader、Animator、光照、NavMesh、动态加载、项目配置或人工使用上的不确定性。

删除候选只针对非 `.meta` 资源文件计数。第二阶段应通过 Unity `AssetDatabase` 或 Project 窗口删除，使 `.meta` 与资源原子处理，禁止手工删除 `.meta`。

## 2. 当前分类总表

| 分类 | 文件数 | 大小 MB | 说明 |
|---|---:|---:|---|
| KEEP | 1,751 | 1,743.45 | 目标场景闭包 + 当前结构入口 + TMP 核心 Resources |
| REMOVE | 145 | 5.789 | 已标记为候选；因 Git 回退基线未确认，执行状态为 BLOCKED |
| REVIEW | 893 | 873.40 | 不能凭静态无引用直接删除 |
| 合计 | 2,789 | 2,622.63 | 与 `Assets` 非 `.meta` 文件总数一致 |

## 3. KEEP

| 资源路径或路径族 | 类型 | 目标场景直接引用 | 间接使用 | 动作 | 判断依据 | 删除风险 | 删除前必须验证 |
|---|---|---|---|---|---|---|---|
| `Assets/Scenes/Combat/Level1_CircularArena_Combat.unity` | Scene | 是（根） | 是 | KEEP | 唯一正式战斗入口 | 会删除整个原型 | 无；继续保留 |
| `Assets/3DGamekit/Prefabs/Characters/Ellen/Ellen.prefab` 及其闭包 | Prefab、FBX、材质、Animator、Clip、纹理 | 是 | 是 | KEEP | 当前唯一 Ellen 实例来源 | 玩家模型、骨架或动画损坏 | 打开 Prefab、播放动画并通过 1v1 实机测试 |
| `Assets/3DGamekit/Prefabs/Characters/Enemies/Chomper/Chomper.prefab` 及其闭包 | Prefab、FBX、材质、Animator、Clip、音效 | 是 | 是 | KEEP | 当前 26 个 Chomper 实例来源，未来选一只作对手 | 怪兽消失、动画丢失、NavMesh/AI 失效 | 选定唯一对手并完成战斗验证后才可讨论 Prefab 精简 |
| CircularArea 环境 Prefab、石墙、圆形地面、楼梯、平台、岩石、植被、灯光和后处理 | Prefab、FBX、Material、Texture、Shader、Lighting | 是 | 是 | KEEP | Level1 与目标场景共享环境 | 场地缺面、碰撞或粉紫材质 | Scene/Game 视图实机检查；材质与碰撞测试 |
| 目标场景直接引用的 21 个 Shader Graph、2 个 Shader 和对应材质 | Shader、Material | 是 | 是 | KEEP | 静态闭包证明被保留场景使用 | 材质显示异常或粉紫 | Shader 编译、Play Mode 全场景截图 |
| 目标场景闭包中的 411 个 OGG | AudioClip | 是 | 是 | KEEP | Prefab、场景和音频脚本序列化引用 | 缺失环境音、攻击或角色反馈 | 音频播放测试与 Console 检查 |
| 目标场景闭包中的 180 个 FBX、30 个 AnimationClip、16 个 Animator Controller | 模型、动画 | 是 | 是 | KEEP | 人物、敌人、机关和 VFX 的静态依赖 | 绑定、Clip 或状态机损坏 | Animation 预览、Play Mode、Animator 参数检查 |
| `Assets/3DGamekit/Scenes/GamePlay/Level1.unity` | Scene | 否 | 当前结构入口 | KEEP | 当前 Build Settings 引用；设计文档要求作为只读基准 | 无法对照原始关卡或重建副本 | 更新 Build Settings 前保持 |
| `Assets/3DGamekit/Scenes/Start.unity` | Scene | 否 | 当前结构入口 | KEEP | 当前 Build Settings 第一位；`TransitionStart.prefab` 含 `Start` 字符串 | 打包入口与旧 UI 流程中断 | 替换入口并实测后再改判 |
| `Assets/3DGamekit/Scenes/GamePlay/Level2.unity` | Scene | 否 | 运行时字符串 | KEEP | 当前 Build Settings；目标场景 `TransitionPoint.newSceneName = Level2` | 保留的旧触发器报错或错误切关 | 删除/替换目标场景中的 Level2 TransitionPoint 并更新 Build Settings |
| `Assets/Settings/PC_RPAsset.asset`、`PC_Renderer.asset` 及依赖 | URP 配置 | 否 | ProjectSettings | KEEP | Graphics/Quality 设置直接引用 | 整个工程渲染管线失效 | 不得删除 |
| `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset` | URP 配置 | 否 | ProjectSettings/URP | KEEP | 全局 URP 配置入口 | URP 全局设置丢失 | 不得删除 |
| `Assets/Settings/SampleSceneProfile.asset` | URP Volume Profile | 否 | 当前旧场景结构 | KEEP（当前） | 当前 Build 场景闭包仍使用；目标场景最终不需要时再重分类 | 旧场景或当前后处理配置丢失 | 替换旧 Volume 后重新计算 |
| `Assets/3DGamekit/Art/LegacyShaders/Internal-ScreenSpaceShadows.shader` | Shader | 否 | GraphicsSettings | KEEP | 当前 GraphicsSettings 明确引用 | 屏幕空间阴影或 URP 配置报错 | 检查 GraphicsSettings 后再讨论 |
| `Assets/TextMesh Pro/Resources/` | TMP 字体、材质、设置、样式 | 否 | Resources 约定 | KEEP | TMP 默认资源按约定从 Resources 获取 | TMP 默认字体、样式或精灵丢失 | TMP 文本显示测试和打包测试 |

KEEP 集中的具体 GUID 链由第二阶段 Unity `AssetDatabase` 再验证，尤其是 Lightmap、Reflection Probe、NavMesh 和 Shader 子资源。

## 4. REMOVE：低风险删除候选

以下项目均不在目标战斗场景闭包内，也不在当前 Build Settings、URP 配置、TMP 核心 Resources 或已识别运行时字符串路径中。它们构成最终精简工程的第一批删除候选，但本阶段不执行删除。

| 资源路径 | 类型 | 文件数 | 大小 MB | 目标直接 | 间接使用 | 动作 | 判断依据 | 风险 | 删除前验证 |
|---|---|---:|---:|---|---|---|---|---|---|
| `Assets/TextMesh Pro/Examples & Extras/` | 示例场景、示例脚本、字体、材质、Prefab、纹理 | 136 | 5.53 | 否 | 未发现 | REMOVE | 全部 TextMesh Pro 演示内容；目标场景未引用；没有 Addressables、AssetBundle 或目标脚本字符串加载 | TMP 示例资源被误用会导致该示例场景失效，但不影响目标场景 | Editor 编译；目标场景打开；TMP HUD 构建 |
| `Assets/TutorialInfo/` | 教程窗口、图标、布局 | 6 | 0.24 | 否 | 未发现 | REMOVE | URP 模板教程内容，不参与运行时 | 仅失去模板帮助面板 | 确认项目 README 已由团队文档替代 |
| `Assets/Readme.asset` | URP 模板说明 ScriptableObject | 1 | 0.00 | 否 | 未发现 | REMOVE | URP 空模板说明 | 无运行时风险 | 打开目标场景并保存确认 |
| `Assets/3DGamekit/Readme.asset` | Game Kit 说明 ScriptableObject | 1 | 0.00 | 否 | 未发现 | REMOVE | 示例包说明 | 无运行时风险 | 保留外部来源记录后删除 |
| `Assets/Scenes/SampleScene.unity` | Scene | 1 | 0.01 | 否 | 未发现 | REMOVE | 空 URP 模板场景，不在 Build Settings | 无目标运行时风险 | 确认目标场景可独立打开并构建 |
| 合计 | — | 145 | 5.789 | — | — | — | — | — | — |

说明：

- 删除 `TextMesh Pro/Examples & Extras/` 时不得误删 `Assets/TextMesh Pro/Resources/`。
- `Assets/Settings/SampleSceneProfile.asset` 虽未直接进入目标场景，但被当前旧场景结构闭包使用，因此不列入本批 145 个删除候选；当前仍为 KEEP，待旧场景图移除后再重分类。
- 示例内容里的字体许可证文本应在删除前抄录到项目许可证台账；当前没有要求保留示例本身。

## 5. REVIEW：不能直接删除的静态外部资源

以下统计均排除上述 145 个低风险删除候选，合计 893 个文件、873.40 MB。

| 路径族 | 文件数 | 大小 MB | 主要类型 | 目标直接引用 | 间接使用/风险 | 动作 | 必须验证 |
|---|---:|---:|---|---|---|---|---|
| `Assets/3DGamekit/Art/` | 234 | 360.67 | TIF、FBX、材质、Shader、模型、纹理 | 否 | Shader 路径、材质、模型子资源和未来竞技场编辑 | REVIEW | 逐材质 Shader 映射、FBX Avatar/Clip、角色材质 |
| `Assets/3DGamekit/Scenes/` | 188 | 430.67 | LightingData、NavMeshData、Reflection Probe、场景依赖数据 | 否 | Lightmap、NavMesh、反射探针与 Editor 内部关联 | REVIEW | 打开全部保留场景；重新烘焙验证；对比重复光照数据 |
| `Assets/3DGamekit/Audio/` | 186 | 24.85 | OGG | 否 | 可能作为后续拳击/怪兽音效候选 | REVIEW | 音频试听、许可证、事件映射；确认不会动态加载 |
| `Assets/3DGamekit/Scripts/` | 90 | 0.17 | C#、Editor 工具 | 否 | C# 类型被其他保留脚本引用时不产生 GUID 边 | REVIEW | 全量编译、Roslyn/程序集依赖分析、Editor 菜单检查 |
| `Assets/3DGamekit/Packages/` | 101 | 4.61 | Timeline、SFX、Interactive、World Building 脚本与资源 | 否 | 程序集、动画、Timeline、反射和编辑器工具体验 | REVIEW | 编译、Timeline Playable、Editor 工具打开测试 |
| `Assets/3DGamekit/Prefabs/` | 60 | 8.71 | Prefab、VFX、UI、武器、关卡预组合 | 否 | 旧场景依赖，以及未来作为竞技场/音频素材 | REVIEW | 逐 Prefab 逆依赖；旧场景迁移后重新计算 |
| `Assets/TextMesh Pro/Shaders/` | 22 | 1.18 | TMP Shader、Shader Graph、cginc | 否 | 材质可通过 Shader 名称或运行时页面选择间接使用 | REVIEW | TMP 所有 Canvas、字体材质、移动端切换测试 |
| `Assets/TextMesh Pro/Sprites/` | 3 | 0.11 | EmojiOne 图集、JSON | 否 | 可能由 TMP Settings 或运行时富文本加载 | REVIEW | TMP sprite asset 配置与富文本测试 |
| `Assets/TextMesh Pro/Fonts/` | 1 | 0.00 | 字体许可证/源文件 | 否 | 字体 SDF 生成源，不是运行时字体资产 | REVIEW | 确认 TMP SDF 可独立重导入 |
| `Assets/3DGamekit/Documentation/` | 2 | 3.80 | PDF | 否 | 团队说明与许可证审计价值 | REVIEW | 将来源摘要写入许可证台账后再决定删除 |
| `Assets/_Recovery/0.unity` | Scene | 1 | 38.51 | 否 | Editor 恢复场景，可能是用户备份 | REVIEW | 由用户确认无恢复需要；保存目标场景后再处理 |
| `Assets/_Recovery/0 (1).unity` | Scene | 1 | 0.00 | 否 | 同一编辑器的额外恢复快照 | REVIEW | 确认两项恢复场景均可弃用 |
| `Assets/InputSystem_Actions.inputactions` | Input Actions | 1 | 0.04 | 否 | 未来键鼠与动捕输入适配可能采用 | REVIEW | 新 `CombatInputAdapter` 设计完成后决定 |
| `Assets/Settings/Mobile_RPAsset.asset`、`Mobile_Renderer.asset` | URP 配置 | 2 | 0.00 | 否 | Windows 第一阶段不使用，但可能保留多平台能力 | REVIEW | 明确只支持 Windows 并完成 Standalone 构建 |
| `Assets/3DGamekit/Gizmos/` | Gizmo 资源 | 1 | 0.06 | 否 | Editor 工具显示 | REVIEW | 关闭对应编辑器工具或替换后再删除 |
| 合计 | — | 893 | 873.40 | — | — | REVIEW | — |

## 6. 高优先级 REVIEW 项

| 资源路径 | 大小 MB | 原因 | 建议顺序 |
|---|---:|---|---|
| `Assets/3DGamekit/Scenes/Data/LightingData/Level1/LightingData.asset` | 227.48 | 与目标场景实际引用的 `GamePlay/Level1/LightingData.asset` 不是同一文件，可能是备份/另一套烘焙 | 先比较场景引用与烘焙集，确认后最后处理 |
| `Assets/3DGamekit/Scenes/Data/LightingData/Level2/LightingData.asset` | 110.88 | Level2 仍由 Build Settings 和 `TransitionPoint` 间接可达 | 先移除目标场景切关和 Build 条目 |
| `Assets/_Recovery/0.unity` | 38.51 | 编辑器恢复数据，静态不可见全局结构依赖 | 用户确认无需恢复后再进入删除候选 |
| `Assets/3DGamekit/Art/Textures/Effects/Cutscene_LensDirt.tif` | 35.06 | 纹理可能被材质、Shader 路径或编辑器工作流使用 | 逐材质 Shader 使用验证 |
| `Assets/3DGamekit/Art/Textures/Dropship/` | 约 110 | 大体积飞船舱材质源；目标场景闭包中未命中 | 确认 Dropship 不再作为环境装饰使用 |
| `Assets/3DGamekit/Packages/EyeShader/Textures/Eye_sss.psd` | 3.89 | Shader/PSD 生成链不能只按静态 GUID 判断 | 检查 Eye Shader 和材质后再定 |

## 7. 隔离执行规则

第二阶段不得直接按本文件的路径调用文件系统批量删除。正确流程是：

1. 在 Unity Editor 内保存目标场景并完成导入/Console 检查。
2. 先只启用 145 个低风险候选，验证目标场景、TMP UI 与 Windows 构建。
3. 对 REVIEW 项生成 Unity `AssetDatabase.GetDependencies` 逆依赖证据。
4. 按“场景/旧玩法族”成组验证，而不是按扩展名批量删除。
5. 删除通过 Unity Editor；GUID 和 `.meta` 同步处理。
6. 每次成组删除后重新打开目标场景、播放 Animator、运行 Play Mode 并构建。

## 8. 明确禁止

- 不删除目标场景依赖闭包内的 Ellen、Chomper、CircularArea、材质、动画、音频或 Prefab。
- 不删除 `Assets/TextMesh Pro/Resources/`。
- 不删除被 `GraphicsSettings.asset`、`QualitySettings.asset` 或 Build Settings 引用的资源。
- 不因一个文件位于 `3DGamekit` 就批量删除整个目录。
- 不手工删除 `.meta`。
- 不覆盖第三方原始 Prefab 和源资源。

## 9. 本次执行记录

| 项目 | 结果 |
|---|---|
| 实际删除的非 `.meta` 资源 | 0 |
| 实际移动的资源 | 0 |
| 实际修改的 `.meta` | 0 |
| 145 个低风险 REMOVE 候选 | 全部保留，未执行 |
| 1,751 个 KEEP 资源 | 全部保留 |
| 893 个 REVIEW 资源 | 全部保留，未擅自转成 REMOVE |
| 阻塞原因 | 当前工程没有可验证的 Git 回退基线 |

详细证据与验证状态见 `CLEANUP_EXECUTION_REPORT.md`。
