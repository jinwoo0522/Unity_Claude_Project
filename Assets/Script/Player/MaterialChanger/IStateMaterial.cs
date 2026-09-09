using UnityEngine;

// 상태별 머티리얼 연출 단위
// 원본 에셋을 공유하면 진행도를 바꿀 때 모든 캐릭터가 같이 변하므로, 캐릭터마다 자기 인스턴스를 들고 굴린다
public interface IStateMaterial
{
    MaterialChanger.MAT_TAG Tag { get; }    // 인스펙터 등록 순서와 무관하게 자신을 식별한다
    Material Material { get; }              // 캐릭터 전용 런타임 인스턴스

    void Init();                            // 원본 에셋을 복제해 인스턴스를 만든다
    void Enter();                           // 연출 시작값으로 초기화
    void Tick(float fTimeDelta);
    void Release();                         // 인스턴스 파괴 (수동 해제하지 않으면 누수된다)
}
