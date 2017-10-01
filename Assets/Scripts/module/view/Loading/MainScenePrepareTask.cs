using UnityEngine;
using System.Collections;
using xk_System.AssetPackage;

public class MainScenePrepareTask:Singleton<MainScenePrepareTask>
{
    public LoadProgressInfo mTask = new LoadProgressInfo();
    public IEnumerator Prepare()
    {
        ObjectRoot.Instance.scene_root.root.SetActive(true);
        ObjectRoot.Instance.scene_root.mCamera.gameObject.SetActive(true);
        mTask.progress = 100;
        yield return 0;
    }
}
