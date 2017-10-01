using System;
using System.Collections.Generic;
/// <summary>
/// 每一帧做一件事
/// </summary>
public class EnterFrame : Singleton<EnterFrame>
{
    private Dictionary<Action<object>, object> m_dicListeners;
    public EnterFrame()
    {
        this._init();
    }

    private void _init()
    {
        this.m_dicListeners = new Dictionary<Action<object>, object>();
    }

    public void add(Action<object> listener, object objParam)
    {
        if (!m_dicListeners.ContainsKey(listener))
        {
            m_dicListeners.Add(listener,objParam);
        }
    }
    public bool exists(Action<object> fun)
    {
        return m_dicListeners.ContainsKey(fun);
    }

    public void remove(Action<object> listener)
    {
        if (m_dicListeners.ContainsKey(listener))
        {
            this.m_dicListeners.Remove(listener);
        }
    }

    public void update()
    {
        if (this.m_dicListeners.Count > 0)
        {
            Dictionary<Action<object>, object> mDic = new Dictionary<Action<object>, object>(m_dicListeners);
            foreach (KeyValuePair<Action<object>, object> pair in mDic)
            {
                pair.Key(pair.Value);
            }
        }
    }
}


