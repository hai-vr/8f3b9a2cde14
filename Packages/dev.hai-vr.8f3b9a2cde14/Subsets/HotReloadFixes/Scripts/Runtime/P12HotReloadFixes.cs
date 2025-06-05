using System.Linq;
using Hai.Project12.HaiSystems.Supporting;
using UnityEditor;
using UnityEngine;
using Valve.VR;

namespace Hai.Project12.HotReloadFixes.Runtime
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
        private static int _totalAttempts;
        private const float ArtificialDelayPreventDoubleClickingPlayModeButton = 1.25f;
        private const int FailsafeAttempts = 3;
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
                _totalAttempts++;
                if (_totalAttempts < FailsafeAttempts && Time.realtimeSinceStartup - _enterPlayModeRealtime < ArtificialDelayPreventDoubleClickingPlayModeButton)
                {
                    var msg = "To prevent a bug with async tasks bleeding between Play Mode and Edit Mode, we cannot allow you" +
                            $" exiting Play Mode within {ArtificialDelayPreventDoubleClickingPlayModeButton:0.00} seconds of having just entered Play Mode." +
                            " In other words, we are preventing you from double-clicking the Play Mode button by accident. For more information, see P12HotReloadFixes.cs." +
                            $" There is a failsafe: You can try to exit Play Mode {FailsafeAttempts - _totalAttempts} more times to force exiting Play Mode.";
                    H12Debug.LogError(msg);
                    ShowEditorNotification($"CAUTION: You are still in Play Mode.\nYou can only exit Play Mode in {ArtificialDelayPreventDoubleClickingPlayModeButton - (Time.realtimeSinceStartup - _enterPlayModeRealtime):0.0} seconds\nCheck your error logs to know why.", 10f);
                    EditorApplication.isPlaying = true;
                }
            }
            else if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                _enterPlayModeRealtime = Time.realtimeSinceStartup;
                _totalAttempts = 0;
            }
        }

        private static void ShowEditorNotification(string text, float durationSeconds)
        {
            var editorWindows = Resources.FindObjectsOfTypeAll(typeof(SceneView)).Cast<EditorWindow>();
            foreach (var editorWindow in editorWindows)
            {
                editorWindow.ShowNotification(new GUIContent(text, ""), durationSeconds);
            }
        }
    }
#endif
}
