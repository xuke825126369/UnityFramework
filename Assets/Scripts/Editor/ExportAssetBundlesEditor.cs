using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using xk_System.Crypto;
using System.Xml;
using xk_System.AssetPackage;
using System;

public class ExportAssetBundlesEditor : MonoBehaviour
{
    static string extention = AssetBundlePath.ABExtention;
    static string BuildAssetPath = "Assets/ResourceABs";
    static string OutAssetPath = Application.streamingAssetsPath;
    static List<AssetBundleBuild> mBundleList = new List<AssetBundleBuild>();

    [MenuItem("UnityEditor/BuildAllPackage")]
    static void BuildABs()
    {
        Debug.Log("Start Build AssetBundles");
        Init();
        MakeAtlasTools.MakeAllAtlas();
        //ExportAssemblyInfoEditor.GenerationAssemblyInfo();
        ExportAssetInfoEditor.GenericAssetCSInfo();
        ExportVersionInfoEditor.GenerationVersionInfo();
        CreateAssetBundleBuilds();
        Debug.Log("Finish Build AssetBundle");
    }

    static void Init()
    {
        mBundleList.Clear();
        ClearFolder();
    }

    static void ClearFolder()
    {
        string path = OutAssetPath;
        DirectoryInfo mdir = new DirectoryInfo(path);
        foreach(FileInfo f in mdir.GetFiles())
        {
            f.Delete();
        }
    }

    static void  CreateAssetBundleBuilds()
    {
        DirectoryInfo mDirectoryInfo = new DirectoryInfo(BuildAssetPath);
        DirectoryInfo[] mDirInfos = mDirectoryInfo.GetDirectories();
        if (mDirInfos.Length > 0)
        {
            foreach (var v in mDirInfos)
            {
                string path1 = Path.Combine(BuildAssetPath, v.Name);
                string bundleName1 = v.Name;
                CreateSingleBundle(v, bundleName1, path1);
            }
        }
        ToBuildAssetBundles(mBundleList.ToArray());
    }

    static void CreateSingleBundle(DirectoryInfo mDirectoryInfo,string bundleName,string path)
    {
        FileInfo[] mFileInfos = mDirectoryInfo.GetFiles();
        if (mFileInfos.Length > 0)
        {
            AssetBundleBuild mAssetInfo = new AssetBundleBuild();
            mAssetInfo.assetBundleName = bundleName+extention;
            mAssetInfo.assetBundleVariant ="";
            List<string> mAssetNames = new List<string>();
            foreach (FileInfo mFileInfo in mFileInfos)
            {
                if (mFileInfo.Extension != ".meta")
                {
                    string assetName = Path.Combine(path,mFileInfo.Name);
                    mAssetNames.Add(assetName);
                }

                mAssetInfo.assetNames = mAssetNames.ToArray();

            }
            mBundleList.Add(mAssetInfo);
        }

        DirectoryInfo[] mDirInfos = mDirectoryInfo.GetDirectories();
        if(mDirInfos.Length>0)
        {
            foreach (var v in mDirInfos)
            {
                string path1 = Path.Combine(path,v.Name);
                string bundleName1 = bundleName + "_" + v.Name;
                CreateSingleBundle(v, bundleName1,path1);
            }
        }      
    }

    /// <summary>
    /// BuildAssetBundleOptions.AppendHashToAssetBundleName:打包后的Bundle名称追加Hash字串
    /// </summary>
    /// <param name="mBuilds"></param>
    static void ToBuildAssetBundles(AssetBundleBuild[] mBuilds)
    {
        Debug.Log("AssetBundle Cout：" + mBuilds.Length);
        BuildPipeline.BuildAssetBundles(OutAssetPath, mBuilds, BuildAssetBundleOptions.UncompressedAssetBundle|BuildAssetBundleOptions.ForceRebuildAssetBundle, GetBuildTarget());
    }

    static private BuildTarget GetBuildTarget()
    {
        BuildTarget target = BuildTarget.StandaloneWindows64;
#if UNITY_WEBGL
         target = BuildTarget.WebGL;
#elif UNITY_STANDALONE
        target = BuildTarget.StandaloneWindows;
#elif UNITY_IPHONE
		target = BuildTarget.iPhone;
#elif UNITY_ANDROID
		target = BuildTarget.Android;
#endif
        return target;
    }

    static private string BundleCryption(FileInfo mfile)
    {
        FileStream mStream = mfile.OpenRead();
        EncryptionSystem_md5 mdata = new EncryptionSystem_md5();
        string mStr = mdata.Encryption(mStream);
        mStream.Close();
        return mStr;
    }

}
