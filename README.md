# DevelopTools

- [快速开始](#快速开始charactermodelunity)
- [功能介绍](#功能介绍)
- [BuffManagerUnity 示例](#buffmanagerunity-使用样例)
- [CharacterModelUnity 示例](#charactermodelunity-注册使用样例)

## 简介
DevelopTools ，全名 Library_Of_Ruina-DevelopTools 是一款开发用于辅助**废墟图书馆**进行Mod开发的工具  
该工具提供了几个便捷的类库和一些方法，用于辅助Mod开发者更加便捷的实现功能  
同时也提供许多可操作性的配置，供Mod开发者们自定义配置使用  

该程序的初衷是希望能够为各位开发者带来更加**舒适**和**规范**的开发体验，所以对许多功能进行了**集成封装**，使用了更加规范的调用方式(*当然也带来了新的bug*)  

***目前该项目仍在开发中，DevelopTools也仅为Debug版本，后续程序集内的代码可能会有较大修改，请谨慎使用***  

如果你需要，可以查询 [使用文档](DETAILS.md)

## 快速开始（CharacterModelUnity）

目标：修改原版罗兰的Hp为325，并且添加"DawnOffice"下的4号被动  
    
    //初始化类
    public class IndexInitializer : ModInitializer
    {
        //初始化函数
        public override void OnInitializeMod()
        {
            var op = CharacterModelUnity.RegisterOperation(); // 注册op操作组
            var corecardop = CharacterModelUnity.CreateCoreCardOperator(op); // 创建corecardop核心书页修改器 - 如要修改核心书页
            corecardop._goalLorId = new LorId(GamesLoader.orginal, 102); // 修改目标
            corecardop._maxhp = 325; // 修改项 - Hp
            corecardop._AddPassive.Add(new LorId("DawnOffice", 4)); // 修改项 - Passive

            op._rigisterOperation.Add(CharacterModelUnity.OperationRegister.RegisterCoreBook); // 添加操作符 - 修改战斗书页

            CharacterModelUnity.EnableRegisterOperation(); // 开始修改
        }
    }

> **注意**：这只是 `CharacterModelUnity` 的用法，纯反射操作，不需要 Harmony。
> 如果你想用 `BuffManagerUnity` / `CardAllyManagerUnity` / `DataManagerUnity`，
> 需要先 `GamesLoader.RegisterHarmony("你的ID")`，再调对应的 `EnableAdvancedFunction()`。
> 具体看下面的示例。


## 使用介绍
1. 将**DevelopTools.dll** 放在Mod文件夹的**Assemblies**目录下
2. 使用**引用**来选择**你Mod文件夹**下在上一步放置在**Assemblies**下的**DevelopTools.dll**，像选择**Assembly-CSharp**一样    
3. 添加**using DevelopTools**才能使用


至此，这份dll已经可以使用了  

## 功能介绍
| 我想... | 用这个 |
|---------|--------|
| 加/删/改 Buff | `BuffManagerUnity` |
| 给卡牌加 Buff | `BuffManagerUnity` |
| 加/删/查卡牌 | `CardAllyManagerUnity` |
| 改角色 HP/速度/抗性 | `CharacterModelUnity` |
| 替换核心书页/战斗书页 | `CharacterModelUnity` |
| 战斗中存跨舞台数据 | `DataManagerUnity` |

## 使用模板
四个模块都可以在战斗过程中使用，但**CharacterModelUnity**的***注册模块必须在初始化函数中调用过***   

### BuffManagerUnity 使用样例
创建Buff类

    public class BattleUnitBuf_BurnBreakAttack : BattleUnitBuf , IMaxStack , IInsistRound // 使用接口
    {
    //初始化
    public BattleUnitBuf_BurnBreakAttack(BattleUnitModel model)
    {
        //构造函数必须自己绑定对象 - 必须
        this._owner = model;
        //加载图片 - 可选
        BuffManagerUnity.LoadImage(this, "breakburn");
    }

    // 功能实现
    public override void OnRoundEnd()
    {
        this._owner.TakeBreakDamage(this.stack);
        this.stack = (this.stack / 3) * 2;
        if (this.stack == 0)
            this.Destroy();
    }

    // 接口均为可选，不写也能运行

    public int MaxStack { get; set; } = 8; // 接口，最大值为8，具体细节由BuffManagerUnity负责

    public int InsistRound { get; set; } = 2; // 接口，buff存活回合为2，具体细节由BuffManagerUnity负责
    }

使用Buff

    public override void OnSucceedAttack(BattleUnitModel target)
    {
        BuffManagerUnity.AddStackBuff(new BattleUnitBuf_BurnBreakAttack(target), target,5); // 对target施加5层BattleUnitBuf_BurnBreakAttack
    }

Buff定义时无需额外书写其它层数修改函数

### CharacterModelUnity 注册使用样例
CharacterModelUnity的注册功能必须写在OnInitializeMod中，否则无法执行  

上面使用的**LoadImage**需要设置图片读取文件夹才可使用，仅需要指明路径即可，不需要定义读取过程，支持读取**指定文件夹下的所有子文件夹**  

除此之外，CharacterModelUnity同时支持**使用xml文件进行注册**，具体详见[操作指南](DETAILS.md)  

    //初始化类
    public class IndexInitializer : ModInitializer
    {
        //初始化函数
        public override void OnInitializeMod()
        {
            GamesLoader.RegisterHarmony("Study"); // 注册Harmony，如要使用高级功能 - 必须
            GamesLoader.SetImageLoadPath(GamesLoader.GetDllPath() + "/ArtWork"); // 指定图片文件夹 - 如要加载图片 - 必须

            var op = CharacterModelUnity.RegisterOperation(); // 注册op操作组 - 必须
            var corecardop = CharacterModelUnity.CreateCoreCardOperator(op); // 创建corecardop核心书页修改器 - 如要修改核心书页 - 必须
            corecardop._goalLorId = new LorId(GamesLoader.orginal, 102); // 修改目标 - 必须
            corecardop._maxhp = 143; // 修改项 - 可选
            corecardop._AddPassive.Add(new LorId("Study", 2));


            var cardop = CharacterModelUnity.CreateBattleCardOperator(op); // 创建cardop战斗书页修改器 - 如要修改战斗书页 - 必须
            cardop._goalLorId = new LorId(GamesLoader.orginal, 702008); // 修改目标 - 必须
            cardop._rarity = Rarity.Unique;// 修改项 - 可选
            cardop._cost = 4;
            cardop._script = "AddBreakBurnCardForOur";

            op._rigisterOperation.Add(CharacterModelUnity.OperationRegister.RegisterCard); // 添加操作符 - 修改核心书页 - 必须
            op._rigisterOperation.Add(CharacterModelUnity.OperationRegister.RegisterCoreBook); // 添加操作符 - 修改战斗书页 - 必须

            CharacterModelUnity.EnableRegisterOperation(); // 开始修改 - 必须
        }
    }

代码大意：

注册**Harmony的对象绑定ID:Study**，然后选择当前.dll文件下的**ArtWork文件夹作为图像读取文件夹**  

创建**op作为注册组**  

创建**corecardop作为核心书页的注册实例**，绑定到op中  

将LorId为原版，102号核心书页(罗兰之页)的生命值改为143，添加1个LorId的被动  

创建**cardop作为战斗书页的注册实例**，绑定到op中  
将LorId为原版，702008号战斗书页(逻辑工作室)的稀有度改为金卡，光芒消耗改为4费，书页效果改为"AddBreakBurnCardForOur"  

之后对op进行需要执行的**操作声明**，需要执行**核心书页注册**和**战斗书页注册**操作  

执行**EnableRegisterOperation**函数开始注册，这样相关内容就被改变了  
