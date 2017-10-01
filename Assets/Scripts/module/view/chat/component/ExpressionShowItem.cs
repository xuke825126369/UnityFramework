using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using xk_System.View;
using xk_System.View.Modules;

public class ExpressionShowItem : CommonScrollViewItem
{
    public Button mExpressionBtn;

    private Sprite mSaveData;
    protected override void Awake()
    {
        mExpressionBtn.onClick.AddListener(Click_Expression);
    }

    private void Click_Expression()
    {
        WindowManager.Instance.GetView<ChatView>().AddInputContent(TextUtility.Instance.mSpriteNameExchangeDic[mSaveData.name]);
        WindowManager.Instance.GetView<ChatView>().mExpressionView.gameObject.SetActive(false);
    }

    public override void RefreshItem(object data)
    {
        base.RefreshItem(data);
        mSaveData = data as Sprite;
        mExpressionBtn.image.sprite = mSaveData;
    }

    public override Vector2 GetCellSize()
    {
        return new Vector2(110,110);
    }
}
