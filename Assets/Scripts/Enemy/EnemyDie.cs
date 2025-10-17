using UnityEngine;
using DG.Tweening;
public class EnemyDie : MonoBehaviour
{
    // Time To Scale Game Object to Zero and Destroy It
    [SerializeField] float timeToDie = 2f;
    public static EnemyDie Instance;


    private void Awake()
    {
        if (Instance == null) { 
            Instance = this;
        }
    }
    public void Die(GameObject currentEnmey)
    {
        currentEnmey.transform.DOScale(Vector3.zero, timeToDie)
            .OnComplete(() => Destroy(currentEnmey));
    }



}
