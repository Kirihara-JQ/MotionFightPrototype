# C 成员开发工作流 V2.0：Unity 6000.3.27f1 + URP + Level 1 CircularArea 竞技场 + 全资源整合 Codex 指南

> 项目：人体动捕 1v1 三维格斗系统  
> 文档用途：C 成员独立开发键盘鼠标可玩原型，供后续接入 A/B 的人体动捕与角色驱动模块  
> 需求基线：SRS V1.5（2026-10-28）、数据接口 v0.3、`10_1v1格斗场景与规则`  
> 负责人：史佳琪（C）  
> 编写日期：2026-10-29  
> 当前状态：开发计划，不代表原型已经实现或测试通过
> 引擎基线：用户指定的 Unity `6000.3.27f1`

---

## 0. 给 Codex 与开发者的执行摘要

> **V2.0 场景基线变更（2026-10-29）**：本版废止“抽取少量环境资产并从零搭建 Arena”的旧方案。正式战斗场景以原始 `Level1` 场景完整副本为底，保留场景、灯光、建筑、角色和其他资源对象；`CircularArea` 及其周边地形作为主要 1v1 战斗区域。原始 Level 1 场景不覆盖、不重命名、不直接修改。竞技场模式通过新增的 `__CombatOverlay` 和显式组件引用接入。保留对象不意味着允许 Game Kit 原始控制器与新战斗逻辑同时驱动同一 Actor；冲突组件只在副本中按清单禁用，禁止删除第三方资产或批量清理整关对象。

> **需求范围优先级说明**：较早的项目总方案将闪避、体力等魂类机制列作总体方向；当前实现以 2026-10-28 的《1v1 三维格斗场景与规则》及 SRS V1.5 为首版验收基线：真实移动/转向、左右直拳、一种格挡、单只电脑怪兽、双方 HP、权威伤害、胜负/平局、暂停恢复与重开。踢腿、闪避/翻滚、体力条、连招和多人对战不进入当前 MVP，除非团队明确变更需求。

### 0.1 本次要交付什么

在 A/B 的真实人体动捕与角色驱动尚未完成的情况下，先把**Level 1 的 CircularArea 场景改造成可玩的键鼠 1v1 战斗原型**，并保留整张 Level 1 的现有环境与人物。实现：

1. 从原始 `Level1` 另存 `Level1_CircularArena_Combat.unity`，不修改原始关卡。
2. 复用 Level 1 全部场景内容，重点以 `CircularArea` 与周边环境做 1v1 战斗区域；场景外观、灯光、人物及敌人模型保持在副本中。
3. 优先使用 Level 1 现有的 Ellen 作为玩家视觉模型和一只既有怪兽（优先检查 Chomper）作为对手；仅在明确的资产缺口下引入备用动画/音频。
4. 玩家 WASD 移动、鼠标控制/微调相机、Q 锁定目标、鼠标左键左直拳、鼠标右键右直拳、Space 格挡。
5. 怪兽有限状态机具备接近、可读预警、攻击生效、收招、受击与死亡；双方 HP、胜负、平局、暂停/恢复、重新开始可运行。
6. 伤害权威仅有 `CombatResolver`/`MatchController`；Game Kit 的旧 Damageable/AI/PlayerController 不允许并行控制同一 Actor。
7. 为 A/B 的 `AvatarState`、有效拳部/躯干锚点、时间戳和追踪状态保留适配接口；此阶段不接 ZED SDK、不伪称动捕验收完成。
8. 将资源来源/许可证、实际动画 Clip、CircularArea 场景审计、禁用组件原因及测试记录保存到 `Docs/`。

### 0.2 首版范围锁定

| 首版必须完成 | 保留但不一定参与本局 | 明确不纳入首版 |
|---|---|---|
| Level 1 完整场景副本、CircularArea 竞技场、现有 Ellen/指定怪兽、左右直拳、格挡、HP/胜负、怪兽 AI、暂停/重开、事件日志 | Level 1 的其他人物、敌人、关卡道具、灯光、机关、旧 UI、原始 AI/伤害组件；保留对象，按竞技场模式清单处理冲突行为 | 从零搭完整替代地图、删除/拆空 Level 1、踢腿、复杂连招、武器系统、翻滚/闪避、多人联机、行为树、多敌人同时参战 |

**关键区分：**“保留 Level 1 全部内容”指战斗副本完整保留原有场景对象与资源；“1v1”则通过 CombatParticipants 清单限定当前局只有一名玩家和一只参战怪兽。非参战角色可保留可见模型与 Animator，但其旧 AI/攻击/旧伤害脚本在本局内按配置停用，不能进入本局结算。

**不要把键鼠原型测试结果写成真实人体动捕已验收。**

## 1. Unity 版本与工程技术选型

### 1.1 锁定版本：Unity 6000.3.27f1

**主工程固定使用 `Unity 6000.3.27f1`，不允许 Codex 自动升降版本。** 这是用户明确指定的 Editor 版本；该版本的官方发布说明：<https://unity.com/cn/releases/editor/whats-new/6000.3.27f1>。发布说明列出本版 Input System 包更新至 `1.20.1`，并列出一些已知问题（详见 1.4 节）。

开始前核对 `ProjectSettings/ProjectVersion.txt` 中的 `m_EditorVersion`，仓库中保留 `Packages/manifest.json` 与 `Packages/packages-lock.json`。Unity Hub、Codex 使用的项目路径、打包电脑和团队成员尽可能统一为 `6000.3.27f1`。不要把版本号改成 `6000.3` 或擅自升级到其他 Unity 6 补丁版本。

### 1.2 打开现有工程与建立安全副本

当前已存在的工程与 `AssetImportSandbox_InternationalURP` 是本轮的工作基础。不要因为工作流而重建空白项目。

1. 在 Unity Hub 确认打开的 Editor 是 `6000.3.27f1`，项目使用 URP。
2. 确认当前场景/包材质已经正常显示；本次不再重复全包材质转换。先提交 Git 或复制整个项目作为回退点。
3. 保留 `ProjectSettings/ProjectVersion.txt`、`Packages/manifest.json` 和 `Packages/packages-lock.json`，不升级、不降级、不擅自更改渲染管线。
4. 在 Project 面板搜索实际导入的 Level 1 场景文件，记录真实路径。使用 `File > Save As` 另存为 `Level1_CircularArena_Combat.unity`。
5. 重新打开原始 Level 1，确认它仍未改变；然后重新打开战斗副本进行后续工作。
6. 如果当前打开的 `AssetImportSandbox_InternationalURP` 只是导入验证工程，而不是正式主工程，先复制/分支它作为主工程，再从本机已验证的资源路径继续，不要在沙盒直接覆盖原始测试状态。
7. Windows 桌面为首个运行/构建目标，C# 与当前已有输入包为准；不要为了阶段 1 新装无关依赖。

### 1.3 技术栈与兼容原则

| 子系统 | 选型 | 本次实施原则 |
|---|---|---|
| 渲染 | URP | 主工程从 Universal 3D 模板创建；优先使用 3D Game Kit 在 Unity 6000.3 系列列明兼容的 URP 版本，隔离测试中国区包和所有 Built-in 材质 |
| 输入 | Unity Input System | 在 Package Manager 中确认当前 Editor 可用版本；Unity `6000.3.27f1` 发布说明列出 `com.unity.inputsystem` 更新到 1.20.1。不要为追求更新而随意升级。文档：<https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/index.html> |
| 玩家移动 | `CharacterController` | 仅供键鼠原型使用；未来由 B 的动捕驱动替代移动源 |
| 相机 | 自建平滑第三人称相机 | 先避免不必要的 Cinemachine 依赖；以后可替换 |
| 怪兽 AI | C# 有限状态机 | 一只怪兽、一种近战攻击，不引入行为树框架 |
| 动画 | Animator Controller + Humanoid 重定向 | 动画资源与战斗规则分离；缺动画时可用可见占位动作 |
| 战斗参数 | ScriptableObject | HP、伤害、攻击窗口、距离与冷却统一配置 |
| 命中检测 | Hitbox/Hurtbox + `SphereCast` 或轨迹采样 | 物理检测只产生候选接触，最终伤害由唯一结算器处理 |
| UI | Canvas + TextMeshPro | HUD、暂停、结果与调试面板分离 |
| 音频 | AudioSource（AudioMixer 可选） | 事件触发音效，BGM 循环；静音时视觉反馈仍完整 |
| 记录 | JSONL / CSV | 默认只保存对局/互动事件，不录制原始相机帧或逐帧动捕数据 |

**输入包版本说明：**在 Window → Package Manager → Unity Registry 中确认 Input System 版本。Unity `6000.3.27f1` 发布说明列出 `com.unity.inputsystem` 从 1.20.0 更新至 1.20.1；若项目当前版本不同，以 Package Manager 和 `packages-lock.json` 的实际结果为准。安装后按提示启用 Input System 并重启 Editor。代码中统一使用 Input Actions 或统一使用设备状态读取，不能一半新输入、一半旧 Input Manager。


### 1.4 Codex 必须遵守的版本纪律与已知风险

- 不得把 `6000.3.27f1` 自动改成其他 `6000.x`、2022.3 或 Beta 版本。
- 不得因资源包兼容问题自行更换 URP/Built-in/HDRP。先在隔离工程定位问题并汇报。
- 更改 `manifest.json` 或 `packages-lock.json` 前，先解释原因，并仅更改当前功能必需的依赖。
- 资产导入不等于兼容性通过。每个角色、动画、Shader 和 Prefab 都要在当前 Editor 中打开、播放并构建验证。
- **Unity `6000.3.27f1` 官方已知问题**：发布说明列出使用 `SceneManager.LoadSceneAsync` 反复切换场景时 Player 可能停止响应。首版重开对局优先通过重置 Match/角色状态实现，不要频繁异步重载场景；若确需重载，必须专项压力测试并记录复现结果。官方说明：<https://unity.com/cn/releases/editor/whats-new/6000.3.27f1>。
- 官方还列出 Windows Editor 在高鼠标轮询率下移动鼠标可能导致帧率下降。若第三人称镜头出现卡顿，先区分 Editor 已知问题与项目代码问题，再尝试降低鼠标轮询率/在 Build 中复测，不要立即改坏相机逻辑。
## 2. 直接可用的角色、环境、怪兽和音频资源

