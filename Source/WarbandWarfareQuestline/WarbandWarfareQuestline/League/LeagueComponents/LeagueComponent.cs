using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League
{
    public class LeagueComponent : IExposable
    {

        private bool _isActive;
        private int _lastExecuteTick;


        public LeagueComponent()
        {
            _isActive = false;
            _lastExecuteTick = -CooldownTicks;
        }
        public virtual float CooldownDays => 10;

        public virtual int CooldownTicks => (int)(GenDate.TicksPerDay * CooldownDays);

        public virtual int RemainingCooldownTicks => _lastExecuteTick + CooldownTicks - GenTicks.TicksGame;

        public virtual string RemainingDaysLabel => ((float)RemainingCooldownTicks / GenDate.TicksPerDay).ToString("0.0");

        public bool IsActive => _isActive;


        public bool IsAvailable()
        {
            return RemainingCooldownTicks <= 0;
        }

        public virtual void TryToExecute()
        {
            if (!_isActive) return;

            if (IsAvailable())
            {
                SetLastExecuteTick();
                Execute();
                Log.Message("WAW: LeagueComponent executed");
            }
            else
            {
                SendAvailabilityNotification();
            }

        }

        public void SetActive(bool isActive)
        {
            _isActive = isActive;
        }

        public virtual void SendAvailabilityNotification()
        {

        }

        protected virtual void Execute()
        {

        }

        public virtual void Tick()
        {
           
        }

        public void SetLastExecuteTick()
        {
            _lastExecuteTick = GenTicks.TicksGame;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref _isActive, "isActive", false);
            Scribe_Values.Look(ref _lastExecuteTick, "lastExecuteTick", 0);
        }
    }
}
