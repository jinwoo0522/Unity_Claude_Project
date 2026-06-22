using Unity.Netcode;
using UnityEngine;

public class PlayerCameraRotate : NetworkBehaviour
{
    Camera _mainCam;
    NetworkVariable<float> fCameraYaw = new NetworkVariable<float>
    (0f
    , NetworkVariableReadPermission.Everyone 
    , NetworkVariableWritePermission.Owner);
    public override void OnNetworkSpawn()
    {
        if(IsOwner == false) return;
        
        _mainCam = GameManager.Instance.cameraManager.Get_Camera(CameraManager.CameraTag.MAIN);
    }

    void Update()
    {
        if(IsOwner == true)
        {
            SyncCameraYaw();
        }

        if(IsServer == true)
        {
            RotateWithCamera();
        }
    }
    private void SyncCameraYaw()
    {
        if(_mainCam == null)
            Debug.LogWarning("카메라 NULL");

        fCameraYaw.Value = _mainCam.transform.eulerAngles.y;
    }
    private void RotateWithCamera()
    {
        transform.rotation = Quaternion.Euler(0f, fCameraYaw.Value , 0f);
    }
    
}
