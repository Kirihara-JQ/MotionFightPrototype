# 交付物完成清单与待优化项目

文档基线：`MotionFightPrototype v1.1`
基线提交：`fdfc206 feat: complete arena combat and HUD V2`
Unity：`6000.3.27f1`
正式场景：`Assets/Scenes/Combat/Level1_CircularArena_Combat.unity`

## 一、总体状态

| 维度 | 状态 | 说明 |
| --- | --- | --- |
| 可运行战斗原型 | 已完成 | Ellen 对 Chomper 的 1v1 流程可运行 |
| 原版动作链 | 已完成并验证 | 移动、攻击、受击、死亡和 Root Motion 保留 |
| 权威 HP/伤害 | 已完成 | 统一经过 `CombatResolver` |
| 对局管理 | 已完成 | Ready、Fighting、Paused、胜负和平局状态 |
| Combat HUD V2 | 已完成 | Prefab、场景绑定、响应式布局和交互完成 |
| Windows 构建 | 已完成 | Standalone 构建成功 |
| 自动测试体系 | 部分完成 | 已执行实机验证，但缺少完整自动化回归套件 |
| 资源彻底瘦身 | 未完成 | 893 个 REVIEW 项和原始 Level1 等高风险资源仍保留 |
| 人体动捕接入 | 未开始 | 仅保留动作调用边界 |

## 二、已完成交付物

### 1. 工程与场景

- [x] 建立独立正式战斗场景 `Level1_CircularArena_Combat.unity`。
- [x] 正式场景作为唯一启用构建入口。
- [x] 保留 CircularArea、环境碰撞、灯光、URP 材质和必要音频。
- [x] 保留一名可控制 Ellen。
- [x] 仅一只 Chomper 进入实际战斗，其余实例不参与本局。
- [x] 原始 Level1 文件未被覆盖或删除。
- [x] Unity 版本保持 `6000.3.27f1`。
- [x] URP 与 Input System 未被替换或升级。

### 2. 玩家与怪兽动作

- [x] Ellen 继续使用 `PlayerController`、`PlayerInput`、CharacterController 和原版 Animator。
- [x] Chomper 继续使用 `ChomperBehavior`、`EnemyController`、NavMeshAgent 和 Animator Root Motion。
- [x] 左右直拳改为武器轻击重击接入原版 Staff/MeleeWeapon 动作链。
- [x] 原版攻击前摇、攻击窗口、收招和冷却由动作模块控制。
- [x] 恢复 Ellen 受击动画、受伤音频和相机反馈。
- [x] Ellen 受击无敌时间为 `1.5 s`，Renderer 约每 `0.3 s` 闪烁。
- [x] Ellen 死亡播放原版 `Ellen_Death` 并保持最终帧。
- [x] Chomper 攻击保留 Root Motion 前冲。
- [x] Chomper 受击、死亡和布娃娃替换由原版动作链处理。
- [x] Ellen 死亡后停止 Chomper 战斗行为并保持待机。

### 3. 战斗核心

- [x] `CombatantHealth` 作为玩家和怪兽唯一 HP/死亡数据源。
- [x] `CombatResolver` 作为唯一有效伤害入口。
- [x] `LegacyDamageBridge` 接收原版 Damageable 事件。
- [x] 原版 Damageable 池不参与显示、死亡和胜负。
- [x] 自伤被拒绝。
- [x] 单次攻击对多 Collider 只结算一次。
- [x] 正面格挡和背后攻击规则完成。
- [x] 左右拳采用独立配置：
  - `LeftStraight.asset`：10 伤害。
  - `RightStraight.asset`：12 伤害。
- [x] 格挡角默认 `120°`，格挡伤害倍率默认 `0.1`。
- [x] HP 限制为 0 到最大 HP。
- [x] 死亡事件只触发一次。

### 4. 对局系统

