using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool hasArrived = false; //  new flag to prevent re-clicking
    public bool HasArrived => hasArrived; // public to read only in input to know he is arrived or no 


    private PlayerAttack playerAttack;

    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack == null)
            playerAttack = gameObject.AddComponent<PlayerAttack>();
    }

    public void MoveTo(Vector3 position)
    {
        //  Prevent moving again if already arrived
        if (hasArrived) return;

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
                hasArrived = true; //  lock movement after arrival
                PlayerManager.Instance.touchedPlayers.Add(this.gameObject); // to control in other thing
                // When arrived, enable attack for this player
                if (playerAttack != null)
                {
                    Transform enemy = GameManager.Instance.GetFrontlineEnemy(playerAttack.playerColor);
                    if (enemy != null)
                    {
                        playerAttack.EnableAttack(GameManager.Instance.GetFirstRowEnemiesByColor(playerAttack.playerColor));
                    }
                }
            }
        }
    }
}
