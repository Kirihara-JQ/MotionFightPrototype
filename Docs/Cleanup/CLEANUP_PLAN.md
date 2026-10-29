# CircularArea 1v1 项目瘦身计划

审计日期：2026-10-30  
当前阶段：第一阶段完成，等待确认  
执行状态：未删除、未移动、未修改任何 Unity 资源或 `.meta`

## 1. 当前判断

目标场景已经存在，因此不需要重新创建副本。但它目前仍是完整 Level1 场景，不是可发布的最小 1v1 工程。

本阶段给出三类结果：

| 分类 | 文件数 | 大小 MB | 下一步 |
|---|---:|---:|---|
| KEEP | 1,751 | 1,743.45 | 保持，不在第一批删除中处理 |
| REMOVE 候选 | 145 | 5.789 | 在 Editor 导入、编译与构建验证后优先清理 |
| REVIEW | 893 | 873.40 | 按依赖族分批复核，不能直接批量删除 |

## 2. 先决条件

在进入任何删除操作前，必须满足：

1. 用户明确批准第二阶段。
2. 为工程建立可恢复备份或版本控制快照。当前目录不是 Git 仓库。
3. 用 Unity `6000.3.27f1` 打开本工程，而不是当前已连接的 `MotionFightPrototype`。
4. 目标战斗场景可以独立打开，Console 没有编译错误。
5. 目标场景中的 Ellen、CircularArea、Chomper、材质、动画、光照与音频显示正常。
6. 确认 Build Settings 将由旧三场景入口迁移到目标战斗场景。

## 3. 分阶段执行计划

### 阶段 2A：建立可恢复基线

- 外部完整备份或版本控制快照。
- 打开目标场景，保存并检查 Missing Script、Missing Prefab、粉色材质。
- 记录 Unity `AssetDatabase.GetDependencies(targetScene, true)` 的实际结果。
- 不删除资源。
- 将目标战斗场景加入 Build Settings 的计划与战斗入口设计对齐；修改前再次确认。

通过条件：目标场景打开无阻断错误，构建配置方案明确。

### 阶段 2B：先实现并验证战斗机

在删除旧玩法前先建立正式替代物：

- `__CombatOverlay`
- 键鼠 `CombatInputAdapter`
- 玩家移动、左右直拳、格挡
- 唯一 `CombatResolver`
- 唯一 HP/比赛状态权威 `MatchController`
- 一只 Chomper 的新 AI
- HUD、暂停/恢复、结果与重开
- 面向未来 RGB-D `AvatarState` 的输入适配接口

旧 `PlayerController`、`EnemyController`、`Damageable` 和新系统不得并行写入同一角色的 Transform、Animator、HP 或攻击状态。

通过条件：1v1 完整流程可玩；暂停、重开与结果稳定；无重复扣血。

### 阶段 2C：精确禁用旧玩法

只在目标场景副本中处理：

- 禁用 Ellen 的旧移动/输入/近战控制；
- 禁用非参战敌人的旧 AI、旧伤害与自动攻击；
- 用战斗重开替代 Checkpoint/CheckpointGroup；
- 移除或替换目标场景中 `newSceneName = Level2` 的 TransitionPoint；
- 用新 HUD 替换旧 `HealthUI`/`DialogueCanvas`；
- 评估并处理 DeathVolume、陷阱、Timeline、机关、旧场景管理等对象。

通过条件：目标场景仍是单场景可运行；没有旧对象改动比赛 HP 或场景状态。

### 阶段 2D：切换 Build 入口

在 2B/2C 验证通过后：

1. 将 `Level1_CircularArena_Combat.unity` 设为正式启动场景。
2. 移除 Start、Level1、Level2 的旧 Build 条目。
3. 验证 `TransitionStart.prefab` 中 `newSceneName = Start` 不再可达。
4. 验证 Loading 流程是否已由新战斗 UI 替代。

通过条件：Windows Standalone 构建可以直接进入战斗场景。

### 阶段 2E：删除 145 个低风险示例资源

优先处理：

| 路径 | 文件数 | 大小 MB |
|---|---:|---:|
| `Assets/TextMesh Pro/Examples & Extras/` | 136 | 5.53 |
| `Assets/TutorialInfo/` | 6 | 0.24 |
| `Assets/Readme.asset` | 1 | 0.00 |
| `Assets/3DGamekit/Readme.asset` | 1 | 0.00 |
| `Assets/Scenes/SampleScene.unity` | 1 | 0.01 |

通过 Unity Editor 删除；先保存字体/许可证摘要。删除后立刻验证 TMP HUD、目标场景打开和 Standalone 构建。

