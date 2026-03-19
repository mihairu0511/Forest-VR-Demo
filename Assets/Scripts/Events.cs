using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Events : MonoBehaviour
{
    [Header("Auto Sequence")]
    public bool playOnStart = true;
    public bool loopSequence = true;
    public float delayBeforeRestart = 4f;

    [Header("References")]
    public Transform xrOrigin;

    [Header("Fairy")]
    public Transform fairy;
    public GameObject fairyObject;
    public Animator fairyAnim;

    [Header("Fairy - General")]
    public float fairyMoveSmoothness = 5f;
    public bool fairyLookAlongMovement = true;

    [Header("Fairy - Figure 8 Hover")]
    public float fairyHoverDistanceForward = 2f;
    public float fairyHoverHeight = 0f;
    public float fairyFigure8Width = 0.8f;
    public float fairyFigure8Height = 0.4f;
    public float fairyFigure8Speed = 1.5f;
    public float fairyHoverDuration = 10f;

    [Header("Fairy - Orbit Around Player")]
    public float fairyOrbitRadius = 2.5f;
    public float fairyOrbitHeight = 1.8f;
    public float fairyOrbitSpeed = 30f;
    public int fairyOrbitLoops = 3;

    [Header("Fairy - Dip")]
    public float fairyDipHeight = -2f;
    public float fairyDipDuration = 4f;

    [Header("Fairy - Rise")]
    public float fairyRiseHeight = 4f;
    public float fairyRiseDuration = 6f;

    [Header("Fairy - Eagle Attention")]
    public float fairyGoToEagleDuration = 3f;
    public Vector3 fairyEagleOffset = new Vector3(0f, 1f, 0f);
    public float fairyEagleCircleRadius = 1.2f;
    public float fairyEagleCircleHeight = 1f;
    public float fairyEagleCircleSpeed = 120f;
    public int fairyEagleCircleLoops = 2;

    [Header("Eagle")]
    public Transform eagle;
    public Animator eagleAnim;
    public Vector3 eagleGroundStartPosition = new Vector3(83.5f, 31f, 97.8f);
    public float eagleFlyUpDuration = 3f;

    [Header("Eagle Circle Flight")]
    public float eagleCircleRadius = 6.0f;
    public float eagleCircleY = 41.5f;
    public float eagleCircleAngularSpeedDeg = 35f;
    public int eagleCircleLoops = 3;

    [Header("Eagle Dive")]
    public float eagleDiveDuration = 2.5f;
    public float eagleDiveY = 38.0f;
    public float eagleAttackStartDistance = 2.0f;
    public float eagleAttackSlowMultiplier = 0.25f;
    public float eagleAttackHoldTime = 0.35f;

    [Header("Eagle Fly Away")]
    public float eagleFlyAwayDistance = 80f;
    public float eagleFlyAwayHeightOffset = 30f;
    public float eagleFlyAwayDuration = 5f;

    [Header("Bear")]
    public Transform bear;
    public Vector3 bearHomePosition = new Vector3(76f, 31f, 100f);
    public float bearPreRoarDelay = 1.5f;
    public float bearRunToXRDuration = 3.0f;
    public float bearWaitAtXRBeforeRoar = 0.4f;
    public float bearReturnDuration = 3.0f;
    public Vector3 bearOffsetFromXR = new Vector3(0f, 0f, -2f);

    [Header("Facing")]
    public bool faceTargetWhileMoving = true;
    public float turnSpeed = 8.0f;

    [Header("Animator Parameters")]
    [SerializeField] private Animator anim;
    [SerializeField] private string isFlyingParam = "isFlying";
    [SerializeField] private string isDiving = "isDiving";
    [SerializeField] private string isAttacking = "isAttacking";
    [SerializeField] private string isTakingOff = "isTakingOff";
    [SerializeField] private string isRunning = "isRunning";
    [SerializeField] private string isResting = "isResting";
    [SerializeField] private string roarTrigger = "roar";

    [Header("OSC Addresses")]
    public string oscEagle = "/event/eagle";
    public string oscBear = "/event/bear";
    public string oscFairy = "/event/fairy";

    private Coroutine masterRoutine;
    private Vector3 fairyPreviousPosition;

    Transform EagleT => eagle != null ? eagle : transform;

    void Awake()
    {
        if (eagleAnim != null)
            anim = eagleAnim;

        if (anim == null)
        {
            if (eagle != null) anim = eagle.GetComponent<Animator>();
            else anim = GetComponent<Animator>();
        }

        if (fairyObject == null && fairy != null)
            fairyObject = fairy.gameObject;
    }

    void Start()
    {
        if (playOnStart)
        {
            masterRoutine = StartCoroutine(MasterSequenceLoop());
        }
    }

    IEnumerator MasterSequenceLoop()
    {
        do
        {
            yield return RunWholeSequence();

            if (loopSequence)
                yield return new WaitForSeconds(delayBeforeRestart);

        } while (loopSequence);
    }

    IEnumerator RunWholeSequence()
    {
        ResetActorsToStart();
        yield return RunFairySequence();
        anim.SetBool(isTakingOff, true);
        OSCHub.Instance?.SendInt(oscEagle, 1);
        yield return RunEagleSequence();
        yield return RunBearSequence();
    }

    void ResetActorsToStart()
    {
        if (fairyObject != null)
            fairyObject.SetActive(true);

        if (fairy != null && xrOrigin != null)
        {
            fairy.position = xrOrigin.position
                             + xrOrigin.forward * fairyHoverDistanceForward
                             + Vector3.up * fairyHoverHeight;

            fairyPreviousPosition = fairy.position;
        }

        if (eagle != null)
        {
            eagle.gameObject.SetActive(true);
            eagle.position = eagleGroundStartPosition;
        }

        if (bear != null)
        {
            bear.gameObject.SetActive(true);
            bear.position = bearHomePosition;
        }

        if (anim != null)
        {
            anim.SetBool(isFlyingParam, false);
            anim.SetBool(isDiving, false);
            anim.SetBool(isAttacking, false);
        }

        Animator bearAnim = GetBearAnimator();
        if (bearAnim != null)
        {
            bearAnim.SetBool(isRunning, false);
            bearAnim.SetBool(isResting, true);
        }

        OSCHub.Instance?.SendInt(oscFairy, 0);
        OSCHub.Instance?.SendInt(oscEagle, 0);
        OSCHub.Instance?.SendInt(oscBear, 0);
    }

    IEnumerator RunFairySequence()
    {
        if (fairy == null || xrOrigin == null)
            yield break;

        if (fairyObject != null)
            fairyObject.SetActive(true);

        OSCHub.Instance?.SendInt(oscFairy, 1);

        float t = 0f;
        while (t < fairyHoverDuration)
        {
            t += Time.deltaTime;
            Vector3 desired = GetFairyFigure8Position(t);
            MoveFairy(desired);
            yield return null;
        }

        float orbitAngle = 0f;
        float orbitAccumulated = 0f;
        while (orbitAccumulated < 360f * fairyOrbitLoops)
        {
            orbitAngle += fairyOrbitSpeed * Time.deltaTime;
            orbitAccumulated += fairyOrbitSpeed * Time.deltaTime;

            Vector3 desired = GetFairyOrbitPosition(orbitAngle);
            MoveFairy(desired);
            yield return null;
        }

        Vector3 dipStart = fairy.position;
        Vector3 dipEnd = xrOrigin.position + xrOrigin.forward * 0.5f + Vector3.up * fairyDipHeight;
        yield return MoveFairyOverTime(dipStart, dipEnd, fairyDipDuration);

        Vector3 riseStart = fairy.position;
        Vector3 riseEnd = xrOrigin.position + Vector3.up * fairyRiseHeight;
        yield return MoveFairyOverTime(riseStart, riseEnd, fairyRiseDuration);

        if (eagle != null)
        {
            Vector3 eagleAttentionPoint = eagle.position + fairyEagleOffset;
            yield return MoveFairyOverTime(fairy.position, eagleAttentionPoint, fairyGoToEagleDuration);

            float eagleCircleAngle = 0f;
            float eagleCircleAccum = 0f;
            while (eagleCircleAccum < 360f * fairyEagleCircleLoops)
            {
                eagleCircleAngle += fairyEagleCircleSpeed * Time.deltaTime;
                eagleCircleAccum += fairyEagleCircleSpeed * Time.deltaTime;

                float rad = eagleCircleAngle * Mathf.Deg2Rad;
                Vector3 center = eagle.position + Vector3.up * fairyEagleCircleHeight;

                Vector3 offset = new Vector3(
                    Mathf.Cos(rad) * fairyEagleCircleRadius,
                    Mathf.Sin(rad * 2f) * 0.2f,
                    Mathf.Sin(rad) * fairyEagleCircleRadius
                );

                Vector3 desired = center + offset;
                MoveFairy(desired);
                yield return null;
            }
        }

        OSCHub.Instance?.SendInt(oscFairy, 0);
        if (fairyObject != null)
            fairyObject.SetActive(false);
    }

    Vector3 GetFairyFigure8Position(float timeValue)
    {
        Vector3 center =
            xrOrigin.position +
            xrOrigin.forward * fairyHoverDistanceForward +
            Vector3.up * fairyHoverHeight;

        float tt = timeValue * fairyFigure8Speed;
        float x = Mathf.Sin(tt) * fairyFigure8Width;
        float y = Mathf.Sin(tt * 2f) * fairyFigure8Height;

        return center + xrOrigin.right * x + Vector3.up * y;
    }

    Vector3 GetFairyOrbitPosition(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 center = xrOrigin.position + Vector3.up * fairyOrbitHeight;
        Vector3 offset = new Vector3(
            Mathf.Cos(rad) * fairyOrbitRadius,
            0f,
            Mathf.Sin(rad) * fairyOrbitRadius
        );
        return center + offset;
    }

    void MoveFairy(Vector3 desired)
    {
        if (fairy == null) return;

        fairy.position = Vector3.Lerp(
            fairy.position,
            desired,
            fairyMoveSmoothness * Time.deltaTime
        );

        if (fairyLookAlongMovement)
        {
            Vector3 moveDir = fairy.position - fairyPreviousPosition;
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
                fairy.rotation = Quaternion.Slerp(fairy.rotation, targetRot, 8f * Time.deltaTime);
            }
        }

        fairyPreviousPosition = fairy.position;
    }

    IEnumerator MoveFairyOverTime(Vector3 start, Vector3 end, float duration)
    {
        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);

        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            u = u * u * (3f - 2f * u);

            Vector3 desired = Vector3.Lerp(start, end, u);
            MoveFairy(desired);
            yield return null;
        }

        fairy.position = end;
        fairyPreviousPosition = fairy.position;
    }

    IEnumerator RunEagleSequence()
    {
        if (eagle == null || xrOrigin == null)
            yield break;

        eagle.gameObject.SetActive(true);

        Vector3 center = xrOrigin.position;
        Vector3 circleStart = center + new Vector3(eagleCircleRadius, 0f, 0f);
        circleStart.y = eagleCircleY;

        if (anim != null)
        {
            anim.SetBool(isFlyingParam, false);
            anim.SetBool(isDiving, false);
            anim.SetBool(isAttacking, false);
        }

        yield return MoveEagleToPosition(circleStart, eagleFlyUpDuration, true, false);

        float angleDeg = 0f;
        float accumulated = 0f;

        while (accumulated < 360f * eagleCircleLoops)
        {
            angleDeg += eagleCircleAngularSpeedDeg * Time.deltaTime;
            accumulated += eagleCircleAngularSpeedDeg * Time.deltaTime;

            float rad = angleDeg * Mathf.Deg2Rad;
            Vector3 desired = center + new Vector3(
                Mathf.Cos(rad) * eagleCircleRadius,
                0f,
                Mathf.Sin(rad) * eagleCircleRadius
            );
            desired.y = eagleCircleY;

            EagleT.position = Vector3.Lerp(EagleT.position, desired, 1f - Mathf.Exp(-10f * Time.deltaTime));

            float nextRad = (angleDeg + 5f) * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(
                Mathf.Cos(nextRad) * eagleCircleRadius,
                0f,
                Mathf.Sin(nextRad) * eagleCircleRadius
            );
            next.y = EagleT.position.y;
            FaceTowardsXZ(next);

            if (anim != null)
            {
                anim.SetBool(isFlyingParam, true);
                anim.SetBool(isDiving, false);
                anim.SetBool(isAttacking, false);
            }

            yield return null;
        }

        Vector3 diveTarget = center;
        diveTarget.y = eagleDiveY;

        OSCHub.Instance?.SendInt(oscEagle, 2);

        if (anim != null)
        {
            anim.SetBool(isFlyingParam, false);
            anim.SetBool(isDiving, true);
            anim.SetBool(isAttacking, false);
        }

        yield return MoveEagleToPosition(diveTarget, eagleDiveDuration, true, true);

        Vector3 awayDir = (EagleT.position - xrOrigin.position);
        awayDir.y = 0f;
        if (awayDir.sqrMagnitude < 0.0001f)
            awayDir = xrOrigin.forward;
        awayDir.Normalize();

        Vector3 farAway = EagleT.position + awayDir * eagleFlyAwayDistance + Vector3.up * eagleFlyAwayHeightOffset;

        if (anim != null)
        {
            anim.SetBool(isFlyingParam, true);
            anim.SetBool(isDiving, false);
            anim.SetBool(isAttacking, false);
        }

        OSCHub.Instance?.SendInt(oscEagle, 1);
        yield return MoveEagleToPosition(farAway, eagleFlyAwayDuration, true, false);

        eagle.gameObject.SetActive(false);
        OSCHub.Instance?.SendInt(oscEagle, 0);
    }

    IEnumerator MoveEagleToPosition(Vector3 target, float duration, bool easeInOut, bool driveAttackNearXROrigin)
    {
        Vector3 start = EagleT.position;

        if (duration <= 0f)
        {
            EagleT.position = target;
            yield break;
        }

        float t = 0f;
        float attackTimer = 0f;

        while (t < duration)
        {
            bool inAttackRange = false;
            if (driveAttackNearXROrigin && xrOrigin != null)
            {
                float dNow = Vector3.Distance(EagleT.position, xrOrigin.position);
                inAttackRange = dNow <= eagleAttackStartDistance;
            }

            float timeMult = 1f;
            if (driveAttackNearXROrigin && inAttackRange)
                timeMult = Mathf.Clamp01(eagleAttackSlowMultiplier);

            t += Time.deltaTime * timeMult;
            float u = Mathf.Clamp01(t / duration);
            if (easeInOut) u = u * u * (3f - 2f * u);

            EagleT.position = Vector3.Lerp(start, target, u);

            if (driveAttackNearXROrigin && xrOrigin != null && anim != null)
            {
                float d = Vector3.Distance(EagleT.position, xrOrigin.position);
                bool shouldAttack = d <= eagleAttackStartDistance;

                anim.SetBool(isAttacking, shouldAttack);
                anim.SetBool(isDiving, !shouldAttack);
                anim.SetBool(isFlyingParam, false);

                if (shouldAttack)
                {
                    FaceTowardsXZ(xrOrigin.position);

                    attackTimer += Time.deltaTime;
                    if (attackTimer >= eagleAttackHoldTime)
                        break;
                }
                else
                {
                    attackTimer = 0f;
                    FaceTowardsXZ(target);
                }
            }
            else
            {
                FaceTowardsXZ(target);
            }

            yield return null;
        }

        if (!(driveAttackNearXROrigin && xrOrigin != null && anim != null &&
              Vector3.Distance(EagleT.position, xrOrigin.position) <= eagleAttackStartDistance))
        {
            EagleT.position = target;
        }
    }

    IEnumerator RunBearSequence()
    {
        if (bear == null || xrOrigin == null)
            yield break;

        Animator bearAnim = GetBearAnimator();
        if (bearAnim == null)
            yield break;

        bear.gameObject.SetActive(true);

        bearAnim.SetBool(isRunning, false);
        bearAnim.SetBool(isResting, true);
        bearAnim.SetTrigger(roarTrigger);
        OSCHub.Instance?.SendInt(oscBear, 2);

        yield return new WaitForSeconds(bearPreRoarDelay);

        Vector3 xrTarget = xrOrigin.position + bearOffsetFromXR;
        xrTarget.y = bear.position.y;

        bearAnim.SetBool(isResting, false);
        bearAnim.SetBool(isRunning, true);
        OSCHub.Instance?.SendInt(oscBear, 1);

        yield return MoveBearToPosition(xrTarget, bearRunToXRDuration);

        bearAnim.SetBool(isRunning, false);
        bearAnim.SetBool(isResting, true);
        yield return new WaitForSeconds(bearWaitAtXRBeforeRoar);

        bearAnim.SetTrigger(roarTrigger);
        OSCHub.Instance?.SendInt(oscBear, 2);

        yield return new WaitForSeconds(1.5f);

        bearAnim.SetBool(isResting, false);
        bearAnim.SetBool(isRunning, true);
        OSCHub.Instance?.SendInt(oscBear, 1);

        yield return MoveBearToPosition(bearHomePosition, bearReturnDuration);

        bearAnim.SetBool(isRunning, false);
        bearAnim.SetBool(isResting, true);
        OSCHub.Instance?.SendInt(oscBear, 0);
    }

    IEnumerator MoveBearToPosition(Vector3 target, float duration)
    {
        Vector3 start = bear.position;
        duration = Mathf.Max(0.0001f, duration);
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            u = u * u * (3f - 2f * u);

            bear.position = Vector3.Lerp(start, target, u);

            Vector3 dir = target - bear.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                bear.rotation = Quaternion.Slerp(bear.rotation, targetRot, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
            }

            yield return null;
        }

        bear.position = target;
    }

    Animator GetBearAnimator()
    {
        if (bear == null) return null;
        return bear.GetComponentInChildren<Animator>();
    }

    void FaceTowardsXZ(Vector3 targetPos)
    {
        if (!faceTargetWhileMoving) return;

        Vector3 dir = targetPos - EagleT.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        EagleT.rotation = Quaternion.Slerp(
            EagleT.rotation,
            targetRot,
            1f - Mathf.Exp(-turnSpeed * Time.deltaTime)
        );
    }
}

public static class AnimatorExtensions
{
    public static bool HasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
    {
        foreach (var param in self.parameters)
        {
            if (param.type == type && param.name == name)
                return true;
        }
        return false;
    }
}