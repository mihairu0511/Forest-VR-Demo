using UnityEngine;

public class Fairy : MonoBehaviour
{
    public Transform xrOrigin;
    public float orbitRadius = 2.5f;
    public float moveSpeed = 2f;
    public float directionChangeInterval = 2f;

    private Vector3 targetPosition;
    private float timer;

    void Start()
    {
        PickNewTarget();
    }

    void Update()
    {
        if (xrOrigin == null) return;

        timer += Time.deltaTime;

        if (timer >= directionChangeInterval)
        {
            PickNewTarget();
            timer = 0f;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    void PickNewTarget()
    {
        Vector3 randomDirection = Random.onUnitSphere;

        float randomDistance = Random.Range(0.5f * orbitRadius, orbitRadius);

        targetPosition = xrOrigin.position + randomDirection * randomDistance;
    }
}