using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBulletEffect : MonoBehaviour
{
    [SerializeField] private GameObject _effect;

    private void OnDestroy()
    {
       var obj = Instantiate(_effect);
        obj.transform.position = transform.position;
    }
}
