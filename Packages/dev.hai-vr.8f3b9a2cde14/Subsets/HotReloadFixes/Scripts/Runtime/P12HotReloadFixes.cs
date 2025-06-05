using Basis.Scripts.Boot_Sequence;
using UnityEditor;
using UnityEngine;
using Valve.VR;

namespace Subsets.HotReloadFixes.Scripts.Runtime
{
    public class P12HotReloadFixes : MonoBehaviour
    {
        private void OnDestroy()
        {
            FixSteamVRRemembersConnectedDevices();
        }

        private static void FixSteamVRRemembersConnectedDevices()
        {
            // Fix an issue where entering VR twice in two separate Play Mode sessions
            // would not trigger device connection events when domain reload is OFF.
            SteamVR.connected = new bool[Valve.VR.OpenVR.k_unMaxTrackedDeviceCount];
        }
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class P12HotReloadFixesEditor
    {
        private const float ArtificialDelayPreventDoubleClickingPlayModeButton = 1.25f;
        private static float _enterPlayModeRealtime;

        static P12HotReloadFixesEditor()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingPlayMode)
            {
                // Prevent exiting Play Mode when we're still booting, to prevent a weird issue with Addressables async stuff failing,
                // which happens when we're entering and exiting Play Mode too quickly.
                //     "I'm running into a weird issue where `await AddressableLoadProcess.LoadAssetAsync<T>(LoadRequest)`
                //     or `await Addressables.LoadResourceLocationsAsync(LoadRequest.Key, typeof(T)).Task` never completes
                //     for the rest of the editor session until the next domain reload if we enter and exit Play mode too fast"
                // I think it has something to do with Addressables async tasks starting in Play Mode and ending in Edit Mode, but can't be sure.
                if (Time.realtimeSinceStartup - _enterPlayModeRealtime < ArtificialDelayPreventDoubleClickingPlayModeButton)
                {
                    EditorApplication.isPlaying = true;
                }
            }
            else if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                _enterPlayModeRealtime = Time.realtimeSinceStartup;
            }
        }
    }
#endif
}
