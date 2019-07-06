using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    public new Rigidbody2D rigidbody;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        Destroy(10);
    }

    IEnumerator SpawnMeteorNextFixedFrameCoroutine()
    {
        yield return new WaitForFixedUpdate();
        Game.game.SpawnMeteor();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) Game.game.GameOver();
        else
        {
            collision.gameObject.GetComponent<Meteor>().Destroy(0);
            if (collision.gameObject && collision.CompareTag("Bullet")) StartCoroutine(SpawnMeteorNextFixedFrameCoroutine());
        }
    }

    public async void Destroy(int waitTimeSec)
    {
        if (waitTimeSec != 0) await Task.Delay(waitTimeSec * 1000);

        if (!this) return;

        Game.game.PlayDeathParticle(transform.position);
        Game.game.Movables.Remove(rigidbody);
        Game.game.MeteorPool.Enqueue(gameObject);

        gameObject.SetActive(false);
    }
}