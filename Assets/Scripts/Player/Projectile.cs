using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Projectile : MonoBehaviour
{
    public Action onHit;                     // callback to PlayerAttack
    public float speed = 10f;                // projectile speed
    public ParticleSystem hitParticles;      // optional particle effect
    public float lifetime = 5f;              // safety timer for pooled reuse

    private Transform target;
    private float lifeTimer;
    private bool hasHit = false;
    [SerializeField] GameObject bulletBody;

    private void OnEnable()
    {
        lifeTimer = 0f;
        hasHit = false;
    }

    public void SetTarget(Transform enemy)
    {
        target = enemy;
        bulletBody.SetActive(true);
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
            target.gameObject.GetComponent<EnemyDie>().Die();
            Deactivate();
        }
    }

    private void PlayHitEffect()
    {
        if (hitParticles != null)
        {  
            hitParticles.Play(); 
        }
    }

    private void Deactivate()
    {
        target = null;
        hasHit = false;
        bulletBody.SetActive(false); // return to pool
    }
}
