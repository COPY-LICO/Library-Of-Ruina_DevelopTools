using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopTools.DataManagerUnity
{
    public static partial class DataManagerUnity
    {
        public static void EnableAdvancedFunction()
        {
            if (_isInit)
                return;
            if (GamesLoader.harmony == null)
            {
                GamesLoader.Log("Error: Not Rejister An Availiable Harmony");
                return;
            }

            GamesLoader.harmony.PatchAll(typeof(OnInvitionEnd_Patch));
            _isInit = true;
            GamesLoader.Log("Info: Successfully Patch DataManagerUnity");
        }

        public static void OnInvitionEnd()
        {
            _dataDict.Clear();
        }

        public static void SaveData(string id,object value)
        {
            _dataDict[id] = value;
        }

        public static object GetData(string id)
        {
            _dataDict.TryGetValue(id, out var value);
            return value;
        }

        public static void DeleteData(string id)
        {
            _dataDict.Remove(id);
        }

        private static Dictionary<string, object> _dataDict = new Dictionary<string, object>();

        private static bool _isInit = false;
    }
}
