using System;
using System.Collections.Generic;
using Hai.Project12.HaiSystems.Supporting;
using UnityEngine;
using UnityEngine.Rendering;

#if !HVR_IS_BASIS
using UdonSharp;
using VRC.SDKBase;
#else
#endif

namespace Hai.Project12.DataViz.Runtime
{
#if !HVR_IS_BASIS || HVR_BASIS_USES_SHIMS
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DebugInfoDisplayManager : UdonSharpBehaviour
#else
    public class P12DebugInfoDisplayManager : MonoBehaviour
#endif
    {
        private readonly HashSet<P12DebugInfoDisplay> _displays = new HashSet<P12DebugInfoDisplay>();

        public bool NeedsUpdateLightVolumes { get; private set; }

        private void Awake()
        {
            var isUrp = GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("UniversalRenderPipeline");
            NeedsUpdateLightVolumes = !isUrp;
        }

        public void _WhenDisplayEnabled(P12DebugInfoDisplay debugInfoDisplay)
        {
            _displays.Add(debugInfoDisplay);
        }

        public void _WhenDisplayDisabled(P12DebugInfoDisplay debugInfoDisplay)
        {
            _displays.Remove(debugInfoDisplay);
        }

        private void LateUpdate()
        {
#if !HVR_IS_BASIS
            var hmd = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
#else
            var hmd = H12Cross._BasisOnly_StubGetEquivalentFloatingHMD();
#endif
            var hmdDirectionNoRoll = hmd.rotation * Vector3.forward;
            hmdDirectionNoRoll.y = 0;
            foreach (var display in _displays)
            {
                display._UpdateTransform(hmd.position, hmdDirectionNoRoll);
            }
        }
    }
}
