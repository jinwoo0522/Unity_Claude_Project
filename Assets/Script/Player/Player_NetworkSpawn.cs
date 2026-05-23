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
             Debug.Log($"[확인] GameManager.Instance null? {GameManager.Instance == null}");
             
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

            Debug.Log($"{NetworkManager.Singleton.LocalClientId} : 카메라 연결 성공!");
            canvas.worldCamera = MainCamera;
            PlayerCamera.Target.TrackingTarget = GetComponent<Transform>();

            TextMeshProUGUI Text = ClientText.GetComponent<TextMeshProUGUI>();
            Text.text = $"client : { NetworkManager.Singleton.LocalClientId }"; 
        }
    

    }

    
    // Update is called once per frame
}
