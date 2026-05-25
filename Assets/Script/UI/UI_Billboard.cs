using UnityEngine;

public class UI_Billboard : MonoBehaviour
{
    Camera mainCam;
    void Start()
    {
        mainCam = GameManager.Instance.cameraManager?.Get_Camera(CameraManager.CameraTag.MAIN);

        if(mainCam == null)
            GameManager.Instance.DebugMessage<UI_Billboard>("카메라가 Null");
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = mainCam.transform.rotation;
    }
}
