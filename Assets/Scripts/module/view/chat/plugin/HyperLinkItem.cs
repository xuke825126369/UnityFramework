using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HyperLinkItem : MonoBehaviour
{
    public Text mContent;
    public Button mButton;
    public Text mUnderLineText;
    private hyperlink mSaveData;

    private void Awake()
    {
        mButton.onClick.AddListener(Click_Text);
    }

    public void Click_Text()
    {
        Debug.LogError("点击超链接");
    }


    public void RefreshItem(hyperlink mdata)
    {
        mSaveData = mdata;
        Show();
    }

    public void Show()
    {
        mContent.text = mSaveData.OriContent;
        mContent.color = getHyperLinkColor();
        float width = TextUtility.Instance.GetWidth(mUnderLineText, mSaveData.OriContent);
        mUnderLineText.GetComponent<RectTransform>().sizeDelta = new Vector2(width, mSaveData.mMaxPos.y - mSaveData.mMinPos.y);
        mButton.GetComponent<RectTransform>().sizeDelta = new Vector2(width, mUnderLineText.GetComponent<RectTransform>().sizeDelta.y);
        string content = "";
        while (true)
        {
            content += "_";
            float width1 = TextUtility.Instance.GetWidth(mUnderLineText, content);
            if (width1 >= width)
            {
                if(width1-width>=TextUtility.Instance.GetWidth(mUnderLineText,"_")/2f)
                {
                   content= content.Remove(content.Length-1);
                }
                break;
            }
        }
        mUnderLineText.text = content;
    }

    private Color getHyperLinkColor()
    {
        return TextUtility.Instance.GetTextColor(TextUtility.TextType_Color_HyperLink);

    }
}
