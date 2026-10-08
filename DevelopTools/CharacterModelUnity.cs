using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace DevelopTools
{
    public static partial class CharacterModelUnity
    {
        // 获取己方阵营标志
        public static Faction GetSelfFaction(BattleUnitModel unit)
        {
            return unit.faction;
        }

        // 获取敌方阵营标志
        public static Faction GetEnemyFaction(BattleUnitModel unit)
        {
            return unit.faction == Faction.Enemy ? Faction.Player : Faction.Enemy;
        }

        // 获取存活角色列表
        public static List<BattleUnitModel> GetFactionAliveModel(Faction faction)
        {
            return BattleObjectManager.instance.GetAliveList(faction);
        }

        // 获取角色列表
        public static List<BattleUnitModel> GetFactionModel(Faction faction)
        {
            return BattleObjectManager.instance.GetList(faction);
        }

        // 获取存活人数
        public static int GetAliveListNumber(Faction faction)
        {
            return BattleObjectManager.instance.GetAliveList(faction).Count;
        }

        // 获取人数
        public static int GetListNumber(Faction faction)
        {
            return BattleObjectManager.instance.GetList(faction).Count;
        }

        // 获取死亡人数
        public static int GetDeadListNumber(Faction faction)
        {
            return BattleObjectManager.instance.GetList(faction).Count - BattleObjectManager.instance.GetAliveList(faction).Count;
        }

       // private static bool _isPatch = false;
    }
}
