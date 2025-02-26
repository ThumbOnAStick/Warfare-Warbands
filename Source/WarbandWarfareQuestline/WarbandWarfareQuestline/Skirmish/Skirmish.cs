using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands;
using WarfareAndWarbands.Warband;

namespace WarbandWarfareQuestline.Skirmish
{
    public class Skirmish : IExposable
    {
        private List<Warband> _warbads;
        private List<MapParent> _sites;
        private int _creationTick;
        protected Faction _faction;

        public virtual int Bonus => 7500;

        protected int SkirmishEndTicks => _creationTick + GenDate.TicksPerDay * 5;

        public Skirmish()
        {

        }
        public Skirmish(List<Warband> warbands, int creationTick)
        {
            this._warbads = warbands;
            this._sites = new List<MapParent>();
            this._creationTick = creationTick;
        }
        public Skirmish(List<MapParent> sites, int creationTick)
        {
            this._warbads = new List<Warband>();
            this._sites = sites;
            this._creationTick = creationTick;
        }
        public Skirmish(List<Warband> warbands, List<MapParent> sites, int creationTick)
        {
            this._warbads = warbands;
            this._sites = sites;    
            this._creationTick = creationTick;
        }

        public void SendBonus()
        {
            GameComponent_WAW.playerBankAccount.Deposit(Bonus);
            Letter l = LetterMaker.MakeLetter("WAW.SkirmishBonus".Translate(), "WAW.SkirmishBonus.Desc".Translate(Bonus), LetterDefOf.PositiveEvent);
            Find.LetterStack.ReceiveLetter(l);   
        }

        public bool HasMap()
        {
            return _warbads.Any(x => x.HasMap) && _sites.Any(x => x.HasMap);
        }

        public virtual bool ShouldGiveBonus()
        {
            return _warbads.All(x => x == null || x.Destroyed) && _sites.All(x => x == null || x.Destroyed);
        }

        public virtual void PostDestroy()
        {

        }

        public virtual void NotifyPlayer()
        {
            Messages.Message("WAW.SkirmishEvent".Translate(), lookTargets: this._warbads.First(), MessageTypeDefOf.NeutralEvent);
        }

        public virtual bool ShouldDestroy()
        {
            return SkirmishEndTicks < GenTicks.TicksGame;
        }

        public void SetFaction(Faction f)
        {
            this._faction = f;
        }

        public void PreDestroy()
        {
            _warbads.ForEach(x =>
            {
                if (x != null && !x.HasMap && !x.Destroyed)
                {
                    x.Destroy();
                }
            });
            _sites.ForEach(x =>
            {
                if (x != null && !x.HasMap && !x.Destroyed)
                {
                    x.Destroy();
                }
            });
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _warbads, "warband", LookMode.Reference);
            Scribe_Collections.Look(ref _sites, "sites", LookMode.Reference);
            Scribe_References.Look(ref _faction, "_faction");

        }
    }
}
