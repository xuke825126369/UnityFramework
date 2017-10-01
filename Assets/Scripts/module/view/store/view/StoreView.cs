using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using xk_System.Db;
using System.Collections.Generic;

namespace xk_System.View.Modules
{
    public class StoreView : xk_View
    {
        public Text mMoney;
        public Button mCloseBtn;

        public Button mRecommendItemBtn;
        public Button mAllItemBtn;
        public StoreAllItemView mAllItemView;
        public StoreRecommendView mRecommendItemView;
        public ItemShowView mItemShowView;
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void Awake()
        {
            base.Awake();
            mCloseBtn.onClick.AddListener(Click_CloseBtn);
            mAllItemBtn.onClick.AddListener(Click_AllItemBtn);
            mRecommendItemBtn.onClick.AddListener(Click_RecommendItemBtn);
        }

        private void Click_CloseBtn()
        {
            HideView<StoreView>();
        }

        private void Click_RecommendItemBtn()
        {
            mAllItemView.gameObject.SetActive(false);
            mRecommendItemView.gameObject.SetActive(true);
        }

        private void Click_AllItemBtn()
        {
            mAllItemView.gameObject.SetActive(true);
            mRecommendItemView.gameObject.SetActive(false);
        }
    }
}
	
