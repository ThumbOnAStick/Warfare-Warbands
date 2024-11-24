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
    public class NPCWarbandComponent:IExposable
    {
        public NPCWarbandComponent(Warband warband)
        {
            this.warband = warband;
            this.targetTile = 0;
        }

        public void Tick()
        {
            if(warband.Faction == Faction.OfPlayer)
            {
                return;
            }
            CheckAllEnemiesDefeated();
            CheckShouldDestroySite();
            AIWarbandRaidUpdate();
        }

        public void SetTargetTile(int targetTile)
        {
            this.targetTile = targetTile;
        }

        public void CheckAllEnemiesDefeated()
        {
            if(warband == null)
            {
                return;
            }
            if (IsWarbandDefeated() && defeated == false)
            {
                this.defeated = true;
                OnDefeated();
            }

        }

        public void OnDefeated()
        {
            GameComponent_WAW.Instance.DecreaseDurability(warband.Faction, progressPoint);

            SendHostileWarbandDefeatedMessage();

        }

        public MapParent TryGetTarget()
        {
            if (HasTargetingFaction())
                return Find.WorldObjects.WorldObjectAt<MapParent>(this.targetTile);
            else
                return null;
        }


        void TryAffectGoodwill()
        {
            if (HasTargetingFaction())
            {
                Faction.OfPlayer.TryAffectGoodwillWith(Find.WorldObjects.WorldObjectAt<MapParent>(this.targetTile).Faction, defeatAward);
            }
        }

        void TryToOccupySettlement(ref MapParent factionBase)
        {
            IntRange range = new IntRange(0, 100);
            int val = range.RandomInRange;
            if (val < WAWSettings.occupyChance)
            {
                factionBase.Destroy();
                WarbandUtil.AddNewHome(targetTile, warband.Faction, factionBase.def);
                BeatOpponent();
            }
        }

        void MoveAndDestroy(MapParent factionBase)
        {
            var tile = factionBase.Tile;
            factionBase.Destroy();
            warband.ResettleTo(tile);
        }

         void BeatOpponent()
        {
            GameComponent_WAW.Instance.AddDurability(this.warband.Faction, progressPoint);
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

        void AIWarbandRaidUpdate()
        {
            if (warband.Faction == Faction.OfPlayer)
            {
                return;
            }
            if (!HasTargetingFaction())
            {
                return;
            }
            var factionBase = this.TryGetTarget();
            if (factionBase.Faction == null)
            {
                return;
            }
            if (Find.TickManager.TicksGame - warband.creationGameTicks > assaultDuration && factionBase.Map == null)
            {
                if (factionBase as Warband != null)
                {
                    GameComponent_WAW.Instance.DecreaseDurability(factionBase.Faction, progressPoint);
                    MoveAndDestroy(factionBase);
                    BeatOpponent();
                }
                else if (factionBase as Settlement != null)
                {
                    TryToOccupySettlement(ref factionBase);
                    warband.Destroy();
                }
                else
                {
                    MoveAndDestroy(factionBase);
                }
            }


        }


        public bool HasTargetingFaction()
        {
            return this.targetTile != 0 && Find.WorldObjects.AnyMapParentAt(this.targetTile) && Find.WorldObjects.WorldObjectAt<MapParent>(this.targetTile).Faction != null;
        }

        private bool IsWarbandDefeated()
        {
            return warband.HasMap && !GenHostility.AnyHostileActiveThreatToPlayer(warband.Map, false, false);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.defeated, "defeated");
            Scribe_Values.Look(ref this.targetTile, "targetTile");

        }


        private bool defeated = false;
        private readonly Warband warband;
        private int targetTile;
        private static readonly int progressPoint = 5;
        private static readonly int defeatAward = 5;
        private static readonly int assaultDuration = WAWSettings.eventFrequency * 60000;


    }
}
