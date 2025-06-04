#if HVR_IS_BASIS
using UnityEngine;
using Basis.Scripts.Drivers;

namespace Hai.Project12.DataViz.Runtime
{
    public class H12Cross
    {
        public static bool IsValid(object thing)
        {
            return thing != null;
        }
        public static CrossTrackerData _BasisOnly_StubGetEquivalentFloatingHMD()
        {
            var cam = BasisLocalCameraDriver.Instance?.Camera?.transform;
            if (cam == null) return new CrossTrackerData { position = Vector3.zero, rotation = Quaternion.identity };

            return new CrossTrackerData
            {
                position = cam.position,
                rotation = cam.rotation
            };
        }
    }

    public class CrossTrackerData
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}
#endif
