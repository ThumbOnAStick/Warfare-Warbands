using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Noise;
using Verse.Sound;
using WarfareAndWarbands.Warband.Mercenary;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.WarbandComponents;
using static System.Net.Mime.MediaTypeNames;

namespace WarfareAndWarbands.Warband
{
    public class Warband : Site
    {
        public Dictionary<string, int> bandMembers;
        public NPCWarbandManager npcWarbandManager;
        public PlayerWarbandManager playerWarbandManager;
        private List<string> _stringBuffers;
        private List<int> _intBuffers;
        private FactionDef _pawnKindFactionType;
        private bool isSettling = false;
        private string customName = "Warband";
        public static readonly int playerAttackRange = 10;

     
        public Warband()
        {
            bandMembers = new Dictionary<string, int>();
            npcWarbandManager = new NPCWarbandManager(this);
            playerWarbandManager = new PlayerWarbandManager(this);
        }
        public override Color ExpandingIconColor => this.Faction.Color;
        public override string Label => 
            this.customName != "Warband" ?
            this.customName :
            (this.def.label + "(" + this.Faction.Name + ")");

        public FactionDef PawnKindFactionType => _pawnKindFactionType;

        public override Texture2D ExpandingIcon
        {
            get
            {
                if (this.playerWarbandManager == null || !this.playerWarbandManager.upgradeHolder.HasUpgrade)
                {
                    return WAWTex.WarbandTex;
                }
                else
                {
                    return this.playerWarbandManager.upgradeHolder.TextureOverride();
                }
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            this.forceRemoveWorldObjectWhenMapRemoved = false;
            if (this.bandMembers.Count < 1)
            {
                if (this.Faction.IsPlayer)
                {
                    GeneratePlayerCombatGroup();
                }
                else
                {
                    GenerateNPCCombatGroup();
                }
            }
            WarbandUtil.RefreshAllPlayerWarbands();
        }

        public void SetFactionType(FactionDef type)
        {
            Log.Message("faction type set");
            this._pawnKindFactionType = type;   
        }

        public void SetMembers(Dictionary<string, int> members)
        {
            this.bandMembers = new Dictionary<string, int>(members);
        }

        public void SetCustomName(string name)
        {
            this.customName = name;
        }

        public string GetCustomName()
        {
            return this.customName;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            if (this.Faction != Faction.OfPlayer && !this.Faction.HostileTo(Faction.OfPlayer))
            {
                yield break;
            }
            else if (this.Faction == Faction.OfPlayer)
            {
                foreach (FloatMenuOption floatMenuOption2 in WarbandUI.PlayerWarbandLeaderChoices(this, caravan))
                {
                    yield return floatMenuOption2;
                }
                yield break;
            }
            foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_Enter.GetFloatMenuOptions(caravan, this))
            {
                yield return floatMenuOption2;
            }
            foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_VisitSite.GetFloatMenuOptions(caravan, this))
            {
                yield return floatMenuOption2;
            }

        }

        public void GeneratePlayerCombatGroup()
        {
            this.bandMembers = new Dictionary<string, int>(GameComponent_WAW.playerWarband.bandMembers);
        }

        public override string GetInspectString()
        {
            var outString = "Members:";
            foreach (var member in bandMembers)
            {
                if (member.Value > 0)
                    outString += "\n" + WarbandUtil.GetSoldierLabel(member.Key) + "(" + member.Value + ")";
            }
            if (npcWarbandManager != null && npcWarbandManager.HasTargetingFaction())
            {
                outString += "\n" + "WAW.TargetingMapParent".Translate(npcWarbandManager.TryGetTarget().Label);

            }

            if (this.Faction == Faction.OfPlayer)
            {
                outString = playerWarbandManager.GetInspectString();
            }

            return outString;
        }

        public bool CanPlayerWarbandBeRaidded()
        {
            if (this.playerWarbandManager.injuriesManager != null &&
                !this.playerWarbandManager.injuriesManager.GetActiveMembers(bandMembers).Any(x => x.Value > 0))
                return false;
            if (this.playerWarbandManager.leader.Leader != null &&
               this.playerWarbandManager.leader.Leader.Spawned)
                return false;
            return true;
        }

