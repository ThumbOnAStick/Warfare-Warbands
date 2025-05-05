using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands
{
    public class WarfareAndWarbandsMod : Mod
    {
        public WarfareAndWarbandsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<WAWSettings>();

        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoWindowsContent(inRect);
        }
     

        public override string SettingsCategory()
        {
            return "WAW.Settings".Translate();
        }

        public static WAWSettings settings;

    }
}
