using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI.Group;
using Verse;
using RimWorld.Planet;

namespace WarfareAndWarbands.Warband.WarbandRecruiting
{
    public static class DebugAction_WarbandRecruiting
    {
        [DebugAction("WAW", "Spawn Warband Recruiting", actionType = DebugActionType.Action)]
        public static void SpawnRecruitingWarband()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(SpawnRecruitingWarband), true);

        }

        static bool SpawnRecruitingWarband(GlobalTargetInfo info)
        {
            WarbandRecruitingUtil.SpawnRecruitingWarband(info.Tile);
            return true;
        }
    }
}
