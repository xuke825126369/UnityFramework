using UnityEngine;
using System.Collections;
using System;

namespace xk_System.View.Modules
{
    public class WindowLoadingView : xk_GlobalView
    {
        private GameObject obj;
        WTimer mWTimer;
        protected override void Awake()
        {
            base.Awake();
            obj = transform.Find("Image").gameObject;
            WTimerCallBack mCallBack = new WTimerCallBack();
            mCallBack.onRunning = PlayAnimation;
            mWTimer = new WTimer(50,0,mCallBack);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mWTimer.start();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            mWTimer.stop();
        }

        void PlayAnimation(object  mdata)
        {
            obj.transform.Rotate(new Vector3(0, 0, 10));
        }
    }
}