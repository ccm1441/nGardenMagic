using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MonsterScriptable))]
public class MonsterCustom : Editor
{
    private MonsterScriptable _monster;

    public override void OnInspectorGUI()
    {
        _monster = (MonsterScriptable)target;

        GUIStyle style = new GUIStyle();
        style.richText = true;

        GUILayout.Label("<size=15><color=yellow>몬스터 정보</color></size>", style);
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.black);
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        _monster.MonsterPrefab = EditorGUILayout.ObjectField(_monster.MonsterPrefab, typeof(GameObject), false, GUILayout.Width(64), GUILayout.Height(64)) as GameObject;
            EditorGUILayout.BeginVertical();
                _monster.Name = EditorGUILayout.TextField("몬스터 이름", _monster.Name);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("출현 시간(분/초)");
                _monster.StartSpawnTime.x = EditorGUILayout.FloatField(_monster.StartSpawnTime.x);
                _monster.StartSpawnTime.y = EditorGUILayout.FloatField(_monster.StartSpawnTime.y);
                EditorGUILayout.EndHorizontal();
        _monster.AttackType = (MonsterAttackType)EditorGUILayout.EnumFlagsField("공격 방식", _monster.AttackType);
            EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        // 공통
        _monster.Damage = EditorGUILayout.FloatField("공격력", _monster.Damage);
        _monster.Speed = EditorGUILayout.FloatField("이동 속도", _monster.Speed);
        _monster.HP = EditorGUILayout.FloatField("체력", _monster.HP);
        _monster.EXP = EditorGUILayout.FloatField("경험치", _monster.EXP);
        _monster.color = EditorGUILayout.ColorField("몬스터 색상", _monster.color);
        _monster.spawnMap = (MapType)EditorGUILayout.EnumFlagsField("출현 맵", _monster.spawnMap);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("죽을때 소리");
        _monster.sound = EditorGUILayout.ObjectField(_monster.sound, typeof(AudioClip), false) as AudioClip;
        EditorGUILayout.EndHorizontal();

        // 충돌 + 원거리
        if ((int)_monster.AttackType == 3)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("총알 프리팹");
            _monster.BulletPrefab = EditorGUILayout.ObjectField(_monster.BulletPrefab, typeof(GameObject), false ) as GameObject;
            EditorGUILayout.EndHorizontal();

            _monster.AttackTime = EditorGUILayout.FloatField("공격 딜레이", _monster.AttackTime);
            _monster.AttackRange = EditorGUILayout.FloatField("공격 거리", _monster.AttackRange);
        }

        _monster.useHitReAction = EditorGUILayout.Toggle("맞았을 때 변경사항 사용", _monster.useHitReAction);
        if (_monster.useHitReAction)
        {
            _monster.reactionType = (MonsterHitSpecial)EditorGUILayout.EnumPopup("변경할 타입", _monster.reactionType);

            if (_monster.reactionType == MonsterHitSpecial.ChangeColor)
                _monster.reactionColor = EditorGUILayout.ColorField("변경할 색", _monster.reactionColor);
            else if (_monster.reactionType == MonsterHitSpecial.ChangeSpeed)
                _monster.reactionSpeed = EditorGUILayout.FloatField("변경할 속도", _monster.reactionSpeed);
        }

        // 몬스터 스폰 정보
        GUILayout.Space(15);
        GUILayout.Label("<size=15><color=yellow>리젠 옵션</color></size>", style);
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.black);
        GUILayout.Space(5);

        _monster.ReSpawnTime = EditorGUILayout.FloatField("리젠 속도(초)", _monster.ReSpawnTime);
        _monster.RespawnCount = EditorGUILayout.FloatField("리젠당 몬스터 수", _monster.RespawnCount);

        EditorUtility.SetDirty(_monster);
      //  DrawDefaultInspector();
    }
}
