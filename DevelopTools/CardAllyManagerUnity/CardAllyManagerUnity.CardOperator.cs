using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DevelopTools
{
    public static partial class CardAllyManagerUnity
    {
        // ======== run logical =============

        private static void OnRoundEnd()
        {
            for (int i = _modelcardlist.Count - 1; i >= 0; i--)
            {
                Pair pair = _modelcardlist[i];
                pair.insistRound--;
                if (pair.insistRound <= 0)
                {
                    _modelcardlist.Remove(pair);
                    if (pair.liveList == CardList.egolist)
                        pair.card.owner.personalEgoDetail.RemoveCard(pair.card.GetID());
                    else if(pair.liveList == CardList.normallist)
                        pair.card.owner.allyCardDetail.DiscardACardByAbility(pair.card);
                }
            }
        }

        private static void OnWaveEnd()
        {
            for (int i = _modelcardlist.Count - 1; i >= 0; i--)
            {
                Pair pair = _modelcardlist[i];
            }
            _modelcardlist.Clear();
        }

        // ========== Operation Function ============

        // 添加格式化书页
        public static void AddFormatCard(BattleUnitModel model, LorId lorid,  int insistRound = 0, CardAbility ability = CardAbility.temp ,CardList cardlist = CardList.normallist, BattleDiceCardBuf startbuf = null)
        {
            BattleDiceCardModel card = null;
            // 添加槽
            // 添加Buff
            if (startbuf != null)
            {
                card.AddBuf(startbuf);
            }

            if (cardlist == CardList.normallist)
            {
                card = model.allyCardDetail.AddNewCard(lorid);
            }
            else if (cardlist == CardList.egolist)
            {
                model.personalEgoDetail.AddCard(lorid);
                card = model.personalEgoDetail.GetCardAll().Find(x => x.GetID().id == lorid.id);
            }
            else
            {
                return;
            }

            // 是否进入牌库
            if (ability == CardAbility.temp)
            {
                card.exhaust = true;
            }
            else if (ability == CardAbility.permanent)
            {
                card.exhaust = false;
            }

            if (card == null)
            {
                GamesLoader.Log($"Error: Can not get lorid : {lorid} card, card will fallback to default");
            }
             

            // 卡牌存活回合数 - 0为不会自毁，不进入循环
            if(insistRound > 0)
            {
                Pair pair = new Pair(card, model, insistRound,cardlist);
                _modelcardlist.Add(pair);
            }
        }


        public class Pair
        {
            public Pair(BattleDiceCardModel card, BattleUnitModel owner, int round, CardList list)
            {
                this.card = card;
                this.owner = owner;
                this.insistRound = round;
                this.liveList = list;
            }

            public BattleDiceCardModel card;
            public BattleUnitModel owner;
            public int insistRound;
            public CardList liveList;
        }

        // ego和普通书页栏
        private static List<Pair> _modelcardlist = new List<Pair>();

    }

    // ========== Enum ===========
    public enum CardList
    {
        normallist,
        egolist
    }

    public enum CardAbility
    {
        temp,
        permanent
    }

}
