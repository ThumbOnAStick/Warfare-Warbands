using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using static System.Collections.Specialized.BitVector32;

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
                this.bandMembers = GameComponent_WAW.playerWarband.bandMembers;
            }
            else
            {
                bandMembers = new Dictionary<string, int>();
                GenerateNPCCombatGroup();
            }
        }

        public override Color ExpandingIconColor => this.Faction.Color;

        public override string Label => this.def.label + "(" + this.Faction.NameColored + ")";


        private bool IsWarbandDefeated()
        {
            return this.HasMap && !GenHostility.AnyHostileActiveThreatToPlayer(base.Map, false, false);
        }
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
            if (this.HasTargetingFaction())
            {
                outString += "\n" + "WAW.TargetingMapParent".Translate(this.TryGetTarget().Label);
            }
            return outString;
        }
        public Dictionary<string, int> GenerateNPCCombatGroup()
        {
            Faction f = this.Faction;
            var combatGroup = f.def.pawnGroupMakers.Where(x => x.kindDef == PawnGroupKindDefOf.Combat && x.maxTotalPoints > 1000).RandomElement();
            float actualPoints = Math.Max(StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap), 1000) ;
            PawnGroupMakerParms parms = new PawnGroupMakerParms(){ points = actualPoints, faction = f, groupKind = combatGroup.kindDef };
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

            CheckAllEnemiesDefeated();

            bool flag;
            if (this.HasMap && this.ShouldRemoveMapNow(out flag))
            {
                GenerateNPCCombatGroup(GetMapBandMembers());
                Map map = this.Map;
                Current.Game.DeinitAndRemoveMap(map, true);
            }
            CheckShouldDestroySite();
            AIWarbandRaidUpdate();

        }



        void CheckAllEnemiesDefeated()
        {
            if (IsWarbandDefeated() && defeated == false)
            {
                this.defeated = true;
                OnDefeated();
            }

        }

        void CheckShouldDestroySite()
        {
            if (!this.HasMap && this.defeated)
            {
                this.Destroy();
                return;
            }
        }

        void SendWarbandDefeatedMessage()
        {
            Letter defeatLetter = LetterMaker.MakeLetter("WAW.DefeatWarbandLetter.Label".Translate(), "WAW.DefeatWarbandLetter.Desc".Translate(this.Faction.NameColored), LetterDefOf.PositiveEvent);
            Find.LetterStack.ReceiveLetter(defeatLetter);
        }

        MapParent TryGetTarget()
        {
            if (HasTargetingFaction())
                return Find.WorldObjects.WorldObjectAt<MapParent>(this.targetTile);
            else
                return null;
        }

        bool HasTargetingFaction()
        {
            return this.targetTile != 0 && Find.WorldObjects.AnyMapParentAt(this.targetTile) && Find.WorldObjects.WorldObjectAt<MapParent>(this.targetTile).Faction != null;
        }

        void TryAffectGoodwill()
        {
            if (HasTargetingFaction())
            {
                Faction.OfPlayer.TryAffectGoodwillWith(Find.WorldObjects.WorldObjectAt<MapParent>(this.targetTile).Faction, defeatAward);
            }
        }
        void AIWarbandRaidUpdate()
        {
            if (this.Faction == Faction.OfPlayer)
            {
                return;
            }
            if (!this.HasTargetingFaction())
            {
                return;
            }
            var factionBase = this.TryGetTarget();
            if (factionBase.Faction == null)
            {
                return;
            }
            if (Find.TickManager.TicksGame - this.creationGameTicks > assaultDuration && factionBase.Map == null)
            {
                if (factionBase as Warband != null)
                {
                    GameComponent_WAW.Instance.DecreaseDurability(this.Faction, progressPoint);
                    MoveAndDestroy(factionBase);
                    BeatOpponent();
                }
                else if (factionBase as Settlement != null)
                {
                    TryToOccupySettlement(ref factionBase);
                    Destroy();
                }
                else
                {
                    MoveAndDestroy(factionBase);
                }
            }


        }

        void TryToOccupySettlement(ref MapParent factionBase)
        {
            IntRange range = new IntRange(0, 100);
            int val = range.RandomInRange;
            if (val < WAWSettings.occupyChance)
            {
                factionBase.Destroy();
                WarbandUtil.AddNewHome(targetTile, Faction, factionBase.def);
                BeatOpponent();
            }
        }

        void MoveAndDestroy(MapParent factionBase)
        {
            var tile = factionBase.Tile;
            factionBase.Destroy();
            this.ResettleTo(tile);
        }
        void BeatOpponent()
        {
            GameComponent_WAW.Instance.AddDurability(this.Faction, progressPoint);
        }

        public void OnDefeated()
        {
            GameComponent_WAW.Instance.DecreaseDurability(this.Faction, progressPoint);

            SendWarbandDefeatedMessage();

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
            Scribe_Values.Look(ref this.defeated, "defeated");
            Scribe_Values.Look(ref this.targetTile, "targetTile");
            Scribe_Values.Look(ref this.isSettling, "isSettling");
            Scribe_Collections.Look(ref this.bandPawns, "bandPawns", LookMode.Value);

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

        public bool OrderPlayerWarbandToResettle(GlobalTargetInfo info)
        {
            if (info.WorldObject != null)
            {
                Messages.Message("WAW.InvalidObject".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }

            float distance = Find.WorldGrid.ApproxDistanceInTiles(info.Tile, this.Tile);
            int cost = (int)distance * 100;
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

        public void OrderPlayerWarbandToAttack()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(this), CameraJumper.MovementMode.Pan);
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(OrderPlayerWarbandToAttack), false,
                onUpdate: delegate
                {
                    GenDraw.DrawWorldRadiusRing(this.Tile, playerAttackRange);
                });
        }

        public bool OrderPlayerWarbandToAttack(GlobalTargetInfo info)
        {
            if (info.WorldObject == null ||
                info.WorldObject as MapParent == null||
                WarbandUtil.IsWorldObjectNonHostile(info.WorldObject))
            {
                Messages.Message("WAW.InvalidObject".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }

            if (Find.WorldGrid.ApproxDistanceInTiles(info.Tile, this.Tile) > playerAttackRange)
            {
                Messages.Message("WAW.FarObject".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }

            int cost = (int)PlayerWarbandArrangement.GetCost(this.bandMembers);
            if (!WarbandUtil.TryToSpendSilver(Find.AnyPlayerHomeMap, cost))
            {
                Messages.Message("WAW.CantAfford".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }

            var enemy = (MapParent)info.WorldObject;
            LongEventHandler.QueueLongEvent(delegate ()
            {
                WarbandUtil.OrderPlayerWarbandToAttack(enemy, this);
                SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            }, "GeneratingMapForNewEncounter", false, null, true);

            return true;
        }

      

        public Dictionary<string, int> bandMembers;
        public int targetTile = 0;
        private List<string> bandPawns;
        private List<string> stringBuffers;
        private List<int> intBuffers;
        public static readonly int playerAttackRange = 10;
        private static readonly int defeatAward = 5;
        private readonly int assaultDuration = WAWSettings.eventFrequency * 60000;
        private readonly int progressPoint = 5;
        private bool defeated = false;
        private bool isSettling = false;


    }
}
