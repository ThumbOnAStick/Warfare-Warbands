using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;
using Verse.AI;
using Verse.Sound;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades;
using WarfareAndWarbands.Warfare.UI;

namespace WarfareAndWarbands.Warband.Mercenary
{
    public class JobDriver_RecycleVehicle : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed, false);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.ClosestTouch);
            Toil recycleVehicle = new Toil();
            recycleVehicle.initAction = delegate ()
            {
                Pawn actor = recycleVehicle.actor;
                VehiclePawn vehicle = (VehiclePawn)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                // Recycle Vehicle
                var defName = vehicle.def.defName;
                actor.TryGetComp<CompMercenary>()?.GetWarband()?.playerWarbandManager.upgradeHolder.GetUpgrade<Upgrade_Vehicle>().AddVehicle(defName);
                SoundDefOf.Building_Deconstructed.PlayOneShot(SoundInfo.InMap(vehicle));
                vehicle.Destroy();
            };
            yield return recycleVehicle;
            yield break;
        }
    }
}
