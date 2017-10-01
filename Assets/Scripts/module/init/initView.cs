using UnityEngine;
using System.Collections;
using xk_System.View;
using xk_System.View.Modules;
using xk_System.Debug;
using xk_System.Net;
using xk_System.Db;
using UnityEngine.UI;
using xk_System.AssetPackage;
using System;

namespace xk_System.Init
{
    public class initView : MonoBehaviour
    {
        public Scrollbar mProgressBar;
        public Text mProgressText;
        public Text mProgressDes;

        void Update()
        {
            float jindu = InitModel.Instance.mTask.getProgress();
            mProgressBar.size = jindu;
            mProgressText.text = jindu * 100 + "/" + 100;
            mProgressDes.text = InitModel.Instance.mTask.getDes();

            if (jindu >= 1f)
            {
                WindowManager.Instance.ShowView<LoginView>();
                Destroy(this.gameObject);
            }
        }
    }
}