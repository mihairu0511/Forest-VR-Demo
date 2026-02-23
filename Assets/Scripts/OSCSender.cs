using UnityEngine;
using OscJack;
using UnityEngine.XR;

public class OSCSender : MonoBehaviour
{
    [Header("References")]
    public Transform audioSourceTransform;
    public Camera vrCamera;

    [Header("OSC Settings")]
    public string host = "127.0.0.1";
    public int port = 9000;

    private OscClient client;

    void Start()
    {
        client = new OscClient(host, port);
    }

    void Update()
    {
        if (!audioSourceTransform || !vrCamera) return;

        Vector3 relPos = vrCamera.transform.InverseTransformPoint(audioSourceTransform.position);
        client.Send("/source/rel", relPos.x, relPos.y, relPos.z);

        float azimuth = Mathf.Atan2(relPos.x, relPos.z);
        client.Send("/source/azimuth", azimuth);

        float distance = relPos.magnitude;
        client.Send("/source/distance", distance);

        float elevation = Mathf.Atan2(relPos.y, distance);
        client.Send("/source/elevation", elevation);
    }

    void OnDestroy()
    {
        client?.Dispose();
    }
}
