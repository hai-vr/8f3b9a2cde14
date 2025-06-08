using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Hai.Project12.Remesher.Runtime
{
    /// Given SkinnedMeshRenderers, this splits the mesh into several meshes where each mesh represents a bone,
    /// and adds a convex hull MeshCollider to each bone. All the mesh simplification concerns are handled by
    /// what is already built-in the MeshCollider, not us.
    public class P12Remesher : MonoBehaviour
    {
        private const float BoneWeightAcceptanceThreshold = 0.4f;

        // TODO: Should we use FastMidphase? does it matter?
        private const MeshColliderCookingOptions Cooking = MeshColliderCookingOptions.EnableMeshCleaning
                                                           | UnityEngine.MeshColliderCookingOptions.WeldColocatedVertices
                                                           | UnityEngine.MeshColliderCookingOptions.CookForFasterSimulation
                                                           | UnityEngine.MeshColliderCookingOptions.UseFastMidphase;

        [SerializeField] private SkinnedMeshRenderer[] sources; // UGC Rule.
        [SerializeField] private bool createRigidbodies = true;

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

        private void ManipulateSmr(SkinnedMeshRenderer skinnedMeshRenderer)
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

            // Triage each vertexId into the bones it belongs to.

            for (var vertexId = 0; vertexId < vertexCount; vertexId++)
            {
                if (boneCountPerVertex[vertexId] > 0) // Guarantees that thisVertexWeights is non-empty
                {
                    var thisVertexWeights = ReadInputBoneWeightsAsNewList(vertexId, boneCountPerVertex, vertexIdToStartingIndexInsideBoneWeightsArray, allBoneWeights);

                    var mostWeighted = thisVertexWeights[0];
                    boneIndexToMajorlyVertexIds[mostWeighted.boneIndex].Add(vertexId);

                    // Notice how this starts at 1. We always want the heaviest bone to be assigned, even if it's below threshold.
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

            // Rebuild the mesh

            var generatedMeshes = new List<Mesh>();
            var whichBoneIndexForThatGeneratedMesh = new List<int>();

            var originalVertices = originalMesh.vertices;
            var originalTriangles = originalMesh.triangles;
            for (var boneIndex = 0; boneIndex < totalBoneCount; boneIndex++)
            {
                var majorlyVertexIds = boneIndexToMajorlyVertexIds[boneIndex];

                if (majorlyVertexIds.Count > 0)
                {
                    var verticesForThisBone = new Vector3[majorlyVertexIds.Count];
                    for (var index = 0; index < majorlyVertexIds.Count; index++)
                    {
                        var vertexId = majorlyVertexIds[index];
                        verticesForThisBone[index] = originalVertices[vertexId];
                    }

                    if (verticesForThisBone.Length >= 3)
                    {
                        var trianglesForThisBone = ReconstructTriangles(majorlyVertexIds, originalTriangles);

                        var thereIsAtLeastOneTriangle = trianglesForThisBone.Length >= 3;
                        if (thereIsAtLeastOneTriangle)
                        {
                            var meshForThisBone = new Mesh();

                            meshForThisBone.vertices = verticesForThisBone;
                            meshForThisBone.triangles = trianglesForThisBone;

                            generatedMeshes.Add(meshForThisBone);
                            whichBoneIndexForThatGeneratedMesh.Add(boneIndex);

                            Physics.BakeMesh(meshForThisBone.GetInstanceID(), true, Cooking);
                        }
                    }
                }
            }

            //

            var smrBones = skinnedMeshRenderer.bones;
            var bindpose = originalMesh.bindposes;

            for (var index = 0; index < generatedMeshes.Count; index++)
            {
                var generatedMesh = generatedMeshes[index];
                var boneIndex = whichBoneIndexForThatGeneratedMesh[index];

                var smrBone = smrBones[boneIndex];
                var bindposeForThisBone = bindpose[boneIndex];

                var go = new GameObject
                {
                    name = $"{skinnedMeshRenderer.name}_MeshCollider_Bone{boneIndex:000}",
                    transform =
                    {
                        // TODO: When adding the head, if it's a local avatar, then add it to the same system that the
                        // shadow clone head transform uses to avoid first-person shrinking.
                        parent = smrBone != null ? smrBone.transform : null,
                        localPosition = bindposeForThisBone.GetPosition(),
                        localRotation = bindposeForThisBone.rotation,
                        localScale = skinnedMeshRenderer.transform.localScale // FIXME: Likely incorrect
                    }
                };
                go.SetActive(false); // Ensure that components initialize in one go, after we have defined them.

                var ourCollider = go.AddComponent<MeshCollider>();
                ourCollider.convex = true;
                ourCollider.sharedMesh = generatedMesh;
                ourCollider.cookingOptions = Cooking;

                if (createRigidbodies)
                {
                    var ourRigidbody = go.AddComponent<Rigidbody>();
                    ourRigidbody.isKinematic = true;
                    ourRigidbody.mass = 1f;
                    ourRigidbody.automaticCenterOfMass = true;
                    ourRigidbody.useGravity = false;
                }

                go.SetActive(true);
            }
        }

        private static int[] ReconstructTriangles(List<int> majorlyVertexIds, int[] originalTriangles)
        {
            var reconstructedTriangles = new List<int>();

            var keepThoseVertices = new HashSet<int>(majorlyVertexIds);

            for (var indexWithinTriangles = 0; indexWithinTriangles < originalTriangles.Length; indexWithinTriangles += 3)
            {
                var vertexIdForA = originalTriangles[indexWithinTriangles];
                var vertexIdForB = originalTriangles[indexWithinTriangles + 1];
                var vertexIdForC = originalTriangles[indexWithinTriangles + 2];

                // (Note: The default Basis avatar is painted too weird, so the arms and lower body won't pass this condition.)
                if (keepThoseVertices.Contains(vertexIdForA)
                    && keepThoseVertices.Contains(vertexIdForB)
                    && keepThoseVertices.Contains(vertexIdForC))
                {
                    // The index of the vertexId inside the majorlyVertexIds list is the new vertexId of our new mesh.
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
