using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelGridHint))]
public class LevelGridHintEditor : Editor
{
    SerializedProperty m_hand;
    SerializedProperty m_line;
    SerializedProperty m_hintcord;
    SerializedProperty m_showhint;

    void OnEnable()
    {
        // Fetch the objects from the GameObject script to display in the inspector        
        m_hand = serializedObject.FindProperty("hand");
        m_line = serializedObject.FindProperty("hintLine");
        m_hintcord = serializedObject.FindProperty("AddOrRemove");
        m_showhint = serializedObject.FindProperty("ShowHintAtStart");
    }
    public override void OnInspectorGUI()
    {
        LevelGridHint myTarget = (LevelGridHint)target;

        EditorGUILayout.PropertyField(m_hand, new GUIContent("Hand "));
        EditorGUILayout.PropertyField(m_line, new GUIContent("Hintline "));
        EditorGUILayout.PropertyField(m_hintcord, new GUIContent("Hintcord "));
        EditorGUILayout.PropertyField(m_showhint, new GUIContent("ShowHintAtStart "));        

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(" ", GUILayout.Width(100), GUILayout.Height(20)))
        {
        }
        if (GUILayout.Button("Up", GUILayout.Width(100), GUILayout.Height(20)))
        {
            if (myTarget.AddOrRemove.x > 0)
            {
                myTarget.AddOrRemove = new Vector2(myTarget.AddOrRemove.x - 1, myTarget.AddOrRemove.y);
                myTarget.AddToHintLine();
                Toggle(myTarget.transform);
            }
        }
        if (GUILayout.Button("Remove All", GUILayout.Width(100), GUILayout.Height(20)))
        {
            myTarget.hintLine.ResetAllPoints();
            Toggle(myTarget.transform);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Left", GUILayout.Width(100), GUILayout.Height(20)))
        {
            if (myTarget.AddOrRemove.y > 0)
            {
                myTarget.AddOrRemove = new Vector2(myTarget.AddOrRemove.x, myTarget.AddOrRemove.y - 1);
                myTarget.AddToHintLine();
                Toggle(myTarget.transform);
            }
        }
        if (GUILayout.Button("Start", GUILayout.Width(100), GUILayout.Height(20)))
        {
            myTarget.AddToHintLine();
        }
        if (GUILayout.Button("Right", GUILayout.Width(100), GUILayout.Height(20)))
        {
            if (myTarget.AddOrRemove.y < myTarget.GetGridSize().y - 1)
            {
                myTarget.AddOrRemove = new Vector2(myTarget.AddOrRemove.x, myTarget.AddOrRemove.y + 1);
                myTarget.AddToHintLine();
                Toggle(myTarget.transform);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("", GUILayout.Width(100), GUILayout.Height(20)))
        {
        }
        if (GUILayout.Button("Down", GUILayout.Width(100), GUILayout.Height(20)))
        {
            if (myTarget.AddOrRemove.x < myTarget.GetGridSize().x - 1)
            {
                myTarget.AddOrRemove = new Vector2(myTarget.AddOrRemove.x + 1, myTarget.AddOrRemove.y);
                myTarget.AddToHintLine();
                Toggle(myTarget.transform);
            }
        }
        if (GUILayout.Button("", GUILayout.Width(100), GUILayout.Height(20)))
        {

        }
        EditorGUILayout.EndHorizontal();

        // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }
    void Toggle(Transform tr)
    {
        tr.gameObject.SetActive(false);
        tr.gameObject.SetActive(true);
    }
}
