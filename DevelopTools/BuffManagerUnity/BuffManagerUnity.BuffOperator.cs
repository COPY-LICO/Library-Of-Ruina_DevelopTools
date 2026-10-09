using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopTools
{
    public static partial class BuffManagerUnity
    {

        // ======= run logical =======
        private static void OnRoundEnd()
        {
            for (int i = _buflist.Count - 1; i >= 0; i--)
            {
                BattleUnitBuf buf = _buflist[i];

                if(buf == null)
                {
                    _buflist.Remove(buf);
                    continue;
                }

                if (buf is IInsistRound insistRound)
                {
                    insistRound.InsistRound--;
                    if(insistRound.InsistRound <= 0)
                    {
                        _buflist.Remove(buf);
                        buf.Destroy();
                    }
                }
            }
        }

        private static void OnWaveEnd()
        {
            for (int i = _buflist.Count - 1; i >= 0; i--)
            {
                BattleUnitBuf buf = _buflist[i];
                buf.Destroy();
            }
            _buflist.Clear();
        }

        // ======== buf operator ========
        public static int GetBufInsistRound(BattleUnitBuf buf)
        {
            if (buf is IInsistRound insistRound)
            {
                return insistRound.InsistRound;
            }
            return -1;
        }

        private static List<BattleUnitBuf> _buflist = new List<BattleUnitBuf>();
    }

    // ========= InterFace ===========

    //Buff 持续回合
    public interface IInsistRound
    {
        int InsistRound { get; set; }
    }
}
