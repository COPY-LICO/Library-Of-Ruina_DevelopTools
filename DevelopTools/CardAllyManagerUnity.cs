using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopTools
{
    public static partial class CardAllyManagerUnity
    {
        // Base Function

        // ========= OnlyPage Start ========
        public static void EnableOnlyPage(string packageid)
        {
            if (_isInit)
                return;

            _packageid = packageid;
            var harmony = new Harmony(packageid);
            harmony.PatchAll(typeof(EnableOnlyPage_Patch));
            _isInit = true;
        }

        private static bool _isInit = false;

        private static string _packageid;
    }
}
