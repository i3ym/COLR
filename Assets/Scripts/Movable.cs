using System.Collections;
using UnityEngine;

public abstract class Movable : MonoBehaviour
{
    [HideInInspector]
    public new Rigidbody2D rigidbody;

    void Start() => rigidbody = GetComponent<Rigidbody2D>();
   
    void OnEnable() => StartCoroutine(DestroyCoroutine(10));

    protected abstract void OnTriggerEnter2D(Collider2D collision);

    protected IEnumerator DestroyCoroutine(int waitTimeSec)
    {
        if (waitTimeSec != 0) yield return new WaitForSeconds(waitTimeSec);
        Death();
    }
    public virtual void Death()
    {
        StopAllCoroutines();

        Game.game.Movables.Remove(rigidbody);

        gameObject.SetActive(false);
    }
}