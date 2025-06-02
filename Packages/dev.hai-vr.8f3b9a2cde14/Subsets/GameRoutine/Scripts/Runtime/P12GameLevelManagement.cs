using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Subsets.GameRoutine.Scripts.Runtime
{
    public class P12GameLevelManagement : MonoBehaviour
    {
        public const string SampleEmbeddedLevel1 = "Assets/_Hai_BasisCyanKey/BasisCyanKey.unity";
        public const string SampleEmbeddedLevel2 = "Assets/_Hai_BasisCyanKey/BasisCyanKey_Level2.unity";

        [SerializeField] private GameObject defaultSceneElements;

        private H12BasisGameRoutineBlindSpots _h12BasisGameRoutineBlindSpots;
        private bool _isActivelyLoadingLevel;

        private bool _hasLevel;
        private Scene _level;

        private void Start()
        {
            _h12BasisGameRoutineBlindSpots = new H12BasisGameRoutineBlindSpots();
        }

        /// Unloads the previous game level, loads the new one, hides default scene objects, and re-tetrahedralize probes.
        public async Task Load(string level, BasisProgressReport.ProgressReportState progressReportStateFn)
        {
            if (_hasLevel)
            {
                await _h12BasisGameRoutineBlindSpots.UnloadGameLevel(_level);
                _hasLevel = false;
            }

            // TODO: Prevent multi loads
            _isActivelyLoadingLevel = true;

            var basisProgressReport = new BasisProgressReport();
            basisProgressReport.OnProgressReport += (string UniqueID, float progress, string eventDescription) =>
            {
                progressReportStateFn.Invoke(UniqueID, progress, eventDescription);
            };

            var loadedLevel = await _h12BasisGameRoutineBlindSpots.LoadEmbeddedGameLevel(level, true, basisProgressReport, LoadSceneMode.Additive);

            defaultSceneElements.SetActive(false);

            await TetrahedralizeProbes();

            _hasLevel = true;

            _isActivelyLoadingLevel = false;
            _level = loadedLevel;
        }

        /// Unloads the previous game level, restores default scene objects, and re-tetrahedralize probes.
        public async Task UnloadAndReturnToDefaultScene()
        {
            if (!_hasLevel) return;

            await _h12BasisGameRoutineBlindSpots.UnloadGameLevel(_level);

            defaultSceneElements.SetActive(true);

            await TetrahedralizeProbes();

            _hasLevel = false;
        }

        private static async Task TetrahedralizeProbes()
        {
            // TODO: Is doing all that await stuff even necessary?
            var tetrahedralizationTask = new TaskCompletionSource<bool>();
            Action tetraFn = () => tetrahedralizationTask.SetResult(true);
            LightProbes.tetrahedralizationCompleted += tetraFn;
            LightProbes.TetrahedralizeAsync();
            await tetrahedralizationTask.Task;
            LightProbes.tetrahedralizationCompleted -= tetraFn;
        }
    }
}
