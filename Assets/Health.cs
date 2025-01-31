using System;
using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void ReceiveDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Destroy(gameObject);

        OnHit();
    }
    
    public void OnHit()
    {
        StopAllCoroutines(); // Stop any existing color transition
        StartCoroutine(HitColorCoroutine());
    }
    
    private IEnumerator HitColorCoroutine()
    {
        // Turn the entity red
        spriteRenderer.color = new Color(1, 0.25f, 0.25f);
        yield return new WaitForSeconds(0.1f); // Be the above color for X many seconds

        // Fade back to original color
        float elapsedTime = 0f;
        while (elapsedTime < 0.2f) // Fade from the hit color to the original colour in seconds
        {
            spriteRenderer.color = Color.Lerp(new Color(1, 0.25f, 0.25f), originalColor, elapsedTime / 0.2f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = originalColor;
    }
}
