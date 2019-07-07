using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    public new Rigidbody2D rigidbody;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        StartCoroutine(DestroyCoroutine(10));
    }

    IEnumerator SpawnMeteorNextFixedFrameCoroutine()
    {
        yield return new WaitForFixedUpdate();
        Game.game.SpawnMeteor();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) Game.game.GameOver();
        else if (collision.gameObject)
        {
            collision.gameObject.GetComponent<Meteor>().Destroy();
            if (collision.CompareTag("Bullet")) StartCoroutine(SpawnMeteorNextFixedFrameCoroutine());
        }
    }

    IEnumerator DestroyCoroutine(int waitTimeSec)
    {
        if (waitTimeSec != 0) yield return new WaitForSeconds(waitTimeSec);
        Destroy();
    }
    void Destroy()
    {
        StopAllCoroutines();

        Game.game.Movables.Remove(rigidbody);
        Game.game.PlayDeathParticle(transform.position);
        Game.game.MeteorPool.Enqueue(gameObject);

        gameObject.SetActive(false);
    }
}