using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace xk_System.View.Modules
{
    public class MsgBoxView : xk_DialogView
    {
        public const int YesNoView=1;
        public const int SureView = 2;
        private Action mYesEvent;

        private Text mText;
        private Button yesBtn;
        private Button noBtn;
        private Button sureBtn;
        private Button closeBtn;

        protected override void Awake()
        {
            base.Awake();
            yesBtn.onClick.AddListener(Click_YesBtn);
            noBtn.onClick.AddListener(Click_SureBtn);
            sureBtn.onClick.AddListener(Click_SureBtn);
            closeBtn.onClick.AddListener(Click_SureBtn);
        }

        private void FindObject()
        {
            Transform t = transform.Find("Text");
            mText = t.GetComponent<Text>();

            t = transform.Find("yes");
            yesBtn = t.GetComponent<Button>();

            t = transform.Find("no");
            noBtn = t.GetComponent<Button>();

            t = transform.Find("sure");
            sureBtn = t.GetComponent<Button>();

            t = transform.Find("close");
            closeBtn = t.GetComponent<Button>();
        }

        private void Click_SureBtn()
        {
            HideView<MsgBoxView>();
        }

        private void Click_YesBtn()
        {
            HideView<MsgBoxView>();
            if (mYesEvent != null)
            {
                mYesEvent();
            }
        }

        public override void InitView(object data = null)
        {
            base.InitView(data);
            if (data is InitViewInfo)
            {
                InitViewInfo mTipInfo = data as InitViewInfo;
                mText.text = mTipInfo.mContent;
                switch (mTipInfo.mType)
                {
                    case YesNoView:
                        yesBtn.gameObject.SetActive(true);
                        noBtn.gameObject.SetActive(true);
                        sureBtn.gameObject.SetActive(false);
                        if (mTipInfo.mEvnet != null)
                        {
                            mYesEvent = mTipInfo.mEvnet;
                        }
                        break;
                    case SureView:
                        yesBtn.gameObject.SetActive(false);
                        noBtn.gameObject.SetActive(false);
                        sureBtn.gameObject.SetActive(true);
                        break;
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            mYesEvent = null;
        }

        public class InitViewInfo
        {
            public int mType;
            public Action mEvnet;
            public string mContent;
        }

    }
}