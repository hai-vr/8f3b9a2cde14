using System.Collections;
using System.Collections.Generic;
using Basis.Scripts.Device_Management.Devices.Desktop;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hai.Project12.UserInterfaceElements.Runtime
{
    public class P12HookBasisLocalInputActions : MonoBehaviour
    {
        private void OnEnable()
        {
            var instance = BasisLocalInputActions.Instance; // May be null if called too early.
            if (instance)
            {
                Register(instance);
            }
            else
            {
                StartCoroutine(nameof(RegisterLater));
            }
            // BasisLocalInputActions blia = null;
            // blia.Escape.action.performed += BasisLocalInputActions.OnEscapePerformed;
        }

        private IEnumerator RegisterLater()
        {
            while (!BasisLocalInputActions.Instance)
            {
                yield return new WaitForSeconds(0);
            }
            Register(BasisLocalInputActions.Instance);
        }

        private void Register(BasisLocalInputActions instance)
        {
            instance.RightMousePressed.action.performed -= OnRightMouse;
            instance.RightMousePressed.action.performed += OnRightMouse;
            instance.RightMousePressed.action.canceled -= OnRightMouse;
            instance.RightMousePressed.action.canceled += OnRightMouse;
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
