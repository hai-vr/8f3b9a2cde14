using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Subsets.GameRoutine.Scripts.Runtime
{
    public class H12BasisGameRoutineBlindSpots
    {
        // From BasisBundleLoadAsset.

        public async Task<Scene> LoadEmbeddedGameLevel(string scenePath, bool MakeActiveScene, BasisProgressReport progressCallback, LoadSceneMode loadSceneMode)
        {
            string UniqueID = BasisGenerateUniqueID.GenerateUniqueID();
            bool AssignedIncrement = false;
            if (!string.IsNullOrEmpty(scenePath))
            {
                // Load the scene asynchronously
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePath, loadSceneMode);
                // Track scene loading progress
                while (!asyncLoad.isDone)
                {
                    progressCallback.ReportProgress(UniqueID, 50 + asyncLoad.progress * 50, "loading scene"); // Progress from 50 to 100 during scene load
                    await Task.Yield();
                }

                BasisDebug.Log("Scene loaded successfully from AssetBundle.");
                Scene loadedScene = SceneManager.GetSceneByPath(scenePath);
                //---// bundle.MetaLink = loadedScene.path;
                // Set the loaded scene as the active scene
                if (loadedScene.IsValid())
                {
                    if (MakeActiveScene)
                    {
                        SceneManager.SetActiveScene(loadedScene);
                        //---// AssignedIncrement = bundle.Increment();
                    }

                    BasisDebug.Log("Scene set as active: " + loadedScene.name);
                    progressCallback.ReportProgress(UniqueID, 100, "loading scene"); // Set progress to 100 when done
                    return loadedScene;
                }

                BasisDebug.LogError("Failed to get loaded scene.");
            }
            else
            {
                BasisDebug.LogError("Path was null or empty! this should not be happening!");
            }

            return new Scene();
        }
    }
}
