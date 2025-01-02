using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.Compatibility_VPE;
using WarfareAndWarbands.Warband.Mercenary;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades.Psycaster
{
    public class PsycasterInfo:IExposable
    {
        CustomizationRequest _targetRequest;
        List<string> _targetPsycasterPaths;

        public PsycasterInfo()
        {

        }

        public PsycasterInfo(CustomizationRequest request, List<string> psycasterPaths)
        {
            _targetPsycasterPaths = psycasterPaths;
            _targetRequest = request;
        }

        public List<string> TargetPsycasterPaths => _targetPsycasterPaths;

        public Texture TargetPsycasterPathTex()
        {
            return VPE.GetBackGroundImageTexture(_targetPsycasterPaths.First());
        }
        public Texture TargetPsycasterPathTex(int i)
        {
            return VPE.GetBackGroundImageTexture(_targetPsycasterPaths[i]);
        }
        public IEnumerable<Texture> AllPsycasterPathTex()
        {
            return _targetPsycasterPaths.Select(x => VPE.GetBackGroundImageTexture(x));
        }

        public int GetEltexCost()
        {
            return this._targetPsycasterPaths.Count;
        }


        public Pawn CreatePsycaster(Warband warband)
        {
            Pawn pawn = CreatePsycaster();
            MercenaryUtil.SetMercenaryComp(pawn, warband, "", Faction.OfPlayer);
            return pawn;
        }

        public Pawn CreatePsycaster()
        {
            if (Find.FactionManager.OfEmpire == null)
            {
                return null;
            }

            Pawn pawn;
            var defaultPawnKind = CustomizationUtil.GenerateDefaultKindDef(_targetRequest, FactionDefOf.Empire);
            try
            {
                pawn = PawnGenerator.GeneratePawn(defaultPawnKind, Faction.OfEmpire);
                VPE.GivePathAbilitiesToPawn(pawn, this._targetPsycasterPaths);
                this._targetRequest.CustomizePawn(ref pawn);
                pawn.TryGetComp<CompMercenary>()?.SetEmpireBackground();
                return pawn;
            }
            catch (Exception ex)
            {
                Log.Message($"WAW: An error occured while trying to create an imperial pawn: {ex}");
                return null;
            }

        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _targetPsycasterPaths, "_targetPsycasterPaths");
            Scribe_Deep.Look(ref this._targetRequest, "_targetRequest");
        }
    }
}
