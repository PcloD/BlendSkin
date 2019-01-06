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
        public List<BlendSource> blendSources = new List<BlendSource>();
        public List<Material> materials = new List<Material>();

        [SerializeField] ComputeShader m_blendCompute;
        MeshBuffers m_baseMeshBuffers = new MeshBuffers();
        List<MeshBuffers> m_blendSourceBuffers = new List<MeshBuffers>();

        int m_kSkinning4 = -1;
        int m_kBlendPrepare = -1;
        int m_kBlendAdd = -1;
        int m_kBlendAddSkinning4 = -1;
        int m_kBlendFinish = -1;
        #endregion


        #region Types
        [Serializable]
        public class BlendSource
        {
            public SkinnedMeshRenderer mesh;
            [Range(-5, 5)] public float weight;
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

            public bool valid
            {
                get { return srcPoints != null; }
            }
            public bool isSkinned
            {
                get { return bones != null && weights != null; }
            }

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

                {
                    var b = smr.bones;
                    if (b.Length > 0)
                    {
                        var matrices = new Matrix4x4[b.Length];
                        for (int bi = 0; bi < b.Length; ++bi)
                            if (b[bi] != null)
                                matrices[bi] = b[bi].localToWorldMatrix;
                        bones = new ComputeBuffer(b.Length, 64);
                        bones.SetData(matrices);
                    }
                }

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
        void ReleaseBuffers()
        {
            m_baseMeshBuffers.Release();
            foreach (var b in m_blendSourceBuffers)
                b.Release();
            m_blendSourceBuffers.Clear();
        }

        void SetupResources()
        {
#if UNITY_EDITOR
            if (m_blendCompute == null)
                m_blendCompute = AssetDatabase.LoadAssetAtPath<ComputeShader>(AssetDatabase.GUIDToAssetPath("6c9bb1542a25b414d8eb9a86d401531f"));
#endif
        }

        void SetupKernels()
        {
            if (m_kSkinning4 < 0)
                m_kSkinning4 = m_blendCompute.FindKernel("Skinning4");
            if (m_kBlendPrepare < 0)
                m_kBlendPrepare = m_blendCompute.FindKernel("BlendPrepare");
            if (m_kBlendAdd < 0)
                m_kBlendAdd = m_blendCompute.FindKernel("BlendAdd");
            if (m_kBlendAddSkinning4 < 0)
                m_kBlendAddSkinning4 = m_blendCompute.FindKernel("BlendAddSkinning4");
            if (m_kBlendFinish < 0)
                m_kBlendFinish = m_blendCompute.FindKernel("BlendFinish");
        }

        void SetupBuffers()
        {
            if (!m_baseMeshBuffers.valid)
                m_baseMeshBuffers.Assign(baseMesh);

            if (m_blendSourceBuffers.Count != blendSources.Count)
            {
                m_blendSourceBuffers.Clear();
                for (int si = 0; si < blendSources.Count; ++si)
                {
                    var tmp = new MeshBuffers();
                    tmp.Assign(blendSources[si].mesh);
                    m_blendSourceBuffers.Add(tmp);
                }
            }
        }

        void DoSkinningAndBlending()
        {
            SetupKernels();
            SetupBuffers();

            // todo
        }

        void Draw()
        {
            // todo
        }

#if UNITY_EDITOR
        void Reset()
        {
            ReleaseBuffers();
            SetupResources();
        }

        void OnValidate()
        {
            ReleaseBuffers();
            SetupResources();
        }
#endif

        void OnEnable()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                EditorApplication.update += Update;
#endif
        }

        void OnDisable()
        {
            ReleaseBuffers();
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                EditorApplication.update -= Update;
#endif
        }

        void Update()
        {
            if (baseMesh == null)
                return;

            DoSkinningAndBlending();
            Draw();
        }
        #endregion
    }
}
