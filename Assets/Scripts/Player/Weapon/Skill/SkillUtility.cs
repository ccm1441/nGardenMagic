using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUtility : MonoBehaviour
{
    protected SkillScriptable _skillInfo;
    protected Player _player;
    protected WeaponSlot _weaponSlot;
    protected GameObject _detectedEmeny;

    // 스킬별 공통 옵션
    public bool _coroutineStop;
    protected Vector3 _direction;
    protected bool _isTargetingSKill;
    protected bool _isBouncingSKill;
    protected int _bouncingCount;
    protected bool _isPenetartSkill;
    protected int _penetartCount;
    protected float _height, _width;

    private float propertyTime, _result;

    public virtual void Init(WeaponSlot weapon)
    {
      _player = GameManager.GetInstance().player;
       _weaponSlot = weapon;
        _skillInfo = weapon.skillInfo;
        print(weapon);
    }

    // 쿨타임 게산
    protected float CalculateCoolTime()
    {
        // 속성 쿨타임
        propertyTime = _player.GetPropertyCoolTime(_skillInfo.Type);

        // 퍼센트만 계산한 쿨타임
        _result = _skillInfo.AttackCoolTime - (_skillInfo.AttackCoolTime * (propertyTime + _weaponSlot.addStat.addCooltimePer) * 0.01f);

        // 초단위 계산한 쿨타임
        _result += _weaponSlot.addStat.addCooltimeSec;

     return _result;
    }

    // 사운드
    protected void PlaySound(string id)
    {
        var _game = GameManager.GetInstance();
        var sound = 0;

        if (id == "_skill_FireBall") sound = _game._skill_FireBall;
        else if (id == "_skill_FireStrike") sound = _game._skill_FireStrike;
        else if (id == "_skill_LightingBolt") sound = _game._skill_LightingBolt;
        else if (id == "_skill_LightingThunder") sound = _game._skill_LightingThunder;
        else if (id == "_skill_LightingStorm") sound = _game._skill_LightingStorm;
        else if (id == "_skill_Whomhole") sound = _game._skill_Whomhole;
        else if (id == "_skill_WindStorm") sound = _game._skill_WindStorm;
        else if (id == "_skill_EarthBoom") sound = _game._skill_EarthBoom;
        else if (id == "_skill_EarthEarthquake") sound = _game._skill_EarthEarthquake;
        else if (id == "_skill_EarthMissile") sound = _game._skill_EarthMissile;
        else if (id == "_skill_EarthWall") sound = _game._skill_EarthWall;
        else if (id == "_skill_FireStep") sound = _game._skill_FireStep;
        else if (id == "_skill_WaterBlast") sound = _game._skill_WaterBlast;
        else if (id == "_skill_WaterField") sound = _game._skill_WaterField;
        else if (id == "_skill_WindCutter") sound = _game._skill_WindCutter;

        _game.PlayerEffectSound(sound, 0.5f);
    }


    // 카메라 범위 가져오기
    protected void GetCameraRange()
    {
        _height = 2 * GameManager.GetInstance().camera.orthographicSize;
        _width = _height * GameManager.GetInstance().camera.aspect;
    }
}
