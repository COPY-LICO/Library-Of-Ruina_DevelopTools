using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopTools
{
    public static partial class BuffManagerUnity
    {
        [HarmonyPatch(typeof(StageController), "RoundEndPhase_WaitForEffect")]
        public static class OnRoundEnd_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(bool __result)
            {
                if(__result)
                    BuffManagerUnity.OnRoundEnd();
            }
        }

        [HarmonyPatch(typeof(StageController), "CloseBattleScene")]
        public static class OnWaveStart_Patch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                  BuffManagerUnity.OnWaveEnd();
            }
        }
    }
}
