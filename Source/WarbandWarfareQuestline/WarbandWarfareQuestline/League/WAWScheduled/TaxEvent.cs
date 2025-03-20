using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands;
using WarbandWarfareQuestline.League.Policies;

namespace WarbandWarfareQuestline.League.WAWScheduled
{
    internal class TaxEvent: WAWScheduledEvent
    {

        public TaxEvent()
        {

        }

        float GetTaxBonus()
        {
            float result = 0;
            result += GameComponent_League.Instance.PolicyTree.GetTaxBonus();  
            return result;
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

        int GetTaxRaw()
        {
            int totalTax = 0;
            foreach (var f in GameComponent_League.Instance.Factions)
            {
                totalTax += f.Tax;
            }
            return totalTax;
        }


        int GetTax(out float discount, out float bonus)
        {
            int totalTax = GetTaxRaw();
            discount = GetTaxDiscount();
            bonus = GetTaxBonus();
            totalTax = (int)(totalTax * (1 - discount + bonus));
            return totalTax;
        }

        void DepositTax(out int totalTax, out float discount, out float bonus)
        {
            totalTax = GetTax(out discount, out bonus);
            GameComponent_WAW.playerBankAccount.Deposit(totalTax);
        }

        bool CanPayTax()
        {
            return GameComponent_League.Instance.Factions.Count > 0;
        }

        void PayTax()
        {
            if (!CanPayTax())
            {
                return;
            }
            DepositTax(out int totalTax, out float discount, out float bonus);
            NotifyTaxPayed(totalTax, discount + bonus);
        }


        void NotifyTaxPayed(int amount, float rate)
        {
            TaggedString stringBuilder = "WAW.TaxPaid".Translate(amount);
            if (rate != 0)
            {
                stringBuilder += "(" + (rate * 100).ToString("0.0") + "%)";
            }
            Messages.Message(stringBuilder, MessageTypeDefOf.NeutralEvent);
        }

        public override void MakeAction()
        {
            base.MakeAction();
            PayTax();
        }
    }
}
