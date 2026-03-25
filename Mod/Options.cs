using System.Collections.Generic;
using XRL;

namespace UD_Modding_Toolbox
{
    [HasModSensitiveStaticCache]
    [HasOptionFlagUpdate(Prefix = Const.MOD_PREFIX)]
    public static class Options
    {
        // Debug Settings
        [OptionFlag] public static bool DebugEnableLogging;

        public static bool? EnableLogging
            => XRL.UI.Options.HasOption(Const.MOD_PREFIX + nameof(DebugEnableLogging))
            ? DebugEnableLogging
            : null
            ;

        [OptionFlag] public static bool DebugGeneralDebugDescriptions;

        // Checkbox settings
        // [OptionFlag] public static bool ExampleOption;

    } 
}
