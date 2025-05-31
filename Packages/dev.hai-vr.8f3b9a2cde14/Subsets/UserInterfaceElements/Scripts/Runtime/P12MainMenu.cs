using System.Collections.Generic;
using System.Linq;
using Basis.Scripts.UI.UI_Panels;
using BattlePhaze.SettingsManager;
using UnityEngine;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12MainMenu : MonoBehaviour
    {
        [SerializeField] private RectTransform rootTransform;

        [SerializeField] private RectTransform mainPos;
        [SerializeField] private RectTransform sidePos;

        [SerializeField] private BasisUIMovementDriver uiMovementDriver;

        [SerializeField] private P12UIEScriptedPrefabs prefabs;
        [SerializeField] private Transform layoutGroupHolder;
        [SerializeField] private Transform titleGroupHolder;
        [SerializeField] private GameObject settings;

        private List<string> _audioOptions;
        private List<string> _controlsOptions;
        private List<string> _videoOptions;

        private P12UILine _lineMainMenu;

        private H12Builder _h12builder;
        private bool _weMadeCursorVisible;

        private void Start()
        {
            Debug.Log(string.Join(",", SettingsManager.Instance.Options
                .Where(input => input.Type == SettingsManagerEnums.IsType.Dynamic)
                .Select(input => input.Name)
            ));

            _h12builder = new H12Builder(prefabs, layoutGroupHolder, titleGroupHolder, 1.75f);

            _h12builder.P12CenteredButton("New Game", () => { });
            _h12builder.P12CenteredButton("Load Game", () => { });
            _h12builder.P12CenteredButton("Co-op", () => { });
            _h12builder.P12CenteredButton("Settings", () =>
            {
                var newActive = !settings.activeSelf;
                settings.SetActive(newActive);

                if (newActive)
                {
                    rootTransform.localPosition = sidePos.localPosition;
                    uiMovementDriver.SetUILocation();
                    if (!_weMadeCursorVisible)
                    {
                        BasisCursorManagement.UnlockCursor(nameof(P12MainMenu));
                        _weMadeCursorVisible = true;
                    }
                }
                else
                {
                    rootTransform.localPosition = mainPos.localPosition;
                }
            });
            _h12builder.P12CenteredButton("Back to Sandbox", () =>
            {
                settings.SetActive(false);
                if (_weMadeCursorVisible)
                {
                    BasisCursorManagement.LockCursor(nameof(P12MainMenu));
                    _weMadeCursorVisible = false;
                }
            });
        }
    }
}
