using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VanillaPsycastsExpanded;
using VFECore.Abilities;
using UnityEngine;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades.Psycaster;

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

        public static Texture GetBackGroundImageTexture(string pathDefName)
        {
            var allPaths = GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(PsycasterPathDef));
            if(allPaths.Any(x => x.defName == pathDefName))
            {
                var rndPath = (PsycasterPathDef)allPaths.First(x => x.defName == pathDefName);
                return rndPath.altBackgroundImage;
            }
            return null;
        }

        public static void GivePathAbilitiesToPawn(Pawn p, List<string> pathDefNames)
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
                hediff.ImproveStats(10);
            }
            p.psychicEntropy.RechargePsyfocus();
            var allPaths = GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(PsycasterPathDef));
            foreach (string pathDefName in pathDefNames)
            {
                var rndPath = (PsycasterPathDef)allPaths.First(x => x.defName == pathDefName);
                foreach (Def def in rndPath.abilities)
                {
                    VFECore.Abilities.AbilityDef abilityDef = (VFECore.Abilities.AbilityDef)def;
                    p.TryGetComp<CompAbilities>()?.GiveAbility(abilityDef);
                }
            }
        }

        public static string GetPsyPathLabel(this PsycasterInfo info)
        {
            if(info.TargetPsycasterPaths.Count < 1)
            {
                return "";
            }
            var allPaths = GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(PsycasterPathDef));
            if (!allPaths.Any(x => x.defName == info.TargetPsycasterPaths.First()))
            {
                return "";
            }
            var rndPath = (PsycasterPathDef)allPaths.First(x => x.defName == info.TargetPsycasterPaths.First());
            return rndPath.label;
        }

        public static bool TryToSpendEltexFromColony(Map currentMap, int cost)
        {
            if (currentMap == null)
            {
                currentMap = Find.AnyPlayerHomeMap;
            }
            if (cost <= 0)
            {
                return true;
            }
            IEnumerable<Thing> eltexes = from x in currentMap.listerThings.AllThings
                                         where x.def == VPE_DefOf.VPE_Eltex && x.IsInAnyStorage()
                                         select x;
            if (eltexes.Sum((Thing t) => t.stackCount) < cost)
            {
                Messages.Message("WAW.CantAffordEltex".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }
            int debt = cost;
            for (int i = 0; i < eltexes.Count() && debt > 0; i++)
            {
                Thing eltex = null;
                foreach (Thing item2 in currentMap.thingGrid.ThingsAt(eltexes.ElementAt(i).Position))
                {
                    if (item2.def == VPE_DefOf.VPE_Eltex)
                    {
                        eltex = item2;
                        break;
                    }
                }

                if (eltex == null)
                {
                    continue;
                }

                if (debt >= eltex.stackCount)
                {
                    debt -= eltex.stackCount;
                    eltex.Destroy();
                }
                else
                {
                    eltex = eltex.SplitOff(debt);
                    debt = 0;
                }

            }
            return true;
        }
    }
}
