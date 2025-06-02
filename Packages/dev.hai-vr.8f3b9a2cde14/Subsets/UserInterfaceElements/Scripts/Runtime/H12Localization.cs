using System.Collections.Generic;

namespace Hai.Project12.UserInterfaceElements
{
    internal class H12Localization
    {
        private static readonly Dictionary<string, string> LocalizationKeyToP12Name = new Dictionary<string, string>()
        {
            { "ui.settings.option.ambient_occlusion", "Ambient Occlusion" },
            { "ui.settings.option.antialiasing", "Antialiasing" },
            { "ui.settings.option.close_point_shadows", "Close Point Shadows" },
            { "ui.settings.option.contact_shadows", "Contact Shadows" },
            { "ui.settings.option.controller_dead_zone", "Controller Deadzone" },
            { "ui.settings.option.debug_visuals", "Debug Visuals" },
            { "ui.settings.option.depth_support", "Depth Support" },
            { "ui.settings.option.field_of_view", "Field Of View" },
            { "ui.settings.option.foveated_rendering_level", "Foveated Rendering Level" },
            { "ui.settings.option.hdr_support", "HDR Support" },
            { "ui.settings.option.hearing_range", "Hearing Range" },
            { "ui.settings.option.main_audio", "Main Audio" },
            { "ui.settings.option.master_quality", "Master Quality" },
            { "ui.settings.option.maximum_avatar_distance", "Maximum Avatar Distance" },
            // { "ui.settings.option.microphone_volume", "Mcrophone Volume" },
            { "ui.settings.option.memory_allocation", "Memory Allocation" },
            { "ui.settings.option.menus_volume", "Menus Volume" },
            { "ui.settings.option.microphone_denoiser", "Microphone Denoiser" },
            { "ui.settings.option.microphone_range", "Microphone Range" },
            { "ui.settings.option.microphone_volume", "Microphone Volume" },
            { "ui.settings.option.monitor", "Monitor" },
            { "ui.settings.option.opaque_support", "Opaque Support" },
            { "ui.settings.option.player_volume", "Player Volume" },
            { "ui.settings.option.quality_level", "Quality Level" },
            { "ui.settings.option.render_resolution", "Render Resolution" },
            { "ui.settings.option.resolution", "Resolution" },
            { "ui.settings.option.screen_mode", "Screen Mode" },
            { "ui.settings.option.shadow_quality", "Shadow Quality" },
            { "ui.settings.option.snap_turn_angle", "Snap Turn Angle" },
            { "ui.settings.option.terrain_quality", "Terrain Quality" },
            { "ui.settings.option.texture_quality", "Texture Quality" },
            { "ui.settings.option.up_scaler", "Upscaler" },
            { "ui.settings.option.vertical_sync", "Vertical Sync" },
            { "ui.settings.option.volumetric_quality", "Volumetric Quality" },
            { "ui.settings.option.world_volume", "World Volume" },
            //
            { "ui.settings.option.microphone", "Microphone" },
            { "ui.settings.extra.trigger", "Trigger" },
            { "ui.settings.option.vr_mode", "VR Mode" },
            { "ui.settings.option.desktop_mode", "Desktop Mode" },
            { "ui.settings.option.debug", "Debug" },
            { "ui.settings.option.moderation", "Moderation" },
            { "ui.settings.action.switch_to_vr", "Switch to VR" },
            { "ui.settings.action.switch_to_desktop", "Switch to Desktop" },
            { "ui.settings.action.open_console", "Open Console" },
            { "ui.settings.action.open_admin_panel", "Open Admin Panel" },
            //
            { "ui.settings.menu.gadgets", "Gadgets" },
            { "ui.settings.menu.actions", "Actions" },
            { "ui.settings.menu.audio", "Audio" },
            { "ui.settings.menu.video", "Video" },
            { "ui.settings.menu.controls", "Controls" },
            //
            { "ui.settings.option.turn", "Turn" },
            { "ui.settings.action.snap_turn", "Snap Turn" },
            { "ui.settings.action.smooth_turn", "Smooth Turn" },
            { "ui.settings.action.no_turn", "Do Not Turn" },
        };

        public static string _L(string key)
        {
            return Localize(key, null);
        }

        public static string _L_With_Unlocalizable(string key, string otherwise)
        {
            return Localize(key, otherwise);
        }

        // This function was split into two (without using optional params) to identify the callers of the functions.
        private static string Localize(string key, string englishNameNullable)
        {
            if (LocalizationKeyToP12Name.TryGetValue(key, out var value)) return value;

            if (englishNameNullable != null) return $"{englishNameNullable} {{{key}}}";

            return $"{{{key}}}";
        }
    }
}
