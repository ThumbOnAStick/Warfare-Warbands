using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands;

namespace WAWLeadership.LeadershipAttributes
{
    public class LeadershipExp : IExposable
    {
        public int CurrLevel => currLevel;
        public int CurrExp => currExp;
        private int currLevel;
        private int currExp;

        public LeadershipExp()
        {
            
        }

        public SimpleCurve ExpCurve()
        {
            return new SimpleCurve
            {
                { 1, 10 },
                { 2, 30 },
                { 5, 100 },
                { 10, 500 }
            };

        }

        public override string ToString()
        {
            return $"{this.currExp}/{this.ExpNeededForNextLevel()}";
        }

        public void AddExp(int amount, out bool levelUp, out int levelUpAmount)
        {
            levelUp = false;
            levelUpAmount = 0;
            currExp += amount;
            if (ShouldLevelUpNow(out int leak))
            {
                levelUp = true;
                LevelUP(leak, out levelUpAmount);
            }
        }

        public int ExpNeededForNextLevel()
        {
            return (int)ExpCurve().Evaluate(currLevel);
        }

        public float ExpPercent()
        {
            return (float)currExp / ExpNeededForNextLevel();
        }

        public bool ShouldLevelUpNow(out int leak)
        {
            leak = currExp - ExpNeededForNextLevel();
            if (currLevel >= 10)
                return false;
            return leak >= 0;
        }

        public void LevelUP(int leak, out int levelUpAmount)
        {
            levelUpAmount = 0;
            currLevel++;
            currExp = leak;
            while (ShouldLevelUpNow(out int extraLeak) && currLevel < 10)
            {
                currLevel++;
                currExp = extraLeak;
                levelUpAmount++;
            }
        }


        public void ExposeData()
        {
            Scribe_Values.Look(ref this.currExp, "currExp", 0);
            Scribe_Values.Look(ref this.currLevel, "currLevel", 0);

        }
    }
}
