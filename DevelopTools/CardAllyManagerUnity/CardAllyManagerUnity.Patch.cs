using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LOR_DiceSystem;
using LOR_XML;

namespace DevelopTools
{
    public static partial class CardAllyManagerUnity
    {
        [HarmonyPatch(typeof(BookModel), "SetXmlInfo")]

        public static class EnableOnlyPage_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(BookModel __instance, BookXmlInfo ____classInfo, ref List<DiceCardXmlInfo> ____onlyCards)
            {
                foreach (int id in ____classInfo.EquipEffect.OnlyCard)
                {
                    DiceCardXmlInfo cardItem = ItemXmlDataList.instance.GetCardItem(new LorId(GamesLoader._packageid, id), false);
                    ____onlyCards.Add(cardItem);
                }
            }
        }

        [HarmonyPatch(typeof(StageController), "RoundEndPhase_WaitForEffect")]
        public static class OnRoundEnd_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(bool __result)
            {
                if (__result)
                    CardAllyManagerUnity.OnRoundEnd();
            }
        }

        [HarmonyPatch(typeof(StageController), nameof(StageController.StartBattle))]
        public static class OnWaveStart_Patch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                CardAllyManagerUnity.OnWaveStart();
            }
        }


    }
}
