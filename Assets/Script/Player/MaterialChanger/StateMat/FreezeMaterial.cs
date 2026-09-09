using UnityEngine;


[System.Serializable]
public class FreezeMaterial : IStateMaterial
{
    [SerializeField] private Material _sourceMat;   // 프로젝트 원본 에셋 (직접 수정하지 않는다)

    private Material _instanceMat;
    public MaterialChanger.MAT_TAG Tag => MaterialChanger.MAT_TAG.FROZEN;
    public Material Material => _instanceMat;

    public void Init()
    {
        _instanceMat = new Material(_sourceMat);
    }

    public void Enter()
    {
    }

    public void Tick(float fTimeDelta)
    {
    }

    public void Release()
    {
        Object.Destroy(_instanceMat);
    }
}
