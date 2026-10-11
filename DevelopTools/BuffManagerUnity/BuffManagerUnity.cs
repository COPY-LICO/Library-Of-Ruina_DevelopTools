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
        public static void EnableAdvancedFunction()
        {
            if (_isInit)
                return;
            if (GamesLoader.harmony == null)
            {
                GamesLoader.Log("Error: Not Rejister An Availiable Harmony");
                return;
            }

            GamesLoader.harmony.PatchAll(typeof(OnRoundEnd_Patch));
            GamesLoader.harmony.PatchAll(typeof(OnWaveStart_Patch));
            _isInit = true;
            GamesLoader.Log("Info: Successfully Patch BuffManagerUnity");
        }


        // =========== Picture Loader =========

        // 图片加载 - Buff图片资源
        public static void LoadImage(BattleUnitBuf buf, string name)
        {
            if (!FilesLoader._artWorks.TryGetValue(name, out var sprite))
            {
                GamesLoader.Log($"[BuffManager] 未找到图片: {name}");
                return;
            }

            typeof(BattleUnitBuf).GetField("_bufIcon", AccessTools.all).SetValue(buf, FilesLoader._artWorks[name]);
            typeof(BattleUnitBuf).GetField("_iconInit", AccessTools.all).SetValue(buf, true);
        }

        // 图片加载 - 书页Buff图片资源
        public static void LoadImage(BattleDiceCardBuf buf, string name)
        {
            if (!FilesLoader._artWorks.TryGetValue(name, out var sprite))
            {
                GamesLoader.Log($"[BuffManager] 未找到图片: {name}");
                return;
            }

            typeof(BattleDiceCardBuf).GetField("_bufIcon", AccessTools.all).SetValue(buf, FilesLoader._artWorks[name]);
            typeof(BattleDiceCardBuf).GetField("_iconInit", AccessTools.all).SetValue(buf, true);
        }

        // ========= CardBuff Picture Operator =========

        public static void AddCardBuff(BattleDiceCardBuf buf, BattleUnitModel model, BattleDiceCardModel card)
        {
            CardAllyManagerUnity.GetTopCard(model, card).AddBuf(buf);
        }

        public static void AddCardBuff(BattleDiceCardBuf buf, BattleUnitModel model, LorId lorid)
        {
            CardAllyManagerUnity.GetTopCard(model, lorid).AddBuf(buf);
        }

        public static void DestoryCardBuff(BattleDiceCardBuf buf, BattleUnitModel model, BattleDiceCardModel card)
        {
            CardAllyManagerUnity.GetTopCard(model, card).RemoveBuf(buf);
        }

        public static void DestoryCardBuff(BattleDiceCardBuf buf, BattleUnitModel model, LorId lorid)
        {
            CardAllyManagerUnity.GetTopCard(model, lorid).RemoveBuf(buf);
        }

        public static void TransferCardBuf(BattleDiceCardBuf buf, BattleDiceCardModel card, BattleUnitModel from, BattleUnitModel to)
        {
            CardAllyManagerUnity.GetTopCard(from, card).RemoveBuf(buf);
            CardAllyManagerUnity.GetTopCard(to, card).AddBuf(buf);
        }

        public static void TransferCardBuf(BattleDiceCardBuf buf, LorId lorid, BattleUnitModel from, BattleUnitModel to)
        {
            CardAllyManagerUnity.GetTopCard(from, lorid).RemoveBuf(buf);
            CardAllyManagerUnity.GetTopCard(to, lorid).AddBuf(buf);
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
    }

    // =========== InterFace =================

    //Buff 最大上限
    public interface IMaxStack
    {
        int MaxStack { get; set; }
    }
}
