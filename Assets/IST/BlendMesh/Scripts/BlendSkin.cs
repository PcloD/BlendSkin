using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IST.BlendSkin
{
    [ExecuteInEditMode]
    [AddComponentMenu("BlendSkin/BlendSkin")]
    public class BlendSkin : MonoBehaviour
    {
        #region Fields
        public SkinnedMeshRenderer baseMesh;
        public List<BlendSource> blendSources;

        [SerializeField] ComputeShader blendCompute;
        MeshBuffers baseMeshBuffers = new MeshBuffers();
        List<MeshBuffers> blendSourceBuffers = new List<MeshBuffers>();
        #endregion


        #region Types
        [Serializable]
        public class BlendSource
        {
            public SkinnedMeshRenderer mesh;
            public float weight;
        }

        // non-serializable
        public class MeshBuffers
        {
            public ComputeBuffer bones;
            public ComputeBuffer weights;
            public ComputeBuffer srcPoints;
            public ComputeBuffer srcNormals;
            public ComputeBuffer srcTangents;
            public ComputeBuffer dstPoints;
            public ComputeBuffer dstNormals;
            public ComputeBuffer dstTangents;


            public void Release()
            {
                if (weights != null)
                {
                    weights.Release();
                    weights = null;
                }
                if (bones != null)
                {
                    bones.Release();
                    bones = null;
                }

                if (srcPoints != null)
                {
                    srcPoints.Release();
                    srcPoints = null;
                }
                if (srcNormals != null)
                {
                    srcNormals.Release();
                    srcNormals = null;
                }
                if (srcTangents != null)
                {
                    srcTangents.Release();
                    srcTangents = null;
                }

                if (dstPoints != null)
                {
                    dstPoints.Release();
                    dstPoints = null;
                }
                if (dstNormals != null)
                {
                    dstNormals.Release();
                    dstNormals = null;
                }
                if (dstTangents != null)
                {
                    dstTangents.Release();
                    dstTangents = null;
                }
            }

            public void Assign(SkinnedMeshRenderer smr)
            {
                Release();
                if (smr == null)
                    return;
                var mesh = smr.sharedMesh;
                if (mesh == null)
                    return;

                var b = smr.bones;

                var vertexCount = mesh.vertexCount;
                {
                    var w = mesh.boneWeights;
                    if (w.Length > 0)
                    {
                        // todo: support unlimited influence count on 2019.x
                        weights = new ComputeBuffer(w.Length, 32);
                        weights.SetData(w);
                    }
                }
                {
                    var p = mesh.vertices;
                    srcPoints = new ComputeBuffer(p.Length, 12);
                    srcPoints.SetData(p);
                    dstPoints = new ComputeBuffer(p.Length, 12);
                }
                {
                    var n = mesh.normals;
                    if (n.Length == 0)
                        n = new Vector3[vertexCount];

                    srcNormals = new ComputeBuffer(n.Length, 12);
                    srcNormals.SetData(n);
                    dstNormals = new ComputeBuffer(n.Length, 12);
                }
                {
                    var t = mesh.tangents;
                    if (t.Length == 0)
                        t = new Vector4[vertexCount];

                    srcTangents = new ComputeBuffer(t.Length, 16);
                    srcTangents.SetData(t);
                    dstTangents = new ComputeBuffer(t.Length, 16);
                }
            }
        }
        #endregion


        #region Impl
        void Release()
        {
            baseMeshBuffers.Release();
            foreach (var b in blendSourceBuffers)
                b.Release();
            blendSourceBuffers.Clear();
        }

#if UNITY_EDITOR
        void Reset()
        {
            blendCompute = AssetDatabase.LoadAssetAtPath<ComputeShader>(AssetDatabase.GUIDToAssetPath("6c9bb1542a25b414d8eb9a86d401531f"));
        }
#endif

        void OnEnable()
        {

        }

        void OnDisable()
        {
            Release();
        }

        void Update()
        {
        }
        #endregion
    }
}
