using Basis.Scripts.Device_Management.Devices.Desktop;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12HijackBasisLocalInputActions : MonoBehaviour
    {
        private void OnEnable()
        {
            var instance = BasisLocalInputActions.Instance;
            instance.RightMousePressed.action.performed -= OnRightMouse;
            instance.RightMousePressed.action.performed += OnRightMouse;
            instance.RightMousePressed.action.canceled -= OnRightMouse;
            instance.RightMousePressed.action.canceled += OnRightMouse;
            // BasisLocalInputActions blia = null;
            // blia.Escape.action.performed += BasisLocalInputActions.OnEscapePerformed;
        }

        private void OnDisable()
        {
            var instance = BasisLocalInputActions.Instance;
            instance.RightMousePressed.action.performed -= OnRightMouse;
            instance.RightMousePressed.action.canceled -= OnRightMouse;
        }

        private void OnRightMouse(InputAction.CallbackContext ctx)
        {
            BasisLocalInputActions.Instance.InputState.SecondaryTrigger = ctx.ReadValue<float>();
        }
    }
}
