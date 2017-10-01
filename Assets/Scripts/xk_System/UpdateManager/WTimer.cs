using System;
/// <summary>
/// 固定一段时间做一件事
/// </summary>
public class WTimer
{
    private readonly uint m_fixUpdateTime=0;
    private readonly uint m_nRepeatCount;
    private uint m_nCurrentCount = 0;
    private uint m_nCurrentTime = 0;
    private WTimerCallBack m_objListeners;
    private bool m_bRunning;

    public WTimer(uint FrameTime, uint FrameCount,WTimerCallBack objListeners = null)
    {
        this.m_nRepeatCount = FrameCount;
        this.m_fixUpdateTime =FrameTime;
        this.m_objListeners = objListeners;
        this.m_nCurrentCount = 0;
        this.m_nCurrentTime = 0;
        m_bRunning = false;
    }

    public void start()
    {
        if (!this.m_bRunning)
        {
            this.m_bRunning = true;
            WTimerManager.Instance.addTimer(this);
            if ((this.m_objListeners != null) && (this.m_objListeners.onStart != null))
            {
                this.m_objListeners.onStart(this.m_objListeners.onStartParam);
            }
        }
    }

    public void reset()
    {
        this.stop();
        this.m_nCurrentCount = 0;
        this.m_nCurrentTime = 0;
        this.start();
    }

    public void stop()
    {
        WTimerManager.Instance.removeTimer(this);
        this.m_bRunning = false;
    }

    internal void wentBy(uint n)
    {
        if (this.m_bRunning)
        {
            this.m_nCurrentTime += n;
            if (this.m_nCurrentTime >= this.m_fixUpdateTime)
            {
                this.m_nCurrentTime = 0;
                this.m_nCurrentCount++;
                if ((this.m_objListeners != null) && (this.m_objListeners.onRunning != null))
                {
                    this.m_objListeners.onRunning(this.m_objListeners.onRunningParam);
                }
                if ((this.m_nRepeatCount > 0) && (this.m_nCurrentCount >= this.m_nRepeatCount))
                {
                    this.stop();
                    if ((this.m_objListeners != null) && (this.m_objListeners.onEnd != null))
                    {
                        Action<object> onEnd = this.m_objListeners.onEnd;
                        object onEndParam = this.m_objListeners.onEndParam;
                        onEnd(onEndParam);
                    }
                }
            }
            
        }
    }
}

public class WTimerCallBack
{
    public Action<object> onStart;
    public object onStartParam;
    public Action<object> onRunning;
    public object onRunningParam;
    public Action<object> onEnd;
    public object onEndParam;
}


