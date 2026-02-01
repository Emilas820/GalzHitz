using UnityEngine;
using Unity.Cinemachine; // 시네머신 기능을 쓰기 위해 필수!

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("시네머신 설정")]
    // 인스펙터에서 PlayerCam(가상 카메라)을 드래그해서 넣어주세요.
    public CinemachineCamera virtualCamera; 
    
    // 게임 시작 시 기본 타겟(플레이어)을 기억하기 위함
    public Transform playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 카메라가 따라갈 대상을 바꾸는 핵심 함수
    public void SetTarget(Transform newTarget)
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = newTarget;
            Debug.Log($"카메라 타겟 변경: {newTarget.name}");
        }
    }

    // 다시 플레이어에게로 카메라를 돌리는 함수
    public void ResetToPlayer()
    {
        SetTarget(playerTransform);
    }
}