# 资源清理执行报告

执行日期：2026-10-30
工程目录：`E:\Unity Engine\AssetImportSandbox_InternationalURP`
Unity：`6000.3.27f1`
最终状态：**低风险清理完成；编译、Play Mode 与 Windows Standalone 构建通过**

## 1. 执行摘要

本次使用 Unity `AssetDatabase` 对已审查清单中的 145 个低风险资源逐一删除，全部成功。没有删除任何 REVIEW 项，没有手工删除 `.meta`，也没有删除正式战斗场景、Ellen、Chomper、CircularArea 环境、材质、纹理、动画、音频或仍被使用的脚本。

原始 Level1 场景被保留。虽然正式战斗场景没有直接 GUID 引用它，但 Unity `AssetDatabase` 发现正式场景通过 `Assets/3DGamekit/Scenes/GamePlay/Level1/LightingData.asset` 递归依赖原始 Level1 Scene 资产，因此不满足第三步的删除条件。

主要结果：

| 项目 | 结果 |
|---|---|
| 精确 REMOVE 资源 | 145 |
| 实际删除成功 | 145 |
| 删除失败 | 0 |
| Unity 自动移除的关联 `.meta` | 145 |
| 手工修改 `.meta` | 0 |
| REVIEW 项删除 | 0 |
| 正式战斗场景 | 保留 |
| 原始 Level1 | 保留，存在 LightingData 间接依赖 |
| Compile | 0 errors |
| Play Mode | 通过 |
| Windows Standalone Build | Success |

精确删除清单保存在 [REMOVE_EXECUTION_MANIFEST.txt](<E:/Unity Engine/AssetImportSandbox_InternationalURP/Docs/Cleanup/REMOVE_EXECUTION_MANIFEST.txt>)。

## 2. 前置条件

| 前置条件 | 结果 | 证据 |
|---|---|---|
| 完整工程已有 Git 回退基线 | 通过 | 基线提交 `ee9a0c49efa14303091019b5686f321c8a9da599` |
| 本地与远端 main 一致 | 通过 | 清理前两者均为 `0cb0838282a9377b74de7988a85f0e6f098e1d44` |
| 独立战斗场景存在 | 通过 | `Assets/Scenes/Combat/Level1_CircularArena_Combat.unity` |
| 不是唯一 Level1 | 通过 | 原始 `Assets/3DGamekit/Scenes/GamePlay/Level1.unity` 同时存在 |
| CircularArea | 通过 | 运行时 `Environment/Structures/CircularArea` 可定位 |
| Ellen | 通过 | 运行时 1 个有效 `PlayerController` / Ellen 实例 |
| Chomper | 通过 | 运行时找到 26 个 `ChomperBehavior` |
| 材质和动画 | 通过 | 运行时 Animator 已初始化；Shader 错误为 0 |

## 3. 删除前验证

删除前通过 Unity Editor 执行：

- `AssetDatabase.Refresh(ForceSynchronousImport)`；
- 确认 145 个候选均存在；
- 计算正式场景完整依赖，共 1,647 个依赖资源；
- 扫描 613 个场景、Prefab、材质、动画、Controller 和 Asset 型依赖方；
- 验证候选与正式场景依赖交集为 0；
- 验证候选入站引用为 0。

通过后使用 `AssetDatabase.DeleteAsset` 对精确清单逐项删除，没有使用文件系统通配删除。

## 4. 实际删除路径

### 4.1 路径汇总

| 路径 | 删除资源数 | 说明 |
|---|---:|---|
| `Assets/TextMesh Pro/Examples & Extras/` | 136 | TMP 示例场景、脚本、字体、材质、Prefab、纹理和示例 Resources |
| `Assets/TutorialInfo/` | 6 | URP 模板教程窗口、图标、布局和脚本 |
| `Assets/Readme.asset` | 1 | URP 模板说明 |
| `Assets/3DGamekit/Readme.asset` | 1 | Game Kit 示例说明 |
| `Assets/Scenes/SampleScene.unity` | 1 | URP 空白示例场景 |
| **合计** | **145** | 0 失败；对应 145 个 `.meta` 由 Unity 自动处理 |