### 2.1 资源优先级与边界

本版以已导入且已修复材质的 Level 1 完整场景为 Primary。资产补充必须针对已记录的真实缺口，不允许因“整合所有资源”而把全部资源包无差别叠加。第三方原始场景、Prefab 和 Hierarchy 保持不变；所有战斗接入都发生在 Level 1 的 Save As 副本。

### 2.2 主场景、角色与补充资源清单

本版“整合资源”的中心不是把所有第三方包堆进工程，而是优先复用已经导入、材质修复成功的 **3D Game Kit Level 1 原场景、既有环境和人物**，并将其接到项目自己的战斗系统。

- **主场景/环境：**Level 1 原始场景的完整副本。保留 CircularArea、周边地面/墙体/柱体/装饰、灯光、Volume、相机、道具、触发器、人物和敌人 GameObject。
- **玩家：**优先使用 Level 1 已有 Ellen 的场景实例、网格、材质与 Rig；战斗副本中禁用冲突的原玩家控制行为，但不删除对象或修改 Prefab 源。
- **怪兽：**优先使用 Level 1 里现有且位于 CircularArea 内/附近的一只 Chomper。如果对象实际不存在或动画不能使用，再由开发者确认选择其他已有怪兽；不要默认导入第二套新怪兽。
- **玩家拳击动画：**先盘点 Ellen 现有 Clip。原动作若只有法杖近战，不得假称为左右直拳。确实缺少拳击动作时，才导入 Quaternius Universal Animation Library 2 或 Free Those Who Fighter 中经过兼容测试的动画，作为补充 Clip，而不是替换 Level 1 人物。
- **音频：**从既定 CC0 打击、怪兽、环境与 UI 音频资源中只挑实际需要的 Cue。先确认文件、许可证和试听结果。
- **Quaternius Universal Base Characters / Ultimate Monsters / Modular Dungeons：**保留在资产候选与许可证台账里，但本版默认不作为主场景、玩家或怪兽替代品；只有 Level 1 原资源出现明确缺口时才启用。

任何第三方资产/组件的实际名字、路径和动画 Clip 名称均必须从当前工程中枚举。Codex 不允许猜测。

### 2.3 音频资源清单

| 用途 | 资源 | 许可证/注意事项 | 下载入口 |
|---|---|---|---|
| 拳击命中、身体打击 | 37 hits/punches | CC0；压缩包为 `.7z`，需要 7-Zip 等解压工具 | <https://lpc.opengameart.org/content/37-hitspunches> |
| 怪兽咆哮、受伤、死亡、喘息 | 80 CC0 creature SFX | CC0；包含多类 creature grunt / roar / hurt / scream | <https://opengameart.org/content/80-cc0-creature-sfx> |
| 地牢环境氛围 | Dark Cavern Ambient | CC0；提供淡入淡出与连续循环版本，适合低音量背景层 | <https://opengameart.org/content/dark-cavern-ambient> |
| 战斗 BGM（可选） | Battle Theme（Wolfgang_） | CC0；紧张管弦乐，可作为对局音乐 | <https://opengameart.org/content/battle-theme-0> |
| UI 按钮、开关与菜单 | Kenney UI Audio | CC0 | <https://kenney.nl/assets/ui-audio> |
| 碰撞、金属、环境打击补充 | Kenney Impact Sounds | CC0；先试听后再挑选适合拳击的声音 | <https://kenney.nl/assets/impact-sounds> |

**建议实际导入的最小音频集合：**

- `SFX_Punch_Hit_01/02`：从 37 hits/punches 中挑 2–3 个不刺耳的打击声。
- `SFX_Punch_Whiff`：可用轻量 whoosh，来自声音包中合适的挥动声音；若找不到，先无声运行，不用命中声冒充挥空声。
- `SFX_Monster_Telegraph`：怪兽攻击前提示，可用短咆哮或低吼。
- `SFX_Monster_Hit`、`SFX_Monster_Death`：从 80 CC0 creature SFX 中挑选。
- `SFX_Guard`：用较闷的打击声或金属/盾牌类声音，不要与普通命中完全相同。
- `SFX_UI_Click`、`SFX_Victory`、`SFX_Defeat`：UI 音效可用 Kenney UI Audio 或自制简短音调。
- `BGM_Dungeon_Ambient`：先只用 Dark Cavern Ambient 连续循环版本；战斗音乐可作为第二阶段选项。

所有声音下载后放到 `Assets/ThirdParty/Audio/`，由 C 创建的 `AudioCueLibrary` ScriptableObject 映射到逻辑 cue。请建立 `Assets/ThirdParty/ASSET_LICENSES.md`，至少记录资源名、作者/发布者、来源 URL、许可证、下载日期、实际使用文件、是否修改以及需要的署名。虽然 CC0 通常无需署名，仍建议保留出处用于课程项目审计。

### 2.4 3D Game Kit Level 1 的保留与战斗接入策略

**此版直接以 Level 1 场景为舞台，不把它仅作为零散资产库。**

#### 操作规则

1. 将原始 Level 1 另存为 `Level1_CircularArena_Combat.unity`。原始 Level 1 作为只读基准，任何场景改动只发生在新副本。
2. 搜索 `CircularArea`，记录完整路径、Transform、地面和周边 Collider/Renderer 范围。若多个匹配，逐个核对实际场景位置，不可凭对象名猜。
3. 不改动、删除或重新组织原关卡的 Hierarchy；只增加新根节点 `__CombatOverlay`，其下包含 `ArenaModeController`、`ArenaReferences`、`CombatParticipants`、`CombatSystems`、`UI_Overlay` 与 `Debug`。
4. 优先引用已有 Ellen 与指定 Chomper。若 Chomper 不在 CircularArea 附近，才在副本内调整其根位置到 `MonsterSpawn`；保留其他敌人和关卡角色对象。
5. 对旧组件先做清单：Ellen 的旧 `PlayerController`/输入脚本、敌人的 `EnemyController`/具体 Behaviour、`Damageable`、`ContactDamager`、原关卡自动伤害/传送/重置/结束流程组件等。名称和类型以实际 Inspector 为准。
6. 只通过 `ArenaModeController` 的 Inspector 引用列表禁用冲突行为组件。不允许基于猜测做字符串扫描后直接删除对象；不允许让旧组件与新脚本同时写 Actor Transform、Animator、HP、MatchState 或攻击窗口。
7. 场景原人物全部留存；本局由 `CombatParticipants` 明确指定 Player 和一只 Monster。其他敌人可留在画面里，但其原 AI/伤害行为在竞技场模式下停用，保证不会干扰 1v1。可视外观默认保留。
8. 地面、墙体、石柱、灯光和周边装饰均沿用 Level 1 原资源。只有经现场检查确认边界缺失时，才追加最少量自有 Collider；禁止创建覆盖旧地面的新 Ground。
9. 优先复用现有 Camera/CameraRig；禁用冲突的原跟随/输入组件，但不删除相机对象。确保只有一台主 Camera 和一个 AudioListener。
10. 原版 Damageable、敌人 AI 与 SceneLinked state machine 可作为理解原动画/攻击点的参考，但不作为新项目的并行权威战斗系统。HP、格挡、攻击窗口和结果只能由 `CombatResolver`/`MatchController` 管理。

#### 建议的新增层级

```text
[Level 1 原始层级完整保留]
└── __CombatOverlay
    ├── ArenaModeController
    ├── ArenaReferences
    │   ├── PlayerSpawn
    │   ├── MonsterSpawn
    │   └── CircularAreaReference
    ├── CombatParticipants
    │   ├── PlayerReference -> 当前 Level 1 人形角色
    │   └── MonsterReference -> 指定的现有怪兽
    ├── CombatSystems
    │   ├── MatchController
    │   ├── CombatResolver
    │   ├── CombatInputAdapter
    │   ├── MonsterAIController
    │   ├── SessionEventLogger
    │   └── RecoveryBridge
    ├── UI_Overlay
    └── Debug
```

这是一组新加的辅助节点，不代表要把原场景对象重新拖入这些分组。参与者引用应指向原场景里已有的角色实例。

#### 视觉与动画原则

- 保留 Level 1 的光照和环境风格，优先微调主光源、雾、后处理与 UI 色彩，不先混入另一套风格的完整地牢。
- Ellen 的法杖/手持物对象不删除。如果徒手直拳动画和道具冲突，可在战斗副本通过显式配置隐藏指定 Renderer；原始 Level 1 和源 Prefab 不变。
- 角色 Animator、Rig 和现有 Clip 都以 Inspector 实测为准。可新建项目自有 Animator Controller/Adapter，但不得覆盖第三方 Controller 资源。

### 2.5 资产审计、兼容验收与许可证台账

请在 `Docs/` 中维护下列文件：

- `CircularArea_SceneAudit.md`：Level 1 原始场景路径、CircularArea 完整 Hierarchy 路径、场地边界、玩家/怪兽路径、相机、冲突组件清单和禁用原因。
- `ASSET_DECISIONS.md`：每项资源的 Primary / Supplement / Optional / Fallback / Not Used 状态与理由。主场景 `Level1_CircularArena_Combat` 和原关卡现有角色是 Primary。
- `ASSET_LICENSES.md`：来源 URL、发布者、许可证、实际使用的文件、是否修改、署名要求。
- `ASSET_GAPS.md`：缺失动画、材质、碰撞体或不能运行的旧组件；不能用臆测填充。
- `AnimationClipMap.md`：模型/Animator/Clip 的实际名称、Clip 长度、动作意义、测试结果、Humanoid Avatar 重定向结果。
- `ArenaComponentConflictList.md`：每个被禁用组件的场景路径、类型、冲突原因、禁用范围、验证恢复方案。

