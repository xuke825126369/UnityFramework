using UnityEngine;
using System.Collections;

public class PingView : MonoBehaviour
{
    public string ip = "192.168.3.30";
    private Ping ping;
    private int time;


    public void InitPingView(string ip)
    {
        this.ip = ip;
        StartCoroutine(pingIP());
    }

    public IEnumerator pingIP()
    {
        while (true)
        {
            ping = new Ping(ip);
            while (!ping.isDone)
            {
                yield return 0;
            }
            time = ping.time;
            ping.DestroyPing();
            yield return new WaitForSeconds(1f);            
        }
    }
}
