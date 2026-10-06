using HarmonyLib;
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
    public static partial class BuffManagerUnity
    {
        // BaseFunction

        // =========== Init Advanced Function ===========
        public static void ActiveAdvancedFunction(string packageid)
        {
            if (_isInit)
                return;
            var harmony = new Harmony(packageid);
            harmony.PatchAll();
            _isInit = true;
        }


        // =========== Picture Loader =========

        // 图片加载
        public static void LoadImage(BattleUnitBuf buf, string name)
        {
            if (!_artWorks.TryGetValue(name, out var sprite))
            {
                Debug.LogWarning($"[BuffManager] 未找到图片: {name}");
                return;
            }

            typeof(BattleUnitBuf).GetField("_bufIcon", AccessTools.all).SetValue(buf, _artWorks[name]);
            typeof(BattleUnitBuf).GetField("_iconInit", AccessTools.all).SetValue(buf, true);
        }

        // 指定图片文件路径
        public static void SetImageLoadPath(DirectoryInfo dir)
        {
            _artWorks.Clear();
            GetArtWorks(dir);
        }

        // 指定图片文件加载文件夹
        public static void SetImageLoadDict(Dictionary<string, Sprite> artworks)
        {
            _artWorks = artworks;
        }

        // 私有 - 读取图片文件
        private static void GetArtWorks(DirectoryInfo dir)
        {
            if (dir.GetDirectories().Length != 0)
            {
                DirectoryInfo[] directories = dir.GetDirectories();
                for (int i = 0; i < directories.Length; i++)
                {
                    GetArtWorks(directories[i]);
                }
            }
            foreach (System.IO.FileInfo fileInfo in dir.GetFiles())
            {
                Texture2D texture2D = new Texture2D(2, 2);
                texture2D.LoadImage(File.ReadAllBytes(fileInfo.FullName));
                Sprite value = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0f, 0f));
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                _artWorks[fileNameWithoutExtension] = value;
            }
        }


        // =========== Buff Operator =============

        // 激活Buff - 0层数
        // 该函数强制参数buf必须为新建实例
        public static void ActiveBuff(BattleUnitBuf buf, BattleUnitModel model)
        {
            BattleUnitBuf tempBuff = GetActiveBuff(buf, model);
            if (tempBuff == null)
            {
                tempBuff = AddBuf(buf, model);
            }
        }

        // 其余接口均可用已有buf实例，也可用新建buf实例
        // 设置Buff层数
        public static void SetStackBuff(BattleUnitBuf buf, BattleUnitModel model, int stack)
        {
            BattleUnitBuf tempBuff = GetActiveBuff(buf, model);
            if (tempBuff == null)
            {
                tempBuff = AddBuf(buf, model);
            }

            if (tempBuff is IMaxStack maxStack)
            {
                tempBuff.stack = Math.Min(maxStack.MaxStack, stack);
            }
            else
            {
                tempBuff.stack = stack;
            }
        }

        // 添加Buff层数
        public static void AddStackBuff(BattleUnitBuf buf, BattleUnitModel model, int stack)
        {
            BattleUnitBuf tempBuff = GetActiveBuff(buf, model);
            if (tempBuff == null)
            {
                tempBuff = AddBuf(buf, model);
            }

            if (tempBuff is IMaxStack maxStack)
            {
                tempBuff.stack = Math.Min(maxStack.MaxStack, tempBuff.stack + stack);
            }
            else
            {
                tempBuff.stack += stack;
            }
        }

        // 添加Buff - 私有
        private static BattleUnitBuf AddBuf(BattleUnitBuf buf, BattleUnitModel model)
        {
            model.bufListDetail.AddBuf(buf);
            buf.stack = 0;

            if (buf is IInsistRound insistRound)
            {
                _buflist.Add(buf);
            }

            return buf;
            
        }

        // 清除Buff层数
        public static void ClearBuffStack(BattleUnitBuf buf, BattleUnitModel model)
        {
            BattleUnitBuf tempBuff = GetActiveBuff(buf, model);
            if (tempBuff == null)
                return;
            tempBuff.stack = 0;
        }

        // 摧毁Buff
        public static void DestroyBuff(BattleUnitBuf buf, BattleUnitModel model)
        {
            BattleUnitBuf tempBuff = GetActiveBuff(buf, model);
            if (tempBuff == null)
                return;

            if(_buflist.Contains(tempBuff))
                _buflist.Remove(tempBuff);

            tempBuff.Destroy();
        }

        // 摧毁所有激活的Buff
        public static void DestroyAllBuffs(BattleUnitModel model)
        {
            foreach (var t in model.bufListDetail.GetActivatedBufList().ToList())
            {
                if (_buflist.Contains(t))
                    _buflist.Remove(t);
                t.Destroy();
            }
        }

        public static void DestroyAllBuffs(BattleUnitModel model, BufPositiveType type = BufPositiveType.None)
        {
            foreach (var t in model.bufListDetail.GetActivatedBufList().ToList())
            {
                if(t.positiveType == type)
                {
                    if (_buflist.Contains(t))
                        _buflist.Remove(t);

                    t.Destroy();
                }
            }
        }

        public static void DestroyAllBuffs(BattleUnitModel model, BattleUnitBuf buf)
        {
            foreach (var t in model.bufListDetail.GetActivatedBufList().ToList())
            {
                if(t.GetType() == buf.GetType())
                {
                    if (_buflist.Contains(t))
                        _buflist.Remove(t);

                    t.Destroy();
                }
            }
        }

        // 获取激活的Buff实例层数
        public static int GetActiveBuffStack(BattleUnitBuf buf, BattleUnitModel model)
        {
            BattleUnitBuf tempBuff = GetActiveBuff(buf, model);
            if (tempBuff == null)
                return 0;
            else
                return tempBuff.stack;
        }

        // 获取激活的Buff实例
        public static BattleUnitBuf GetActiveBuff(BattleUnitBuf buf, BattleUnitModel model)
        {
            BattleUnitBuf tempBuff = model.bufListDetail.GetActivatedBufList()
                .Find((BattleUnitBuf temp) => temp.GetType() == buf.GetType());
            if (tempBuff == null)
                return null;
            else
                return tempBuff;
        }

        // 转移Buff实例
        public static void TransferBuff(BattleUnitBuf buf, BattleUnitModel from, BattleUnitModel to)
        {
            var active = GetActiveBuff(buf, from);
            if (active == null) 
                return;

            from.bufListDetail.RemoveBuf(active);
            to.bufListDetail.AddBuf(active);

            typeof(BattleUnitBuf).GetField("_owner", AccessTools.all).SetValue(active, to);
        }

        // 复制Buff实例
        public static void CopyToBuff(BattleUnitBuf buf, BattleUnitModel from, BattleUnitModel to)
        {
            var active = GetActiveBuff(buf, from);
            if (active == null)
                return;

            var newBuf = (BattleUnitBuf)Activator.CreateInstance(active.GetType());

            var type = active.GetType();
            while (type != null && type != typeof(object))
            {
                foreach (var field in type.GetFields(BindingFlags.Instance
                                                    | BindingFlags.Public
                                                    | BindingFlags.NonPublic
                                                    | BindingFlags.DeclaredOnly))
                {
                    field.SetValue(newBuf, field.GetValue(active));
                }
                type = type.BaseType;
            }

            typeof(BattleUnitBuf).GetField("_owner", AccessTools.all).SetValue(newBuf, to);

            to.bufListDetail.AddBuf(newBuf);
        }


        // 初始化高级功能
        private static bool _isInit = false;
        // 图片资源
        private static Dictionary<string, Sprite> _artWorks = new Dictionary<string, Sprite>();
    }

    // =========== InterFace =================

    //Buff 最大上限
    public interface IMaxStack
    {
        int MaxStack { get; set; }
    }
}
