using UnityEngine;

// lets UI buttons and events request a scene change through the TransitionHandler
public class SceneLoader : MonoBehaviour
{
    public void Load(string sceneName) => TransitionHandler.Instance.LoadScene(sceneName);
}
