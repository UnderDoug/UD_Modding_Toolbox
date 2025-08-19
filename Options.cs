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

        public static bool getClassDoDebug(string Class)
        {
            if (classDoDebug.ContainsKey(Class))
            {
                return classDoDebug[Class];
            }
            return doDebug;
        }

        // Debug Settings
        [OptionFlag] public static int DebugVerbosity;
        [OptionFlag] public static bool DebugIncludeInMessage;
        [OptionFlag] public static bool DebugGeneralDebugDescriptions;

        // Checkbox settings
        // [OptionFlag] public static bool ExampleOption;

    } //!-- public static class Options
}
