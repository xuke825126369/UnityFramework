using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace xk_System.Db
{
    public class SkillConfig : Singleton<SkillConfig>
    {
        public readonly List<SkillDB> mSkillConfig = null;

        public SkillConfig()
        {
            mSkillConfig = DbManager.Instance.GetDb<SkillDB>();
        }

        public SkillDB FindItem(int id)
        {
           return  mSkillConfig.Find((x) => x.id == id);
        }
    }
}