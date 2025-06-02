using Basis.Scripts.UI;
using UnityEngine;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12HijackBasisPointRaycaster : MonoBehaviour
    {
        private int _maskForOnlyUiLayer;

        private BasisPointRaycaster _basisPointRaycasterLateInit;
        private LayerMask _defaultMask;

        private void Start()
        {
            _maskForOnlyUiLayer = 1 << LayerMask.NameToLayer("UI");
        }

        private void Update()
        {
            if (_basisPointRaycasterLateInit == null)
            {
                _basisPointRaycasterLateInit = FindAnyObjectByType<BasisPointRaycaster>(); // May return nullk
                if (_basisPointRaycasterLateInit != null)
                {
                    _defaultMask = _basisPointRaycasterLateInit.Mask;
                }
            }
        }

        public void HijackMask()
        {
            if (_basisPointRaycasterLateInit == null) return;

            _basisPointRaycasterLateInit.Mask = _maskForOnlyUiLayer;
            BasisDebug.Log("P12 BasisPointRaycaster now uses the UI mask to prevent world geometry from interfering.", BasisDebug.LogTag.Input);
        }

        public void ReturnMask()
        {
            if (_basisPointRaycasterLateInit == null) return;

            _basisPointRaycasterLateInit.Mask = _defaultMask;
            BasisDebug.Log("P12 BasisPointRaycaster now uses the default mask.", BasisDebug.LogTag.Input);
        }
    }
}
