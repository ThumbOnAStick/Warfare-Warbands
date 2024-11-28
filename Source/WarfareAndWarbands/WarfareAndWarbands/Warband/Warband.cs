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
        public override void PostAdd()
        {
            base.PostAdd();
            this.forceRemoveWorldObjectWhenMapRemoved = false;
            if (this.Faction.IsPlayer)
            {
                this.bandMembers = new Dictionary<string, int>(GameComponent_WAW.playerWarband.bandMembers);
            }
            else
            {
                bandMembers = new Dictionary<string, int>();
                GenerateNPCCombatGroup();
            }   
        }

        public Warband()
        {
            npcWarbandManager = new NPCWarbandManager(this);
            playerWarbandManager = new PlayerWarbandManager(this);
        }

        public override Color ExpandingIconColor => this.Faction.Color;

        public override string Label => this.def.label + "(" + this.Faction.NameColored + ")";


        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            if (this.Faction == Faction.OfPlayer || !this.Faction.HostileTo(Faction.OfPlayer))
            {
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


        public override string GetInspectString()
        {
            var outString = "Members:";
            foreach (var member in bandMembers)
            {
                if (member.Value > 0)
                    outString += "\n" + member.Key + "(" + member.Value + ")";
            }
            if (npcWarbandManager.HasTargetingFaction())
            {
                outString += "\n" + "WAW.TargetingMapParent".Translate(npcWarbandManager.TryGetTarget().Label);
            }
            if (this.Faction == Faction.OfPlayer)
            {
                if (!playerWarbandManager.CanFireRaid())
                {
                    string cooldownString = "WAW.AvialableIn".Translate(playerWarbandManager.GetRemainingDays().ToString("0.0"));
                    outString += "\n" + cooldownString;
                }
   
            }
            return outString;
        }

        public Dictionary<string, int> GenerateNPCCombatGroup()
        {
            Faction f = this.Faction;
            var combatGroup = f.def.pawnGroupMakers.Where(x => x.kindDef == PawnGroupKindDefOf.Combat && x.maxTotalPoints > 1000).RandomElement();
            float actualPoints = Math.Max(StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap), 1000);
            PawnGroupMakerParms parms = new PawnGroupMakerParms() { points = actualPoints, faction = f, groupKind = combatGroup.kindDef };
            var results = PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, combatGroup.options, parms);
            bandMembers = new Dictionary<string, int>();
            foreach (var ele in results)
            {
                string defName = ele.Option.kind.defName;
                if (!bandMembers.ContainsKey(defName))
                    bandMembers.Add(defName, 1);
                else
                    bandMembers[defName]++;
            }

            return bandMembers;
        }

        public Dictionary<string, int> GenerateNPCCombatGroup(List<Pawn> pawns)
        {
            bandMembers = new Dictionary<string, int>();
            Faction f = this.Faction;
            foreach (var ele in pawns)
            {
                if (ele.Dead)
                    continue;
                string defName = ele.kindDef.defName;
                if (!bandMembers.ContainsKey(defName))
                    bandMembers.Add(defName, 1);
                else
                    bandMembers[defName]++;
            }
            Log.Message(pawns.Count);
            return bandMembers;
        }
        public override void PostMapGenerate()
        {
            SpawnPawns();
        }

        public override void PostRemove()
        {
            base.PostRemove();
        }


        public override void Tick()
        {
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i].SitePartTick();
            }

            for (int j = 0; j < parts.Count; j++)
            {
                parts[j].def.Worker.SitePartWorkerTick(parts[j]);
            }

            npcWarbandManager.Tick();

            if (this.HasMap && this.ShouldRemoveMapNow(out bool flag))
            {
                Map map = this.Map;
                Current.Game.DeinitAndRemoveMap(map, true);

            }

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
                Log.Message($"Error while trying to generate npc warband pawn:{e.Message},{e.StackTrace}");
            }
            SpawnPawnsNearCenter(pawnList);
            LordJob_DefendPoint lordJobDefendPoint = new LordJob_DefendPoint(Map.Center);
            LordMaker.MakeNewLord(this.Faction, lordJobDefendPoint, base.Map, pawnList);
        }

        void SpawnPawnsNearCenter(List<Pawn> pawnList)
        {
            foreach (Pawn p in pawnList)
            {
                if (!p.Spawned)
                    GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(Map.Center, Map, 10), Map);
            }
        }


        public override Texture2D ExpandingIcon => this.def.ExpandingIconTexture;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, int>(ref this.bandMembers,
  "bandMembers", LookMode.Value, LookMode.Value, ref stringBuffers, ref intBuffers);
            Scribe_Values.Look(ref this.isSettling, "isSettling");
            npcWarbandManager.ExposeData();
            playerWarbandManager.ExposeData();
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
            if (this.GetMemberCount() < 1)
            {
                TryDestroyWarband();
            }
        }   


        void TryDestroyWarband()
        {
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
            playerWarbandManager.ResetRaidTick();
        }

        public override void Draw()
        {
            base.Draw();
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

            playerWarbandManager.StoreAll(things);
        }

        public void Store(ref Thing thing)
        {
            playerWarbandManager.StoreThing(ref thing);
        }

       

        public void WithdrawLoot()
        {
            playerWarbandManager.WithdrawLoot();
        }

        public void WithdrawLootInSilver()
        {
            playerWarbandManager.WithdrawLootInSilver();
        }

        

        public Dictionary<string, int> bandMembers;
        public NPCWarbandManager npcWarbandManager;
        public PlayerWarbandManager playerWarbandManager;
        public static readonly int playerAttackRange = 10;
        private List<string> stringBuffers;
        private List<int> intBuffers;
        private bool isSettling = false;
    }
}
