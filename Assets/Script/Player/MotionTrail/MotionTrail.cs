using UnityEngine;

public class MotionTrail : MonoBehaviour, IPoolable
{
    public IPoolReturner Handler { get; set; }
    [SerializeField] private MeshFilter _mf;
    [SerializeField] private MeshRenderer _mr;
    private Mesh _bakedMesh;
    private float fTrailTime;
    private float fAccTime;
    private Color vBaseColor;

    void Awake()
    {
        _bakedMesh = new();
    }


    public void Play(Vector3 vPosition, Quaternion qRotation, SkinnedMeshRenderer smr,
     Color vColor, float fTime = 0.5f)
    {
        smr.BakeMesh(_bakedMesh);
        _mf.mesh = _bakedMesh;

        fTrailTime = fTime;
        fAccTime = 0f;
        vBaseColor = vColor;
        _mr.material.color = vColor;
        transform.SetPositionAndRotation(vPosition, qRotation);
    }

    private void Update()
    {
        // 수명이 0이면 즉시 반납
        if (fTrailTime <= 0f)
        {
            Handler.Return(this);
            return;
        }

        // 경과 시간만큼 알파를 서서히 낮춘다
        fAccTime += Time.deltaTime;
        float fRatio = 1f - fAccTime / fTrailTime;

        Color vColor = vBaseColor;
        vColor.a = vBaseColor.a * fRatio;
        _mr.material.color = vColor;

        // 알파가 0이 되면 풀에 반납
        if (fRatio <= 0f)
            Handler.Return(this);
    }
    public void Active()
    {
        gameObject.SetActive(true);
    }

    public void Release()
    {
        gameObject.SetActive(false);
    }

    public void Destroy() => GameObject.Destroy(gameObject);
}
