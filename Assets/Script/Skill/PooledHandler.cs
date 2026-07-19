using UnityEngine;
using UnityEngine.Pool;

// 순수 오브젝트 풀 — NGO를 모른다. IPoolable을 구현한 Component 프리팹(스킬/이펙트 등)을 풀링한다.
public class PooledHandler : IPoolReturner
{
    private readonly ObjectPool<IPoolable> _pool;
    private readonly GameObject _prefab;   // IPoolable을 구현한 Component 프리팹

    public PooledHandler(GameObject prefab, int iMinSize, int iMaxSize)
    {
        _prefab = prefab;
        _pool = new ObjectPool<IPoolable>(
            Create,
            OnGet,
            OnReturn,
            OnDestroy,
            false,
            iMinSize,
            iMaxSize);
    }

    private IPoolable Create()
    {
        GameObject obj = Object.Instantiate(_prefab);
        IPoolable poolable = obj.GetComponent<IPoolable>();   // 생성한 인스턴스에서 IPoolable 획득
        poolable.Handler = this;               // 스스로 반납할 수 있도록 핸들러 주입
        return poolable;
    }

    private void OnGet(IPoolable obj)     => obj.Active();
    private void OnReturn(IPoolable obj)  => obj.Release();
    private void OnDestroy(IPoolable obj) => obj.Destroy();

    public IPoolable Get() => _pool.Get();

    // IPoolReturner — 풀링 대상이 스스로를 반납
    public void Return(object obj) => _pool.Release((IPoolable)obj);

    public void Prewarm(int iCount)
    {
        var temp = new IPoolable[iCount];
        for (int i = 0; i < iCount; ++i)
            temp[i] = _pool.Get();        // 생성 + 활성화
        for (int i = 0; i < iCount; ++i)
            _pool.Release(temp[i]);       // 비활성화하고 풀에 반납
    }
}
