using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using xk_System.AssetPackage;

public class SpriteInfo
{
    public int index;
    public int Length;
    public string OriContent;

    public string mSpriteText;
    public float pos;
    public float width;
    public float height;
}

public class hyperlink
{
    public int index;
    public int Length;
    public string OriContent;

    public string content;
    public Vector2 mMinPos;//左下角，以左下角为原点
    public Vector2 mMaxPos;
}

public class CommonInfo
{
    public int index;
    public int Length;
    public string content;
}

public class ModifyStrResult
{
    public string resultStr=string.Empty;
    public List<SpriteInfo> mMatchExpressList = new List<SpriteInfo>();
    public List<hyperlink> mMatchHyperLinkList =new List<hyperlink>();
}

/// <summary>
/// 此富文本与UGUI富文本不兼容，必须禁掉UGUI富文本
/// </summary>
public class TextUtility : Singleton<TextUtility>
{
    //<xk_1,#1,50,50>
    private readonly Regex mSpriteRegex = new Regex(@"<xk_1,(.[^,<>]+),(\d+),(\d+)>", RegexOptions.Singleline);
    //<xk_2,aaa,bbb>
    private readonly Regex mHyperLinkRegex = new Regex(@"<xk_2,(.[^,<>]+),(.[^,<>]+)>", RegexOptions.Singleline);

    //<xk_1,#1,50,50>
    private readonly Regex mSpriteSimpleRegex = new Regex(@"#\d+", RegexOptions.Singleline);
    public Dictionary<string, string> mCodeExchangeDic = new Dictionary<string, string>();
    public Dictionary<string, string> mSpriteNameExchangeDic = new Dictionary<string, string>();
    public TextUtility()
    {
        
    }

    public override void Init()
    {
        Dictionary<string, Sprite> mDic = AtlasManager.Instance.GetAtlas("emoj");
        int i = 1;
        foreach (var v in mDic)
        {
            string codeStr = "<xk_1," + v.Key + "," + 30 + "," + 30 + ">";
            string keyStr = "#" + i;
            mSpriteNameExchangeDic.Add(v.Key, keyStr);
            mCodeExchangeDic.Add(keyStr, codeStr);
            i++;
        }
    }

    public List<ModifyStrResult> ModifyStrResult(Text mText, string content)
    {
        ModifyStrResult result = new ModifyStrResult();
        result.resultStr = content;
        MatchSpriteText(mText, ref result);
        MatchHyperLinkText(mText, ref result);

        List<ModifyStrResult> resultList = GetLineList(mText, result);
        return resultList;
    }

    public void MatchSpriteSimpleText(ref string result)
    {
        ulong Max = (ulong)mSpriteNameExchangeDic.Count;
        int MaxLength = Max.ToString().Length;
        if(MaxLength==0)
        {
            return;
        }
        ulong Min = 1;
        string content = result;
        List<SpriteInfo> mMatchExpressList = new List<SpriteInfo>();
        foreach (Match v in mSpriteSimpleRegex.Matches(content))
        {
            int index = v.Index;
            int Length = v.Length;
            string NumberStr = v.Value.Substring(1);
            ulong number = 0;
            ulong.TryParse(NumberStr,out number);
            if(number>=Min)
            {
                if(number>Max)
                {
                    NumberStr = number.ToString().Substring(0, MaxLength);
                    ulong.TryParse(NumberStr, out number);
                    if(NumberStr.Length==1 && number>Max)
                    {
                        continue;
                    }else
                    {
                        if (number > Max)
                        {
                            number /= 10;
                        }
                    }
                }
            }else
            {
                continue;
            }

            string aaaa = v.Value[0] + number.ToString();
            SpriteInfo mSpriteInfo = new SpriteInfo();
            mSpriteInfo.index = index;
            mSpriteInfo.Length = aaaa.Length;
            mSpriteInfo.OriContent = aaaa;
            mMatchExpressList.Add(mSpriteInfo);
        }

        for (int i = mMatchExpressList.Count - 1; i >= 0; i--)
        {
            var v = mMatchExpressList[i];
            content = content.Remove(v.index, v.Length);
            string insertContent = mCodeExchangeDic[v.OriContent];
            content = content.Insert(v.index, insertContent);
        }
        result = content;
    }

