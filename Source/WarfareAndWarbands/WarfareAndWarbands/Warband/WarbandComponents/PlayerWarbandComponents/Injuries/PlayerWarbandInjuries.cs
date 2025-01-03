using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents
{
    public class PlayerWarbandInjuries : IExposable
    {
        private List<PlayerWarbandRecoveryGroup> recoveries;
        private float recoverRateMultiplier;

        public PlayerWarbandInjuries()
        {
            recoveries = new List<PlayerWarbandRecoveryGroup>();
            recoverRateMultiplier = 1;
        }

        public void InjurePawn(Pawn pawn)
        {
            int ticksGame = GenTicks.TicksGame;
            PlayerWarbandRecoveryGroup selectedGroup;
            if (recoveries.Count < 1 || recoveries.Last().NoExtraSlots())
            {
                selectedGroup = AddNewRecovery(ticksGame);
            }
            else
            {
                selectedGroup = recoveries.Last();
            }
            selectedGroup.InjurePawn(pawn.kindDef.defName);
        }

        public void InjurePawn(string pKind, int ticksGame)
        {
            PlayerWarbandRecoveryGroup selectedGroup;
            if (recoveries.Count < 1 || recoveries.Last().NoExtraSlots())
            {
                selectedGroup = AddNewRecovery(ticksGame);
            }
            else
            {
                selectedGroup = recoveries.Last();
            }
            selectedGroup.InjurePawn(pKind);
        }
        public void RecoverAll()
        {
            recoveries.Clear();
        }
        
        public void RemovePawn(string pKindName)
        {
            if (recoveries.Count < 1)
            {
                return;
            }
            for (int i = 0; i < this.recoveries.Count; i++)
            {
                var group = recoveries[i];
                if (group.TryToRemovePawn(pKindName))
                {
                    break;
                }
            }
        }

        public bool IsInjured()
        {
            return this.GetRecoveryDays() > 0;
        }

        public string LeaderFactorString()
        {
            return "WAW.leaderRecoveryFactor".Translate((recoverRateMultiplier * 100).ToString("0.0"));
        }

        public float GetRecoveryDays()
        {
            float result = 0;
            foreach (var recovery in this.recoveries)
            {
                result = Math.Max(result, recovery.GetRemainingDays(this.recoverRateMultiplier));
            }
            return result;  
        }

        PlayerWarbandRecoveryGroup AddNewRecovery(int ticksGame)
        {
            PlayerWarbandRecoveryGroup newGroup = new PlayerWarbandRecoveryGroup(ticksGame);
            recoveries.Add(newGroup);
            return newGroup;
        }

        public Dictionary<string, int> GetInjuries()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach(var recover in recoveries)
            {
               foreach(var keyValuePair in recover.GetInjuries())
                {
                    if (result.ContainsKey(keyValuePair.Key))
                    {
                        result[keyValuePair.Key] += keyValuePair.Value;
                    }
                    else
                    {
                        result.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }
            }

            return result;
        }

        public void SetRecoverRateMultiplier(float value)
        {
            this.recoverRateMultiplier = value;
        }

        public int GetActiveMemberCount(Dictionary<string, int> members)
        {
            var memebers = GetActiveMembers(members);
            int result = 0;
            foreach(var ele in members)
            {
                result += ele.Value;
            }
            return result;
        }

        public Dictionary<string, int> GetActiveMembers(Dictionary<string, int> members, Dictionary<string, int> injuries)
        {
            Dictionary<string, int> result = new Dictionary<string, int>(members);
            for (int i = 0; i < result.Count; i++)
            {
                var member = result.ElementAt(i);
                if (injuries.ContainsKey(member.Key))
                {
                    result[member.Key] = Math.Max(result[member.Key] - injuries[member.Key], 0);
                }
            }

            return result;
        }

        public Dictionary<string, int> GetActiveMembers(Dictionary<string, int> members)
        {
            Dictionary<string, int> result = new Dictionary<string, int>(members);
            Dictionary<string, int> injuries = this.GetInjuries();
            for(int i = 0; i< injuries.Count; i++)
            {
                var member = injuries.ElementAt(i);
                if (injuries.ContainsKey(member.Key))
                {
                    result[member.Key] = Math.Max(result[member.Key] - injuries[member.Key], 0); 
                }
            }

            return result;
        }

        public void Tick()
        {
            int ticksGame = GenTicks.TicksGame;
            for (int i = 0; i < recoveries.Count; i++)
            {
                var recovery = recoveries.ElementAtOrDefault(i);
                if (recovery.CanBeRemoved(ticksGame, this.recoverRateMultiplier))
                {
                    recoveries.RemoveAt(i);
                }
            }
        }



        public void ExposeData()
        {
            Scribe_Collections.Look(ref recoveries, "recoveries", LookMode.Deep);
            if(recoveries == null)
            {
                recoveries = new List<PlayerWarbandRecoveryGroup>();
            }
        }

    }
}
