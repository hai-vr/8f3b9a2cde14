using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using Hai.Project12.HaiSystems.Supporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hai.Project12.ListenServer.Runtime
{
    public class P12HookHeadShadowDriver : MonoBehaviour
    {
        [EarlyInjectable] [SerializeField] private P12HeadShadowDriver headShadowDriver;

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= EndCameraRendering;
            Application.onBeforeRender -= OnBeforeRender;

            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            RenderPipelineManager.endCameraRendering += EndCameraRendering;
            Application.onBeforeRender += OnBeforeRender;

            // This is null if we load too early. If so, we need to treat OnLocalPlayerCreatedAndReady as being an avatar change.
            if (BasisLocalPlayer.Instance)
            {
                BasisLocalPlayer.Instance.OnLocalAvatarChanged -= OnLocalAvatarChanged;
                BasisLocalPlayer.Instance.OnLocalAvatarChanged += OnLocalAvatarChanged;
            }
            else
            {
                BasisLocalPlayer.OnLocalPlayerCreatedAndReady -= OnLocalPlayerCreatedAndReady;
                BasisLocalPlayer.OnLocalPlayerCreatedAndReady += OnLocalPlayerCreatedAndReady;
            }
        }

        private void OnLocalPlayerCreatedAndReady()
        {
            BasisLocalPlayer.OnLocalPlayerCreatedAndReady -= OnLocalPlayerCreatedAndReady;

            BasisLocalPlayer.Instance.OnLocalAvatarChanged -= OnLocalAvatarChanged;
            BasisLocalPlayer.Instance.OnLocalAvatarChanged += OnLocalAvatarChanged;
            OnLocalAvatarChanged();
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= EndCameraRendering;
            Application.onBeforeRender -= OnBeforeRender;
            if (BasisLocalPlayer.Instance)
            {
                BasisLocalPlayer.Instance.OnLocalAvatarChanged -= OnLocalAvatarChanged;
            }
            BasisLocalPlayer.OnLocalPlayerCreatedAndReady -= OnLocalPlayerCreatedAndReady;
        }

        private void BeginCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            if (IsFirstPersonCamera(cam)) headShadowDriver.BeforeRenderFirstPerson();
            else headShadowDriver.BeforeRenderThirdPerson();
        }

        private void EndCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            if (IsFirstPersonCamera(cam)) headShadowDriver.AfterRenderFirstPerson();
            else headShadowDriver.AfterRenderThirdPerson();
        }

        private void OnLocalAvatarChanged()
        {
            headShadowDriver.Initialize(BasisLocalPlayer.Instance.BasisAvatar);
        }

        private void OnBeforeRender()
        {
            headShadowDriver.PrepareThisFrame();
        }

        private static bool IsFirstPersonCamera(Camera cam)
        {
            return cam.GetInstanceID() == BasisLocalCameraDriver.CameraInstanceID;
        }
    }
}
