using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// 캐릭터 전신의 머티리얼을 일괄 교체/복구한다 (빙결 등 상태 표현용)
// 상태 판정은 서버에서 하지만 렌더링은 각 피어에서 이뤄지므로 ClientRpc로 전파한다
public class MaterialChanger : NetworkBehaviour
{
    public enum MAT_TAG
    {
        FROZEN,
        DISSOLVE,
    }

    [SerializeReference, SubclassSelector] private IStateMaterial[] _changeMats;   // 등록 순서 무관, 각자 Tag로 식별한다

    private readonly Dictionary<MAT_TAG, IStateMaterial> _matTable = new();
    private IStateMaterial _currentMat;
    private bool _isChanged;

    private SkinnedMeshRenderer[] _renderers;
    private Material[][] _originMats;   // 렌더러별 원본 머티리얼 배열

    public override void OnNetworkSpawn()
    {
        // 캐릭터가 여러 조각으로 나뉘어 있어 자식 렌더러를 전부 잡아야 한다 (골렘 5조각)
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        _originMats = new Material[_renderers.Length][];

        for (int i = 0; i < _renderers.Length; ++i)
            _originMats[i] = _renderers[i].sharedMaterials;

        // 머티리얼 인스턴스는 캐릭터마다 하나씩 만든다 — 공유 에셋을 건드리지 않아야 개별 연출이 가능하다
        for (int i = 0; i < _changeMats.Length; ++i)
        {
            _changeMats[i].Init();
            _matTable.Add(_changeMats[i].Tag, _changeMats[i]);
        }
    }

    public override void OnNetworkDespawn()
    {
        _isChanged = false;

        for (int i = 0; i < _changeMats.Length; ++i)
            _changeMats[i].Release();

        _matTable.Clear();
    }

    public void Change(MAT_TAG tag)
    {
        if(IsSpawned == false) return;

        Change_ClientRpc((int)tag);
    }

    public void Restore()
    {
        if(IsSpawned == false) return;

        Restore_ClientRpc();
    }

    // 연출 진행은 각 피어가 로컬로 굴린다 — CC의 Update는 서버에서만 돌기 때문에 여기서 대신 돌린다
    private void Update()
    {
        if(_isChanged == false) return;

        _currentMat.Tick(Time.deltaTime);
    }

    // 서버는 화면을 그리지 않으므로 직접 적용 없이 전파만 한다
    [ClientRpc]
    private void Change_ClientRpc(int iMatNumber)
    {
        _currentMat = _matTable[(MAT_TAG)iMatNumber];
        _currentMat.Enter();
        _isChanged = true;

        for (int i = 0; i < _renderers.Length; ++i)
        {
            // 슬롯 수만큼 채우지 않으면 해당 파츠가 원래 재질로 남는다 (엘프 body는 2슬롯)
            // sharedMaterials로 넣어야 Unity가 렌더러마다 또 복제하지 않고 인스턴스 하나를 공유한다
            Material[] mats = new Material[_originMats[i].Length];
            for (int j = 0; j < mats.Length; ++j)
                mats[j] = _currentMat.Material;

            _renderers[i].sharedMaterials = mats;
        }
    }

    [ClientRpc]
    private void Restore_ClientRpc()
    {
        _isChanged = false;

        for (int i = 0; i < _renderers.Length; ++i)
            _renderers[i].sharedMaterials = _originMats[i];
    }
}
