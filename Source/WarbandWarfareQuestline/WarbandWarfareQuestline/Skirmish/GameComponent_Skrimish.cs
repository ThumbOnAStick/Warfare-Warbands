using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.Skirmish
{
    public class GameComponent_Skrimish : GameComponent
    {
        private List<Skirmish> _skirmishes;
        private readonly List<string> _eventList;

        public static GameComponent_Skrimish Instance;
        private const string skirmishString = "Skirmish";
        private const string siegeString = "Siege";

        public GameComponent_Skrimish(Game game)
        {
            _skirmishes = new List<Skirmish>();
            _eventList = new List<string>() { skirmishString, siegeString };
            Instance = this;
        }

        public void CreateRandomSkirmsish()
        {
            var val = _eventList.RandomElement();
            switch (val)
            {
                case skirmishString:
                    Skirmish skirmish = SkirmishHelper.CreateRandomSkirmish();
                    if (skirmish != null)
                    {
                        Register(skirmish);
                    }
                    break;

                case siegeString:
                    Skirmish siege = SkirmishHelper.CreateSiege();
                    if (siege != null)
                    {
                        Register(siege);
                    }
                    break;
            }
        }

        public void Register(Skirmish skirmish)
        {
            if (skirmish == null)
                return;
                
            if (_skirmishes == null)
                _skirmishes = new List<Skirmish>();
                
            _skirmishes.Add(skirmish);
        }

        public void OnWorldObjectDestroyed()
        {
            if (_skirmishes == null)
            {
                _skirmishes = new List<Skirmish>();
                return;
            }
            
            _skirmishes.RemoveAll(s => s == null);
            
            for (int i = 0; i < _skirmishes.Count; i++)
            {
                var skirmish = _skirmishes[i];
                if (skirmish == null)
                    continue;
                    
                if (skirmish.ShouldGiveBonus())
                {
                    skirmish.SendBonus();
                    DestroySkirmish(skirmish);
                    return;
                }
            }
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if(GenTicks.TicksGame % 100 != 0)
            {
                return;
            }
            
            if (_skirmishes == null)
            {
                _skirmishes = new List<Skirmish>();
                return;
            }
            
            _skirmishes.RemoveAll(s => s == null);
            
            for (int i = 0; i < _skirmishes.Count; i++)
            {
                var skirmish = _skirmishes[i];
                if (skirmish == null)
                    continue;
                    
                if (skirmish.ShouldDestroy())
                {
                    DestroySkirmish(skirmish);
                    return;
                }
            }
        }

        void DestroySkirmish(Skirmish skirmish)
        {
            if (skirmish == null)
                return;
            
            if (_skirmishes == null)
                return;
                
            if (_skirmishes.Contains(skirmish))
                _skirmishes.Remove(skirmish);
            
            skirmish.PreDestroy();
            skirmish.PostDestroy();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _skirmishes, "skirmishes", LookMode.Deep);
            if(_skirmishes == null)
            {
                _skirmishes = new List<Skirmish>();
            }
        }


    }
}
