using UnityEngine;

public class VoidZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object entering the void is the player
        if (collision.CompareTag("Player"))
        {
            // Get the player's respawn component
            PlayerRespawn respawn =
                collision.GetComponent<PlayerRespawn>();

            // Respawn the player
            if (respawn != null)
            {
                respawn.RespawnPlayer();
            }
        }
    }
}