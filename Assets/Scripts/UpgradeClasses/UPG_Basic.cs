using Unity.Netcode;
using UnityEngine;

public class UPG_Basic : ShootClass
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        
        bulletTimer += Time.deltaTime;
        if (bulletTimer >= bulletCooldown && !canShoot)
        {
            canShoot = true;
        }

        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (canShoot)
        {
            bulletTimer = 0;
            canShoot = false;
            
            Debug.Log("spawningbullet");
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().linearVelocity = transform.right * bulletSpeed;
            bullet.GetComponent<Rigidbody2D>().linearVelocity += GetComponent<Rigidbody2D>().linearVelocity;
            bullet.GetComponent<Bullet>().bulletDamage = bulletDamage;
            bullet.GetComponent<Bullet>().isPlayerBullet = true;
            bullet.transform.localScale = new Vector3(bulletSize, bulletSize, bulletSize);
        }
    }
}