具体 145 条路径均在 `REMOVE_EXECUTION_MANIFEST.txt` 中；没有按目录一次性清除。

### 4.2 引用复核

- 145 个被删除资源在 Git 基线中各有一个 GUID。
- 在剩余所有 `.unity`、`.prefab`、`.asset`、`.mat`、`.anim`、`.controller`、`.playable`、Shader 和 C# 文件中检索这 145 个旧 GUID。
- 命中数：0。
- 因此没有保留资源继续引用已删除项。

## 5. 清理前后数量

| 指标 | 清理前 | 清理后 | 变化 |
|---|---:|---:|---:|
| `Assets` 全部文件 | 5,946 | 5,656 | -290 |
| 非 `.meta` 资源 | 2,789 | 2,644 | -145 |
| `.meta` 文件 | 3,157 | 3,012 | -145 |
| Scene | 40 | 8 | -32 |
| Prefab | 219 | 216 | -3 |
| C# | 308 | 272 | -36 |
| 非 `.meta` 资源大小 | 2,622.63 MB | 2,616.84 MB | -5.79 MB |
| Assets 全部文件大小 | 2,628.76 MB | 2,622.90 MB | -5.86 MB |

Scene 减少量包含 31 个 TextMesh Pro 示例场景和 `SampleScene.unity`。

## 6. Build Settings

`ProjectSettings/EditorBuildSettings.asset` 已更新为唯一入口：

```text
Assets/Scenes/Combat/Level1_CircularArena_Combat.unity
```

旧 `Start`、`Level1`、`Level2` 已从 Build Settings 移除。旧场景文件仍按 KEEP/REVIEW 状态保留。

## 7. 原始 Level1 处理

原始路径：

`Assets/3DGamekit/Scenes/GamePlay/Level1.unity`

状态：**保留，未删除**

原因：

```text
Level1_CircularArena_Combat.unity
  -> Assets/3DGamekit/Scenes/GamePlay/Level1/LightingData.asset
  -> Assets/3DGamekit/Scenes/GamePlay/Level1.unity
```

正式场景没有直接引用原始场景 GUID，但 Unity 递归依赖结果包含该 Scene 资产。依据执行要求第三步，停止删除。

其余环境、模型、材质、纹理、动画、Shader 和脚本全部保留。

## 8. 保留与未处理

| 类别 | 数量 | 状态 |
|---|---:|---|
| KEEP | 1,751 | 全部保留 |
| REVIEW | 893 | 全部保留，未转成 REMOVE |
| 原始 Level1 | 1 个场景文件 | 因 LightingData 间接依赖保留 |
| 其余 Game Kit 场景 | 保留 | 未自动删除 |
| 旧玩法、平台跳跃、解谜、旧 AI、旧伤害和旧 HUD | 保留 | 正式场景仍引用，不能在本阶段安全删除 |

重点 REVIEW 路径继续包括：

- `Assets/3DGamekit/Art/`
- `Assets/3DGamekit/Scenes/`
- `Assets/3DGamekit/Audio/`
- `Assets/3DGamekit/Scripts/`
- `Assets/3DGamekit/Packages/`
- `Assets/3DGamekit/Prefabs/`
- `Assets/TextMesh Pro/Shaders/`
- `Assets/_Recovery/`

## 9. 编译和资源完整性

### 9.1 Compile / Console

- 删除后 Unity 脚本重新编译：完成。
- 编译错误：0。
- 当前 Console：0 error、0 warning。
- 构建期间有一条既有 C# 警告：`SimpleFXSynth.cs(20,32): warning CS0109`。
- Play Mode 期间出现一条 Cinemachine `CompositionModifier` 的 SerializeReference 警告，不影响运行。
- MCP WebSocket 在长时间构建期间断连多次，属于编辑器工具传输警告，不是项目编译错误。

### 9.2 Missing Script / Reference / Material / Shader

