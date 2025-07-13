using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Makes an object collectible by players or other specified objects.
/// Attach to any GameObject with a Collider.
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    [Header("Collectible Settings")]
    [Tooltip("Tags that can collect this item (leave empty for all tags)")]
    public string[] CollectorTags = { "Player" };

    [Tooltip("Value of this collectible (for use with score systems)")]
    public int Value = 1;

    [Tooltip("Should this object be destroyed when collected")]
    public bool DestroyOnCollect = true;

    [Tooltip("Effect to spawn when collected (optional)")]
    public GameObject CollectionEffect;

    [Header("Movement")]
    [Tooltip("Should the collectible bob up and down")]
    public bool BobUpAndDown = true;

    [Tooltip("Bob amplitude (how far it moves)")]
    public float BobAmount = 0.2f;

    [Tooltip("Bob speed")]
    public float BobSpeed = 2f;

    [Tooltip("Should the collectible rotate")]
    public bool Rotate = true;

    [Tooltip("Rotation speed in degrees per second")]
    public float RotationSpeed = 90f;

    [Header("Sound")]
    [Tooltip("Sound to play when collected (optional)")]
    public AudioClip CollectionSound;

    [Tooltip("Volume of the collection sound")]
    [Range(0f, 1f)]
    public float SoundVolume = 1f;

    [Header("Events")]
    [Tooltip("Event triggered when the item is collected")]
    public UnityEvent<GameObject> OnCollected;

    // Original position for bobbing motion
    private Vector3 _startPos;
    private float _bobTimer = 0f;

    // Cached components
    private Collider _collider;
    private Collider2D _collider2D;

    // Start is called before the first frame update
    private void Start()
    {
        // Save the initial position for bobbing motion
        _startPos = transform.position;

        // Cache components
        _collider = GetComponent<Collider>();
        _collider2D = GetComponent<Collider2D>();

        // Make sure we have at least one collider
        if (_collider == null && _collider2D == null)
        {
            Debug.LogWarning("CollectibleItem: No Collider/Collider2D found. The collectible won't be triggered by collisions.");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Update the bobbing motion
        if (BobUpAndDown)
        {
            _bobTimer += Time.deltaTime * BobSpeed;
            float bobOffset = Mathf.Sin(_bobTimer) * BobAmount;
            transform.position = new Vector3(_startPos.x, _startPos.y + bobOffset, _startPos.z);
        }

        // Update the rotation
        if (Rotate)
        {
            transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
        }
    }

    // Called when a regular 3D collider enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other.gameObject);
    }

    // Called when a 2D collider enters the trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other.gameObject);
    }

    // Check if the object can collect this item and handle collection
    private void TryCollect(GameObject collector)
    {
        // Check if the collector has a valid tag
        bool canCollect = false;

        // If no collector tags specified, anyone can collect
        if (CollectorTags.Length == 0)
        {
            canCollect = true;
        }
        else
        {
            // Check if the collector has any of the valid tags
            foreach (string tag in CollectorTags)
            {
                if (collector.CompareTag(tag))
                {
                    canCollect = true;
                    break;
                }
            }
        }

        // If can collect, handle the collection
        if (canCollect)
        {
            Collect(collector);
        }
    }

    /// <summary>
    /// Handle collection of this item
    /// </summary>
    /// <param name="collector">The GameObject that collected this item</param>
    public void Collect(GameObject collector)
    {
        // Trigger the collection event
        OnCollected?.Invoke(collector);

        // Try to find a score manager on the collector and add points
        ScoreManager scoreManager = collector.GetComponent<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddScore(Value);
        }

        // Play collection sound if specified
        if (CollectionSound != null)
        {
            AudioSource.PlayClipAtPoint(CollectionSound, transform.position, SoundVolume);
        }

        // Spawn collection effect if specified
        if (CollectionEffect != null)
        {
            Instantiate(CollectionEffect, transform.position, Quaternion.identity);
        }

        // Destroy the object if specified
        if (DestroyOnCollect)
        {
            Destroy(gameObject);
        }
        else
        {
            // Just disable the colliders if not destroying
            if (_collider != null)
                _collider.enabled = false;

            if (_collider2D != null)
                _collider2D.enabled = false;

            // And disable the renderer
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;

            // Also disable any child renderers
            Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in childRenderers)
                r.enabled = false;
        }
    }
}
