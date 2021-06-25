using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRotate : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _radius;

    private float _currentDistance;
    private float _addDistance;
    private Vector3 position;
    private float test;

    private void Start()
    {
        position = Vector3.zero;
    }

    private void Update()
    {
         transform.Rotate(new Vector3(0,1,0) * _speed);
      //  transform.rotation = Quaternion.Euler(new Vector3(0, test, 0));
      //  test += Time.deltaTime * _speed;
    }

    // 넣을때마다 360 나누어서 간격 조정해주기
    public void ResetCircle(int index)
    {
        // 간격 계산
        _currentDistance = 0;
        _addDistance = 360 / index;      

        // 각격 적용
        for (int i = 0; i < index; i++)
        {
            var obj = transform.GetChild(i);

            obj.gameObject.SetActive(true);
            position.x = _radius * Mathf.Cos(_currentDistance * Mathf.Deg2Rad) ;
            position.y = 0;
            position.z = _radius * Mathf.Sin(_currentDistance  * Mathf.Deg2Rad);
                  
            obj.localPosition = position;
            _currentDistance += _addDistance;
        }
    }
}
