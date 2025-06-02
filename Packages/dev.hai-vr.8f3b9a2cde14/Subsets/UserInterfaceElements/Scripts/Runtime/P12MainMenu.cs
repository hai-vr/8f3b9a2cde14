using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Basis.Scripts.UI.UI_Panels;
using BattlePhaze.SettingsManager;
using Subsets.GameRoutine.Scripts.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject temp___sceneElements; // TEMP: We'll need a way to clean the scene, so that the directional light in it doesn't interfere.
        [SerializeField] private Transform temp___worldSpaceDefaultPos; // TEMP: The UI system will need a manager on its own.
        [SerializeField] private Transform temp___worldSpaceUICenter; // TEMP: The UI system will need a manager on its own.

        [SerializeField] private P12GameLevelManagement gameLevelManagement;
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
            _h12builder = new H12Builder(prefabs, layoutGroupHolder, titleGroupHolder, 1.75f);

            MakeMenu();
        }

        private void MakeMenu()
        {
            Clear();
            _h12builder.P12CenteredButton("New Game", NewGame);
            _h12builder.P12CenteredButton("Load Game", UnloadGame);
            _h12builder.P12CenteredButton("Co-op", () => { });
            _h12builder.P12CenteredButton("Settings", () =>
            {
                var newActive = !settings.activeSelf;
                settings.SetActive(newActive);

                if (newActive)
                {
                    // rootTransform.localPosition = sidePos.localPosition;
                    uiMovementDriver.SetUILocation();
                    MakeCursorVisible();
                }
                else
                {
                    // rootTransform.localPosition = mainPos.localPosition;
                }
            });
            _h12builder.P12CenteredButton("Back to Sandbox", () =>
            {
                settings.SetActive(false);
                MakeCursorHidden();
            });
        }

        private void MakeLoading(float progress)
        {
            Clear();
            _h12builder.P12CenteredButton($"Loading... ({progress:0}%)", () => { });
        }

        private void Clear()
        {
            foreach (var comp in layoutGroupHolder.GetComponentsInChildren<P12UILine>())
            {
                if (comp) Destroy(comp.gameObject);
            }
        }

        private void NewGame()
        {
            gameLevelManagement.Load(P12GameLevelManagement.SampleEmbeddedLevel, (UniqueID, progress, eventDescription) =>
            {
                if (progress == 100f)
                {
                    MakeMenu();
                }
                else
                {
                    MakeLoading(progress);
                }
            });

            temp___worldSpaceUICenter.position = temp___worldSpaceDefaultPos.position;
            temp___worldSpaceUICenter.rotation = temp___worldSpaceDefaultPos.rotation;

            MakeCursorHidden();
        }

        private void UnloadGame()
        {
            gameLevelManagement.UnloadAndReturnToDefaultScene();
        }

        private void MakeCursorHidden()
        {
            if (_weMadeCursorVisible)
            {
                BasisCursorManagement.LockCursor(nameof(P12MainMenu));
                _weMadeCursorVisible = false;
            }
        }

        private void MakeCursorVisible()
        {
            if (!_weMadeCursorVisible)
            {
                BasisCursorManagement.UnlockCursor(nameof(P12MainMenu));
                _weMadeCursorVisible = true;
            }
        }
    }
}
