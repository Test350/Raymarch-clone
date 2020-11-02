using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework.Constraints;

[CustomEditor(typeof(Operation)), CanEditMultipleObjects]

public class OperatorEditor : Editor
{
    public SerializedProperty
        Operation_Prop,
        BlendStrength_Prop,
        Shapes_Prop;


    void OnEnable()
    {
        this.Operation_Prop = serializedObject.FindProperty("operation");
        this.BlendStrength_Prop = serializedObject.FindProperty("blendStrength");
        this.Shapes_Prop = serializedObject.FindProperty("shapes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(Operation_Prop);
        EditorGUILayout.PropertyField(Shapes_Prop);

        Operation.Op operation = (Operation.Op)Operation_Prop.enumValueIndex;

        switch (operation)
        {
            case Operation.Op.None:
            case Operation.Op.Intersect:
            case Operation.Op.Subtract:
                break;
            case Operation.Op.Blend:
                EditorGUILayout.Slider(BlendStrength_Prop, 0.0f, 3.0f);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
