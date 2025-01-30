using Unity.Netcode;
using UnityEngine;

public class ShootClass : NetworkBehaviour
{
    public GameObject bulletPrefab;

    public float bulletCooldown;
    public float bulletTimer;
    public bool canShoot;
    public float bulletSpeed;
    public float bulletDamage;

    public float bulletSize;
}
