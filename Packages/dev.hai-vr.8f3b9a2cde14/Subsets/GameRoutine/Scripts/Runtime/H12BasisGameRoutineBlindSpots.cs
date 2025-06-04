/*
MIT License

Copyright (c) 2024 MR LUKE B DOOLAN

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

-----------------------------------------------------------------------

This project includes code from third-party projects licensed under the Apache License 2.0. For details, see `Assets/third_party/plugins/SteamAudio/LICENSE`.

This project includes third-party trademarks as described in `Assets/third_party/plugins/SteamAudio/TRADEMARK_RIGHTS.md`. For more details, see `Assets/third_party/plugins/SteamAudio/TRADEMARK_RIGHTS.md`.
*/

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Subsets.GameRoutine.Scripts.Runtime
{
    public class H12BasisGameRoutineBlindSpots
    {
        // From BasisBundleLoadAsset.

        /// Load scene without a bundle. Calling this does not auto-re-tetrahedralize probes.
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

        /// Unload provided scene. Calling this does not auto-re-tetrahedralize probes.
        public async Task UnloadGameLevel(Scene loadedLevelNullable)
        {
            await SceneManager.UnloadSceneAsync(loadedLevelNullable);
        }
    }
}
