using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [SerializeField] private CinemachineCamera cinemachine;

    private Camera _mainCamera;
 
    private void Awake()
    {
        Instance = this;

        _mainCamera = Camera.main;
    }

    private void Start()
    {
        if (PlayerEntity.Instance != null)
            cinemachine.Target.TrackingTarget = PlayerEntity.Instance.transform;
    }

    /// Возвращает позицию курсора мыши в мировом пространстве
    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0;

        return mouseWorldPos;
    }

    public Vector2 GetMouseScreenPosition()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();

        return mouseScreenPos;
    }
}