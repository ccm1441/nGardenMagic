using UnityEngine;

public class DisableReturnObj : MonoBehaviour
{
    private float _timer;
   [SerializeField] private float _time;
    private bool _update;
    [SerializeField] private bool _monster;

    public void Init(float time)
    {
        _update = true;
        _time = time;
        _timer = 0;       
    }

    private void Update()
    {
        if (!_update) return;

        _timer += Time.deltaTime;

        if (_timer >= _time)
        {
            _update = false;
            if (_monster) GameManager.GetInstance().spawn.ReturnDieEffect(gameObject);
            else            GameManager.GetInstance().spawn.ReturnSkill(gameObject);          
        }
    }
}
