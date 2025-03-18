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
        private List<Policy> _children;

        public Policy() 
        {
            _children = new List<Policy>();
        }

        public Policy(PolicyDef def, bool disabled) : this()
        {
            _def = def;
            _disabled = disabled;

        }

        public PolicyDef Def => this._def;
        public bool Disabled => this._disabled;
        public List<Policy> Children => this._children;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref _def, "_def");
            Scribe_Values.Look(ref _disabled, "_disabled");
            Scribe_Collections.Look(ref _children, "_children", LookMode.Deep);
        }

        public void Execute()
        {
            this._disabled = false;
            this._def.workerClass?.Execute();
        }

        public void Tick()
        {
            this._def.workerClass?.Tick();
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

        public void AddChild(Policy p)
        {
            this._children.Add(p);  
        }

    }
}
