using UnityEngine;

// 디졸브 표현 — Height를 0에서 1로 올려 몸이 서서히 사라지는 연출
[System.Serializable]
public class DissolveMaterial : IStateMaterial
{
    // 문자열 조회는 매 프레임 낭비이므로 ID를 캐싱한다
    private static readonly int _iHeightID = Shader.PropertyToID("_DissolveHeight");

    [SerializeField] private Material _sourceMat;                   // 프로젝트 원본 에셋 (직접 수정하지 않는다)
    [SerializeField, Min(0.01f)] private float _fDuration = 1.5f;   // 0에서 1까지 도달하는 시간(초), 0이면 나눗셈이 깨져 하한을 둔다

    private Material _instanceMat;
    private float _fHeight;

    public MaterialChanger.MAT_TAG Tag => MaterialChanger.MAT_TAG.DISSOLVE;
    public Material Material => _instanceMat;

    public void Init()
    {
        _instanceMat = new Material(_sourceMat);
    }

    // 재사용 시 이전 진행도가 남아 있으면 이미 사라진 상태로 보이므로 매번 되돌린다
    public void Enter()
    {
        _fHeight = 0f;
        _instanceMat.SetFloat(_iHeightID, _fHeight);
    }

    public void Tick(float fTimeDelta)
    {
        if(_fHeight >= 1f) return;

        _fHeight = Mathf.Min(_fHeight + fTimeDelta / _fDuration, 1f);
        _instanceMat.SetFloat(_iHeightID, _fHeight);
    }

    public void Release()
    {
        Object.Destroy(_instanceMat);
    }
}
