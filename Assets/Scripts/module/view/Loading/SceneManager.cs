using UnityEngine;
using System.Collections;
using xk_System.View;
using UnityEngine.SceneManagement;
using xk_System.View.Modules;
using System.Collections.Generic;

public static class SceneInfo
{
    public const string Scene_1 = "init";
    public const string Scene_2 = "main";
    public const string Scene_3 = "fight";
}

public class SceneSystem : Singleton<SceneSystem>
{
    public void GoToScene(string sceneName)
    {
        LoadSceneResource(sceneName);
    }

    private void LoadSceneResource(string sceneName)
    {
        WindowManager.Instance.CleanManager();
        WindowManager.Instance.ShowView<SceneLoadingView>();
        TaskProgressBar mTask = SceneSystemLoadingModel.Instance.GetPrepareTask(SceneInfo.Scene_2);
        EnterFrame.Instance.add(StartTask,mTask);
    }

    private void StartTask(object data)
    {
        TaskProgressBar mTask = data as TaskProgressBar;
        float jindu = mTask.getProgress();
        if(jindu>=1f)
        {
            Debug.LogError("进入主界面");
            EnterFrame.Instance.remove(StartTask);
            WindowManager.Instance.HideView<SceneLoadingView>();
            WindowManager.Instance.ShowView<MainView>();
        }
    }
}

public class SceneSystemLoadingModel:Singleton<SceneSystemLoadingModel>
{
    TaskProgressBar mTask=null;
    public TaskProgressBar GetPrepareTask(string sceneName)
    {
        switch (sceneName)
        {
            case SceneInfo.Scene_1:
                break;
            case SceneInfo.Scene_2:
                PrepareScene_Main_Task();
                break;
            case SceneInfo.Scene_3:
                break;
        }
        return mTask;
    }

    public void PrepareScene_Main_Task()
    {
        Queue<SubTaskProgress> mSubTaskList = new Queue<SubTaskProgress>();
        SubTaskProgress mSubTask = new SubTaskProgress();
        mSubTask.SubMaxProgress = 100;
        mSubTask.mSubTask = MainScenePrepareTask.Instance.mTask;
        mSubTaskList.Enqueue(mSubTask);
        mTask = new TaskProgressBar(mSubTaskList);
        UpdateManager.Instance.xStartCoroutine(MainScenePrepareTask.Instance.Prepare());

    }
}
