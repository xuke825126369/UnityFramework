using UnityEngine;
using System.Collections;
using System;
using xk_System.Db;
using UnityEngine.UI;
using xk_System.AssetPackage;

public class StoreCompoundItem : CommonScrollViewItem
{
    public Image mIcon;
    private ItemDB mSaveData;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Click_Icon()
    {

    }

    public override void RefreshItem(object data)
    {
        base.RefreshItem(data);
        mSaveData = data as ItemDB;
        Sprite mSprite = AtlasManager.Instance.GetSprite("item", mSaveData.id.ToString());
        mIcon.sprite = mSprite;
    }

    public override Vector2 GetCellSize()
    {
        return new Vector2(60,40);
    }
}
