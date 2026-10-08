using HarmonyLib;
using HyperCard;
using Sd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DevelopTools
{
    public static partial class CardAllyManagerUnity
    {
        // Base Function

        // ========= Patch ========

        // 是否启用专属书页
        public static void EnableOnlyPage()
        {
            if (_isInit)
                return;
            if (GamesLoader.harmony == null)
                return;

            GamesLoader.harmony.PatchAll(typeof(EnableOnlyPage_Patch));
            _isInit = true;
        }

        // 是否启用高级功能
        public static void EnableAdvancedFunction()
        {
            GamesLoader.harmony.PatchAll(typeof(OnRoundEnd_Patch));
            GamesLoader.harmony.PatchAll(typeof(OnWaveStart_Patch));
        }




        // ========= Card Operator ============

            // 获取符合条件的第一张书页
        public static BattleDiceCardModel GetTopCard(BattleUnitModel model,BattleDiceCardModel card)
        {
            foreach (BattleDiceCardModel tempcard in model.allyCardDetail.GetAllDeck())
            {
                if (tempcard == card)
                {
                    return tempcard;
                }
            }
            return null;
        }

        public static BattleDiceCardModel GetTopCard(BattleUnitModel model, LorId lorid)
        {
            foreach (BattleDiceCardModel tempcard in model.allyCardDetail.GetAllDeck())
            {
                if (tempcard.GetID() == lorid)
                {
                    return tempcard;
                }
            }
            return null;
        }

        // 获取最小费用的第一张书页
        public static BattleDiceCardModel GetCostMinCard(BattleUnitModel model)
        {
            var deck = model.allyCardDetail.GetAllDeck();
            int lowestcost = 10;
            int index = 0;
            for (int i = 0; i < deck.Count; i++)
            {
                if (deck[i].GetCost() < lowestcost)
                {
                    lowestcost = deck[i].GetCost();
                    index = i;
                }
            }

            return deck[index];
        }

        // 获取最大费用的第一张书页
        public static BattleDiceCardModel GetCostMaxCard(BattleUnitModel model)
        {
            var deck = model.allyCardDetail.GetAllDeck();
            int lowestcost = -1;
            int index = 0;
            for (int i = 0; i < deck.Count; i++)
            {
                if (deck[i].GetCost() > lowestcost)
                {
                    lowestcost = deck[i].GetCost();
                    index = i;
                }
            }

            return deck[index];
        }

        // 获取减少的书页费用
        public static int GetCostReduction(BattleUnitModel model, BattleDiceCardModel card)
        {
            return -(GetTopCard(model, card).GetCost() - GetTopCard(model, card).GetOriginCost());
        }

        // 获取指定费用的书页列表
        public static List<BattleDiceCardModel> GetCostCardList(BattleUnitModel model,int cost)
        {
            List<BattleDiceCardModel> list = new List<BattleDiceCardModel>();
            foreach (BattleDiceCardModel tempcard in model.allyCardDetail.GetAllDeck())
            {
                if (tempcard.GetCost() == cost)
                {
                    list.Add(tempcard);
                }
            }

            return list;
        }

        private static bool _isInit = false;
    }
}
