// 회전 소스(조준 입력·추적 대상 등)는 구현체가 각자 가지고, 외부에는 회전 실행만 노출한다
// 호출 시점은 상태가 정한다 — 회전이 허용되는 상태만 Rotate를 부른다
public interface IEntityRotate
{
    public void Rotate();
}
