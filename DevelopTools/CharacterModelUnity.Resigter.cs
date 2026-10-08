using HarmonyLib;
using LOR_DiceSystem;
using LOR_XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.GraphicsBuffer;

namespace DevelopTools
{

    public static partial class CharacterModelUnity
    {
        public static void EnableRegisterOperation()
        {
            RegisterCoreBook();
        }

        public static BookEquipEffect AdvancedRegisterTool(LorId lorid)
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
        }

        // 修改目标属性 - 局内
        public static void SetModelValue(BattleUnitModel model, object value, OperationValue op)
        {
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
        public static Operator RegisterOperation(LorId lorid)
        {
            if (_isInit == false)
                _isInit = true;

            Operator op = new Operator();
            op._lorId = lorid;
            _operateList.Add(op);
            return op;
        }

        public static void RegisterCoreBook()
        {
            foreach (Operator op in _operateList)
            {
                BookEquipEffect equipEffect;
                List<LorId> equipDeck;

                // 初始化操作
                if (op._lorId.packageId == GamesLoader.orginal)
                {
                    equipEffect = Singleton<BookXmlList>.Instance.GetData(op._lorId.id).EquipEffect;
                    equipDeck = Singleton<DeckXmlList>.Instance.GetData(op._lorId.id).cardIdList;
                }
                else
                {
                    equipEffect = Singleton<BookXmlList>.Instance.GetData(op._lorId).EquipEffect;
                    equipDeck = Singleton<DeckXmlList>.Instance.GetData(op._lorId).cardIdList;
                }

                // 核心书页替换
                if (op._rigisterOperation.Contains(OperationRegister.LinkCoverCoreBook) == true)
                {
                    equipEffect = Singleton<BookXmlList>.Instance.GetData(op._rigsterId, true).EquipEffect;
                    equipDeck = Singleton<DeckXmlList>.Instance.GetData(op._rigsterId).cardIdList;

                    if (op._lorId.packageId == GamesLoader.orginal)
                    {
                        Singleton<BookXmlList>.Instance.GetData(op._lorId.id).EquipEffect = equipEffect;
                        Singleton<DeckXmlList>.Instance.GetData(op._lorId.id).cardIdList = equipDeck;
                    }
                    else
                    {
                        Singleton<BookXmlList>.Instance.GetData(op._lorId).EquipEffect = equipEffect;
                        Singleton<DeckXmlList>.Instance.GetData(op._lorId).cardIdList = equipDeck;
                    }

                }

                // 核心书页注入
                if (op._rigisterOperation.Contains(OperationRegister.RegisterCoreBook) == true)
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

                // RegisterCard
                RegisterBattleCard(op);
            }
        }

        public static void RegisterBattleCard(Operator op)
        {
            if (op._cardoperator == null)
                return;

            foreach (var cardop in op._cardoperator)
            {
                DiceCardXmlInfo card;
                List<DiceBehaviour> dicelist;

                var field = ItemXmlDataList.instance.GetType().GetField("_cardInfoTable", AccessTools.all);
                if (field == null)
                    continue;

                var dict = (Dictionary<LorId, DiceCardXmlInfo>)field.GetValue(ItemXmlDataList.instance);

                if (cardop._lorId.packageId == GamesLoader.orginal)
                {
                    card = ItemXmlDataList.instance.GetCardItem(cardop._lorId.id);
                    dicelist = ItemXmlDataList.instance.GetCardItem(cardop._lorId.id).DiceBehaviourList;
                }
                else
                {
                    card = ItemXmlDataList.instance.GetCardItem(cardop._lorId);
                    dicelist = ItemXmlDataList.instance.GetCardItem(cardop._lorId).DiceBehaviourList;
                }

                // 战斗书页替换
                if (op._rigisterOperation.Contains(OperationRegister.LinkCoverXmlCard) == true)
                {
                    card = ItemXmlDataList.instance.GetCardItem(cardop._rigsterId);

                    dict[cardop._lorId] = card;
                }

                if (op._rigisterOperation.Contains(OperationRegister.RegisterCard) == true)
                {
                    if (cardop._cost > -1)
                    {
                        card.Spec.Cost = cardop._cost;
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

                    dict[cardop._lorId] = card;
                }


                // Register Dice
                if (cardop._rigsterId != null && op._rigisterOperation.Contains(OperationRegister.LinkCoverDiceList) == true)
                {
                    dicelist = ItemXmlDataList.instance.GetCardItem(cardop._rigsterId).DiceBehaviourList;
                }

                if (op._lorId.packageId == GamesLoader.orginal)
                {
                    ItemXmlDataList.instance.GetCardItem(cardop._lorId.id).DiceBehaviourList = dicelist;
                }
                else
                {
                    ItemXmlDataList.instance.GetCardItem(cardop._lorId).DiceBehaviourList = dicelist;
                }

            }
        }
        // =========== enum ============

        public enum OperationValue
        {
            Hp,
            MaxHp,
            CurLight,
            RecoveryLight
        }

        public class Operator
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

            // CardList
            public List<CardOperator> _cardoperator = new List<CardOperator>();

            // Operator Sign
            public List<OperationRegister> _rigisterOperation = new List<OperationRegister>();

            // Record
            public LorId _lorId;
            public LorId _rigsterId;


            // 书页注入
            public class CardOperator
            {
                // Record
                public LorId _lorId;

                // Attribute
                public int _cost = -1;
                public string _script;
                public string _desc;
                public string _artwork;
                public Rarity _rarity = (Rarity)(-1);
                public CardRange _style = (CardRange)(-1);

                // Operator - Unenable
                // public List<DiceOperator> _AddDiceBehaviour = new List<DiceOperator>();
                // public List<DiceOperator> _DeleteDiceBehaviour = new List<DiceOperator>();

                // Operator
                public LorId _rigsterId;
                public List<DiceBehaviour> list = null;
            }

            // 骰子注入 - Unenable
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

        public enum OperationRegister
        {
            RegisterCoreBook, // 注册核心书页 - 优先级1
            LinkCoverCoreBook, // 切换核心书页 - 优先级2
            RegisterCard,  // 注册战斗书页 - 优先级1 (战斗书页骰子列表目前不支持注册)
            LinkCoverXmlCard, // 切换战斗书页 - 优先级2
            LinkCoverDiceList // 切换战斗书页骰子列表 - 优先级3 (切换战斗书页后无需再次切换战斗书页骰子列表)
        }

        private static List<Operator> _operateList = new List<Operator>();
        private static bool _isInit = false;
    }
}
