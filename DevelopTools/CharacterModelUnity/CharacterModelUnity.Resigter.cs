using HarmonyLib;
using HyperCard;
using LOR_DiceSystem;
using LOR_XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevelopTools
{
    public static partial class CharacterModelUnity
    {

        public static void EnableRegisterOperation()
        {
            if (_isRigster == true)
            {
                GamesLoader.Log("Can not Register Again");
                return;
            }

            foreach (Operator operation in _operateList)
            {
                RegisterCoreBook(operation);
                RegisterBattleCard(operation);
            }
            
            _operateList.Clear();
            GamesLoader.Log("Info: Register Successfully");
        }

        /*public static BookEquipEffect AdvancedRegisterTool(LorId lorid)
        {
            BookEquipEffect equipEffect;
            if (lorid.packageId == GamesLoader.orginal)
            {
                equipEffect = Singleton<BookXmlList>.Instance.GetData(lorid.id).EquipEffect;
            }
            else
            {
                equipEffect = Singleton<BookXmlList>.Instance.GetData(lorid).EquipEffect;
            }

            return equipEffect;
        }*/

        // 修改目标属性 - 局内
        public static void SetModelValue(BattleUnitModel model, object value, OperationValue op)
        {
            GamesLoader.Log("Warning: SetModeValue - is NOT a complete function");

            if (op == OperationValue.Hp)
            {
                model.UnitData.hp = (float)value;
            }
            else if (op == OperationValue.MaxHp)
            {
                typeof(BookModel).GetField("_maxHp", AccessTools.all).SetValue(model.Book, value);
                if (model.hp > model.MaxHp)
                    model.SetHp((int)value);
            }
            else if (op == OperationValue.CurLight)
            {
                model.cardSlotDetail.SetPlayPoint((int)value);
            }
            else if (op == OperationValue.RecoveryLight)
            {
                model.cardSlotDetail.SetRecoverPoint((int)value);
            }
            //...........
        }

        // 修改目标属性 - 游戏外
        public static Operator RegisterOperation()
        {
            Operator op = new Operator();
            _operateList.Add(op);
            return op;
        }

        public static Operator.CoreCardOperator CreateCoreCardOperator(Operator operation = null)
        {
            var op = new Operator.CoreCardOperator();
            if (operation != null)
                operation._corecardOperator.Add(op);
            return op;
        }

        public static Operator.BattleCardOperator CreateBattleCardOperator(Operator operation = null)
        {
            var op = new Operator.BattleCardOperator();
            if (operation != null)
                operation._cardoperator.Add(op);
            return op;
        }

        private static void RegisterCoreBook(Operator operation)
        {
            if (operation._corecardOperator == null)
            {
                GamesLoader.Log("Warning: CoreCardOperator is NULL");
                return;
            }

            if (_isRigster == false)
                _isRigster = true;

            // RegisterCoreCard
            foreach (Operator.CoreCardOperator op in operation._corecardOperator)
            {
                if (op._goalLorId == null)
                {
                    GamesLoader.Log("Error: CoreCardOperator Has Invaild goalLorId!");
                    continue;
                }

                BookEquipEffect equipEffect = null;
                    List<LorId> equipDeck = null;
                    // 初始化操作
                    if (op._goalLorId.packageId == GamesLoader.orginal)
                    {
                        equipEffect = Singleton<BookXmlList>.Instance.GetData(op._goalLorId.id).EquipEffect;
                        equipDeck = Singleton<DeckXmlList>.Instance.GetData(op._goalLorId.id).cardIdList;
                    }
                    else
                    {
                        equipEffect = Singleton<BookXmlList>.Instance.GetData(op._goalLorId).EquipEffect;
                        equipDeck = Singleton<DeckXmlList>.Instance.GetData(op._goalLorId).cardIdList;
                    }

                    // 核心书页替换
                    if (operation._rigisterOperation.Contains(OperationRegister.LinkCoverCoreBook) == true && op._rigsterId != null)
                    {
                        equipEffect = Singleton<BookXmlList>.Instance.GetData(op._rigsterId, true).EquipEffect;
                        equipDeck = Singleton<DeckXmlList>.Instance.GetData(op._rigsterId).cardIdList;

                        if (op._goalLorId.packageId == GamesLoader.orginal)
                        {
                            Singleton<BookXmlList>.Instance.GetData(op._goalLorId.id).EquipEffect = equipEffect;
                            Singleton<DeckXmlList>.Instance.GetData(op._goalLorId.id).cardIdList = equipDeck;
                        }
                        else
                        {
                            Singleton<BookXmlList>.Instance.GetData(op._goalLorId).EquipEffect = equipEffect;
                            Singleton<DeckXmlList>.Instance.GetData(op._goalLorId).cardIdList = equipDeck;
                        }

                    }
                    else if (operation._rigisterOperation.Contains(OperationRegister.LinkCoverCoreBook) == true && op._rigsterId == null)
                    {
                        GamesLoader.Log("Warning: Choose LinkCoverCoreBook But There is not a vaild rigsterId");
                    }

                    // 核心书页注入
                    if (operation._rigisterOperation.Contains(OperationRegister.RegisterCoreBook) == true)
                    {
                        // 清理原版属性
                        if (op._ClearOriginPassive == true)
                        {
                            equipEffect.PassiveList.Clear();
                        }
                        if (op._ClearOnlyCard == true)
                        {
                            equipEffect.OnlyCard.Clear();
                        }
                        if (op._ClearOriginDeck == true)
                        {
                            equipDeck.Clear();
                        }


                        // 注册修改属性
                        // Part One int
                        if (op._maxhp > 0)
                        {
                            equipEffect.Hp = op._maxhp;
                        }
                        if (op._maxbp > 0)
                        {
                            equipEffect.Break = op._maxbp;
                        }
                        if (op._minSpeed > 0)
                        {
                            equipEffect.SpeedMin = op._minSpeed;
                        }
                        if (op._maxSpeed > 0)
                        {
                            equipEffect.Speed = op._maxSpeed;
                        }
                        if (op._deadLine > 0)
                        {
                            equipEffect.DeadLine = op._deadLine;
                        }
                        if (op._passivePointLimit > 0)
                        {
                            equipEffect.PassiveCost = op._passivePointLimit;
                        }
                        if (op._speedDiceNumber > 0)
                        {
                            equipEffect.SpeedDiceNum = op._speedDiceNumber;
                        }
                        if (op._maxPlayLight > 0)
                        {
                            equipEffect.MaxPlayPoint = op._maxPlayLight;
                        }
                        if (op._startPlayLight > 0)
                        {
                            equipEffect.StartPlayPoint = op._startPlayLight;
                        }
                        if (op._addStartDraw > 0)
                        {
                            equipEffect.AddedStartDraw = op._addStartDraw;
                        }

                        // Part Two Resist
                        if ((int)op._SlashResist > -1)
                        {
                            equipEffect.SResist = op._SlashResist;
                        }
                        if ((int)op._PenetrateResist > -1)
                        {
                            equipEffect.PResist = op._PenetrateResist;
                        }
                        if ((int)op._HitResist > -1)
                        {
                            equipEffect.HResist = op._HitResist;
                        }
                        if ((int)op._SlashBreakResist > -1)
                        {
                            equipEffect.SBResist = op._SlashBreakResist;
                        }
                        if ((int)op._PenetrateBreakResist > -1)
                        {
                            equipEffect.PBResist = op._PenetrateBreakResist;
                        }
                        if ((int)op._HitBreakResist > -1)
                        {
                            equipEffect.HBResist = op._HitBreakResist;
                        }

                        // Part Three Operator
                        if (op._AddPassive.Count != 0)
                        {
                            foreach (var item in op._AddPassive)
                            {
                                equipEffect.PassiveList.Add(item);
                            }
                        }
                        if (op._DeletePassive.Count != 0)
                        {
                            foreach (var item in op._DeletePassive)
                            {
                                equipEffect.PassiveList.Remove(item);
                            }
                        }
                        if (op._AddCardDeck.Count != 0)
                        {
                            foreach (var item in op._AddCardDeck)
                            {
                                equipDeck.Add(item);
                            }
                        }
                        if (op._DeleteCardDeck.Count != 0)
                        {
                            foreach (var item in op._DeleteCardDeck)
                            {
                                equipDeck.Remove(item);
                            }
                        }
                        if (op._AddOnlyCard.Count != 0)
                        {
                            foreach (var item in op._AddOnlyCard)
                            {
                                equipEffect.OnlyCard.Add(item.id);
                            }
                        }
                        if (op._DeleteOnlyCard.Count > 0)
                        {
                            foreach (var item in op._DeleteOnlyCard)
                            {
                                equipEffect.OnlyCard.Remove(item.id);
                            }
                        }
                    }
                }

        }

        private static void RegisterBattleCard(Operator operation)
        {
            if (operation._cardoperator == null)
                return;

            if (_isRigster == false)
                _isRigster = true;

            foreach (Operator.BattleCardOperator cardop in operation._cardoperator)
            {
                if (cardop._goalLorId == null)
                {
                    GamesLoader.Log("Error: CardOperator Has Invaild goalLorId!");
                    continue;
                }

                DiceCardXmlInfo card;
                List<DiceBehaviour> dicelist;

                var field = ItemXmlDataList.instance.GetType().GetField("_cardInfoTable", AccessTools.all);
                if (field == null)
                    continue;

                var dict = (Dictionary<LorId, DiceCardXmlInfo>)field.GetValue(ItemXmlDataList.instance);

                if (cardop._goalLorId.packageId == GamesLoader.orginal)
                {
                    card = ItemXmlDataList.instance.GetCardItem(cardop._goalLorId.id);
                    dicelist = ItemXmlDataList.instance.GetCardItem(cardop._goalLorId.id).DiceBehaviourList;
                }
                else
                {
                    card = ItemXmlDataList.instance.GetCardItem(cardop._goalLorId);
                    dicelist = ItemXmlDataList.instance.GetCardItem(cardop._goalLorId).DiceBehaviourList;
                }

                // 战斗书页替换
                if (operation._rigisterOperation.Contains(OperationRegister.LinkCoverXmlCard) == true && cardop._rigsterId != null)
                {
                    card = ItemXmlDataList.instance.GetCardItem(cardop._rigsterId);

                    dict[cardop._goalLorId] = card;
                }
                else if(operation._rigisterOperation.Contains(OperationRegister.LinkCoverXmlCard) == true && cardop._rigsterId == null)
                {
                    GamesLoader.Log("Warning: Choose LinkCoverXmlCard But There is not a vaild rigsterId");
                }

                if (operation._rigisterOperation.Contains(OperationRegister.RegisterCard) == true)
                {
                    if(cardop._clearOriginKeywords == true)
                    {
                        card.Keywords.Clear();
                    }

                    if (cardop._cost > -1)
                    {
                        card.Spec.Cost = cardop._cost;
                    }
                    if (cardop._chapter > -1)
                    {
                        card.Chapter = cardop._chapter;
                    }
                    if (cardop._priority > -1)
                    {
                        card.Priority = cardop._priority;
                    }
                    if (cardop._script != null)
                    {
                        card.Script = cardop._script;
                    }
                    if (cardop._desc != null)
                    {
                        card.ScriptDesc = cardop._desc;
                    }
                    if (cardop._artwork != null)
                    {
                        card.Artwork = cardop._artwork;
                    }
                    if (cardop._rarity != (Rarity)(-1))
                    {
                        card.Rarity = cardop._rarity;
                    }
                    if (cardop._style != (CardRange)(-1))
                    {
                        card.Spec.Ranged = cardop._style;
                    }

                    if (cardop._Addkeywords.Count > 0)
                    {
                        foreach (var keyword in cardop._Addkeywords)
                        {
                            card.Keywords.Add(keyword);
                        }
                    }
                    if (cardop._Deletekeywords.Count > 0)
                    {
                        foreach (var keyword in cardop._Deletekeywords)
                        {
                            card.Keywords.Remove(keyword);
                        }
                    }

                    dict[cardop._goalLorId] = card;
                }


                // Register Dice
                if (cardop._rigsterId != null && operation._rigisterOperation.Contains(OperationRegister.LinkCoverDiceList) == true)
                {
                    dicelist = ItemXmlDataList.instance.GetCardItem(cardop._rigsterId).DiceBehaviourList;
                }

                if (cardop._goalLorId.packageId == GamesLoader.orginal)
                {
                    ItemXmlDataList.instance.GetCardItem(cardop._goalLorId.id).DiceBehaviourList = dicelist;
                }
                else
                {
                    ItemXmlDataList.instance.GetCardItem(cardop._goalLorId).DiceBehaviourList = dicelist;
                }

            }
        }

        public class Operator
        {

            // CoreCardList
            public List<CoreCardOperator> _corecardOperator = new List<CoreCardOperator>();
            // CardList
            public List<BattleCardOperator> _cardoperator = new List<BattleCardOperator>();
            // Operator Sign
            public List<OperationRegister> _rigisterOperation = new List<OperationRegister>();

            // 核心书页注入
            public class CoreCardOperator
            {
                // Attrbiute
                public int _maxhp;
                public int _maxbp;
                public int _minSpeed;
                public int _maxSpeed;
                public int _deadLine;
                public int _passivePointLimit;
                public int _speedDiceNumber;
                public int _maxPlayLight;
                public int _startPlayLight;
                public int _addStartDraw;

                public AtkResist _SlashResist = (AtkResist)(-1);
                public AtkResist _PenetrateResist = (AtkResist)(-1);
                public AtkResist _HitResist = (AtkResist)(-1);
                public AtkResist _SlashBreakResist = (AtkResist)(-1);
                public AtkResist _PenetrateBreakResist = (AtkResist)(-1);
                public AtkResist _HitBreakResist = (AtkResist)(-1);

                // Operate
                public List<LorId> _AddPassive = new List<LorId>();
                public List<LorId> _DeletePassive = new List<LorId>();
                public List<LorId> _AddOnlyCard = new List<LorId>();
                public List<LorId> _DeleteOnlyCard = new List<LorId>();
                public List<LorId> _AddCardDeck = new List<LorId>();
                public List<LorId> _DeleteCardDeck = new List<LorId>();

                // Vis
                public bool _ClearOriginPassive = false;
                public bool _ClearOriginDeck = false;
                public bool _ClearOnlyCard = false;

                // Record
                public LorId _goalLorId;
                public LorId _rigsterId;
            }

            // 书页注入
            public class BattleCardOperator
            {
                // Record
                public LorId _goalLorId;
                public LorId _rigsterId;

                // Attribute
                public int _cost = -1;
                public int _chapter = -1;
                public int _priority = -1;
                public string _script;
                public string _desc;
                public string _artwork;
                public Rarity _rarity = (Rarity)(-1);
                public CardRange _style = (CardRange)(-1);

                // Keywords
                public List<string> _Addkeywords = new List<string>();
                public List<string> _Deletekeywords = new List<string>();

                // vis
                public bool _clearOriginKeywords = false;

                // Operator - Unenabled
                // public List<DiceOperator> _AddDiceBehaviour = new List<DiceOperator>();
                // public List<DiceOperator> _DeleteDiceBehaviour = new List<DiceOperator>();

                // Operator
                // public List<DiceBehaviour> _dicelist = null;
            }

            // 骰子注入 - Unenabled
            /*
            public class DiceOperator
            {
                // Attribute
                public int _min;
                public int _max;
                public string _script;
                public string _actionscript;
                public string _effectres;
                public string _desc;
                public BehaviourType _type = (BehaviourType)(-1);
                public BehaviourDetail _detail = (BehaviourDetail)(-1);

            }*/
        }
        // =========== enum ============

        public enum OperationRegister
        {
            RegisterCoreBook, // 注册核心书页 - 优先级1
            LinkCoverCoreBook, // 切换核心书页 - 优先级2
            RegisterCard,  // 注册战斗书页 - 优先级1 (战斗书页骰子列表目前不支持注册)
            LinkCoverXmlCard, // 切换战斗书页 - 优先级2
            LinkCoverDiceList // 切换战斗书页骰子列表 - 优先级3 (切换战斗书页后无需再次切换战斗书页骰子列表)
        }

        public enum OperationValue
        {
            Hp,
            MaxHp,
            CurLight,
            RecoveryLight
        }

        private static List<Operator> _operateList = new List<Operator>();
        private static bool _isRigster = false;
    }
}
