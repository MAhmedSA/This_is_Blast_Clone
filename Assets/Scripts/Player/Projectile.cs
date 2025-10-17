using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Action onHit;                     // callback to PlayerAttack
    public float speed = 10f;                // projectile speed
    public ParticleSystem hitParticles;      // optional particle effect
    public float lifetime = 5f;              // safety timer for pooled reuse

    private Transform target;
    private float lifeTimer;
    private bool hasHit = false;

    private void OnEnable()
    {
        lifeTimer = 0f;
        hasHit = false;
    }

    public void SetTarget(Transform enemy)
    {
        target = enemy;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (target == null || hasHit)
            return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Deactivate();
            return;
        }

        // Move toward target
        transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * speed);

        // Rotate to face target
        transform.LookAt(target);

        // Hit check
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            hasHit = true;
            PlayHitEffect();
            onHit?.Invoke(); // Notify PlayerAttack or manager
            EnemyDie.Instance.Die(target.gameObject); // Call enemy die method
            Deactivate();
        }
    }

    private void PlayHitEffect()
    {
        if (hitParticles != null)
        {
            // Detach from projectile so effect stays after projectile disappears
            hitParticles.transform.parent = null;
            hitParticles.Play();
            Destroy(hitParticles.gameObject, hitParticles.main.duration);
        }
    }

    private void Deactivate()
    {
        target = null;
        hasHit = false;
        gameObject.SetActive(false); // return to pool
    }
}
