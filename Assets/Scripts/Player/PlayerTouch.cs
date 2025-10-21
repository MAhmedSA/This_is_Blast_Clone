using UnityEngine;

public class PlayerTouch : MonoBehaviour
{
    void FixedUpdate()
    {
        PlayerInput();
    }

    // Function to handle trace input touch for player
    void PlayerInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // Create a ray from the camera through the touch position
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                   

                    if (hit.collider.CompareTag("Player"))
                    {
                        PlayerMovement player = hit.collider.GetComponent<PlayerMovement>();
                        if (player != null)
                        {
                            // only move if this player has not already locked
                            if (!player.HasArrived)
                            {
                                Transform freeSlot = PlayerManager.Instance.GetFreeLocation();
                                if (freeSlot != null)
                                {
                                    if (!TutorialManager.instance.IsTutorialCompleted) {
                                        TutorialManager.instance.DisapleTutorial();
                                    }
                                    PlayerManager.Instance.touchedPlayers.Add(hit.collider.gameObject);
                                    player.MoveTo(freeSlot.position, freeSlot); 
                                }
                            }
                        }
                      
                    }
                }
            }
        }
    }
}
