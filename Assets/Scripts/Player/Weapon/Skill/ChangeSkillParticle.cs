using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSkillParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;

    public bool changeSize;
    public bool changeShapeSize;
    public bool changeColliderSize;
    public bool changeScaleSize;

    private float returnFloat;
    private Vector3 returnVector;

    public void ChangeRange(float value)
    {
        print("스킬 크기 변경 요청 : " + value);

        if (changeSize) ChangeSize(value);
        if (changeShapeSize) ChangeShapeSize(value);
        if (changeColliderSize) ChangeColliderSize(value);
      //  if (changeScaleSize) ChangeScaleSize(value);
    }

   private void ChangeSize(float value)
    {
        var main = _particle.main;
        returnFloat = main.startSizeMultiplier;

        main.startSizeMultiplier += main.startSizeMultiplier * value * 0.01f;
    }

    private void ChangeShapeSize(float value)
    {
        var main = _particle.shape;

        switch (main.shapeType)
        {
            case ParticleSystemShapeType.Sphere:
            case ParticleSystemShapeType.Circle:
                returnFloat = main.radius;
                main.radius += main.radius * value * 0.01f;
                break;
            case ParticleSystemShapeType.Box:
                returnVector = main.scale;
                main.scale += main.scale * value * 0.01f;
                break;
            default:
                break;
        }
    }

    private void ChangeColliderSize(float value)
    {
       if(TryGetComponent(out CircleCollider2D _))
        {
            var collider = GetComponent<CircleCollider2D>();
            returnFloat = collider.radius;
            collider.radius += collider.radius * value * 0.01f;
        }
    }

    public void ChangeScaleSize(float value)
    {
        changeScaleSize = true;
        returnVector = transform.localScale;
        transform.localScale += transform.localScale * value * 0.01f;
    }

    public void ReturnObject()
    {
        if (changeSize)
        {
            if (returnFloat == 0) return;

            var main = _particle.main;

            main.startSizeMultiplier = returnFloat;           
        }
        else if (changeShapeSize)
        {

            var main = _particle.shape;

            switch (main.shapeType)
            {
                case ParticleSystemShapeType.Sphere:
                case ParticleSystemShapeType.Circle:
                    if (returnFloat == 0) break;
                    main.radius = returnFloat;
                    break;
                case ParticleSystemShapeType.Box:
                    if (returnVector == Vector3.zero) break;
                    main.scale = returnVector;
                    break;
                default:
                    break;
            }
        }
        else if (changeColliderSize)
        {
            if (returnFloat == 0) return; 
            if (TryGetComponent(out CircleCollider2D _))
            {
                var collider = GetComponent<CircleCollider2D>();
                collider.radius = returnFloat;
            }
        }
        else
        {
            if (returnVector == Vector3.zero) return;

                returnVector = transform.localScale;
            transform.localScale = returnVector;
        }
    }

    private void OnDisable()
    {
      
        ReturnObject();
    }
}
