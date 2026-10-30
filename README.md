# MotionFightPrototype v1.1

基于 Unity 3D Game Kit 资源整理的 CircularArea 1v1 格斗原型。

项目保留 Ellen 与 Chomper 的原版移动、攻击、受击、死亡和 Root Motion 动作链，在原有动作模块之上接入权威 HP、伤害结算、对局状态与 Combat HUD V2。项目目标是一个可运行、可继续迭代的 1v1 格斗垂直切片，而不是完整保留 3D Game Kit 示例玩法。

当前状态：**Windows Standalone 构建通过，核心战斗与 HUD 已完成 Play Mode 验证。**

## 核心特性

- CircularArea 独立正式战斗场景。
- 一名玩家 Ellen、一只实际参战 Chomper。
- 保留原版第三人称移动、相机和 Chomper 追击/攻击动作。
- 鼠标左键左直拳、鼠标右键右直拳、左 Shift 正面格挡。
- 统一 `CombatResolver` 伤害入口与 `CombatantHealth` 权威 HP。
- Ellen 受击、受伤音频、相机反馈、无敌闪烁和原版死亡动作。
- Chomper 原版攻击前摇、Root Motion 前冲、受击、死亡与布娃娃替换。
- `MatchController` 管理 Ready、Fighting、Paused、胜负和平局。
- Combat HUD V2：双方 HP、延迟血条、低血量提示、命中/格挡反馈、暂停和结算面板。
- 键鼠操作提示，并为未来 RGB-D 动捕指令入口预留接口。

## 环境要求

| 项目 | 版本/要求 |
| --- | --- |
| Unity | `6000.3.27f1` |
| 渲染管线 | URP `17.3.0` |
| 输入 | Input System `1.20.1` 已安装；当前原型保留项目既有输入路径，不要擅自切换 Active Input Handling |
| UI | UGUI + TextMesh Pro |
| 操作系统 | 当前验证环境为 Windows，目标构建为 Windows Standalone x64 |
| 资源同步 | Git LFS，仓库中的 PNG、FBX、音频等大文件依赖 LFS |

必须使用 Unity `6000.3.27f1` 打开工程。不要升级或降级 Unity、URP 或 Input System 后再直接覆盖现有序列化资产。

## 快速开始

1. 安装 Git LFS：

   ```powershell
   git lfs install
   ```

2. 克隆仓库并同步 LFS 资源：

   ```powershell
   git clone https://github.com/Kirihara-JQ/MotionFightPrototype.git
   cd MotionFightPrototype
   git lfs pull
   ```

3. 使用 Unity Hub 以 Unity `6000.3.27f1` 打开仓库目录。

4. 打开正式战斗场景：

   ```text
   Assets/Scenes/Combat/Level1_CircularArena_Combat.unity
   ```

5. 等待脚本编译和资源导入完成，进入 Play Mode。

该场景是当前 Build Settings 中唯一的启用场景。游戏启动后经过短暂 Ready 状态自动进入 Fighting。

## 操作

| 输入 | 动作 |
| --- | --- |
| `W A S D` | 移动 |
| 鼠标 | 相机观察 |
| 鼠标左键 | 左直拳，10 点基础伤害 |
| 鼠标右键 | 右直拳，12 点基础伤害 |
| 左 `Shift` | 正面格挡 |
| `Esc` | 暂停/恢复 |
| `R` | 重开当前对战 |

格挡只保护角色正面，当前格挡角为 `120°`。成功格挡时保留 `10%` 伤害；参数可在场景组件 Inspector 中调整。

## 战斗规则

- 原版 `Damageable` 仍作为动画事件命中载体，但不再负责正式 HP、死亡或胜负。
- 所有有效伤害必须经过：

  ```text
  LegacyDamageBridge
    -> CombatResolver
    -> CombatantHealth
  ```

- 玩家和 Chomper 最大 HP 均为 `100`。
- Ellen 受击后无敌时间为 `1.5 s`，Renderer 约每 `0.3 s` 切换一次可见状态。
- Chomper 受击无敌时间当前为 `0.35 s`。
- 战斗结束后停止新的移动决策、AI 行为和攻击结算。
- Chomper 死亡保留原版布娃娃替换；重开通过重新加载正式场景完成。

## 架构

```text
PlayerController / PlayerInput
  -> PlayerCombatInputAdapter
  -> original Ellen melee animation + MeleeWeapon
  -> original Damageable event
  -> LegacyDamageBridge
  -> CombatResolver
  -> CombatantHealth
  -> MatchController
  -> MatchHUD

ChomperBehavior / EnemyController
  -> NavMeshAgent + original Animator Root Motion
  -> original attack / ContactDamager
  -> LegacyDamageBridge
  -> CombatResolver
  -> CombatantHealth
```

职责边界：

- `CombatantHealth`：唯一 HP 与死亡状态来源。
- `CombatResolver`：唯一权威伤害入口，统一处理格挡和伤害。
- `MatchController`：唯一对局状态、暂停、重开和胜负管理者。
- `MatchHUD`：只负责表现和交互转发，不独立计算 HP 或胜负。
- `ChomperBehavior` / `EnemyController`：仅由原版组件负责 Chomper 行为决策。

