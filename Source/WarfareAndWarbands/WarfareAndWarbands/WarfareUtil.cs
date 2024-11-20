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


        public static void DefeatFaction(Faction f)
        {
            foreach (var facBase in Find.WorldObjects.SettlementBases.FindAll(b => b.Faction == f))
            {
                if(!facBase.HasMap)
                {
                    facBase.Destroy();
                }
            }
            f.defeated = true;
        }


        public static string GetWarDurabilityString(Faction f)
        {
            int warProgress = CompWAW.GetDurability(f);
            if (warProgress < 25)
            {
                return warProgressString1;
            }
            else if (warProgress < 50)
            {
                return warProgressString2;
            }
            else if (warProgress < 75)
            {
                return warProgressString3;
            }
            else
            {
                return warProgressString4;
            }
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
    }
}
