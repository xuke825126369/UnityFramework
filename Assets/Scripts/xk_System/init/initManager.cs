using UnityEngine;
using System.Collections;
using xk_System.Db;
using xk_System.View;
using xk_System.AssetPackage;

public class initManager : Singleton<initManager>
{
    public LoadProgressInfo mTask=new LoadProgressInfo();

    public IEnumerator StartInitSystem()
    {
        mTask.Des = "正在加载资源";
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
        mTask.progress += 60;
    }

    private void InitAudioSystem()
    {
        AudioManager.Instance.Init();
        mTask.progress+=5;
    }

    private void InitDBSystem()
    {
        SubTaskProgress mSubTask = new SubTaskProgress();
        mSubTask.SubMaxProgress = 25;
        mSubTask.mSubTask = DbManager.Instance.mTask;
        mTask.mSubTaskList.Enqueue(mSubTask);
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
        mTask.progress += 5;
    }

    private IEnumerator InitWindowManager()
    {
        yield return WindowManager.Instance.InitWindowManager();
        mTask.progress+=5;
    }
}
