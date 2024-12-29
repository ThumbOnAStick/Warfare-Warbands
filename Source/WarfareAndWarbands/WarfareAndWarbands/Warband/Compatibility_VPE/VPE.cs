using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VanillaPsycastsExpanded;
using VFECore.Abilities;

namespace WarfareAndWarbands.Warband.Compatibility_VPE
{
    public static class VPE
    {
        public static void GiveRandomPathAbilitiesToPawn(Pawn p)
        {
            if (p.psychicEntropy == null)
            {
                return;
            }
            if (p.psychicEntropy.PsychicSensitivity <= 0)
            {
                return;
            }
            p.ChangePsylinkLevel(1);
            var hediff = p.Psycasts();
            if (hediff != null)
            {
                hediff.SetLevelTo(15);
                hediff.SpentPoints(hediff.points);
            }
            p.psychicEntropy.RechargePsyfocus();
            var allPaths = GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(PsycasterPathDef));
            var rndPath = (PsycasterPathDef)allPaths.RandomElement();
            foreach (Def def in rndPath.abilities)
            {
                VFECore.Abilities.AbilityDef abilityDef = (VFECore.Abilities.AbilityDef)def;
                p.TryGetComp<CompAbilities>()?.GiveAbility(abilityDef);
            }
        }

        public static void GivePathAbilitiesToPawn(Pawn p, string pathDefName)
        {
            if (p.psychicEntropy == null)
            {
                return;
            }
            if (p.psychicEntropy.PsychicSensitivity <= 0)
            {
                return;
            }
            p.ChangePsylinkLevel(1);
            var hediff = p.Psycasts();
            if (hediff != null)
            {
                hediff.SetLevelTo(15);
                hediff.SpentPoints(hediff.points);
            }
            p.psychicEntropy.RechargePsyfocus();
            var allPaths = GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(PsycasterPathDef));
            var rndPath = (PsycasterPathDef)allPaths.First(x => x.defName == pathDefName);
            foreach (Def def in rndPath.abilities)
            {
                VFECore.Abilities.AbilityDef abilityDef = (VFECore.Abilities.AbilityDef)def;
                p.TryGetComp<CompAbilities>()?.GiveAbility(abilityDef);
            }
        }
    }
}
