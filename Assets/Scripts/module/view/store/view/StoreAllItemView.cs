using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using xk_System.Db;

namespace xk_System.View.Modules
{
    public class StoreAllItemView : MonoBehaviour
    {
        public CommonScrollView mScrollView;

        public Button mDayeBtn;
        public Button mDuiXianBtn;

        public Button mXiaoHaoPinBtn;
        public Button mGongZiZhuangBtn;
        public Button mShiYeBtn;

        public Button mHuaJiaBtn;
        public Button mShengMingZhiBtn;
        public Button mShengMingHuiFuBtn;
        public Button mMoFaKangXingBtn;

        public Button mGongjiSuduBtn;
        public Button mBaoJiBtn;
        public Button mGongjiliBtn;
        public Button mShengMingTouQuBtn;

        public Button mLengQueSuoJianBtn;
        public Button mFaLiZhiBtn;
        public Button mFaLiHuiFuBtn;
        public Button mFaShuQiangDuBtn;

        public Button mXieZiBtn;
        public Button mOtherYiDongSuduBtn;


        private void Awake()
        {
            RefreshView();
        }

        private void RefreshView()
        {
            List<ItemDB> mItemList = ItemConfig.Instance.mItemConfig;
            List<object> mList = new List<object>();
            foreach (var v in mItemList)
            {
                ItemData mItemData = ItemConfig.Instance.GetItemData(v.id);               
                mList.Add(mItemData);
            }
            mScrollView.InitView(mList);
        }
    }
}