# 敌人实例审计

阶段：2A
日期：2026-10-30
场景：`Assets/Scenes/Combat/Level1_CircularArena_Combat.unity`

## 1. 审计结论

场景中的 26 个 Chomper 都是 `Enemies[18]` 下的独立场景 Prefab 实例，来源均为：

`Assets/3DGamekit/Prefabs/Characters/Enemies/Chomper/Chomper.prefab`

没有发现会复制、实例化或补生成 Chomper/Grenadier 的敌人 Spawner、敌人生成器或 Addressables 流程。现有 `ObjectPooler` 主要用于投射物，`EllenSpawn` 只负责玩家重生特效，不是敌人来源。

正式场景副本现在只激活：

`Enemies[18]/Chomper[16]`

它是审计时距离 CircularArea 最近的 Chomper，已移动到 CircularArea 内。

## 2. Chomper 实例

下表位置为阶段 2A 修改前的世界坐标。所有对象的直接父节点都是 `Enemies[18]`。

| 兄弟索引 | 完整路径 | 修改前位置 | 当前状态 |
|---:|---|---|---|
| 0 | `Enemies[18]/Chomper[0]` | `(-64.783, 19.540, -45.202)` | 停用 |
| 1 | `Enemies[18]/Chomper[1]` | `(-54.106, 19.560, -25.842)` | 停用 |
| 2 | `Enemies[18]/Chomper[2]` | `(3.048, 7.966, 4.327)` | 停用 |
| 3 | `Enemies[18]/Chomper[3]` | `(7.680, 7.540, 8.040)` | 停用 |
| 4 | `Enemies[18]/Chomper[4]` | `(3.750, 7.540, -4.790)` | 停用 |
| 5 | `Enemies[18]/Chomper[5]` | `(7.828, 10.380, -15.177)` | 停用 |
| 6 | `Enemies[18]/Chomper[6]` | `(2.817, 9.700, -9.745)` | 停用 |
| 7 | `Enemies[18]/Chomper[7]` | `(-11.750, 12.480, -5.650)` | 停用 |
| 8 | `Enemies[18]/Chomper[8]` | `(-14.540, 12.480, -8.660)` | 停用 |
| 9 | `Enemies[18]/Chomper[9]` | `(-10.980, 12.480, -8.710)` | 停用 |
| 10 | `Enemies[18]/Chomper[10]` | `(-35.850, 12.360, 19.450)` | 停用 |
| 11 | `Enemies[18]/Chomper[11]` | `(-34.760, 12.850, 13.800)` | 停用 |
| 12 | `Enemies[18]/Chomper[12]` | `(-23.000, 12.530, 15.330)` | 停用 |
| 13 | `Enemies[18]/Chomper[13]` | `(-53.756, 20.010, -34.617)` | 停用 |
| 14 | `Enemies[18]/Chomper[14]` | `(-63.751, 19.680, -36.483)` | 停用 |
| 15 | `Enemies[18]/Chomper[15]` | `(-125.422, 28.000, -74.700)` | 停用 |
| **16** | **`Enemies[18]/Chomper[16]`** | 原为 `(-110.385, 28.000, -69.514)` | **唯一参战；现为 `(-112.000, 21.080, -43.000)`** |
| 17 | `Enemies[18]/Chomper[17]` | `(-153.610, 22.077, -42.310)` | 停用 |
| 18 | `Enemies[18]/Chomper[18]` | `(-159.850, 22.819, -38.850)` | 停用 |
| 19 | `Enemies[18]/Chomper[19]` | `(-148.890, 21.680, -47.610)` | 停用 |
| 20 | `Enemies[18]/Chomper[20]` | `(-158.592, 22.032, -48.973)` | 停用 |
| 21 | `Enemies[18]/Chomper[21]` | `(-72.592, 29.000, -10.303)` | 停用 |
| 22 | `Enemies[18]/Chomper[22]` | `(-74.770, 29.000, -3.780)` | 停用 |
| 23 | `Enemies[18]/Chomper[23]` | `(-100.408, 31.000, -5.861)` | 停用 |
| 24 | `Enemies[18]/Chomper[24]` | `(-100.188, 31.000, -12.919)` | 停用 |
| 25 | `Enemies[18]/Chomper[25]` | `(8.144, 8.004, 2.108)` | 停用 |

