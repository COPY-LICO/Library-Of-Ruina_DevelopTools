using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LOR_DiceSystem;
using LOR_XML;
using System.IO;

namespace DevelopTools
{

    public static partial class CharacterModelUnity
    {
        public static void EnableRegisterOperation()
        {
            foreach (Operator op in _operateList)
            {
                BookEquipEffect equipEffect;
                if (op._lorId.packageId == GamesLoader.orginal)
                {
                    equipEffect = Singleton<BookXmlList>.Instance.GetData(op._lorId.id).EquipEffect;
                }
                else
                {
                    equipEffect = Singleton<BookXmlList>.Instance.GetData(op._lorId).EquipEffect;
                }

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
                if( (int)op._SlashResist > -1)
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
                if (op._Passive.Count != 0)
                {
                    foreach (var item in op._Passive)
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
            }
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
                if(model.hp > model.MaxHp)
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
            public int _maxhp;
            public int _maxbp;
            public int _minSpeed;
            public int _maxSpeed;
            public AtkResist _SlashResist = (AtkResist)(-1);
            public AtkResist _PenetrateResist = (AtkResist)(-1);
            public AtkResist _HitResist = (AtkResist)(-1);
            public AtkResist _SlashBreakResist = (AtkResist)(-1);
            public AtkResist _PenetrateBreakResist = (AtkResist)(-1);
            public AtkResist _HitBreakResist = (AtkResist)(-1);
            public List<LorId> _Passive = new List<LorId>();
            public List<LorId> _DeletePassive = new List<LorId>();
            public LorId _lorId;
        }

        private static List<Operator> _operateList = new List<Operator>();
        private static bool _isInit = false;
    }
}
