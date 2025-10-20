using UnityEngine;
using DG.Tweening;
public class EnemyDie : MonoBehaviour
{
    // Time To Scale Game Object to Zero and Destroy It
    [SerializeField] float timeToDie = 0.2f;
    public string currentColor;

   
    
    public void Die()
    {
        currentColor = gameObject.GetComponent<EnemyMovement>().colorEnemy;
        gameObject.transform.DOScale(Vector3.zero, timeToDie)
            .OnComplete(
        () => {
            
            if (currentColor == "red")
            {
                GameManager.Instance.redEnemies.Remove(gameObject.transform);
                GameManager.Instance.NumberOfEnemiesDefeated += 1;
                Destroy(gameObject);
            }
            if (currentColor == "blue")
            {
                GameManager.Instance.blueEnemies.Remove(gameObject.transform);
                GameManager.Instance.NumberOfEnemiesDefeated += 1;
                Destroy(gameObject);
            }
            if (currentColor == "green")
            {
                GameManager.Instance.greenEnemies.Remove(gameObject.transform);
                GameManager.Instance.NumberOfEnemiesDefeated += 1;
                Destroy(gameObject);
            }
            if (currentColor == "yellow")
            {
                GameManager.Instance.yellowEnemies.Remove(gameObject.transform);
                GameManager.Instance.NumberOfEnemiesDefeated += 1;
                Destroy(gameObject);
            }
        } );
        
    }
    


}
