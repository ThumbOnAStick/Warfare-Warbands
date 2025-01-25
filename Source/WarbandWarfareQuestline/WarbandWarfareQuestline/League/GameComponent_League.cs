using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.League.UI;
using WarbandWarfareQuestline.Questline;
using WarfareAndWarbands;
using WarfareAndWarbands.Warband;

namespace WarbandWarfareQuestline.League
{
    public class GameComponent_League:GameComponent
    {
        public static GameComponent_League Instance;
        private int _lastCheckTick = 0;
        private List<MinorFaction> _minorFactions;
        private List<MinorFaction> _minorFactionsTemp;
        private readonly int policyGenerationDays = 15;
        private readonly int eventGenerationDays = 15;
        

        public GameComponent_League(Game game)
        {
            Instance = this;
            _minorFactions = new List<MinorFaction>();
            _minorFactionsTemp = new List<MinorFaction>();
        }

        public int EventGenrationTicks => eventGenerationDays * GenDate.TicksPerDay;
        public List<MinorFaction> Factions => _minorFactions;
        public List<MinorFaction> FactionsTemp => _minorFactionsTemp;

        bool ShouldCheckNow()
        {
            return GenTicks.TicksGame - _lastCheckTick > EventGenrationTicks;
        }

        void ResetLastCheckTick()
        {
            _lastCheckTick = GenTicks.TicksGame;
        }

        float GetTaxDiscount()
        {
            var allAvialablePlayerWarbands = WarbandUtil.AllActivePlayerWarbands();
            float discount = 0;
            foreach (var w in allAvialablePlayerWarbands)
            {
                if (w.playerWarbandManager.upgradeHolder.HasUpgrade)
                {
                    discount += w.playerWarbandManager.upgradeHolder.SelectedUpgrade.Wage;
                }
            }
            return discount;
        }

        int GetTax()
        {
            int totalTax = 0;
            foreach (var f in _minorFactions)
            {
                totalTax += f.Tax;
            }
            return totalTax; 
        }


        int GetTax(out float discount)
        {
            int totalTax = GetTax();
            discount = GetTaxDiscount();
            totalTax = (int)(totalTax * (1 - discount));
            return totalTax;
        }

        void DepositTax(out int totalTax, out float discount)
        {
            totalTax = GetTax(out discount);
            GameComponent_WAW.playerBankAccount.Deposit(totalTax);
        }

        bool ShouldPayTax()
        {
            return _minorFactions.Count > 0;
        }

        void PayTax()
        {
            if (!ShouldPayTax())
            {
                return;
            }
            DepositTax(out int totalTax, out float discount);
            NotifyTaxPayed(totalTax, discount);
        }

        void NotifyTaxPayed(int amount, float discount)
        {
            TaggedString stringBuilder = "WAW.TaxPaid".Translate(amount);
            if (discount > 0)
            {
                stringBuilder += "(" + "WAW.TaxWageDiscount".Translate((discount * 100).ToString("0.00")) + ")";
            }
            Messages.Message(stringBuilder, MessageTypeDefOf.NeutralEvent);
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (ShouldCheckNow())
            {
                PayTax();
                ResetLastCheckTick();
            }
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            if (!Prefs.DevMode)
            {
                return;
            }
            Quests.GiveVillageQuest();
            Window_League.AppendDrawingEvent();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            Window_League.AppendDrawingEvent();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Log.Message("WAW: league module active");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _minorFactions, "_minorFactions", LookMode.Deep);
            Scribe_Collections.Look(ref _minorFactionsTemp, "_minorFactionsTemp", LookMode.Deep);
        }

    }
}
