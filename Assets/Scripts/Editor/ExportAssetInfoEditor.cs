using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using xk_System.AssetPackage;

public class ExportAssetInfoEditor : MonoBehaviour
{
    static string extention = AssetBundlePath.ABExtention;
    static string BuildAssetPath = "Assets/ResourceABs/";
    static string CsOutPath = "Assets/Scripts/auto";

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~创建AB文件所有的信息~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [MenuItem("UnityEditor/GenerationPackage/Generation AssetInfo_1 Cs File")]
    public static void GenericAssetCSInfo()
    {
        Debug.Log("Start Generation AssetInfo Cs Info");
        CreateABCSFile();
        Debug.Log("Finish Generation AssetInfo Cs Info");
    }

    private static void CreateABCSFile()
    {
        string m = "";
        m += "#if UNITY_EDITOR\n";
        m += "namespace xk_System.AssetPackage\n{\n";
        DirectoryInfo mDir = new DirectoryInfo(BuildAssetPath);
        m += "\tpublic partial class " + mDir.Name + "Folder\n\t{\n";
        m += "\t\tpublic ResourceABsFolder()\n\t\t{\n";
        foreach (var v in mDir.GetDirectories())
        {
            m += CreateDirClass(v);
        }

        m += "\t\t\t}\n";
        m += "\t}\n";
        // m += s;
        m += "}\n";
        m += "#endif\n";
        string fileName = CsOutPath + "/" + mDir.Name + ".cs";
        StreamWriter mSw = new StreamWriter(fileName, false);
        mSw.Write(m);
        mSw.Close();
    }

    private static string CreateDirClass(DirectoryInfo mDir)
    {
        string m = "";
        int mFilesLength = 0;
        foreach (var v in mDir.GetFiles())
        {
            if (v.Extension != ".meta")
            {
                mFilesLength++;
                break;
            }
        }
        if (mFilesLength > 0)
        {
            string DirPath = GetRealPath(mDir.FullName);
            string assetPath = GetAssetPath(DirPath);
            string bundlePath = GetBundleName(DirPath);
            m += "\t\t\t\tmBundleInfoDic.Add(\"" + bundlePath + "\""+","+"\"" + assetPath + "\");\n";
        }

        if (mDir.GetDirectories().Length > 0)
        {
            foreach (var v in mDir.GetDirectories())
            {
                m += CreateDirClass(v);
            }
        }
        return m;
    }

    public static string GetRealPath(string filePath)
    {
        filePath = filePath.Replace(@"\", "/");
        return filePath;
    }

    public static string GetAssetPath(string DirFullName)
    {
        int index= DirFullName.IndexOf(BuildAssetPath);
        string path = DirFullName.Substring(index);
        if(path.EndsWith("/"))
        {
            path = path.Remove(path.Length-1);
        }
        return path;
    }

    public static string GetBundleName(string DirFullName)
    {
        if (DirFullName.EndsWith("/"))
        {
            DirFullName = DirFullName.Remove(DirFullName.Length - 1);
        }
        int index = DirFullName.IndexOf(BuildAssetPath);
        string path = DirFullName.Substring(index + BuildAssetPath.Length);
        path = path.Replace("/", "_");
        path=path.ToLower()+AssetBundlePath.ABExtention;
        return path;
    }
}
