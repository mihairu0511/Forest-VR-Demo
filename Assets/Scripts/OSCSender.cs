 using UnityEngine;
using OscJack;
using UnityEngine.XR;

public class OSCSender : MonoBehaviour
{
    [Header("References")]
    public Transform audioSourceTransform1;
    public Transform audioSourceTransform2;
    public Transform audioSourceTransform3;
    public Transform audioSourceTransform4;
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
        if (!audioSourceTransform1 || !audioSourceTransform2 || !audioSourceTransform3 || !audioSourceTransform4 || !vrCamera) return;

        SendAudioSourceData1(audioSourceTransform1);
        SendAudioSourceData2(audioSourceTransform2);
        SendAudioSourceData3(audioSourceTransform3);
        SendAudioSourceData4(audioSourceTransform4);
        
    }

    void SendAudioSourceData1(Transform audioSourceTransform)
    {
        Vector3 relPos1 = vrCamera.transform.InverseTransformPoint(audioSourceTransform.position);
        client.Send("/source/rel1", relPos1.x, relPos1.y, relPos1.z);

        float azimuth1 = Mathf.Atan2(relPos1.x, relPos1.z);
        client.Send("/source/azimuth1", azimuth1); 

        float distance1 = relPos1.magnitude;
        client.Send("/source/distance1", distance1);

        float elevation1 = Mathf.Atan2(relPos1.y, distance1);
        client.Send("/source/elevation1", elevation1);
    }

    void SendAudioSourceData2(Transform audioSourceTransform)
    {
        Vector3 relPos2 = vrCamera.transform.InverseTransformPoint(audioSourceTransform.position);
        client.Send("/source/rel2", relPos2.x, relPos2.y, relPos2.z);

        float azimuth2 = Mathf.Atan2(relPos2.x, relPos2.z);
        client.Send("/source/azimuth2", azimuth2); 

        float distance2 = relPos2.magnitude;
        client.Send("/source/distance2", distance2);

        float elevation2 = Mathf.Atan2(relPos2.y, distance2);
        client.Send("/source/elevation2", elevation2);
    }

    void SendAudioSourceData3(Transform audioSourceTransform)
    {
        Vector3 relPos3 = vrCamera.transform.InverseTransformPoint(audioSourceTransform.position);
        client.Send("/source/rel3", relPos3.x, relPos3.y, relPos3.z);

        float azimuth3 = Mathf.Atan2(relPos3.x, relPos3.z);
        client.Send("/source/azimuth3", azimuth3); 

        float distance3 = relPos3.magnitude;
        client.Send("/source/distance3", distance3);

        float elevation3 = Mathf.Atan2(relPos3.y, distance3);
        client.Send("/source/elevation3", elevation3);
    }
    
    void SendAudioSourceData4(Transform audioSourceTransform)
    {
        Vector3 relPos4 = vrCamera.transform.InverseTransformPoint(audioSourceTransform.position);
        client.Send("/source/rel4", relPos4.x, relPos4.y, relPos4.z);

        float azimuth4 = Mathf.Atan2(relPos4.x, relPos4.z);
        client.Send("/source/azimuth4", azimuth4); 

        float distance4 = relPos4.magnitude;
        client.Send("/source/distance4", distance4);

        float elevation4 = Mathf.Atan2(relPos4.y, distance4);
        client.Send("/source/elevation4", elevation4);
    }

    void OnDestroy()
    {
        client?.Dispose();
    }
}
