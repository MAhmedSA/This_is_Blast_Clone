using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool hasArrived = false;
    public bool HasArrived => hasArrived;

    public Transform currentSlot;
    private PlayerAttack playerAttack;

    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack == null)
            playerAttack = gameObject.AddComponent<PlayerAttack>();
    }

    public void MoveTo(Vector3 position, Transform slot = null)
    {
        if (hasArrived) return;
        currentSlot = slot;
        targetPosition = position;
        isMoving = true;
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
                hasArrived = true;
                PlayerManager.Instance.touchedPlayers.Add(this.gameObject);

                if (playerAttack != null)
                {
                    // Use the new method that prevents enemy duplication
                    PlayerManager.Instance.AssignEnemiesToPlayer(this.gameObject);
                    playerAttack.SetAttack();
                }
            }
        }
    }
}