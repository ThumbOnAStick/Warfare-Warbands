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
        private List<string> _eventList;

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
                    Register(SkirmishHelper.CreateRandomSkirmish());
                    break;

                case siegeString:
                    Register(SkirmishHelper.CreateSiege());
                    break;
            }
        }

        public void Register(Skirmish skirmish)
        {
            _skirmishes.Add(skirmish);
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            for (int i = 0; i < _skirmishes.Count; i++)
            {
                var skirmish = _skirmishes[i];
                if (skirmish.ShouldDestroy())
                {
                    DestroySkirmish(skirmish);
                    return;
                }
                if (skirmish.ShouldGiveBonus())
                {
                    skirmish.SendBonus();
                    DestroySkirmish(skirmish);
                }
            }
        }

        void DestroySkirmish(Skirmish skirmish)
        {
            skirmish.PreDestroy();
            _skirmishes.Remove(skirmish);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _skirmishes, "skirmishes");
        }


    }
}
