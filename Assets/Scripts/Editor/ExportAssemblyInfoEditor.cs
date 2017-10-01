using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Xml;
using System;

public class ExportAssemblyInfoEditor : MonoBehaviour
{
    static string OriginAssemblyPath = @"F:\KB_Client\KBClient\KBClient.csproj";
    static string TargetAssemblyPath = @"F:\KB_Client\KBClient\xk_Project.csproj";
    static string ProjectName = "Project";
    static string TargetAssemblyName = "xk_Project_" + string.Format("{0}_{1}_{2}_{3}_{4}_{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
    [MenuItem("UnityEditor/GenerationPackage/Generation HotUpdate Assembly")]
    public static void GenerationAssemblyInfo()
    {
        UnityEngine.Debug.Log("Start Generation HotUpdate Assembly");
        TargetAssemblyName = "xk_Project_" + string.Format("{0}_{1}_{2}_{3}_{4}_{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        GenerationAssembly();
        if(File.Exists(TargetAssemblyPath))
        {
            File.Delete(TargetAssemblyPath);
        }
        UnityEngine.Debug.Log("Finish Generation HotUpdate Assembly");
    }

    static void CopyAssembly()
    {
        File.Copy(OriginAssemblyPath, TargetAssemblyPath, true);
        ParseProjectXML();
    }

    static void ParseProjectXML()
    {
        FileInfo mProjectFile = new FileInfo(TargetAssemblyPath);
        FileStream mStream = mProjectFile.OpenRead();
        XmlDocument mDoc = new XmlDocument();
        mDoc.Load(mStream);
        mStream.Close();
        foreach (XmlNode x1 in mDoc.ChildNodes)
        {
            if (x1.Name.Equals(ProjectName))
            {
                XmlNode x2 = x1.FirstChild;
                foreach (XmlNode x3 in x2)
                {
                    if (x3.Name.Equals("AssemblyName"))
                    {
                        x3.InnerText = TargetAssemblyName;
                        break;
                    }
                }
            }
        }
        mDoc.Save(TargetAssemblyPath);
    }

    private static void GenerationAssembly()
    {
        CopyAssembly();
        string cmd = @"F:\KB_Client\KBClient\Assets\Scripts\Editor\bat\GenerationAssembly_MSBuild.bat";
        processCommand(cmd, "");
    }

    private static void processCommand(string command, string argument)
    {
        ProcessStartInfo start = new ProcessStartInfo(command);
        start.Arguments = argument;
        start.CreateNoWindow = false;
        start.ErrorDialog = true;
        start.UseShellExecute = true;

        if (start.UseShellExecute)
        {
            start.RedirectStandardOutput = false;
            start.RedirectStandardError = false;
            start.RedirectStandardInput = false;
        }
        else
        {
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
            start.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
        }

        Process p = Process.Start(start);

        if (!start.UseShellExecute)
        {
            //printOutPut(p.StandardOutput);
            // printOutPut(p.StandardError);
        }

        p.WaitForExit();
        p.Close();
    }


}
