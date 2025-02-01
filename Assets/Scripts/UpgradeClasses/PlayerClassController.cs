using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode.Components;
using UnityEngine.Serialization;

public class PlayerClassController : ShootClass
{
    public GameObject DefaultPrefab;
    public GameObject TwinsPrefab;
    
    public Transform turretTransform;
    public float recoilDuration = 0.25f; // How long it takes to return

    private GameObject spawnedBullet;
    
    private Rigidbody2D rb;
    
    public enum Upgrades {Default, Twins, Knight}
    public Upgrades currentUpgrade;

    private Vector2 bulletSpawnPosition;

    void Start()
    {
        currentHealth = maxHealth;
        
        rb = GetComponent<Rigidbody2D>();
        
        switch (currentUpgrade)
        {
            case Upgrades.Default:
                GameObject defaultPrefab = Instantiate(DefaultPrefab, transform.position, Quaternion.identity);
                defaultPrefab.transform.SetParent(transform);
                break;
            case Upgrades.Twins:
                GameObject twinsPrefab = Instantiate(TwinsPrefab, transform.position, Quaternion.identity);
                twinsPrefab.transform.SetParent(transform);
                break;
            case Upgrades.Knight:
                
                break;
        }
    }
    
    void Update()
    {
        if (!IsOwner) return;

        bulletTimer += Time.deltaTime;
        if (bulletTimer >= bulletCooldown && !canShoot)
        {
            canShoot = true;
        }

        // Only trigger shooting if the player presses the fire button
        if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && canShoot)
        {
            bulletTimer = 0;
            canShoot = false;

            switch (currentUpgrade)
            {
                case Upgrades.Default:
                    UPG_Default_Shoot();
                    StartCoroutine(RecoilChildren());
                    break;
                case Upgrades.Twins:
                    UPG_Twins_Shoot();
                    StartCoroutine(RecoilChildren());
                    break;
                case Upgrades.Knight:
                    UPG_Default_Shoot();
                    StartCoroutine(RecoilChildren());
                    break;
            }
        }
    }
    void UPG_Default_Shoot()
    {
        // Instantiate the bullet on the server
        GameObject bullet = Instantiate(bulletPrefab, transform.position + (transform.right / 2f), Quaternion.identity);

        // Spawn the bullet across the network (important to make it visible to all clients)
        NetworkObject networkObject = bullet.GetComponent<NetworkObject>();
        networkObject.Spawn();

        // Initialize the bullet with proper properties
        bullet.GetComponent<Bullet>().Initialise(true, bulletDamage, gameObject);
        

        // Bullet behavior and settings
        networkObject.GetComponent<Rigidbody2D>().linearVelocity = transform.right * bulletSpeed; 
        bullet.transform.localScale = new Vector3(bulletSize, bulletSize, bulletSize);
    }
    
    async void UPG_Twins_Shoot()
    {
        for (int i = 0; i < 2; i++)
        {
            if (i == 0)
            {
                bulletSpawnPosition = transform.position + (transform.right / 2f) + (transform.up / 3.5f);
                //bulletSpawnPosition += (transform.up / 4f);
                //bulletSpawnPosition += new Vector2(0, 0.28f);
            }
            else
            {
                bulletSpawnPosition = transform.position + (transform.right / 2f) + (-transform.up / 3.5f);
                //bulletSpawnPosition += (transform.up / 4f);
                //bulletSpawnPosition += new Vector2(0, -0.28f);
            }
            
            // Instantiate the bullet on the server
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPosition, Quaternion.identity);

            // Spawn the bullet across the network (important to make it visible to all clients)
            NetworkObject networkObject = bullet.GetComponent<NetworkObject>();
            networkObject.Spawn();

            // Initialize the bullet with proper properties
            bullet.GetComponent<Bullet>().Initialise(true, bulletDamage, gameObject);
        

            // Bullet behavior and settings
            networkObject.GetComponent<Rigidbody2D>().linearVelocity = transform.right * bulletSpeed; 
            bullet.transform.localScale = new Vector3(bulletSize, bulletSize, bulletSize);

            if (i == 0) await Task.Delay(Mathf.RoundToInt(recoilDuration * 1000));
        }
    }
    
    private IEnumerator RecoilChildren()
    {
        for (int i = 0; i < transform.GetChild(0).transform.childCount; i++)
        {
            turretTransform = transform.GetChild(0).transform.GetChild(i).transform;

            Vector3 originalPosition = turretTransform.localPosition;

            // Determine the recoil direction based on player facing
            float direction = transform.localScale.x > 0 ? -1f : 1f; // If facing right, move left. If facing left, move right.
            rb.AddForce(new Vector2(-transform.right.x * playerRecoilAmount, -transform.right.y * playerRecoilAmount), ForceMode2D.Impulse);
            Vector3 recoilPosition = originalPosition + (Vector3.right * direction * turretRecoilAmount);

            float elapsed = 0f;
            while (elapsed < recoilDuration * 0.5f) // Move back quickly
            {
                elapsed += Time.deltaTime;
                turretTransform.localPosition = Vector3.Lerp(originalPosition, recoilPosition, elapsed / (recoilDuration * 0.5f));
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < recoilDuration * 0.5f) // Move forward slowly
            {
                elapsed += Time.deltaTime;
                turretTransform.localPosition = Vector3.Lerp(recoilPosition, originalPosition, elapsed / (recoilDuration * 0.5f));
                yield return null;
            }

            turretTransform.localPosition = originalPosition; // Ensure exact reset
        }
    }
}
