using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public string projectileTag = "Bullet";
    public float moveSpeed = 5f;
    public float attackCooldown = 1f;
    public string playerColor;
    public int attackCount = 2;

    public List<Transform> targetEnemies = new List<Transform>();
    private int currentTargetIndex = 0;
    public bool isAttacking = false;
    private bool canShoot = true;
    
    private TextMeshProUGUI attackText;
    public bool isFirstRow;
    [SerializeField] Transform targetPosOutScreen;

    private int originalAttackCount;
    private Coroutine attackWaitCoroutine;
    private bool isProcessingHit = false;

    private void Awake()
    {
        targetPosOutScreen = GameObject.FindWithTag("OutSidePos").transform;
        attackText = GetComponentInChildren<TextMeshProUGUI>();
        originalAttackCount = attackCount;
        UpdateAttackText();
    }

    private void Update()
    {
        if (!isAttacking) return;

        // Check if we should stop attacking
        if (attackCount <= 0 || !HasValidTargets())
        {
            FinishAttacking();
            return;
        }

        // Get current target safely
        Transform currentTarget = GetCurrentTarget();
        if (currentTarget == null)
        {
            AdvanceToNextTarget();
            return;
        }

        if (canShoot && !isProcessingHit)
        {
            // Rotation code
            Vector3 targetPos = currentTarget.position;
            Vector3 direction = targetPos - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, lookRotation.eulerAngles.y);

                float rotationSpeed = 5f;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            ShootProjectile(currentTarget);
            canShoot = false;

            // Start coroutine and store reference
            if (attackWaitCoroutine != null)
                StopCoroutine(attackWaitCoroutine);
            attackWaitCoroutine = StartCoroutine(AttackWaitCoroutine());
        }
    }

    private bool HasValidTargets()
    {
        if (targetEnemies.Count == 0) return false;

        foreach (var target in targetEnemies)
        {
            if (target != null && target.gameObject.activeInHierarchy) return true;
        }
        return false;
    }

    private Transform GetCurrentTarget()
    {
        if (currentTargetIndex < targetEnemies.Count &&
            targetEnemies[currentTargetIndex] != null &&
            targetEnemies[currentTargetIndex].gameObject.activeInHierarchy)
        {
            return targetEnemies[currentTargetIndex];
        }
        return null;
    }

    private void AdvanceToNextTarget()
    {
        // Find next valid target without recursion
        int startIndex = currentTargetIndex;
        currentTargetIndex++;

        while (currentTargetIndex < targetEnemies.Count)
        {
            if (targetEnemies[currentTargetIndex] != null && targetEnemies[currentTargetIndex].gameObject.activeInHierarchy)
            {
                return; // Found valid target
            }
            currentTargetIndex++;
        }

        // If we reached the end and no valid target found
        if (currentTargetIndex >= targetEnemies.Count)
        {
            // Try to find any valid target from the beginning
            for (int i = 0; i < targetEnemies.Count; i++)
            {
                if (targetEnemies[i] != null && targetEnemies[i].gameObject.activeInHierarchy)
                {
                    currentTargetIndex = i;
                    return;
                }
            }

            // No valid targets found at all
            FinishAttacking();
        }
    }

    public void SetAttackCount(int value)
    {
        this.attackCount = value;
        this.originalAttackCount = value;
        UpdateAttackText();
    }

    public void AssignTargets(List<Transform> enemyList)
    {
        // Filter out null and inactive enemies
        targetEnemies = enemyList?.FindAll(t => t != null && t.gameObject.activeInHierarchy) ?? new List<Transform>();
        currentTargetIndex = 0;

        // Find first valid target
        AdvanceToNextTarget();

        UpdateAttackText();
    }

    private void UpdateAttackText()
    {
        if (attackText != null)
            attackText.text = attackCount.ToString();
    }

    public void EnableAttack(List<Transform> enemies)
    {
        if (enemies == null || enemies.Count == 0) return;

        // Filter out null and inactive enemies
        List<Transform> validEnemies = enemies.FindAll(t => t != null && t.gameObject.activeInHierarchy);
        int count = Mathf.Min(attackCount, validEnemies.Count);

        if (count > 0)
        {
            targetEnemies = validEnemies.GetRange(0, count);
            currentTargetIndex = 0;
            AdvanceToNextTarget();
            UpdateAttackText();
        }
        else
        {
          
            FinishAttacking();
        }
    }

    public void SetAttack()
    {
        if (targetEnemies.Count > 0 && HasValidTargets() && attackCount > 0)
        {
            isAttacking = true;
            canShoot = true;
           
        }
        else
        {
           
            FinishAttacking();
        }
    }

    public void StopAttack()
    {
        isAttacking = false;
        canShoot = false;

        if (attackWaitCoroutine != null)
        {
            StopCoroutine(attackWaitCoroutine);
            attackWaitCoroutine = null;
        }
    }

    private void ShootProjectile(Transform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
          
            AdvanceToNextTarget();
            canShoot = true;
            return;
        }

        GameObject proj = ObjectPool.Instance.SpawnFromPool(projectileTag, transform.position, Quaternion.identity);
        if (proj == null)
        {
           
            canShoot = true;
            return;
        }

        proj.SetActive(true);
        Projectile p = proj.GetComponent<Projectile>();
        if (p == null)
        {
           
            canShoot = true;
            return;
        }

        p.SetTarget(target);
        p.onHit = () =>
        {
            OnProjectileHit(target);
        };

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("Attack");

       
    }

    private IEnumerator AttackWaitCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canShoot = true;
        attackWaitCoroutine = null;
    }

    private void OnProjectileHit(Transform hitTarget)
    {
        if (isProcessingHit) return; // Prevent re-entrancy

        isProcessingHit = true;

      

        // Decrement the current attack count
        attackCount = Mathf.Max(0, attackCount - 1);
        UpdateAttackText();

        // Mark this target as hit (but don't remove from list to maintain indices)
        if (hitTarget != null)
        {
            // The target should be handled by the enemy system
            // We just advance to next target
        }

        // Move to next target
        AdvanceToNextTarget();

        isProcessingHit = false;

        // Check if we should finish
        if (attackCount <= 0)
        {
            FinishPlayerRole();
        }
        else if (!HasValidTargets())
        {
            FinishAttacking();
        }
    }

    private void FinishAttacking()
    {
        if (!isAttacking) return; // Already finished

       

        StopAttack();

        if (PlayerManager.Instance != null)
            PlayerManager.Instance.UnlockColorAttack(playerColor);
    }

    void FinishPlayerRole()
    {
       
        StopAttack();

        if (GetComponent<PlayerMovement>() != null)
        {
            PlayerMovement pm = GetComponent<PlayerMovement>();
            if (pm.currentSlot != null)
            {
                PlayerManager.Instance.FreeLocation(pm.currentSlot);
            }
        }

        DOTween.To(() => transform.position, x => transform.position = x, targetPosOutScreen.position, 3f)
            .OnComplete(() =>
            {
                if (PlayerManager.Instance != null)
                {
                    PlayerManager.Instance.touchedPlayers.Remove(this.gameObject);
                    PlayerManager.Instance.PlayerLeftPosition(this.gameObject);
                }
                Destroy(gameObject);
            });
    }

    public void ResetAttackCount()
    {
        attackCount = originalAttackCount;
        UpdateAttackText();
    }

    // Clean up when destroyed
    private void OnDestroy()
    {
        StopAttack();
    }
}