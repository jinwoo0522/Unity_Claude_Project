using UnityEngine;

public interface ISkill
{
    PooledHandler Handler { get; set; }

    void Active();
    void Release();
    void Destroy();
    
    // 스킬이 스스로 풀에 반납 (수명 종료 판단은 스킬 내부/서버 로직에서)
    public void ReturnToPool()
    {
        Handler.Return(this);
    }

}
