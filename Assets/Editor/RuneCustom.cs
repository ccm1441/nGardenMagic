using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(RuneScriptable))]
public class RuneCustom : Editor
{
    private ReorderableList _normalSkill;

    private void OnEnable()
    {
       _normalSkill = new ReorderableList(serializedObject, serializedObject.FindProperty("normalAbilityList"), true, true, true, true);

        _normalSkill.drawElementCallback =
       (Rect rect, int index, bool isActive, bool isFocused) =>
       {
           var element = _normalSkill.serializedProperty.GetArrayElementAtIndex(index);
           rect.y += 2;
           EditorGUI.PropertyField(
           new Rect(rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight),
           element.FindPropertyRelative("ability"), GUIContent.none);
           EditorGUI.PropertyField(
           new Rect(rect.x + 160, rect.y, 70, EditorGUIUtility.singleLineHeight),
           element.FindPropertyRelative("value"), GUIContent.none);
           EditorGUI.PropertyField(
           new Rect(rect.x + 240, rect.y, 100, EditorGUIUtility.singleLineHeight),
           element.FindPropertyRelative("unit"), GUIContent.none);
       };


        _normalSkill.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "노말 등급 옵션");
        };
    }


    public override void OnInspectorGUI()
    {
        GUIStyle style = new GUIStyle();
        RuneScriptable rune = (RuneScriptable)target;

        GUILayout.Label("<size=15><color=yellow>룬 기본정보</color></size>", style);
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.black);
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        rune.Image = EditorGUILayout.ObjectField(rune.Image, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64)) as Sprite;
        EditorGUILayout.BeginVertical();
        rune.Name = EditorGUILayout.TextField("룬 이름", rune.Name);
        rune.ProType = (PropertyType)EditorGUILayout.EnumPopup("속성", rune.ProType);
        rune.RuneType = (RuneType)EditorGUILayout.EnumPopup("룬 분류", rune.RuneType);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        rune.Explain =  EditorGUILayout.TextArea(rune.Explain, GUILayout.Height(40));

        GUILayout.Space(10);
        GUILayout.Label("<size=15><color=yellow>룬 옵션 설정</color></size>", style);
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.black);
        GUILayout.Space(5);

        // 등급 룬 일시
        if (rune.RuneType == RuneType.Class)
        {
            rune.CommonAbility = (SkillAbility)EditorGUILayout.EnumPopup("능력", rune.CommonAbility);
            EditorGUILayout.IntField("C 등급(%)", rune.CValue);
            EditorGUILayout.IntField("B 등급(%)", rune.BValue);
            EditorGUILayout.IntField("A 등급(%)", rune.AValue);
            EditorGUILayout.IntField("S 등급(%)", rune.SValue);
            EditorGUILayout.IntField("S+ 등급(%)", rune.SPValue);
        }
        else
        {
            rune.targetSkill = (TargetSkill)EditorGUILayout.EnumPopup("변경 할 스킬", rune.targetSkill);
            serializedObject.Update();
            _normalSkill.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(rune);

        //DrawDefaultInspector();
    }
}
