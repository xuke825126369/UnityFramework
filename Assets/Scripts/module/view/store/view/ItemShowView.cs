using UnityEngine;
using System.Collections;
using xk_System.Db;
using System.Collections.Generic;
using xk_System.Model.Modules;
using xk_System.Model;

namespace xk_System.View.Modules
{
    public class ItemShowView : MonoBehaviour
    {
        public StoreSubItemLineManager mLineManager;
        public StoreSubItem mSubItemPrefab;
        public Transform mSubItemParent;

        public CommonScrollView mItemCompoundView;
        private ItemData mSavaData;
        private Dictionary<SubItem, StoreSubItem> mSubItemDic = new Dictionary<SubItem, StoreSubItem>();

        public ItemAttView mItemAttView;

        public void ShowItem(ItemData mItem)
        {
            mSavaData = mItem;
            RefreshCanCompoundItem();
            RefreshSubItem();
            RefreshItemAtt(mSavaData.mItemDB);
        }

        private void RefreshCanCompoundItem()
        {
            List<object> mList = new List<object>();
            List<ItemDB> mCanCompoundItemArray = ItemConfig.Instance.GetCanCompoundItemList(mSavaData.mItemDB.id);
            for (int i = 0; i < mCanCompoundItemArray.Count; i++)
            {
                mList.Add(mCanCompoundItemArray[i]);
            }
            mItemCompoundView.InitView(mList);
        }

        private void RefreshSubItem()
        {
            mSubItemDic.Clear();
            List<SubItem> mSubITemList = mSavaData.mSubItemList;
            for (int i = 0; i < mSubITemList.Count; i++)
            {
                if (i >= mSubItemParent.childCount)
                {
                    GameObject obj = Instantiate<GameObject>(mSubItemPrefab.gameObject);
                    obj.transform.SetParent(mSubItemParent);
                    obj.transform.localScale = Vector3.one;
                    obj.transform.localPosition = Vector3.zero;
                }
                StoreSubItem mItem1 = mSubItemParent.GetChild(i).GetComponent<StoreSubItem>();
                if (mSavaData.MaxLayer <= 3)
                {
                    mItem1.mIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
                }
                else
                {
                    mItem1.mIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
                }
                Vector3 mPos= ModelSystem.Instance.GetModel<StoreModel>().GetLocalPos(mSubITemList[i].Layer, mSubITemList[i].cout, mSubITemList[i].index);
                if (i == 0)
                {
                    mItem1.transform.localPosition = mPos;
                }
                else
                {
                    Vector3 locPos= mSubItemDic[mSubITemList[i].mItemDBParent].transform.localPosition + mPos;
                    if (mSavaData.MaxLayer <= 3)
                    {
                        mItem1.transform.localPosition = locPos;
                    }
                    else
                    {
                        mItem1.transform.localPosition = new Vector3(locPos.x,locPos.y+10,0);
                    }
                }
                mSubItemDic.Add(mSubITemList[i],mItem1);
                mItem1.RefreshItem(mSubITemList[i].mItemDB);
                mItem1.gameObject.SetActive(true);
            }

            for (int i = mSubITemList.Count; i < mSubItemParent.childCount; i++)
            {
                mSubItemParent.GetChild(i).gameObject.SetActive(false);
            }

            mLineManager.Refresh(mSubItemDic, mSavaData);
        }

        private void RefreshItemAtt(ItemDB mItem)
        {
            mItemAttView.RefreshAtt(mItem.id);
        }
    }
}
