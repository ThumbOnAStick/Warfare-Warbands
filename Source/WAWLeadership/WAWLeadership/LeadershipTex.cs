using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WAWLeadership
{
    [StaticConstructorOnStartup]
    internal class LeadershipTex
    {
        public static readonly Texture2D Interact = ContentFinder<Texture2D>.Get("Things/Mote/SpeechSymbols/Chitchat");


    }
}
