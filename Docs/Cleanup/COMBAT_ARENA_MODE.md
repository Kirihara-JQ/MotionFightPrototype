# CircularArea 1v1 战斗模式

阶段：2A
日期：2026-10-30
正式场景：`Assets/Scenes/Combat/Level1_CircularArena_Combat.unity`

## 1. 完成状态

阶段 2A 的 1v1 场景隔离已完成：

- 1 名可控制的 Ellen；
- 1 只处于活动状态、带 `EnemyController` 的 Chomper；
- 其余 25 个 Chomper、Grenadier 和 GrenadierActivator 在正式场景副本中停用；
- Ellen 和参战 Chomper 都出生在 `Environment[26]/Structures[2]/CircularArea[40]` 内；
- 旧关卡切换、检查点、死亡区、教程信息、旧菜单和旧 Timeline 根对象已停用；
- Build Settings 仍只包含正式战斗场景；
- 编译、Play Mode 和 Windows Standalone 构建均通过。

没有删除任何 Assets 或 `.meta`，没有修改原始 Level1、Chomper 源 Prefab、模型、材质或动画。

## 2. 运行时结构

新增场景根对象：

```text
__CombatOverlay[35]
├── ArenaReferences[0]
│   ├── PlayerSpawn[0]
│   ├── MonsterSpawn[1]
│   └── CircularAreaReference[2]
├── CombatParticipants[1]
│   ├── PlayerReference[0]
│   └── MonsterReference[1]
├── CombatSystems[2]
├── Debug[3]
└── ArenaModeController[4]
```

`ArenaModeController` 使用 `[DefaultExecutionOrder(-10000)]`，在所有旧玩法对象初始化前应用竞技场模式。

## 3. 参战对象

| 项目 | 实际对象 |
|---|---|
| 玩家 | `Ellen[17]` |
| 玩家控制器 | `Ellen[17]` 上的 `Gamekit3D.PlayerController` |
| 玩家输入 | `Ellen[17]` 上的 `PlayerInput` |
| 参战怪兽 | `Enemies[18]/Chomper[16]` |
| 怪兽行为 | 该对象上的 `Gamekit3D.ChomperBehavior`、`EnemyController`、`NavMeshAgent` |
| CircularArea | `Environment[26]/Structures[2]/CircularArea[40]` |
| 相机控制器 | `CameraRig[16]` 上的 `Gamekit3D.CameraSettings` |

### 3.1 出生点

| 对象 | 世界位置 | 朝向 |
|---|---|---|
| `__CombatOverlay/ArenaReferences/PlayerSpawn` | 约 `(-128.000, 21.097, -43.000)` | `+X`，面向参战 Chomper |
| `__CombatOverlay/ArenaReferences/MonsterSpawn` | 约 `(-112.000, 21.080, -43.000)` | `-X`，面向 Ellen |
| `CircularAreaReference` | CircularArea 原点 `(-120.000, 21.000, -43.000)` | 不参与旋转 |

两个出生点的 XZ 间距为 16 米。地面射线命中了 CircularArea 内的 `FloorCircular02_LOD00/LOD01` 碰撞体，Chomper 出生点也能采样到 NavMesh。

### 3.2 `ArenaModeController` 职责

当前脚本只负责场景契约：

- 停用 `nonParticipatingEnemies` 中的 27 个 GameObject；
- 保证唯一参战 Chomper 激活；
- 应用玩家和怪兽出生位置；
- 把键盘鼠标相机设为当前相机方案；
- 确保旧流程根对象停用；
- 在首帧后建立面向两名参战者的第三人称构图。

本阶段脚本不处理攻击、伤害、HP、胜负或回合状态。

## 4. 旧流程隔离

以下对象仅在正式战斗场景副本中设为 inactive：

