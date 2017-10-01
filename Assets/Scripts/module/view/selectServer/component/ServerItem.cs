using UnityEngine;
using System.Collections;
using xk_System.Db;
using System;
using UnityEngine.UI;
namespace xk_System.View.Modules
{
    public class ServerItem : xScrollViewItem<ServerListDB>
    {
        public Text mServerName;
        public Button mServerBtn;

        protected override void Awake()
        {
            base.Awake();
            mServerBtn.onClick.AddListener(Click_Btn);
        }
        protected void Click_Btn()
        {
            WindowManager.Instance.GetView<SelectServerView>().RefreshText(mSaveData);
        }

        public override Vector2 GetCellSize()
        {
            return new Vector2(210, 100);
        }

        public override void RefreshItem(ServerListDB data)
        {
            base.RefreshItem(data);
            mServerName.text = data.id+"区  "+data.serverName;
        }
    }
}