阶段 0 通过标准：Unity `6000.3.27f1` URP 工程可打开；Level 1 原始和副本都可加载；CircularArea 的位置与范围已记录；材质未重新变粉；玩家/怪兽模型可见；没有编译红错；没有第三方原始对象被删除；资源来源和许可证台账已建立。

## 2.6 ZED / RGB-D 后续整合边界

主工程首版**不依赖 ZED SDK**，确保键鼠原型可独立打开、运行与打包。当前官方 ZED Unity 插件文档写明支持 Unity 2021.3 及更新版本，并有 URP/HDRP 材质转换说明；但这不等于已针对 Unity `6000.3.27f1`、当前 ZED SDK 和实验室摄像机型号完成验证。官方入口：<https://docs.stereolabs.com/docs/integrations/unity/>；ZED SDK 5.5.1 发布页：<https://www.stereolabs.com/developers/release>。

接入阶段依次在独立 `ZED_CompatibilitySandbox` 中安装匹配摄像机型号的 SDK 与 Unity 插件，运行官方彩色图/深度/人体追踪示例；记录 Editor、SDK、插件、驱动、GPU 与相机型号。验证后才建立 `IZedBodyTrackingProvider` 等适配层，把 B 输出的 `AvatarState`/动作事件交给 C 的输入接口。禁止战斗系统直接依赖 ZED 原始骨骼帧。若插件在 `6000.3.27f1` 下无法编译或运行，保留键鼠 Provider，不得擅自降级主工程。

## 3. 原型体验和键鼠交互方案

### 3.1 推荐按键

| 输入 | 行为 | 规则 |
|---|---|---|
| `W / A / S / D` | 玩家移动 | 相对相机平面方向移动，Y 轴不受输入影响 |
| 鼠标移动 | 转动第三人称相机 | 限制俯仰角；不直接改变碰撞检测规则 |
| `Q` | 锁定/解除锁定怪兽 | 锁定时相机优先同时显示玩家与怪兽；玩家可朝向怪兽 |
| 鼠标左键 | 左直拳 | 按下时尝试开始一笔左拳，若处于冷却/收招则拒绝并输出调试原因 |
| 鼠标右键 | 右直拳 | 同上；使用独立攻击 ID 和轨迹 |
| `Space`（按住） | 格挡 | 在有效姿势/时效内，面对攻击来源的正面格挡将该次伤害减为零 |
| `Esc` | 暂停/恢复菜单 | 手动暂停时冻结 AI、移动、攻击窗口、冷却和对局时钟 |
| `R` | 结果界面重新开始 | 仅在结果界面有效；新建 match_id，清除旧攻击与去重缓存 |
| `F1` | 显示/隐藏调试面板 | 显示玩家/怪兽状态、当前攻击阶段、血量和最近事件 |
| `F2` | 显示/隐藏 Gizmos | Scene/Game 视图辅助检查 hitbox/hurtbox，不改变真实规则 |

若鼠标右键被操作系统/编辑器窗口占用，要先在 Game 窗口中点击取得焦点。锁定目标后，玩家转向使用柔和朝向目标；无锁定时根据相机平面移动方向旋转。相机移动必须与人物移动拆开，不能因为转动镜头就改写玩家在世界中的真实位置。

### 3.2 镜头

- 第三人称斜后方视角：略高于玩家肩部，能看到玩家双拳与怪兽上半身。
- 镜头目标点位于玩家和怪兽的加权中点附近；锁定时可稍微拉远，让双方同框。
- 鼠标水平控制 yaw，垂直控制 pitch，pitch 夹角建议先限制在 `-20° ~ 55°`。
- 相机碰撞可先简化：用 `SphereCast` 从目标点到期望镜头位置检测墙壁，缩短相机距离避免穿墙。
- 镜头不应影响命中判定、格挡方向或双方世界坐标。

### 3.3 玩家移动

- 当前原型使用 `CharacterController`；移动输入先归一化，再投影到水平面。
- 可用 `CharacterController.Move` 处理地面与墙体碰撞，重力只由移动组件处理。
- 进入攻击动作时不要将角色强制向敌人平移；可以轻微前移动画根节点，但不能破坏玩家的世界根部位置。
- 玩家与怪兽的初始距离应能在开场时清楚区分：太远允许玩家接近，太近避免角色互相穿入。
- **未来动捕接入时的原则：**当前 `KeyboardMotionProvider` 仅在 Prototype 模式启用；集成 B 的角色驱动后禁用它，不能同时由 B 和 C 移动 `PlayerRoot`。

---

## 4. 战斗规则和初始参数

所有数值均为**开发初值**，必须通过实际原型调试；不应写成已通过测试的性能/体验指标。参数存入 ScriptableObject，运行时不能由 UI、音效或怪兽 AI 私自修改血量。

### 4.1 建议初始数值

| 参数 | 初始值 | 说明 |
|---|---:|---|
| 玩家最大 HP | 100 | `CombatConfig` |
| 怪兽最大 HP | 100 | `CombatConfig` |
| 玩家直拳伤害 | 10 | 左右拳可以先使用同一伤害 |
| 怪兽近战攻击伤害 | 12 | 只用于游戏数值，不推断真实打击力度 |
| 玩家拳前摇 | 0.12–0.20 s | 根据攻击动画长度调整 |
| 玩家拳生效窗口 | 0.08–0.15 s | 仅此阶段启用 hit detection |
| 玩家拳收招 | 0.25–0.45 s | 收招完成并满足复位条件后才能再出拳 |
| 怪兽预警 | 0.70–1.00 s | 预警动画/颜色/音效先出现，然后攻击生效 |
| 怪兽攻击生效 | 0.20–0.35 s | 对玩家攻击的唯一有效时间段 |
| 怪兽收招 | 0.70–1.20 s | 玩家可以在其收招期间反击 |
| 格挡正面范围 | 以双方 forward 点积判定 | 首版正面有效，背面/侧后攻击不格挡 |
| 同次攻击命中同一目标 | 最多结算 1 次 | 唯一键 `(match_id, attack_id, target_actor_id)` |
| 初始对局 | WaitingForReady → Ready → Fighting | 必须明确开始，不能场景加载后立刻扣血 |

### 4.2 玩家攻击状态机

每次直拳都独立创建 `attack_id`，生命周期如下：

```text
Idle
  └── 按下拳击输入且输入有效/满足冷却
        ↓
     Startup（前摇；不能扣血）
        ↓
     Active（仅在此期间检测三维命中）
        ↓
     Recovery（不能新开攻击）
        ↓
     Recovered（复位通过后回到 Idle）
```

- 输入缓慢摆手（键鼠中为反复点击）时，如果上一次攻击仍在 Recovery，拒绝新攻击并显示/记录 `AttackRejected_Recovery`。
- 攻击中的命中窗口不应由 `OnTriggerEnter` 自己决定；状态机/AttackProfile 决定允许结算的时间。
- 命中候选来自最终场景中的拳部路径和对手 hurtbox。快速运动可用 `SphereCast` / 线段采样 / 运动碰撞体方案避免穿透。
- 同一 attack_id 在多个 FixedUpdate、多个 hurtbox 或重复事件中触碰同一个目标，只能扣血一次。
- 挥空要能识别：攻击 Active 阶段结束且未命中，则发布 `Whiff` 事件，可触发轻量挥空音。

### 4.3 格挡

- `Space` 按住才进入格挡；松开后退出。
- 格挡判断必须检查：对局在 Fighting、玩家生命值大于零、输入有效、格挡覆盖方向满足规则。
- 建议以 `Vector3.Dot(defender.forward, directionToAttacker)` 决定正面覆盖；具体阈值作为 `CombatConfig` 参数，不要散落在多个类。
- 首版正面有效格挡：本次伤害为 0，并产生 `Blocked` 事件；非正面格挡不生效，按命中规则正常扣血。
- 格挡成功的音效和 UI 由事件触发，不能在 `GuardController` 中直接改 HP。

### 4.4 怪兽攻击状态机

```text
Idle
 ├── 距离超出追击范围 → Approach
 └── 距离进入攻击范围 → Telegraph
       ↓ 等待预警结束
     AttackActive
       ↓ 窗口关闭
     Recovery
       ↓
     Idle / Approach

任意可受击状态 ── 有效命中 → HitReaction ── 恢复后回原状态
HP <= 0 → Dead（不再攻击、不再移动、不再受击）
任何状态 + MatchPaused/TrackingInvalid → Paused（全局冻结）
```

- AI 只能看到当前玩家位置与战斗状态，不读取未来真实动作，不通过随机瞬间扣血实现难度。
- 怪兽接近采用简单的方向移动与距离判断即可；平坦封闭竞技场不要求 NavMesh。
- `Telegraph` 阶段必须显示明确预警，可以是怪兽抬臂/后仰动画、短咆哮、头顶图标或材质发亮（至少两种组合更清楚）。
- `AttackActive` 期间启动怪兽攻击体；在 Recovery、HitReaction、Dead、Paused 时不能产生新攻击。
- 如果攻击没有命中玩家，记录 `MonsterWhiff`；不要通过距离范围判断直接给玩家扣血。

### 4.5 唯一伤害结算

采用唯一的 `CombatResolver` 和 `MatchController`：

