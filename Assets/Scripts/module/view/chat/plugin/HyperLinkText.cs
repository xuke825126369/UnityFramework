using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class HyperLinkText : MonoBehaviour
{
    public Transform Parent;
    public HyperLinkItem mItemPrefab;

    private void Awake()
    {
       // mText.GetComponent<Button>().onClick.AddListener(Click_Text);
    }

    public void Click_Text()
    {
        /*if (mSaveData.Count > 0)
        {
            Vector3 mpos = Input.mousePosition;
            Camera mCamera = Camera.main;
            if (mCamera == null)
            {
                mCamera = GameObject.FindObjectOfType<Camera>();
                Debug.LogError("当前相机找不到");
            }
            Vector3 mWorldPos = mCamera.ScreenToWorldPoint(mpos);
            Vector3 mLocalPos = mText.transform.InverseTransformPoint(mWorldPos);
            Debug.LogError("本地坐标：" + mLocalPos);

            foreach (var v in mSaveData)
            {
                if (mLocalPos.x >= v.mMinPos.x && mLocalPos.x <= v.mMaxPos.x)
                {
                    Debug.LogError("点击超链接");
                }
            }
        }*/
    }

    public void Init(List<hyperlink> mdata)
    {
        for (int i = 0; i < mdata.Count; i++)
        {
            if (i >= Parent.childCount)
            {
                GameObject obj = Instantiate(mItemPrefab.gameObject) as GameObject;
                obj.transform.SetParent(Parent);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
            }
            Parent.GetChild(i).GetComponent<RectTransform>().anchoredPosition = mdata[i].mMinPos;
            Parent.GetChild(i).GetComponent<HyperLinkItem>().RefreshItem(mdata[i]);
            Parent.GetChild(i).gameObject.SetActive(true);
        }
        for (int i = mdata.Count; i < Parent.childCount; i++)
        {
            Parent.GetChild(i).gameObject.SetActive(false);
        }
    }
}
