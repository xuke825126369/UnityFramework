using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace xk_System.Debug
{
    //用来打印日志文件或写入日志文件
    public static class DebugSystem
    {
        public static void Log(object s)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            UnityEngine.Debug.Log(s);
#else
           // Console.WriteLine(s);
#endif
        }
        public static void LogWarning(object s)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            UnityEngine.Debug.LogWarning(s);
#else
           // Console.WriteLine(s);
#endif
        }

        public static void LogError(object s)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            UnityEngine.Debug.LogError(s);
#else
            //Console.WriteLine(s);
#endif
        }

        public static void LogBitStream(string tag, byte[] stream)
        {
            StringBuilder aaStr =new StringBuilder();
            aaStr.Append("<color=red>");
            aaStr.Append(tag + ": ");
            aaStr.Append("</color>");
            aaStr.Append("<color=yellow>");
            foreach (byte b in stream)
            {
                aaStr.Append(b + " | ");
            }
            aaStr.Append("</color>");
            DebugSystem.Log(aaStr);
        }

        public static void LogErrorBitStream(string tag, byte[] stream)
        {
            StringBuilder aaStr = new StringBuilder();
            aaStr.Append("<color=red>");
            aaStr.Append(tag+": ");
            aaStr.Append("</color>");
            aaStr.Append("<color=yellow>");
            foreach (byte b in stream)
            {
                aaStr.Append(b + " | ");
            }
            aaStr.Append("</color>");
            DebugSystem.LogError(aaStr);
        }
        /// <summary>
        /// 红：red
        /// 绿：green
        /// 蓝：blue
        /// </summary>
        /// <param name="data"></param>
        /// <param name="color"></param>
        public static void LogColor(object data,string color)
        {          
            string aaStr= "<color="+color+">";
            aaStr += data.ToString();
            aaStr += "</color>";
            DebugSystem.Log(aaStr);
        }

        public static void LogColor(object data)
        {
            string aaStr = "<color=" + "yellow" + ">";
            aaStr += data.ToString();
            aaStr += "</color>";
            DebugSystem.Log(aaStr);
        }

    }

    public class LogManager:MonoBehaviour
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
                outpath = "G:\\outLog.txt";
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                outpath = Application.persistentDataPath + "/outLog.txt";
            }

            if (System.IO.File.Exists(outpath))
            {
                File.Delete(outpath);
            }
            Application.logMessageReceived+= HandleLog;
        }

        void Update()
        {
            if (LogList.Count > 0)
            {
                lock(LogList)
                {
                    foreach (string t in LogList)
                    {
                        using (StreamWriter writer = new StreamWriter(outpath,true, Encoding.UTF8))
                        {
                            writer.WriteLine(LogType.Log.ToString() + ": " + t);
                        }
                    }
                    LogList.Clear();
                }
            }

            if (LogWarningList.Count > 0)
            {
                lock(LogWarningList)
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
                lock(LogErrorList)
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
                lock(LogExceptionList)
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
                lock(LogList)
                {
                    LogList.Add(logString);
                }
            }
            else if (type == LogType.Warning)
            {
                lock(LogWarningList)
                {
                    LogWarningList.Add(logString);
                }
            }
            else if (type == LogType.Error)
            {
                lock(LogErrorList)
                {
                    LogErrorList.Add(logString);
                    errorstr = logString;
                    LogErrorList.Add(stackTrace);
                }
            }
            else if (type == LogType.Exception)
            {
                lock(LogExceptionList)
                {
                    errorstr = logString;
                    LogExceptionList.Add(logString);
                    LogExceptionList.Add(stackTrace);
                }
            }
        }

        private string errorstr = "";
        private void OnGUI()
        {
            GUI.Label(new Rect(0,0,Screen.width,100),errorstr);
        }

    }
}