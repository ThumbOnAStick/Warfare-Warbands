using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents
{
    internal class PlayerWarbandRecoveryGroup : IExposable
    {
        public PlayerWarbandRecoveryGroup()
        {
            this.startTick = GenTicks.TicksGame;
            injuries = new Dictionary<string, int>();
        }
        public PlayerWarbandRecoveryGroup(int startTcik)
        {
            this.startTick = startTcik;
            injuries = new Dictionary<string, int>();
        }
        Dictionary<string, int> injuries;
        private int startTick;
        private List<string> pawnKindNamesCache;
        private List<int> pawnNumberCache;

        private static readonly int maxInjuredPawns = 3;

        public float GetRecoveriesDays()
        {
            return GetInjuriesCount() * 1.5f;
        }
        public int GetRecoveriesTick()
        {
            return (int)GetRecoveriesDays() * 60000;
        }

        public float GetRemainingDays()
        {
            return GenDate.TicksToDays(this.startTick + this.GetRecoveriesTick() - GenTicks.TicksGame);
        }

        public void InjurePawn(string kindName)
        {
            if (injuries.ContainsKey(kindName))
            {
                injuries[kindName]++;
            }
            else
            {
                injuries.Add(kindName, 1);
            }
        }

        public int GetInjuriesCount()
        {
            int count = 0;
            foreach (var keyValuePair in injuries)
            {
                count += keyValuePair.Value;
            }
            return count;
        }

        public bool NoExtraSlots()
        {
            return GetInjuriesCount() >= maxInjuredPawns;
        }

        public bool CanBeRemoved(int ticksGame)
        {
            return ticksGame - this.startTick > this.GetRecoveriesTick() ||
                this.injuries.Count < 1 ||
                !this.injuries.Any(x => x.Value > 0);
        }

        public bool TryToRemovePawn(string kindName)
        {
            if (this.injuries.ContainsKey(kindName) && this.injuries[kindName] > 0)
            {
                this.injuries[kindName] = Math.Max(this.injuries[kindName] - 1, 0);
                return true;
            }
            return false;
        }

        public Dictionary<string, int> GetInjuries()
        {
            return injuries;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref injuries, "injuries", keyLookMode: LookMode.Value, valueLookMode: LookMode.Value
         , ref pawnKindNamesCache, ref pawnNumberCache);
        }
    }
}
