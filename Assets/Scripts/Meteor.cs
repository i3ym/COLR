using System.Collections;
using UnityEngine;

public class Meteor : Movable
{
    IEnumerator SpawnMeteorNextFixedFrameCoroutine()
    {
        yield return new WaitForFixedUpdate();
        Game.game.SpawnMeteor();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeSelf || !collision.gameObject.activeSelf) return;

        if (collision.CompareTag("Player")) Game.game.GameOver();
        else if (collision.CompareTag("Meteor"))
        {
            collision.gameObject.GetComponent<Meteor>().Death();
            Death();
            Game.game.SpawnMeteorNextFrame();
        }
    }

    public override void Death()
    {
        base.Death();
        Game.game.PlayDeathParticle(transform.position);
    }
}