using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public class NPCWarbandManager:IExposable
    {
        private bool defeated = false;
        private int targetTile;
        private readonly Warband warband;
        private static readonly int defeatAward = 5;

        public NPCWarbandManager(Warband warband)
        {
            this.warband = warband;
            this.targetTile = 0;
        }

        public void Tick()
        {
            if(GenTicks.TicksGame % 2500 != 0)
            {
                return;
            }
            if (warband.Faction == Faction.OfPlayer)
            {
                return;
            }
            CheckAllEnemiesDefeated();
            CheckShouldDestroySite();
            AIWarbandRaidUpdate();
            TryToSpawnWarbandMembersInTargetMap();
        }

        public void SetTargetTile(int targetTile)
        {
            this.targetTile = targetTile;
        }

        public void TryToSpawnWarbandMembersInTargetMap()
        {
            var mapP = TryGetTarget();
            if (mapP != null && mapP.HasMap && !this.warband.HasMap )
            {
                //Spawn warband
                this.warband.SpawnOffenders(mapP.Map);
                if (!this.warband.Destroyed)
                    this.warband.Destroy();
            }
        }

        public void CheckAllEnemiesDefeated()
        {
            if(warband == null)
            {
                return;
            }
            if (IsWarbandDefeated() && defeated == false)
            {
                SetDefeated();
                OnDefeated();
            }

        }

        public void OnDefeated()
        {
            DescreaseDurability();
            SendHostileWarbandDefeatedMessage();
        }

        void DescreaseDurability()
        {
            GameComponent_WAW.Instance.DecreaseDurability(warband.Faction, defeatAward);
        }

        public MapParent TryGetTarget()
        {
            if (HasTargetingFaction())
                return Find.WorldObjects.WorldObjectAt<MapParent>(this.targetTile);
            else
                return null;
        }
   
        void SendHostileWarbandDefeatedMessage()
        {
            Letter defeatLetter = LetterMaker.MakeLetter("WAW.DefeatWarbandLetter.Label".Translate(), "WAW.DefeatWarbandLetter.Desc".Translate(warband.Faction.NameColored), LetterDefOf.PositiveEvent);
            Find.LetterStack.ReceiveLetter(defeatLetter);
        }

        void CheckShouldDestroySite()
        {
            if (warband!= null && !warband.Destroyed && !warband.HasMap && defeated)
            {
                warband.Destroy();
                return;
            }
        }

        public Dictionary<string, int> GenerateNPCCombatGroup(List<Pawn> pawns)
        {
            var bandMembers = new Dictionary<string, int>();
            Faction f = warband.Faction;
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

        public Dictionary<string, int> GenerateNPCCombatGroup()
        {
            Faction f = warband.Faction;
            var combatGroup = f.def.pawnGroupMakers.Where(x => x.kindDef == PawnGroupKindDefOf.Combat && x.maxTotalPoints > 1000).RandomElement();
            float actualPoints = Math.Max(StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap), 1000);
            PawnGroupMakerParms parms = new PawnGroupMakerParms() { points = actualPoints, faction = f, groupKind = combatGroup.kindDef };
            var results = PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, combatGroup.options, parms);
            var bandMembers = new Dictionary<string, int>();
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

        void AIWarbandRaidUpdate()
        {
          
        }


        public bool HasTargetingFaction()
        {
            return this.targetTile != 0 && Find.WorldObjects.AnyMapParentAt(this.targetTile) && Find.WorldObjects.WorldObjectAt<MapParent>(this.targetTile).Faction != null;
        }

        private bool IsWarbandDefeated()
        {
            return warband.HasMap && (!GenHostility.AnyHostileActiveThreatToPlayer(warband.Map, false, false) || this.warband.bandMembers.Count < 1);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.defeated, "defeated", false);
            Scribe_Values.Look(ref this.targetTile, "targetTile", 0);

        }

        public void SetDefeated()
        {
            this.defeated = true;
        }




    }
}
