using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warfare.Bank
{
    public class WAWBankAccount:IExposable
    {
        private int _balance;
        private bool _expanseReductionAvailable;
        private const float _interestRate = 0.05f;
        private const float _expenseReduction = 0.1f;

        public WAWBankAccount()
        {
            this._balance = 3000;
        }

        public WAWBankAccount(int balance)
        {
            this._balance = balance;
        }

        public int Balance => this._balance;
        public float GrowthRatio => _interestRate + 1;
        public int Interest => (int)(this.Balance * _interestRate);
        public bool ExpanseReductionAvailable => _expanseReductionAvailable;

        public void ReturnInterestPerSeason()
        {
            this._balance += this.Interest;
        }

        public void Deposit(int amount)
        {
            this._balance += amount;
        }

        // Activate
        public void ActivateExpanseReduction()
        {
            this._expanseReductionAvailable = true;
        }

        //Deactivate
        public void DeActivateExpanseReduction()
        {
            this._expanseReductionAvailable = false;
        }

        public void Spend(int amount)
        {
            if (CanSpend())
            {
                int actualAmount = _expanseReductionAvailable ? (int)(amount * (1f- _expenseReduction)) : amount;
                this._balance -= actualAmount; 
            }

        }

        public bool CanSpend()
        {
            return this._balance > 0;
        }



        public void ExposeData()
        {
            Scribe_Values.Look(ref _balance, "balance", 0);
        }
    }
}
