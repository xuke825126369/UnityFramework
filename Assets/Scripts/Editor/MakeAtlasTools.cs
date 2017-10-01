using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

public class MakeAtlasTools
{
    [MenuItem("UnityEditor/GenerationPackage/MakeAllAtlas")]
    public static void MakeAllAtlas()
    {
        string targetDir = Application.dataPath + "/ResourceABs/atlas";
        CheckAtlas(targetDir);
        Debug.Log("检查图集完毕");
    }

    private static void CheckAtlas(string path)
    {
        DirectoryInfo mDir = new DirectoryInfo(path);
        foreach (var v in mDir.GetDirectories())
        {
            CheckSubAtals(v.FullName);
        }
    }

    private static void CheckSubAtals(string path)
    {
        DirectoryInfo mDir = new DirectoryInfo(path);
        foreach (var v1 in mDir.GetFiles())
        {
            if (v1.Extension != ".meta" && (v1.Extension == ".png" || v1.Extension == ".jpg"))
            {
                string assetPath = GetAssetPath(mDir.FullName, v1.Name);
                string tag = GetTagName(mDir.FullName);
                CheckTextureInfoAndRepairIfNeed(assetPath, tag);
            }
            else if (v1.Extension != ".meta" && v1.Extension != ".png" && v1.Extension != ".jpg")
            {
                Debug.LogError("文件格式不对：" + v1.FullName + " not sprite!!!");
            }
        }

        foreach (var v1 in mDir.GetDirectories())
        {
            CheckSubAtals(v1.FullName);
        }
    }


    public static string GetAssetPath(string DirFullName, string assetName)
    {
        DirFullName = DirFullName.Replace(@"\","/");
        int index = DirFullName.IndexOf("Assets/ResourceABs/");
        string path = DirFullName.Substring(index);
        if (path.EndsWith("/"))
        {
            path = path.Remove(path.Length - 1);
        }
        path += "/" + assetName;
        return path;
    }

    public static string GetTagName(string DirFullName)
    {
        DirFullName = DirFullName.Replace(@"\", "/");
        string Staff = "Assets/ResourceABs/atlas";
        int index = DirFullName.IndexOf(Staff);
        string path = DirFullName.Substring(index+Staff.Length);
        if (path.EndsWith("/"))
        {
            path = path.Remove(path.Length - 1);
        }
        path = path.Replace("/","_").ToLower();
        return path;
    }

    /// <summary>
    /// 检测一张图片是否正确设置为sprite，sprite的参数是否符合要求//
    /// </summary>
    /// <param name="assetPath"> 图片在Unity项目中的路径（Assets/...）</param>
    /// <param name="UITypeFileName">图片的打包标签参考对象</param>
    /// <param name="needRepair">是否需要修复，需要修复的话就不打错误日志了而是修复日志</param>
    /// <returns>图片是否设置正确</returns>
    private static bool CheckTextureInfoAndRepairIfNeed(string assetPath, string Tag = null, bool needRepair = true)
    {
        TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        bool isRight = true;
        if (ti.textureType != TextureImporterType.Sprite)
        {
            /*if (ti.textureType != TextureImporterType.Default || ti.spriteImportMode == SpriteImportMode.None)
            {
                if (!needRepair)
                {
                    string debugLog = "TextrueType must be sprite where path is \"{0}\", maybe you forget to set it?If not ,don't move it into {1} file.";
                    Debug.LogError(string.Format(debugLog, assetPath));
                }
                isRight = false;
            }*/
            isRight = false;
        }
        else if (!string.IsNullOrEmpty(Tag) && ti.spritePackingTag != Tag)
        {
            if (!needRepair)
            {
                string debugLog = "The spritePackingTag of the texture is different from the name of its parent file where path is \"{0}\", if this is not your wanted, check it.";
                Debug.LogError(string.Format(debugLog, assetPath));
            }
            isRight = false;
        }
        else if (ti.mipmapEnabled)
        {
            if (!needRepair)
            {
                string debugLog = "Are you sure this sprite need mipmap where path is \"{0}\"? If not, check it.";
                Debug.LogError(string.Format(debugLog, assetPath));
            }
            // UI图都不需要mipmap//
            isRight = false;
        } else if (ti.textureCompression != TextureImporterCompression.Compressed)
        {
            isRight = false;
        } else if(ti.spritePixelsPerUnit!=100)
        {
            isRight = false;
        }

        if (!isRight && needRepair)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePackingTag = Tag;
            ti.mipmapEnabled = false;
            ti.spritePixelsPerUnit = 100;
            ti.textureCompression = TextureImporterCompression.Compressed;
            ti.SaveAndReimport();
            Debug.Log(string.Format("The set of Texture where path is \"{0}\" has been repaired.", assetPath));
        }
        return isRight;
    }
}
