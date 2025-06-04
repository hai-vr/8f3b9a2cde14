using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hai.Project12.UserInterfaceElements.Runtime
{
    public class P12UIDropdown : MonoBehaviour
    {
        /// If the dropdown is ranked in such a way that "Lower indices are more expensive", set this to true.
        /// Also works after the component already started.
        public bool usesReverseRank;

        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private Button lower;
        [SerializeField] private Button higher;

        private void Awake()
        {
            lower.onClick.AddListener(LowerClicked);
            higher.onClick.AddListener(HigherClicked);
        }

        private void LowerClicked()
        {
            if (usesReverseRank) IncreaseRank();
            else DecreaseRank();
        }

        private void HigherClicked()
        {
            if (usesReverseRank) DecreaseRank();
            else IncreaseRank();
        }

        private void DecreaseRank()
        {
            if (dropdown.value > 0)
            {
                dropdown.value -= 1;
            }
        }

        private void IncreaseRank()
        {
            if (dropdown.value < dropdown.options.Count - 1)
            {
                dropdown.value += 1;
            }
        }
    }
}