    private void MatchSpriteText(Text mText, ref ModifyStrResult result)
    {
        string content = result.resultStr;
        List<SpriteInfo> mMatchExpressList = new List<SpriteInfo>();
        foreach (Match v in mSpriteRegex.Matches(content))
        {
            SpriteInfo mm = new SpriteInfo();
            mm.index = v.Index;
            mm.Length = v.Length;
            mm.mSpriteText = v.Groups[1].Value;
            int width = 0;
            int.TryParse(v.Groups[2].Value, out width);
            mm.width = width;
            int.TryParse(v.Groups[3].Value, out width);
            mm.height = width;
            mMatchExpressList.Add(mm);
        }
        result.mMatchExpressList.AddRange(mMatchExpressList);

        List<int> mStartIndexList = new List<int>();
        for (int i = 0; i < mMatchExpressList.Count; i++)
        {
            var v = mMatchExpressList[i];
            content = content.Remove(v.index, v.Length);
            string insertStr = GetNull(mText, mMatchExpressList[i].width);
            content = content.Insert(v.index, insertStr);

            int moveLength = insertStr.Length - v.Length;
            MoveIndex(result, v.index, moveLength);

            v.Length = insertStr.Length;
            v.OriContent = insertStr;
        }
        result.resultStr = content;
    }
    //从左往右
    private void MoveIndex(ModifyStrResult result, int StartIndex, int MoveLength)
    {
        foreach (var v in result.mMatchExpressList)
        {
            if (v.index > StartIndex)
            {
                v.index += MoveLength;
            }
        }

        foreach (var v in result.mMatchHyperLinkList)
        {
            if (v.index > StartIndex)
            {
                v.index += MoveLength;
            }
        }

    }

    private void MatchHyperLinkText(Text mText, ref ModifyStrResult result)
    {
        string content = result.resultStr;
        List<hyperlink> mMatchHyperLinkList = new List<hyperlink>();
        foreach (Match v in mHyperLinkRegex.Matches(content))
        {
            hyperlink mm = new hyperlink();
            mm.index = v.Index;
            mm.Length = v.Length;
            mm.content = v.Groups[1].Value;
            mm.OriContent = v.Groups[2].Value;
            mMatchHyperLinkList.Add(mm);
        }
        result.mMatchHyperLinkList.AddRange(mMatchHyperLinkList);

        List<int> mStartIndexList = new List<int>();
        for (int i = 0; i < mMatchHyperLinkList.Count; i++)
        {
            var v = mMatchHyperLinkList[i];
            content = content.Remove(v.index, v.Length);
            string insertStr = v.OriContent;
            content = content.Insert(v.index, insertStr);

            int moveLength = insertStr.Length - v.Length;
            MoveIndex(result, v.index, moveLength);

            v.Length = insertStr.Length;
        }
        result.resultStr = content;
    }

    private string GetNull(Text mText, float width)
    {
        string resultStr = string.Empty;
        string addStr = " ";
        while (true)
        {
            resultStr += addStr;
            if (GetWidth(mText, resultStr) >= width)
            {
                break;
            }
        }
        return resultStr;
    }

    public float GetWidth(Text mText, string content)
    {
        Font font = mText.font;
        int fontsize = mText.fontSize;
        string text = content;
        font.RequestCharactersInTexture(text, fontsize, FontStyle.Normal);
        CharacterInfo characterInfo;
        float LineWidth = 0f;
        if (content.Length > 0)
        {
            for (int i = 0; i < text.Length; i++)
            {
                font.GetCharacterInfo(text[i], out characterInfo, fontsize);
                LineWidth += characterInfo.advance;
            }
        }
        return LineWidth;
    }

