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

        public override string Label => this.def.label;


        private bool IsWarbandDefeated()
        {
            return this.HasMap && !GenHostility.AnyHostileActiveThreatToPlayer(base.Map, true, false);
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
            PawnGroupMakerParms parms = new PawnGroupMakerParms() { points = ThreatPoints, faction = f, groupKind = combatGroup.kindDef };
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
            if (IsWarbandDefeated() && !defeated)
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
                factionBase.Destroy();
                WarbandUtil.AddNewHome(targetTile, Faction, factionBase.def);
                this.Destroy();
            }


        }

        void OnDefeated()
        {
            GameComponent_WAW.Instance.DecreaseDurability(this.Faction, progressPoint);
            SendWarbandDefeatedMessage();

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

        public void OrderPlayerWarbandToAttack()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(OrderPlayerWarbandToAttack), false);
        }

        public bool OrderPlayerWarbandToAttack(GlobalTargetInfo info)
        {
            if (info.WorldObject == null ||
                info.WorldObject.Faction == null || !info.WorldObject.Faction.HostileTo(Faction.OfPlayer) ||
                (MapParent)info.WorldObject == null)
            {
                return false;
            }
            var enemy = (MapParent)info.WorldObject;
            LongEventHandler.QueueLongEvent(delegate ()
            {
                WarbandUtil.OrderPlayerWarbandToAttack(enemy, this);
            }, "GeneratingMapForNewEncounter", false, null, true);

            return true;
        }


        public Dictionary<string, int> bandMembers;
        public int targetTile = 0;
        private List<string> bandPawns;
        private List<string> stringBuffers;
        private List<int> intBuffers;
        private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        private static readonly int ThreatPoints = 10000;
        private static readonly int defeatAward = 5;
        private readonly int assaultDuration = 60000;
        private bool defeated = false;
        private int progressPoint = 5;

    }
}
