using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace WarfareAndWarbands.Warband.Compatibility_Vehicle
{
    public static class Vehicle
    {

        public static void DebugGenerateVehicleCaravan(int tile)
        {
            var pList = new List<Pawn>();
            for (int j = 0; j < 12; j++)
            {
                Pawn p = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                pList.Add(p);
            }
            Dictionary<string, int> vehicleTable = new Dictionary<string, int>
            {
                { "VVE_Bulldog", 3 }
            };
            GenerateVehicleCaravan(tile, pList, vehicleTable);
        }

        public static Caravan GenerateVehicleCaravan(int tile, List<Pawn> soldiers, Dictionary<string, int> vehicleTable)
        {
            List<Pawn> colonists = soldiers;
            var vList = GenerateVehiclesFromTable(vehicleTable);
            colonists.AddRange(vList);
            var vehicleCaravan = CaravanHelper.MakeVehicleCaravan(colonists, Faction.OfPlayer, tile, true);
            BoardAllCaravanPawns(vehicleCaravan);
            return vehicleCaravan;
        }

        public static List<Pawn> GenerateVehiclesFromTable(Dictionary<string, int> vehicleTable)
        {
            var allVehicles = GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(VehicleDef));
            List<Pawn> result = new List<Pawn>();
            foreach (var pair in vehicleTable)
            {
                if (!allVehicles.Any(x => x.defName == pair.Key))
                {
                    continue;
                }
                var targetDef = (VehicleDef)allVehicles.First(x => x.defName == pair.Key);
                for (int i = 0; i < pair.Value; i++)
                {
                    VehiclePawn vehicle = VehicleSpawner.GenerateVehicle(targetDef, Faction.OfPlayer);
                    vehicle.CompFueledTravel.Refuel(vehicle.CompFueledTravel.FuelCapacity);
                    if (vehicle.CompVehicleTurrets != null)
                    {
                        var turrets = vehicle.CompVehicleTurrets.Turrets;
                        foreach (var t in turrets)
                        {
                            ThingDef thingDef = t.def.ammunition.AllowedThingDefs.FirstOrDefault<ThingDef>();
                            for (int j = 0; j < 5; j++)
                            {
                                var fullStackOfAmmo = ThingMaker.MakeThing(thingDef);
                                fullStackOfAmmo.stackCount = thingDef.stackLimit;
                                float statValue = vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
                                if (MassUtility.GearAndInventoryMass(vehicle) + fullStackOfAmmo.def.BaseMass * fullStackOfAmmo.stackCount < statValue)
                                    vehicle.inventory.TryAddAndUnforbid(fullStackOfAmmo);
                            }
                        }

                    }
                    result.Add(vehicle);

                }
            }
            return result;
        }

        public static void BoardAllCaravanPawns(Caravan caravan)
        {

            List<Pawn> list = (from p in caravan.PawnsListForReading
                               where !(p is VehiclePawn)
                               select p).ToList<Pawn>();
            List<VehiclePawn> list2 = (from p in caravan.PawnsListForReading
                                       where p is VehiclePawn
                                       select p).Cast<VehiclePawn>().ToList<VehiclePawn>();


            foreach (VehiclePawn vehiclePawn in list2)
            {
                for (int i = 0; i < vehiclePawn.PawnCountToOperate; i++)
                {
                    if (list.Count <= 0)
                    {
                        return;
                    }
                    for (int j = 0; j < vehiclePawn.handlers.Count; j++)
                    {
                        var vehicleHandler = vehiclePawn.handlers[j];
                        if (vehicleHandler.AreSlotsAvailable)
                        {
                            vehicleHandler.thingOwner.TryAddOrTransfer(list.Pop<Pawn>());
                            break;
                        }
                    }
                }
            }

            if (list.Count > 0)
            {
                int num = 0;
                while (list.Count > 0)
                {
                    VehiclePawn vehiclePawn2 = list2[num];
                    for (int j = 0; j < vehiclePawn2.handlers.Count; j++)
                    {
                        var vehicleHandler2 = vehiclePawn2.handlers[j];
                        if (vehicleHandler2.AreSlotsAvailable)
                        {
                            vehicleHandler2.thingOwner.TryAddOrTransfer(list.Pop<Pawn>());
                            break;
                        }
                    }
                    num = ((num + 2 > list2.Count) ? 0 : (++num));
                    int aviableHandlerCount = 0;
                    for (int j = 0; j < list2.Count; j++)
                    {
                        var vehicle = list2[j];
                        for (int k = 0; k < vehicle.handlers.Count; k++)
                        {
                            if (vehicle.handlers[k].AreSlotsAvailable)
                            {
                                aviableHandlerCount++;
                            }
                        }
                    }
                    if (aviableHandlerCount < 1)
                    {
                        break;
                    }
                }
            }
        }

        public static void RecycleVehicleTargetor(Pawn caster)
        {
            Find.Targeter.BeginTargeting(
                new TargetingParameters() { validator = x => x.Thing is VehiclePawn },
                delegate (LocalTargetInfo target)
                {
                    TryToRecycleVehicle(caster, target.Thing);
                });
            
        }

        public static void TryToRecycleVehicle(Pawn pawn, Thing vehicle)
        {
            Job job = new Job(WAWDefof.WAWRecycleVehicle, vehicle);
            pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
        }

    }
}
