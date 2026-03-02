using UnityEngine;

public class PositionPublisher : MonoBehaviour
{
    [Header("References")]
    public Transform audioSourceTransform1;
    public Transform audioSourceTransform2;
    public Transform audioSourceTransform3;
    public Transform audioSourceTransform4;
    public Camera vrCamera;

    private OSCHub hub;

    private void Start()
    {
        hub = OSCHub.Instance;
        if (vrCamera == null) vrCamera = Camera.main;
    }

    private void Update()
    {
        if (hub == null) hub = OSCHub.Instance;
        if (hub == null) return;

        if (!audioSourceTransform1 || !audioSourceTransform2 || !audioSourceTransform3 || !audioSourceTransform4 || !vrCamera)
            return;

        SendAudioSourceData(1, audioSourceTransform1);
        SendAudioSourceData(2, audioSourceTransform2);
        SendAudioSourceData(3, audioSourceTransform3);
        SendAudioSourceData(4, audioSourceTransform4);
    }

    private void SendAudioSourceData(int index, Transform audioSourceTransform)
    {
        Vector3 relPos = vrCamera.transform.InverseTransformPoint(audioSourceTransform.position);

        hub.SendSourceRel(index, relPos);

        float azimuth = Mathf.Atan2(relPos.x, relPos.z);
        hub.SendSourceAzimuth(index, azimuth);

        float distance = relPos.magnitude;
        hub.SendSourceDistance(index, distance);

        float elevation = Mathf.Atan2(relPos.y, Mathf.Max(distance, 1e-6f));
        hub.SendSourceElevation(index, elevation);
    }
}