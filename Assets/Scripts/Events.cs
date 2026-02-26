using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using OscJack;


public class Events : MonoBehaviour
{
    [Header("Trigger")]
    public Key diveKey = Key.E;
    public Key resetKey = Key.T;
    public Key spawnBearKey = Key.Y;

    [Header("Eagle (Controlled Target)")]
    public Transform eagle;
    public Animator eagleAnim;

    [Header("XR Origin (Circle Center)")]
    public Transform xrOrigin;

    [Header("Circle Flight")]
    public float circleRadius = 6.0f;
    public float circleY = 41.5f;
    public float circleAngularSpeedDeg = 35f;
    public float circleMoveSmoothing = 10f;
    public bool faceAlongPath = true;

    [Header("Dive")]
    public float diveDuration = 2.5f;
    public float returnDuration = 1.0f;

    public float diveY = 38.0f;

    public float leaveForwardDistance = 6.0f;

    public float leaveForwardDuration = 2.5f;

    public Transform bear;
    public Vector3 bearOffset = new Vector3(0f, 0f, -2f);

    [Header("Reset")]
    public Vector3 resetPosition = new Vector3(83.5f, 41.5f, 97.8f);

    [Header("Facing")]
    public bool faceTargetWhileMoving = true;
    public float turnSpeed = 8.0f;

    [SerializeField] private Animator anim;
    [SerializeField] private string isFlyingParam = "isFlying";
    [SerializeField] private string isDiving = "isDiving";
    [SerializeField] private string isAttacking = "isAttacking";

    public float attackStartDistance = 2.0f;

    public float attackSlowMultiplier = 0.25f;

    public float attackHoldTime = 0.35f;

    [Header("OSC Settings")]
    public string host = "127.0.0.1";
    public int port = 9000;

    Coroutine _routine;
    bool _inScriptedMove;
    float _angleDeg;

    Transform EagleT => eagle != null ? eagle : transform;

    void Awake()
    {
        if (eagleAnim != null) anim = eagleAnim;

        if (anim == null)
        {
            if (eagle != null) anim = eagle.GetComponent<Animator>();
            else anim = GetComponent<Animator>();
        }
    }

    void Start()
    {
        if (xrOrigin != null)
        {
            Vector3 center = xrOrigin.position;
            Vector3 to = EagleT.position - center;
            to.y = 0f;
            if (to.sqrMagnitude > 0.0001f)
            {
                _angleDeg = Mathf.Atan2(to.z, to.x) * Mathf.Rad2Deg;
            }
        }

        if (anim != null)
        {
            anim.SetBool(isFlyingParam, true);
            anim.SetBool(isDiving, false);
            anim.SetBool(isAttacking, false);
        }

        Debug.Log($"Events running on {gameObject.name}. Eagle={(eagle != null ? eagle.name : "(self)")}, Animator={(anim != null ? anim.name : "none")}");
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (!_inScriptedMove)
        {
            TickCircleFlight();
        }

        if (Keyboard.current[diveKey].wasPressedThisFrame)
        {
            Debug.Log("Trigger pressed: " + diveKey);
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(DiveInFrontThenReturn());
        }

        if (Keyboard.current[resetKey].wasPressedThisFrame)
        {
            Debug.Log("Trigger pressed: " + resetKey);
            if (_routine != null) StopCoroutine(_routine);
            ResetEagle();
        }

        if (Keyboard.current[spawnBearKey].wasPressedThisFrame)
        {
            Debug.Log("Trigger pressed: " + spawnBearKey);
            SpawnBear();
        }
    }

    void TickCircleFlight()
    {
        if (xrOrigin == null) return;

        _angleDeg += circleAngularSpeedDeg * Time.deltaTime;
        if (_angleDeg > 360f) _angleDeg -= 360f;

        Vector3 center = xrOrigin.position;

        float rad = _angleDeg * Mathf.Deg2Rad;
        Vector3 desired = center + new Vector3(Mathf.Cos(rad) * circleRadius, 0f, Mathf.Sin(rad) * circleRadius);
        desired.y = circleY;

        if (circleMoveSmoothing <= 0f)
        {
            EagleT.position = desired;
        }
        else
        {
            float k = 1f - Mathf.Exp(-circleMoveSmoothing * Time.deltaTime);
            EagleT.position = Vector3.Lerp(EagleT.position, desired, k);
        }

        if (faceAlongPath)
        {
            float nextRad = (_angleDeg + 5f) * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(Mathf.Cos(nextRad) * circleRadius, 0f, Mathf.Sin(nextRad) * circleRadius);
            next.y = EagleT.position.y;
            FaceTowardsXZ(next);
        }
        else
        {
            Vector3 lookAt = center;
            lookAt.y = EagleT.position.y;
            FaceTowardsXZ(lookAt);
        }

        if (anim != null)
        {
            anim.SetBool(isFlyingParam, true);
            anim.SetBool(isDiving, false);
            anim.SetBool(isAttacking, false);
        }
    }