        public Dictionary<string, int> GenerateNPCCombatGroup()
        {
            bandMembers = npcWarbandManager.GenerateNPCCombatGroup();
            return bandMembers;
        }

        public Dictionary<string, int> GenerateNPCCombatGroup(List<Pawn> pawns)
        {
            bandMembers = npcWarbandManager.GenerateNPCCombatGroup(pawns);
            return bandMembers;
        }
        public override void PostMapGenerate()
        {
            this.playerWarbandManager?.cooldownManager?.SetLastRaidTick();
            SpawnPawns();
        }

        public override void Destroy()
        {
            base.Destroy();
            WarbandUtil.RefreshAllPlayerWarbands();
        }



        public override void Tick()
        {

            base.Tick();
            npcWarbandManager?.Tick();
            playerWarbandManager?.Tick();
            RemoveMapCheck();
            SiteCheck();

        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            bool result = base.ShouldRemoveMapNow(out alsoRemoveWorldObject);
            if (this.Faction == Faction.OfPlayer)
            {
                result = !GenHostility.AnyHostileActiveThreatToPlayer(this.Map, false, false) && result;
            }
            alsoRemoveWorldObject = this.Faction != Faction.OfPlayer;
            return result;
        }


        public void ResettleTo(int tile)
        {
            this.Tile = tile;
        }

        public void SpawnPawns()
        {
            List<Pawn> pawnList = MercenaryUtil.GenerateWarbandPawns(this);
            try
            {
                GenSpawn.Spawn(pawnList.First(), CellFinder.RandomClosewalkCellNear(Map.Center, Map, 10), Map);
            }
            catch (Exception e)
            {
                Log.Message($"Error while trying to generate warband pawn:{e.Message},{e.StackTrace}");
                return;
            }
            SpawnPawnsNearCenter(pawnList);
            if (this.Faction != Faction.OfPlayer)
            {
                LordJob_DefendPoint lordJobDefendPoint = new LordJob_DefendPoint(Map.Center);
                LordMaker.MakeNewLord(this.Faction, lordJobDefendPoint, base.Map, pawnList);
            }
        }

        void SpawnPawnsNearCenter(List<Pawn> pawnList)
        {
            foreach (Pawn p in pawnList)
            {
                if (!p.Spawned)
                {
                    if (CellFinder.TryFindRandomCellNear(Map.Center, Map, 15, new Predicate<IntVec3>(x => x.Walkable(Map)), out IntVec3 cell))
                        GenSpawn.Spawn(p, cell, Map);
                    else
                        GenSpawn.Spawn(p, Map.Center, Map);
                }
            }
        }

      


