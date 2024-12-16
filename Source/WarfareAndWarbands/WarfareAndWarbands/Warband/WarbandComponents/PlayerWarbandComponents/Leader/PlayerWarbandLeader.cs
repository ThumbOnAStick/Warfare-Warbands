using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents
{
    public class PlayerWarbandLeader : IExposable
    {
        public Pawn Leader
        {
            get
            {
                return leader;
            }
            set
            {
                leader = value;
            }
        }

        public UnityEvent onLeaderChanged;
        Pawn leader;


        public PlayerWarbandLeader()
        {
            onLeaderChanged = new UnityEvent();
        }

        public void OnLeaderChanged()
        {
            this.onLeaderChanged.Invoke();
        }

        public void AssignLeader(Pawn p, Caravan caravan = null)
        {
            if (leader != null && !leader.Dead)
            {
                return;
            }
            if (p == null)
            {
                return;
            }
            this.leader = p;
            caravan?.RemovePawn(p);
            SendLeaderSetMessage(p);
            ResolveCaravan(caravan);
            OnLeaderChanged();
        }

        void ResolveCaravan(Caravan caravan)
        {
            if (caravan == null)
            {
                return;
            }
            for (int i = 0; i < caravan.pawns.Count; i++)
            {
                if (caravan.pawns[i].IsColonist)
                {
                    return;
                }
            }
            caravan.Destroy();
        }

        void SendLeaderSetMessage(Pawn pawn)
        {
            string label = "WAW.LeaderSet".Translate();
            string desc = "WAW.LeaderSet.Desc".Translate(pawn.NameFullColored, pawn.gender.GetObjective(), pawn.gender.GetPronoun());
            Letter letter = LetterMaker.MakeLetter(label, desc, LetterDefOf.NeutralEvent);
            Find.LetterStack.ReceiveLetter(letter);
        }


        public void ReturnLeaderHome()
        {
            if (leader == null)
            {
                return;
            }
            Map playerMap = Find.AnyPlayerHomeMap;
            if (playerMap == null)
            {
                return;
            }
            Current.Game.CurrentMap = playerMap;
            var cell = CellFinder.StandableCellNear(playerMap.Center, playerMap, 50);
            if (cell == IntVec3.Invalid)
            {
                cell = DropCellFinder.RandomDropSpot(playerMap);
            }
            leader.TryGetComp<CompMercenary>()?.ResetAll();
            leader.SetFaction(Faction.OfPlayer);
            CameraJumper.TryJump(cell, playerMap);
            ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
            List<Thing> leaders = new List<Thing>() { this.leader };
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(leaders, true, false);
            activeDropPodInfo.spawnWipeMode = new WipeMode?(WipeMode.Vanish);
            DropPodUtility.MakeDropPodAt(cell, playerMap, activeDropPodInfo);
        }

        public bool IsLeaderAvailable()
        {
            return leader != null && !leader.Dead;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref leader, "leader");
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            outChildren = new List<IThingHolder>() { leader };
        }


    }
}
