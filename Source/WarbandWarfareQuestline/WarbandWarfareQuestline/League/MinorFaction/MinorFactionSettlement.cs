using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using WarfareAndWarbands;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warband.VassalWarband;
using WarfareAndWarbands.Warband.VassalWarband.UI;

namespace WarbandWarfareQuestline.League
{
    public class MinorFactionSettlement : MapParent
    {
        private string _nameInt;
        private Material _cachedMat;
        private MinorFaction _minorFaction;
        private VassalHolder _armyHolder;
        private readonly string villageMatPath = "World/WorldObjects/Expanding/MinorFaction_Village";
        private readonly string townMatPath = "World/WorldObjects/Expanding/MinorFaction_Village";

        public string Name
        {
            get
            {
                return this._nameInt;
            }
            set
            {
                this._nameInt = value;
            }
        }

        public MinorFaction MinorFaction => this._minorFaction;
        public string MatPath => this._minorFaction.TechLevel < TechLevel.Industrial? villageMatPath : townMatPath;
        public int Budget => this._minorFaction.TechLevel < TechLevel.Industrial ? 3500 : 5000;
        public float Duration => this._minorFaction.TechLevel < TechLevel.Industrial ? 3.5f : 10f;
        public override Material Material
        {
            get
            {
                if (this._cachedMat == null)
                {
                    Color color = _minorFaction != null ? _minorFaction.FactionColor : Color.white;
                    this._cachedMat = MaterialPool.MatFrom(villageMatPath, ShaderDatabase.WorldOverlayTransparentLit, color, WorldMaterials.WorldObjectRenderQueue);
                }
                return this._cachedMat;
            }
        }
        public override Texture2D ExpandingIcon => WAWTex.Village;
        public override string Label => Name;

        public void SetMinorFaction(MinorFaction minorFaction)
        {
            this._minorFaction = minorFaction;
        }

        #region MapGen
        void DamageBuildings()
        {
            for (int i = 0; i < Map.listerThings.AllThings.Count; i++)
            {
                Thing t = this.Map.listerThings.AllThings[i];

                if (t.def.IsBuildingArtificial && t.def.useHitPoints)
                {
                    if (t.def.CanHaveFaction)
                        t.SetFaction(null);
                    int damage = (int)(t.MaxHitPoints * new FloatRange(0f, 1.5f).RandomInRange);
                    t.TakeDamage(new DamageInfo(DamageDefOf.Flame, damage));
                }
            }
        }

        void GenerateCorpses()
        {
            for (int i = 0; i < 10; i++)
            {
                IntVec3 cell = CellFinder.RandomNotEdgeCell(50, Map);
                MinorFactionBaseUtil.SpawnCorpse(PawnKindDefOf.Villager, cell, Map);
            }
        }

        void GenerateLooters()
        {
            if (this.Faction != null)
            {
                PawnGroupMaker pawnGroupMaker = (from x in Faction.def.pawnGroupMakers
                                                 where x.kindDef == PawnGroupKindDefOf.Combat && x.maxTotalPoints > 1000f
                                                 select x).RandomElement<PawnGroupMaker>();
                float points = Math.Max(StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap), 5000f);
                PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
                {
                    points = points,
                    faction = Faction,
                    groupKind = pawnGroupMaker.kindDef,

                };
                IEnumerable<PawnGenOptionWithXenotype> enumerable = PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(pawnGroupMakerParms.points, pawnGroupMaker.options, pawnGroupMakerParms);
                IEnumerable<Pawn> list = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);
                this.SpawnPawnsNearCenter(list);

            }
        }

        void GenerateLootedBase()
        {
            LongEventHandler.QueueLongEvent(delegate ()
            {
                DamageBuildings();
                GenerateLooters();
                GenerateCorpses();
            }, "WAW.GeneratingRaiders", false, null, true, null);
        }
        #endregion

   
        private void SpawnPawnsNearCenter(IEnumerable<Pawn> pawnList)
        {                
            LordJob_AssaultColony lordJob = new LordJob_AssaultColony(Faction);
            var lord = LordMaker.MakeNewLord(base.Faction, lordJob, base.Map, null);
            foreach (Pawn pawn in pawnList)
            {
                bool flag = !pawn.Spawned;
                if (flag)
                {
                    bool flag2 = CellFinder.TryFindRandomCellNear(base.Map.Center, base.Map, 15, (IntVec3 x) => x.Walkable(base.Map), out IntVec3 loc, -1);
                    if (flag2)
                    {
                        GenSpawn.Spawn(pawn, loc, base.Map, WipeMode.Vanish);
                    }
                    else
                    {
                        GenSpawn.Spawn(pawn, base.Map.Center, base.Map, WipeMode.Vanish);
                    }
                }
                lord.AddPawn(pawn);
            }
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = false;
            return !base.Map.mapPawns.AnyPawnBlockingMapRemoval;
        }

        public override void PostMapGenerate()
        {
            base.PostMapGenerate();
            GenerateLootedBase();
        }


        public override void PostAdd()
        {
            base.PostAdd();
            this._armyHolder = new VassalHolder(Duration, Budget);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            return CaravanArrivalAction_Enter.GetFloatMenuOptions(caravan, this);
        }

        public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
        {
            yield break;
        }

        public override IEnumerable<FloatMenuOption> GetShuttleFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<int, TransportPodsArrivalAction> launchAction)
        {
            yield break;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {

            if (this.Faction == Faction.OfPlayer)
            {
                yield return new Command_Action()
                {
                    action =
                    () =>
                    {
                        CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
                        Find.WorldSelector.ClearSelection();
                        Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(SpawnVassalForPlayer), true);
                    },
                    defaultLabel = "WAW.SpawnVassalCommand".Translate(),
                    defaultDesc = "WAW.SpawnVassalCommand.Desc".Translate(),
                    icon = WAWTex.WarbandTex,
                    Disabled = !this._armyHolder.CanUse(),
                    disabledReason = "WAW.VassalWillBeAvialableIn".Translate(_armyHolder.GetRemainingDays()),
                };
                
            }

        }

        bool SpawnVassalForPlayer(GlobalTargetInfo info)
        {
            if (info.WorldObject != null)
            {
                return false;
            }
            Find.WindowStack.Add(new Window_CreateVassalWarband(info, this._armyHolder));
            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this._nameInt, "nameInt");
            Scribe_References.Look(ref this._minorFaction, "_minorFaction");
            Scribe_Deep.Look(ref this._armyHolder, "_armyHolder");

            if(_armyHolder == null)
            {
                _armyHolder = new VassalHolder(Duration, Budget);
            }
        }

    }
}
