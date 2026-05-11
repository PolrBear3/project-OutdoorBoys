using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_Rain : WeatherEvent
{
    public override void Activate_Event()
    {
        Debug.Log("Rain");
    }
}