| 检查项 | 结果 |
|---|---|
| 正式场景 Missing Script | 0 |
| 正式场景渲染器数量 | 13,630 |
| Shader 编译错误 | 0 |
| 可见模型缺失材质 | 0 |
| 无材质 MeshRenderer | 8 个 `Environment/InvisibleWalls/InvisibleWall`，用于不可见边界 |
| 无材质 Particle/Trail Renderer | 70 个，主要是未启用的 VFX/Trail 槽位 |
| 删除资源残留 GUID 引用 | 0 |
| 广泛保留资源静态 GUID 扫描 | 发现 519 个既有未解析 token，均不属于本次删除项，继续 REVIEW |

519 个既有未解析 token 不是本次清理新增的引用。它们所在资源未进入正式场景运行的故障路径，但后续仍需按资源族继续审计。

## 10. Play Mode 验证

验证场景：`Level1_CircularArena_Combat`

| 项目 | 结果 |
|---|---|
| 场景启动 | 通过 |
| GameObject 数 | 19,354 |
| Ellen | 1 个有效玩家 |
| Chomper | 26 个行为实例 |
| CircularArea | 通过 |
| CharacterController | 1 |
| Collider | 3,087 |
| Animator | 67；6 个没有 Controller，多为纯骨骼 Animator |
| Camera | 1 个 CameraBrain |
| AudioListener | 1 |
| Canvas | 18 |
| HealthUI | 1，运行时生命图标可见 |
| URP Shader | 运行时无 Shader 错误 |
| 键鼠设备 | Editor 检测到键盘和鼠标 |

实际键盘输入测试：

- 向 Unity Game 窗口发送持续 2 秒的 `W`。
- Ellen 从约 `(23.9500, 1.3378, 64.2900)` 移动到 `(24.1364, 2.3501, 78.8075)`。
- 结论：键盘移动输入、CharacterController 和场景碰撞移动链路正常。

验证截图：

- `Logs/CleanupValidation/after_remove_scene_view.png`
- `Logs/CleanupValidation/after_remove_play_mode.png`
- `Logs/CleanupValidation/after_remove_chomper.png`

## 11. Windows Standalone Build

| 项目 | 结果 |
|---|---|
| BuildPipeline 结果 | Success |
| 产物 | `Builds/CleanupValidation/MotionFightPrototypev1.1.exe` |
| 完整构建数据目录 | `Builds/CleanupValidation/MotionFightPrototypev1.1_Data/` |
| Build 文件数 | 1,716 |
| Build 大小 | 1,716.85 MB |
| EXE SHA-256 | `F083B0E879A9298CDCF9933A8F3B4F2B013E13F7D3E51221FC0D756B79E28807` |
| 启动存活检查 | 通过：进程持续运行 10 秒；检查后主动关闭 |
| Player 日志编译/运行错误 | 未发现 |
| Player 日志图形警告 | D3D12 在隐藏启动下切换 `2560x1600` 分辨率失败，不影响进程启动 |

构建产物位于被 `.gitignore` 排除的 `Builds/`，不会提交进仓库。

## 12. Unity 自动更新的设置

构建过程中 Unity 自动更新了：

- `Assets/Settings/PC_RPAsset.asset` 的 Shader Prefiltering 设置；
- `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset` 的 URP Runtime Settings 序列化；
- `ProjectSettings/EditorBuildSettings.asset` 的正式场景入口。

这些变更来自 Unity URP 构建流程，构建成功且运行时无 Shader 错误，保留在最终工作区中，需随本次清理提交。

## 13. 限制

- 没有删除 893 个 REVIEW 项。
- 没有删除仍被正式场景依赖的旧玩法脚本、UI、AI、伤害系统、敌人和环境资源。
- 没有删除原始 Level1。
- 519 个固有未解析 GUID token 继续标记为 REVIEW。
- Windows Player 只完成了 10 秒启动存活检查；没有人工完成整局游戏流程验收。

## 14. 最终结论

已安全完成当前审查范围内的低风险资源清理。正式场景、Ellen、Chomper、CircularArea、URP 材质、动画、碰撞体、相机、HUD、Play Mode 和 Windows 构建均未因本次删除失败。高风险示例玩法和旧系统仍保持原位，等待新战斗系统完成后再按 REVIEW 证据逐步处理。