    public List<ModifyStrResult> GetLineList(Text mText, ModifyStrResult result)
    {
        Font font = mText.font;
        int fontsize = mText.fontSize;
        string text = result.resultStr;
        font.RequestCharactersInTexture(text, fontsize, FontStyle.Normal);
        CharacterInfo characterInfo;

        List<ModifyStrResult> resultList = new List<ModifyStrResult>();

        float LineWidth = 0f;
        string LineStr = string.Empty;
        ModifyStrResult mLineResult = new ModifyStrResult();
        int SpriteIndex = 0;
        int HyperLinkIndex = 0;
        if (text.Length > 0)
        {
            for (int i = 0; i < text.Length;)
            {
                font.GetCharacterInfo(text[i], out characterInfo, fontsize);
                if (SpriteIndex < result.mMatchExpressList.Count && result.mMatchExpressList[SpriteIndex].index == i)
                {
                    int startIndex = result.mMatchExpressList[SpriteIndex].index;
                    int Length = result.mMatchExpressList[SpriteIndex].Length;
                    string content = result.mMatchExpressList[SpriteIndex].OriContent;
                    float width = GetWidth(mText, content);
                    if (LineWidth + width > mText.rectTransform.rect.width)
                    {
                        mLineResult.resultStr = LineStr;
                        resultList.Add(mLineResult);

                        LineStr = content;
                        LineWidth = width;

                        mLineResult = new ModifyStrResult();
                        SpriteInfo mdata = new SpriteInfo();
                        mdata.mSpriteText = result.mMatchExpressList[SpriteIndex].mSpriteText;
                        mdata.OriContent = content;
                        mdata.index = 0;
                        mdata.Length = Length;
                        mdata.width = result.mMatchExpressList[SpriteIndex].width;
                        mdata.height = result.mMatchExpressList[SpriteIndex].height;
                        mdata.pos = width / 2f;
                        mLineResult.mMatchExpressList.Add(mdata);
                    }
                    else
                    {
                        SpriteInfo mdata = new SpriteInfo();
                        mdata.index = LineStr.Length;
                        mdata.Length = result.mMatchExpressList[SpriteIndex].Length;
                        mdata.mSpriteText = result.mMatchExpressList[SpriteIndex].mSpriteText;
                        mdata.OriContent = result.mMatchExpressList[SpriteIndex].OriContent;
                        mdata.width = result.mMatchExpressList[SpriteIndex].width;
                        mdata.height = result.mMatchExpressList[SpriteIndex].height;
                        mdata.pos = GetWidth(mText, LineStr) + width / 2f;
                        mLineResult.mMatchExpressList.Add(mdata);

                        LineStr += content;
                        LineWidth += width;
                    }
                    SpriteIndex++;
                    i += Length;
                }
                else if (HyperLinkIndex < result.mMatchHyperLinkList.Count && result.mMatchHyperLinkList[HyperLinkIndex].index == i)
                {
                    int startIndex = result.mMatchHyperLinkList[HyperLinkIndex].index;
                    int Length = result.mMatchHyperLinkList[HyperLinkIndex].Length;
                    string content = result.mMatchHyperLinkList[HyperLinkIndex].OriContent;
                    float width = GetWidth(mText, content);
                    if (LineWidth + width > mText.rectTransform.rect.width)
                    {
                        string content1 = "";
                        float width1 = 0f;
                        int Length1 = 0;
                        for (int j = 0; j < content.Length; j++)
                        {
                            string content2 = content1 + content[j];
                            float tempWidth = GetWidth(mText, content2);
                            if (LineWidth + tempWidth > mText.rectTransform.rect.width)
                            {
                                Length1 = j;
                                break;
                            }
                            else
                            {
                                content1 = content2;
                                width1 = tempWidth;
                            }
                        }
                        hyperlink mdata = new hyperlink();
                        mdata.index = LineStr.Length;
                        mdata.Length = Length1;
                        mdata.content = result.mMatchHyperLinkList[HyperLinkIndex].content;
                        mdata.OriContent = content.Substring(0, Length1);
                        string temp = LineStr.Substring(0, mdata.index);
                        float xMin = GetWidth(mText, temp);
                        mdata.mMinPos = new Vector2(xMin, 0f);
                        temp = (LineStr + mdata.OriContent).Substring(0, mdata.index + mdata.Length);
                        float xMax = GetWidth(mText, temp);
                        mdata.mMaxPos = new Vector2(xMax, mText.rectTransform.rect.height);
                        mLineResult.mMatchHyperLinkList.Add(mdata);


                        mLineResult.resultStr = LineStr + content1;
                        resultList.Add(mLineResult);

                        LineStr = content.Substring(Length1, Length - Length1);
                        LineWidth = width - width1;

                        mLineResult = new ModifyStrResult();
                        hyperlink mdata1 = new hyperlink();
                        mdata1.index = 0;
                        mdata1.Length = Length - Length1;
                        mdata1.content = result.mMatchHyperLinkList[HyperLinkIndex].content;
                        mdata1.OriContent = content.Substring(Length1, Length - Length1);
                        mdata1.mMinPos = new Vector2(0f, 0f);
                        temp = (LineStr + mdata1.OriContent).Substring(0, mdata1.Length);
                        xMax = GetWidth(mText, temp);
                        mdata1.mMaxPos = new Vector2(xMax, mText.rectTransform.rect.height);
                        mLineResult.mMatchHyperLinkList.Add(mdata1);
                    }
                    else
                    {
                        hyperlink mdata = new hyperlink();
                        mdata.index = LineStr.Length;
                        mdata.Length = content.Length;
                        mdata.content = result.mMatchHyperLinkList[HyperLinkIndex].content;
                        mdata.OriContent = content;
                        string temp = (LineStr + content).Substring(0, mdata.index);
                        float xMin = GetWidth(mText, temp);
                        mdata.mMinPos = new Vector2(xMin, 0f);
                        temp = (LineStr + content).Substring(0, mdata.index + mdata.Length);
                        float xMax = GetWidth(mText, temp);
                        mdata.mMaxPos = new Vector2(xMax, mText.rectTransform.rect.height);
                        mLineResult.mMatchHyperLinkList.Add(mdata);

                        LineStr += content;
                        LineWidth += width;
                    }
                    i += Length;
                    HyperLinkIndex++;
                }
                else if (LineWidth + characterInfo.advance > mText.rectTransform.rect.width)
                {
                    mLineResult.resultStr = LineStr;
                    resultList.Add(mLineResult);

                    LineStr = text[i].ToString();
                    LineWidth = characterInfo.advance;
                    i++;
                    mLineResult = new ModifyStrResult();
                }
                else
                {
                    LineWidth += characterInfo.advance;
                    LineStr += text[i];
                    i++;
                }
            }
            if (LineStr.Length > 0)
            {
                mLineResult.resultStr = LineStr;
                resultList.Add(mLineResult);
            }

        }
        return resultList;
    }

    public const int TextType_Color_Common = 0;
    public const int TextType_Color_channel_System = 1;
    public const int TextType_Color_channel_World = 2;
    public const int TextType_Color_channel_Guild = 3;
    public const int TextType_Color_channel_Team = 4;
    public const int TextType_Color_channel_Private = 5;
    public const int TextType_Color_channel_Nearby = 6;

    public const int TextType_Color_HyperLink = 7;
    public const int TextType_Color_Equip = 8;

    public Color GetTextColor(int type)
    {
        switch(type)
        {
            case 1:
                return new Color(1f,0,0);
            case 2:
                return new Color(0, 1f, 0);
            case 3:
                return new Color(0, 0, 1f);
            case 4:
                return new Color(0f, 0, 1f);
            case 5:
                return new Color(0f, 1f, 0);
            case 6:
                return new Color(1f, 0, 0);
            case 7:
                return new Color(0, 1f, 0);
        }
        return Color.white;

    }
}
