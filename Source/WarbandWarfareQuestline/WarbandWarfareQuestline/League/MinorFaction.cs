using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands;
using static System.Collections.Specialized.BitVector32;

namespace WarbandWarfareQuestline
{
    public class MinorFaction : IExposable, ILoadReferenceable
    {
        private string _factionName;
        private FactionTraitDef _trait;
        private TechLevel _techLevel;
        private Color _factionColor;
        private string ID;

        public MinorFaction()
        {
            this.ID = Guid.NewGuid().ToString();
        }
        public MinorFaction(FactionTraitDef trait, TechLevel level, Color factionColor) : this()
        {
            this._trait = trait;
            this._techLevel = level;
            this._factionColor = factionColor;
        }
        public MinorFaction(string factionName, FactionTraitDef trait, TechLevel level, Color factionColor)
        {
            this._factionName = factionName;
            this._trait = trait;    
            this._techLevel = level;    
            this._factionColor = factionColor;
        }

        public string FactionName => _factionName;
        public FactionTraitDef Trait => _trait;
        public string FactionID => this.ID;
        public Color FactionColor => _factionColor;

        public void Init()
        {
            GenerateName();
        }

        public void GenerateName()
        {
            if (this._techLevel < TechLevel.Industrial)
            {
                _factionName = NameGenerator.GenerateName(FactionDefOf.PlayerTribe.factionNameMaker, from fac in Find.FactionManager.AllFactionsVisible select fac.Name, false, null);
            }
            else
            {
                _factionName = NameGenerator.GenerateName(FactionDefOf.PlayerColony.factionNameMaker, from fac in Find.FactionManager.AllFactionsVisible select fac.Name, false, null);
            }
        }

        public string NameForSettlement()
        {
            if (this._techLevel < TechLevel.Industrial)
            {
                return NameGenerator.GenerateName(FactionDefOf.PlayerTribe.settlementNameMaker);
            }
            else
            {
                return NameGenerator.GenerateName(FactionDefOf.PlayerColony.settlementNameMaker);
            }
        }

        public Texture2D GetFactionIcon()
        {
            Texture2D result = this._techLevel < TechLevel.Industrial? WAWTex.Village : WAWTex.Town;
            return result;
        }


        public void ExposeData()
        {
            Scribe_Values.Look(ref this._techLevel, "_level");
            Scribe_Values.Look(ref _factionName, "_factionName");
            Scribe_Values.Look(ref _factionColor, "_factionColor");
            Scribe_Values.Look(ref ID, "ID");
            Scribe_Defs.Look(ref _trait, "_trait");
        }

        public string GetUniqueLoadID()
        {
            return "MinorFaction_" + ID;
        }
    }
}
