using System.Collections.Generic;
using System.Linq;
using BattlePhaze.SettingsManager;
using UnityEngine;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12MainMenu : MonoBehaviour
    {
        [SerializeField] private P12UIEScriptedPrefabs prefabs;
        [SerializeField] private Transform layoutGroupHolder;
        [SerializeField] private Transform titleGroupHolder;

        private List<string> _audioOptions;
        private List<string> _controlsOptions;
        private List<string> _videoOptions;

        private P12UILine _lineMainMenu;

        private H12Builder _h12builder;

        private void Start()
        {
            Debug.Log(string.Join(",", SettingsManager.Instance.Options
                .Where(input => input.Type == SettingsManagerEnums.IsType.Dynamic)
                .Select(input => input.Name)
            ));

            _h12builder = new H12Builder(prefabs, layoutGroupHolder, titleGroupHolder, 1.75f);

            // _lineMainMenu = _h12builder.P12TitleButton("Main Menu", () => { });
            // _lineMainMenu.SetFocused(true);

            _h12builder.P12CenteredButton("New Game", () => { });
            _h12builder.P12CenteredButton("Load Game", () => { });
            _h12builder.P12CenteredButton("Go to Sandbox", () => { });
            _h12builder.P12CenteredButton("Settings", () => { });
        }
    }
}
