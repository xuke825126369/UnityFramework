using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using xk_System.Db;
using xk_System.AssetPackage;

public class StoreSubItem : MonoBehaviour
{
    public Button mBack;
    public Image mIcon;
    public Text mPrice;

    private ItemDB mSaveData;
    private void Awkae()
    {
        mBack.onClick.AddListener(Click_Item);
    }

    private void Click_Item()
    {

    }

    public void RefreshItem(ItemDB mItemDB)
    {
        mSaveData = mItemDB;
        mPrice.text = "" + mSaveData.CompoundPrice;
        Sprite mSprite = AtlasManager.Instance.GetSprite("item", mSaveData.id.ToString());
        mIcon.sprite = mSprite;
    }

	
}
