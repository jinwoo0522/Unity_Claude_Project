// 풀링 대상 공통 인터페이스 — 스킬/이펙트 등 풀에서 관리되는 오브젝트가 구현
public interface IPoolable
{
    IPoolReturner Handler { get; set; }

    void Active();
    void Release();
    void Destroy();

    // 스스로 풀에 반납 (수명 종료 판단은 각 구현부에서)
    public void ReturnToPool()
    {
        Handler.Return(this);
    }
}
