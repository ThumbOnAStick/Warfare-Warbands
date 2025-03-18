using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace WarfareAndWarbands
{
    [DefOf]
    public static class WAWDefof
    {
        public static JobDef GetInformationFromConsole;
        public static JobDef WAWRecycleVehicle;
        public static FactionDef PlayerWarband;
        public static WorldObjectDef WAW_Warband;
        public static WorldObjectDef WAW_WarbandVassal;
        public static WorldObjectDef WAW_WarbandRecruiting;
        public static WorldObjectDef WAW_MinorFactionSettlement;
        public static WorldObjectDef WAW_SettlementConstruction;
        public static ThingDef WAW_LootChest;
        public static ThingDef LootChestIncoming;
        public static SitePartDef WAWEmptySite;
        public static ThingDef ActiveDropPodLootChest;
        public static QuestScriptDef WAW_SaveVillage;
    }
}
