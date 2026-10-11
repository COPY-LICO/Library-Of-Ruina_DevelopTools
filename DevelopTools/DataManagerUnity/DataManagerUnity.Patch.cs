using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopTools
{
    public static partial class DataManagerUnity
    {
        [HarmonyPatch(typeof(StageController), nameof(StageController.GameOver))]
        public static class OnInvitionEnd_Patch
        { 
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (Singleton<StageController>.Instance.Phase == StageController.StagePhase.TeleportPurpleTear)
                    return;

                DataManagerUnity.OnInvitionEnd();
            }
        }
    }
}
