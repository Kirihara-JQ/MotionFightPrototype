# Combat HUD V2.0 设计说明

## 目标

Combat HUD V2.0 将原有调试型血量界面升级为 `Premium Dark Fantasy × Futuristic Combat` 风格，同时保持 CircularArea 场景为主要视觉主体。

设计约束：

- Unity `6000.3.27f1`，保持 URP。
- 参考分辨率 `1920×1080`，Canvas Scaler 使用 `Scale With Screen Size`，匹配值 `0.5`。
- HUD 只负责表现和交互转发，不拥有 HP、伤害、胜负、暂停或重开状态。
- 复用现有 `MatchController`、`CombatResolver`、`CombatantHealth`，不创建第二套战斗逻辑。
- 不覆盖原始 Level1、第三方 Prefab、Animator Controller、URP 或 Packages 配置。

## 视觉系统

| 用途 | 颜色 |
| --- | --- |
| 玩家青蓝 | `#24C8F4` |
| 玩家深色背景 | `#071B2B` |
| 玩家浅色装饰 | `#81DDF5` |
| 怪兽猩红 | `#ED4963` |
| 怪兽深色背景 | `#240E19` |
| 怪兽浅色装饰 | `#FF9AA8` |
| 胜利金色 | `#FFC46C` |
| 主要文字 | `#F4F7FB` |
| 次要文字 | `#8FA6BA` |

界面由半透明深色面板、细描边、斜切短线、菱形节点和少量发光组成。没有使用整张参考图作为背景，也没有添加全屏高频闪光。

## 场景层级

```text
Canvas_CombatHUD
└── HUD_Root
    ├── TopHUD
    │   ├── PlayerPanel
    │   │   ├── PlayerPortrait
    │   │   ├── PlayerName
    │   │   ├── PlayerRole
    │   │   └── PlayerHPBar
    │   ├── MatchStatus
    │   └── MonsterPanel
    │       ├── MonsterPortrait
    │       ├── MonsterName
    │       ├── MonsterRole
    │       └── MonsterHPBar
    ├── ResultPanel
    ├── PausePanel
    ├── CombatFeedback
    └── BottomControlBar
```

场景中只有 `Canvas_CombatHUD` 为活动 Canvas，只有根级 `EventSystem` 为活动事件系统。旧 `HealthUI` 已禁用，旧 Canvas 对象保留为未激活对象，避免破坏第三方场景引用。

## 顶部角色面板

- 玩家面板位于左上角，怪兽面板位于右上角，`1920×1080` 下均为约 `590×124`。
- 面板包含真实 Ellen 与 Chomper 头像、角色名、职责、HP 数字和百分比。
- HP 使用 `Image.Type.Filled` 与 `Horizontal` 填充。
- 玩家血条从左向右，怪兽血条从右向左。
- HP 文本位于血条内部并固定数字宽度为三位，避免数值变化造成横向抖动。
- 低血量阈值为 `25%`，触发血条光晕和低血量提示。

## 血条表现

- 主条过渡时间：`0.25s`。
- 延迟条等待：`0.35s`。
- 延迟条下降：`0.45s`。
- 受击闪白：`0.18s`。
- UI 动画全部使用 `Time.unscaledDeltaTime`，暂停时仍可播放。
- `CombatantHealth` 是唯一 HP 数据源，`CombatantHealthBarView` 只订阅其 `HealthChanged` 事件。

## 对局状态与结果

顶部中央状态由 `MatchController.State` 驱动：

- `Ready`：`STANDBY / PREPARE FOR COMBAT`
- `Fighting`：`FIGHT / DEFEAT THE MONSTER`
- `Paused`：`PAUSED / COMBAT SUSPENDED`
- `PlayerWin`：`MATCH COMPLETE / MONSTER DEFEATED`
- `MonsterWin`：`MATCH COMPLETE / PLAYER DEFEATED`
- `Draw`：`MATCH COMPLETE / BOTH COMBATANTS HAVE FALLEN`

结果面板默认隐藏。对局结束后通过缩放和淡入出现：

- 玩家胜利：`PLAYER WINS`
- 怪兽胜利：`MONSTER WINS`
- 平局：`DRAW`

`RESTART` 调用现有 `MatchController.RestartMatch()`。当前构建列表没有有效主菜单场景，因此 `MAIN MENU` 使用禁用状态，不伪造场景跳转。

## 暂停与操作提示

- `Esc` 通过 `MatchController.TogglePause()` 进入或退出暂停。
- `RESUME` 调用 `MatchController.ResumeMatch()`。
- `RESTART` 调用正式战斗场景重开流程。
- Editor 内 `EXIT` 禁用；独立播放器内调用 `Application.Quit()`。
- 底部操作栏显示当前实际键鼠绑定：`WASD / MOUSE / LMB / RMB / L-SHIFT / ESC`。
- `ControlHintView` 提供 `KeyboardMouse` 与 `MotionControl` 两种显示模式，为后续 RGB-D 输入保留表现层切换入口。

## 响应式布局

主要面板采用四角锚点，顶部状态使用顶部中央锚点，结果与暂停卡使用屏幕中心锚点，控制栏使用底部横向拉伸锚点。

已检查：

- `1920×1080`
- `1600×900`
- `2560×1440`
- `1680×1050`（16:10）

检查中修正了 HP 数字超出面板底边的问题。当前版本四组尺寸均无左右面板出界、顶部状态遮挡、结果面板偏移或底部提示栏出界。

## 真实资源

- 字体：`Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset`
- Ellen 头像：`Assets/3DGamekit/Art/Sprites/AnimatedSprites/Ellen/CutsceneAnimation/Ellen_CutscenePortrait_000.png`
- Chomper 头像：`Assets/3DGamekit/Art/Sprites/UI/Menu/LoadingChomper.png`

面板、血条、键帽、光晕和菱形装饰为项目内生成的轻量 PNG 精灵，没有导入来源不明字体、插件或外部角色图片。