- [x] `MatchController` 管理 Ready、Fighting、Paused、PlayerWin、MonsterWin 和 Draw。
- [x] 对局结束立即停止玩家攻击、AI 行为和伤害窗口。
- [x] `Esc` 暂停/恢复。
- [x] `R` 重开。
- [x] Restart 使用正式战斗场景重载。
- [x] 重开后 HP、AI 和 HUD 状态复位。
- [x] 双方同帧死亡时由 `MatchController` 判定平局。

### 5. Combat HUD V2

- [x] 左上玩家青蓝面板、右上怪兽猩红面板。
- [x] 真实 Ellen 和 Chomper 头像。
- [x] 当前/最大 HP、百分比和 HP 条。
- [x] 主 HP 条、延迟条、受击闪光和低血量光晕。
- [x] 顶部中央真实对局状态。
- [x] 玩家胜利、怪兽胜利和平局面板。
- [x] 暂停、恢复、重开和退出交互。
- [x] `MAIN MENU` 在无有效场景时禁用。
- [x] 底部键鼠操作提示，并预留动捕显示模式。
- [x] 按钮 Hover、Pressed、面板淡入/淡出和 Restart fade。
- [x] HUD Prefab 和可重复 Editor 生成工具。
- [x] 1920×1080、1600×900、2560×1440、16:10 布局验证。

### 6. 清理与文档

- [x] 删除 145 个已验证 REMOVE 资源。
- [x] 不删除 REVIEW 项。
- [x] 不手工删除 `.meta`。
- [x] 正式场景依赖资源全部保留。
- [x] 原始 Level1 因 LightingData 间接依赖保留。
- [x] 工程已推送到 GitHub `main`。
- [x] 新增根目录 README。
- [x] UI、战斗、清理和验收文档已归档到 `Docs/`。

## 三、已执行验证

| 验证项 | 状态 | 说明 |
| --- | --- | --- |
| Scripts Compile | 通过 | 0 个新增编译错误 |
| 正式场景 Play Mode | 通过 | Ready -> Fighting 正常 |
| 左右拳伤害 | 通过 | 10 / 12 点 |
| 正面格挡 | 通过 | 20 伤害被降低为 2 |
| Ellen 受击/无敌 | 通过 | 原版动作与 1.5 秒无敌 |
| Ellen 死亡 | 通过 | 原版死亡动作并保持 |
| Chomper 前冲/攻击 | 通过 | 原版 Root Motion |
| Pause / Resume | 通过 | 真实按钮调用 MatchController |
| Restart | 通过 | 场景重载且 HP 复位 |
| HP/延迟条 | 通过 | 实时更新和延迟下降 |
| 低血量提示 | 通过 | 玩家 12 HP 时触发 |
| 玩家胜/怪兽胜 | 通过 | 实机与 CombatResolver 结算验证 |
| Draw 展示 | 部分通过 | 面板文案已验证，未构造严格同帧死亡实战 |
| 单 Canvas/EventSystem/Camera/AudioListener | 通过 | Play Mode 各 1 个活动实例 |
| Windows Standalone Build | 通过 | `Build Finished, Result: Success.` |
| Standalone EXE 整局回归 | 未执行 | 构建成功，但未人工完整游玩 |

## 四、待优化项目

### P0：发布前必须完成

1. **完成独立 Windows Player 整局人工验收**
   - 启动构建产物。
   - 手动验证 WASD、鼠标视角、左/右键、Shift、Esc、R。
   - 完成玩家胜、怪兽胜和重开全流程。

2. **补充双方同帧死亡自动化测试**
   - 构造同一物理帧双方 HP 归零。
   - 验证只进入一次 `Draw`。
   - 验证结果面板、攻击停止和重开复位。

3. **建立可重复的 PlayMode 战斗回归测试**
   - 伤害唯一入口。
   - 格挡正反面。
   - 多 Collider 去重。
   - 死亡事件只触发一次。
   - 暂停期间 HP、AI 和攻击窗口冻结。

