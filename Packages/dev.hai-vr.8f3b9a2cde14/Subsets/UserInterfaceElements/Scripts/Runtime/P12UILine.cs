using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12UILine : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text value;
        [SerializeField] private LayoutElement control; // Nullable

        private float _originalSize = -1;

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
            if (_originalSize == -1) _originalSize = title.fontSize;

            title.fontSize = focused ? _originalSize : _originalSize * 0.6f;
        }

        public void SetControlExpansion(float controlExpansion)
        {
            if (control == null) return;

            control.minWidth = control.minWidth * controlExpansion;
        }
    }
}
