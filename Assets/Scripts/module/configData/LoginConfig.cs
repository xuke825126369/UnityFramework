using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace xk_System.Db
{
    public class LoginConfig : Singleton<LoginConfig>
    {
        public List<ServerListDB> mServerConfig;
        public LoginConfig()
        {
            mServerConfig = DbManager.Instance.GetDb<ServerListDB>();
        }

        public ServerListDB FindServerItem(int serverId)
        {
            return  mServerConfig.Find((x) => x.id == serverId);
        }
    }
}