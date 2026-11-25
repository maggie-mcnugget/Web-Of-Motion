using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    public PlayerSpawnPoint spawnPoint;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Public function that other scripts can call to kill and respawn the player
    public void RespawnPlayer()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("No Player Spawn Point assigned!");
            return;
        }

        // Stop all player movement
        rb.linearVelocity = Vector2.zero;

        // Move player back to spawn position
        rb.position = spawnPoint.SpawnPosition;

        // Clear any remaining velocity
        rb.linearVelocity = Vector2.zero;
    }
}
