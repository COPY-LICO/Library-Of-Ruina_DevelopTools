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
                    DiceCardXmlInfo cardItem = ItemXmlDataList.instance.GetCardItem(new LorId(_packageid, id), false);
                    ____onlyCards.Add(cardItem);
                }
            }
        }


    }
}
