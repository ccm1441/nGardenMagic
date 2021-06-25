using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBullet : MonoBehaviour
{
    private Portal _portal;
   public float damage;

    public void SetPortal(Portal portal, float damage)
    {
        _portal = portal;
        this.damage = damage;
        Invoke("Disable", 2f);
    }

    private void OnDisable()
    {
        _portal.ReturnBullet(gameObject);   
    }

    private void Disable() => gameObject.SetActive(false);
}
