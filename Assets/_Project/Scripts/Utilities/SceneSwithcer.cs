using UnityEngine;

public class SceneSwithcer : MonoBehaviour
{
    public void SwitchScene(string id)
    {
        SceneLoader.Instance.LoadScene(id);
    }
}