1. `AttackDetector` / `MonsterAttackDriver` 生成带 ID 的攻击事件。
2. Hitbox 或扫掠检测产生 `HitCandidate`，其中包含攻击来源、目标、攻击 ID、时间和命中部位。
3. `CombatResolver` 验证对局状态、攻击有效窗口、双方可受击状态、命中质量、格挡与去重。
4. 所有有效候选先进入本结算步的候选集合；然后向 `MatchController.ApplyDamageBatch()` 提交。
5. `MatchController` 统一更新双方 HP，范围限制在 `[0, maxHP]`。
6. 同一个物理结算步收集双方有效伤害后再决定胜负：玩家 HP 与怪兽 HP 同时归零，判平局；否则生命值仍大于零的一方获胜。
7. 发布 `InteractionEvent` 与 `MatchState`，UI/音频/特效订阅，不反向修改权威状态。
8. 对局结束后禁止新攻击和伤害，只允许结果界面与重新开始流程。

### 4.6 暂停和重新开始

暂停或关键输入失效时：

- 冻结玩家移动、怪兽 AI、攻击窗口、伤害、收招/冷却计时和对局时钟。
- 清除所有未完成攻击，避免恢复后旧拳再次命中。
- 保留当前双方 HP。
- UI 提示暂停原因，采集/追踪恢复检测在未来集成时仍可继续运行。
- 恢复前确认输入、身份、标定与场景映射有效，再经用户明确确认恢复。

重新开始时：

- 生成新 `match_id`。
- 恢复双方初始位置、朝向、HP、AI 初始状态与攻击状态。
- 清空 attack_id 活跃集合和去重表，但保留上一局历史事件文件。
- 不能仅把 HP 改回 100 而不重置 AI、碰撞体和攻击窗口。

---

## 5. 工程架构与目录结构

### 5.1 推荐目录

```text
MotionFightPrototype/
├── Assets/
│   ├── ThirdParty/
│   │   ├── Characters/
│   │   ├── Animations/
│   │   ├── Monsters/
│   │   ├── Environment/
│   │   └── Audio/
│   └── _Project/
│       ├── Scenes/
│       │   ├── Boot.unity
│       │   ├── Setup.unity
│       │   └── Level1_CircularArena_Combat.unity
│       ├── Scripts/
│       │   ├── Core/
│       │   ├── Input/
│       │   ├── Arena/
│       │   ├── AI/
│       │   ├── Combat/
│       │   ├── UI/
│       │   ├── Audio/
│       │   ├── Recording/
│       │   ├── Integration/
│       │   └── Debug/
│       ├── Data/
│       │   ├── CombatConfig/
│       │   ├── MonsterAIConfig/
│       │   ├── ArenaConfig/
│       │   └── AudioCueLibrary/
│       ├── Prefabs/
│       │   ├── Player/
│       │   ├── Monster/
│       │   ├── Hitboxes/
│       │   └── UI/
│       ├── Materials/
│       ├── VFX/
│       ├── Tests/
│       └── Editor/
│   └── TextMesh Pro/
├── ProjectSettings/
├── Packages/
├── Docs/
│   ├── README.md
│   ├── ASSET_LICENSES.md
│   ├── ASSET_GAPS.md
│   ├── AnimationClipMap.md
│   └── CodexTasks.md
└── README.md
```

### 5.2 场景层级：原 Level 1 保持原样，只追加战斗覆层

```text
Level1_CircularArena_Combat
├── [原 Level 1 的全部对象/层级——完整保留，不移动重排]
│   ├── CircularArea（完整路径由 SceneAudit 记录）
│   ├── 原环境、地面、周边建筑、灯光、Volume、主相机/CameraRig
│   ├── 原人物/敌人（Ellen、Chomper 和其他实际存在的角色）
│   └── 原机关、触发器、旧 UI、场景管理器等
└── __CombatOverlay
    ├── ArenaModeController
    ├── ArenaReferences
    │   ├── CircularAreaReference
    │   ├── PlayerSpawn
    │   ├── MonsterSpawn
    │   └── ArenaBoundsDebug
    ├── CombatParticipants
    │   ├── PlayerReference -> 现有 Ellen 实例
    │   └── MonsterReference -> 选定的现有 Chomper/现有关卡怪兽
    ├── CombatSystems
    │   ├── MatchController
    │   ├── CombatResolver
    │   ├── CombatConfig
    │   ├── CombatInputAdapter
    │   ├── MonsterAIController
    │   ├── SessionEventLogger
    │   └── RecoveryBridge
    ├── UI_Overlay
    └── Debug
        ├── CombatGizmos
        └── StateDebugOverlay
```

**不要把原 Level 1 全部对象拖进新的自建层级。** 除非场景审计证明某个新增节点必须位于特定父节点下，否则保留第三方对象的原始父子关系。`PlayerReference` 与 `MonsterReference` 是引用，不是复制出新的角色。

如果现有场景已有主 Camera、Canvas 或 EventSystem，先检查再复用；只在缺失时才创建，避免第二台 Main Camera、多个 AudioListener 或重复 UI。

### 5.3 核心类与边界

| 类 / 接口 | 主要职责 | 不应该做的事情 |
|---|---|---|
| `PrototypeInputBootstrap` | 在当前原型启用键鼠输入源 | 不参与伤害结算 |
| `IPlayerMotionProvider` | 提供移动/根部驱动数据的抽象 | 不知道 HP 或怪兽 AI |
| `KeyboardMotionProvider` | 当前原型的 WASD 移动 | A/B 接入后不能和 B 同时移动 PlayerRoot |
| `ICombatInputProvider` | 提供左右拳、格挡、输入有效性 | 不碰撞、不修改 HP |
| `KeyboardCombatInputProvider` | 鼠标左右键和 Space 生成格斗输入 | 不决定命中是否成功 |
| `AvatarStateCombatProvider`（未来适配） | 把 B 的最终 AvatarState / 动作事件转成 C 的战斗输入 | 当前先建接口/文档，不要求依赖尚未完成的类 |
| `FightCameraController` | 镜头旋转、跟随、锁定构图 | 不改写角色真实世界位置 |
| `AttackDetector` | 玩家拳输入、攻击阶段、复位、attack_id | 不直接扣血 |
| `GuardController` | 玩家格挡状态与方向覆盖 | 不直接扣血 |
| `MonsterAIController` | AI 状态、移动、攻击预警与恢复 | 不通过距离越界直接扣血 |
| `MonsterAttackDriver` | 怪兽攻击窗口、攻击 ID 和攻击区域 | 不在预警/收招状态造成伤害 |
| `Hitbox` / `Hurtbox` | 空间判定与攻击方/目标归属 | 不直接修改 HP |
| `CombatResolver` | 候选命中验证、格挡、去重、伤害批次 | 不呈现 UI、不播放动画的唯一控制以外内容 |
| `FighterHealth` | HP 数据与边界保护（由 MatchController 权威更新） | 不自行决定比赛结果 |
| `MatchController` | 创建对局、双方 HP、暂停、结果、重新开始 | 不依赖 Animator Clip 名称做核心胜负逻辑 |
| `CombatHUD` | 只读 MatchState 并绘制 HP 和状态提示 | 不计算伤害 |
| `FeedbackPresenter` | 视觉反馈、轻量屏幕震动、特效触发 | 不重复结算 |
| `AudioCuePlayer` | 通过逻辑 cue 播放音频 | 不与任何 Animator 直接耦合 |
| `SessionEventLogger` | 写入 JSONL/CSV 事件及会话摘要 | 不记录原始相机帧；不阻塞战斗主线程 |
| `RecoveryBridge` | 暂停输入失效与未来动捕恢复流程 | 不自己更改 HP |

---

## 6. 共享接口：先设计为可替换输入源

### 6.1 当前原型的输入数据

当前键鼠输入可用下列内部数据模型（类名可调整，但字段语义必须保持清晰）：

```csharp
public struct CombatInputFrame
{
    public double timestampSeconds;
    public bool inputValid;
    public bool leftPunchPressed;
    public bool rightPunchPressed;
    public bool guardHeld;
}

public struct PrototypeMotionFrame
{
    public double timestampSeconds;
    public Vector2 moveInput;     // keyboard prototype only
    public Vector2 lookDelta;
    public bool lockTargetPressed;
}
```

`CombatInputFrame` 是离散战斗输入；`PrototypeMotionFrame` 是当前键鼠移动与镜头输入。未来动捕的持续关节/根部数据属于另一个接口，不要把整个人体每帧的 3D 骨架硬塞进键鼠输入结构里。

### 6.2 对接 B 的预留合同

最新需求中，B 交给 C 的主要接口为 `AvatarState`：**完成驱动与约束后的最终角色位置、拳部/躯干锚点及有效性**；C 输出 `InteractionEvent` 和 `MatchState`。字段细节以团队接口 v0.3 为准，不应在个人原型中另造一套同名但含义不同的协议。

当前建议的输入抽象：

```text
IPlayerMotionProvider
  ├── KeyboardMotionProvider       (本次启用)
  └── AvatarStateMotionProvider    (后续由 B/集成时适配；不与键盘同时驱动根节点)

ICombatInputProvider
  ├── KeyboardCombatInputProvider  (本次启用)
  └── AvatarStateCombatProvider    (后续根据最终拳部锚点/动作事件适配)

两种输入源都进入同一套：
AttackDetector / GuardController → CombatResolver → MatchController
```

关键限制：

- C 只读取 B 的最终角色状态，不在 C 的代码里读取 ZED 原始帧或重做骨骼重定向。
- 接入动捕时，`PlayerRoot` 的位置/旋转写权限归 B；C 只读最终世界坐标的拳部和躯干锚点，控制战斗窗口和规则。
- 键鼠输入提供的角色位置不能计入真实行走动捕的验收。
- `MatchController` 只依赖 `CombatResolver` 提交的有效伤害，不依赖输入是键鼠还是动作识别。

---

## 7. UI、氛围和音频设计

