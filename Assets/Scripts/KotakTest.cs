using UnityEngine;

public class KotakTest : MonoBehaviour
{
    public static KotakTest Instance;
    public Vector3 respawnPosition = new Vector3(-5, 1, 0);

    private void Awake()
    {
        Instance = this;
    }

    public void Death()
    {
        transform.position = respawnPosition;
    }

    public void SetRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }
}
