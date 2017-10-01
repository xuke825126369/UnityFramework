using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using xk_System.Model.Modules;
using game.protobuf.data;
using xk_System.Model;
using System;
using xk_System.Debug;

namespace xk_System.View.Modules
{
    public class ChatView : xk_View
    {
        private const int MaxChatCout = 50;
        public ChatScrollView mScrollView;
        public InputField mInput;
        public Button mSendBtn;
        public Button mCloseBtn;
        public Button mExpressionBtn;
        public ExpressionView mExpressionView;
        private ChatModel mChatModel;

        protected override void Awake()
        {
            base.Awake();
            mChatModel = GetModel<ChatModel>();
            TextUtility.Instance.Init();
            mSendBtn.onClick.AddListener(OnClick_SureSendBtn);
            mCloseBtn.onClick.AddListener(ClickCloseBtn);
            mExpressionBtn.onClick.AddListener(Click_OpenExpressionView);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ModelSystem.Instance.GetModel<ChatModel>().addDataBind(RefreshView, "mChatDataList");

            mInput.text = string.Empty;
            StartCoroutine(InitData());
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ModelSystem.Instance.GetModel<ChatModel>().removeDataBind(RefreshView, "mChatDataList");
        }

        private void Click_OpenExpressionView()
        {
            mExpressionView.gameObject.SetActive(true);
        }


        private void ClickCloseBtn()
        {
            HideView<ChatView>();
        }

        private IEnumerator InitData()
        {
            yield return 0;
            System.Random mRandom = new System.Random();
            for (int i = 0; i < 100; i++)
            {
                ChatItemData mChatInfo = new ChatItemData();
                mChatInfo.time = TimeUtility.GetTimeStamp(DateTime.Now);
                mChatInfo.ChannelId = UnityEngine.Random.Range(1, 7);
                mChatInfo.name = "我是一只调皮的精灵1[消息ID：" + (100-1-i) + "]";
                int contentId = 2;
                if (contentId == 2)
                {
                    mChatInfo.content = "<xk_1,emoj_2,50,50>sddddddddd<xk_2,#1,我是你妈>";
                }
                else
                {
                    mChatInfo.content = "<xk_2,#1,我是你妈我是你爸我是你爷我是你儿子>#sdf sdf sadfasasfdasfd<xk_2,#1,我是你妈>dfs dsssss#3sssssssssss#2dddd<xk_2,#1,我是你妈>ddddddddddddsdfsdd<xk_2,#1,我是你妈>dddddddddddddddddddddddd";
                }
                mChatModel.ReceiveData(mChatInfo);
            }
        }
        public void AddInputContent(string content)
        {
            mInput.text += content;
        }


        private void OnClick_SureSendBtn()
        {
            ChatItemData mdata = new ChatItemData();
            mdata.time = TimeUtility.GetTimeStamp(DateTime.Now);
            mdata.name = "xuke";
            mdata.content = mInput.text;
            TextUtility.Instance.MatchSpriteSimpleText(ref mdata.content);
            mChatModel.ReceiveData(mdata);
            mInput.text = string.Empty;
        }

        private void RefreshView(object data)
        {
            mScrollView.InitView(mChatModel.mChatDataList);
        }
    }
}