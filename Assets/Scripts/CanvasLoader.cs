using UnityEngine;

public class CanvasLoader : MonoBehaviour
{
    void Start() => GetComponent<Canvas>().worldCamera = Camera.main;
}