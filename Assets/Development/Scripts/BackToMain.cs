using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMain : MonoBehaviour
{
    public void SceneMove()
    {
        SceneManager.LoadScene("Main");
    }
}
