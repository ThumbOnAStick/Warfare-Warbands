using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League.Policies
{
    public class Policy : IExposable
    {

        private PolicyDef _def;
        private bool _disabled;

        public Policy() { }

        public Policy(PolicyDef def, bool disabled)
        {
            _def = def;
            _disabled = disabled;
        }

        public PolicyDef Def => this._def;
        public bool Disabled => this._disabled;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref _def, "_def");
            Scribe_Values.Look(ref _disabled, "_disabled");
        }

        public void Execute()
        {
            this._def.workerClass.Execute();
        }

        public void Tick()
        {
            this._def.workerClass.Tick();
        }

        public void Unlock()
        {
            this._disabled = false;
        }

        public bool IsParentOf(Policy p)
        {
            return p.Def.prerequisite == Def;
        }

        public bool HasPrerequisite()
        {
            return this.Def.prerequisite != null;
        }
    }
}
