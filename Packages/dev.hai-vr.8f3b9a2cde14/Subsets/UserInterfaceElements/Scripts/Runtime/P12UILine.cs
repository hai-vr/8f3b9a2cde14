using TMPro;
using UnityEngine;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12UILine : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text value;

        public void SetTitle(string rawString)
        {
            title.text = rawString;
        }

        public void SetValue(string displayedValue)
        {
            value.text = displayedValue;
        }
    }
}
