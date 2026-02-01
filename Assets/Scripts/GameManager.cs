using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("연결")]
    public Player player; // ★ 인스펙터에서 Player 오브젝트를 드래그해서 넣어주세요!

    void Awake()
    {
        if(Instance == null) Instance = this;
    }

    // 조이스틱이 드래그 중일 때 계속 호출함
    public void UpdateInput(float angleRatio, float powerRatio)
    {
        if (player != null)
        {
            // 플레이어에게 "조준해!" 명령
            player.UpdateAim(angleRatio, powerRatio);
        }
    }

    // 조이스틱을 놓았을 때 호출함
    public void RequestThrow()
    {
        if (player != null)
        {
            // 플레이어에게 "던져!" 명령
            player.Throw();
        }
    }
}