### 7.1 开局流程

```text
Boot
  ↓ 检查资源与配置
Setup / StartPanel
  ↓ 选择“键鼠原型模式”并按提示操作
Arena Ready（玩家与怪兽处于安全待机）
  ↓ 点击 Start / 按 Enter
Fighting
  ├── Esc → Paused → Continue
  ├── HP 归零 → ResultPanel
  └── R / Rematch → 新 match_id → Arena Ready
```

正式动捕接入后会在 Ready 前增加设备、标定、角色预览和有效性检查；当前键鼠原型不要假装已经完成这些步骤，只显示 `Input Mode: Keyboard Prototype`。

### 7.2 HUD 组件

- 左上角：玩家头像/名字、HP 条、当前模式 `KEYBOARD PROTOTYPE`。
- 右上角：怪兽名字、HP 条。
- 中上方：`READY / FIGHTING / PAUSED / VICTORY / DEFEAT / DRAW`。
- 中央短暂提示：`LEFT PUNCH`、`RIGHT PUNCH`、`HIT`、`BLOCKED`、`WHIFF`、`INPUT LOST`。
- 暂停面板：Continue / Restart / Quit to Menu。
- 结果面板：Victory/Defeat/Draw、Final HP、Rematch、Back to Menu。
- 调试面板（F1）：当前玩家输入源、玩家状态、怪兽 AI 状态、攻击阶段、Active attack_id、最近命中/格挡事件。

### 7.3 音频层级

- `AudioSource_BGM`：地牢氛围循环，低音量，Loop=true。
- `AudioSource_SFX`：攻击、命中、格挡、怪兽预警/受击/死亡、UI 点击。
- `AudioCueLibrary` 用逻辑键映射 AudioClip，如 `Player.Punch.Hit`、`Player.Punch.Whiff`、`Player.Guard.Block`、`Monster.Telegraph`、`Monster.Hit`、`Match.Victory`、`UI.Click`。
- 同一逻辑事件只播放一次音效；不要把 AudioSource 直接写进 HP 更新代码。
- 关闭声音时，预警、命中、格挡和结果仍需有可见反馈。

### 7.4 暗黑奇幻视觉方向

- 色彩：蓝灰冷色环境光 + 少量橙色火光；避免全屏纯黑导致模型轮廓不可读。
- 场地：石砖地面、模块化石柱/墙体、两侧火盆、中央简洁的战斗圆场。
- 氛围：采用 URP 的 Lighting、RenderSettings Fog、天空盒和材质颜色；若确实需要后处理，再单独评估与 Unity 6.3 URP 兼容的 Post Processing Stack v2，不作为原型必需依赖。先用简单光照，避免大量实时点光源。
- 战斗反馈：命中时很短的 hit-stop 或轻微镜头震动（可配置、可关闭），怪兽受击明显后仰；不要用长时间全屏特效遮挡角色。
- 可读性优先于“黑暗”：攻击预警、双方 HP 与目标位置必须在录屏压缩后仍清晰。

---

## 8. 分阶段全流程工作流

> 建议每个阶段都完成“编译 → Play Mode → 记录问题 → 修复 → Git 提交”再进入下一阶段。不要一次性要求 Codex 生成整个游戏，否则会难以定位输入、碰撞和动画之间的错误。

### 阶段 0：备份、复制 Level 1 与 CircularArea 现场审计

**Codex/人工共同执行**

1. 确认 Unity Editor 是 `6000.3.27f1`、URP；材质已经修复，不要重复对全包运行 Material Upgrade。
2. 创建 Git 提交或复制整个工程作为回滚点。
3. 在 Project 面板定位已导入的 Level 1 Scene，记录真实文件路径，然后 `File > Save As` 为 `Level1_CircularArena_Combat.unity`。
4. 打开原始 Level 1 验证未改动，再打开副本继续开发。
5. 在 Hierarchy 搜索 `CircularArea`，核对完整路径、Transform、地面、Renderer、Collider、周边环境和摄像机可视范围；记录到 `Docs/CircularArea_SceneAudit.md`。不要默认 CircularArea 一定是根节点，也不要凭想象填写尺寸。
6. 盘点所有实际人物/敌人：Ellen、CircularArea 内/附近的 Chomper，以及场景中其他现存角色。记录模型、Animator、Controller、Rig、HP/伤害组件、AI、输入组件和 GameKit 原始挂点。
7. 新增 `__CombatOverlay`，在其中建立 PlayerSpawn/MonsterSpawn 和角色引用，不复制角色。
8. 建立 `Docs/ArenaComponentConflictList.md`，将所有可能与新战斗系统冲突的组件逐个记录；本阶段先只读盘点，不批量禁用。
9. 维护 `ASSET_LICENSES.md`、`ASSET_DECISIONS.md`、`ASSET_GAPS.md`、`AnimationClipMap.md`。目前 Level 1 现有资产为 Primary，Quaternius 角色/怪兽为 Fallback/Optional。
10. 若当前工程未包含需要的 CC0 音频，再选择少量必要文件；不因文档列出所有候选而全量导入所有包。

**阶段 0 验收**

- 原始 Level 1 和副本均可打开，原始关卡未修改。
- 副本仍保留全部原始场景对象与人物。
- CircularArea 的真实完整路径、Transform 与战斗可用边界已审计。
- URP 材质保持正常，无新增粉色材质；Console 无红色编译错误。
- 玩家、指定怪兽、相机、地面与碰撞体已形成资产盘点表。
- 禁用组件清单尚未执行批量修改；所有待禁用项都有依据和回滚方式。

### 阶段 1：在 Level 1 副本上搭建战斗覆层

**禁止新建一个完全替代 Level 1 的简化场景作为主线。** 本阶段的目标是在真实的 Level 1 副本上创建可运行的战斗骨架。

**Codex 任务**

- 读取 `Docs/CircularArea_SceneAudit.md` 与 `Docs/ArenaComponentConflictList.md`；先检查项目/场景当前状态，不删除、重排或重命名第三方对象。
- 新建自有目录 `Assets/_Project/`，放置项目脚本、ScriptableObject、UI、Prefab 和测试工具。
- 创建 `Tools/MotionFight/Validate Level1 CircularArea Arena` Editor 菜单，用于检查场景里是否存在 `ArenaModeController`、PlayerSpawn、MonsterSpawn、PlayerReference、MonsterReference、主相机和指定区域引用。工具要打印错误/警告，不可自动重建地形。
- 新建 `__CombatOverlay` 与 `ArenaModeController`；通过 Inspector 引用 Level 1 现有 Ellen、指定怪兽、相机、CircularArea 及 HUD，不按不确定名字用 `Find()` 猜对象。
- 保留原关卡所有对象。在明确的冲突列表里对指定组件执行可逆禁用；不要禁用整个角色 GameObject，避免把 Renderer/Animator 一并关闭。
- 对无法绑定的角色或 Clip 提供显式占位/空引用报告；优先引用现有角色，不自动生成新 Capsule 遮挡 Ellen/Chomper。
- 在 CircularArea 内设置出生点，优先基于现有地面 Collider/场景坐标，不创建新的大地板或重新摆放整个关卡。
- 使用现有 Camera/CameraRig；若需切换控制器，只在副本中禁用冲突的跟随/输入组件。检查 Main Camera/AudioListener 唯一性。
- 先完成对局状态骨架与 UI 连接，不实现复杂战斗动画。

**阶段 1 验收**

- `Level1_CircularArena_Combat` 可直接 Play；CircularArea 和 Level 1 周边场景保持正常显示。
- Ellen/现有指定怪兽可见，位置清晰，当前对局只选中一个玩家和一个对手。
- 原始 Level 1 可以重新打开且未被改变。
- 无新的地形替代品，无重复相机/AudioListener，无 missing script 或编译红错。
- 新增对象全部位于 `__CombatOverlay` 或 `Assets/_Project/`，第三方原层级保持稳定。

### 阶段 2：键鼠移动、镜头和锁定

**Codex 任务**

- 实现 Input System 键鼠输入源。
- WASD 移动、鼠标旋转镜头、Q 锁定/解除锁定怪兽。
- 玩家移动基于 `CharacterController`，与相机输入分离。
- 锁定时玩家可柔和朝向怪兽；相机应尽量保持两者都在画面内。
- 加入场地边界与相机墙体防穿透。

**阶段 2 验收**

- WASD 不改变玩家高度；人物不会从地面掉落或穿过竞技场边界。
- 鼠标移动只控制镜头；锁定切换不瞬移角色或重置血量。
- 相机不影响命中坐标。

### 阶段 3：HP、玩家拳击与格挡

**Codex 任务**

- 创建 `CombatConfig` / `AttackProfile` ScriptableObject。
- 建立 Player AttackDetector、GuardController、Hitbox/Hurtbox、CombatResolver 和 MatchController。
- 左右鼠标键产生左右拳；Space 产生格挡状态。
- 实现攻击前摇、生效、收招、复位、冷却和每次唯一 attack_id。
- 只在 Active 状态中对目标 hurtbox 做三维检测；单次攻击同一目标最多结算一次。
- 实现挥空、命中、被格挡不同事件与 UI 反馈。

**阶段 3 验收**

- 两次点击过快不会绕过攻击状态机；攻击期间不会直接多次扣血。
- Active 窗口之外接触不会产生伤害。
- 持续接触与多个 hurtbox 不重复扣血。
- 正面有效格挡按规则将伤害归零，背面攻击不被挡。
- HP 始终在 0 到 maxHP 之间。

### 阶段 4：怪兽 AI 与完整对局

**Codex 任务**