## 关键目录

```text
Assets/Scenes/Combat/
  Level1_CircularArena_Combat.unity       正式战斗场景

Assets/_Project/Scripts/Combat/
  CombatantHealth.cs                     权威 HP
  CombatResolver.cs                      权威伤害结算
  LegacyDamageBridge.cs                  原版 Damageable 桥接
  MatchController.cs                     对局状态
  MatchHUD.cs                            HUD 总协调

Assets/_Project/Scripts/UI/
  CombatantHealthBarView.cs              HP 条视图
  MatchStatusView.cs                     对局状态视图
  MatchResultView.cs                     胜负视图
  PauseMenuView.cs                       暂停视图
  ControlHintView.cs                     操作提示

Assets/_Project/Data/Combat/
  LeftStraight.asset                     左直拳配置
  RightStraight.asset                    右直拳配置

Assets/_Project/Prefabs/UI/
  PF_CombatHUD_V2.prefab                 Combat HUD V2

Assets/_Project/Editor/UI/
  CombatHUDV2Builder.cs                  HUD 可重复生成工具
```

## HUD 重建

Combat HUD V2 使用 Editor 工具生成和接入。若 Prefab 需要重新生成，在 Unity 中执行：

```text
Tools/Motion Fight/UI/Rebuild Combat HUD V2
```

该工具会复用现有 Canvas，清理重复的生成型 HUD 根节点，并确保场景中存在一个有效 EventSystem。执行后请保存正式战斗场景。

## 构建

1. 确认 Build Profiles / Build Settings 中只有：

   ```text
   Assets/Scenes/Combat/Level1_CircularArena_Combat.unity
   ```

2. 平台选择 `Windows x86_64`。

3. 构建并运行。

本次最终验证构建输出：

```text
Builds/Validation/MotionFightPrototype.exe
```

`Builds/`、`Logs/`、`Library/`、`Temp/` 等生成目录已被 `.gitignore` 排除。

## 已验证

- Unity `6000.3.27f1` 编译无新增错误。
- 正式场景 Play Mode 启动并通过 Ready -> Fighting。
- 左右直拳分别通过原版动作链造成 `10` / `12` 点伤害。
- 正面格挡、背后不受格挡保护、Ellen 无敌闪烁和原版受击反应可用。
- Ellen 原版死亡动画及死亡状态、Chomper 攻击前冲和死亡流程已实测。
- HP 条、延迟条、低血量、命中/格挡反馈、暂停/恢复、重开和胜负面板已验证。
- 活动 Canvas、EventSystem、Camera、AudioListener 均只有一份。
- `1920×1080`、`1600×900`、`2560×1440` 和 `1680×1050` UI 布局已检查。
- Windows Standalone 构建成功。

详细测试以 [UI V2 测试报告](Docs/UI/UI_V2_TEST_REPORT.md) 和 [原版动作战斗测试报告](Docs/Combat/PHASE_2C_TEST_REPORT.md) 为准。

## 已知限制

- 当前没有有效主菜单场景，HUD 中的 `MAIN MENU` 按设计禁用。
- Editor 内 `EXIT` 禁用，避免误关闭 Unity Editor；独立播放器中才调用 `Application.Quit()`。
- 平局面板文案已验证，但尚未构造并自动化验证“双方严格同帧死亡”的完整实战路径。
- Windows Standalone 构建已通过，但最终 EXE 尚未完成整局人工按键回归。
- RGB-D / ZED 动捕尚未接入，仅保留统一动作调用边界。
- 为保持场景和原版动作安全，场景中仍保留旧示例资源、26 个 Chomper 实例和若干未启用旧 UI。
- 清理报告中的 `893` 个 REVIEW 资源和 `519` 个既有未解析 GUID token 仍保留。
- Console 保留一条 Cinemachine `CompositionModifier` 序列化警告，不影响当前编译和运行。

## 文档

- [交付完成清单与待优化项目](Docs/DELIVERY_AND_OPTIMIZATION.md)
- [Combat HUD V2 设计](Docs/UI/UI_V2_DESIGN.md)
- [Combat HUD V2 实施报告](Docs/UI/UI_V2_IMPLEMENTATION_REPORT.md)
- [Combat HUD V2 测试报告](Docs/UI/UI_V2_TEST_REPORT.md)
- [原版动作战斗架构](Docs/Combat/ORIGINAL_ACTION_COMBAT.md)
- [MatchController 设计](Docs/Combat/MATCH_CONTROLLER_DESIGN.md)
- [资源清理执行报告](Docs/Cleanup/CLEANUP_EXECUTION_REPORT.md)
- [资源保留/删除/隔离清单](Docs/Cleanup/ASSET_KEEP_REMOVE_QUARANTINE.md)

## 版本与授权说明

本项目版本标识为 `MotionFightPrototype v1.1`，当前 Unity 工程目录名可以保持为本地实际目录名，不影响产品/仓库名称。

仓库根目录当前没有独立 `LICENSE` 文件。Unity 3D Game Kit、Unity 包和其他第三方资源的授权仍遵循各自原始许可；对外发布、商用或再分发前必须单独核验相关许可条款。
