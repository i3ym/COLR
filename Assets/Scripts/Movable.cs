using System.Collections;
using UnityEngine;

public abstract class Movable : MonoBehaviour
{
    [HideInInspector]
    public new Rigidbody2D rigidbody;

    void Start() => rigidbody = GetComponent<Rigidbody2D>();

    protected abstract void OnTriggerEnter2D(Collider2D collision);

    public virtual void Death()
    {
        StopAllCoroutines();

        Game.game.Movables.Remove(rigidbody);

        gameObject.SetActive(false);
    }
}