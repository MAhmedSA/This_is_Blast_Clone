using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    private int row, column;
    private bool isMoving = false;
    private bool hasReachedStart = false;
    private Vector3 startTargetPos;

    public void InitializeEnemy(int r, int c, Vector3 startPos)
    {
        row = r;
        column = c;
        startTargetPos = startPos;
        StartCoroutine(MoveToStartPosition());
    }

    private IEnumerator MoveToStartPosition()
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, startTargetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startTargetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = startTargetPos;
        isMoving = false;
        hasReachedStart = true;

        GameManager.Instance.OccupyCell(row, column, this);
    }

    private void Update()
    {
        if (hasReachedStart && !isMoving)
        {
            TryMoveForward();
        }
    }

    void TryMoveForward()
    {
        int nextRow = row - 1; // move downward along Y (toward 0)

        if (nextRow >= 0)
        {
            if (GameManager.Instance.IsCellFree(nextRow, column))
            {
                Vector3 targetPos = GameManager.Instance.GetWorldPosition(nextRow, column);
                StartCoroutine(MoveToCell(nextRow, column, targetPos));
            }
        }
    }

    IEnumerator MoveToCell(int newRow, int newCol, Vector3 targetPos)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        GameManager.Instance.FreeCell(row, column);
        row = newRow;
        column = newCol;
        GameManager.Instance.OccupyCell(row, column, this);

        isMoving = false;

        if (row == 0)
        {
            Debug.Log($"{name} reached first line → Checking player attack.");
           
        }
    }
}
