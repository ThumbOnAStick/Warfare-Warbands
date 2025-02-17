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
        public string factionID;
        public MinorFactionSettlement questSettlement;
        public MinorFaction faction;
        private readonly int _forceQuestEndTicks = 5 * GenDate.TicksPerDay;

        public MinorFaction Faction
        {
            get
            {
                if(faction == null)
                {
                    faction = questSettlement.MinorFaction;
                }
                return faction;
            }
        }

        void FinishQuest()
        {
            quest.End(QuestEndOutcome.Success);
            quest.GetFirstOrAddPart<QuestPart_Choice>()?.choices?.First()?.rewards?.First()?.Notify_Used();
            questSettlement?.SetFaction(Find.FactionManager.OfPlayer);

        }

        void FailQuest()
        {
            quest.End(QuestEndOutcome.Fail);
        }

        void CheckHostility()
        {
            if (!GenHostility.AnyHostileActiveThreatToPlayer(this.questSettlement.Map))
            {
                FinishQuest();
            }
        }

        bool IsTimeOut()
        {
            return GenTicks.TicksGame > this.EnableTick + _forceQuestEndTicks;
        }

        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get
            {
                yield return questSettlement;
            }
        }

        void VillageTick()
        {
            if (this.questSettlement.HasMap)
            {
                CheckHostility();
            }
            else if (IsTimeOut())
            {
                FailQuest();
            }
        }

        public override void PreQuestAccept()
        {
            base.PreQuestAccept();
            this.Enable(new SignalArgs());
            questSettlement = Faction.GenerateSettlementOccupied();

        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
        }

        public override void QuestPartTick()
        {
            base.QuestPartTick();

            // Check village
            VillageTick();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.questSettlement, "questSettlement");
            Scribe_References.Look(ref this.faction, "faction");
            Scribe_Values.Look(ref this.factionID, "factionID");
        }
    }
}
