using UnityEngine;
using System.Collections;
using xk_System.Db;
using xk_System.View;
using xk_System.AssetPackage;

public class initManager : Singleton<initManager>
{
    public IEnumerator StartInitSystem()
    {
        InitGlobalManager();
        yield return InitBundleManager();
        InitDBSystem();
        UpdateManager.Instance.xStartCoroutine(InitEventSystem());
        UpdateManager.Instance.xStartCoroutine(InitWindowManager());
        InitAudioSystem();
    }

    private void InitGlobalManager()
    {
        UpdateManager.Instance.Init();
    }

    private IEnumerator InitBundleManager()
    {
        yield return AssetBundleManager.Instance.InitAssetBundleManager();
    }

    private void InitAudioSystem()
    {
        AudioManager.Instance.Init();
    }

    private void InitDBSystem()
    {
        UpdateManager.Instance.xStartCoroutine(DbManager.Instance.initDbSystem());
    }

    private IEnumerator InitEventSystem()
    {
        AssetInfo mAssetInfo = ResourceABsFolder.Instance.getAsseetInfo("manager", "ObjectRoot");
        yield return AssetBundleManager.Instance.AsyncLoadAsset(mAssetInfo);
        GameObject obj = AssetBundleManager.Instance.LoadAsset(mAssetInfo) as GameObject;
        ObjectRoot mObjectRoot = obj.GetComponent<ObjectRoot>();
        mObjectRoot.Init();
        obj.SetActive(true);
        MonoBehaviour.DontDestroyOnLoad(obj);
    }

    private IEnumerator InitWindowManager()
    {
        yield return WindowManager.Instance.InitWindowManager();
    }
}
