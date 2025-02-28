using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple trigger zone that detects when objects enter, stay in, or exit the zone.
/// Attach to any GameObject with a Collider set to "Is Trigger".
/// </summary>
public class TriggerZone : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("Layers that will trigger this zone")]
    public LayerMask TargetLayers = -1; // Default to "Everything"
    
    [Tooltip("Tags that will trigger this zone (leave empty for all tags)")]
    public List<string> TargetTags = new List<string>();
    
    [Tooltip("Optional key that must be pressed while in trigger to activate")]
    public KeyCode ActivationKey = KeyCode.None;
    
    [Tooltip("Should the activation key trigger only once or repeatedly while held?")]
    public bool ActivateOncePerPress = true;
    
    [Header("Trigger Events")]
    [Tooltip("Event triggered when an object enters the zone")]
    public UnityEvent<GameObject> OnTriggerEnterEvent;
    
    [Tooltip("Event triggered when an object stays in the zone")]
    public UnityEvent<GameObject> OnTriggerStayEvent;
    
    [Tooltip("Event triggered when an object exits the zone")]
    public UnityEvent<GameObject> OnTriggerExitEvent;
    
    [Tooltip("Event triggered when activation key is pressed while in the zone")]
    public UnityEvent<GameObject> OnActivationEvent;
    
    // Keep track of objects currently in the trigger zone
    private Dictionary<GameObject, bool> _objectsInTrigger = new Dictionary<GameObject, bool>();
    
    // Required component
    private Collider _collider;
    
    // Initialize and check requirements
    private void Awake()
    {
        // Get the collider component
        _collider = GetComponent<Collider>();
        
        // Make sure the collider is a trigger
        if (_collider != null && !_collider.isTrigger)
        {
            Debug.LogWarning("TriggerZone: Collider is not set as a trigger. Setting isTrigger to true.");
            _collider.isTrigger = true;
        }
        else if (_collider == null)
        {
            Debug.LogError("TriggerZone: No Collider found on this GameObject. Please add a Collider component.");
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Check for activation key press if specified
        if (ActivationKey != KeyCode.None && Input.GetKey(ActivationKey))
        {
            // Loop through all objects in the trigger
            List<GameObject> toRemove = new List<GameObject>();
            
            foreach (var entry in _objectsInTrigger)
            {
                GameObject obj = entry.Key;
                bool wasActivatedThisPress = entry.Value;
                
                // Skip if set to activate once per press and this object was already activated
                if (ActivateOncePerPress && wasActivatedThisPress)
                    continue;
                
                // Clean up destroyed objects
                if (obj == null)
                {
                    toRemove.Add(obj);
                    continue;
                }
                
                // Trigger the activation event
                OnActivationEvent?.Invoke(obj);
                
                // Mark as activated for this press
                _objectsInTrigger[obj] = true;
            }
            
            // Clean up destroyed objects
            foreach (var obj in toRemove)
            {
                _objectsInTrigger.Remove(obj);
            }
        }
        // Reset activation flags when key is released
        else if (ActivationKey != KeyCode.None && Input.GetKeyUp(ActivationKey))
        {
            // Reset all activation flags
            List<GameObject> objects = new List<GameObject>(_objectsInTrigger.Keys);
            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    _objectsInTrigger[obj] = false;
                }
            }
        }
    }
    
    // Check if an object meets the layer and tag requirements
    private bool IsValidTarget(GameObject obj)
    {
        // Check if object exists and is on the target layer
        if (obj == null || !LayerInLayerMask(obj.layer, TargetLayers))
            return false;
        
        // If no specific tags are set, accept all objects on the target layer
        if (TargetTags.Count == 0)
            return true;
        
        // Check if the object has any of the target tags
        foreach (string tag in TargetTags)
        {
            if (obj.CompareTag(tag))
                return true;
        }
        
        // If we got here, the object has none of the target tags
        return false;
    }
    
    // Utility method to check if a layer is in a layer mask
    private bool LayerInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }
    
    // Called when an object enters the trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (IsValidTarget(other.gameObject))
        {
            // Add to the tracked objects
            _objectsInTrigger[other.gameObject] = false;
            
            // Trigger the enter event
            OnTriggerEnterEvent?.Invoke(other.gameObject);
        }
    }
    
    // Called every frame while an object is in the trigger zone
    private void OnTriggerStay(Collider other)
    {
        if (IsValidTarget(other.gameObject))
        {
            // Make sure the object is being tracked
            if (!_objectsInTrigger.ContainsKey(other.gameObject))
            {
                _objectsInTrigger[other.gameObject] = false;
            }
            
            // Trigger the stay event
            OnTriggerStayEvent?.Invoke(other.gameObject);
        }
    }
    
    // Called when an object exits the trigger zone
    private void OnTriggerExit(Collider other)
    {
        if (IsValidTarget(other.gameObject))
        {
            // Remove from tracked objects
            _objectsInTrigger.Remove(other.gameObject);
            
            // Trigger the exit event
            OnTriggerExitEvent?.Invoke(other.gameObject);
        }
    }
    
    // 2D versions for 2D projects
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsValidTarget(other.gameObject))
        {
            // Add to the tracked objects
            _objectsInTrigger[other.gameObject] = false;
            
            // Trigger the enter event
            OnTriggerEnterEvent?.Invoke(other.gameObject);
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsValidTarget(other.gameObject))
        {
            // Make sure the object is being tracked
            if (!_objectsInTrigger.ContainsKey(other.gameObject))
            {
                _objectsInTrigger[other.gameObject] = false;
            }
            
            // Trigger the stay event
            OnTriggerStayEvent?.Invoke(other.gameObject);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsValidTarget(other.gameObject))
        {
            // Remove from tracked objects
            _objectsInTrigger.Remove(other.gameObject);
            
            // Trigger the exit event
            OnTriggerExitEvent?.Invoke(other.gameObject);
        }
    }
}
