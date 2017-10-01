using System.Collections.Generic;
using xk_System.HotUpdate;

namespace xk_System.Init
{
    public class InitModel:Singleton<InitModel>
    {
        public TaskProgressBar mTask;
        public InitModel()
        {
            Queue<SubTaskProgress> mList = new Queue<SubTaskProgress>();
            SubTaskProgress mInfo = new SubTaskProgress();
            mInfo.SubMaxProgress = 50;
            mInfo.mSubTask = AssetBundleHotUpdateManager.Instance.mTask;
            mList.Enqueue(mInfo);

            mInfo = new SubTaskProgress();
            mInfo.SubMaxProgress = 50;
            mInfo.mSubTask = initManager.Instance.mTask;
            mList.Enqueue(mInfo);

            mTask = new TaskProgressBar(mList);
        }
    }
}