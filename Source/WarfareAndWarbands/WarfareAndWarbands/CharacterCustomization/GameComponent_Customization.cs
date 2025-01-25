using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        public List<CustomizationRequest> customizationRequests;
        public List<PawnKindDef> generatedKindDefs;



        public GameComponent_Customization(Game game)
        {
            Instance = this;
            generatedKindDefs = new List<PawnKindDef>();
            customizationRequests = new List<CustomizationRequest>();
        }
        public override void StartedNewGame()
        {
            base.StartedNewGame();

        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            GenerateAllPawnKindDef();
            RemoveNulls();
        }

        private void RemoveNulls()
        {
            customizationRequests.RemoveAll(x => x == null);
        }

        private void GenerateAllPawnKindDef()
        {
            foreach (var request in customizationRequests)
            {

                var factionDef = Faction.OfPlayer != null ? Faction.OfPlayer.def : FactionDefOf.OutlanderCivil;
                var defaultKindDef = CustomizationUtil.GenerateDefaultKindDef(request, factionDef);
                if (HARActive())
                {
                    defaultKindDef.race = request.GetAlienRace();
                }
                generatedKindDefs.Add(defaultKindDef);
            }
            WarbandUtil.RefreshSoldierPawnKinds();
        }


        bool HARActive()
        {
            return ModsConfig.IsActive("erdelf.HumanoidAlienRaces");
        }


        public void AddRequest(string defName, string label, CustomizationRequest request)
        {
            request.defName = defName;  
            request.label = label;  
            AddRequest(request);
        }

        public void AddRequest(CustomizationRequest request)
        {
            var factionDef = Faction.OfPlayer != null ? Faction.OfPlayer.def : FactionDefOf.OutlanderCivil;
            var defaultKindDef = CustomizationUtil.GenerateDefaultKindDef(request, factionDef);
            AddRequest(defaultKindDef, request);
        }

        public void AddRequest(PawnKindDef def, CustomizationRequest request)
        {
            if (!customizationRequests.Any(x => x.defName == def.defName))
            {
                customizationRequests.Add(request);
                generatedKindDefs.Add(def);
                WarbandUtil.RefreshSoldierPawnKinds();
                GameComponent_WAW.playerWarband.Refresh();
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
            if (generatedKindDefs.Any(x => x.defName == request.defName))
            {
                generatedKindDefs.RemoveAll(x => x.defName == request.defName);
            }
            customizationRequests.RemoveAll(x => x.defName == request.defName);
            WarbandUtil.RefreshSoldierPawnKinds();
            GameComponent_WAW.playerWarband.Refresh();

        }

        public void DeletePawnKindDef(PawnKindDef def)
        {
            if (generatedKindDefs.Contains(def))
            {
                generatedKindDefs.Remove(def);
            }
            customizationRequests.RemoveAll(x => x.defName == def.defName);

            WarbandUtil.RefreshSoldierPawnKinds();
            GameComponent_WAW.playerWarband.Refresh();
        }


        void AddClothes(CustomizationRequest request)
        {
            request.AddApperal("Apparel_HatHood");
            request.AddApperal("Apparel_Pants");
            request.AddApperal("Apparel_BasicShirt");

        }

        void AddWeapons(CustomizationRequest request)
        {
            ThingDef revolverdef = DefDatabase<ThingDef>.GetNamed("Gun_Revolver");
            request.weaponRequest = revolverdef;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref customizationRequests, "customizationRequests", LookMode.Deep);
        }
    }
}
