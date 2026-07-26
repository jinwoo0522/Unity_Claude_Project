using Unity.Netcode;
using UnityEngine;

// 캐릭터 전신의 머티리얼을 일괄 교체/복구한다 (빙결 등 상태 표현용)
// 상태 판정은 서버에서 하지만 렌더링은 각 피어에서 이뤄지므로 ClientRpc로 전파한다
public class MaterialChanger : NetworkBehaviour
{
    public enum MAT_TAG
    {
        FROZEN,
    }

    [SerializeField] private Material[] _changeMats;   // MAT_TAG 순서대로 인스펙터에 등록

    private SkinnedMeshRenderer[] _renderers;
    private Material[][] _originMats;   // 렌더러별 원본 머티리얼 배열

    public override void OnNetworkSpawn()
    {
        // 캐릭터가 여러 조각으로 나뉘어 있어 자식 렌더러를 전부 잡아야 한다 (골렘 5조각)
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        _originMats = new Material[_renderers.Length][];

        for (int i = 0; i < _renderers.Length; ++i)
            _originMats[i] = _renderers[i].sharedMaterials;
    }

    public void Change(MAT_TAG tag) => Change_ClientRpc((int)tag);

    public void Restore() => Restore_ClientRpc();

    // 서버는 화면을 그리지 않으므로 직접 적용 없이 전파만 한다
    [ClientRpc]
    private void Change_ClientRpc(int iMatNumber)
    {
        Material changeMat = _changeMats[iMatNumber];

        for (int i = 0; i < _renderers.Length; ++i)
        {
            // 슬롯 수만큼 채우지 않으면 해당 파츠가 원래 재질로 남는다 (엘프 body는 2슬롯)
            Material[] mats = new Material[_originMats[i].Length];
            for (int j = 0; j < mats.Length; ++j)
                mats[j] = changeMat;

            _renderers[i].sharedMaterials = mats;
        }
    }

    [ClientRpc]
    private void Restore_ClientRpc()
    {
        for (int i = 0; i < _renderers.Length; ++i)
            _renderers[i].sharedMaterials = _originMats[i];
    }
}
