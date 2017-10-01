using UnityEngine;
using System.Collections;
using System;

namespace xk_System.Model
{
    public class TimeModel :xk_Model
    {
        public ulong mServerTime;

        public override void initModel()
        {
            base.initModel();
            ModelSystem.Instance.GetModel<TimeMessage>().mDataBindServerTime.addDataBind(UpdataClientTime);
        }

        public override void destroyModel()
        {
            base.destroyModel();
            ModelSystem.Instance.GetModel<TimeMessage>().mDataBindServerTime.removeDataBind(UpdataClientTime);
        }

        private void UpdataClientTime(ulong serverTime)
        {
            mServerTime = serverTime;
        }


    }
}