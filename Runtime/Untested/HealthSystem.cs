using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple health system that can be attached to any GameObject.
/// Handles health, damage, healing, and death events.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health amount")]
    public float MaxHealth = 100f;
    
    [Tooltip("Current health amount (will be set to MaxHealth on start)")]
    public float CurrentHealth;
    
    [Tooltip("If true, object will be destroyed when health reaches 0")]
    public bool DestroyOnDeath = true;
    
    [Tooltip("Delay before destroying the object after death (in seconds)")]
    public float DestroyDelay = 0f;
    
    [Header("Invulnerability")]
    [Tooltip("Time of invulnerability after taking damage (in seconds)")]
    public float InvulnerabilityTime = 0f;
    
    [Header("Events")]
    [Tooltip("Event triggered when damage is taken")]
    public UnityEvent<float> OnDamage;
    
    [Tooltip("Event triggered when health is restored")]
    public UnityEvent<float> OnHeal;
    
    [Tooltip("Event triggered when health reaches 0")]
    public UnityEvent OnDeath;
    
    // Track if the object is dead
    private bool _isDead = false;
    
    // Track if currently invulnerable
    private bool _isInvulnerable = false;
    
    // Timer for invulnerability
    private float _invulnerabilityTimer = 0f;
    
    // Initialize health on start
    private void Start()
    {
        CurrentHealth = MaxHealth;
    }
    
    // Update is called once per frame to handle invulnerability timer
    private void Update()
    {
        // Handle invulnerability timer
        if (_isInvulnerable)
        {
            _invulnerabilityTimer -= Time.deltaTime;
            
            if (_invulnerabilityTimer <= 0f)
            {
                _isInvulnerable = false;
            }
        }
    }
    
    /// <summary>
    /// Apply damage to this health system
    /// </summary>
    /// <param name="damageAmount">Amount of damage to apply</param>
    /// <returns>True if damage was applied, false if invulnerable or already dead</returns>
    public bool TakeDamage(float damageAmount)
    {
        // Check if already dead or invulnerable
        if (_isDead || _isInvulnerable)
            return false;
        
        // Apply damage
        CurrentHealth -= damageAmount;
        
        // Trigger damage event
        OnDamage?.Invoke(damageAmount);
        
        // Check for death
        if (CurrentHealth <= 0f)
        {
            CurrentHealth = 0f;
            Die();
            return true;
        }
        
        // Apply invulnerability if set
        if (InvulnerabilityTime > 0f)
        {
            _isInvulnerable = true;
            _invulnerabilityTimer = InvulnerabilityTime;
        }
        
        return true;
    }
    
    /// <summary>
    /// Heal this health system
    /// </summary>
    /// <param name="healAmount">Amount of health to restore</param>
    /// <returns>True if healing was applied, false if already at max health or dead</returns>
    public bool Heal(float healAmount)
    {
        // Check if already dead or at max health
        if (_isDead || CurrentHealth >= MaxHealth)
            return false;
        
        // Apply healing
        CurrentHealth += healAmount;
        
        // Cap health at maximum
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        
        // Trigger heal event
        OnHeal?.Invoke(healAmount);
        
        return true;
    }
    
    /// <summary>
    /// Kill this health system immediately
    /// </summary>
    public void Die()
    {
        // Prevent multiple deaths
        if (_isDead)
            return;
        
        _isDead = true;
        CurrentHealth = 0f;
        
        // Trigger death event
        OnDeath?.Invoke();
        
        // Destroy the object if specified
        if (DestroyOnDeath)
        {
            if (DestroyDelay > 0f)
            {
                Destroy(gameObject, DestroyDelay);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    /// <summary>
    /// Check if this health system is currently dead
    /// </summary>
    /// <returns>True if dead, false otherwise</returns>
    public bool IsDead()
    {
        return _isDead;
    }
    
    /// <summary>
    /// Check if this health system is currently invulnerable
    /// </summary>
    /// <returns>True if invulnerable, false otherwise</returns>
    public bool IsInvulnerable()
    {
        return _isInvulnerable;
    }
    
    /// <summary>
    /// Get the current health percentage (0-1)
    /// </summary>
    /// <returns>Current health as a percentage between 0 and 1</returns>
    public float GetHealthPercentage()
    {
        return CurrentHealth / MaxHealth;
    }
}
