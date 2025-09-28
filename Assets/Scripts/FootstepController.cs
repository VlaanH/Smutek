using UnityEngine;
using System.Collections.Generic;

public class FootstepController : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceData
    {
        public string tag;
        public AudioClip[] footstepClips;  // звуки шагов
    }

    public AudioSource audioSource;
    public List<SurfaceData> surfaces;   // список поверхностей

    public float raycastDistance = 1.2f;
    public float minStepInterval = 0.35f;
    public float maxStepInterval = 0.55f;
    public float moveThreshold = 0.1f;

    public Rigidbody rb; // Rigidbody игрока
    private float stepTimer;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        ResetStepTimer();
    }

    void Update()
    {
        if (rb != null && rb.velocity.magnitude > moveThreshold)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                ResetStepTimer();
            }
        }

        Debug.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * raycastDistance, Color.green);
    }

    void ResetStepTimer()
    {
        stepTimer = Random.Range(minStepInterval, maxStepInterval);
    }

    void PlayFootstep()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, raycastDistance))
        {
            string hitTag = hit.collider.tag;

            foreach (var surface in surfaces)
            {
                if (surface.tag == hitTag && surface.footstepClips.Length > 0)
                {
                    AudioClip clip = surface.footstepClips[Random.Range(0, surface.footstepClips.Length)];
                    audioSource.PlayOneShot(clip);
                    return;
                }
            }
        }
    }
}