- 实现怪兽 FSM：Idle、Approach、Telegraph、AttackActive、Recovery、HitReaction、Dead、Paused。
- 只实现一个近战攻击，怪兽在攻击生效前提供可见预警。
- 玩家靠近时怪兽停止无目的穿过玩家；攻击结束后进入恢复窗口。
- 怪兽使用自身 Rig 与攻击区域，不与玩家人体骨骼共用绑定逻辑。
- 接入唯一 HP 与胜负结算，覆盖平局、暂停/恢复和重新开始。

**阶段 4 验收**

- 怪兽能持续完成接近—预警—攻击—收招循环。
- 怪兽在预警/收招/倒地/暂停期间不会任意扣血。
- 玩家胜、玩家负、同一结算步双方归零时结果正确且只触发一次。
- Restart 后双方位置、HP、AI、攻击窗口、去重集合均复位。

### 阶段 5：复用 Level 1 现有人物动画并补齐拳击/音频缺口

**不再把 Level 1 环境拼成新 Arena，也不默认替换 Ellen/Chomper。** 本阶段先从真实资产清单映射动画，再补缺口。

**人工操作 + Codex 辅助**

1. 检查 Level 1 的 Ellen 实例：模型、Avatar/Rig、Animator、Animator Controller、已有 Idle/Move/Run/Hit/Death/近战 Clip。写入 `AnimationClipMap.md`，记录真实 Clip 名与长度。
2. 检查当前选定的 Chomper/怪兽的模型、材质、Rig、Animator、移动/攻击/受击/死亡 Clip，检查攻击点在动画中的实际位置。
3. 不重命名或覆盖第三方 Animator Controller。若需新的战斗 Animator，在 `_Project/Animations/` 建立自有 Controller，并在战斗场景副本的 Animator 组件上做实例级覆盖。
4. 若 Ellen 原生动作不含左右徒手直拳，测试 Quaternius Universal Animation Library 2 的拳击相关 Clip 是否可通过 Humanoid 重定向到 Ellen。若 Avatar/手部轨迹不合格，试用 Free Those Who Fighter 作为备选或先做占位动作；不能猜 Clip 名或宣称未测试的重定向有效。
5. 为一拳建立独立的 AttackProfile，Startup/Active/Recovery 时间以实测 Clip 长度和命中观察设置。动画事件只发出开关/提示事件，不直接扣血；最终窗口由 `CombatResolver` 校验。
6. 在 ArenaModeController 中配置角色引用与需要禁用的原有控制脚本。禁用范围必须精确到组件，不删除对象、Prefab 或现有 Animator。非参战敌人保留模型并在竞技场模式中关闭旧 AI/旧攻击行为。
7. 复用 Level 1 原场景灯光、雾效、道具与环境装饰；只调整对局需要的构图。保持所有材质在 URP 下正常，不重新批量转换已修复的全包材质。
8. 接入已挑选的 CC0 音效 Cue；若没有合适的拳击/怪兽音效，先保留静音占位，不使用错误音效假装命中。
9. 在 `ASSET_GAPS.md` 记录本地资源缺口与后备选择，并更新许可证记录。

**阶段 5 验收**

- 角色仍是 Level 1 现有人物，模型、材质、比例、脚底和 Rig 正常。
- 如果使用补充动画，记录它实际播放于目标 Avatar 的测试结果；不出现明显 T-Pose/错骨骼。
- 怪兽动画与自建 AI 状态对应，旧 AI 不会覆盖自建位置或状态。
- 原 Level 1 的源资源与源场景未被修改；只有副本实例与 `_Project` 资产发生变化。
- 命中声音只在确认命中事件后播放，挥空不会播放命中声。
- 即使补充动画或音频缺失，场景仍可用现有动画或明确占位状态启动。

### 阶段 6：UI、暂停、记录、异常处理

**Codex 任务**

- 统一 HUD、开始面板、暂停面板和结果面板。
- 任何阶段可用 Esc 暂停；暂停时冻结 AI、移动、攻击、伤害、冷却、动画状态推进（必要的 UI 动画除外）和比赛时钟。
- 写入 JSONL 事件日志：session_id、match_id、event_seq、event_type、attacker_id、target_id、attack_id、blocked、damage、HP before/after、timestamp、source。
- 默认只记事件和配置，不录制原始 RGB/深度/逐帧骨架。
- 为未来追踪失效预留 `RecoveryBridge`；当前可通过 debug 按钮模拟 InputInvalid，验证双方暂停与恢复确认。

**阶段 6 验收**

- 暂停期间双方不能继续掉血；恢复时清除跨暂停未完成攻击。
- Restart 创建新 match_id，不删除历史对局日志。
- 日志中每个有效命中只出现一次，并且数值与 HUD 一致。
- 录制开关状态明确；不会在未授权时保存摄像头图像或逐帧人体数据。

### 阶段 7：全流程测试与可交付演示

- 连续完成至少 10 局：击中、挥空、格挡、玩家赢、玩家输、平局/双归零、暂停、恢复、重新开始。
- 检查不同视角/距离下命中是否合理；重点测试拳部在一帧内移动很远时是否漏检。
- 使用 F1/F2 检查攻击阶段、AI 状态、碰撞区域与有效 attack_id。
- 记录开发环境、版本、资源许可证、已知问题、测试截图与日志。
- 构建 Windows Development Build，确认脱离 Editor 后音频、输入、UI 和场景加载仍正常。

---

## 9. 必须建立的测试用例

| ID | 场景 | 预期结果 |
|---|---|---|
| TC-C01 | 站立等待，不输入攻击 | 双方 HP 不变 |
| TC-C02 | 玩家左右拳命中怪兽 | 怪兽 HP 仅扣一次，显示 HIT/播放命中音 |
| TC-C03 | 玩家拳没有碰到怪兽 | 显示 WHIFF，不扣 HP |
| TC-C04 | 拳在 Startup/Recovery 期间碰到怪兽 | 不扣 HP |
| TC-C05 | 一个攻击窗口多帧贴住怪兽 | 同一个 attack_id 只扣一次 |
| TC-C06 | 同一个怪兽有多个 hurtbox 被同时碰到 | 同一攻击只对该目标结算一次 |
| TC-C07 | Space 按住，怪兽从玩家正面进攻 | 若格挡方向/时效有效，伤害为 0 并显示 BLOCKED |
| TC-C08 | Space 按住，但怪兽从背面攻击 | 不算有效正面格挡，按命中规则结算 |
| TC-C09 | 怪兽预警阶段玩家靠近 | 预警期间尚不扣血 |
| TC-C10 | 怪兽攻击 Active 命中玩家 | 玩家 HP 只扣一次 |
| TC-C11 | 怪兽处于 Recovery / HitReaction / Dead | 不启动新攻击 |
| TC-C12 | 玩家 HP 到 0 | 只显示一次 Defeat；后续不再扣血 |
| TC-C13 | 怪兽 HP 到 0 | 怪兽停止攻击和移动，只显示一次 Victory |
| TC-C14 | 同一个结算步双方 HP 都归 0 | 显示 DRAW，唯一结果 |
| TC-C15 | 战斗中按 Esc | 立即冻结 AI、伤害、移动和攻击窗口 |
| TC-C16 | 暂停后恢复 | 不发生暂停前攻击残留命中；HP 保持不变 |
| TC-C17 | 结果界面按 R | 新 match_id、双方初始 HP、AI 重新初始化 |
| TC-C18 | F1 调试开关 | 不改变战斗判定与结果 |
| TC-C19 | 音频静音 | 静音下仍能完成战斗，视觉提示充分 |
| TC-C20 | 缺少怪兽动画 Clip 或音频 Clip | 明确降级/占位；不抛异常、不阻塞核心系统 |
| TC-C21 | 测试输入无效/追踪丢失模拟 | 双方战斗暂停；恢复须确认后继续 |
| TC-C22 | 角色换成另一个有效 Humanoid 模型 | 战斗核心逻辑无需改写，只重新绑定模型/Clip |
| TC-C23 | 打包后运行 | 键鼠、HUD、音频、对局、Restart 正常 |

每项测试记录：测试日期、Unity 版本、资源版本、输入方法、操作步骤、预期、实际、通过/失败、截图/日志路径。不要只写“已经测试”；失败项应创建 bug 记录。

---

## 10. Codex 协作方法：如何让它稳定地产生可用产物

### 10.1 先给 Codex 项目上下文

把本文件放进仓库 `Docs/C成员_键鼠控制格斗原型_Unity6000.3.27f1_整合3DGameKit资源包_全流程工作流_Codex指南.md`，同时把最新版需求资料中的以下原则摘进 `Docs/PROJECT_CONTEXT.md`：

- 一名真人玩家 + 一只电脑怪兽，一个竞技场。
- 首版键鼠模式是当前开发与测试替代输入，不是最终动捕验收。
- C 主责 FR-05、FR-07、FR-08、FR-09、FR-10、FR-13。
- `CombatResolver` 是唯一验证命中与应用伤害的模块；`MatchController` 是 HP、比赛状态和胜负的唯一权威来源。
- A/B 尚未完成；C 当前不得直接依赖他们还不存在的程序集或 SDK。
- 未来以团队数据接口 v0.3 中 `AvatarState` 作为角色驱动完成后的输入，并输出 `InteractionEvent`、`MatchState`。不要更改既定接口字段语义。
- Unity 锁定 `6000.3.27f1`，URP，Windows 桌面原型；优先使用 3D Game Kit 官方原始包在 Unity 6.3 的 URP 兼容路径。
- 当前输入源必须可替换，战斗核心不应该知道是键鼠还是人体动作触发攻击。

### 10.2 Codex 总控提示词（复制后即可使用）

