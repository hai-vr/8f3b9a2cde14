using System.Collections.Generic;

namespace Hai.Project12.UserInterfaceElements
{
    public class H12BattlePhazeNameToLocalizationKey
    {
        // We want to transform the original BattlePhaze english names to localization keys,
        // so that we may transform those localized keys into our own strings, which may differ from BattlePhaze's english names.
        private static readonly Dictionary<string, string> BattlePhazeNameToLocalizationKey = new Dictionary<string, string>()
        {
            { "Ambient Occlusion", "ui.settings.option.ambient_occlusion" },
            { "Antialiasing", "ui.settings.option.antialiasing" },
            { "Close Point Shadows", "ui.settings.option.close_point_shadows" },
            { "Contact Shadows", "ui.settings.option.contact_shadows" },
            { "Controller DeadZone", "ui.settings.option.controller_dead_zone" },
            { "Debug Visuals", "ui.settings.option.debug_visuals" },
            { "Depth Support", "ui.settings.option.depth_support" },
            { "Field Of View", "ui.settings.option.field_of_view" },
            { "Foveated Rendering Level", "ui.settings.option.foveated_rendering_level" },
            { "HDR Support", "ui.settings.option.hdr_support" },
            { "Hearing Range", "ui.settings.option.hearing_range" },
            { "Main Audio", "ui.settings.option.main_audio" },
            { "Master Quality", "ui.settings.option.master_quality" },
            { "Maximum Avatar Distance", "ui.settings.option.maximum_avatar_distance" },
            { "Mcrophone Volume", "ui.settings.option.microphone_volume" },
            { "Memory Allocation", "ui.settings.option.memory_allocation" },
            { "Menus Volume", "ui.settings.option.menus_volume" },
            { "Microphone Denoiser", "ui.settings.option.microphone_denoiser" },
            { "Microphone Range", "ui.settings.option.microphone_range" },
            { "Microphone Volume", "ui.settings.option.microphone_volume" },
            { "Monitor", "ui.settings.option.monitor" },
            { "Opaque Support", "ui.settings.option.opaque_support" },
            { "Player Volume", "ui.settings.option.player_volume" },
            { "Quality Level", "ui.settings.option.quality_level" },
            { "Render Resolution", "ui.settings.option.render_resolution" },
            { "Resolution", "ui.settings.option.resolution" },
            { "ScreenMode", "ui.settings.option.screen_mode" },
            { "Shadow Quality", "ui.settings.option.shadow_quality" },
            { "Snap Turn Angle", "ui.settings.option.snap_turn_angle" },
            { "Terrain Quality", "ui.settings.option.terrain_quality" },
            { "Texture Quality", "ui.settings.option.texture_quality" },
            { "UpScaler", "ui.settings.option.up_scaler" },
            { "Vertical Sync", "ui.settings.option.vertical_sync" },
            { "Volumetric Quality", "ui.settings.option.volumetric_quality" },
            { "World Volume", "ui.settings.option.world_volume" },
        };

        public static string GetKeyOrNull(string battlePhazeName)
        {
            var found = BattlePhazeNameToLocalizationKey.TryGetValue(battlePhazeName, out var value);
            if (found) return value;
            return null;
        }
    }
}
