using System;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace CloudTek.Build
{
    public static class ToolSettingsExtensions
    {
        public static TSettings When<TSettings>(this TSettings settings, bool predicate,
            Func<TSettings, TSettings> action)
            where TSettings : ToolSettings
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            if (predicate)
            {
                settings = action(settings);
            }

            return settings;
        }

        public static TSettings Execute<TSettings>(this TSettings settings, 
            Func<TSettings, TSettings> action)
            where TSettings : ToolSettings
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            settings = action(settings);

            return settings;
        }
    }
}