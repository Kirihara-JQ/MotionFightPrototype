# Combat HUD V2.0 测试报告

测试环境：

- Unity `6000.3.27f1`
- URP
- Windows Standalone x64 构建目标
- 正式场景：`Assets/Scenes/Combat/Level1_CircularArena_Combat.unity`

## 已验证

| 项目 | 结果 | 证据 |
| --- | --- | --- |
| UI 脚本编译 | 通过 | Unity Console 无编译错误 |
| Prefab 与场景绑定 | 通过 | `HUD_Root` 为连接 Prefab，`MatchHUD` 引用完整 |
| 玩家 HP 读取 | 通过 | 初始 `100 / 100`，战斗过程中实时变化 |
| 怪兽 HP 读取 | 通过 | 初始 `100 / 100`，伤害后 `070 / 100`、`20%` |
| HP 主条动画 | 通过 | 结算后 `fillAmount` 从 `1.0` 平滑变为目标值 |
| HP 延迟条 | 通过 | 伤害调用当帧主/延迟条仍为 `1.0`，延迟结束后同步为 `0.7` |
| 玩家血条方向 | 通过 | `Image.Type.Filled`、`Horizontal`、origin `0` |
| 怪兽血条方向 | 通过 | `Image.Type.Filled`、`Horizontal`、origin `1` |
| HP 文本不溢出 | 通过 | 实机截图复核，四位内容恒定宽度显示 |
| 低血量提示 | 通过 | 玩家 12 HP 时警告 CanvasGroup alpha 为约 `0.765` |
| 玩家胜利 | 通过 | 通过 `CombatResolver` 结算怪物 100 伤害，显示 `PLAYER WINS / YOU DEFEATED THE CHOMPER` |
| 怪兽胜利 | 通过 | 实际 Play Mode 战斗中玩家死亡，显示 `MONSTER WINS / YOU HAVE BEEN DEFEATED` |
| 平局面板文案 | 已显示验证 | `MatchResultView.Show(Draw)` 显示 `DRAW / BOTH COMBATANTS HAVE FALLEN` |
| 结果面板默认隐藏 | 通过 | 战斗开始后 `ResultPanel.activeInHierarchy == false` |
| 暂停面板默认隐藏 | 通过 | 进入 Fighting 后保持隐藏 |
| Pause / Resume | 通过 | `MatchController.PauseMatch()` 后暂停面板可见，真实 `ResumeButton.onClick` 恢复到 Fighting |
| Restart | 通过 | 结果面板真实 Restart 按钮触发场景重载，HP 恢复到 100/100，UI 关闭 |
| 格挡反馈 | 通过 | 正面格挡实际结算 `20 → 2` 伤害，反馈文本更新为 `GUARD` |
| 玩家受击反馈 | 通过 | 去往玩家的伤害触发 `DAMAGE` 反馈 |
| 怪兽受击反馈 | 通过 | 去往怪兽的伤害触发 `HIT` 反馈并闪烁 HP 条 |
| 单 Canvas | 通过 | Play Mode 中活动 Canvas 数量为 `1` |
| 单 EventSystem | 通过 | Play Mode 中活动 EventSystem 数量为 `1` |
| 单 Camera / AudioListener | 通过 | Play Mode 中活动 Camera 数量为 `1`，AudioListener 数量为 `1` |
| Main Menu 状态 | 按设计禁用 | 当前构建列表只有正式战斗场景 |
| Editor Exit 状态 | 按设计禁用 | 不关闭 Unity Editor |
| 1920×1080 | 通过 | 实机截图复核 |
| 1600×900 | 通过 | 实机截图复核 |
| 2560×1440 | 通过 | 实机截图复核 |
| 1680×1050（16:10） | 通过 | 实机截图复核 |
| Windows Standalone 构建 | 通过 | `Build Finished, Result: Success.`，输出 `Builds/Validation/MotionFightPrototype.exe` |
| Console 编译错误 | 无 | 只有既有 Cinemachine `CompositionModifier` 序列化警告 |

截图位于本地忽略目录：

```text
Logs/UIValidation/combat_hud_v2_fighting_1920.png
Logs/UIValidation/combat_hud_v2_1600x900.png
Logs/UIValidation/combat_hud_v2_2560x1440.png
Logs/UIValidation/combat_hud_v2_16x10.png
```

## 未单独测试

| 项目 | 状态 | 说明 |
| --- | --- | --- |
| 双人同帧死亡的实际 MatchController 结算 | 未单独执行 | Draw 面板文案已通过视图方法验证；同帧死亡规则由 `MatchController.DetermineOutcome` 保持，未在本轮 UI 测试中构造双方同帧死亡 |
| 独立 Windows 构建启动和交互 | 未执行 | 构建已成功；未启动生成的播放器进行二次人工验收 |
| 鼠标左键、右键、Shift 的完整人工操作回归 | 本 UI 轮未重复自动化 | 战斗输入脚本未改动；需人工按键确认相机、出拳和格挡手感 |
| 真实主菜单跳转 | 不适用 | 当前没有有效主菜单场景 |
| RGB-D / ZED 输入 | 未执行 | 本阶段明确不接入 |

## Console 状态

Play Mode 和构建完成后未发现新的编译错误、Missing Script 或 Missing Reference。

唯一保留警告：

```text
The type Unity.Cinemachine CompositionModifier is being serialized by [SerializeReference],
but is missing the [Serializable] attribute.
```

该警告来自 Cinemachine，不是 Combat HUD V2 新增代码。
