using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
public class Player_NetworkSpawn : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private GameObject ClientText;

    public override void OnNetworkSpawn()
    {

        if(IsOwner == true)
        {
             CinemachineCamera PlayerCamera 
                = GameManager.Instance.cameraManager?.Get_Cinemachine(CameraManager.CinemachineTag.PLAYER);

            if(PlayerCamera == null)
            {
                Debug.Log($"{NetworkManager.Singleton.LocalClientId} : 플레이어 시네머신 연결 실패!");
                return;
            }

            Camera MainCamera = GameManager.Instance.cameraManager?.Get_Camera(CameraManager.CameraTag.MAIN);

            if(MainCamera == null)
            {
                Debug.Log($"{NetworkManager.Singleton.LocalClientId} : 메인 카메라 연결 실패!");
                return;
            }

            canvas.worldCamera = MainCamera;
            PlayerCamera.Target.TrackingTarget = GetComponent<Transform>();

            // 본인 머리 위 Canvas는 owner 로컬에서만 숨김 — 타 클라이언트 인스턴스엔 영향 없음
            canvas.gameObject.SetActive(false);

            // 화면 HUD 탐색 후 Stat에 바인딩 — 이후 HP·마나 변경이 좌측 하단 바에 자동 반영
            PlayerHUD hud = FindAnyObjectByType<PlayerHUD>(FindObjectsInactive.Include);
            GetComponent<Stat>().BindOwnerHUD(hud);

            TextMeshProUGUI Text = ClientText.GetComponent<TextMeshProUGUI>();
            Text.text = $"client : { NetworkManager.Singleton.LocalClientId }";
        }

        if(IsServer == true)
        {
            UpdateName_ClientRpc($"client : { GetComponent<NetworkObject>().OwnerClientId }");
        }
    }

    [ClientRpc]
    void UpdateName_ClientRpc(string strName)
    {
        TextMeshProUGUI Text = ClientText.GetComponent<TextMeshProUGUI>();
        Text.text = strName;
    }

    // Update is called once per frame
}
