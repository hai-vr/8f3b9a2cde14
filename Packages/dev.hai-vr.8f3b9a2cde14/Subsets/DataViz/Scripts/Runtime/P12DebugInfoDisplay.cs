using System;
using Hai.Project12.HaiSystems.Supporting;
using TMPro;
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
    public class DebugInfoDisplay : UdonSharpBehaviour
#else
    public class P12DebugInfoDisplay : MonoBehaviour
#endif
    {
        public bool needsToBeReadableOnMirrors = false;
        public bool needsProxyVolume = false;
        public bool needsToSupportSpecialCharactersInProxyVolume = false;
        public bool needsToShiftTowardsPlayer = false;

        [SerializeField] private TMP_Text text;
        [SerializeField] private LightProbeProxyVolume proxyVolume;
        [EarlyInjectable] [SerializeField] private P12DebugInfoDisplayManager manager;

        private bool _initialized;
        private Vector3 _position = Vector3.zero;
        private int _textChildCount;

        private void OnEnable()
        {
            manager._WhenDisplayEnabled(this);
            if (!_initialized)
            {
                if (!needsProxyVolume)
                {
                    Destroy(proxyVolume);
                    text.GetComponent<MeshRenderer>().lightProbeUsage = LightProbeUsage.BlendProbes;
                }

                _initialized = true;
            }
        }

        private void OnDisable()
        {
            manager._WhenDisplayDisabled(this);
        }

        public void _SetText(string msg)
        {
            text.text = msg;
        }

        public void _SetDesiredPosition(Vector3 position)
        {
            _position = position;
        }

        public void _SetColor(Color color)
        {
            text.color = color;
        }

        public void _UpdateTransform(Vector3 hmdPosition, Vector3 hmdDirectionNoRoll)
        {
            var actualPosition = needsToShiftTowardsPlayer ? (_position + (hmdPosition - _position).normalized * 0.2f) : _position;
            transform.position = actualPosition;

            if (needsToBeReadableOnMirrors)
            {
                transform.rotation = Quaternion.LookRotation(hmdDirectionNoRoll);
            }
            else
            {
                var billboardDirection = actualPosition - hmdPosition;
                billboardDirection.y = 0;
                transform.rotation = Quaternion.LookRotation(billboardDirection, Vector3.up);
            }

            if (needsProxyVolume && needsToSupportSpecialCharactersInProxyVolume)
            {
                TryUpdateProxyVolumes();
            }
        }

        private void TryUpdateProxyVolumes()
        {
            // This might not always be the case, but a change in the number of children within the text
            // could indicate that TMP has spawned a new sub mesh to handle another font (i.e. when
            // a character from another language is inserted into the text).
            // Checking the number of children should be quicker than checking all the MeshRenderers
            var currentChildCount = text.gameObject.transform.childCount; // TMP_Text.trasnform is not exposed to Udon (???!!!)
            if (currentChildCount == _textChildCount) return;

            _textChildCount = currentChildCount;

            _DoUpdateProxyVolumesWithinTextHiearchy(proxyVolume, text);
        }

        public static void _DoUpdateProxyVolumesWithinTextHiearchy(LightProbeProxyVolume whichProxyVolume, TMP_Text whichText)
        {
            var subComponents = whichText.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var meshRenderer in subComponents)
            {
                if (H12Cross.IsValid(meshRenderer)) // Defensive
                {
                    if (meshRenderer.lightProbeUsage != LightProbeUsage.UseProxyVolume)
                    {
                        meshRenderer.lightProbeUsage = LightProbeUsage.UseProxyVolume;
                        meshRenderer.lightProbeProxyVolumeOverride = whichProxyVolume.gameObject;
                    }
                }
            }
        }
    }
}
