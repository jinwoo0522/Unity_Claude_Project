using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager
{
    public enum CameraTag { MAIN };
    public enum CinemachineTag { PLAYER };
    private Dictionary<CameraTag , Camera> Cameras = new Dictionary<CameraTag , Camera>();
    private Dictionary<CinemachineTag , CinemachineCamera> Cinemachines = new Dictionary<CinemachineTag , CinemachineCamera>();
    public void Push_Camera(CameraTag strCameraName , Camera camera)
    {
        Cameras.Add(strCameraName , camera);
    }
    public Camera Get_Camera(CameraTag strCameraName)
    {
        Camera camera;

        if(Cameras.TryGetValue(strCameraName , out camera))
            return camera;

        Debug.Log("해당 키값을 가진 카메라가 없음");
        return null;
    }

    public void Remove_Camera(CameraTag strCameraName)
    {
        Cameras.Remove(strCameraName);
    }

    public void Push_Cinemachine(CinemachineTag strCinemachineName , CinemachineCamera camera)
    {
        Cinemachines.Add(strCinemachineName , camera);
    }
    public CinemachineCamera Get_Cinemachine(CinemachineTag strCinemachineName)
    {
        CinemachineCamera cinemachine;

        if(Cinemachines.TryGetValue(strCinemachineName , out cinemachine))
            return cinemachine;

        Debug.Log("해당 키값을 가진 카메라가 없음");
        return null;
    }

    public void Remove_Cinemachine(CinemachineTag strCameraName)
    {
        Cinemachines.Remove(strCameraName);
    }
}
