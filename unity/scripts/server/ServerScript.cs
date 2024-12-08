using System;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.UI;

public class ServerScript : MonoBehaviour
{
    Socket ledSocket;
    Socket rainSocket;

    Socket ledClient;
    Socket rainClient;

    Thread ledThread;
    Thread rainThread;

    public static bool isLight = false;
    public static bool isRain = false;

    public TextMeshProUGUI ESP12Status;
    public TextMeshProUGUI ESP32Status;

    [SerializeField]
    Light[] lights;

    float[] intecities = new float[10];

    bool ledClientConnected = false;
    bool rainClientConnected = false;

    Color green50 = new Color(0, 1f, 0, 0.01f);
    Color red50 = new Color(1f, 0, 0, 0.01f);

    void Start()
    {
        ledSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        ledSocket.Bind(new IPEndPoint(IPAddress.Any, 1953));
        ledSocket.Listen(1);

        rainSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        rainSocket.Bind(new IPEndPoint(IPAddress.Any, 1924));
        rainSocket.Listen(1);

        ledThread = new Thread(LedSocketVoid) {  }; 
        ledThread.Start();

        rainThread = new Thread(RainSocketVoid) {  };
        rainThread.Start();
    }
    void OnDestroy()
    {
        if(ledClient != null) ledClient.Close();
        if(rainClient != null) rainClient.Close();
        if(ledThread != null) ledThread.Abort();
        if(rainThread != null) rainThread.Abort();
    }
    private void UpdateStatus ()
    {
        if (ledClientConnected) ESP32Status.color = green50;
        else ESP32Status.color = red50;
        if (rainClientConnected) ESP12Status.color = green50;
        else ESP12Status.color = red50;
    }
    Socket ReceiveClient(ref Socket from)
    {
        Socket toReturn = from.Accept();
        
        toReturn.ReceiveTimeout = 1500;
        toReturn.SendTimeout = 1500;
        
        return toReturn;
    }
    bool redPoleState = false;
    Stopwatch redPoleTimer = new Stopwatch();
    void UpdateLights()
    {
        if(redPoleTimer.ElapsedMilliseconds >= 500)
        {
            redPoleTimer.Restart();
            redPoleState = !redPoleState;
        }
        
        for(int i = 0; i < 10; i++)
        {
            float toMake = intecities[i];
            if (ledClientConnected && i == 7)
            {
                toMake = 6;
                if(isRain && !isLight) toMake += 3;
                if (redPoleState) lights[i].intensity = toMake;
                else lights[i].intensity = 0;
            }
            else
            {
                if (isRain && !isLight) toMake += 3;
                if (!isLight && toMake < 2 && rainClientConnected) toMake = 2;
                lights[i].intensity = toMake;
            }
        }
    }
    void LedSocketVoid()
    {
        byte[] packetGet = { (byte)ESPPrefix.Get, 100 };
        byte[] packetPost = { (byte)ESPPrefix.Post, 0, 0, 100 };
        Stopwatch updateTimer = new Stopwatch();
        Stopwatch postTiemr = new Stopwatch();
        while (true)
        {
            if (!ledClientConnected)
            {
                ledClient = ReceiveClient(ref ledSocket);
                ledClientConnected = true;
                updateTimer.Restart();
                postTiemr.Restart();
                redPoleTimer.Restart();
            }
            try
            {
                if(postTiemr.ElapsedMilliseconds >= 100)
                {
                    postTiemr.Restart();
                    if (isLight) packetPost[1] = 1;
                    else packetPost[1] = 0;
                    if (isRain) packetPost[2] = 1;
                    else packetPost[2] = 0;
                    ledClient.Send(packetPost);
                }
                if(updateTimer.ElapsedMilliseconds >= 10)
                {
                    ledClient.Send(packetGet);
                    byte[] received = new byte[10];
                    ledClient.Receive(received);
                    string test = "";
                    for (int i = 0; i < 10; i++)
                    {
                        test += received[i].ToString();
                        intecities[i] = map(received[i], 0f, 255f, 0f, 6f);
                    }
                }
            }
            catch (Exception asd)
            {
                ledClientConnected = false;
                UnityEngine.Debug.Log(asd.Message);
            }
        }
    }
    float map(byte x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
    void RainSocketVoid()
    {
        byte[] packet = { (byte)ESPPrefix.Get, 100 };
        Stopwatch updateTimer = new Stopwatch();
        while (true)
        {
            if(!rainClientConnected)
            {
                rainClient = ReceiveClient(ref rainSocket);
                rainClientConnected = true;
                updateTimer.Restart();
            }
            try
            {
                if(updateTimer.ElapsedMilliseconds >= 100)
                {
                    updateTimer.Restart();
                    rainClient.Send(packet);
                    byte[] data = new byte[6];
                    rainClient.Receive(data);
                    isLight = data[0] == 1;
                    isRain = data[1] == 1;
                }
            }
            catch (Exception asd)
            {
                UnityEngine.Debug.Log(asd.Message);
                isLight = false;
                isRain = false;
                rainClientConnected = false;
            }
        }
    }
    void Update()
    {
        UpdateLights();
        UpdateStatus();
    }
    enum ESPPrefix
    {
        Post = 1,
        Get = 2,
    }
}
