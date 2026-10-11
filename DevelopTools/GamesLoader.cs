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
            GamesLoader.Log($"Info: SetImageLoadPath is {dir}");
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
            GamesLoader.Log($"Info: Register A Harmony : {packageid}");
        }

        // 日志
        public static void Log(string msg)
        {
            try
            {
                string path = Path.Combine(GamesLoader.GetDllPath(), "tool_logs.txt");
                File.AppendAllText(path, DateTime.Now.ToString("HH:mm:ss.fff") + "  " + msg + "\n");
            }
            catch { }
        }

        // 删除日志
        public static void DeleteLog()
        {
            string logPath = Path.Combine(GamesLoader.GetDllPath(), "tool_logs.txt");

            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
        }


        public static Harmony harmony;

        public static string _packageid;

        public static readonly string orginal = "";
    }
}
