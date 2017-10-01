using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using xk_System.Debug;

public class LoadProgressInfo
{
    public const uint MaxProgress = 100;
    public const uint MinProgress = 0;
    public uint progress = 0;
    public string Des = "";
    public SubTaskProgress CurrentSubTask=null;
    public Queue<SubTaskProgress> mSubTaskList = new Queue<SubTaskProgress>();

    public uint getCurrentProgress()
    {
        uint subProgress = 0;
        if (mSubTaskList.Count > 0)
        {
            if (CurrentSubTask == null)
            {
                CurrentSubTask = mSubTaskList.Dequeue();
            }
        }
        if (CurrentSubTask != null)
        {
            uint targetPro = CurrentSubTask.getCurrentProgress();
            if (targetPro >= CurrentSubTask.SubMaxProgress)
            {
                progress += targetPro;
                CurrentSubTask = null;
            }else
            {
                subProgress += targetPro;
            }
        }

        return progress + subProgress;
    }
}

public class SubTaskProgress
{
    public uint SubMaxProgress;
    public LoadProgressInfo mSubTask;

    public uint getCurrentProgress()
    {
        if (mSubTask != null)
        {
            uint currentProgress = (uint)Mathf.CeilToInt(SubMaxProgress / 100f * mSubTask.getCurrentProgress());
            if (currentProgress > SubMaxProgress)
            {
                currentProgress = SubMaxProgress;
            }
            return currentProgress;
        }
        return 0;
    }

    public string GetDes()
    {
        return mSubTask.Des;
    }
}


public class TaskProgressBar
{
    uint currentProgress = 0;
    LoadProgressInfo mdata;
    public TaskProgressBar(Queue<SubTaskProgress> mSubTaskList)
    {
        currentProgress = 0;
        mdata = new LoadProgressInfo();
        mdata.mSubTaskList = mSubTaskList;
        CheckTask();
    }

    public void CheckTask()
    {
        uint SumProgress = 0;
        foreach (var v in mdata.mSubTaskList)
        {
            SumProgress += v.SubMaxProgress;
        }
        if (SumProgress != LoadProgressInfo.MaxProgress)
        {
            DebugSystem.LogError("此任务分配不合理");
        }
    }

    private string LastDes = "";
    public string getDes()
    {
        if (mdata.CurrentSubTask != null)
        {
            LastDes = mdata.CurrentSubTask.GetDes();
        }
        return LastDes;
    }

    private uint LastJindu = 0;
    private uint AddJindu = 0;
    public float getProgress()
    {
        uint targetPro = mdata.getCurrentProgress();
        if(LastJindu!=targetPro)
        {
            DebugSystem.LogError("当前进度："+targetPro);
            LastJindu = targetPro;
        }
        if (currentProgress < targetPro)
        {
            if (AddJindu < 10)
            {
                AddJindu++;
            }
            currentProgress += AddJindu;
            if(currentProgress>targetPro)
            {
                currentProgress = targetPro;
                AddJindu = 0;
            }
        }
        return currentProgress / 100f;
    }
}
