using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float bulletDamage;
    public bool isPlayerBullet;

    void Start()
    {
        Invoke("Timeout", 5f);
    }

    void Timeout()
    {
        Destroy(gameObject);
    }
}