```text
你是熟悉 Unity 6.3 / C# / URP / 3D Game Kit 的资深 Unity 游戏开发工程师。本项目 Editor 锁定 Unity 6000.3.27f1，URP；禁止升级/降级 Editor，禁止擅自切换渲染管线。用户已在 AssetImportSandbox_InternationalURP 导入 3D Game Kit 并修复材质，请不要重新导入资源包或重复全包材质转换。

场景方案已经确定：主战斗场景必须以完整 3D Game Kit Level1 场景为基础，通过 File > Save As 另存为 Level1_CircularArena_Combat.unity；以 CircularArea 及周边场景为竞技场。必须保留场景副本内原 Level1 的全部环境、人物、敌人、灯光、物件、触发器和资源对象，不要从零建地面，不要删减或重排第三方 Hierarchy，不要修改原始 Level1 和任何第三方 Prefab 源。

先检查真实工程，找到 Level1 Scene，并在 Hierarchy 搜索 CircularArea，记录完整路径、Transform、Collider/Renderer、地面边界、相机、Ellen、Chomper 和其他原有关卡角色。不要猜路径、对象名或 Clip 名。建立 Docs/CircularArea_SceneAudit.md 与 Docs/ArenaComponentConflictList.md。

在场景根节点仅新增本项目 __CombatOverlay，包含 ArenaModeController、ArenaReferences、PlayerSpawn、MonsterSpawn、CombatParticipants、MatchController、CombatResolver、MonsterAIController、UI_Overlay 与 Debug。优先引用 Level1 中现有 Ellen 作为玩家、CircularArea 内/附近一只现有 Chomper 作为怪兽；如果实际对象缺失或不满足动画要求，先报告证据与备选，不自动换成 Quaternius 模型。其他角色对象保留，只通过显式名单在 Arena 模式禁用会干扰 1v1 的旧 AI/旧攻击/旧伤害组件。禁用行为组件，不关闭 Renderer/Animator，不删除对象。

可玩原型要求：WASD 移动，鼠标相机，Q 锁定目标，鼠标左键左直拳、右键右直拳、Space 格挡；不包含踢腿、翻滚、武器系统、复杂连招、多敌人参战。怪兽 FSM：Idle/Approach/Telegraph/AttackActive/Recovery/HitReaction/Dead/Paused。双方 HP、有效窗口、格挡、胜负/平局、暂停/恢复、重新开始与 JSONL 事件记录完备。

必须遵守：
1. 先审计，再修改；每个场景/组件更改都有回滚路径。不要批量删除或改名第三方对象。
2. 原关卡行为组件若会冲突，只在战斗副本通过 ArenaModeController 显式禁用；同一 Actor 的 Transform/Animator/HP/AI/伤害只能有一个写入权威。
3. 原始 Game Kit Damageable 不与自建 CombatResolver 并行扣血；UI/Audio/VFX/Animation Event 不得更改 HP。
4. CombatResolver 统一校验 attack_id、MatchState、Startup/Active/Recovery、三维碰撞、格挡、追踪有效性和去重；MatchController 是 HP 与胜负的唯一权威。
5. 人物/怪兽优先使用 Level1 现有模型与动画。拳击 Clip 不存在时，先盘点 AnimationClipMap；只有需要时才测试 Quaternius Universal Animation Library 2/Free Those Who Fighter 动画到 Ellen Avatar 的兼容性，不能猜名称。
6. 主工程当前阶段不依赖 ZED SDK 或 A/B 尚未实现的程序集。保留接口以接入 AvatarState、左右拳锚点、质量、时间戳、追踪状态与恢复事件。
7. 将资源来源/许可证、实际 Clip 名、禁用组件原因、场景边界和测试记录写入 Docs。测试没运行时标 Not Run，不得宣称通过。
8. 一次只完成我指定的阶段，先报告将修改的文件和场景操作，再实施。阶段完成后提供新增/修改文件、Unity 手动步骤、验证结果、风险和下一步前置条件。

当前第一任务：只做 Level1/CircularArea 场景审计与安全副本；输出 SceneAudit、组件冲突清单和保留/待禁用对象报告。除新建文档和必要的 __CombatOverlay 骨架外，不要实现完整战斗，不要批量启停脚本。
```

### 10.3 分阶段 Codex 提示词

#### Prompt A：Level 1 场景审计与 CircularArea 战斗覆层

```text
执行阶段 0/1 的场景部分。先不要生成一个空白 Arena，也不要从零创建地面。
1. 检查原始 Level1 Scene 的实际路径；确保已另存为 Level1_CircularArena_Combat.unity，原始 Level1 未被覆盖。
2. 搜索并定位 CircularArea，记录完整 Hierarchy 路径、Transform、Collider/Renderer、地面边界、周边环境和相机。
3. 盘点 Level1 所有实际人物/敌人，包括 Ellen、Chomper 及其他角色，记录 Animator/Controller、移动 AI、Damageable/ContactDamager/PlayerController 和可能引起场景切换/HP 重置的行为组件。
4. 只在场景根节点新增 __CombatOverlay；在其中建立 ArenaModeController、ArenaReferences、PlayerSpawn、MonsterSpawn、CombatParticipants 和调试验证工具。
5. 保留原 Level1 场景内容与 Hierarchy，不删除、重命名、重排原对象，不修改第三方 Prefab。不要在本阶段批量禁用组件，只输出冲突建议与可回滚列表。
6. 使用现有 Ellen 与一只适当的现有关卡怪兽作为候选引用；如果角色不在 CircularArea 附近，只报告并提出在副本中调整实例位置的方案，不复制出第二份角色。
7. 不创建替代 Ground、整圈新墙或 Quaternius 新场景。
完成后输出 Docs/CircularArea_SceneAudit.md、Docs/ArenaComponentConflictList.md、修改文件清单、Unity 人工验收步骤。不得声称实测过未运行的场景。
``` 

#### Prompt B：输入、移动和相机

```text
执行工作流阶段 2。只做 Input System 键鼠输入、WASD CharacterController 移动、鼠标第三人称相机、Q 锁定目标和场地边界。移动与相机必须分离；镜头旋转不能修改玩家世界坐标；锁定不会瞬移角色。请保持核心接口可替换，并提供缺省 Inspector 配置。结束后逐项检查阶段 2 验收标准。
```

#### Prompt C：拳击、格挡与伤害

```text
执行工作流阶段 3。实现左右拳攻击生命周期、Space 格挡、hitbox/hurtbox、CombatResolver、MatchController、HP UI 与日志事件。先使用 Capsule/Sphere 占位锚点。采用唯一 attack_id；只有 Active 窗口可提交命中候选；同一个攻击对同一目标最多扣血一次。UI/音频/特效不得修改 HP。实现并验证 TC-C01 至 TC-C08。
```

#### Prompt D：怪兽 AI 与对局闭环

```text
执行工作流阶段 4。实现怪兽有限状态机 Idle/Approach/Telegraph/AttackActive/Recovery/HitReaction/Dead/Paused，只有 AttackActive 可以命中玩家。预警必须在视觉上可读。集成双方 HP、Victory/Defeat/Draw、Esc 暂停、R 再开一局。暂停期间冻结 AI 和攻击窗口，恢复时清除旧攻击，保留 HP。实现并验证 TC-C09 至 TC-C17。
```

#### Prompt E：在 Level1 角色上配置动画、战斗组件与音频

```text
执行工作流阶段 5。当前主场景是 Level1_CircularArena_Combat，基于完整 Level1 的副本，使用 CircularArea 及周边环境。不得改回空白 Arena 或其他简化场景，不得删去 Level1 原有环境/人物。

先扫描当前工程与 Docs/AnimationClipMap.md。优先使用场景里已存在的 Ellen 和指定 Chomper 的模型、材质、Rig、Animator 与已有动画。只对场景副本实例赋值，不修改原始 Level1 或第三方 Prefab 源。

1. 枚举实际 AnimatorController/AnimationClip；记录 Idle、Move、Attack、Hit、Death 等 Clip 的真实名称和长度。
2. 如果 Ellen 没有左右徒手直拳，输出动画缺口后，再测试 Quaternius Universal Animation Library 2 的拳击 Clip 是否能兼容 Ellen Avatar；必要时把 Free Those Who Fighter 作为另一个导入测试备选。不许猜 Clip 名。
3. 不删除 Ellen 的法杖/手持对象。如果徒手动作与其冲突，可提供仅在 Combat Scene 生效的 Renderer 可选开关，默认不改原始 Prefab。
4. 配置 ArenaModeController 的参与者引用与显式组件禁用列表。只禁用会冲突的旧 PlayerController/EnemyController/Damageable 等组件，不关闭模型 Renderer/Animator，不在新系统中并用旧 Damageable。
5. 对怪兽动画做适配；保证其 Root Transform 只能由一个系统驱动，避免 NavMeshAgent、Root Motion 与新 AI 同时修改位置。
6. 复用 Level1 现有环境与灯光，不重新建造场景。只补入必要音频 Cue，记录许可证。
7. CombatResolver 的 Active 窗口仍是命中的唯一权威；Animator Event 不能直接扣血。
完成后给出 Clip Map、组件禁用清单、资产缺口、Inspector 配置步骤和逐项验收结果；未运行项目时标记 Not Run。
```

#### Prompt F：UI、日志、异常和构建

```text
执行工作流阶段 6 和 7。完善开始、暂停、恢复、结果 UI；JSONL/CSV 记录；追踪/输入无效模拟开关；静音设置；F1/F2 调试面板；Windows 构建说明。检查所有清单测试。不要声称运行过 Unity 测试，除非当前环境确实能启动 Unity 并跑测试。输出逐条测试结果表，未知/无法实测项目标为 Not Run。
```

### 10.4 每次 Codex 任务结束时要求的报告模板

```text
## 本阶段变更
- 新增文件：
- 修改文件：
- 删除文件及原因：

## Unity 中需要人工完成的操作
1.
2.

## 验证步骤
1.
2.

## 测试结果
- Compile: Pass/Fail/Not Run
- Play Mode: Pass/Fail/Not Run
- Build: Pass/Fail/Not Run

## 已知问题
- 

## 下一阶段前置条件
- 
```

