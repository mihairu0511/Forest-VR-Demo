using UnityEngine;
using OscJack;

public class OSCHub : MonoBehaviour
{
    public static OSCHub Instance { get; private set; }

    [Header("OSC Settings")]
    public string host = "127.0.0.1";
    public int port = 9000;

    private OscClient client;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        client = new OscClient(host, port);
    }

    public void SendSourceRel(int index, Vector3 relPos)
    {
        client.Send($"/source/rel{index}", relPos.x, relPos.y, relPos.z);
    }

    public void SendSourceAzimuth(int index, float azimuth)
    {
        client.Send($"/source/azimuth{index}", azimuth);
    }

    public void SendSourceDistance(int index, float distance)
    {
        client.Send($"/source/distance{index}", distance);
    }

    public void SendSourceElevation(int index, float elevation)
    {
        client.Send($"/source/elevation{index}", elevation);
    }

    public void SendInt(string address, int value)
    {
        if (client == null) return;
        client.Send(address, value);
    }

    private void OnDestroy()
    {
        client?.Dispose();
        client = null;
        if (Instance == this) Instance = null;
    }
}