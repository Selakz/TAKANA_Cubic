using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Switch scene and pass information through scenes.<para/>
/// It's considered that the animation it used only needs one trigger from start state to end.<para/>
/// </summary>
public class SceneLoader : MonoBehaviour
{
    // Serializable and Public

    // Private
    private static SceneLoader instance = null;
    private bool isLoadStart = false;
    private PackagedAnimator sceneSwitchAnimator;
    private AsyncOperation asyncLoad;

    // Static

    // Defined Function

    /// <summary>
    /// If there is a SceneLoader existing, it won't load new scenes.
    /// </summary>
    public static void LoadScene(string sceneName, string prefabName, string triggerName = "SceneLoadDone")
    {
        if (instance == null)
        {
            GameObject gameObject = new() { name = "SceneLoader" };
            gameObject.AddComponent<Canvas>();
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<SceneLoader>();
            instance.sceneSwitchAnimator = new(prefabName, gameObject.transform, false);
            instance.sceneSwitchAnimator.Play();
            instance.StartCoroutine(instance.ILoadScene(sceneName, triggerName));
        }
    }

    IEnumerator ILoadScene(string sceneToLoad, string triggerParam)
    {
        isLoadStart = true;
        yield return new WaitForSeconds(0.5f);
        asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        sceneSwitchAnimator.SetParam(triggerParam);
    }

    void DestroyWhenOver()
    {
        // It requires that the scene switch prefab has DestroyAfterAnimEnd.
        if (sceneSwitchAnimator.IsOver && asyncLoad.isDone)
        {
            Destroy(gameObject);
            instance = null;
        }
    }

    // System Function
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (isLoadStart)
        {
            DestroyWhenOver();
        }
    }
}