| 完整路径 | 原功能 | 当前状态 |
|---|---|---|
| `TransitionStart[2]` | `TransitionPoint.newSceneName = Level2` | 已停用 |
| `SceneController[1]` | 旧场景切换与持久数据流程 | 已停用 |
| `Level01Gameplay[20]/Checkpoints[10]` | 5 个旧检查点及 CheckpointGroup | 已停用 |
| `DeathVolume[22]` | 旧关卡死亡区 | 已停用 |
| `MenuCanvases[11]` | 旧开始/暂停菜单 Canvas | 已停用 |
| `Level01InformationZones[13]` | 旧教程/信息区域 | 已停用 |
| `Level01Sequences[23]` | 3 组旧 Timeline 与序列相机 | 已停用 |
| `Enemies[18]/GrenadierActivator[26]` | 切换 Grenadier 激活状态 | 已停用 |
| `Enemies[18]/Grenadier[27]` | 旧 Grenadier | 保持 inactive |

未停用整个 `Environment`、`Lighting`、`Level01Gameplay` 或其他大根节点。

## 5. 自动验证

以下项目由 Unity Editor / AssetDatabase / Editor API 自动确认：

- 当前活动场景是 `Level1_CircularArena_Combat`；
- Build Settings 只有该场景；
- `__CombatOverlay` 存在且已保存；
- 管理器引用 Ellen、PlayerInput、Chomper、出生点、CircularArea 和 CameraSettings 均非空；
- 非参战敌人引用列表有 27 项；
- 旧流程停用列表有 7 项；
- `TransitionStart` 在运行时 inactive；
- 参战 Chomper 位于 NavMesh 上；
- 脚本编译错误为 0；
- Windows Standalone Build 返回 Success。

## 6. Play Mode 验证

实际运行结果：

- 活动 `PlayerController`：1；
- 活动 `EnemyController`：1；
- 活动 `ChomperBehavior`：1；
- 活动场景始终为 `Level1_CircularArena_Combat`；
- Ellen 起点约 `(-128.000, 21.064, -43.000)`；
- Chomper 起点约 `(-112.000, 21.080, -43.000)`；
- `TransitionStart`、`Level01Sequences`、检查点、旧菜单、教程信息和死亡区均未激活；
- `HealthCanvas` 仍激活，生命 HUD 可见；
- 相机为 `KeyboardAndMouseFreeLookRig`；
- Console 没有编译错误或运行时错误，仅有两项 Cinemachine SerializeReference 警告。

验证截图：`Logs/CleanupValidation/phase2a_1v1_final.png`。

## 7. 需要人工确认

- 从 Game 视图人工确认第三人称构图是否满足最终战斗镜头要求；
- 人工操作鼠标环视，确认灵敏度和遮挡行为；
- 进入更近的怪物攻击距离，确认旧 Chomper 预警和动作节奏适合后续改造；
- 确认旧 HUD 的临时样式是否满足演示需要；
- 人工完成一次长时间运行，确认没有其他旧关卡触发流程。

## 8. 尚未实现

- 左右直拳；
- 格挡；
- 新攻击动画；
- 新的伤害结算；
- 单一 HP 权威和胜负判断；
- 新暂停/重开/结果 UI；
- 正式竞技场边界与掉落恢复；
- 动捕 `AvatarState` 接口。

本阶段没有用旧攻击代码冒充新战斗系统。

## 9. 修改文件与回退

本阶段计划提交的源文件：

- `Assets/Scenes/Combat/Level1_CircularArena_Combat.unity`
- `Assets/_Project/Scripts/Arena/ArenaModeController.cs`
- `Assets/_Project/Scripts/Arena/ArenaModeController.cs.meta`
- `Assets/_Project.meta`
- `Assets/_Project/Scripts.meta`
- `Assets/_Project/Scripts/Arena.meta`

构建过程曾自动移除 `Dust_Particle_01_Mat.mat` 的 `_FADING_ON` keyword；该自动变化已恢复为 Git 基线内容，最终不修改任何材质源文件。

回退基线：`bdbfadfd1edff2356d8eb430c25cfda598b0e40a`。

完成阶段 2A 提交后，优先使用：

```powershell
git revert <phase2a_commit>
```

这会生成一个新提交并恢复场景与管理器文件，不会删除仓库历史。
