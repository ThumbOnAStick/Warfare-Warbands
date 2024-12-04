using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.HarmonyPatches;
using WarfareAndWarbands.Warband;
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
        }

        private void GenerateAllPawnKindDef()
        {
            foreach (var request in customizationRequests)
            {
                var defaultKindDef = CustomizationUtil.GenerateDefaultKindDef(request);
                generatedKindDefs.Add(defaultKindDef);
            }
            WarbandUtil.Refresh();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref customizationRequests, "customizationRequests", LookMode.Deep);


        }


        public void AddPawnKindDefFromRequest(string defName, string label, CustomizationRequest request)
        {
            request.defName = defName;  
            request.label = label;  
            var defaultKindDef = CustomizationUtil.GenerateDefaultKindDef(request);
            AddPawnKindDef(defaultKindDef, request);
        }

        public void AddPawnKindDef(PawnKindDef def, CustomizationRequest request)
        {
            if (!customizationRequests.Any(x => x.defName == def.defName))
            {
                customizationRequests.Add(request);
                generatedKindDefs.Add(def);
            }
            WarbandUtil.Refresh();
            GameComponent_WAW.playerWarband.Refresh();
        }

        public void DeleteRequest(CustomizationRequest request)
        {
            if (generatedKindDefs.Any(x => x.defName == request.defName))
            {
                generatedKindDefs.RemoveAll(x => x.defName == request.defName);
            }
            customizationRequests.RemoveAll(x => x.defName == request.defName);
            WarbandUtil.Refresh();
            GameComponent_WAW.playerWarband.Refresh();
        }

        public void DeletePawnKindDef(PawnKindDef def)
        {
            if (generatedKindDefs.Contains(def))
                generatedKindDefs.Remove(def);
            customizationRequests.RemoveAll(x => x.defName == def.defName);
            WarbandUtil.Refresh();
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



    }
}
