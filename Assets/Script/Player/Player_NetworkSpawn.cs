using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;

// 플레이어 스폰 시 카메라/머리 위 Canvas/클라이언트 이름 등 네트워크 초기 셋업 담당.
public class Player_NetworkSpawn : NetworkBehaviour
{
    [SerializeField] private Canvas     canvas;
    [SerializeField] private GameObject ClientText;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            UpdateName_ClientRpc($"client : { GetComponent<NetworkObject>().OwnerClientId }");

        if (!IsOwner) return;

        UICameraBind();

        // 본인 머리 위 Canvas는 owner 로컬에서만 숨김 — 타 클라이언트 인스턴스엔 영향 없음
        canvas.gameObject.SetActive(false);

        TextMeshProUGUI text = ClientText.GetComponent<TextMeshProUGUI>();
        text.text = $"client : { NetworkManager.Singleton.LocalClientId }";
    }

    void UICameraBind()
    {
        CinemachineCamera PlayerCamera
            = GameManager.Instance.cameraManager.Get_Cinemachine(CameraManager.CinemachineTag.PLAYER);

        Camera MainCamera
            = GameManager.Instance.cameraManager.Get_Camera(CameraManager.CameraTag.MAIN);

        canvas.worldCamera = MainCamera;
        PlayerCamera.Target.TrackingTarget = GetComponent<Transform>();
    }

    [ClientRpc]
    void UpdateName_ClientRpc(string strName)
    {
        TextMeshProUGUI text = ClientText.GetComponent<TextMeshProUGUI>();
        text.text = strName;
    }
}
