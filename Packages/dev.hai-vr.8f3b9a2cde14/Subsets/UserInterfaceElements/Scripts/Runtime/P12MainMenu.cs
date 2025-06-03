using System;
using System.Collections.Generic;
using Basis.Scripts.UI.UI_Panels;
using Subsets.GameRoutine.Scripts.Runtime;
using UnityEngine;
using static Hai.Project12.UserInterfaceElements.H12Localization;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12MainMenu : MonoBehaviour
    {
        public const float StandardControlExpansion = 1.75f;

        [SerializeField] private GameObject temp___sceneElements; // TEMP: We'll need a way to clean the scene, so that the directional light in it doesn't interfere.
        [SerializeField] private Transform temp___worldSpaceDefaultPos; // TEMP: The UI system will need a manager on its own.
        [SerializeField] private Transform temp___worldSpaceUICenter; // TEMP: The UI system will need a manager on its own.

        [SerializeField] private P12GameLevelManagement gameLevelManagement;
        [SerializeField] private P12HijackBasisPointRaycaster hijackRaycaster;
        [SerializeField] private RectTransform rootTransform;

        [SerializeField] private RectTransform mainPos;
        [SerializeField] private RectTransform sidePos;

        [SerializeField] private BasisUIMovementDriver uiMovementDriver;

        [SerializeField] private P12UIEScriptedPrefabs prefabs;
        [SerializeField] private P12UIHaptics haptics;
        [SerializeField] private Transform layoutGroupHolder;
        [SerializeField] private Transform titleGroupHolder;
        [SerializeField] private GameObject settings;

        private List<string> _audioOptions;
        private List<string> _controlsOptions;
        private List<string> _videoOptions;

        private P12UILine _lineMainMenu;

        private H12Builder _h12builder;

        private bool _weMadeCursorVisible;

        private void Awake()
        {
            _h12builder = new H12Builder(prefabs, layoutGroupHolder, titleGroupHolder, StandardControlExpansion, haptics);

            MakeMenu();
        }

        private void OnEnable()
        {
            H12Localization.OnLocalizationChanged -= OnLocalizationChanged;
            H12Localization.OnLocalizationChanged += OnLocalizationChanged;
        }

        private void OnDisable()
        {
            H12Localization.OnLocalizationChanged -= OnLocalizationChanged;
        }

        private void OnLocalizationChanged()
        {
            // TODO: Track state of the menu, so that we remake the correct part of it.
            MakeMenu();
        }

        private void MakeMenu()
        {
            Clear();
            _h12builder.P12CenteredButton(_L("ui.main_menu.new_game"), NewGame);
            _h12builder.P12CenteredButton(_L("ui.main_menu.load_game"), UnloadGame);
            _h12builder.P12CenteredButton(_L("ui.main_menu.coop"), () => { });
            _h12builder.P12CenteredButton(_L("ui.main_menu.settings"), () =>
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
            _h12builder.P12CenteredButton(_L("ui.main_menu.sandbox"), () =>
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
            settings.SetActive(false);
            gameLevelManagement.Load(P12GameLevelManagement.SampleEmbeddedLevel1, (UniqueID, progress, eventDescription) =>
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
            settings.SetActive(false);
            gameLevelManagement.UnloadAndReturnToDefaultScene();
        }

        private void MakeCursorHidden()
        {
            if (_weMadeCursorVisible)
            {
                BasisCursorManagement.LockCursor(nameof(P12MainMenu));
                hijackRaycaster.ReturnMask();
                _weMadeCursorVisible = false;
            }
        }

        private void MakeCursorVisible()
        {
            if (!_weMadeCursorVisible)
            {
                BasisCursorManagement.UnlockCursor(nameof(P12MainMenu));
                hijackRaycaster.HijackMask();
                _weMadeCursorVisible = true;
            }
        }
    }
}
