using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.League;

namespace WarbandWarfareQuestline.Questline
{
    public class QuestPart_VillageLooted : QuestPartActivable
    {
        private int ticks = 0;
        public MinorFactionSettlement questSettlement;
        public MinorFaction faction;

        void FinishQuest()
        {
            quest.End(QuestEndOutcome.Success);
        }

        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get
            {
                yield return questSettlement;
            }
        }

        public override void PreQuestAccept()
        {
            base.PreQuestAccept();
            this.Enable(new SignalArgs());
            questSettlement = faction.GenerateSettlementOccupied();

        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
        }

        public override void QuestPartTick()
        {
            base.QuestPartTick();
            ticks++;
            if(ticks > 5000)
            {
                FinishQuest();
            }
        }
    }
}
