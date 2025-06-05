using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hai.Project12.UserInterfaceElements.Runtime
{
    public class P12UILine : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text value;
        [SerializeField] private LayoutElement control; // Nullable

        private float _originalFontSize = -1;
        private float _initialMinWidth = -1;

        public void SetTitle(string rawString)
        {
            title.text = rawString;
        }

        public void SetValue(string displayedValue)
        {
            value.text = displayedValue;
        }

        public void SetFocused(bool focused)
        {
            if (_originalFontSize == -1) _originalFontSize = title.fontSize;

            title.fontSize = focused ? _originalFontSize : _originalFontSize * 0.6f;
        }

        public void SetControlExpansion(float controlExpansion)
        {
            if (control == null) return;
            if (_initialMinWidth == -1) _initialMinWidth = control.minWidth;

            control.minWidth = _initialMinWidth * controlExpansion;
        }
    }
}
