using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class UPG_Basic : ShootClass
{
    public Transform turretTransform; // Assign in Inspector (Child Object)
    public float recoilAmount = 0.2f; // How much the turret moves backward
    public float recoilDuration = 0.25f; // How long it takes to return

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
            ShootServerRpc();  // Request the server to spawn the bullet
            bulletTimer = 0;
            canShoot = false;

            // Trigger recoil effect
            StartCoroutine(RecoilEffect());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ShootServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Spawning bullet");

        // Instantiate the bullet on the server
        GameObject bullet = Instantiate(bulletPrefab, transform.position + (transform.right / 2f), Quaternion.identity);

        // Spawn the bullet across the network (important to make it visible to all clients)
        NetworkObject networkObject = bullet.GetComponent<NetworkObject>();
        networkObject.Spawn();

        // Initialize the bullet with proper properties
        bullet.GetComponent<Bullet>().Initialise(true, bulletDamage);

        // Bullet behavior and settings
        bullet.GetComponent<Rigidbody2D>().linearVelocity = transform.right * bulletSpeed;
        bullet.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
        bullet.transform.localScale = new Vector3(bulletSize, bulletSize, bulletSize);
    }

    private IEnumerator RecoilEffect()
    {
        if (turretTransform == null) yield break; // Prevent errors if not assigned

        Vector3 originalPosition = turretTransform.localPosition;

        // Determine the recoil direction based on player facing
        float direction = transform.localScale.x > 0 ? -1f : 1f; // If facing right, move left. If facing left, move right.
        Vector3 recoilPosition = originalPosition + (Vector3.right * direction * recoilAmount);

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
