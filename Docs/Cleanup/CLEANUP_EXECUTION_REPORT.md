# 资源清理执行报告

执行日期：2026-10-30  
执行结果：**删除未执行；Git 回退基线已另行建立**  
工程目录：`E:\Unity Engine\AssetImportSandbox_InternationalURP`

> 2026-10-30 补充：用户已确认本工程为正式工程。当前目录现已初始化为 `main` 分支 Git 仓库，完整源码基线提交为 `ee9a0c4`；远程 `origin` 指向指定 GitHub 仓库。本报告中的删除和 Unity 验证状态仍然没有变化。

## 1. 执行摘要

本次按要求先验证备份、Git 回退点和正式战斗场景。正式战斗场景与关键角色引用存在，但当前工程不是 Git 仓库，指定远程对应的本地仓库也不是当前完整工程。

由于删除要求明确建立在“完整工程已有可回退提交”的基础上，因此：

- 实际删除资源：0。
- 实际移动资源：0。
- 实际修改 `.meta`：0。
- Packages、ProjectSettings、原始第三方资源和正式场景：0 修改。
- 战斗场景、所有 KEEP 资源、145 个 REMOVE 候选和 893 个 REVIEW 资源均保持原状。

## 2. 前置条件检查

| 前置条件 | 结果 | 证据 |
|---|---|---|
| 完整工程已备份并建立可回退 Git 提交 | **FAIL** | 当前目录执行 `git rev-parse` 返回 `not a git repository` |
| 本地 Git 仓库对应指定远程 | 部分满足但路径不一致 | `E:\Unity Engine\MotionFightPrototype` 的 `origin` 为 `https://github.com/Kirihara-JQ/MotionFightPrototype.git` |
| 本地 Git 工作区可作为当前工程回退点 | **FAIL** | 仓库只有 `SampleScene.unity`，没有目标战斗场景或 `Docs/Cleanup`；工作区有 21 项未提交变化 |
| 独立战斗场景存在且不同于原始 Level1 | **PASS（静态）** | `Assets/Scenes/Combat/Level1_CircularArena_Combat.unity` 与 `Assets/3DGamekit/Scenes/GamePlay/Level1.unity` 同时存在，哈希和大小不同 |
| 战斗场景包含 CircularArea | **PASS（静态）** | 目标场景中找到唯一的 `m_Name: CircularArea` |
| 战斗场景引用 Ellen | **PASS（静态）** | 目标场景包含 Ellen Prefab GUID `9d620787554a948e3876fe7dbdba24e8` 的引用 |
| 战斗场景引用 Chomper | **PASS（静态）** | 目标场景包含 Chomper Prefab GUID `e9569c7e13e51264082910415a120d38` 的引用；上一轮实例统计为 26 个 |
| 目标场景静态依赖原始 Level1 场景文件 | 未发现 | 原始 Level1 Scene GUID `ed46e53182fb08b4482fd3c4a75bc631` 在目标场景中的引用次数为 0 |

前置条件 1 是删除的安全门槛，因此即使后续静态检查满足，也不能继续删除。

### 2.1 Git 仓库状态

当前工程：

```text
E:\Unity Engine\AssetImportSandbox_InternationalURP
git rev-parse --is-inside-work-tree
=> fatal: not a git repository
```

指定远程对应的同级本地仓库：

```text
E:\Unity Engine\MotionFightPrototype
origin: https://github.com/Kirihara-JQ/MotionFightPrototype.git
HEAD:   3a9933ef7049108072790bd3355878c396049a7e
subject: chore: initial Unity 6000.3.27f1 URP project
```

该仓库不等同于当前待清理工程：

| 项目 | 当前待清理工程 | Git 仓库 |
|---|---:|---:|
| `Assets` 文件数 | 6,642 | 1,005 |
| 场景数 | 40 | 1 |
| Prefab 数 | 219 | 0 |
| C# 数 | 308 | 2 |
| 目标战斗场景 | 存在 | 不存在 |
| `Docs/Cleanup` | 存在 | 不存在 |
| 未提交变更 | 不适用（无 Git） | 21 项 |

不能把该仓库的现有提交当作当前 3D Game Kit 工程的回退点，也不能把两个不同工程的内容直接互相覆盖。

## 3. 正式战斗场景检查

| 项目 | 结果 |
|---|---|
| 路径 | `Assets/Scenes/Combat/Level1_CircularArena_Combat.unity` |
| 大小 | 40,378,691 bytes |
| SHA-256 | `418A7ECFA7DE52A500C8367C80C5FF593326F7211DF90AAAD16C8D374044B150` |
| 原始 Level1 大小 | 40,378,672 bytes |
| 原始 Level1 SHA-256 | `1C03005F21D4499666B6710E36A4F6B247494EEF18ADF45F1D102A19262AD74A` |
| 独立文件确认 | 是 |
| 原 Level1 Scene GUID 引用 | 0 |
| `CircularArea` | 存在 |
| Ellen 引用 | 存在 |
| Chomper 引用 | 存在 |
| `__CombatOverlay` | 不存在 |
| 旧 Level2 切关字符串 | 存在，`TransitionPoint.newSceneName = Level2` |

场景静态差异仍主要是 `CameraRig` 的位置和旋转。当前尚未建立新格斗覆盖层，所以此时删除旧玩法代码或资源会破坏正式场景现有引用。

## 4. 清理前后统计

报告中的“执行前”与“执行后”均为本次读取到的数据。由于阻塞，两个时点相同。

