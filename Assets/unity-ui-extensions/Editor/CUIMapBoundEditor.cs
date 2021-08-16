/// Credit Titinious (https://github.com/Titinious)
/// Sourced from - https://github.com/Titinious/CurlyUI

using UnityEditor;

namespace UnityEngine.UI.Extensions
{
    [CustomEditor(typeof(CUIMapBound))]
    [CanEditMultipleObjects]
    public class CUIMapBoundEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        protected void OnSceneGUI()
        {
            CUIMapBound script = (CUIMapBound)this.target;

            if (script.ControlPoints != null)
            {
                Vector3[] controlPoints = script.ControlPoints;

                Transform handleTransform = script.transform;
                Quaternion handleRotation = script.transform.rotation;

                for (int p = 0; p < controlPoints.Length; p++)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPt = Handles.DoPositionHandle(handleTransform.TransformPoint(controlPoints[p]), handleRotation);
                    if (EditorGUI.EndChangeCheck())
                    {

                        Undo.RecordObject(script, "Move Point");
                        EditorUtility.SetDirty(script);
                        controlPoints[p] = handleTransform.InverseTransformPoint(newPt);
                        script.Refresh();
                    }
                }
            }
        }
    }
}