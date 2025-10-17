using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    public string projectileTag = "Bullet";
    public float moveSpeed = 5f;
    public float attackCooldown = 1f; // time between bullets
    public string playerColor;

    private Transform targetEnemy;
    private Vector3 targetPosition;
    private bool isAttacking = false;
    private bool canShoot = true;

    private void Update()
    {
        if (targetEnemy == null || !isAttacking) return;

        // 1️⃣ Move toward target position first
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.LookAt(targetPosition);
            return; // wait until arrived
        }

        // 2️⃣ Attack logic after reaching position
        if (canShoot)
        {
            ShootProjectile();
            canShoot = false;
            StartCoroutine(AttackWaitCoroutine());
        }
    }

    public void EnableAttack(Transform enemy, Vector3 position)
    {
        if (string.IsNullOrEmpty(playerColor) || !PlayerManager.Instance.CanColorAttack(playerColor))
            return;

        targetEnemy = enemy;
        targetPosition = position;
        isAttacking = true;

        // Lock color so other players of same color wait
        PlayerManager.Instance.LockColorAttack(playerColor);
    }

    private void ShootProjectile()
    {
        GameObject proj = ObjectPool.Instance.SpawnFromPool(projectileTag, transform.position, Quaternion.identity);
        if (proj != null && targetEnemy != null)
        {
            proj.SetActive(true);
            Projectile p = proj.GetComponent<Projectile>();
            if (p != null)
                p.SetTarget(targetEnemy);
            AudioManager.Instance.PlaySound("Attack");
            // Callback when projectile hits
            p.onHit = OnProjectileHit;
        }
    }

    private IEnumerator AttackWaitCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canShoot = true;
    }

    private void OnProjectileHit()
    {
        // Reset attacking state and unlock color
        isAttacking = false;
        PlayerManager.Instance.UnlockColorAttack(playerColor);
    }
}
