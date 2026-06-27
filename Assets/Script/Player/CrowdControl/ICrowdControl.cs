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
    public void Apply(CCData data);
    public bool isFlag {get;}
    public void Tick(float fTimeDelta);
}
