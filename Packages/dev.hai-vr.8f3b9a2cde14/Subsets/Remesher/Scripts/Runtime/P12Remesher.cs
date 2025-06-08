using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Hai.Project12.Remesher.Runtime
{
    public class P12Remesher : MonoBehaviour
    {
        private const float BoneWeightAcceptanceThreshold = 0.4f;
        [SerializeField] private SkinnedMeshRenderer[] sources; // UGC Rule.

        private void Awake()
        {
            foreach (var skinnedMeshRenderer in sources)
            {
                if (null != skinnedMeshRenderer)
                {
                    ManipulateSmr(skinnedMeshRenderer);
                }
            }
        }

        private static void ManipulateSmr(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var originalMesh = skinnedMeshRenderer.sharedMesh;

            var vertexCount = originalMesh.vertexCount;
            var boneCountPerVertex = originalMesh.GetBonesPerVertex();
            var allBoneWeights = originalMesh.GetAllBoneWeights();
            var vertexIdToStartingIndexInsideBoneWeightsArray = CalculateVertexIdToStartingIndexInsideBoneWeightsArray(boneCountPerVertex);

            var totalBoneCount = originalMesh.bindposes.Length;

            var boneIndexToMajorlyVertexIds = new List<List<int>>();
            for (var i = 0; i < totalBoneCount; i++)
            {
                boneIndexToMajorlyVertexIds.Add(new List<int>());
            }

            for (var vertexId = 0; vertexId < vertexCount; vertexId++)
            {
                if (boneCountPerVertex[vertexId] > 0) // Guarantees that thisVertexWeights is non-empty
                {
                    var thisVertexWeights = ReadInputBoneWeightsAsNewList(vertexId, boneCountPerVertex, vertexIdToStartingIndexInsideBoneWeightsArray, allBoneWeights);

                    var mostWeighted = thisVertexWeights[0];
                    boneIndexToMajorlyVertexIds[mostWeighted.boneIndex].Add(vertexId);

                    // TODO: Add the other vertices that have a weight greater than some number (maybe 0.4)
                    for (var i = 1; i < thisVertexWeights.Count; i++)
                    {
                        var currentWeight = thisVertexWeights[i];
                        if (currentWeight.weight > BoneWeightAcceptanceThreshold)
                        {
                            boneIndexToMajorlyVertexIds[currentWeight.boneIndex].Add(vertexId);
                        }
                    }
                }
            }

            var generatedMeshes = new List<Mesh>();
            var whichBoneIndexForThatGeneratedMesh = new List<int>();

            var originalVertices = originalMesh.vertices;
            var originalTriangles = originalMesh.triangles;
            for (var boneIndex = 0; boneIndex < totalBoneCount; boneIndex++)
            {
                var majorlyVertexIds = boneIndexToMajorlyVertexIds[boneIndex];

                if (majorlyVertexIds.Count > 0)
                {
                    var meshForThisBone = new Mesh();
                    var verticesForThisBone = new Vector3[majorlyVertexIds.Count];
                    for (var index = 0; index < majorlyVertexIds.Count; index++)
                    {
                        var vertexId = majorlyVertexIds[index];
                        verticesForThisBone[index] = originalVertices[vertexId];
                    }

                    meshForThisBone.vertices = verticesForThisBone;
                    meshForThisBone.triangles = ReconstructTriangles(majorlyVertexIds, originalTriangles);

                    generatedMeshes.Add(meshForThisBone);
                    whichBoneIndexForThatGeneratedMesh.Add(boneIndex);
                }
            }

            //

            var smrBones = skinnedMeshRenderer.bones;

            for (var index = 0; index < generatedMeshes.Count; index++)
            {
                var generatedMesh = generatedMeshes[index];
                var boneIndex = whichBoneIndexForThatGeneratedMesh[index];

                var smrBone = smrBones[boneIndex];
                var go = new GameObject
                {
                    name = $"{skinnedMeshRenderer.name}_MeshCollider_Bone{boneIndex:000}",
                    transform =
                    {
                        parent = smrBone != null ? smrBone.transform : null,
                        position = skinnedMeshRenderer.transform.position,
                        rotation = skinnedMeshRenderer.transform.rotation,
                        localScale = skinnedMeshRenderer.transform.localScale // FIXME: Incorrect
                    }
                };
                go.SetActive(false);

                var collider = go.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = generatedMesh;
                collider.cookingOptions = MeshColliderCookingOptions.EnableMeshCleaning
                                          | MeshColliderCookingOptions.WeldColocatedVertices
                                          | MeshColliderCookingOptions.CookForFasterSimulation;

                var rigidbody = go.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.mass = 1f;
                rigidbody.automaticCenterOfMass = true;
                rigidbody.useGravity = false;

                go.SetActive(true);
            }
        }

        private static int[] ReconstructTriangles(List<int> majorlyVertexIds, int[] originalTriangles)
        {
            var reconstructedTriangles = new List<int>();

            var keepThoseVertices = new HashSet<int>(majorlyVertexIds);

            for (var indexWithinTriangles = 0; indexWithinTriangles < originalTriangles.Length; indexWithinTriangles += 3)
            {
                var vertexIdForA = originalTriangles[indexWithinTriangles + 0];
                var vertexIdForB = originalTriangles[indexWithinTriangles + 1];
                var vertexIdForC = originalTriangles[indexWithinTriangles + 2];

                if (keepThoseVertices.Contains(vertexIdForA)
                    && keepThoseVertices.Contains(vertexIdForB)
                    && keepThoseVertices.Contains(vertexIdForC))
                {
                    // This triangle needs to be kept.
                    var regeneratedVertexIdForA = majorlyVertexIds.IndexOf(vertexIdForA);
                    var regeneratedVertexIdForB = majorlyVertexIds.IndexOf(vertexIdForB);
                    var regeneratedVertexIdForC = majorlyVertexIds.IndexOf(vertexIdForC);
                    reconstructedTriangles.Add(regeneratedVertexIdForA);
                    reconstructedTriangles.Add(regeneratedVertexIdForB);
                    reconstructedTriangles.Add(regeneratedVertexIdForC);
                }
            }

            return reconstructedTriangles.ToArray();
        }

        private static int[] CalculateVertexIdToStartingIndexInsideBoneWeightsArray(NativeArray<byte> boneCountPerVertex)
        {
            var startingIndices = new List<int>();
            var anchor = 0;
            foreach (var boneCountForThatVertex in boneCountPerVertex)
            {
                startingIndices.Add(anchor);
                anchor += boneCountForThatVertex;
            }

            var idToStartingIndexInsideBoneWeightsArray = startingIndices.ToArray();
            return idToStartingIndexInsideBoneWeightsArray;
        }

        private static List<BoneWeight1> ReadInputBoneWeightsAsNewList(int vertexId, NativeArray<byte> boneCountPerVertex, int[] vertexIdToStartingIndexInsideBoneWeightsArray, NativeArray<BoneWeight1> allBoneWeights)
        {
            var startingIndex = vertexIdToStartingIndexInsideBoneWeightsArray[vertexId];

            var boneWeight1s = new List<BoneWeight1>();
            for (var offset = 0; offset < boneCountPerVertex[vertexId]; offset++)
            {
                var currentBoneWeight = allBoneWeights[startingIndex + offset];
                boneWeight1s.Add(currentBoneWeight);
            }

            return boneWeight1s;
        }
    }
}
