using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using XkProtobufData;
using xk_System.Debug;

public class SelectHeroItem : MonoBehaviour
{
    public Text mName;
    public Text mGender;
    public Text mProfession;

    public void RefreshItem(struct_PlayerDetailInfo mHeroInfo)
    {
        DebugSystem.Log("role id: "+mHeroInfo.Id);
        mName.text="姓名："+ mHeroInfo.Name;
        if (mHeroInfo.Gender == 1)
        {
            mGender.text = "性别：男";
        }else if(mHeroInfo.Gender ==2)
        {
            mGender.text = "性别：女";
        }

        if(mHeroInfo.Profession==1)
        {
            mProfession.text = "职业：战士";
        }
        else if(mHeroInfo.Profession==2)
        {
            mProfession.text = "职业：法师";
        }
        else if(mHeroInfo.Profession==3)
        {
            mProfession.text = "职业：道士";
        }
    }

}
