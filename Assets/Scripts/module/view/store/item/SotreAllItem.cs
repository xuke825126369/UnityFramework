using UnityEngine;
using System.Collections;
using xk_System.Db;
using System;
using UnityEngine.UI;
using xk_System.AssetPackage;

namespace xk_System.View.Modules
{
    public class SotreAllItem : CommonScrollViewItem
    {
        public Image mIcon;
        public Text mName;
        public Text mPrice;
        public Button mback;

        private ItemData mSaveData;
        protected override void Awake()
        {
            base.Awake();
            mback.onClick.AddListener(Click_Item);
        }

        private void Click_Item()
        {
            WindowManager.Instance.GetView<StoreView>().mItemShowView.ShowItem(mSaveData);
        }

        public override void RefreshItem(object data)
        {
            base.RefreshItem(data);
            mSaveData = data as ItemData;
            mName.text = mSaveData.mItemDB.Name;
            mPrice.text = "" + mSaveData.mItemDB.CompoundPrice;
            Sprite mSprite = AtlasManager.Instance.GetSprite("item", mSaveData.mItemDB.id.ToString());
            mIcon.sprite = mSprite;
        }


        public override Vector2 GetCellSize()
        {
            return new Vector2(170, 65);
        }
    }
}