4. **明确发布流程中的主菜单策略**
   - 若 v1.1 只作为单场景原型，继续隐藏主菜单。
   - 若需要完整产品，新增合法主菜单场景并加入 Build Profiles。
   - 不在 HUD 中伪造跳转。

5. **清除或隔离 Cinemachine 序列化警告**
   - 确认 `CompositionModifier` 的序列化方式。
   - 不通过升级 Packages 盲修。

### P1：玩法与动作完善

1. **补充 Ellen 专属格挡动作**
   - 当前格挡状态和伤害逻辑已生效，但尚无独立第三方动作资源。
   - 添加时不得覆盖原版 Animator Controller，应通过受控副本或上层动作钩子接入。

2. **完善左右直拳差异化**
   - 当前主要差异为伤害。
   - 后续可增加起手、有效帧、收招、命中停顿、音效和相机反馈差异。

3. **增加战斗反馈表现**
   - 命中停顿与短时慢动作。
   - 更强的武器碰撞音和材质音。
   - 受击方向/强弱分级。
   - KO 镜头和结算节奏。

4. **优化 Chomper AI 的竞技可读性**
   - 动画预警与有效伤害窗口的一致性。
   - 避免贴脸后难以观察的攻击角度。
   - 调整攻击频率、追逐距离和转身速度。

5. **建立数值调参入口**
   - 将 HP、伤害、冷却、格挡倍率集中到可追踪配置。
   - 增加开发用参数面板或调参 ScriptableObject。

### P1：工程与资源

1. **将场景精简为真正的单体 Chomper 实例**
   - 当前为安全起见保留 26 个源实例，并在战斗模式中隔离其他实例。
   - 后续可在确认第三方 Prefab、Spawner 和引用后，将正式场景改为一个实例。

2. **继续处理 893 个 REVIEW 资源**
   - 按引用族逐批验证，不使用目录通配删除。
   - 优先处理仍被保留但正式战斗未使用的示例场景、教程、解谜和旧 UI。

3. **审计 519 个既有未解析 GUID token**
   - 先区分历史无效引用、可选工具引用和真实运行依赖。
   - 不得为了降低数量而批量清除。

4. **解除原始 Level1 的 LightingData 间接依赖**
   - 为正式场景生成独立 Lighting Data。
   - 重新验证后再决定是否删除原始 Level1。

5. **旧 UI 与旧功能隔离**
   - 正式场景中仍有大量未激活 Canvas 和旧玩法组件。
   - 应在新系统稳定后按对象路径、GUID 和运行时行为逐步清理。

### P2：扩展能力

1. **RGB-D / ZED 人体动捕接入**
   - 复用 `PlayerCombatInputAdapter.RequestAttack` / `RequestGuard` 边界。
   - 不直接让动捕脚本操作 HP、Animator 或 MatchController。

2. **输入重绑定与手柄支持**
   - 统一键鼠、手柄和动捕动作命令。
   - 增加可配置按键提示。

3. **HUD 安全区与更多比例**
   - 增加 21:9、带系统安全区和窗口化异常尺寸检查。

4. **音频与本地化**
   - UI 音效、角色语音、字幕和多语言文本。
   - 当前 README 和 HUD 文本以中文文档/英文界面为主。

5. **性能与包体分析**
   - 检查场景 Draw Call、动画开销、未激活对象数量和构建包体。
   - 当前构建包体约 1.7 GB，主要来自保留的 3D Game Kit 资源。

## 五、维护约束

后续修改必须遵守：

- 不覆盖原始 Level1。
- 不覆盖第三方 Chomper/Ellen Prefab、Animator Controller 或源模型。
- 不让普通攻击、AI 或旧 Damageable 绕过 `CombatResolver`。
- 不让 HUD 自行维护第二套 HP 或胜负状态。
- 不新增重复 Canvas、EventSystem、Camera 或 AudioListener。
- 不升级 Unity、URP、Input System 后再直接提交未审查的序列化差异。
- 大文件继续通过 Git LFS 提交。
- 未实际执行的测试必须标记为“未执行”，不能写成通过。
