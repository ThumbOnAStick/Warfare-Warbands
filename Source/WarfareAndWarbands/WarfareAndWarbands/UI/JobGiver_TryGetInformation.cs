using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;

namespace WarfareAndWarbands.UI
{
    public class JobGiver_TryGetInformation : JobDriver
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
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate (Toil to)
            {
                Building_CommsConsole building_CommsConsole = (Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                return !building_CommsConsole.CanUseCommsNow;
            });
            Toil openComms = new Toil();
            openComms.initAction = delegate ()
            {
                Pawn actor = openComms.actor;
                Building_CommsConsole building_CommsConsole = (Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                bool canUseCommsNow = building_CommsConsole.CanUseCommsNow;
                if (canUseCommsNow)
                {
                    Find.WindowStack.Add(new Window_WAW(actor, actor.Map));
                }
            };
            yield return openComms;
            yield break;
        }
    }
}
