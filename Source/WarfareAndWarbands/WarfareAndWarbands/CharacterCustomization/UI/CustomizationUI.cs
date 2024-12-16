using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.CharacterCustomization.UI
{
    internal static class CustomizationUI
    {
        static Window_Customization windowCache;
        static Dictionary<ThingDef, ThingStyleDef> dicCache;

        public static void GetStyleOptions(ThingDef item,
            ref Dictionary<ThingDef, ThingStyleDef> dic,
            IEnumerable<ThingStyleDef> styles, Window_Customization window)
        {
            windowCache = window;
            dicCache = dic;
            var opts = StyleOptions(item, styles);
            if (opts.Count() > 0)
                Find.WindowStack.Add(new FloatMenu(opts.ToList()));
        }

        public static IEnumerable<FloatMenuOption> StyleOptions(
            ThingDef item,
            IEnumerable<ThingStyleDef> styles)
        {
            yield return new FloatMenuOption("WAW.DefaultStyle".Translate(), delegate {
                ChangeStyle(ref dicCache, item, null); });
                foreach (var style in styles)
            {
                yield return new FloatMenuOption(style.Category.label, delegate{ ChangeStyle(ref dicCache, item, style); });
            }
        }
        
        static void ChangeStyle(ref Dictionary<ThingDef, ThingStyleDef> dic, ThingDef def ,ThingStyleDef style)
        {
            if(dic.ContainsKey(def))
            {
                dic[def] = style;
            }
            else
            {
                dic.Add(def, style);    
            }
            windowCache.TryToChangeEquipmentStyle(def);
        }
    }
}
