using UnityEngine;
using System.Collections;

public class Level10GateController : MonoBehaviour
{
    [Header("Dual Gate Settings")]
    [Tooltip("Left side wall/gate that opens upwards")]
    public Transform leftGateWall;  
    [Tooltip("Right side wall/gate that opens upwards")]
    public Transform rightGateWall; 
    [Tooltip("How high both gates lift when button is pressed")]
    public float openDistance = 5f; 
    [Tooltip("How fast both gates open")]
    public float openSpeed = 2f;    
    
    [Header("Audio")]
    public AudioClip gateOpenSound;
    private AudioSource audioSource;
    private bool isGateOpen = false;
    public static bool areGatesOpen = false;

    [Header("Button Press Visuals")]
    public float pressDownDistance = 0.2f; 
    private SpriteRenderer originalSr;
    private Transform visualTransform;
    private int playersOnButton = 0;

    void Start()
    {
        areGatesOpen = false;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Visual vs Physics Separation Trick to prevent collider flickering
        originalSr = GetComponent<SpriteRenderer>();
        if (originalSr != null)
        {
            GameObject visualObj = new GameObject("ButtonVisual");
            visualTransform = visualObj.transform;
            visualTransform.SetParent(transform);
            visualTransform.localPosition = Vector3.zero;
            visualTransform.localRotation = Quaternion.identity;
            visualTransform.localScale = Vector3.one;

            SpriteRenderer newSr = visualObj.AddComponent<SpriteRenderer>();
            newSr.sprite = originalSr.sprite;
            newSr.color = originalSr.color;
            newSr.sortingLayerID = originalSr.sortingLayerID;
            newSr.sortingOrder = originalSr.sortingOrder;

            originalSr.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playersOnButton++;
            if (playersOnButton == 1 && visualTransform != null)
            {
                visualTransform.localPosition = new Vector3(0, -pressDownDistance, 0);
            }

            if (!isGateOpen)
            {
                isGateOpen = true;
                areGatesOpen = true;
                if (gateOpenSound != null) audioSource.PlayOneShot(gateOpenSound);
                
                if (leftGateWall != null) StartCoroutine(OpenGate(leftGateWall));
                if (rightGateWall != null) StartCoroutine(OpenGate(rightGateWall));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playersOnButton--;
            if (playersOnButton <= 0)
            {
                playersOnButton = 0;
                if (visualTransform != null)
                {
                    visualTransform.localPosition = Vector3.zero;
                }
            }
        }
    }

    private IEnumerator OpenGate(Transform gate)
    {
        Vector3 targetPosition = gate.position + Vector3.up * openDistance;
        while (Vector3.Distance(gate.position, targetPosition) > 0.01f)
        {
            gate.position = Vector3.MoveTowards(gate.position, targetPosition, openSpeed * Time.deltaTime);
            yield return null;
        }
        gate.position = targetPosition;
    }
}