        public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
        {
            yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate ()
            {
                representative.TryLaunch(this.Tile, new TransportPodsArrivalAction_FormCaravan());
            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
        }

        public override IEnumerable<FloatMenuOption> GetShuttleFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<int, TransportPodsArrivalAction> launchAction)
        {
            yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate ()
            {
                launchAction(this.Tile, new TransportPodsArrivalAction_FormCaravan());
            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
        }

        public void OrderPlayerWarbandToResettle()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(this), CameraJumper.MovementMode.Pan);
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(OrderPlayerWarbandToResettle), true, extraLabelGetter: ExtraLabel);
        }

        public string ExtraLabel(GlobalTargetInfo targetInfo)
        {
            int cost = GetResettleCost(targetInfo);
            return "WAW.ResettleFee".Translate(cost);
        }

        public void TryToRemovePawn(string kindName)
        {
            if (this.bandMembers.ContainsKey(kindName) && bandMembers[kindName] > 0)
            {
                bandMembers[kindName]--;
            }
            TryRemoveInjuries(kindName);
            if (this.GetMemberCount() < 1)
            {
                TryDestroyWarband();
            }
        }

     

        void TryRemoveInjuries(string kindName)
        {
            if (this.Faction == Faction.OfPlayer &&
                this.playerWarbandManager != null &&
                this.playerWarbandManager.injuriesManager != null)
            {
                this.playerWarbandManager.injuriesManager.RemovePawn(kindName);
            }
        }


        public bool HasLeader()
        {
            return this.playerWarbandManager != null &&
                this.playerWarbandManager.leader != null &&
                this.playerWarbandManager.leader.Leader != null &&
                !this.playerWarbandManager.leader.Leader.Dead;
        }

        void TryDestroyWarband()
        {
            if (HasLeader())
            {
                return;
            }
            if (!this.HasMap)
                this.Destroy();
            this.npcWarbandManager?.SetDefeated();
            TryToNotifyWarbandDestruction();
        }

        void TryToNotifyWarbandDestruction()
        {
            if (this.Faction == Faction.OfPlayer)
            {
                Letter lostWarband = LetterMaker.MakeLetter(label: "WAW.LostWarband".Translate(), text: "WAW.LostWarband.Desc".Translate(), LetterDefOf.NegativeEvent);
                Find.LetterStack.ReceiveLetter(lostWarband);
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            playerWarbandManager?.ResetRaidTick();
        }


        public int GetResettleCost(GlobalTargetInfo targetInfo)
        {
            if (!targetInfo.IsValid)
            {
                return 0;
            }
            float distance = Find.WorldGrid.ApproxDistanceInTiles(targetInfo.Tile, this.Tile);
            var curve = WarbandUtil.ResettleCurve();
            int memberCount = this.GetMemberCount();
            int costPerPawn = (int)curve.Evaluate(memberCount);
            int cost = (int)distance * costPerPawn * memberCount;
            return cost;
        }

        public bool OrderPlayerWarbandToResettle(GlobalTargetInfo info)
        {
            return playerWarbandManager.OrderPlayerWarbandToResettle(info, this);
        }



        public int GetMemberCount()
        {
            int result = 0;
            foreach (var ele in this.bandMembers)
            {
                result += ele.Value;
            }
            return result;
        }



        public void StoreAll(IEnumerable<Thing> things)
        {

            playerWarbandManager?.StoreAll(things);
        }

        public void Store(ref Thing thing)
        {
            playerWarbandManager?.StoreThing(ref thing);
        }



        public void WithdrawLoot()
        {
            playerWarbandManager?.WithdrawLoot();
        }

        public void WithdrawLootInSilver()
        {
            playerWarbandManager?.WithdrawLootInSilver();
        }

        void RemoveMapCheck()
        {
            if (this.HasMap && this.ShouldRemoveMapNow(out bool flag))
            {
                Map map = this.Map;
                Current.Game.DeinitAndRemoveMap(map, true);
            }
        }

        void SiteCheck()
        {
            if (this.Faction != Faction.OfPlayer)
            {
                return;
            }
            var predicate = new Predicate<SitePart>(x => x.def.defName == "Outpost" || x.def == SitePartDefOf.BanditCamp);
            if (this.parts.Any(predicate))
            {
                this.parts.RemoveAll(predicate);
                var sitePartList = new List<SitePartDef>
                {
                     WAWDefof.WAWEmptySite,
                };
                SiteMakerHelper.GenerateDefaultParams(0f, this.Tile, this.Faction, sitePartList, out List<SitePartDefWithParams> sitePartDefsWithParams);
                if (sitePartDefsWithParams != null)
                {
                    foreach (SitePartDefWithParams sitePart in sitePartDefsWithParams)
                    {
                        this.AddPart(new SitePart(this, sitePart.def, sitePart.parms));
                    }
                }
                Log.Message("Sites removed.");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, int>(ref this.bandMembers,
  "bandMembers", LookMode.Value, LookMode.Value, ref _stringBuffers, ref _intBuffers);
            Scribe_Values.Look(ref this.isSettling, "isSettling");
            Scribe_Values.Look(ref this.customName, "customName", "Warband");
            Scribe_Defs.Look(ref this._pawnKindFactionType, "_pawnKindFactionType");
            npcWarbandManager?.ExposeData();
            playerWarbandManager?.ExposeData();
        }


    }
}
