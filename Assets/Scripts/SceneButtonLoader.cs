using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonLoader : MonoBehaviour
{
    // Nomes das cenas atribuídos via Inspector
    public string sceneNameButton1;
    public string sceneNameButton2;
    public string sceneNameButton3;

    // Métodos públicos que podem ser conectados aos botões
    public void LoadScene1()
    {
        SceneManager.LoadScene(sceneNameButton1);
    }

    public void LoadScene2()
    {
        SceneManager.LoadScene(sceneNameButton2);
    }

    public void LoadScene3()
    {
        SceneManager.LoadScene(sceneNameButton3);
    }
}
