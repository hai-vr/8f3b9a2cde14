using UnityEngine;

namespace Hai.Project12.HaiSystems.Supporting
{
    public class H12Geofunctions
    {
        /**
         * Creates a rotation which rotates from fromDirection to toDirection, and the direction of fromUpwards is oriented to the direction of toUpwards.
         * The upwards direction will be a best effort to match as they don't need to be perpendicular to the direction.<br/>
         * <br/>
         * Usually, you can use it to reorient an object to match another object.<br/>
         * <br/>
         * This function is an attempt to combine both LookRotation and FromToDirection:
         * - FromToDirection does not enforce an upwards axis, only one axis needs to match.
         * - LookRotation can difficult to grasp when trying to rotate an object to face a direction, but the object's forward isn't the direction that needs to face it.
         */
        public static Quaternion FromToOrientation(Vector3 fromDirection, Vector3 toDirection, Vector3 fromUpwards, Vector3 toUpwards)
        {
            var fromRotation = Quaternion.LookRotation(fromDirection, fromUpwards);
            var toRotation = Quaternion.LookRotation(toDirection, toUpwards);
            return toRotation * Quaternion.Inverse(fromRotation);
        }
    }
}
