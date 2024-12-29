using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.Compatibility_VPE;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades.Psycaster
{
    public class PsycasterInfo
    {
        CustomizationRequest _targetRequest;
        string _targetPsycasterPath;

        public PsycasterInfo()
        {

        }

        public PsycasterInfo(CustomizationRequest request, string psycasterPath)
        {
            _targetPsycasterPath = psycasterPath;
            _targetRequest = request;
        }

        public Pawn CreatePsycaster()
        {
            if(Find.FactionManager.OfEmpire == null)
            {
                return null;
            }

            Pawn pawn;
            var defaultPawnKind = CustomizationUtil.GenerateDefaultKindDef(_targetRequest, FactionDefOf.Empire);
            try
            {
                pawn = PawnGenerator.GeneratePawn(defaultPawnKind, Faction.OfEmpire);
                VPE.GivePathAbilitiesToPawn(pawn, this._targetPsycasterPath);
                this._targetRequest.CustomizePawn(ref pawn);
                pawn.SetFaction(Faction.OfPlayer);
                return pawn;
            }catch(Exception ex)
            {
                Log.Message($"WAW: An error occured while trying to create an imperial pawn: {ex}");
                return null;
            }

        }
    }
}
