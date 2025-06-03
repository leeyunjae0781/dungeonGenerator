using UnityEngine;

public class MonsterProjectile : MonoBehaviour
{
    public LayerMask wallLayer;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & wallLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}
