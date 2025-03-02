using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands;

namespace WarbandWarfareQuestline.League.WAWScheduled
{
    internal class TaxEvent: WAWScheduledEvent
    {

        public TaxEvent()
        {

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
            foreach (var f in GameComponent_League.Instance.Factions)
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

        public override void MakeAction()
        {
            base.MakeAction();
            PayTax();
        }
    }
}
