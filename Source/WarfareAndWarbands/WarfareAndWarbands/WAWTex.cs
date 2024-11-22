using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands
{
    [StaticConstructorOnStartup]

    internal static class WAWTex
    {
        public static readonly Texture2D ModIcon = ContentFinder<Texture2D>.Get("UI/Icon/ModIcon", true);
        public static readonly Texture2D CommandIcon = ContentFinder<Texture2D>.Get("UI/Commands/Resettle", true);


    }
}
