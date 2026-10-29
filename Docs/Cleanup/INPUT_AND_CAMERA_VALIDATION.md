# 输入与相机验证

阶段：2A
日期：2026-10-30

## 1. 当前输入方案

工程保持 Unity `6000.3.27f1`，`activeInputHandler = 2`，同时启用旧 Input Manager 与 Input System。

当前实际控制链路：

```text
Unity Input Manager
  ├── Horizontal / Vertical -> PlayerInput -> PlayerController -> Ellen CharacterController
  ├── Jump -> PlayerInput -> PlayerController
  ├── Fire1 -> PlayerInput.Attack -> 旧法杖近战
  ├── Pause -> 旧 StartUI
  └── CameraX / CameraY -> CinemachineInputAxisController -> KeyboardAndMouseFreeLookRig
```

本阶段没有切换或重配输入后端。`com.unity.inputsystem 1.20.1` 保持安装，但新战斗动作尚未接入 Input Actions。

## 2. 当前实际映射

| 输入 | 当前行为 | 状态 |
|---|---|---|
| `W/A/S/D` 或方向键 | `Horizontal/Vertical` 移动 Ellen | 已验证 |
| 鼠标水平/垂直移动 | `CameraX/CameraY` 控制 Cinemachine FreeLook | 已自动注入移动验证 |
| `Space` | `Jump`，当前仍为跳跃 | 保留 |
| 鼠标左键 | `Fire1`，触发旧法杖攻击 | 保留，不是直拳 |
| 鼠标右键 | `Fire2` 已配置，但旧 `PlayerInput` 未消费 | 未绑定战斗动作 |
| `Esc` | `Pause`，旧 MenuCanvas 已在战斗模式停用 | 当前不打开旧菜单 |

## 3. 后续战斗输入约定

阶段 2A 统一确定以下目标映射；本阶段只记录，不实现动作和伤害：

| 战斗动作 | 建议输入 | 实施边界 |
|---|---|---|
| 左直拳 | 鼠标左键 | 新 `CombatInputAdapter` 发出攻击开始事件 |
| 右直拳 | 鼠标右键 | 与左拳使用独立攻击 ID |
| 格挡 | 按住 `Space` | 接入前必须处理它当前兼作跳跃的问题 |
| 移动 | `W/A/S/D` | 继续沿相机平面移动 |
| 相机环视 | 鼠标 | Cinemachine 保持独立于真实世界位移 |

不建议在本阶段直接把 `Space` 改成格挡，因为现有 `PlayerController` 仍把 Space 用作跳跃。新输入适配器应和移动控制器替换一起接入，避免一次输入同时触发跳跃和格挡。

## 4. 相机结构

实际层级：

```text
CameraRig[16]
├── CameraBrain[0]
│   ├── Camera
│   ├── AudioListener
│   ├── CinemachineBrain
│   └── URP Camera Data
├── KeyboardAndMouseFreeLookRig[1]
└── ControllerFreeLookRig[2]
```

场景中：

- Unity Camera 数量：1；
- AudioListener 数量：1；
- `CameraSettings.inputChoice`：`KeyboardAndMouse`；
- Play Mode 活动虚拟相机：`KeyboardAndMouseFreeLookRig`；
- 备用 `ControllerFreeLookRig`：priority 0，不参与当前输出；
- 旧 `Level01Sequences` 根对象已停用，因此 3 个旧过场 vcam 不参与当前镜头。

`CameraRig` 的 `Gamekit3D.CameraSettings` 会自动把：

- `follow` 指向 `Ellen[17]`；
- `lookAt` 指向 `Ellen[17]/HeadTarget[0]`；
- 键盘鼠标相机 priority 设为 1；
- 手柄相机 priority 设为 0。

## 5. 战斗初始构图

`ArenaModeController` 在首帧后将键盘鼠标相机的初始位置设置为 Ellen 身后约 5 米、向上约 2.7 米，并朝玩家与 Chomper 的中间上部取景。

测试结果：

- 相机成功从 Ellen 身后看向 CircularArea；
- Ellen 位于画面下方；
- Chomper 位于画面中心远处；
- 生命 HUD 正常显示。

截图：`Logs/CleanupValidation/phase2a_1v1_final.png`。

## 6. 已自动验证

- 项目使用现有 Input Manager + Input System 双后端配置；
- `PlayerInput` 读取 `Horizontal`、`Vertical`、`Jump`、`Fire1` 和 `Pause`；
- Cinemachine 读取 `CameraX`、`CameraY`；
- Camera 数量为 1；
- AudioListener 数量为 1；
- 键盘鼠标虚拟相机在 Play Mode 中处于 live 状态；
- 手柄备用相机 priority 为 0；
- 旧 Timeline 相机根对象 inactive。

## 7. 已通过 Play Mode 验证

### 7.1 键盘移动

向 Game 窗口发送持续 0.8 秒的 `W`：

- Ellen 从约 `(-128.000, 21.064, -43.000)` 移动到 `(-122.407, 20.980, -43.000)`；
- 同时 Chomper 从约 `(-112.000, 21.080, -43.000)` 接近到 `(-120.449, 21.050, -43.770)`；
- 场景没有切换；
- 目标 HUD 仍可见。

### 7.2 鼠标相机

向 Game 视图注入一次水平鼠标移动：

- 相机画面前向从约 `(0.993, -0.120, 0.000)` 变为 `(0.991, -0.120, -0.054)`；
- Yaw 从约 `90°` 变为 `93.1°`；
- 活动相机仍为 `KeyboardAndMouseFreeLookRig`。

## 8. 需要人工确认

- 左右直拳的具体按键触发在当前系统中尚未实现；
- 鼠标灵敏度和 Game 视图窗口焦点行为需要人工体验；
- 进入近战后镜头遮挡、目标保持和舒适距离需要人工调整；
- 当前 HUD 是旧生命 HUD，仅用于本阶段保底显示。

## 9. 尚未实现

- Input Actions 战斗映射；
- 左右拳事件；
- 按住格挡；
- 新的攻击/受伤动画层；
- 目标锁定；
- 鼠标与动捕输入的切换接口；
- 新 HUD、暂停和结果 UI。
