# Combat HUD V2.0 实施报告

## 完成范围

本轮已经完成 UI 设计、UGUI 层级、Prefab 生成、场景接入、数据绑定、按钮连接、UI 动效和 Play Mode 验证。工作保留了当前原版动作战斗架构，没有重写 `CombatResolver`、`CombatantHealth`、怪物 AI 或 `MatchController` 的战斗规则。

## 新增或修改的运行时脚本

| 路径 | 职责 |
| --- | --- |
| `Assets/_Project/Scripts/Combat/MatchHUD.cs` | HUD 总协调器、事件订阅、按钮转发、反馈表现 |
| `Assets/_Project/Scripts/UI/CombatantHealthBarView.cs` | 单个角色的 HP、延迟条、闪光、低血量光晕 |
| `Assets/_Project/Scripts/UI/MatchStatusView.cs` | 顶部对局状态文本 |
| `Assets/_Project/Scripts/UI/MatchResultView.cs` | 玩家胜、怪兽胜和平局文案 |
| `Assets/_Project/Scripts/UI/PauseMenuView.cs` | 暂停面板和按钮状态 |
| `Assets/_Project/Scripts/UI/ControlHintView.cs` | 键鼠/动捕操作提示切换 |
| `Assets/_Project/Scripts/UI/HUDPanelAnimator.cs` | 使用 unscaled time 的面板淡入、缩放和淡出 |
| `Assets/_Project/Scripts/UI/HUDButtonMotion.cs` | 按钮 Hover 和 Pressed 视觉反馈 |

`MatchHUD` 在 `OnEnable` 订阅、`OnDisable` 取消订阅，不通过逐帧场景扫描刷新 HP。

## Editor 生成工具

工具路径：

```text
Assets/_Project/Editor/UI/CombatHUDV2Builder.cs
```

菜单：

```text
Tools/Motion Fight/UI/Rebuild Combat HUD V2
```

该工具可重复执行，负责：

- 生成或更新项目内 UI 精灵。
- 创建并保存 HUD Prefab。
- 复用或重命名正式场景 Canvas。
- 清理重复的生成型 HUD 根节点。
- 确保只有一个有效 `EventSystem`。
- 禁用旧 `HealthUI`。
- 写入 `MatchHUD` 的 Prefab/场景引用。

最后一次重复执行验证结果：场景中只有一个 `HUD_Root`、一个活动 Canvas、一个活动 EventSystem、一个活动 Camera 和一个活动 AudioListener。

## 生成的资源

Prefab：

```text
Assets/_Project/Prefabs/UI/PF_CombatHUD_V2.prefab
```

生成精灵：

```text
Assets/_Project/Art/UI/CombatHUDV2/HUD_Panel_Rounded.png
Assets/_Project/Art/UI/CombatHUDV2/HUD_Keycap.png
Assets/_Project/Art/UI/CombatHUDV2/HUD_BarFill.png
Assets/_Project/Art/UI/CombatHUDV2/HUD_Glow.png
Assets/_Project/Art/UI/CombatHUDV2/HUD_Diamond.png
```

## 场景接入

正式场景：

```text
Assets/Scenes/Combat/Level1_CircularArena_Combat.unity
```

实际场景对象：

```text
Canvas_CombatHUD/HUD_Root
```

`HUD_Root` 是 `PF_CombatHUD_V2.prefab` 的连接 Prefab 实例。原 `HealthCanvas` 已重命名为 `Canvas_CombatHUD`，旧 `HealthUI` 被禁用。

Prefab 包含：

- `TopHUD/PlayerPanel`
- `TopHUD/MatchStatus`
- `TopHUD/MonsterPanel`
- `ResultPanel`
- `PausePanel`
- `CombatFeedback`
- `BottomControlBar`

## 真实数据绑定

- `MatchHUD.matchController` 指向场景中的唯一 `MatchController`。
- 玩家 HP 条绑定 Ellen 的 `CombatantHealth`。
- 怪兽 HP 条绑定 Chomper 的 `CombatantHealth`。
- 顶部状态、结果和暂停表现订阅 `MatchController.StateChanged`。
- HP 数字和血条订阅 `CombatantHealth.HealthChanged`。
- 格挡/命中反馈订阅 `CombatResolver.HitResolved`。
- `RESTART`、`RESUME`、`EXIT`、`MAIN MENU` 都是真实 `UnityEngine.UI.Button`。

## 动效

- HP 主条平滑变化。
- HP 延迟条等待后下降。
- 受击血条闪光。
- 低血量光晕脉冲和警告文字。
- `HIT` / `GUARD` 短暂反馈。
- 结果与暂停面板淡入、轻微缩放。
- 结果/暂停面板关闭时淡出。
- Restart 点击后 HUD 淡出，再执行正式场景重载。
- 按钮 Hover 发光、Pressed 缩放。

全部 UI 面板动画使用 unscaled time，暂停不会被 `Time.timeScale` 冻结。

## 本轮修正

实机截图发现 HP 数字原先位于血条下方并超出面板边界。已将 `HP_Value` 与 `HP_Percent` 锚定到血条内部，并重新生成 Prefab 与场景实例。修复后数字不再溢出面板。

## 未实现或保留项

- 当前没有有效主菜单场景入口，`MAIN MENU` 保持禁用，没有创建虚假跳转。
- Editor 内 `EXIT` 禁用；独立播放器构建中启用。
- 没有制作 RenderTexture 头像；使用项目内真实 Ellen/Chomper 精灵，避免额外头像相机和 Draw Call。
- 没有修改或接入 ZED SDK、RGB-D 动捕模块。

## 回退方法

本轮改动尚未提交时，可整体回退以下路径：

```text
Assets/_Project/Art/UI/
Assets/_Project/Prefabs/UI/PF_CombatHUD_V2.prefab
Assets/_Project/Editor/UI/
Assets/_Project/Scripts/UI/
Assets/_Project/Scripts/Combat/MatchHUD.cs
Assets/Scenes/Combat/Level1_CircularArena_Combat.unity
Docs/UI/
```

提交后可用：

```text
git revert <UI_V2_COMMIT_HASH>
```

回退前应确认该提交只包含 Combat HUD V2 和本阶段必要的战斗文档；不要回退原版动作模块、伤害桥接或正式战斗场景隔离提交。
