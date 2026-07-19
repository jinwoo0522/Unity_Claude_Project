// 풀링 대상 공통 인터페이스 — 스킬/이펙트 등 풀에서 관리되는 오브젝트가 구현
public interface IPoolable
{
    IPoolReturner Handler { get; set; }

    void Active();
    void Release();
    void Destroy();

    public void ReturnToPool()
    {
        Handler.Return(this);
    }
}