### 阶段 2F：处理旧场景图

在阶段 2D 完成后，重新计算依赖闭包，然后处理：

- Level2 的唯一依赖约 206 个、212.48 MB；
- Start 的唯一依赖约 32 个、25.05 MB；
- Loading 的唯一依赖约 7 个；
- `_TemplateScene` 的唯一依赖约 6 个、6.31 MB；
- `Assets/_Recovery/` 两场景，约 38.51 MB。

这些数字按“剔除目标场景共享依赖”计算，不能在不同批次直接相加。每批删除后重新计算。

### 阶段 2G：处理大型 REVIEW 资产

按风险从低到高成组验证：

1. 复制/备份光照数据中未使用的 Level1/Level2 烘焙集；
2. 未引用的 Level2 NavMeshData；
3. 不再使用的 Dropship、Gunner、Spitter、过渡关卡美术与音频；
4. 旧 Tool、Editor、Timeline、Interactive 嵌入包；
5. 旧 UI 菜单、Fade、Loading、Dialogue Prefab；
6. 最后才审查 Shader、Shader Graph、FBX、TMP Shader 和脚本程序集。

每批必须逐个恢复验证：

- 目标场景重新打开成功；
- CircularArea 与 Ellen/Chomper 外观和碰撞正常；
- Animator 状态机正常；
- TMP HUD 正常；
- URP Shader 不粉紫；
- 1v1 键鼠流程通过；
- Windows Player 构建成功。

## 4. 预计收益与风险

### 可立即进入候选的收益

- 145 个示例/教程资源，5.789 MB。
- 不涉及目标角色、场景环境或战斗依赖。

### 中期高收益但需前置条件

- Level2 唯一依赖约 212.48 MB；
- Start 唯一依赖约 25.05 MB；
- 重复/未使用 Level1 LightingData 约 227.48 MB；
- `_Recovery/0.unity` 约 38.51 MB；
- 未引用 TIF 纹理、FBX、音频和旧场景数据合计数百 MB。

当前结构外总计约 879.19 MB，但这不等于可以一次性回收 879.19 MB。必须逐批通过 Unity 原生依赖、光照、动画、Shader 和构建验证。

### 主要风险

| 风险 | 影响 | 控制措施 |
|---|---|---|
| 目标场景仍完整保留旧玩法 | 删除旧资源会破坏当前场景 | 先建立 `__CombatOverlay` 与新系统 |
| Build Settings 与默认场景不一致 | 打包后仍进入旧 Start/Level1 | 先切换入口再做场景清理 |
| Level2 字符串切关 | 删除 Level2 导致运行时报错 | 替换 TransitionPoint |
| 脚本类型依赖不产生 GUID 边 | 删除脚本导致编译错误 | 程序集依赖分析 + Editor 编译 |
| Shader 文件名、`Shader.Find` | 材质失效或 Shader 缺失 | 按 Shader/材质族验证 |
| LightingData/NavMesh/Reflection Probe | 场景变暗、反射丢失、AI 无寻路 | 逐光照/导航烘焙集处理 |
| `.meta` 手工删除 | GUID 断裂，Prefab 引用丢失 | 只通过 Unity Editor 删除 |
| 无版本控制 | 无法可靠回滚 | 删除前建立快照 |

## 5. 验证矩阵

| 验证项 | 阶段 2E 后 | 阶段 2F 后 | 阶段 2G 后 |
|---|---|---|---|
| Console 无错误 | 必须 | 必须 | 必须 |
| 目标场景可打开 | 必须 | 必须 | 必须 |
| CircularArea 完整 | 必须 | 必须 | 必须 |
| Ellen/Chomper 可见且动画正常 | 必须 | 必须 | 必须 |
| 26→1 Chomper 转换稳定 | 否 | 按 2C 结果 | 必须 |
| 左右直拳/格挡/HP | 否 | 按进度 | 必须 |
| 暂停/恢复/重开 | 否 | 按进度 | 必须 |
| Windows Player 构建 | 必须 | 必须 | 必须 |
| 无 Missing Prefab/Script | 必须 | 必须 | 必须 |

## 6. 停止点

第一阶段到此停止。没有执行任何场景修改、资源删除、Prefab 修改、脚本删除、包依赖修改、ProjectSettings 修改或 `.meta` 修改。

等待用户确认后，才能开始第二阶段；第二阶段建议从“建立可恢复基线并打开当前工程进行 Unity 原生依赖验证”开始，而不会直接批量删除 REVIEW 资源。
