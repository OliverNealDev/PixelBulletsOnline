using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class ShootClass : NetworkBehaviour
{
    public GameObject bulletPrefab;

    public float bulletCooldown;
    public float bulletTimer;
    public bool canShoot;
    public float bulletSpeed;
    public float bulletDamage;

    public float bulletSize;

    public float maxHealth;
    public float currentHealth;

    public float turretRecoilAmount; // How much the turret moves backward
    public float playerRecoilAmount;
}
