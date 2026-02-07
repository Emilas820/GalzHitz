using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartButton : MonoBehaviour
{
    // 인스펙터에서 따로 지정하고 싶을 때만 수동으로 입력하는 용도로 씁니다.
    public string overrideSceneName = "";

    public void OnClickStart()
    {
        // 싱글톤 및 데이터 체크
        if (GlobalDataManager.Instance == null)
        {
            Debug.LogError("GlobalDataManager 인스턴스를 찾을 수 없습니다!");
            return;
        }

        // 안전장치: 필수 데이터가 다 선택되었는지 확인
        if (GlobalDataManager.Instance.userCharacter == null || 
            GlobalDataManager.Instance.userBag == null || GlobalDataManager.Instance.currentStage == null)
        {
            Debug.LogError("캐릭터나 스테이지가 선택되지 않았습니다!");
            return;
        }

        string sceneToLoad = GlobalDataManager.Instance.currentStage.sceneName;
        // 3. 만약 코드에서 강제로 지정한 이름(overrideSceneName)이 있다면 그걸 우선 사용
        if (!string.IsNullOrEmpty(overrideSceneName))
        {
            sceneToLoad = overrideSceneName;
        }

        // 4. 최종 검증 후 로드
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"{sceneToLoad} 씬으로 이동합니다.");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("StageData에 Scene Name이 비어있습니다!");
        }
    }
}