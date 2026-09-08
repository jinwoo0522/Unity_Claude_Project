using UnityEngine;

public interface ICrowdControl
{
    public struct CCData
    {
        public CCData(float fValue= 0f, float fDecay = 0f, Vector3 vDir = default)
        {
            _fValue = fValue;
            _fDecay = fDecay;
            _vDir = vDir;
        }
        public float   _fValue;
        public float   _fDecay;
        public Vector3 _vDir;
    }
    public bool isFlag {get;}
    public void Apply(CCData data);
    public bool IsExpired();   // 종료조건 판정만 수행 — 상태 변경 금지
    public void Restore();
    public void Tick(float fTimeDelta);
}
