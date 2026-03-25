using System.Collections.Generic;
using XRL;

namespace UD_Modding_Toolbox
{
    [HasModSensitiveStaticCache]
    [HasOptionFlagUpdate(Prefix = "Option_UD_ModdingToolbox_")]
    public static class Options
    {
        public static bool doDebug = true;
        public static Dictionary<string, bool> classDoDebug = new()
        {
            // General
            { nameof(Utils), true },
            { nameof(Extensions), true },

            { "LiquidVolume_Patches", true },
        };

        public static bool getDoDebug(object what = null, List<object> DoList = null, List<object> DontList = null, bool? DoDebug = null)
        {
            DoList ??= new();
            DontList ??= new();

            if (what != null && !DoList.IsNullOrEmpty() && DoList.Contains(what))
            {
                return true;
            }

            if (what != null && !DontList.IsNullOrEmpty() && DontList.Contains(what))
            {
                return false;
            }

            return DoDebug ?? doDebug;
        }

        public static bool getClassDoDebug(string Class) => classDoDebug.ContainsKey(Class) ? classDoDebug[Class] : doDebug;

        // Debug Settings
        [OptionFlag] public static int DebugVerbosity;
        [OptionFlag] public static bool DebugIncludeInMessage;
        [OptionFlag] public static bool DebugGeneralDebugDescriptions;

        // Checkbox settings
        // [OptionFlag] public static bool ExampleOption;

    } 
}
