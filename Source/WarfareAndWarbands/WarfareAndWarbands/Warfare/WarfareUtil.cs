using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband;

namespace WarfareAndWarbands
{
    public static class WarfareUtil
    {
        public static readonly string warProgressString1 = "factionLosing".Translate();
        public static readonly string warProgressString2 = "factionStrikeToSurvive".Translate();
        public static readonly string warProgressString3 = "factionWillingToFight".Translate();
        public static readonly string warProgressString4 = "factionHungerForWar".Translate();



        public static GameComponent_WAW CompWAW
        {
            get
            {
                return GameComponent_WAW.Instance;
            }
        }

        public static void AddToCompWAW(Faction f)
        {
            CompWAW.AppendFactionInfoToTable(f);
        }

        [DebugAction("WAW", "decrease war durability by one", actionType = DebugActionType.Action)]
        public static void DecreaseDurabilityByOneAction()
        {
            IEnumerable<FloatMenuOption> opts = DecreaseDurabilityByOptions(1);
            Find.WindowStack.Add(new FloatMenu(opts.ToList()));

        }

        [DebugAction("WAW", "decrease war durability by ten", actionType = DebugActionType.Action)]
        public static void DecreaseDurabilityByTenAction()
        {
            IEnumerable<FloatMenuOption> opts = DecreaseDurabilityByOptions(10);
            Find.WindowStack.Add(new FloatMenu(opts.ToList()));

        }

        [DebugAction("WAW", "decrease war durability by fifty", actionType = DebugActionType.Action)]
        public static void DecreaseDurabilityByFiftyAction()
        {
            IEnumerable<FloatMenuOption> opts = DecreaseDurabilityByOptions(50);
            Find.WindowStack.Add(new FloatMenu(opts.ToList()));
        }


        public static void TryToDefeatFaction(Faction f)
        {
            if (!WAWSettings.enableFactionDefeat)
            {
                return;
            }
            if(f.IsPlayer || f.Hidden)
            {
                return;
            }
            
            foreach (var facBase in Find.WorldObjects.SettlementBases.FindAll(b => b.Faction == f))
            {
                if(!facBase.HasMap && !facBase.Destroyed)
                {
                    facBase.Destroy();
                }
            }
            f.defeated = true;
        }


        public static string GetWarDurabilityString(this Faction f)
        {
            int warProgress = CompWAW.GetDurability(f);
            string result = "";
            if (warProgress < 25)
            {
                result = warProgressString1;
            }
            else if (warProgress < 50)
            {
                result = warProgressString2;
            }
            else if (warProgress < 75)
            {
                result = warProgressString3;
            }
            else
            {
                result = warProgressString4;
            }
            result += $"({warProgress}/100)";
            return result;
        }


        public static HashSet<Faction> GetValidWarFactions()
        {
            return Find.FactionManager.AllFactions.Where(f => IsValidWarFaction(f)).ToHashSet();
        }

        public static bool IsValidWarFaction(Faction f)
        {
            return !f.IsPlayer && !f.defeated && !f.Hidden;
        }
        public static IEnumerable<FloatMenuOption> DecreaseDurabilityByOptions(int amount)
        {
            HashSet<Faction> valids = GetValidWarFactions();
            foreach (Faction f in valids)
            {
                yield return new FloatMenuOption($"faction: {f.Name}",  delegate {CompWAW.DecreaseDurabilityBy(f, amount); });
            }
        }


        public static WorldObject RandomHostileSettlement(Faction myFaction)
        {
            if (Find.WorldObjects.MapParents.Any(x => x.Faction != null && x.Faction.HostileTo(myFaction)))
                return Find.WorldObjects.MapParents.Where(x => x.Faction != null && x.Faction != Faction.OfPlayer && x.Faction.HostileTo(myFaction)).RandomElement();
            return null;
        }

        public static void SpawnWarbandTargetingBase(Faction f, GlobalTargetInfo info)
        {
            List<SitePartDef> sitePartList = new List<SitePartDef>
            {
                DefDatabase<SitePartDef>.GetNamed("Outpost")
            };
            List<SitePartDefWithParams> sitePartDefsWithParams;
            SiteMakerHelper.GenerateDefaultParams(0f, info.Tile, f, sitePartList, out sitePartDefsWithParams);
            var warband = (Warband.Warband)WorldObjectMaker.MakeWorldObject(WAWDefof.WAW_Warband);
            TileFinder.TryFindNewSiteTile(out int warbandTile, 3, 7, false, TileFinderMode.Near, info.Tile);
            warband.Tile = warbandTile;
            warband.SetFaction(f);
            warband.npcWarbandManager.SetTargetTile(info.Tile);
            if (sitePartDefsWithParams != null)
            {
                foreach (SitePartDefWithParams sitePart in sitePartDefsWithParams)
                {
                    warband.AddPart(new SitePart(warband, sitePart.def, sitePart.parms));
                }
            }
            Find.WorldObjects.Add(warband);
        }

    }
}
