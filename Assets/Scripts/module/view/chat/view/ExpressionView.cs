using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using xk_System.AssetPackage;

public class ExpressionView : MonoBehaviour
{
    public Button mCloseBtn;
    public CommonScrollView mScrollView;
    
    private void Awake()
    {
        mCloseBtn.onClick.AddListener(CloseBtn);
    }
    
    private void Start()
    {
        RefreshView();
    }	

    private void CloseBtn()
    {
        gameObject.SetActive(false);
    }

    private void RefreshView()
    {
        Dictionary<string, Sprite> mDic = AtlasManager.Instance.GetAtlas("emoj");
        List<object> mList = new List<object>();
        foreach(var v in mDic)
        {
            mList.Add(v.Value);
        }
        mScrollView.InitView(mList);
    }
}