### 10.5 Codex 不应该做的事情

- 不得自动升级 Unity Editor 或无理由重写 `Packages/manifest.json`。
- 不得删除用户已有场景、Prefab、资源或项目配置。
- 不得将所有脚本写入单个 `GameManager.cs`。
- 不得在 OnTriggerEnter、Animator Event、HUD 或 AudioSource 回调里直接扣血。
- 不得让玩家 AI、怪兽 AI、UI 与 CombatResolver 多处维护 HP。
- 不得因为动画 Clip 没找到就写入臆测字符串后假定能播放。
- 不得把相机局部坐标当作世界坐标来做命中。
- 不得用 2D 屏幕重叠替代 3D 命中。
- 不得用键盘移动测试结果冒充真实动捕位移结果。
- 不得默认记录/导出摄像机原始图像或逐帧身体数据。
- 不得把第三方资源包复制到公共仓库以外的分发包中，除非许可证明确允许。

---

## 11. Git 与文件管理约定

### 11.1 `.gitignore` 最低要求

使用 Unity 官方推荐的 Unity `.gitignore`，至少忽略：

- `Library/`
- `Temp/`
- `Obj/`
- `Build/`、`Builds/`
- `Logs/`
- `UserSettings/`
- `.vs/`、`*.csproj`、`*.sln`（如团队习惯不提交 IDE 生成文件）
- 大型导出录制文件和本地测试缓存

必须保留 `Assets/`、`Packages/manifest.json`、`Packages/packages-lock.json`、`ProjectSettings/`、`*.meta` 和团队文档。不要忽略 `.meta` 文件，否则 Unity GUID 引用可能丢失。

### 11.2 建议提交点

1. `chore: lock Unity 6000.3.27f1 baseline`
2. `feat: add prototype arena and bootstrap scene`
3. `feat: add keyboard movement and third-person camera`
4. `feat: add player punch and guard combat loop`
5. `feat: add monster FSM and match resolution`
6. `feat: bind art, animation and audio assets`
7. `feat: add combat HUD, pause, rematch and event log`
8. `test: add prototype acceptance checks and docs`

不要把未验证的大量文件一起提交；每个 commit 只包含单一可复核阶段。

---

## 12. 未来与 A/B 动捕模块集成的交接计划

键鼠原型完成后，不需要推倒重做。未来集成步骤应为：

1. B 提供最终角色 `AvatarState`：世界根节点、最终拳部/躯干锚点、时间戳与有效性；具体字段按接口 v0.3。
2. C 写 `AvatarStateCombatProvider`，只把必要拳部轨迹/动作事件映射为 `CombatInputFrame` 等内部战斗输入。
3. C 禁用 `KeyboardMotionProvider`，由 B 独占 `PlayerRoot` 的位置/旋转和骨骼驱动；键鼠 provider 留作开发模式。
4. `AttackDetector` 使用最终锚点轨迹而非键盘点击；仍沿用攻击阶段、复位、attack_id、Hitbox/Hurtbox 和 `CombatResolver`。
5. `GuardController` 接入 B 的格挡姿态/动作事件以及必要关节质量；无效输入触发 `RecoveryBridge`，暂停双方战斗。
6. `MatchController`、怪兽 AI、HP、UI、音频、记录不应因输入来源改变而重写。
7. 重新运行全部测试；单独报告键鼠原型测试和真实动捕测试，不能混在一个结果表里。

### 12.1 C 与 B 的职责边界

- B：动捕后的根节点位置、朝向、角色骨骼和最终锚点。
- C：用这些最终数据判断攻击/格挡的游戏规则，管理怪兽、命中、HP、对局和反馈。
- C 不直接设置 B 的骨骼局部旋转；B 不直接在其姿态处理逻辑中扣游戏 HP。
- 共同维护接口版本和坐标时序；接口改变必须三人同步修改文档与测试。

---

## 13. 最终交付物检查清单

### 必须交付

- [ ] Unity `6000.3.27f1` URP 工程能打开。
- [ ] `Level1_CircularArena_Combat` 在当前已导入的 Level 1 资源下可直接运行；占位模型只能用于独立逻辑测试，不得替代正式场景中的 Ellen/指定怪兽。
- [ ] 玩家 WASD 移动、鼠标相机、Q 锁定。
- [ ] 左右直拳与 Space 格挡正常。
- [ ] 怪兽能接近、预警、攻击、收招、受击与死亡。
- [ ] 有统一命中结算；同一攻击不重复扣血。
- [ ] HP、胜负/平局、暂停、恢复、重新开始正常。
- [ ] 音频 cue 与视觉反馈可用；静音也能读懂战斗。
- [ ] JSONL/CSV 事件记录可导出。
- [ ] F1/F2 调试视图可验证战斗状态和碰撞区域。
- [ ] 已导入资产的许可证、下载来源和真实 Clip 名称有记录。
- [ ] 若使用 3D Game Kit，已记录实际导入的包版本、渲染管线、导入测试结果；未把其 EnemyController/Damageable 与自建战斗系统混用。
- [ ] 所有测试都标注 Pass/Fail/Not Run，并保留证据。
- [ ] README 有环境、运行步骤、键鼠说明和构建步骤。

### 最终录屏建议

建议录制 60–90 秒：开场介绍键鼠操作 → 玩家接近怪兽 → 怪兽预警 → 玩家格挡一次 → 左右拳命中 → 显示双方 HP 变化 → 玩家或怪兽胜利 → 重新开始。另录一段 15 秒调试视图，证明攻击窗口和重复命中去重在运行，而不是只展示预渲染动画。

---

## 14. 资源与官方文档索引

### Unity 版本与 API

- Unity `6000.3.27f1` 官方发布说明与已知问题：<https://unity.com/cn/releases/editor/whats-new/6000.3.27f1>
- Unity Input System 1.20 系列文档：<https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/index.html>
- Animator Controller（Unity 6.3）：<https://docs.unity3d.com/6000.3/Documentation/Manual/class-AnimatorController.html>
- ScriptableObject（Unity 6.3）：<https://docs.unity3d.com/6000.3/Documentation/ScriptReference/ScriptableObject.html>
- Unity Trigger（Unity 6.3）：<https://docs.unity3d.com/6000.3/Documentation/Manual/collider-interactions-ontrigger.html>
- Unity SphereCast（Unity 6.3）：<https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Physics.SphereCast.html>
- Asset Store EULA FAQ：<https://assetstore.unity.com/zh-CN/browse/eula-faq>

### 模型、环境与动画

- 用户指定的中国资源商店页面（需独立工程验证兼容性）：<https://assetstore.u3d.cn/packages/templates/tutorials/assetstore-package-20005962>
- Unity 官方国际版 3D Game Kit（Unity 6.3 URP 兼容）：<https://assetstore.unity.com/packages/templates/packs/3d-game-kit-3d-sample-project-115747>
- 3D Game Kit Reference Guide：<https://learn.unity.com/tutorial/3d-game-kit-reference-guide>
- Quaternius Universal Base Characters（CC0）：<https://quaternius.com/packs/universalbasecharacters.html>
- Free Those Who Fighter Animations（Asset Store EULA）：<https://assetstore.unity.com/packages/3d/animations/free-those-who-fighter-animations-318132>
- Quaternius Universal Animation Library 2（CC0）：<https://quaternius.com/packs/universalanimationlibrary2.html>
- Quaternius Ultimate Monsters（CC0）：<https://quaternius.com/packs/ultimatemonsters.html>
- Quaternius Modular Dungeons（CC0）：<https://quaternius.com/packs/modulardungeon.html>
- Unity Fantasy Asset Pack FREE（Asset Store EULA）：<https://assetstore.unity.com/packages/3d/environments/fantasy/fantasy-asset-pack-free-130407>

### 音频

- 37 hits/punches（CC0）：<https://lpc.opengameart.org/content/37-hitspunches>
- 80 CC0 creature SFX（CC0）：<https://opengameart.org/content/80-cc0-creature-sfx>
- Dark Cavern Ambient（CC0）：<https://opengameart.org/content/dark-cavern-ambient>
- Battle Theme（CC0）：<https://opengameart.org/content/battle-theme-0>
- Kenney UI Audio（CC0）：<https://kenney.nl/assets/ui-audio>
- Kenney Impact Sounds（CC0）：<https://kenney.nl/assets/impact-sounds>

---

## 15. 首次开工的最小操作清单（CircularArea 版）

1. 确认 Unity `6000.3.27f1`、URP 与材质显示正常；Git 提交/备份当前工程。
2. 定位现有 Level1 场景，使用 Save As 生成 `Level1_CircularArena_Combat.unity`，确认原始 Level1 未修改。
3. 在 Hierarchy 搜索 `CircularArea`，记录完整路径、Transform、地面/边界 Collider、周边遮挡与摄像机可视范围。
4. 确定 Ellen 与 CircularArea 内/附近的一只现有怪兽作为当前对局角色；其他人物保留在场景里并记录是否会干扰一对一规则。
5. 新增 `__CombatOverlay`、ArenaModeController、PlayerSpawn、MonsterSpawn 与引用槽；不重排原层级、不复制角色、不创建替代地板。
6. 盘点旧控制/AI/伤害/关卡触发组件，先写 `ArenaComponentConflictList.md`，确认后再在副本内按组件禁用。
7. 先用键鼠完成一个完整战斗闭环，再接 A/B 的真实 `AvatarState`；不在当前阶段接入 ZED SDK。
8. 对每一阶段记录实际修改的场景、组件、测试结果和尚未验证的部分。

**启动成功标准：**打开 Level1 副本能看到 CircularArea 与周边完整场景、Ellen 与目标怪兽；原始关卡可回退；没有材质粉色/编译红错；没有删除原有关卡对象或创建第二套争夺控制权的系统。