    IEnumerator DiveInFrontThenReturn()
    {
        _inScriptedMove = true;

        if (anim != null)
        {
            anim.SetBool(isDiving, true);
            anim.SetBool(isAttacking, false);
            anim.SetBool(isFlyingParam, false);
        }

        if (xrOrigin == null)
        {
            Debug.LogWarning("XR origin not assigned (needed for dive target). Returning to circle.");
            _inScriptedMove = false;
            yield break;
        }

        Vector3 center = xrOrigin.position;

        Vector3 diveTarget = center;
        diveTarget.y = diveY;

        yield return MoveToPosition(diveTarget, diveDuration, easeInOut: true, driveAttackNearXROrigin: true);

        if (anim != null)
        {
            anim.SetBool(isDiving, false);
            anim.SetBool(isAttacking, false);
            anim.SetBool(isFlyingParam, true);
        }

        Vector3 approachDir = (center - EagleT.position);
        approachDir.y = 0f;
        if (approachDir.sqrMagnitude < 0.0001f)
        {
            approachDir = EagleT.forward;
            approachDir.y = 0f;
        }
        approachDir.Normalize();

        Vector3 leaveForwardPoint = EagleT.position + approachDir * leaveForwardDistance;
        leaveForwardPoint.y = circleY;
        yield return MoveToPosition(leaveForwardPoint, leaveForwardDuration, easeInOut: true);

        Vector3 to = EagleT.position - center;
        to.y = 0f;
        if (to.sqrMagnitude > 0.0001f)
        {
            _angleDeg = Mathf.Atan2(to.z, to.x) * Mathf.Rad2Deg;
        }

        float rad = _angleDeg * Mathf.Deg2Rad;
        Vector3 returnPoint = center + new Vector3(Mathf.Cos(rad) * circleRadius, 0f, Mathf.Sin(rad) * circleRadius);
        returnPoint.y = circleY;

        yield return MoveToPosition(returnPoint, returnDuration, easeInOut: true);

        _inScriptedMove = false;
    }

    IEnumerator MoveToPosition(Vector3 target, float duration, bool easeInOut, bool driveAttackNearXROrigin = false)
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
                inAttackRange = dNow <= attackStartDistance;
            }

            float timeMult = 1f;
            if (driveAttackNearXROrigin && inAttackRange)
                timeMult = Mathf.Clamp01(attackSlowMultiplier);

            t += Time.deltaTime * timeMult;
            float u = Mathf.Clamp01(t / duration);
            if (easeInOut) u = u * u * (3f - 2f * u);

            EagleT.position = Vector3.Lerp(start, target, u);

            if (driveAttackNearXROrigin && xrOrigin != null && anim != null)
            {
                float d = Vector3.Distance(EagleT.position, xrOrigin.position);
                bool shouldAttack = d <= attackStartDistance;

                anim.SetBool(isAttacking, shouldAttack);
                anim.SetBool(isDiving, !shouldAttack);
                anim.SetBool(isFlyingParam, false);

                if (shouldAttack)
                {
                    FaceTowardsXZ(xrOrigin.position);

                    attackTimer += Time.deltaTime;
                    if (attackTimer >= attackHoldTime)
                    {
                        break;
                    }
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

        if (!(driveAttackNearXROrigin && xrOrigin != null && anim != null && Vector3.Distance(EagleT.position, xrOrigin.position) <= attackStartDistance))
        {
            EagleT.position = target;
        }
    }

    void ResetEagle()
    {
        EagleT.position = resetPosition;

        if (xrOrigin != null)
        {
            Vector3 center = xrOrigin.position;
            Vector3 to = EagleT.position - center;
            to.y = 0f;
            if (to.sqrMagnitude > 0.0001f)
            {
                _angleDeg = Mathf.Atan2(to.z, to.x) * Mathf.Rad2Deg;
            }
        }

        _inScriptedMove = false;

        if (anim != null)
        {
            anim.SetBool(isFlyingParam, true);
            anim.SetBool(isDiving, false);
            anim.SetBool(isAttacking, false);
        }
    }

    void FaceTowardsXZ(Vector3 targetPos)
    {
        if (!faceTargetWhileMoving) return;

        Vector3 dir = targetPos - EagleT.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        EagleT.rotation = Quaternion.Slerp(EagleT.rotation, targetRot, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
    }

    void SpawnBear()
    {
        if (bear == null || xrOrigin == null)
        {
            Debug.LogWarning("Bear reference or XR origin not assigned.");
            return;
        }

        Vector3 spawnPos = xrOrigin.position + xrOrigin.TransformDirection(bearOffset);
        bear.position = spawnPos;

        Vector3 lookDir = xrOrigin.position - bear.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            bear.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        }

        Animator bearAnim = bear.GetComponentInChildren<Animator>();
        if (bearAnim != null && bearAnim.HasParameterOfType("roar", AnimatorControllerParameterType.Trigger))
        {
            bearAnim.SetTrigger("roar");
        }
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