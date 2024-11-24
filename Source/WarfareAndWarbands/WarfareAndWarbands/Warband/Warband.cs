using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using WarfareAndWarbands.Warband.WarbandComponents;

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
            playerWarbandCoolDown = new PlayerWarbandCooldown();
            lootManager = new PlayerWarbandLootManager();
            resettleManager = new PlayerWarbandResettleManager();   
            npcComponent = new NPCWarbandComponent(this);
            attackManager = new PlayerWarbandAttackManager(this);
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
            if (npcComponent.HasTargetingFaction())
            {
                outString += "\n" + "WAW.TargetingMapParent".Translate(npcComponent.TryGetTarget().Label);
            }
            if (this.Faction == Faction.OfPlayer)
            {
                if (!playerWarbandCoolDown.CanFireRaid())
                {
                    string cooldownString = "WAW.AvialableIn".Translate(playerWarbandCoolDown.GetRemainingDays().ToString("0.0"));
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

            npcComponent.Tick();

            if (this.HasMap && this.ShouldRemoveMapNow(out bool flag))
            {
                GenerateNPCCombatGroup(GetMapBandMembers());
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
            PawnGroupMakerParms result = new PawnGroupMakerParms();
            List<Pawn> pawnList = new List<Pawn>();
            bandPawns = new List<string>();
            foreach (var ele in bandMembers)
            {
                string name = ele.Key;
                int count = ele.Value;
                for (int i = 0; i < count; i++)
                {
                    if (!WarbandUtil.HasPawnKind(name))
                    {
                        continue;
                    }
                    PawnGenerationRequest request = new PawnGenerationRequest(WarbandUtil.TargetPawnKindDef(name), this.Faction, PawnGenerationContext.NonPlayer);
                    Pawn p = PawnGenerator.GeneratePawn(request);
                    pawnList.Add(p);

                }
            }

            foreach (Pawn p in pawnList)
            {
                GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(Map.Center, Map, 10), Map);
                bandPawns.Add(p.ThingID);
            }

            LordJob_DefendPoint lordJobShellFactionFirst = new LordJob_DefendPoint(Map.Center);
            LordMaker.MakeNewLord(this.Faction, lordJobShellFactionFirst, base.Map, pawnList);

        }

        public override Texture2D ExpandingIcon => this.def.ExpandingIconTexture;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, int>(ref this.bandMembers,
  "bandMembers", LookMode.Value, LookMode.Value, ref stringBuffers, ref intBuffers);
            Scribe_Values.Look(ref this.isSettling, "isSettling");
            Scribe_Collections.Look(ref this.bandPawns, "bandPawns", LookMode.Value);
            lootManager.ExposeData();
            playerWarbandCoolDown.ExposeData();
            npcComponent.ExposeData();
        }

        public List<Pawn> GetMapBandMembers()
        {
            List<Pawn> outPawns = new List<Pawn>();
            foreach (var id in bandPawns)
            {
                if (this.Map.mapPawns.AllPawnsSpawned.Any(x => x.ThingID == id))
                    outPawns.Add(this.Map.mapPawns.AllPawns.First(x => x.ThingID == id));
            }
            return outPawns;
        }

        public void OrderPlayerWarbandToResettle()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(this), CameraJumper.MovementMode.Pan);
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(OrderPlayerWarbandToResettle), true);
        }

        public void TryToRemovePawn(string kindName)
        {
            if (this.bandMembers.ContainsKey(kindName) && bandMembers[kindName] > 0)
            {
                bandMembers[kindName]--;
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            this.playerWarbandCoolDown.ResetRaidTick();
        }

        public bool OrderPlayerWarbandToResettle(GlobalTargetInfo info)
        {
            if (info.WorldObject != null)
            {
                Messages.Message("WAW.InvalidObject".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }

            float distance = Find.WorldGrid.ApproxDistanceInTiles(info.Tile, this.Tile);
            var curve = WarbandUtil.ResettleCurve();
            int memberCount = this.GetMemberCount();
            int costPerPawn = (int)curve.Evaluate(memberCount);
            int cost = (int)distance * costPerPawn * memberCount;
            if (!WarbandUtil.TryToSpendSilver(Find.AnyPlayerHomeMap, cost))
            {
                Messages.Message("WAW.CantAfford".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }
            TransportPodsArrivalAction_SpawnWarband action = new TransportPodsArrivalAction_SpawnWarband(this);
            TravelingTransportPods travelingTransportPods = (TravelingTransportPods)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.TravelingTransportPods);
            travelingTransportPods.Tile = this.Tile;
            travelingTransportPods.SetFaction(Faction.OfPlayer);
            travelingTransportPods.destinationTile = info.Tile;
            travelingTransportPods.arrivalAction = action;
            Find.WorldObjects.Add(travelingTransportPods);
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            return true;
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

            lootManager.StoreAll(things);
        }

        public void Store(ref Thing thing)
        {
            lootManager.StoreThing(ref thing);
        }

       

        public void DumpLoot()
        {
            lootManager.DumpLoot();
        }

        public void DumpLootInSilver()
        {
            lootManager.DumpLootInSilver();
        }



        public Dictionary<string, int> bandMembers;
        public PlayerWarbandCooldown playerWarbandCoolDown;
        public PlayerWarbandLootManager lootManager;
        public NPCWarbandComponent npcComponent;
        public PlayerWarbandAttackManager attackManager;
        public PlayerWarbandResettleManager resettleManager;
        public static readonly int playerAttackRange = 10;
        private List<string> bandPawns;
        private List<string> stringBuffers;
        private List<int> intBuffers;
        private bool isSettling = false;

    }
}
