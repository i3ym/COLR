using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    void Start()
    {
        Destroy(this);
        SceneManager.LoadScene(1);
    }
}