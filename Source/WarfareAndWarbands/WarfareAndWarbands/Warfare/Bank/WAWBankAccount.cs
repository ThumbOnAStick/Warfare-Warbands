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
        private readonly float _interestRate = 0.05f;
       
        public WAWBankAccount()
        {

        }

        public WAWBankAccount(int balance)
        {
            this._balance = balance;
        }

        public int Balance => this._balance;
        public float GrowthRatio => this._interestRate + 1;
        public int Interest => (int)(this.Balance * this._interestRate);

        public void ReturnInterestPerSeason()
        {
            this._balance += this.Interest;
        }

        public void Deposit(int amount)
        {
            this._balance += amount;
        }

        public void Spend(int amount)
        {
            if(CanSpend())
            this._balance -= amount; 
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
