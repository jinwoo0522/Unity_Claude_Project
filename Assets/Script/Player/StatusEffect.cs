// 상태이상 추상 기반 — 순수 C# 클래스, MonoBehaviour/NetworkBehaviour 상속 없음
// 코루틴·네트워크는 owner(Player_Status)가 대신 처리하므로 이 클래스는 순수 데이터+로직만 담당
public abstract class StatusEffect
{
    protected readonly Player_Status owner;

    protected StatusEffect(Player_Status owner)
    {
        this.owner = owner;
    }

    // 시간 기반 효과용 훅 — 매 서버 Update마다 Player_Status가 호출
    public virtual void Tick(float dt) { }
}
