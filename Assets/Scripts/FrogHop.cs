using UnityEngine;
using System.Collections;

public class FrogHop : MonoBehaviour
{
    public Transform xrOrigin;

    [Header("Approach")]
    public float stopDistance = 2f;

    [Header("Hop Motion")]
    public float hopDistance = 1.2f;
    public float hopHeight = 0.5f;
    public float hopDuration = 0.4f;
    public float waitBetweenHops = 0.6f;

    [Header("Around Player")]
    public float aroundRadius = 2.5f;
    public float randomOffset = 1.2f;

    private bool isHopping = false;

    [Header("OSC Settings")]
    public string oscFrog = "/event/frog";

    void Start()
    {
        if (xrOrigin == null)
        {
            Debug.LogError("FrogHop: XR Origin is not assigned.");
            return;
        }
        OSCHub.Instance?.SendInt(oscFrog, 0);
        StartCoroutine(HopRoutine());
    }

    IEnumerator HopRoutine()
    {
        while (true)
        {
            if (!isHopping)
            {
                Vector3 targetPoint;
                float distToXR = Vector3.Distance(
                    new Vector3(transform.position.x, 0, transform.position.z),
                    new Vector3(xrOrigin.position.x, 0, xrOrigin.position.z)
                );

                if (distToXR > stopDistance)
                {
                    Vector3 dir = (xrOrigin.position - transform.position);
                    dir.y = 0f;
                    dir.Normalize();

                    targetPoint = transform.position + dir * hopDistance;
                }
                else
                {
                    Vector2 circle = Random.insideUnitCircle.normalized * aroundRadius;
                    Vector3 aroundTarget = xrOrigin.position + new Vector3(circle.x, 0f, circle.y);

                    Vector3 randomJitter = new Vector3(
                        Random.Range(-randomOffset, randomOffset),
                        0f,
                        Random.Range(-randomOffset, randomOffset)
                    );

                    targetPoint = aroundTarget + randomJitter;
                }

                yield return StartCoroutine(HopTo(targetPoint));
                yield return new WaitForSeconds(waitBetweenHops);
            }

            yield return null;
        }
    }

    IEnumerator HopTo(Vector3 targetPos)
    {
        isHopping = true;

        Vector3 startPos = transform.position;
        targetPos.y = startPos.y;

        Vector3 flatDir = targetPos - startPos;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude > 0.001f)
        {
            transform.forward = flatDir.normalized;
        }

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / hopDuration;
            float clampedT = Mathf.Clamp01(t);

            Vector3 pos = Vector3.Lerp(startPos, targetPos, clampedT);
            pos.y += Mathf.Sin(clampedT * Mathf.PI) * hopHeight;

            transform.position = pos;

            yield return null;
        }

        transform.position = targetPos;
        isHopping = false;
        OSCHub.Instance?.SendInt(oscFrog, 1);
    }
}