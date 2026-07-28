using UnityEngine;
using System.Collections;

public class Level10FakeFloorSpike : MonoBehaviour
{
    [Header("Floor Settings")]
    [Tooltip("The fake floor object that disappears when the player approaches")]
    public GameObject floorToDisappear;
    public AudioClip trapTriggerSound;

    [Header("Left Wall Spikes (Trap)")]
    [Tooltip("The spike GameObject situated on the left wall of the pit shaft")]
    public Transform leftWallSpikes;
    [Tooltip("How far the spikes shoot out to the RIGHT (into the shaft) when triggered")]
    public float spikePopOutDistance = 2.0f;
    [Tooltip("How fast the spikes shoot out")]
    public float spikePopSpeed = 20f;
    [Tooltip("If true, automatically sets the spike object tag to DeathTrap")]
    public bool autoTagAsDeathTrap = true;

    private AudioSource audioSource;
    private bool isFloorTriggered = false;
    private bool isSpikeTriggered = false;
    private Vector3 spikesTargetPos;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (leftWallSpikes != null)
        {
            if (autoTagAsDeathTrap)
            {
                // Tag the parent
                if (!leftWallSpikes.CompareTag("DeathTrap")) leftWallSpikes.tag = "DeathTrap";
                
                // Tag all children (in case colliders are on child objects!)
                foreach (Transform child in leftWallSpikes.GetComponentsInChildren<Transform>())
                {
                    if (!child.CompareTag("DeathTrap")) child.tag = "DeathTrap";
                }
            }
            // Spikes on the left wall shoot out towards the RIGHT into the pit shaft
            spikesTargetPos = leftWallSpikes.position + Vector3.right * spikePopOutDistance;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. Floor always disappears when player enters the trigger zone
            if (!isFloorTriggered)
            {
                isFloorTriggered = true;
                if (trapTriggerSound != null) audioSource.PlayOneShot(trapTriggerSound);

                if (floorToDisappear != null)
                {
                    floorToDisappear.SetActive(false); // Make fake floor vanish instantly!
                }
            }

            // 2. Spikes ONLY pop out if the player has already pressed the button to open the gates!
            if (Level10GateController.areGatesOpen && !isSpikeTriggered && leftWallSpikes != null)
            {
                isSpikeTriggered = true;
                StartCoroutine(PopSpikesOut());
            }
        }
    }

    private IEnumerator PopSpikesOut()
    {
        while (Vector3.Distance(leftWallSpikes.position, spikesTargetPos) > 0.01f)
        {
            leftWallSpikes.position = Vector3.MoveTowards(leftWallSpikes.position, spikesTargetPos, spikePopSpeed * Time.deltaTime);
            yield return null;
        }
        leftWallSpikes.position = spikesTargetPos;
    }
}
