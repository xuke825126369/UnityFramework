using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using xk_System.AssetPackage;
using xk_System.Db;

public class ItemAttView : MonoBehaviour
{
    public Image mIcon;
    public Text mName;
    public Text mPrice;
    public Button mBuyBtn;
    public Text mAttDes;
    public Text mOtherAttDes;


    public void RefreshAtt(int Itemid)
    {
        Sprite mSprite = AtlasManager.Instance.GetSprite("",Itemid.ToString());
        mIcon.sprite = mSprite;

        ItemData mItemData = ItemConfig.Instance.GetItemData(Itemid);
        mName.text= mItemData.mItemDB.Name;
        mPrice.text = ""+mItemData.mItemDB.CompoundPrice;
        mAttDes.text = ItemConfig.Instance.GetItemAttDes(Itemid);
        mOtherAttDes.text = ItemConfig.Instance.GetItemOtherAttDes(Itemid);

        mOtherAttDes.rectTransform.anchoredPosition = new Vector2(0, 70 - mAttDes.preferredHeight - 10);
    }


	
}
