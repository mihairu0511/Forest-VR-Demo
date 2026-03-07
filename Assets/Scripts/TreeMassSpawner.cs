using UnityEngine;

public class TreeMassSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform xrOrigin;
    public GameObject[] treePrefabs;

    [Header("Spawn Settings")]
    public int numberOfTrees = 50;
    public float spawnRadius = 20f;
    public Vector2 randomScaleRange = new Vector2(0.8f, 1.2f);
    public bool spawnOnStart = true;

    [Header("Ground Detection")]
    public float raycastHeight = 50f;
    public LayerMask groundLayer;

    [Header("Spacing")]
    public float minimumDistanceBetweenTrees = 2f;

    private Vector3[] spawnedPositions;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnTrees();
        }
    }

    public void SpawnTrees()
    {
        if (xrOrigin == null)
        {
            Debug.LogWarning("XR Origin is not assigned.");
            return;
        }

        if (treePrefabs == null || treePrefabs.Length == 0)
        {
            Debug.LogWarning("No tree prefabs assigned.");
            return;
        }

        spawnedPositions = new Vector3[numberOfTrees];

        int spawnedCount = 0;
        int attempts = 0;
        int maxAttempts = numberOfTrees * 20;

        while (spawnedCount < numberOfTrees && attempts < maxAttempts)
        {
            attempts++;

            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = xrOrigin.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            Vector3 rayOrigin = new Vector3(spawnPos.x, xrOrigin.position.y + raycastHeight, spawnPos.z);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                Vector3 finalPos = hit.point;

                if (IsFarEnough(finalPos, spawnedCount))
                {
                    GameObject chosenTree = treePrefabs[Random.Range(0, treePrefabs.Length)];
                    Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                    GameObject newTree = Instantiate(chosenTree, finalPos, randomRot, transform);

                    float randomScale = Random.Range(randomScaleRange.x, randomScaleRange.y);
                    newTree.transform.localScale *= randomScale;

                    spawnedPositions[spawnedCount] = finalPos;
                    spawnedCount++;
                }
            }
        }

        Debug.Log("Spawned " + spawnedCount + " trees.");
    }

    private bool IsFarEnough(Vector3 position, int currentCount)
    {
        for (int i = 0; i < currentCount; i++)
        {
            if (Vector3.Distance(position, spawnedPositions[i]) < minimumDistanceBetweenTrees)
            {
                return false;
            }
        }
        return true;
    }
}