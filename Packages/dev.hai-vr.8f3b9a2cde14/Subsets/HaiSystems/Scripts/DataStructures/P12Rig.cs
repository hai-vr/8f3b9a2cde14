using System.Collections.Generic;
using UnityEngine;

namespace Hai.Project12.HaiSystems.DataStructures
{
    public class P12Rig : MonoBehaviour
    {
        private readonly Dictionary<HumanBodyBones, Transform> _boneToTransform = new();

        // TODO: Expose RigidBody directly

        /// Returns null if the bone doesn't exist.
        public Transform GetBoneTransform(HumanBodyBones bone)
        {
            return _boneToTransform.GetValueOrDefault(bone);
        }

        /// Setting a bone to null is allowed.
        public void SetBoneTransform(HumanBodyBones bone, Transform value)
        {
            if (value == null) _boneToTransform.Remove(bone);
            else _boneToTransform[bone] = value;
        }
    }
}
