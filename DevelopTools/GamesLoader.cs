using HarmonyLib;
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
    public class GamesLoader
    {
        // 获取dll路径
        public static string GetDllPath()
        {
            return Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
        }

        // 指定图片文件路径
        public static void SetImageLoadPath(string dir)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            FilesLoader._artWorks.Clear();
            FilesLoader.GetArtWorks(dirInfo);
        }

        public static void SetImageLoadPath(DirectoryInfo dir)
        {
            FilesLoader._artWorks.Clear();
            FilesLoader.GetArtWorks(dir);
        }

        // 指定图片文件加载文件夹
        public static void SetImageLoadDict(Dictionary<string, Sprite> artworks)
        {
            FilesLoader._artWorks = artworks;
        }

        // 注册harmony
        public static void RegisterHarmony(string packageid)
        {
            _packageid = packageid;
            harmony = new Harmony(packageid);
        }


        public static Harmony harmony;

        public static string _packageid;

        public static readonly string orginal = "";
    }
}
