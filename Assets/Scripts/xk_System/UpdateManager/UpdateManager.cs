using UnityEngine;
using System.Collections;
using System;

public class UpdateManager : SingleTonMonoBehaviour<UpdateManager>
{
    public void xStartCoroutine(IEnumerator fun)
    {
        StartCoroutine(fun);
    }

    public void xStopCoroutine(IEnumerator fun)
    {
        StopCoroutine(fun);
    }

    private void Update()
    {
        EnterFrame.Instance.update();
    }
}
