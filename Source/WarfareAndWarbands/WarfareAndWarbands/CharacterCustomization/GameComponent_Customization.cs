using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.CharacterCustomization.Compatibility;
using WarfareAndWarbands.HarmonyPatches;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warfare.UI.Test;
using static System.Collections.Specialized.BitVector32;

namespace WarfareAndWarbands.CharacterCustomization
{
    public class GameComponent_Customization : GameComponent
    {
        public static GameComponent_Customization Instance;
        private List<CustomizationRequest> _customizationRequests;
        private List<PawnKindDef> _generatedKindDefs;
        private const int _equipmentBudgetLimit = 120;
        private int _equipmentBudgetLimitOffset;


        public GameComponent_Customization(Game game)
        {
            Instance = this;
            _generatedKindDefs = new List<PawnKindDef>();
            _customizationRequests = new List<CustomizationRequest>();
            _equipmentBudgetLimitOffset = 0;
        }

        public List<CustomizationRequest> CustomizationRequests => _customizationRequests;
        public List<PawnKindDef> GeneratedKindDefs => _generatedKindDefs;
        public int EquipmentBudgetLimit => _equipmentBudgetLimit + _equipmentBudgetLimitOffset;

   

        private void RemoveNulls()
        {
            _customizationRequests.RemoveAll(x => x == null);
        }

        private void GenerateAllPawnKindDef()
        {
            foreach (var request in _customizationRequests)
            {

                var factionDef = Faction.OfPlayer != null ? Faction.OfPlayer.def : FactionDefOf.OutlanderCivil;
                var defaultKindDef = CustomizationUtil.GenerateDefaultKindDef(request, factionDef);
                if (HARActive())
                {
                    defaultKindDef.race = request.GetAlienRace();
                }
                _generatedKindDefs.Add(defaultKindDef);
            }
            WarbandUtil.RefreshSoldierPawnKinds();
        }


        bool HARActive()
        {
            return ModsConfig.IsActive("erdelf.HumanoidAlienRaces");
        }

        public bool IsPawnkindCustom(string name)
        {
            return CustomizationRequests.Any(x => x.defName == name);
        }

        public PawnKindDef GetCustomDef(string name)
        {
            if (IsPawnkindCustom(name))
            {
                return _generatedKindDefs.FirstOrDefault(x => x.defName == name);
            }
            return PawnKindDefOf.Refugee;
        }


        public void AddRequest(string defName, string label, CustomizationRequest request)
        {
            request.defName = defName;  
            request.label = label;  
            AddRequest(request);
        }

        public void ReOrder(List<int> order)
        {
            _customizationRequests = order.Select(i => _customizationRequests[i]).ToList();
        }

        public void AddRequest(CustomizationRequest request)
        {
            var factionDef = Faction.OfPlayer != null ? Faction.OfPlayer.def : FactionDefOf.OutlanderCivil;
            var defaultKindDef = CustomizationUtil.GenerateDefaultKindDef(request, factionDef);
            AddRequest(defaultKindDef, request);
        }

        public void AddRequest(PawnKindDef def, CustomizationRequest request)
        {
            if (!_customizationRequests.Any(x => x.defName == def.defName))
            {
                _customizationRequests.Add(request);
                _generatedKindDefs.Add(def);
                WarbandUtil.RefreshSoldierPawnKinds();
                GameComponent_WAW.playerWarbandPreset.Refresh();
            }
        }

        public void DeleteRequest(CustomizationRequest request)
        {
            if (WarbandUtil.AllPlayerWarbandsCache.Any(x => x.bandMembers.Any(r => r.Key == request.defName && r.Value > 0)) ||
            WarbandUtil.AllPlayerRecruitingWarbandsCache.Any(x => x.BandMembers.Any(r => r.Key == request.defName && r.Value > 0)))
            {
                Messages.Message("WAW.CantDelete".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }
            if (_generatedKindDefs.Any(x => x.defName == request.defName))
            {
                _generatedKindDefs.RemoveAll(x => x.defName == request.defName);
            }
            _customizationRequests.RemoveAll(x => x.defName == request.defName);
            WarbandUtil.RefreshSoldierPawnKinds();
            GameComponent_WAW.playerWarbandPreset.Refresh();

        }

        public void DeletePawnKindDef(PawnKindDef def)
        {
            if (_generatedKindDefs.Contains(def))
            {
                _generatedKindDefs.Remove(def);
            }
            _customizationRequests.RemoveAll(x => x.defName == def.defName);

            WarbandUtil.RefreshSoldierPawnKinds();
            GameComponent_WAW.playerWarbandPreset.Refresh();
        }

        public void IncreaseLimitOffset(int amount)
        {
            _equipmentBudgetLimitOffset += amount;
        }

        public void ReduceLimitOffset(int amount)
        {
            _equipmentBudgetLimitOffset = Math.Max(0, _equipmentBudgetLimitOffset - amount);
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            GenerateAllPawnKindDef();
            RemoveNulls();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _customizationRequests, "customizationRequests", LookMode.Deep);
            Scribe_Values.Look(ref _equipmentBudgetLimitOffset, "equipmentBudgetLimitOffset");
        }
    }
}
