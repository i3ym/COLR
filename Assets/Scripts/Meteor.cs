using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Meteor : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 10f);
    }

    IEnumerator SpawnBulletNextFixedFrameCoroutine()
    {
        yield return new WaitForFixedUpdate();
        Game.game.SpawnMeteor();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") Game.game.GameOver();
        else
        {
            if (collision.tag != "Bullet") StartCoroutine(SpawnBulletNextFixedFrameCoroutine());
            Game.game.DestroyObject(collision.transform);
        }
    }
    void OnDestroy()
    {
        Game.Bullets.Remove(GetComponent<Rigidbody2D>());
    }
}