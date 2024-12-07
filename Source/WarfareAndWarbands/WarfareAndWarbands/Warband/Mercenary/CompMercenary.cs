using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warband
{
    public class CompProperties_Mercenary : CompProperties
    {
        public CompProperties_Mercenary()
        {
            this.compClass = typeof(CompMercenary);
        }
    }

    public class CompMercenary : ThingComp
    {
        bool servesPlayerFaction = false;
        bool retreated = false;
        Faction servingFaction;
        int lastServeTick = 0;
        string pawnKindName;
        Warband warband;
        public CompProperties_Mercenary Props
        {
            get
            {
                return (CompProperties_Mercenary)this.props;
            }
        }

        public Pawn Mercenary
        {
            get
            {
                return (Pawn)this.parent;
            }
        }


        public bool ServesPlayerFaction
        {
            get
            {
                return servesPlayerFaction;
            }
            set
            {
                servesPlayerFaction = value;
            }
        }

        public int LastServeTick
        {
            get
            {
                return lastServeTick;
            }
            set
            {
                lastServeTick = value;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.ServesPlayerFaction)
            {
                if (!Find.CurrentMap.listerThings.AllThings.Any(x => x.def == WAWDefof.WAW_LootChest))
                    yield return WarbandUI.PlaceLootChest(this.warband);
                if (!Mercenary.Downed && !retreated)
                    yield return WarbandUI.RetreatPawn(this);
            }

        }

        public override void Notify_Downed()
        {
            base.Notify_Downed();
            this.warband?.playerWarbandManager?.injuriesManager?.InjurePawn(pawnKindName, GenTicks.TicksGame);

        }
        public string RemainingDays()
        {
            return GenDate.TicksToDays(lastServeTick + serveDuration - Find.TickManager.TicksGame).ToString("0.0");
        }

        public override string CompInspectStringExtra()
        {
            string returnString = "";
            if (this.servesPlayerFaction)
                returnString = "WAW.LeaveIn".Translate(RemainingDays());
            return returnString;
        }

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            if (HasServingFaction() && Mercenary.MapHeld != null)
            {
                TryNotifyPlayerPawnKilled();
                string targetName = pawnKindName;
                this.warband?.TryToRemovePawn(targetName);
            }

        }
        void TryNotifyPlayerPawnKilled()
        {
            if (servesPlayerFaction)
            {
                Messages.Message("WAW.WarbandLoss".Translate(), MessageTypeDefOf.NegativeEvent);
            }
        }

        public void Retreat()
        {
            SetRetreat(true);
            var faction = Find.FactionManager.FirstFactionOfDef(WAWDefof.PlayerWarband);
            if(faction == null)
            {
                Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, true);
            }
            Mercenary.SetFaction(faction);
        }

        public void SetRetreat(bool retreated)
        {
            this.retreated = retreated;
        }

        public void SetWarband(Warband warband)
        {
            this.warband = warband;
        }

        public void ResetDuration()
        {
            this.lastServeTick = GenTicks.TicksGame;
        }


        public void SetServingFaction(Faction f)
        {
            this.servingFaction = f;
        }

        public Faction GetServingFaction()
        {
            return this.servingFaction;
        }

        public bool HasServingFaction()
        {
            return servingFaction != null;
        }

        public void SetPawnKindName(string pawnKindName)
        {
            this.pawnKindName = pawnKindName;
        }

        public Warband GetWarband()
        {
            return this.warband;
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            var inventory = Mercenary.inventory;
            //this.servesPlayerFaction = false;
            //this.servingFaction = null;
            for (int i = 0; i < inventory.innerContainer.Count; i++)
            {
                Thing t = inventory.innerContainer[i];
                this.warband?.Store(ref t);
            }
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            lastServeTick = Find.TickManager.TicksGame;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.servesPlayerFaction)
            {
                if (Find.TickManager.TicksGame - LastServeTick > serveDuration)
                {
                    LastServeTick = Find.TickManager.TicksGame;
                    Retreat();
                }
            }
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref servesPlayerFaction, "servesPlayerFaction", false);
            Scribe_Values.Look(ref retreated, "retreated", false);
            Scribe_Values.Look(ref lastServeTick, "lastServeTick", 0);
            Scribe_Values.Look(ref pawnKindName, "pawnKindName", "none");

            Scribe_References.Look(ref warband, "warband");
            Scribe_References.Look(ref servingFaction, "servingFaction");


        }

        readonly int serveDuration = 60000;

    }
}
