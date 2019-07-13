using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class Movable : MonoBehaviour
{
    [HideInInspector]
    public RawImage RawImage;
    [HideInInspector]
    public new RectTransform transform;

    [HideInInspector]
    public Vector3 Direction;

    void Awake()
    {
        transform = gameObject.transform as RectTransform;
        RawImage = GetComponent<RawImage>();
    }

    public virtual void Death()
    {
        StopAllCoroutines();

        Game.game.Movables.Remove(this);
        gameObject.SetActive(false);
    }
}