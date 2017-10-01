using UnityEngine;
using System.Collections;
using System;
using xk_System.Debug;

public class AndroidSdkManagerTest : MonoBehaviour
{
    AndroidJavaObject mObj;

    string errorStr;
    private void Awake()
    {
        DebugSystem.Log("Sdk Init");
        try
        {
            //AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
           // AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");

            mObj = new AndroidJavaObject("com.xk.sharesdk.ShareSDK");

            if (mObj == null)
            {
                errorStr += "mObj is null\n";
                DebugSystem.LogError("mObj is null");
            }
            else
            {
                errorStr += "mObj is no null\n";
                DebugSystem.Log("mObj is no null");
            }

            int sum= mObj.Call<int>("AAA",10,15);
            errorStr += "sum1: "+sum+"\n";

            try
            {
                AndroidJavaObject mObj1 = new AndroidJavaObject("com.xk.sharesdk.BBB");

                if (mObj1 == null)
                {
                    errorStr += "mObj1 is null\n";
                    DebugSystem.LogError("mObj1 is null");
                }
                else
                {
                    errorStr += "mObj1 is no null\n";
                    DebugSystem.Log("mObj1 is no null");
                    mObj1.Set<int>("aaa", 200);
                    mObj1.Set<int>("bbb", 300);
                }
                sum = mObj.Call<int>("AAA1", mObj1);
                errorStr += "sum2: " + sum + "\n";
            }catch(Exception e)
            {
                errorStr += "xk_Exception111: " + e.Message + "\n";
                DebugSystem.LogError("xk_Exception: " + e.Message);
            }

            BBB mBBB = new BBB();
            mBBB.aaa = 100;
            mBBB.bbb = 200;
            sum = mObj.Call<int>("AAA1", mBBB);
            errorStr += "sum3: " + sum + "\n";
        }
        catch (Exception e)
        {
            errorStr += "xk_Exception: " + e.Message + "\n";
            DebugSystem.LogError("xk_Exception: " + e.Message);
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10,10,500,500), errorStr);
    }
}
public class BBB
{
    public int aaa;
    public int bbb;
}
