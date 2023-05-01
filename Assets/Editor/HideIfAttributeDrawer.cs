using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class HideIfPropertyDrawer : PropertyDrawer {
    HideIfAttribute hideIf;
    private float propertyHeight;

    void print(object message) {
        MonoBehaviour.print(message);
    }

    object ParseArgument(object arg, SerializedProperty property) {
        Type argType = arg.GetType();

        if(argType == typeof(string)) {

            SerializedProperty serializedProperty = property.serializedObject.FindProperty((string)arg);
            if(serializedProperty != null) {
                switch(serializedProperty.propertyType) {
                    case SerializedPropertyType.Boolean:
                        return serializedProperty.boolValue;

                    case SerializedPropertyType.Integer:
                        return serializedProperty.intValue;

                    case SerializedPropertyType.Float:
                        return serializedProperty.floatValue;

                    case SerializedPropertyType.String:
                        return serializedProperty.stringValue;

                    case SerializedPropertyType.Enum:
                        return serializedProperty.enumValueIndex;
                        // return serializedProperty.enumNames[serializedProperty.enumValueIndex];
                        // return serializedProperty.enumValueIndex+":"+serializedProperty.enumNames[serializedProperty.enumValueIndex];

                    default:
                        return "Unsupported type";
                }
                // return serializedProperty.objectReferenceValue
            }
            else {
                // MonoBehaviour.print("Not a var");
                string stringArg = (string)arg;
                if(stringArg[stringArg.Length-1] == 'f' || stringArg[stringArg.Length-1] == 'F') stringArg = stringArg.Remove(stringArg.Length - 1);


                if( float.TryParse(stringArg, out float result)) {
                    return result;
                }
            }
        }

        if(argType.IsEnum) {
            return (int)arg;
        }

        return arg;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return propertyHeight;
        // return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool hide = false;
        hideIf = attribute as HideIfAttribute;

        object arg1 = ParseArgument(hideIf.arg1, property);
        object arg2 = ParseArgument(hideIf.arg2, property);
        // MonoBehaviour.print("ARG1:"+arg1+":"+arg1.GetType());
        // MonoBehaviour.print("ARG2:"+arg2+":"+arg2.GetType());

        if(hideIf.comparisonType == HideIfAttribute.ComparisonType.Equal) {
            if(arg1 == arg2) hide = true;
        }

        if(hideIf.comparisonType == HideIfAttribute.ComparisonType.Equal) {
            hide = arg1.ToString() == arg2.ToString();
        }
        else if(hideIf.comparisonType == HideIfAttribute.ComparisonType.NotEqual) {
            hide = arg1.ToString() != arg2.ToString();
        }
        else {
            if(float.TryParse(arg1.ToString(), out float result1)) {
                if(float.TryParse(arg2.ToString(), out float result2)) {

                    if(hideIf.comparisonType == HideIfAttribute.ComparisonType.Less) {
                        hide = result1 < result2;
                    }
                    else if(hideIf.comparisonType == HideIfAttribute.ComparisonType.Greater){
                        hide = result1 > result2;
                    }
                    else if(hideIf.comparisonType == HideIfAttribute.ComparisonType.LessOrEqual){
                        hide = result1 <= result2;
                    }
                    else if(hideIf.comparisonType == HideIfAttribute.ComparisonType.GreaterOrEqual){
                        hide = result1 >= result2;
                    }
                }
                else {
                    print("Arg2 is not comparable by difference");
                }
            }
            else {
                print("Arg1 is not comparable by difference");
            }
        }
        
        propertyHeight = base.GetPropertyHeight(property, label);
   
        if(hide)
        {
            if (hideIf.disableType ==  HideIfAttribute.DisableType.ReadOnly)
            {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label);
                GUI.enabled = true;
            }
            else
            {
                propertyHeight = 0f;
            }
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
            // base.OnGUI(position, property, label);
        }
    }

}