# 明日方舟风格地图编辑器 & 战斗系统

> 一个基于 Unity 的数据驱动 UGC 地图编辑器，支持地块编辑、模型管理、Buff 系统、波次管理和敌人生成。
> 
> **项目状态：** 核心功能完成，持续优化中
> **代码量：** 约 3500 行
> **开发周期：** 3 个月

---

## 演示视频

[![演示视频](https://img.youtube.com/vi/你的视频ID/0.jpg)](https://youtu.be/你的视频ID)

> 点击图片观看 2 分钟演示视频

---

## 目录

- [核心功能](#-核心功能)
- [技术架构](#-技术架构)
- [技术栈](#-技术栈)
- [代码结构](#-代码结构)
- [快速开始](#-快速开始)
- [项目亮点](#-项目亮点)
- [遇到的问题与解决方案](#-遇到的问题与解决方案)
- [代码统计](#-代码统计)
- [后续计划](#-后续计划)
- [联系方式](#-联系方式)

---

## 核心功能

### 1. 地图编辑系统

| 功能 | 说明 |
|------|------|
| **9 种地块类型** | 地面、高台、墙壁、火焰地板、寒冰地板、治疗地板、出怪点、保护点 |
| **三种编辑模式** | 地块编辑、属性查看、路径编辑 |
| **单击放置** | 点击地块即可放置当前选中的地块类型 |
| **长按批量** | 按住鼠标拖动，批量填充地块 |
| **右键功能** | 清空画笔 / 切换模式 / 取消操作 |
| **地图保存** | JSON 序列化，支持覆盖保存、另存为新地图 |
| **地图加载** | 自动扫描地图文件，按创建时间排序 |

### 2. 地块模型管理系统

| 功能 | 说明 |
|------|------|
| **模型数据库** | TileModelDatabase 统一管理所有模型资源 |
| **Mesh+Material 分离** | 每个模型独立配置网格和材质 |
| **默认模型** | 每个地块类型可配置默认模型 |
| **自定义模型** | 每个地块实例可单独覆盖模型 |
| **高性能切换** | 直接替换 Mesh 和 Material，不创建/销毁对象，无 GC |

### 3. 波次管理系统

| 功能 | 说明 |
|------|------|
| **三面板联动** | WavePanel + RouteSettingPanel + CreateConfigurationPanel |
| **波次列表** | 动态增删、序号自动排序、实时显示敌人数/路径点数 |
| **路径点编辑** | 坐标编辑、停留时间设置、序号自动重排 |
| **敌人配置** | 敌人贴纸动态添加/删除、数量调整（+/- 按钮） |
| **数据同步** | 三面板数据双向绑定，切换波次自动加载 |

### 4. Buff 系统

| 功能 | 说明 |
|------|------|
| **数据驱动** | ScriptableObject 配置，每个 Buff 独立文件 |
| **效果类型** | Damage（伤害/治疗）、attribute（属性修改）、State（状态控制） |
| **持续时间** | Instant（即时）、Timed（计时消失）、Permanent（永久） |
| **叠加规则** | 支持可叠加/不可叠加，最大层数限制，时长刷新策略 |
| **属性修改** | 支持修改生命、攻击、防御、移速 |
| **特效管理** | Buff 添加时自动挂载特效，移除时自动销毁 |

### 5. 敌人生成系统

| 功能 | 说明 |
|------|------|
| **波次执行** | 按波次顺序执行，每波独立倒计时 |
| **敌人出现间隔** | 波次内敌人按配置间隔生成 |
| **NavMesh 寻路** | 多路径点巡逻，停留时间设置 |
| **伤害数字** | 物理白/法术紫/真实黄/治疗绿，对象池管理，自动上升淡出 |

### 6. 地块 Buff 触发系统

| 功能 | 说明 |
|------|------|
| **Buff_Floor 基类** | 所有带 Buff 地块继承，统一触发逻辑 |
| **周期性检测** | 每帧计时，达到间隔给地块上所有敌人施加 Buff |
| **进入立即触发** | OnTriggerEnter 时立即施加，无延迟 |
| **离开移除** | OnTriggerExit 时触发移除事件 |
| **具体地块** | Ice_Floor（减速）、Flame_Floor（燃烧）、Treatment_Floor（治疗） |

---

## 技术架构

### 整体架构图

```
┌─────────────────────────────────────────────────────────────┐
│                        表现层                                │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │TileVisual│  │ UI面板   │  │  Enemy   │  │DamageNum │   │
│  │ 地块显示  │  │ 波次配置  │  │ 敌人实体  │  │ 伤害数字  │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
└─────────────────────────────────────────────────────────────┘
                              ↕ 事件
┌─────────────────────────────────────────────────────────────┐
│                      事件总线 GameEvents                     │
│         OnTileBuffEnter / OnTileBuffExit / ...              │
└─────────────────────────────────────────────────────────────┘
                              ↕
┌─────────────────────────────────────────────────────────────┐
│                        逻辑层                                │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │MapEditor │  │BuffMgr   │  │SpawnFloor│  │MouseMgr  │   │
│  │ 地图编辑  │  │ Buff管理 │  │ 敌人生成  │  │ 鼠标管理  │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
└─────────────────────────────────────────────────────────────┘
                              ↕
┌─────────────────────────────────────────────────────────────┐
│                        数据层                                │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │TileTypeDB│  │ MapData  │  │ BuffData │  │EnemyData │   │
│  │ 地块定义  │  │ 地图数据  │  │ Buff配置 │  │ 敌人配置  │   │
│  │ 字典O(1) │  │ 二维数组  │  │   SO    │  │   SO    │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### 模块依赖关系

```
┌─────────────────────────────────────────────────────────────┐
│                     依赖关系（单向）                          │
│                                                             │
│   数据层 ← 逻辑层 ← 表现层                                   │
│                                                             │
│   TileTypeDB ← MapEditor ← TileVisual                       │
│   BuffData ← BuffManager ← Enemy                            │
│   WaveData ← Spawn_Floor ← UI Panel                         │
│                                                             │
│   模块间通过 GameEvents 事件系统解耦                         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 数据流向图

```
[用户操作] → [MouseManager] → [MapEditor] → [MapData] → [TileVisual]
                              ↓
                        [GameEvents]
                              ↓
                      [BuffManager] → [Enemy]
                              ↓
                      [DamageNumberManager]
```

---

## 🛠️ 技术栈

| 类别 | 技术 | 用途 |
|------|------|------|
| **引擎** | Unity 2021.3 | 游戏引擎 |
| **语言** | C# 9.0 | 编程语言 |
| **寻路** | NavMesh | 敌人自动寻路 |
| **数据存储** | ScriptableObject | 配置数据 |
| **数据结构** | Dictionary | O(1) 快速查找 |
| **序列化** | JsonUtility | 地图保存/加载 |
| **UI** | UGUI | 界面系统 |
| **动画** | Animator | 敌人动画 |
| **特效** | Particle System | 伤害数字、Buff 特效 |
| **版本控制** | Git | 代码管理 |

---

## 代码结构

```
Assets/
├── Scripts/
│   ├── Data/                         # 数据层
│   │   ├── TileData.cs               # 地块数据（只存ID）
│   │   ├── MapData.cs                # 地图数据（二维数组）
│   │   ├── BuffData.cs               # Buff 配置（SO）
│   │   ├── EnemyData.cs              # 敌人配置（SO）
│   │   ├── WaveData.cs               # 波次数据
│   │   ├── TileTypeDatabase.cs       # 地块类型数据库（字典）
│   │   ├── BuffDatabase.cs           # Buff 数据库
│   │   ├── EnemyDatabase.cs          # 敌人数据库
│   │   └── TileModelDatabase.cs      # 模型数据库
│   │
│   ├── Logic/                        # 逻辑层
│   │   ├── MapEditor.cs              # 地图编辑逻辑
│   │   ├── MouseManager.cs           # 鼠标交互（三种模式）
│   │   ├── BuffManager.cs            # Buff 管理
│   │   ├── Spawn_Floor.cs            # 敌人生成
│   │   └── GameEvents.cs             # 全局事件系统
│   │
│   ├── Presentation/                 # 表现层
│   │   ├── TileVisual.cs             # 地块显示（颜色、高度、模型）
│   │   ├── Enemy.cs                  # 敌人实体
│   │   ├── EnemyMovement.cs          # 敌人移动（NavMesh）
│   │   ├── DamageNumber.cs           # 伤害数字
│   │   └── DamageNumberManager.cs    # 伤害数字对象池
│   │
│   ├── UI/                           # UI 面板
│   │   ├── WavePanel.cs              # 波次管理面板
│   │   ├── RouteSettingPanel.cs      # 路线设置面板
│   │   ├── CreateConfigurationPanel.cs # 敌人配置面板
│   │   ├── EnemySelectorPanel.cs     # 敌人选择面板
│   │   └── ModelSelectorPanel.cs     # 模型选择面板
│   │
│   ├── Enemy/                        # 敌人系统
│   │   ├── EnemyWaveItem.cs          # 波次预制体
│   │   ├── EnemyWaypointItem.cs      # 路径点预制体
│   │   ├── EnemySticker.cs           # 敌人贴纸
│   │   └── EnemyAvatar.cs            # 敌人头像
│   │
│   └── Floor_Buff/                   # 地块 Buff
│       ├── Buff_Floor.cs             # 地块 Buff 基类
│       ├── Ice_Floor.cs              # 寒冰地板
│       ├── Flame_Floor.cs            # 火焰地板
│       └── Treatment_Floor.cs        # 治疗地板
│
├── Resources/                        # 配置文件
│   ├── Buffs/                        # Buff SO 文件
│   ├── Enemies/                      # 敌人 SO 文件
│   └── Models/                       # 模型配置
│
├── Scenes/
│   └── MainScene.unity               # 主场景
│
└── Maps/                             # 保存的地图文件
    └── *.json
```

---

## 🚀 快速开始

### 环境要求
- Unity 2021.3 或更高版本
- Visual Studio 2019/2022

### 运行步骤

```bash
# 1. 克隆项目
git clone https://github.com/你的用户名/项目名.git

# 2. 用 Unity 打开项目文件夹

# 3. 打开场景
Assets/Scenes/MainScene.unity

# 4. 点击运行
```

### 操作说明

| 操作 | 说明 |
|------|------|
| **左键单击** | 放置当前选中的地块 |
| **左键长按+拖动** | 批量填充地块 |
| **右键单击** | 清空画笔 / 切换模式 |
| **右键空白处** | 切换到属性编辑模式 |
| **点击地块** | 查看地块属性 |
| **Ctrl+S** | 保存地图 |

---

## 项目亮点

### 1. 数据驱动设计
```csharp
// 新增地块只需一行配置，不用改代码
AddType(new TileTypeDefinition(
    "Magma_Floor", "岩浆地板", true, true, 0, Color.red,
    buffId: "buff_magma",
    defaultModelId: "floor_magma"
));
```

### 2. 模块解耦（事件系统）
```csharp
// 地块不知道谁在处理Buff
GameEvents.TriggerBuff(buffId, enemy, this.gameObject);

// BuffManager 不知道谁触发了Buff
void OnBuffEnter(string buffId, GameObject target, ...)
{
    // 只处理逻辑
}
```

### 3. 高性能模型切换
```csharp
// 不创建/销毁对象，直接替换 Mesh 和 Material
meshFilter.mesh = newMesh;
meshRenderer.material = newMaterial;
// 无 GC，性能优秀
```

### 4. O(1) 查找
```csharp
// 字典存储，瞬间获取地块类型
private Dictionary<string, TileTypeDefinition> typeDict;
public TileTypeDefinition GetType(string id) => typeDict[id];
```

---

## 遇到的问题与解决方案

### 问题1：敌人不开始巡逻
| 项目 | 内容 |
|------|------|
| **现象** | 敌人生成后原地不动 |
| **原因** | NavMeshAgent 未初始化就调用 SetDestination |
| **解决** | 添加协程等待一帧，检查 isOnNavMesh |

### 问题2：多个冰地块导致双倍减速
| 项目 | 内容 |
|------|------|
| **现象** | 两个冰地块叠加成 60% 减速 |
| **原因** | 按来源查找 Buff，没按 buffId 去重 |
| **解决** | 改为查找同名 Buff，处理叠加逻辑 |

### 问题3：伤害数字对象池报错
| 项目 | 内容 |
|------|------|
| **现象** | MissingReferenceException |
| **原因** | 对象被销毁后仍试图返回池子 |
| **解决** | 添加空值检查，改进回收逻辑 |

### 问题4：地图重新生成时脚本丢失
| 项目 | 内容 |
|------|------|
| **现象** | Spawn_Floor、Ice_Floor 组件没了 |
| **原因** | 只恢复了地块类型，没恢复组件 |
| **解决** | TileVisual 中根据类型自动添加组件 |

---

## 代码统计

| 模块 | 文件数 | 代码行数 |
|------|--------|---------|
| 数据层 | 8 | ~800 |
| 逻辑层 | 5 | ~700 |
| 表现层 | 5 | ~600 |
| UI 面板 | 5 | ~800 |
| 敌人系统 | 4 | ~400 |
| 地块 Buff | 4 | ~200 |
| **总计** | **31** | **~3500** |

---

## 后续计划

- [ ] 移动端适配（Android/iOS）
- [ ] Addressables 资源管理
- [ ] 性能优化（Profiler 分析）
- [ ] 单元测试
- [ ] 微信小游戏平台发布

---

## 联系方式

- **GitHub**：[github.com/你的用户名](https://github.com/onedayyi)
- **邮箱**：2300331400@qq.com.com
- **演示视频**：[B站 链接]