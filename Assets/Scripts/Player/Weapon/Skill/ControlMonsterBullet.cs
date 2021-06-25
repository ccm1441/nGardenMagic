using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlMonsterBullet : MonoBehaviour
{
    [SerializeField] private MonsterState _state;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
         //   collision.GetComponent<MonsterBase>().SetState(_state);
        }
    }
}
