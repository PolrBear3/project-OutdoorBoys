using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Weather Event")]
public class Weather_ScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private string _upcomingInfoText;
    public string upcomingInfoText => _upcomingInfoText;

    [SerializeField][TextArea(3, 10)] private string _runningInfoText;
    public string runningInfoText => _runningInfoText;

    [Space(20)]
    [SerializeField] private GameObject _activePrefab;
    public GameObject activePrefab => _activePrefab;

    [Space(20)]
    [SerializeField][Range(0, 10)] private int _randomWeightValue;
    public int randomWeightValue => _randomWeightValue;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _activeDayCount;
    public int activeDayCount => _activeDayCount;

    [SerializeField][Range(0, 100)] private int _activeTimeCount;
    public int activeTimeCount => _activeDayCount;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _minActiveTime;
    [SerializeField][Range(0, 100)] private int _maxActiveTime;


    public int Random_ActiveTime()
    {
        return Mathf.Max(1, Random.Range(_minActiveTime, _maxActiveTime + 1));
    }
}
