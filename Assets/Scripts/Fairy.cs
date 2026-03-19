using UnityEngine;

public class Fairy : MonoBehaviour
{
    public Transform xrOrigin;
    public Transform eagleTarget;

    [Header("General")]
    public float moveSmoothness = 5f;
    public bool lookAlongMovement = true;

    [Header("Figure 8 Hover")]
    public float hoverDistanceForward = 2f;
    public float hoverHeight = 0f;
    public float figure8Width = 0.8f;
    public float figure8Height = 0.4f;
    public float figure8Speed = 1.5f;
    public float hoverDuration = 10f;

    [Header("Orbit Around Player")]
    public float orbitRadius = 2.5f;
    public float orbitHeight = 1.8f;
    public float orbitSpeed = 30f;
    public int orbitLoops = 3;

    [Header("Dip Down")]
    public float dipHeight = -2f;
    public float dipDuration = 4f;

    [Header("Rise Above XR")]
    public float riseHeight = 4f;
    public float riseDuration = 6f;

    [Header("Go To Eagle")]
    public float goToEagleDuration = 3f;
    public Vector3 eagleOffset = new Vector3(0f, 1f, 0f);

    [Header("Circle Around Eagle")]
    public float eagleCircleRadius = 1.2f;
    public float eagleCircleHeight = 1f;
    public float eagleCircleSpeed = 120f;
    public int eagleCircleLoops = 2;

    [Header("Disappear")]
    public bool disableObjectAtEnd = true;

    private enum FairyState
    {
        HoverFigure8,
        Orbit,
        Dip,
        Rise,
        GoToEagle,
        CircleEagle,
        Done
    }

    private FairyState currentState = FairyState.HoverFigure8;

    private float stateTimer = 0f;
    private float orbitAngle = 0f;
    private float orbitAccumulatedAngle = 0f;

    private float eagleCircleAngle = 0f;
    private float eagleCircleAccumulatedAngle = 0f;

    private Vector3 previousPosition;
    private Vector3 transitionStartPosition;

    void Start()
    {
        previousPosition = transform.position;
        transitionStartPosition = transform.position;
    }

    void Update()
    {
        if (xrOrigin == null) return;

        stateTimer += Time.deltaTime;

        Vector3 desiredPosition = transform.position;

        switch (currentState)
        {
            case FairyState.HoverFigure8:
                desiredPosition = GetFigure8Position();
                if (stateTimer >= hoverDuration)
                {
                    ChangeState(FairyState.Orbit);
                }
                break;

            case FairyState.Orbit:
                desiredPosition = GetOrbitPosition();
                if (orbitAccumulatedAngle >= 360f * orbitLoops)
                {
                    ChangeState(FairyState.Dip);
                }
                break;

            case FairyState.Dip:
                desiredPosition = GetDipPosition();
                if (stateTimer >= dipDuration)
                {
                    ChangeState(FairyState.Rise);
                }
                break;

            case FairyState.Rise:
                desiredPosition = GetRisePosition();
                if (stateTimer >= riseDuration)
                {
                    if (eagleTarget != null)
                    {
                        ChangeState(FairyState.GoToEagle);
                    }
                    else
                    {
                        ChangeState(FairyState.Done);
                    }
                }
                break;

            case FairyState.GoToEagle:
                desiredPosition = GetGoToEaglePosition();
                if (stateTimer >= goToEagleDuration)
                {
                    ChangeState(FairyState.CircleEagle);
                }
                break;

            case FairyState.CircleEagle:
                if (eagleTarget != null)
                {
                    desiredPosition = GetCircleEaglePosition();
                    if (eagleCircleAccumulatedAngle >= 360f * eagleCircleLoops)
                    {
                        ChangeState(FairyState.Done);
                    }
                }
                else
                {
                    ChangeState(FairyState.Done);
                }
                break;

            case FairyState.Done:
                if (disableObjectAtEnd)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            moveSmoothness * Time.deltaTime
        );

        if (lookAlongMovement)
        {
            Vector3 moveDir = transform.position - previousPosition;
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(moveDir.normalized),
                    8f * Time.deltaTime
                );
            }
        }

        previousPosition = transform.position;
    }

    void ChangeState(FairyState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        transitionStartPosition = transform.position;

        if (newState == FairyState.Orbit)
        {
            orbitAngle = 0f;
            orbitAccumulatedAngle = 0f;
        }

        if (newState == FairyState.CircleEagle)
        {
            eagleCircleAngle = 0f;
            eagleCircleAccumulatedAngle = 0f;
        }
    }

    Vector3 GetFigure8Position()
    {
        Vector3 center =
            xrOrigin.position +
            xrOrigin.forward * hoverDistanceForward +
            Vector3.up * hoverHeight;

        float t = stateTimer * figure8Speed;

        float x = Mathf.Sin(t) * figure8Width;
        float y = Mathf.Sin(t * 2f) * figure8Height;

        return center + xrOrigin.right * x + Vector3.up * y;
    }

    Vector3 GetOrbitPosition()
    {
        orbitAngle += orbitSpeed * Time.deltaTime;
        orbitAccumulatedAngle += orbitSpeed * Time.deltaTime;

        float rad = orbitAngle * Mathf.Deg2Rad;

        Vector3 center = xrOrigin.position + Vector3.up * orbitHeight;

        Vector3 offset = new Vector3(
            Mathf.Cos(rad) * orbitRadius,
            0f,
            Mathf.Sin(rad) * orbitRadius
        );

        return center + offset;
    }

    Vector3 GetDipPosition()
    {
        float t = Mathf.Clamp01(stateTimer / dipDuration);

        Vector3 startPos = xrOrigin.position + Vector3.up * orbitHeight;
        Vector3 endPos = xrOrigin.position + xrOrigin.forward * 0.5f + Vector3.up * dipHeight;

        return Vector3.Lerp(startPos, endPos, t);
    }

    Vector3 GetRisePosition()
    {
        float t = Mathf.Clamp01(stateTimer / riseDuration);

        Vector3 startPos = xrOrigin.position + xrOrigin.forward * 0.5f + Vector3.up * dipHeight;
        Vector3 endPos = xrOrigin.position + Vector3.up * riseHeight;

        return Vector3.Lerp(startPos, endPos, t);
    }

    Vector3 GetGoToEaglePosition()
    {
        if (eagleTarget == null) return transform.position;

        float t = Mathf.Clamp01(stateTimer / goToEagleDuration);
        Vector3 endPos = eagleTarget.position + eagleOffset;

        return Vector3.Lerp(transitionStartPosition, endPos, t);
    }

    Vector3 GetCircleEaglePosition()
    {
        eagleCircleAngle += eagleCircleSpeed * Time.deltaTime;
        eagleCircleAccumulatedAngle += eagleCircleSpeed * Time.deltaTime;

        float rad = eagleCircleAngle * Mathf.Deg2Rad;

        Vector3 center = eagleTarget.position + Vector3.up * eagleCircleHeight;

        Vector3 offset = new Vector3(
            Mathf.Cos(rad) * eagleCircleRadius,
            Mathf.Sin(rad * 2f) * 0.2f,
            Mathf.Sin(rad) * eagleCircleRadius
        );

        return center + offset;
    }
}