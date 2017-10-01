using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using xk_System.Db;
using UnityEngine.UI;

public class StoreSubItemLineManager : MonoBehaviour
{
    public Transform mLineParent;
    public RectTransform mLinePrefab;
    public RectTransform mXiaPrefab;
    public void Refresh(Dictionary<SubItem, StoreSubItem> mSubItemObjDic, ItemData mItemData)
    {
        Dictionary<SubItem, List<SubItem>> mSubItemDic = new Dictionary<SubItem, List<SubItem>>();
        List<SubItem> mSubItemList = mItemData.mSubItemList;
        foreach (var v in mSubItemList)
        {
            if (v.mItemDBParent != null)
            {
                List<SubItem> mlist = null;
                if (!mSubItemDic.TryGetValue(v.mItemDBParent, out mlist))
                {
                    mlist = new List<SubItem>();
                    mSubItemDic.Add(v.mItemDBParent, mlist);
                }
                mlist.Add(v);
            }
        }

        int index = 0;
        foreach (var v in mSubItemDic.Values)
        {
            v.Sort((x, y) => x.index - y.index);
            float width = mSubItemObjDic[v[v.Count - 1]].transform.localPosition.x - mSubItemObjDic[v[0]].transform.localPosition.x;
            if (index >= mLineParent.childCount)
            {
                GameObject obj = Instantiate<GameObject>(mLinePrefab.gameObject);
                obj.transform.SetParent(mLineParent);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
            }
            RectTransform mTran = mLineParent.GetChild(index).GetComponent<RectTransform>();
            Vector2 mOriVec = mTran.sizeDelta;
            mTran.sizeDelta = new Vector2(width, mOriVec.y);
            if (mItemData.MaxLayer <= 3)
            {
                mTran.localPosition = new Vector3(mSubItemObjDic[v[0]].transform.localPosition.x, mSubItemObjDic[v[0]].transform.localPosition.y + 30, 0);
            }else
            {
                mTran.localPosition = new Vector3(mSubItemObjDic[v[0]].transform.localPosition.x, mSubItemObjDic[v[0]].transform.localPosition.y + 20, 0);
            }
            mTran.gameObject.SetActive(true);

            Transform mShang = mTran.Find("shang");
            mShang.localPosition = new Vector3(width/2f,0,0);
            Transform mXiaParent = mTran.Find("xiaParent");
            for (int i = 0; i < v.Count; i++)
            {
                if (i >= mXiaParent.childCount)
                {
                    GameObject obj = Instantiate<GameObject>(mXiaPrefab.gameObject);
                    obj.transform.SetParent(mXiaParent);
                    obj.transform.localScale = Vector3.one;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localEulerAngles= new Vector3(0, 0, 90f);
                }
                Transform mTran1 = mXiaParent.GetChild(i);
                float x = mSubItemObjDic[v[i]].transform.localPosition.x - mSubItemObjDic[v[0]].transform.localPosition.x;
                mTran1.localPosition = new Vector3(x, -5, 0);
                mTran1.gameObject.SetActive(true);
            }

            for (int i = v.Count; i < mXiaParent.childCount; i++)
            {
                mXiaParent.GetChild(i).gameObject.SetActive(false);
            }
            index++;
        }

        for(int i=index;i<mLineParent.childCount;i++)
        {
            mLineParent.GetChild(i).gameObject.SetActive(false);
        }
    }
}
