# DevelopTools 使用文档

- [大纲](#大纲)
- [GamesLoader 和 FilesLoader](#加载性质的类)
- [BuffManagerUnity类](#buffmanagerunity)
- [CardAllyManagerUnity类](#cardallymanagerunity)
- [CharacterModelUnity类](#charactermodelunity)
- [DataManagerUnity类](#datamanagerunity)
- [注意事项](#注意事项)

## 大纲

该部分介绍设计理念和软件运作流程  

### 杂谈
DevelopTools具体由5个部分组成，其中4个部分为具体的功能实现模块，1个部分为该工具自身加载处理的模块  

由于在Mod制作时候，实际运行的是开发者自己的.dll，该工具只是一个代码集成工具并不进行具体的代码调用，部分选项均设置为手动开启  

因此部分涉及到Harmony相关Patch操作的功能函数必须使用Patch进行操作，为了保证后续开发便利和开发者使用习惯，工具并没有设置一个自动Patch函数  

并且4大功能类(CharacterModelUnity除外)的设计回合计时等功能必须要Patch才可正常运作  

受支持的每个功能类都有一个专属的自动Patch函数，调用即可完成相关Patch，无需手动Patch  

因为Patch操作对图书馆和Mod有一定影响，因此不同模块之间的Patch操作是独立的，使用者可以根据自己需求启用  

由于目前功能较少，并没有分别为各个模块内Patch操作添加枚举区分内部细节，均为统一触发，但后续工具复杂后可能一个类内也需要调用多个Patch操作  

**工具后续可能出现较大更新，选择使用一个.dll后请勿轻易切换版本，以免造成调用失败**  

### 运行流程
该工具并无自身独立运行程序，但依旧具备一定的使用流程，流程的触发需要开发者自身选择调用  

常规下，需要在Mod的Initalize(初始化)类下完成一些函数的调用，且一定需要按照特定的顺序，才可启用部分功能  

不过，该工具也有许多功能是可以直接通过静态调用使用的，这种功能通常为某些代码的集成调用，可以大大简化代码编写量  

***最简单流程触发 - 不需要计时功能和注册功能***
- Initalize (初始化类) - 无需任何操作
**可以直接使用部分静态方法和一些特殊方法(特殊方法在未被激活时，部分不需要涉及Patch的模块依旧能够正常运行)**  

***次简单流程触发 - 不需要计时功能和注册功能，但需要加载Buff图片***
- Initalize (初始化类) - 需要指定图片路径
**图片必须指定路径才能加载，这是必须的，加载细则无需担心**  

***次简单流程触发 - 需要Buff计时，跨舞台存储，Card计时等功能***
- Initalize (初始化类) - 需要注册Harmony，然后调用对应类的开启函数，开启需要使用的类的高级功能  
**初始化时调用一次即可，必须注册Harmony后调用才生效**  

***注册流程触发 - 需要执行注册操作***
- Initalize (初始化类) - 不需要注册Harmony，但注册相关操作必须在Initalize中完成，其余地方完成可能无效或出现Bug  
**注册流程详细请见[书页注册](#operator注册示例)**  

***所有功能均要使用 - 需要高级功能和注册***
- Initalize (初始化类) - 需要注册Harmony，完成路径指定，并且在Initalize中完成注册，开启函数的调用必须为注册Harmony后，注册函数和路径指定不受到Harmony影响  

# 加载性质的类
GamesLoader 和 FilesLoader 是组件用来统一化管理和内部处理的类，其中FilesLoader主要用于实现private的内部处理，包括图片加载的具体实现等，开发者不需要操控该类  

GamesLoader 主持了整个工具的生命管理和统一化操作，是进行一些功能改写前的必须操作，以下介绍**详细信息**和**使用样例**  

**所有类均为静态类**

## 方法和函数

`string GetDllPath()` 获取DevelopTools本体的路径，你需要将**DevelopTools**放置到**Assemblie**文件夹下，这样可以用其路径来替代你的dll路径，完成图片加载和相关操作  

`void SetImageLoadPath(string path)` 设置图片资源文件夹的路径，该路径下的**所有图片资源**和**子文件夹下的一切**均会在初始化后被自动加载，支持多目录存放  

常用写法如下：

    GamesLoader.SetImageLoadPath(GamesLoader.GetDllPath()); // 将当前dll文件夹和所有子文件夹设置为图片加载文件夹 - 通常为 Assemblie  
    GamesLoader.SetImageLoadPath(GamesLoader.GetDllPath() + "/ArtWork"); // 将当前dll文件夹下的ArtWork文件夹设置为图片加载文件夹 Assemblie\ArtWork  

`void SetImageLoadPath(DirectoryInfo dir)` 不希望使用string? 有重载DirectoryInfo的函数  

`void SetImageLoadDict(Dictionary<string, Sprite> artworks)` 不希望使用该工具的自动加载, 但又想使用该工具来调用图片? 那么你可以自行加载一个Dictionary传入，工具内的Dictionary会转化为你传入的Dictionary  

`void RegisterHarmony(string packageid)` 注册Harmony，填入的string即为Harmony在工具内的id，由于id会进行许多操作，这边建议直接填入mod的**packageid**  
**该函数是一切高级函数的源头，必须先注册Harmony，其它函数才有Harmony使用**  

`void Log(string msg)` 打印一份日志  

`void DeleteLog()` 删除已有日志，由于工具的局限性，**暂时**采用手动删除日志的方式，如果不调用该函数那么日志会在每次运行工具时**堆积**

## 内部成员

`Harmony harmony` 你注册的Harmony，未调用函数时候为空  

`string _packageid`  dll绑定的mod的packageid，在注册Harmony时候传入的string会自动初始化为该值，若不进行注册可能需要手动填写  

`readonly string orginal = ""`  原版标识符，原版内容的LorId均为("",id)，若觉得 "" 不够美观，可以替换为GamesLoader.orginal，带来更好的心情:)  


## 使用样例

    //初始化类
    public class IndexInitializer : ModInitializer
    {
        //初始化函数
        public override void OnInitializeMod()
        {
            GamesLoader.DeleteLog(); // 我特别建议你在一开始就清空日志，避免后续日志丢失
            GamesLoader.RegisterHarmony("Study");
            GamesLoader.SetImageLoadPath(GamesLoader.GetDllPath() + "/ArtWork");

            GamesLoader.Log("Hello World For My Mod");
        }
    }

### 日志系统

**由于该程序集开发不完整，暂时没有一个完善的事件系统，只对部分核心功能做了日志记录**  
日志集会自动生成在DevelopTools的同级目录，命名为**tool_logs**  

由于日志不会自动清理，建议开发者在初始化时候调用删除  

程序自己触发的日志会有3中类型: **Info** **Warning** **Error**  

- **Info** 特指程序正常运行触发的事件  
- **Warning** 指程序运行过程中可能由于出现不完整信息，无法按照指示完全实现  
- **Error** 指程序无法执行该功能触发的错误  

你也可以通过Log来在自己的mod中打印输出信息进行调试，通过GamesLoader.Log()打印的日志也在**tool_logs**中  

日志样例: 

    22:50:47.524  Info: Register A Harmony : Study
    22:50:47.547  Info: SetImageLoadPath is D:\App\Steam\steamapps\common\Library Of Ruina\LibraryOfRuina_Data\Mods\NormalInvitation\Assemblies\ArtWork
    22:50:47.552  Info: Successfully Patch CardAllyManagerUnity
    22:50:47.555  Info: Successfully Patch DataManagerUnity
    22:50:47.559  Info: Successfully Patch BuffManagerUnity
    22:50:47.563  Info: Register Successfully


# BuffManagerUnity

该类用于管理Buff相关的操作，无论是角色Buff还是书页Buff，均由此类负责  

### 方法和函数

`void EnableAdvancedFunction()` **启用高级功能 (BuffManagerUnity类非正常运行可能不会触发Warning)**  

`void LoadImage(BattleUnitBuf buf, string name)` 加载图片资源，需要指定图片路径后才可使用，可以用作带图片资源的Buff的图片加载，也可以用作后续的图片切换
参数buf - 需要加载图片的buf实例，name - 图片资源的名称(无后缀)

`void LoadImage(BattleDiceCardBuf buf, string name)` 重载 - 书页Buff资源的图片加载  

使用示例: 

    public class BattleUnitBuf_BurnBreakAttack : BattleUnitBuf
    {
        //初始化
        public BattleUnitBuf_BurnBreakAttack(BattleUnitModel model)
        {
            this._owner = model;
            this.stack = 0;
            BuffManagerUnity.LoadImage(this, "breakburn"); // 加载初始图片
        }

        public override void OnRoundEnd()
        {
            if(this.stack > 10)
                BuffManagerUnity.LoadImage(this, "breakburn2"); // 切换图片
        }
    }

`void ActiveBuff(BattleUnitBuf buf, BattleUnitModel model)` 激活角色Buff，激活的buff层数为0(不显示数字角标)  
buf - 一个buf的匹配标签类，model - 激活的角色目标  
**该方法不会重复添加已经存在的buf，使用时会将传入的buf实例直接作为添加的buf，建议以new的形式进行添加**  

`void SetStackBuff(BattleUnitBuf buf, BattleUnitModel model, int stack)` 设置角色Buff层数，若该Buff不存在，则会使用buf作为添加的buf实例并设置为相关层数  
**不会重复添加已经存在的buf**  

`void AddStackBuff(BattleUnitBuf buf, BattleUnitModel model, int stack)` 添加角色Buff层数，正数为增负数为减，若该Buff不存在，则会使用buf作为添加的buf实例并设置为相关层数    
**不会重复添加已经存在的buf**  

`void ClearBuffStack(BattleUnitBuf buf, BattleUnitModel model)` 将目标角色的某个Buff的层数置为0  
**该函数不会销毁Buff**  

`void DestroyBuff(BattleUnitBuf buf, BattleUnitModel model)` 将目标角色的某个Buff实例销毁(移除Buff)  

`void DestroyAllBuffs(BattleUnitModel model)` 将目标角色的所有Buff实例销毁  

`void DestroyAllBuffs(BattleUnitModel model, BufPositiveType type = BufPositiveType.None)` **重载** 将目标角色的所有符合性质的Buff实例销毁  
支持性质: **None** (默认:中立Buff) **Negitive** (负面Buff) **Positive** (正面Buff)  
**Buff性质根据buf标签进行分辨，而不是根据效果分辨**  

`void DestroyAllBuffs(BattleUnitModel model, BattleUnitBuf buf)` **重载** 将目标角色的某个Buff实例销毁(销毁所有Buff实例)  
`DestroyBuff()`只能支持销毁一个Buff，无法销毁同一个Buf类型的多个buf,而该方法可以将同一个Buf类型的所有buf销毁  

`int GetActiveBuffStack(BattleUnitBuf buf, BattleUnitModel model)` 获取某个角色的指定Buff类型的层数，无Buff实例返回值为0  

`BattleUnitBuf GetActiveBuff(BattleUnitBuf buf, BattleUnitModel model)` 获取某个角色的指定Buff实例，无Buff实例返回值为null  

`void TransferBuff(BattleUnitBuf buf, BattleUnitModel from, BattleUnitModel to)` 将from角色的指定buff移动到to角色身上  

`void CopyToBuff(BattleUnitBuf buf, BattleUnitModel from, BattleUnitModel to)` **该方法可能无法按照预期工作**  
将from角色的指定buff复制到to角色身上  

`int GetBufInsistRound(BattleUnitBuf buf,BattleUnitModel model)` 该Buf必须实现**IInsistRound**接口，查询model身上buf还能存活的回合数，**无实现接口和不存在buf返回值为-1**  

**注意: 传入的buf参数正常情况下仅作为查找依据，若目标无存在的buf实例，则传入的buf参数会成为新添加的buf实例**  
**若buf为其它model中的存在的buf，那么会导致添加的buf和存在的buf绑定为同一实例，建议按照示例的标准用法进行使用**  

使用示例: 

    class DiceCardAbility_dmgBreakBurn : DiceCardAbilityBase
    {
        public static string Desc = "[命中时] 目标混乱烧伤层数大于5层，追加混乱烧伤层数 * 10的体力伤害，移除buff，若不满足条件则对自身施加5层混乱烧伤并且移除所有增益Buff";

        public override void OnSucceedAttack(BattleUnitModel target)
        {
            if(BuffManagerUnity.GetActiveBuffStack(new BattleUnitBuf_BurnBreakAttack(target),target) > 5)
            {
                target.TakeDamage(BuffManagerUnity.GetActiveBuffStack(new BattleUnitBuf_BurnBreakAttack(target), target) * 10);
                BuffManagerUnity.DestroyBuff(new BattleUnitBuf_BurnBreakAttack(target), target);
            }
            else
            {
                BuffManagerUnity.AddStackBuff(new BattleUnitBuf_BurnBreakAttack(this.owner), this.owner, 5);
                BuffManagerUnity.DestroyAllBuffs(this.owner, BufPositiveType.Positive);
            }
        }
    }

**书页Buff**  

该部分由于都是浅封装，且和角色Buff原理几乎一致，这里只提供具体方法声明  

**每个方法都提供了card实例和LorId的两种重载方式，LorId为施加CardBuff的书页lorid，model为书页的持有者，buf为具体的书页Buff类**  

`void AddCardBuff(BattleDiceCardBuf buf, BattleUnitModel model, BattleDiceCardModel card)` 添加书页Buff  

`void AddCardBuff(BattleDiceCardBuf buf, BattleUnitModel model, LorId lorid)` **重载** 

`void DestoryCardBuff(BattleDiceCardBuf buf, BattleUnitModel model, BattleDiceCardModel card)` 摧毁书页Buff(书页Buff无层数)  

`void DestoryCardBuff(BattleDiceCardBuf buf, BattleUnitModel model, LorId lorid)` **重载**  

`void TransferCardBuf(BattleDiceCardBuf buf, BattleDiceCardModel card, BattleUnitModel from, BattleUnitModel to)` 转移书页Buff  

`void TransferCardBuf(BattleDiceCardBuf buf, LorId lorid, BattleUnitModel from, BattleUnitModel to)` **重载**  

### 成员变量和私有成员

`private static BattleUnitBuf AddBuf(BattleUnitBuf buf, BattleUnitModel model)` 统一的**角色Buff**施加函数，用于进行初始化层数和接口判断  

`private static bool _isInit = false;` 是否开启高级功能标签  

### 接口

接口是BuffManagerUnity规定的一种格式规范，只要实现了该格式规范，那么该功能就会被自动执行  

**书页最大上限接口**
`public interface IMaxStack
{
    int MaxStack { get; set; }
}`

接口名：**IMaxStack**，接口属性：**MaxStack**  

只要你的Buf类中规定了书页上限接口，那么该属性的值就会成为后续BuffManagerUnity处理层数时候的Buf层数上限，无需重写带有上限的Buf添加接口  
**无该接口则默认为无上限限制**  

使用示例: 

    public class BattleUnitBuf_BurnBreakAttack : BattleUnitBuf , IMaxStack //这里需要加上接口名称
    {
        //初始化
        public BattleUnitBuf_BurnBreakAttack(BattleUnitModel model)
        {
            this._owner = model;
            this.stack = 0;
            BuffManagerUnity.LoadImage(this, "breakburn"); // 加载初始图片
        }

        public override void OnRoundEnd()
        {
            if(this.stack > 10)
                BuffManagerUnity.LoadImage(this, "breakburn2"); // 切换图片
        }

        //"属性名称"必须和"接口属性"对应 - 为 MaxStack !!!

        public int MaxStack { get; set; } = 8; // 声明接口实现，值为8，即该Buff层数上限为8 
    }

这样就可以实现一个上限为8的Buf类  

**Buff持续回合数接口** - ***必须启用高级功能才可正常运作!!!***
`
public interface IInsistRound
{
   int InsistRound { get; set; }
}
`

接口名：**IInsistRound**，接口属性：**InsistRound**  

只要你的Buf类中实现了该接口，并且启用了Patch，那么这个Buff持续时间只能为InsistRound的回合数，时间一到自动销毁，修改层数不影响销毁时间. **InsistRound**需要大于0  

**由于函数特殊性，未启用高级功能不会触发Warning和Error，也不会有任何日志提示，也不会产生其它任何异常情况。若发现Buff未能正常销毁，请检查是否开启高级功能**  

使用示例: 

    public class BattleUnitBuf_BurnBreakAttack : BattleUnitBuf , IMaxStack , IInsistRound //可以同时启用多个接口
    {
        //初始化
        public BattleUnitBuf_BurnBreakAttack(BattleUnitModel model)
        {
            this._owner = model;
            this.stack = 0;
            BuffManagerUnity.LoadImage(this, "breakburn"); // 加载初始图片
        }

        public override void OnRoundEnd()
        {
            if(this.stack > 10)
                BuffManagerUnity.LoadImage(this, "breakburn2"); // 切换图片
        }

        //"属性名称"必须和"接口属性"对应 - 为 MaxStack !!!

        public int MaxStack { get; set; } = 8; // 声明接口实现，值为8，即该Buff层数上限为8 

        //"属性名称"必须和"接口属性"对应 - 为 InsistRound !!!

        public int InsistRound { get; set; } = 2;
    }

这样，我们就得到了一个上限为8，且2回合后自动销毁的Buf类  


# CardAllyManagerUnity

该类用于管理书页相关信息，书页的添加、检查和销毁均由该类负责  

类内提供了一些简便的查找方式和一些高级的使用方式  

## 方法和函数

`void EnableAdvancedFunction()` **启用高级功能**

`void EnableOnlyPage()` **必须启用高级功能** 启用专属书页设置，如果你的角色需要使用专属书页，那么就需要设置好xml文档后在Initalize(初始化类)中进行调用  
**注册Harmony时候的packageid即为加载packageid模组的专属书页**  

使用示例: 

    public class IndexInitializer : ModInitializer
    {
        //初始化函数
        public override void OnInitializeMod()
        {
            GamesLoader.DeleteLog();
            GamesLoader.RegisterHarmony("Study");
            GamesLoader.SetImageLoadPath(GamesLoader.GetDllPath() + "/ArtWork");

            CardAllyManagerUnity.EnableAdvancedFunction(); // 启用高级功能
            CardAllyManagerUnity.EnableOnlyPage(); // 必须在注册Harmony并启用高级功能后
        }
    }

`BattleDiceCardModel GetTopCard(BattleUnitModel model,BattleDiceCardModel card)` 在 model 的牌组中查找与 `card` 匹配的第一张书页，找不到返回 null

`BattleDiceCardModel GetTopCard(BattleUnitModel model, LorId lorid)` **重载**获取符合lorid匹配的第一张书页

`BattleDiceCardModel GetCostMinCard(BattleUnitModel model)` 获取model的费用最小的第一张书页

`BattleDiceCardModel GetCostMaxCard(BattleUnitModel model)` 获取model的费用最大的第一张书页

`int GetCostReduction(BattleUnitModel model, BattleDiceCardModel card)` 获取model手中第一张card书页的费用减少值 (正数为降费，负数为增费)  

`List<BattleDiceCardModel> GetCostCardList(BattleUnitModel model,int cost)` 获取一个List，里面为符合cost光芒花费的所有书页  

`void AddFormatCard(BattleUnitModel model, LorId lorid,  int insistRound = 0, CardAbility ability = CardAbility.temp ,CardList cardlist = CardList.normallist, BattleDiceCardBuf startbuf = null)` **需要启用高级功能，不启用高级功能也可以使用，但若不启用高级功能，insistRound均为无效参数，若参数大于0，则可能触发Warning**  
对model对象添加一个lorid:xxx的书页，该书页于insistRound回合后自毁，该书页的为ability类型的书页，且添加入cardlist中，为该书页添加一个书页buff - startbuf  
**insistRound = 0，默认为不会自毁**  

**由于egolist不具备抽牌功能，permanent类型使用后会重新回到egolist，temp类型使用后不会重新回到egolist**  

**枚举**

**CardList** - **normallist** 书页栏 , **egolist** Ego栏  

**CardAbility** - **temp** 临时书页不洗入牌库循环 , **permanent** 永久书页洗入牌库循环  

使用示例:

    public override void OnSucceedAttack(BattleUnitModel target)
    {
        CardAllyManagerUnity.AddFormatCard(target, new LorId("DawnOffice", 4)); // 在target书页栏内添加符合LorId的临时书页
        //在target书页栏内添加符合LorId的书页，洗入牌库，且添加后3回合后自毁，书页具备BattleDiceCardBuf_BurnBreak类的书页Buff
        CardAllyManagerUnity.AddFormatCard(target, new LorId("DawnOffice", 5), 3, CardAbility.permanent, CardList.normallist, new BattleDiceCardBuf_BurnBreak());
    }

## 私有变量

`private static bool _isInit = false;` 高级功能启用标签  

`private static List<Pair> _modelcardlist = new List<Pair>();` 存储会自毁的书页  


# CharacterModelUnity

这是整个工具里面目前最复杂且功能最强大的类

该类可以在战斗中查询角色信息，修改角色属性，也可以充当注册器对原版和其它mod的书页的数值进行修改  

**属性注册全程使用反射字段进行，不需要进行Patch，因此不涉及功能开启方法，可直接使用**  

- [阵营查询](#基础功能---阵营查询)  
- [属性注册](#高级功能---属性注册修改)  
- [Operator](#操作类-operator)  
- [CoreCardOperator](#核心书页注册类-corecardoperator-注册修改)
- [BattleCardOperator](#战斗书页注册类-battlecardoperator-注册修改)
- [LinkCover](#linkcover-连接覆盖修改)
- [Operator运作流程](#operator-运作流程)  
- [示例](#operator注册示例)  

## 函数和方法

### 基础功能 - 阵营查询

该部分提供了一些浅封装来快捷获得阵营数据  

`Faction GetSelfFaction(BattleUnitModel unit)` 获取unit己方阵营的Faction  

`Faction GetEnemyFaction(BattleUnitModel unit)` 获取unit的敌对阵营的Faction (不用再写三目运算符了)

`List<BattleUnitModel> GetFactionAliveModel(Faction faction)` 获取faction阵营的存活角色列表  

`List<BattleUnitModel> GetFactionModel(Faction faction)` 获取faction阵营的角色列表(存活+死亡)  

`int GetAliveListNumber(Faction faction)` 获取faction阵营的存活人数  

`int GetListNumber(Faction faction)` 获取faction阵营的总人数 (存活+死亡)  

`int GetDeadListNumber(Faction faction)` 获取faction阵营的死亡人数

使用示例: 

    public override void OnRoundStart()
    {
        if (CharacterModelUnity.GetDeadListNumber(this.owner.faction) > 3 && CharacterModelUnity.GetAliveListNumber(CharacterModelUnity.GetEnemyFaction(this.owner)) > 1)
        {
            this.owner.bufListDetail.AddKeywordBufByEtc(KeywordBuf.Strength, 3);
            foreach (BattleUnitModel model in CharacterModelUnity.GetFactionAliveModel(CharacterModelUnity.GetEnemyFaction(this.owner)))
                model.bufListDetail.AddKeywordBufByEtc(KeywordBuf.Weak, 3);
        }
    }

回合开始时，若己方死亡人数大于3且敌方存活人数大于1，则自身获得3层强壮，敌方全体存活角色获得3层虚弱  

### 高级功能 - 属性注册修改

**不需要Patch**  

尽管这个注册函数也就几个，但是起来有一定难度，不同的注册操作都会有自己的结果  

`void EnableRegisterOperation()` 开始注册操作，应该在**所有注册项填写完成**后调用，仅能调用一次  

`void SetModelValue(BattleUnitModel model, object value, OperationValue op)` **不完整的方法!谨慎使用** 修改局内model的数值为value，执行操作类型为op，在战斗内调用  
**注意value类型为object，请不要传入非法的值!**  

| OperationValue枚举 | 用途 |
| Hp | 当前生命值 |
| MaxHp | 最大生命值 |
| CurLight | 当前光芒 |
| RecoveryLight | 每回合回复光芒 |

`Operator RegisterOperation()` 注册一个操作类，操作类用于存放后续的注册操作 **返回值为注册的操作类**  

`Operator.CoreCardOperator CreateCoreCardOperator(Operator operation = null)` 注册一个核心书页操作类，参数为绑定的操作类，默认为不绑定任何操作类 **返回值为注册的核心书页操作类**  

`Operator.BattleCardOperator CreateBattleCardOperator(Operator operation = null)` 注册一个战斗书页操作类，参数为绑定的操作类，默认为不绑定任何操作类 **返回值为注册的战斗书页操作类**  

**尽管注册功能的函数很少，但流程可拼装性极强，值得单独说明**  

## Operator注册

### 操作类 Operator

Operator为执行注册操作的操作类，组合了3个List和2个内部的书页操作类  

| Operator类内变量 | 功能 |
| ------------- | ------------- |
| **`List<CoreCardOperator>` _corecardOperator** | 用于存放核心书页注册类**CoreCardOperator**的列表 |
| **`List<BattleCardOperator>` _cardoperator** | 用于存放战斗书页注册类**BattleCardOperator**的列表 |
| **`List<OperationRegister>` _rigisterOperation** | 用于存放操作符**OperationRegister**的列表 |

在调用`EnableRegisterOperation()`后，所有注册的`Operator`类都会根据`OperationRegister`操作符的指示，将`_corecardOperator`和`_cardoperator`里面的书页操作类全部执行，完成注册  

注册执行流程如下:
`EnableRegisterOperation()` -> `遍历_operateList (用于存放Operator的列表)` -> `根据每个Operator的 _rigisterOperation 进行操作` -> `遍历每个Operator的 _corecardOperator 和 _cardoperator` -> `独立将每个CoreCardOperator和BattleCardOperator操作的属性进行注册`  

**CharacterModelUnity**支持多个**Operator**，每个**Operator**支持多个**CoreCardOperator**和**BattleCardOperator**，根据层级遍历完成批量注册操作  

执行注册操作的必要流程: 
`注册Operator op` -> `创建CoreCardOperator/BattleCardOperator cardop` -> `填写cardop中的修改值` -> `将cardop绑定到op内部 (如果你创建cardop时候填写了op参数，那么会自动挂载到op上，无需手动添加)` -> `调用开始注册函数执行操作`  

***由于注册操作需要在所有xml加载后立刻执行，于是注册操作必须写在Initialize (初始化类)中***  

如果你需要立刻查看操作方式，请点击[示例代码](#operator注册示例)  

### Register 注册修改  

Register注册修改是在cardop里面填入需要修改的属性，然后对op(Operator)填入操作符 **RegisterCoreBook(注册核心书页)**或者**RegisterCard(注册修改战斗书页)**后进行修改  

Register注册修改仅修改在cardop中编辑过的属性，未编辑过的属性不会修改  

一个op支持填入多个操作符，如果使用 **CoreCardOperator 注册修改** 则需要添加 **RegisterCoreBook** 操作符，如果使用 **BattleCardOperator 注册修改** 则需要添加 **RegisterCard**操作符

### 核心书页注册类 CoreCardOperator 注册修改 

CoreCardOperator为后续执行核心书页注册的基本操作类，隶属于Operator的内部类，装载了可以执行的操作字段  
**填写过的字段才会进行注册，默认情况下，不填写字段意味着该属性保留原值**  

| CoreCardOperator字段 | 功能 | 备注 |
| -------------------  | ---- | --- |
| **Record** | **记录** | **忘了谁都不能忘了这个!** |
| _goalLorId | 修改的书页 | LorId (必须添加，否则无修改目标) | 
| _rigsterId | 模板的书页 | LorId (可选添加，用于LinkCover) |
| **Attrbiute** | **核心书页的基础属性** | **类型和备注** |
| _maxhp | 生命值 | int | 
| _maxbp | 混乱值 | int |
| _minSpeed | 最小速度 | int |
| _maxSpeed | 最大速度 | int |
| _deadLine | 死亡线 | int |
| _passivePointLimit | 未知功能 | int |
| _speedDiceNumber | 初始速度骰子数 | int |
| _maxPlayLight | 起始最大光芒 | int |
| _startPlayLight | 起始光芒 | int (由于开局回光，起始光芒为 _startPlayLight + 1)|
| _addStartDraw | 开局额外抽取书页 | int |
| _SlashResist | 斩击抗性 | `AtkResist`(枚举) |
| _SlashBreakResist| 斩击混乱抗性 | `AtkResist` |
| _PenetrateResist | 突刺抗性 | `AtkResist` |
| _PenetrateBreakResist| 突刺混乱抗性 | `AtkResist` |
| _HitResist | 打击抗性 | `AtkResist` |
| _HitBreakResist| 打击混乱抗性 | `AtkResist` |
| **Vis** | **清理标识符** | **默认值** |
| _ClearOriginPassive | 清除原本所拥有的所有被动 | false |
| _ClearOriginDeck | 清除原本所拥有的书页栏 | false |
| _ClearOnlyCard | 清除原本所拥有的所有专属书页 | false |
| **Operate** | **操作列表** | **类型和备注** |
| _AddPassive | 额外添加的被动条 | `List<LorId>`在原有基础上添加新的被动  | 
| _DeletePassive | 删除原有的被动条 | `List<LorId>`在原有基础上删除目标被动 |
| _AddOnlyCard | 额外添加的专属书页 | `List<LorId>`在原有基础上添加专属书页  | 
| _DeleteOnlyCard | 删除原有的专属书页 | `List<LorId>`在原有基础上删除专属书页 |
| _AddCardDeck | 额外添加入牌库的书页 | `List<LorId>`在原有牌库中添加新的书页 | 
| _DeleteCardDeck | 删除原有牌库的书页 | `List<LorId>`在原有牌库中删除目标书页 |

一个CoreCardOperator的对象cardop有上述属性，仅需要填写你所需要修改的属性即可，**默认为不修改**  

**一个CoreCardOperator修改1张核心书页，如果需要修改N张核心书页，那么需要创建N个核心书页对象**  

### 战斗书页注册类 BattleCardOperator 注册修改 

| BattleCardOperator字段 | 功能 | 备注 |
| -------------------  | ---- | --- |
| **Record** | **记录** | **忘了谁都不能忘了这个!** |
| _goalLorId | 修改的书页 | LorId (必须添加，否则无修改目标) | 
| _rigsterId | 模板的书页 | LorId (可选添加，用于LinkCover) |
| **Attrbiute** | **战斗书页的基础属性** | **类型和备注** |
| _cost | 书页光芒花费 | int |
| _chapter | 书页所属的章节 | int 0~7 [传闻 ~ 杂质] (不知道月计发啥颠，不用枚举) |
| _priority | 书页使用优先级 | int (仅来宾生效) |
| _script | 书页使用的书页效果id | string |
| _desc | 书页效果的描述 | string |
| _artwork | 书页的卡面图像 | string |
| **Operator** | **操作列表** | **类型和备注** |
| _Addkeywords | 添加的关键字标签 | List<string> |
| _Deletekeywords | 删除的关键字标签 | List<string> |
| **vis** | **核心书页的基础属性** | **默认值** |
| _clearOriginKeywords | 清空原有的关键字标签 | false |

**由于战斗骰子列表过于特殊，暂时没做出对战斗骰子的注册修改，仅支持LinkCover**  

### LinkCover 连接覆盖修改
LinkCover也是最简单粗暴的修改方式，其不是通过指定一个属性条进行修改，而是通过和另一张可以识别的核心书页或者战斗书页进行连接，将目标书页换做指定的书页，目标和模板公用一套整体  
而LinkCover使用起来也更加简单，你只需要提供一个可以图书馆识别的书页的LorId，然后就载入操作符即可  

**A B,目标:A，模板:B -> B-B(连接共用,A替换为B)**

如果你需要连接覆盖修改核心书页，那么只需要将:  
**CoreCardOperator**中的**_rigsterId**设置为模板书页的LorId，然后**_goalLorId**设置为修改书页的LorId，载入操作符**LinkCoverCoreBook**运行即可  

**由于LinkCover会将A B书页设置为同一个引用对象，所以后续对A B书页的rigister会同时改变A B书页的属性，建议单独另写一份xml书页设置用作LinkCover**  

如果你需要连接覆盖修改战斗书页，那么只需要将：
**BattleCardOperator**中的**_rigsterId**设置为模板书页的LorId，然后**_goalLorId**设置为修改书页的LorId，载入操作符**LinkCoverXmlCard**运行即可  

**战斗书页也会将A B书页设置为同一个引用对象**  

**由于书页骰子并没有设置一个注册修改方法，我们只能通过LinkCover进行修改，这需要注意：**  
***LinkCoverXmlCard会覆盖书页的大部分属性，包括:费用，品质，骰子列表等(不包括章节)，所以可以通过LinkCoverXmlCard来修改书页骰子***  

但有些时候我们只希望修改书页骰子，不希望修改书页本身，那么可以使用: **LinkCoverDiceList**  
在`BattleCardOperator`中设置好_rigsterId，然后使用`LinkCoverDiceList`操作符，那么就可以只连接覆盖掉目标书页的骰子列表，而不是整张书页  

但同属于LinkCover，这样也有一个问题，那就是目标书页的骰子列表和模板书页的骰子列表均属于一个引用，修改其中的一方列表会导致双方一起改变  

### Operator 运作流程

由于Operator下有5个`OperationRegister`操作符，Operator支持多个操作符同时导入，以下是操作符负责的模块的处理顺序

`LinkCoverCoreBook` -> `RegisterCoreBook` -> `LinkCoverXmlCard` -> `RegisterCard` -> `LinkCoverDiceList`  

也就是说，先进行LinkCover再进行Register，那么可以在LinkCover的基础上再进行Register，完成进一步的修改 **这个Register的修改会修改双方**  

最后就是一个Operator的简单使用列表

|目的|OperationRegister|使用的操作类|需要修改的字段|
|---- |---- |---- | ----|
|仅修改核心书页的几个数值| RegisterCoreBook | CoreCardOperator | _goalLorId(必须) + 其它字段的value |
| 替换整个核心书页 | LinkCoverCoreBook | CoreCardOperator | _goalLorId和_rigisterId |
| 修改战斗书页的费用等 | RegisterCard | BattleCardOperator | _goalLorId(必须) + 其它字段的value |
| 替换整张战斗书页 | LinkCoverXmlCard | BattleCardOperator | _goalLorId和_rigisterId |
| 仅替换战斗书页的骰子 | LinkCoverDiceList | BattleCardOperator | _goalLorId和_rigisterId |

### Operator注册示例

    //初始化类
    public class IndexInitializer : ModInitializer
    {
        //初始化函数
        public override void OnInitializeMod()
        {
            GamesLoader.DeleteLog();
            GamesLoader.RegisterHarmony("Study");
            GamesLoader.SetImageLoadPath(GamesLoader.GetDllPath() + "/ArtWork");

            // 注册Operation
            var op = CharacterModelUnity.RegisterOperation();

            // Operator 1 - RegisterCoreBook

            // 创建一个CoreCardOperator绑定到op上
            var corecardop = CharacterModelUnity.CreateCoreCardOperator(op);
            // 修改目标 - 原版102号书页(漆黑静默之页) 
            corecardop._goalLorId = new LorId(GamesLoader.orginal, 102);
            // 添加修改属性
            corecardop._maxhp = 143;
            corecardop._startPlayLight = 5;
            corecardop._maxPlayLight = 5;
            corecardop._passivePointLimit = 30;
            corecardop._AddPassive.Add(new LorId(GamesLoader.orginal, 200004));
            corecardop._AddPassive.Add(new LorId(GamesLoader.orginal, 200005));


            // Operator 2 - LinkCoverCoreBook

            // 创建一个CoreCardOperator绑定到op上
            var corecardop2 = CharacterModelUnity.CreateCoreCardOperator(op);
            // 修改目标 - 原版的8号书页(Binah之页)
            corecardop2._goalLorId = new LorId(GamesLoader.orginal, 8);
            // 使用LinkCover，用102号核心书页覆盖8号核心书页
            corecardop2._rigisterId = new LorId(GamesLoader.orginal, 102);


            // Operator 3 - RegisterCard

            // 创建一个BattleCardOperator绑定到op上
            var cardop = CharacterModelUnity.CreateBattleCardOperator(op);
            // 修改目标 - 原版的702008号书页(逻辑工作室)
            cardop._goalLorId = new LorId(GamesLoader.orginal, 702008);
            // 添加修改属性
            cardop._rarity = Rarity.Unique;
            cardop._cost = 4;
            cardop._script = "otherAlly2strength1thisRound";


            // Operator 4 - LinkCoverXmlCard

            // 创建一个BattleCardOperator绑定到op上
            var cardop2 = CharacterModelUnity.CreateBattleCardOperator(op);
            // 修改目标 - 原版的702002号书页(轮盘重工)
            cardop2._goalLorId = new LorId(GamesLoader.orginal, 702002);
            // 使用LinkCover，用原版的706201号书页(锁)来覆盖702002号书页
            cardop2._rigisterId = new LorId(GamesLoader.orginal, 702008);

            // Operator 5 - LinkCoverDiceList

            // 创建一个BattleCardOperator绑定到op上
            var cardop3 = CharacterModelUnity.CreateBattleCardOperator(op);
            // 修改目标 - 原版的706203书页(震击)
            cardop3._goalLorId = new LorId(GamesLoader.orginal, 706203);
            // 使用LinkCoverDiceList，只用原版的705012号书页(落幕终曲)的骰子来覆盖706203书页的骰子
            cardop3._rigisterId = new LorId(GamesLoader.orginal, 705012);


            // 添加5个操作符 ...
            op._rigisterOperation.Add(CharacterModelUnity.OperationRegister.RegisterCard);
            op._rigisterOperation.Add(CharacterModelUnity.OperationRegister.RegisterCoreBook);
            op._rigisterOperation.Add(CharacterModelUnity.OperationRegister.LinkCoverCoreBook);
            op._rigisterOperation.Add(CharacterModelUnity.OperationRegister.LinkCoverXmlCard);
            op._rigisterOperation.Add(CharacterModelUnity.OperationRegister.LinkCoverDiceList);
            
            // 开始注册
            CharacterModelUnity.EnableRegisterOperation();
        }
    }

**如果你实际去运行了这段代码，你会发现Operator 1 ~ 4都是正常的，但Operator 5似乎不对劲，整个震击都被替换成了落幕终曲，这是为什么呢？**  

**因为我们Operator会执行载入的操作符的所有流程!!! 我们已经载入了LinkCoverXmlCard，那么这个op就会把所有的BattleCardOperator都执行LinkCover，这也包括我们的Operator 5**  

**并非是LinkCoverDiceList有错误，而是LinkCoverXmlCard会替换整张书页，包括骰子列表和其它属性，因此我们需要重新定义一个op2，然后op2放入LinkCoverDiceList而不是LinkCoverXmlCard，之后把我们的Operator 5的挂载对象换做op2，这也就可以正常运作了!**  

你也可以把**GamesLoader.orginal**换做自己mod或者其它mod的**packageid**，这也就可以把目标书页替换到自制mod的书页，反之同理  

### 不太推荐但确实省事的方法

如果你觉得每次都需要确定运算符太麻烦了，你可以直接同时载入前四个运算符，这样这个op对象就可以执行所有的操作(只更换骰子列表除外)  

但Operator内部有很多道检查，如果放入运算符但没有设定目标，程序会正常运行但是会在日志里面打印一堆**Warning**而不是**Error**  

如果你并不在意日志，那么随意，但如果你在意日志的干净，那么投入无用的运算符会产生几个甚至几十条**Warning**(数量随op数量，和op挂载的类而决定)，我相信你不会喜欢这种场面的  

# DataManagerUnity

DataManagerUnity类是用来存储一些数据和资源的类，目前仅支持跨舞台存储

由于原版使用的是floorbattleparam，每个角色只有3个int类型的字段，不利于批量跨舞台存储和加载，所以DataManagerUnity出现了  

## 函数和方法

**该类所有功能都需要在Intialize(初始化类)调用此函数才可生效**

`void EnableAdvancedFunction()` 启用高级功能  

`void SaveData(string id,object value)` 储存一个value，一个id作为该value的ID  

`object GetData(string id)` 获取id对应的ID的value  

`void DeleteData(string id)` 删除id对应的ID的value  

**重点: 由于存储的内容是object，存储时可以省略，但取用时候一定需要注明强制类型转换**  

## 私有方法和成员变量

`private static Dictionary<string, object> _dataDict = new Dictionary<string, object>();` 存储数据的字典  

`private static bool _isInit = false;` 是否启用高级功能的标志  

**使用示例:**

    public class PassiveAbility_Test : PassiveAbilityBase
    {
        public override void OnWaveStart()
        {
            if (DataManagerUnity.GetData("bb") != null)
            {
                BuffManagerUnity.AddStackBuff(new BattleUnitBuf_BurnBreakAttack(this.owner), this.owner , (int)DataManagerUnity.GetData("bb"));
            }
        }

        public override void OnRoundEnd()
        {
            BuffManagerUnity.AddStackBuff(new BattleUnitBuf_BurnBreakAttack(this.owner), this.owner, 10);
            DataManagerUnity.SaveData("bb",BuffManagerUnity.GetActiveBuffStack(new BattleUnitBuf_BurnBreakAttack(this.owner), this.owner));
        }
    }

这个示例在回合结束时存储了一个value，id为bb，然后在舞台开始时候读取了id为bb的value，用作buff层数继承  

**值得一提的是，存储value的字典是全局的，一个id只能对应一个value，并且所有人都可以访问这个id，如果需要多个角色分别存储，需要设置不同的id进行存储和读取**  

# 注意事项

- 启用专属书页是启用这个mod的专属战斗书页，而并非加载这个mod核心书页规定的专属战斗书页。如果你的mod是A，你需要用到的是B的mod的专属战斗书页，你需要启用的是B的启用专属书页而并非自己mod的启用专属书页  

- 部分未完成的函数内可能存在bug，请注意使用  

- 遇到的其它bug也可以**issue**我  

- 如果可以，你可以在Initailize里面用一个毫无意义的字符串来填写我的github仓库地址作为支持  