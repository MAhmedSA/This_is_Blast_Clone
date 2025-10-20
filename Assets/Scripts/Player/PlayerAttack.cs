using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public string projectileTag = "Bullet";
    public float moveSpeed = 5f;
    public float attackCooldown = 2f;
    public string playerColor;
    public int attackCount =2; // Number shown on player

    private List<Transform> targetEnemies = new List<Transform>();
    private int currentTargetIndex = 0;
    private bool isAttacking = false;
    private bool canShoot = true;
    private TextMeshProUGUI attackText;
    public List<Transform> allowedEnemies = new List<Transform>();

    [SerializeField] Transform targetPosOutScreen;
    private void Awake()
    {
        targetPosOutScreen= GameObject.FindWithTag("OutSidePos").transform;
        // Find the Text component inside the prefab
        attackText = GetComponentInChildren<TextMeshProUGUI>();

        // Update text when starting (if already assigned)
        UpdateAttackText();
    }
   
    private void Update()
    {
        if (!isAttacking || targetEnemies.Count == 0) return;

        if (currentTargetIndex >= targetEnemies.Count)
        {
            FinishAttacking();
            return;
        }

        Transform currentTarget=null;
        if (targetEnemies[currentTargetIndex] != null) {
            currentTarget = targetEnemies[currentTargetIndex];
        }

        if (currentTarget == null)
        {
           
            currentTargetIndex++;
            return;
        }

       
        // Attack without moving
        if (canShoot)
        {
            // Rotate player in Z axis only (for 2.5D setup)
            Vector3 targetPos = currentTarget.position;
            Vector3 direction = targetPos - transform.position;
            direction.y = 0f; // ignore vertical rotation

            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // Convert it to Z-axis rotation
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, lookRotation.eulerAngles.y);

            // Smoothly rotate using Slerp
            float rotationSpeed = 5f; // adjust for smoothness
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            
            ShootProjectile(currentTarget);
            canShoot = false;
            StartCoroutine(AttackWaitCoroutine());
            
        }
    }

    public void SetAttackCount(int value)
    {
        this.attackCount = value;
        UpdateAttackText();
    }

    private void UpdateAttackText()
    {
        if (attackText != null)
            attackText.text = attackCount.ToString();
    }
    public void EnableAttack(List<Transform> enemies)
    {
        if (enemies == null || enemies.Count == 0)
        {
           
            return;
        }

        // respect attackCount (limit how many targets we will try)
        int count = Mathf.Min(attackCount, enemies.Count);
        targetEnemies = enemies.GetRange(0, count);
        currentTargetIndex = 0;
        isAttacking = true;
       
     
    }
    public void SetAttack() {
        
        isAttacking = true;
    }
    private void ShootProjectile(Transform target)
    {
        GameObject proj = ObjectPool.Instance.SpawnFromPool(projectileTag, transform.position, Quaternion.identity);
        if (proj == null)
        {
           
            return;
        }

        proj.SetActive(true);
        Projectile p = proj.GetComponent<Projectile>();
        if (p == null)
        {
         
            return;
        }

        p.SetTarget(target);

        // wire up callback safely
        p.onHit = () =>
        {
            OnProjectileHit(target);
        };

        // play sound if you have AudioManager (safe-guard)
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("Attack");
       
    }

    private IEnumerator AttackWaitCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canShoot = true;
    }

    // Now pass the target that was hit — helps us verify same target
    private void OnProjectileHit(Transform hitTarget)
    {
        attackCount = Mathf.Max(0, attackCount - 1);
        if (attackCount == 0) {
            // play animation to make player move out side screen and destroy player object
            FinishPlayerRole();
        }
        UpdateAttackText();
        // if hitTarget equals current target, advance
        if (currentTargetIndex < targetEnemies.Count && targetEnemies[currentTargetIndex] == hitTarget)
        {
            currentTargetIndex++;
        }
        else
        {
            // If not, find and remove it if present
            int found = targetEnemies.IndexOf(hitTarget);
            if (found >= 0) targetEnemies.RemoveAt(found);
        }

        // If we've exhausted targets, finish
        if (currentTargetIndex >= targetEnemies.Count)
        {
            FinishAttacking();
        }
    }

    private void FinishAttacking()
    {
        isAttacking = false;
       
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.UnlockColorAttack(playerColor);
    }

    void FinishPlayerRole()
    {
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
                PlayerManager.Instance.touchedPlayers.Remove(this.gameObject);
                Destroy(gameObject);
            });
    }
}
