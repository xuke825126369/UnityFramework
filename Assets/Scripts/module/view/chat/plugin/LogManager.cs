using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class LogManager : MonoBehaviour
{
    private List<string> LogList = new List<string>();
    private List<string> LogErrorList = new List<string>();
    private List<string> LogWarningList = new List<string>();
    private List<string> LogExceptionList = new List<string>();
    private string outpath;
    void Awake()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            outpath = "F:\\outLog.txt";
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            outpath = Application.persistentDataPath + "/outLog.txt";
        }

        if (System.IO.File.Exists(outpath))
        {
            File.Delete(outpath);
        }
        Application.logMessageReceived += HandleLog;
    }

    void Update()
    {
        if (LogList.Count > 0)
        {
            lock (LogList)
            {
                foreach (string t in LogList)
                {
                    using (StreamWriter writer = new StreamWriter(outpath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(LogType.Log.ToString() + ": " + t);
                    }
                }
                LogList.Clear();
            }
        }

        if (LogWarningList.Count > 0)
        {
            lock (LogWarningList)
            {
                foreach (string t in LogWarningList)
                {
                    using (StreamWriter writer = new StreamWriter(outpath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(LogType.Warning.ToString() + ": " + t);
                    }
                }
                LogWarningList.Clear();
            }
        }

        if (LogErrorList.Count > 0)
        {
            lock (LogErrorList)
            {
                foreach (string t in LogErrorList)
                {
                    using (StreamWriter writer = new StreamWriter(outpath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(LogType.Error.ToString() + ": " + t);
                    }
                }
                LogErrorList.Clear();
            }
        }

        if (LogExceptionList.Count > 0)
        {
            lock (LogExceptionList)
            {
                foreach (string t in LogExceptionList)
                {
                    using (StreamWriter writer = new StreamWriter(outpath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(LogType.Exception.ToString() + ": " + t);
                    }
                }
                LogExceptionList.Clear();
            }
        }
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Log)
        {
            lock (LogList)
            {
                LogList.Add(logString);
            }
        }
        else if (type == LogType.Warning)
        {
            lock (LogWarningList)
            {
                LogWarningList.Add(logString);
            }
        }
        else if (type == LogType.Error)
        {
            lock (LogErrorList)
            {
                LogErrorList.Add(logString);
                errorstr = logString+"\n"+stackTrace;
            }
        }
        else if (type == LogType.Exception)
        {
            lock (LogExceptionList)
            {
                errorstr = logString + "\n" + stackTrace;
                LogExceptionList.Add(logString);
            }
        }
    }

    private string errorstr = "";

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Screen.width, 200), errorstr);
    }

}