唯一的 `Chomper[16]` 保留以下场景组件：

- `Animator`
- `ChomperBehavior`
- `Damageable`
- `ReplaceWithRagdoll`
- `BoxCollider`
- `EnemyController`
- `NavMeshAgent`

未修改 `Chomper.prefab` 源文件、Chomper FBX、材质、Animator Controller 或 AnimationClip。

## 3. Grenadier

| 路径 | 审计结果 | 当前状态 |
|---|---|---|
| `Enemies[18]/Grenadier[27]` | `GrenadierBehaviour`、`EnemyController`、`NavMeshAgent`；原场景即为 inactive | 保持停用 |
| `Enemies[18]/GrenadierActivator[26]` | `GameCommandReceiver` + `ToggleGameObjectActive`，其 `targets[0]` 指向 `Grenadier[27]` | 已停用，避免运行时重新激活 Grenadier |

## 4. 其他敌人或装饰生物

`Effects[27]/EelDogs[42]` 下的 EelDog 是沿 DollyCart 运动的装饰动画对象，没有 `EnemyController` 或 `ChomperBehaviour`，不参与本局战斗判定。因此保留其外观和动画，没有把它算作第二名参战敌人。

没有在正式场景中发现活动的 Spitter 或 Gunner 敌人实例。

## 5. 生成来源

检查结果：

- 26 个 Chomper 是场景内独立的 PrefabInstance；
- 每个实例最近源 Prefab 都是同一个 `Chomper.prefab`；
- `GrenadierActivator` 只切换现有 Grenadier 对象开关；
- 代码中没有 Chomper/Grenadier EnemySpawner；
- 没有发现 Addressables 敌人加载；
- 没有发现 Resources 动态加载敌人；
- `ObjectPooler` 仅用于武器投射物；
- `EllenSpawn` 只负责 Ellen 重生视觉；
- Play Mode 启动后活动 `EnemyController` 数量保持为 1。

## 6. 自动验证

- 场景保存前：`Enemies` 下 1 个根对象 active、27 个 inactive；
- Play Mode：活动 `ChomperBehavior` 为 1；
- Play Mode：活动 `EnemyController` 为 1；
- `ArenaModeController.activeMonster` 指向 `Chomper[16]`；
- `ArenaModeController.nonParticipatingEnemies` 包含 27 个对象；
- 参战 Chomper 的 `NavMeshAgent.isOnNavMesh = true`；
- `TransitionStart`、Checkpoint、DeathVolume、旧信息区和旧 Timeline 均 inactive；
- 构建成功。

## 7. 需要人工确认

- 人工观察 Chomper 从 16 米外向 Ellen 接近的节奏；
- 确认只有一只怪物进入攻击和旧伤害结算；
- 确认停用其他敌人后，场景视觉上没有明显空洞或缺失必要的氛围；
- 后续替换新 AI 时，确认旧 `ChomperBehavior`、`EnemyController` 和旧 `Damageable` 是否按设计逐个停用。

## 8. 尚未实现

- 正式的单怪兽 AI 状态机；
- 可读的攻击预警；
- 新攻击窗口与伤害；
- 怪兽死亡和回合结束；
- 非参战敌人“仅隐藏外观”的最终美术策略；
- 对 893 个 REVIEW 资源的后续清理。

## 9. 回退

本阶段整体回退基线是提交：

`bdbfadfd1edff2356d8eb430c25cfda598b0e40a`

推荐通过 `git revert <phase2a_commit>` 回退场景和新增脚本；不要手工批量重新启用敌人或删除 `.meta`。