| 指标 | 执行前 | 执行后 |
|---|---:|---:|
| `Assets` 全部文件 | 5,946 | 5,946 |
| 非 `.meta` 资源 | 2,789 | 2,789 |
| `.meta` 文件 | 3,157 | 3,157 |
| Scene | 40 | 40 |
| Prefab | 219 | 219 |
| C# | 308 | 308 |
| `Assets` 大小 | 2,628.76 MB（含 meta） | 2,628.76 MB（含 meta） |

上一轮静态分类保持不变：

| 分类 | 文件数 | 大小 MB |
|---|---:|---:|
| KEEP | 1,751 | 1,743.45 |
| REMOVE 候选 | 145 | 5.789 |
| REVIEW | 893 | 873.40 |

## 5. 实际删除

无。

本次没有删除以下任何一类内容：

- 正式战斗场景；
- 原始 Level1、Start、Level2、Loading、模板或恢复场景；
- 145 个已标记的 REMOVE 示例/教程候选；
- TMP 示例、TutorialInfo、Readme、SampleScene；
- 任何 Prefab、FBX、材质、纹理、AnimationClip、Animator Controller、Shader、音频或 C#；
- 任何 `.meta`；
- 任何 Packages 或 ProjectSettings 文件。

## 6. 实际保留

以下内容全部保持原状：

- `Assets/Scenes/Combat/Level1_CircularArena_Combat.unity`
- `Assets/3DGamekit/Scenes/GamePlay/Level1.unity`
- CircularArea 及其环境 Prefab、材质、纹理和 Shader 闭包
- Ellen 及其模型、材质、骨架、Animator 和 Clip
- Chomper、Grenadier、EelDog 及其场景实例和依赖
- 当前 KEEP 清单中的全部资源
- 当前 893 个 REVIEW 资源
- 当前 145 个 REMOVE 候选，因执行阻塞而保留

## 7. 未处理路径

| 路径 | 状态 | 原因 |
|---|---|---|
| `Assets/TextMesh Pro/Examples & Extras/` | 未处理；REMOVE 候选 | Git 回退基线未建立 |
| `Assets/TutorialInfo/` | 未处理；REMOVE 候选 | Git 回退基线未建立 |
| `Assets/Scenes/SampleScene.unity` | 未处理；REMOVE 候选 | Git 回退基线未建立 |
| `Assets/Readme.asset` | 未处理；REMOVE 候选 | Git 回退基线未建立 |
| `Assets/3DGamekit/Readme.asset` | 未处理；REMOVE 候选 | Git 回退基线未建立 |
| `Assets/3DGamekit/Scenes/GamePlay/Level1.unity` | 未处理；KEEP | 当前 Build Settings 引用，且设计文档要求保留基线 |
| `Assets/3DGamekit/Scenes/GamePlay/Level2.unity` | 未处理；KEEP | 目标场景仍含 `newSceneName = Level2`，Build Settings 也引用 |
| `Assets/_Recovery/` | 未处理；REVIEW | 需用户确认恢复文件可弃用 |
| 其余 REVIEW 路径 | 未处理 | 必须通过 Editor/AssetDatabase/Play Mode 逐族验证 |

## 8. 验证结果

| 验证项 | 状态 | 说明 |
|---|---|---|
| Git 回退点 | BLOCKED | 当前工程不在 Git 中 |
| Unity Editor 打开当前工程 | NOT RUN | 当前 Editor 连接的是另一个工程 |
| Compile / Console | NOT RUN | 未打开当前工程 |
| AssetDatabase 依赖复核 | NOT RUN | 未打开当前工程 |
| Missing Script / Missing Reference | NOT RUN | 需要 Unity Editor |
| Missing Material / Shader | NOT RUN | 需要 Unity Editor |
| Animator 状态机运行 | NOT RUN | 需要 Unity Editor |
| 玩家和怪兽模型运行 | NOT RUN | 需要 Unity Editor |
| 碰撞体、输入、相机、HUD | NOT RUN | 需要 Unity Editor |
| Play Mode | NOT RUN | 前置阻塞 |
| Windows Standalone Build | NOT RUN | 前置阻塞 |

静态 GUID 扫描只能说明目标场景文件包含预期的 CircularArea、Ellen 和 Chomper 引用，不能代替 Unity 运行时验证。

## 9. 恢复执行所需条件

只能由用户确认工程归属后继续。安全路径有两条，二者不能混用：

1. 如果当前 `AssetImportSandbox_InternationalURP` 是正式工程：先为这个完整目录建立本地 Git 基线并提交全部所需资源，然后确认要使用的是哪一个本地仓库。
2. 如果 `E:\Unity Engine\MotionFightPrototype` 才是正式工程：先把当前完整 3D Game Kit 工程和已审查文档迁移到该仓库的明确分支，清理其现有未提交改动并建立新提交；不能直接覆盖或强推。

完成回退基线后，才能重新执行：

1. 用 Unity `6000.3.27f1` 打开正确工程。
2. 运行 `AssetDatabase.Refresh` 并检查 Console。
3. 验证目标场景、CircularArea、Ellen、Chomper、URP 材质和动画。
4. 处理 145 个 REMOVE 候选。
5. 重新计算目标场景依赖，再决定原 Level1、Level2 和旧玩法资源。

## 10. 结论

清理计划在本轮被执行到前置门槛，没有进入删除阶段。当前工程保持完整，没有因清理产生缺失引用或不可逆修改。
