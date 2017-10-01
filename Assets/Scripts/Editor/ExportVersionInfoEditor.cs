using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using UnityEditor;
using System;

public class ExportVersionInfoEditor : MonoBehaviour
{
    static string versionPath = Application.dataPath+ "/ResourceABs/version/version.xml";

    [MenuItem("UnityEditor/GenerationPackage/Generation Version Info")]
    public static void GenerationVersionInfo()
    {
        UnityEngine.Debug.Log("Start Generation VersionInfo");
        GenerationVersionFile();
        UnityEngine.Debug.Log("Finish Generation VersionInfo");
    }

    private static void GenerationVersionFile()
    {
        ParseVersionXML();
    }

    static void ParseVersionXML()
    {
        XmlDocument mDoc = new XmlDocument();
        mDoc.Load(versionPath);
        foreach (XmlNode x1 in mDoc.ChildNodes)
        {
            if (x1.Name.Equals("root"))
            {
                foreach (XmlNode x2 in x1)
                {
                    if (x2.Name.Equals("versionId"))
                    {
                        int versionId = 0;
                        int.TryParse(x2.InnerText,out versionId);
                        versionId++;
                        x2.InnerText = versionId.ToString();
                        break;
                    }
                }
            }
        }
        mDoc.Save(versionPath);
    }
}
