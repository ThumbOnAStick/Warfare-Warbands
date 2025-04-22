using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents.Leader;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades;
using static System.Collections.Specialized.BitVector32;

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
        bool _isFromEmpire = false;
        private bool isLeaderCache;
        Faction servingFaction;
        int lastServeTick = 0;
        string pawnKindName; 
        Warband warband;
        private int ServeDuration =>
            this.warband != null && this.warband.playerWarbandManager.upgradeHolder.HasUpgrade ?
            warband.playerWarbandManager.upgradeHolder.SelectedUpgrade.MaintainDays * GenDate.TicksPerDay : 60000;

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

        public bool IsFromEmpire => this._isFromEmpire;


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
                {
                    yield return WarbandUI.RetreatPawn(this);
                    if (!this.IsFromEmpire && Mercenary.MapHeld != null && Mercenary.MapHeld.ParentFaction == Faction.OfPlayer)
                        yield return WarbandUI.RecruitPawn(this);
                    if (warband != null &&
                       warband.playerWarbandManager.upgradeHolder.HasUpgrade &&
                       warband.playerWarbandManager.upgradeHolder.SelectedUpgrade is Upgrade_Vehicle)
                    {
                        yield return WarbandUI.RecycleVehicle(this);
                    }
                }
            }

        }

        public void ResetAll()
        {
            servesPlayerFaction = false;
            retreated = false;
            servingFaction = null;
            lastServeTick = 0;
            pawnKindName = "";
            warband = null;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.isLeaderCache = PlayerWarbandLeaderUtil.IsLeader(Mercenary);
        }


        public override void Notify_Downed()
        {
            base.Notify_Downed();
            if (this.IsFromEmpire)
            {
                return;
            }
            this.warband?.playerWarbandManager?.injuriesManager?.InjurePawn(pawnKindName, GenTicks.TicksGame);
            var faction = Find.FactionManager.FirstFactionOfDef(WAWDefof.PlayerWarband);
            this.Retreat();
        }
        public string RemainingDays()
        {
            return GenDate.TicksToDays(lastServeTick + ServeDuration - Find.TickManager.TicksGame).ToString("0.0");
        }
        void Notify_Killed()
        {
            if (isLeaderCache)
            {
                TryNotifyPlayerLeaderKilled();
                warband?.playerWarbandManager?.leader?.OnLeaderChanged();
            }

            else if (IsFromEmpire)
            {
                TryNotifyPlayerPsyCasterKilled();
            }
            else
            {
                TryKillPlayerMercenary(out bool survived);
                TryNotifyPlayerPawnKilled(survived);
                if (warband.playerWarbandManager.ShouldPlayerWarbandBeRemoved())
                {
                    warband.Destroy();
                }
            }
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
                Notify_Killed();
            }
        }

        void TryKillPlayerMercenary(out bool survived)
        {
            survived = false;
            if (warband != null)
            {
                float chance = warband.playerWarbandManager.RespawnChance;
                float rnd = new FloatRange(0f, 1f).RandomInRange;
                if (chance > rnd)
                {
                    survived = true;
                }
                else
                {
                    this.warband.TryToRemovePawn(pawnKindName);
                }
            }
        }


        void TryNotifyPlayerPawnKilled(bool survived = false)
        {
            if (servesPlayerFaction)
            {
                if (!survived)
                {
                    Messages.Message("WAW.WarbandLoss".Translate(), MessageTypeDefOf.NegativeEvent);
                }
                else if (warband.playerWarbandManager.leader.IsLeaderAvailable())
                {
                    Messages.Message("WAW.WarbandLossFailed".Translate(warband.playerWarbandManager.leader.Leader.NameShortColored), MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        void TryNotifyPlayerLeaderKilled()
        {
            if (servesPlayerFaction)
            {
                Messages.Message("WAW.LeaderKilled".Translate(), MessageTypeDefOf.PawnDeath);
            }
        }

        void TryNotifyPlayerPsyCasterKilled()
        {
            if (Faction.OfEmpire != null)
            {
                Messages.Message("WAW.PsycasterKilled".Translate(Faction.OfEmpire.Name), MessageTypeDefOf.PawnDeath);
                if (warband != null)
                    warband.Faction.TryAffectGoodwillWith(Faction.OfEmpire, -5);
            }
        }

        public void Retreat()
        {
            SetRetreat(true);
            if (servingFaction != Faction.OfPlayer)
            {
                return;
            }
            var faction = Find.FactionManager.FirstFactionOfDef(WAWDefof.PlayerWarband);
            if (faction == null)
            {
                Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, true);
            }
            if (Mercenary.Faction != faction)
                Mercenary.SetFaction(faction);

        }

        public void TryToPromote()
        {
            if (WarbandUtil.TryToSpendSilverFromColonyOrBank(Find.CurrentMap, (int)parent.MarketValue))
            {
                this.ResetAll();
                if (ModsConfig.IdeologyActive)
                {
                    var pIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
                    if (pIdeo != null)
                        Mercenary.ideo?.SetIdeo(pIdeo);
                }
                Mercenary.SetFaction(Faction.OfPlayer);
                Messages.Message("WAW.PromotionSuccess".Translate(Mercenary.NameShortColored), MessageTypeDefOf.PositiveEvent);
            }
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

        public void SetEmpireBackground()
        {
            this._isFromEmpire = true;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.servesPlayerFaction)
            {
                if (Find.TickManager.TicksGame - LastServeTick > ServeDuration)
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
            Scribe_Values.Look(ref _isFromEmpire, "_isFromEmpire", false);
            Scribe_Values.Look(ref lastServeTick, "lastServeTick", 0);
            Scribe_Values.Look(ref pawnKindName, "pawnKindName", "none");
            Scribe_References.Look(ref warband, "warband");
            Scribe_References.Look(ref servingFaction, "servingFaction");


        }


    }
}
