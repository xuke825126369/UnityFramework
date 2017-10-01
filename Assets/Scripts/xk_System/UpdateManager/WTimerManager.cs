using System;
using System.Collections.Generic;

public class WTimerManager:Singleton<WTimerManager>
{
    private DateTime m_lastTime;
    private Dictionary<WTimer, bool> m_mapTimer;

    public WTimerManager()
    {
        this._init();
    }

    private void _init()
    {
        this.m_mapTimer = new Dictionary<WTimer, bool>();
    }

    private void _run(object objParam)
    {
        DateTime now = DateTime.Now;
        TimeSpan span = (TimeSpan)(now - this.m_lastTime);
        uint totalMilliseconds = (uint)span.TotalMilliseconds;


        foreach (KeyValuePair<WTimer, bool> pair in m_mapTimer)
        {
            pair.Key.wentBy(totalMilliseconds);
        }
        this.m_lastTime = now;
    }

    public void addTimer(WTimer target)
    {
        if (!this.hasTimer(target))
        {
            this.m_mapTimer.Add(target, true);
            if (!EnterFrame.Instance.exists(this._run))
            {
                this.m_lastTime = DateTime.Now;
                EnterFrame.Instance.add(this._run, null);
            }
        }
    }

    public bool hasTimer(WTimer target)
    {
        return this.m_mapTimer.ContainsKey(target);
    }

    public void removeTimer(WTimer target)
    {
        if (this.hasTimer(target))
        {
            this.m_mapTimer.Remove(target);
            if (this.m_mapTimer.Count == 0)
            {
                EnterFrame.Instance.remove(this._run);
            }
        }
    }
}


