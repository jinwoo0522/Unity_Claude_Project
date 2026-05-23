using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private static GameManager instance;
    private CinemachineCamera PlayerCinemachine;
    private Camera PlayerCamera;

    public CameraManager cameraManager {get; private set;}
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = new GameManager();
            return instance;
        }
    }

    // Awake는 가장 먼저 실행되는 함수로,
    // 여기서 인스턴스에 대한 초기화를 진행해줍니다.
    private void Awake()
    {
        // 인스턴스가 비어있다면 할당해주고, 
        //해당 오브젝트를 씬 이동간 파괴하지 않게합니다.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // 인스턴스가 이미 할당돼있다면(2개 이상이라면) 파괴합니다.
        else
        {
            Destroy(gameObject);
        }

        if(cameraManager == null)
            cameraManager = new CameraManager();

    }

}
