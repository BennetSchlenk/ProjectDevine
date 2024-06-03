using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public abstract class Reusable : MonoBehaviour
{
    public UnityEvent OnGetEvent;
    public UnityEvent OnBeforeReturn;
    public bool IsUsed { get; set; }

    private ObjectPool<Reusable> pool;

    public void OnGet()
    {
        OnGetEvent.Invoke();
    }

    public void Return()
    {
        OnBeforeReturn.Invoke();
        pool.Release(this);
    }

    public void SetPool(ObjectPool<Reusable> _pool)
    {
        pool = _pool;
    }
}