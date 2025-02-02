using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerClassController : ShootClass
{
    public GameObject bulletPrefab;
    public bool canShoot;
    public float bulletTimer;
    public float currentHealth;
    
    // Stats
    public float movementSpeed;
    public float bulletCooldown;
    public float bulletSpeed;
    public float bulletDamage;
    public float bulletSize;
    public float maxHealth;
    public NetworkVariable<float> meleeDamage = new NetworkVariable<float>();
    public float turretRecoilAmount; // How much the turret moves backward
    public float playerRecoilAmount;
    
    // Tier 0 Classes
    public GameObject DefaultPrefab;
    
    // Tier 1 Classes
    public GameObject TwinsPrefab;
    public GameObject KnightPrefab;
    
    public Transform turretTransform;
    public float recoilDuration = 0.25f; // How long it takes to return

    private Rigidbody2D rb;
    
    public enum Upgrades { Default, Twins, Knight }
    public NetworkVariable<Upgrades> currentUpgrade = new NetworkVariable<Upgrades>();

    private Vector2 bulletSpawnPosition;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        if (IsOwner)
        {
            ChangeClass(currentUpgrade.Value); // Ensure class is applied on start
        }

        // Listen for changes on `currentUpgrade` and update the visuals.
        currentUpgrade.OnValueChanged += (oldValue, newValue) =>
        {
            SpawnUpgradeParts(newValue);
        };
        
        // Force an initial sync of the turret parts.
        SpawnUpgradeParts(currentUpgrade.Value);
    }

    void Update()
    {
        if (!IsOwner) return;

        bulletTimer += Time.deltaTime;
        if (bulletTimer >= bulletCooldown && !canShoot)
        {
            canShoot = true;
        }
        
        if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && canShoot)
        {
            bulletTimer = 0;
            canShoot = false;

            switch (currentUpgrade.Value)
            {
                case Upgrades.Default:
                    UPG_Default_Shoot();
                    StartCoroutine(RecoilChildrenAndPlayer());
                    break;
                case Upgrades.Twins:
                    UPG_Twins_Shoot();
                    StartCoroutine(RecoilChildrenAndPlayer());
                    break;
                case Upgrades.Knight:
                    // Knight-specific shooting can be added here.
                    break;
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        
        MoveUpdate();
        
        switch (currentUpgrade.Value)
        {
            case Upgrades.Knight:
                transform.GetChild(0).GetChild(0).Rotate(0, 0, 360f * Time.fixedDeltaTime);
                break;
        }
    }
    
    void MoveUpdate()
    {
        Vector2 moveInput = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) moveInput.y += 1;
        if (Input.GetKey(KeyCode.A)) moveInput.x -= 1;
        if (Input.GetKey(KeyCode.S)) moveInput.y -= 1;
        if (Input.GetKey(KeyCode.D)) moveInput.x += 1;

        if (moveInput != Vector2.zero)
            moveInput = moveInput.normalized;

        rb.linearVelocityX += moveInput.x * movementSpeed * Time.fixedDeltaTime;
        rb.linearVelocityY += moveInput.y * movementSpeed * Time.fixedDeltaTime;
    }

    void SpawnUpgradeParts(Upgrades upgrade)
    {
        if (IsOwner) StopAllCoroutines();
        
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        GameObject spawnedPrefab = null;
        switch (upgrade)
        {
            case Upgrades.Default:
                spawnedPrefab = Instantiate(DefaultPrefab, transform.position, Quaternion.identity);
                break;
            case Upgrades.Twins:
                spawnedPrefab = Instantiate(TwinsPrefab, transform.position, Quaternion.identity);
                break;
            case Upgrades.Knight:
                spawnedPrefab = Instantiate(KnightPrefab, transform.position, Quaternion.identity);
                break;
        }

        if (spawnedPrefab != null)
        {
            spawnedPrefab.transform.SetParent(transform);
            spawnedPrefab.transform.right = transform.right;
        }
    }
    
    public void ChangeClass(Upgrades upgrade)
    {
        if (!IsOwner) return;
        currentUpgrade.Value = upgrade;
        UpdateClassStats();
    }

    void UpdateClassStats()
    {
        switch (currentUpgrade.Value)
        {
            case Upgrades.Default:
                movementSpeed = 15;
                bulletCooldown = 0.6f;
                bulletSpeed = 5;
                bulletDamage = 1;
                bulletSize = 1;
                maxHealth = 10;
                meleeDamage.Value = 0;
                turretRecoilAmount = 0.15f;
                playerRecoilAmount = 1;
                break;
            case Upgrades.Twins:
                movementSpeed = 15;
                bulletCooldown = 0.6f;
                bulletSpeed = 6;
                bulletDamage = 1;
                bulletSize = 1;
                maxHealth = 15;
                meleeDamage.Value = 0;
                turretRecoilAmount = 0.2f;
                playerRecoilAmount = 1;
                break;
            case Upgrades.Knight:
                movementSpeed = 20;
                bulletCooldown = 0;
                bulletSpeed = 0;
                bulletDamage = 0;
                bulletSize = 0;
                maxHealth = 25;
                meleeDamage.Value = 2;
                turretRecoilAmount = 0;
                playerRecoilAmount = 1;
                break;
        }
    }
    
    [ServerRpc]
    public void ChangeClassServerRpc(Upgrades upgrade)
    {
        if (!IsOwner) return;
        currentUpgrade.Value = upgrade;
    }
    
    void UPG_Default_Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position + (transform.right / 2f), Quaternion.identity);
        NetworkObject networkObject = bullet.GetComponent<NetworkObject>();
        networkObject.Spawn();
        bullet.GetComponent<Bullet>().Initialise(true, bulletDamage, GetComponent<NetworkObject>());
        networkObject.GetComponent<Rigidbody2D>().linearVelocity = transform.right * bulletSpeed; 
        bullet.transform.localScale = new Vector3(bulletSize, bulletSize, bulletSize);
    }
    
    async void UPG_Twins_Shoot()
    {
        for (int i = 0; i < 2; i++)
        {
            bulletSpawnPosition = transform.position + (transform.right / 2f) + (i == 0 ? (transform.up / 3.5f) : (-transform.up / 3.5f));
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPosition, Quaternion.identity);
            NetworkObject networkObject = bullet.GetComponent<NetworkObject>();
            networkObject.Spawn();
            bullet.GetComponent<Bullet>().Initialise(true, bulletDamage, GetComponent<NetworkObject>());
            networkObject.GetComponent<Rigidbody2D>().linearVelocity = transform.right * bulletSpeed; 
            bullet.transform.localScale = new Vector3(bulletSize, bulletSize, bulletSize);
            if (i == 0) await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(recoilDuration * 1000));
        }
    }
    
    private IEnumerator RecoilChildrenAndPlayer()
    {
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            turretTransform = transform.GetChild(0).GetChild(i);
            Vector3 originalPosition = turretTransform.localPosition;
            float direction = transform.localScale.x > 0 ? -1f : 1f;
            rb.AddForce(new Vector2(-transform.right.x * playerRecoilAmount, -transform.right.y * playerRecoilAmount), ForceMode2D.Impulse);
            Vector3 recoilPosition = originalPosition + (Vector3.right * direction * turretRecoilAmount);
            float elapsed = 0f;
            while (elapsed < recoilDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                turretTransform.localPosition = Vector3.Lerp(originalPosition, recoilPosition, elapsed / (recoilDuration * 0.5f));
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < recoilDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                turretTransform.localPosition = Vector3.Lerp(recoilPosition, originalPosition, elapsed / (recoilDuration * 0.5f));
                yield return null;
            }
            turretTransform.localPosition = originalPosition;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Bitrock")/* && meleeDamage.Value > 0*/)
        {
            Vector2 recoilDirection = (transform.position - other.transform.position).normalized;
            rb.AddForce(recoilDirection * 5, ForceMode2D.Impulse);
        }
    }
}
