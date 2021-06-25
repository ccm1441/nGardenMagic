using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditorInternal;

[CanEditMultipleObjects]
[CustomEditor(typeof(SkillScriptable))]
public class SkillCustom : Editor
{
    private ReorderableList _levelUpList;
    private ReorderableList _staffList;
    private ReorderableList _staffAddSkill;
    private ReorderableList _staffSpecialSkill;
    private ReorderableList _staffSelfSkill;


    private SkillScriptable _skill;

    private void OnEnable()
    {
        _levelUpList = new ReorderableList(serializedObject, serializedObject.FindProperty("LevelUpRewardValue"), true, true, true, true);
        _staffList = new ReorderableList(serializedObject, serializedObject.FindProperty("staffOption"), true, true, true, true);
        _staffAddSkill = new ReorderableList(serializedObject, serializedObject.FindProperty("StaffAddSkill"), true, true, true, true);
        _staffSpecialSkill = new ReorderableList(serializedObject, serializedObject.FindProperty("specialSkill"), true, true, true, true);
        _staffSelfSkill = new ReorderableList(serializedObject, serializedObject.FindProperty("selfSkill"), true, true, true, true);

        _levelUpList.drawElementCallback =
        (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = _levelUpList.serializedProperty.GetArrayElementAtIndex(index);
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

        _staffList.drawElementCallback =
       (Rect rect, int index, bool isActive, bool isFocused) =>
       {
           var element = _staffList.serializedProperty.GetArrayElementAtIndex(index);
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

        _staffAddSkill.drawElementCallback =
      (Rect rect, int index, bool isActive, bool isFocused) =>
      {
          var element = _staffAddSkill.serializedProperty.GetArrayElementAtIndex(index);

          rect.y += 2;

          EditorGUI.PropertyField(
          new Rect(rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight),
          element, GUIContent.none);

      };

        _staffSpecialSkill.drawElementCallback =
              (Rect rect, int index, bool isActive, bool isFocused) =>
              {
                  var element = _staffSpecialSkill.serializedProperty.GetArrayElementAtIndex(index);
                  rect.y += 2;
                  EditorGUI.PropertyField(
                  new Rect(rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight),
                  element.FindPropertyRelative("ability"), GUIContent.none);
                  EditorGUI.PropertyField(
                  new Rect(rect.x + 160, rect.y, 45, EditorGUIUtility.singleLineHeight),
                  element.FindPropertyRelative("value"), GUIContent.none);
                  EditorGUI.PropertyField(
                  new Rect(rect.x + 215, rect.y, 80, EditorGUIUtility.singleLineHeight),
                  element.FindPropertyRelative("unit"), GUIContent.none);
                  EditorGUI.PropertyField(
                  new Rect(rect.x + 310, rect.y, 15, EditorGUIUtility.singleLineHeight),
                  element.FindPropertyRelative("SetAbility"), GUIContent.none);
              };

        _staffSelfSkill.drawElementCallback =
      (Rect rect, int index, bool isActive, bool isFocused) =>
      {
          var element = _staffSelfSkill.serializedProperty.GetArrayElementAtIndex(index);
          rect.y += 2;
          EditorGUI.PropertyField(
          new Rect(rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight),
          element.FindPropertyRelative("ability"), GUIContent.none);
          EditorGUI.PropertyField(
          new Rect(rect.x + 160, rect.y, 45, EditorGUIUtility.singleLineHeight),
          element.FindPropertyRelative("value"), GUIContent.none);
          EditorGUI.PropertyField(
          new Rect(rect.x + 215, rect.y, 80, EditorGUIUtility.singleLineHeight),
          element.FindPropertyRelative("unit"), GUIContent.none);
          EditorGUI.PropertyField(
          new Rect(rect.x + 310, rect.y, 15, EditorGUIUtility.singleLineHeight),
          element.FindPropertyRelative("SetAbility"), GUIContent.none);
      };

        _levelUpList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "레벨업 당 증가 수치");
        };

        _staffList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "지팡이 장착 옵션");
        };

        _staffAddSkill.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "특성 스킬 데이터");
        };

        _staffSpecialSkill.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "지팡이 특성 스킬");
        };

        _staffSelfSkill.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "장착시 발동 스킬");
        };
    }

    public override void OnInspectorGUI()
    {
        GUIStyle style = new GUIStyle();
        style.richText = true;

        _skill = (SkillScriptable)target;

        GUILayout.Label("<size=15><color=yellow>스킬 타입 설정</color></size>", style);
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.black);
        GUILayout.Space(5);

        // 스킬 형태 설정
        _skill.UsePassive = GUILayout.Toggle(_skill.UsePassive, "해당 스킬은 " + (_skill.UsePassive ? "패시브" : "지팡이") + " 스킬 입니다.", "button", GUILayout.Height(50));
        GUILayout.Space(15);

        // 공통 정보 입력
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("<size=15><color=yellow>스킬 공통 정보</color></size>", style);
        _skill.isUniqueWeapon = EditorGUILayout.Toggle( "고유무기인가요?", _skill.isUniqueWeapon);
        EditorGUILayout.EndHorizontal();
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.black);
        GUILayout.Space(5);
        

        EditorGUILayout.BeginHorizontal();
        _skill.Image = EditorGUILayout.ObjectField(_skill.Image, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64)) as Sprite;
        _skill.skillImage = EditorGUILayout.ObjectField(_skill.skillImage, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64)) as Sprite;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        _skill.Type = (PropertyType)EditorGUILayout.EnumPopup("무기 속성", _skill.Type);
        EditorGUILayout.EndHorizontal();
        _skill.Name = EditorGUILayout.TextField("스킬 이름(한글)", _skill.Name);
        _skill.MaxLevel = EditorGUILayout.IntField("최대 레벨", _skill.MaxLevel);

        if (_skill.MaxLevel < 0) _skill.MaxLevel = 1;
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.Label("스킬설명(한글)");
        _skill.Explain = EditorGUILayout.TextArea(_skill.Explain, GUILayout.Height(50));
        if (_skill.isUniqueWeapon)
        {
            GUILayout.Label("오픈 조건 설명");
            _skill.openExplain = EditorGUILayout.TextArea(_skill.openExplain, GUILayout.Height(50));

            _skill.map = (MapType)EditorGUILayout.EnumPopup("맵", _skill.map);
            _skill.requireMapOpen = (RequireMapOpen)EditorGUILayout.EnumPopup("오픈 조건",_skill.requireMapOpen);
            _skill.openValue = EditorGUILayout.IntField("값", _skill.openValue);
            
        }

        GUILayout.Space(5);
        serializedObject.Update();
        _levelUpList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.HelpBox("레벨당 증가 수치 입력 방법\n어떤 능력을, 얼마나, 단위(퍼센트 or 값)", MessageType.Info);

        // 지팡이 정보
        if (!_skill.UsePassive)
        {
            GUILayout.Space(15);
            GUILayout.Label("<size=15><color=yellow>지팡이 기본 정보</color></size>", style);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.black);
            GUILayout.Space(5);

            EditorGUILayout.HelpBox("같은 지팡이 계열 이름 통일 필수!\n발동속도 0이면 즉시발동", MessageType.Info);
            _skill.StaffName = EditorGUILayout.TextField("지팡이 이름", _skill.StaffName);

            GUILayout.BeginHorizontal();
            GUILayout.Label("투사체 오브젝트");
            _skill.BulletPrefab = (GameObject)EditorGUILayout.ObjectField(_skill.BulletPrefab, typeof(GameObject), false);
            GUILayout.EndHorizontal();

            _skill.BulletMaxDistance = EditorGUILayout.IntField("최대 거리", _skill.BulletMaxDistance);
            if (_skill.BulletMaxDistance < 0) _skill.BulletMaxDistance = 0;
            _skill.BulletSpeed = EditorGUILayout.IntField("발동 속도(1초기준)", _skill.BulletSpeed);
            if (_skill.BulletSpeed < 0) _skill.BulletSpeed = 0;
            _skill.AttackValue = EditorGUILayout.IntField("공격 계수(%)", _skill.AttackValue);
            if (_skill.AttackValue < 0) _skill.AttackValue = 0;
            _skill.AttackCoolTime = EditorGUILayout.FloatField("쿨타임(초)", _skill.AttackCoolTime);
            if (_skill.AttackCoolTime < 0) _skill.AttackCoolTime = 0;
            _skill.soundName = EditorGUILayout.TextField("사운드 이름", _skill.soundName);

            // 지속 시간 사용
            _skill.UseDuration = EditorGUILayout.Toggle("지속 시간 사용", _skill.UseDuration);
            EditorGUI.indentLevel++;
            if (_skill.UseDuration)
            {
                _skill.DurationTime = EditorGUILayout.FloatField("지속 시간(초)", _skill.DurationTime);
                _skill.DurationAttackCool = EditorGUILayout.FloatField("피해 간격(초)", _skill.DurationAttackCool);
                GUILayout.Space(15);
            }
            EditorGUI.indentLevel--;

            // 범위 사용
            _skill.UseRange = EditorGUILayout.Toggle("피해 범위 사용", _skill.UseRange);
            if (_skill.UseRange)
            {
                _skill.RangeDataType = GUILayout.Toolbar(_skill.RangeDataType, new string[] { "캐릭터 주변", "원형", "백터" });

                if (_skill.RangeDataType == 0)
                {
                    EditorGUILayout.HelpBox("캐릭터 기준 주변 몇칸입니다.\n0 으로 하면 캐릭터크기입니다.(쉴드, 발자국)", MessageType.Info);
                    _skill.RangeCharValue = EditorGUILayout.FloatField("주변 칸수", _skill.RangeCharValue);
                    if (_skill.RangeCharValue < 0) _skill.RangeCharValue = 0;
                }
                else if (_skill.RangeDataType == 1)
                {
                    EditorGUILayout.HelpBox("캐릭터 및 스킬 범위를 원형으로 사용가능합니다.\n반지름으로 입력해주세요.", MessageType.Info);
                    _skill.RangeRadius = EditorGUILayout.FloatField("반지름", _skill.RangeRadius);
                    if (_skill.RangeRadius < 0) _skill.RangeRadius = 0;
                }
                else if (_skill.RangeDataType == 2)
                {
                    EditorGUILayout.HelpBox("캐릭터 및 스킬 범위를 사각형으로 사용가능합니다.\n캐릭터 또는 스킬의 위치를 기준으로 합니다.", MessageType.Info);
                    _skill.RangeVector = EditorGUILayout.Vector2Field("사각형 크기", _skill.RangeVector);
                    if (_skill.RangeVector.x < 0) _skill.RangeVector.x = 0;
                    if (_skill.RangeVector.y < 0) _skill.RangeVector.y = 0;
                }
                GUILayout.Space(15);
            }


            _skill.isSpecialSkill = EditorGUILayout.Toggle("특성 사용", _skill.isSpecialSkill);

            if (_skill.isSpecialSkill)
            {
                GUILayout.Label("특성 설명");
                _skill.specialExplain = EditorGUILayout.TextArea(_skill.specialExplain, GUILayout.Height(50));
                serializedObject.Update();
                _staffSpecialSkill.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.HelpBox("체크 박스를 체크하면 바로 아래 특성과 셋트로 묶입니다.\n예시) 피해량 + 지속시간", MessageType.Info);
            }
            else
            {
                GUILayout.Space(15);
                serializedObject.Update();
                _staffList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();

                serializedObject.Update();
                _staffAddSkill.DoLayoutList();
                serializedObject.ApplyModifiedProperties();

                if (_skill.StaffAddSkill == null) _skill.StaffAddSkill = new List<SkillScriptable>();

                for (int i = 0; i < _skill.StaffAddSkill.Count; i++)
                {
                    if (_skill.StaffAddSkill[i] != null && !_skill.StaffAddSkill[i].isSpecialSkill)
                        EditorGUILayout.HelpBox((i + 1) + "번칸 " + _skill.StaffAddSkill[i].Name + "스킬은 특성 스킬이 아닙니다!", MessageType.Error);
                    else if (_skill.StaffAddSkill[i] == null)
                        EditorGUILayout.HelpBox("빈 칸이 있습니다!\n특성 스킬을 넣어주세요", MessageType.Error);
                }
            }

            GUILayout.Space(15);
            GUILayout.Label("<size=15><color=yellow>지팡이 공격 범위</color></size>", style);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.black);
            GUILayout.Space(5);

            _skill.attackRnage = GUILayout.Toolbar(_skill.attackRnage, new string[] { "캐릭터 주변", "보는 방향", "자기자신" });

            if (_skill.attackRnage != 2)
            {
                if (_skill.attackRnage == 0) _skill.ScriptName = SkillDataName.PlayerAroundSkill;
                else _skill.ScriptName = SkillDataName.PlayerViewSkill;

                _skill.attackType = GUILayout.Toolbar(_skill.attackType, new string[] { "발사,날리기", "떨어뜨리기" });
                _skill.shotMonsterCount = EditorGUILayout.IntField("한번에 몇마리 공격", _skill.shotMonsterCount);
                _skill.shotCount = EditorGUILayout.IntField("한번에 몇회 공격", _skill.shotCount);
                _skill.targetingTarget = EditorGUILayout.Toggle("타겟팅 여부", _skill.targetingTarget);
                _skill.bouncingTarget = EditorGUILayout.Toggle("전이 여부", _skill.bouncingTarget);
                if (_skill.bouncingTarget) _skill.bouncingTargetCount = EditorGUILayout.IntField("전이 수", _skill.bouncingTargetCount);
                _skill.penetrationTarget = EditorGUILayout.Toggle("기본 관통 여부", _skill.penetrationTarget);
                if (_skill.penetrationTarget) _skill.penetrationTargetCount = EditorGUILayout.IntField("관통 수", _skill.penetrationTargetCount);
                _skill.monsterControl = (MonsterState)EditorGUILayout.EnumPopup("특수 상호작용", _skill.monsterControl);
                if (_skill.monsterControl != MonsterState.None) _skill.monsterControlTime = EditorGUILayout.FloatField("작용시간", _skill.monsterControlTime);
                _skill.pullTarget = EditorGUILayout.Toggle("몬스터흡수", _skill.pullTarget);
                _skill.slowTarget = EditorGUILayout.Toggle("몬스터슬로우", _skill.slowTarget);
            }
            else
            {
                _skill.ScriptName = SkillDataName.PlayerSelfSkill;

                serializedObject.Update();
                _staffSelfSkill.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }


        }

        EditorUtility.SetDirty(_skill);
        //DrawDefaultInspector();
    }
}
