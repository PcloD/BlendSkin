using System.Linq;
using UnityEngine;
using UnityEditor;
using IST.BlendSkin;

namespace IST.BlendSkinEditor
{
    [CustomEditor(typeof(IST.BlendSkin.BlendSkin))]
    public class BlendSkinEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
