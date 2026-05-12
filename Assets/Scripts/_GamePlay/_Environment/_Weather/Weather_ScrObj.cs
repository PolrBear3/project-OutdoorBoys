using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Weather Event")]
public class Weather_ScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private string _upcomingInfoText;
    public string upcomingInfoText => _upcomingInfoText;

    [SerializeField][TextArea(3, 10)] private string _descriptionText;
    public string descriptionText => _descriptionText;

    [Space(20)]
    [SerializeField] private Sprite _iconSprite;
    public Sprite iconSprite => _iconSprite;

    [SerializeField] private GameObject _activePrefab;
    public GameObject activePrefab => _activePrefab;

    [Space(20)]
    [SerializeField][Range(0, 10)] private int _randomWeightValue;
    public int randomWeightValue => _randomWeightValue;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _activeDayPoint;
    public int activeDayPoint => _activeDayPoint;

    [SerializeField][Range(0, 100)] private int _activeTimePoint;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _minActiveTime;
    [SerializeField][Range(0, 100)] private int _maxActiveTime;


    public int Active_TimeCount()
    {
        return Mathf.Min(_activeTimePoint, InGame_Manager.instance.time.maxTimecount);
    }

    public int Random_ActiveTime()
    {
        return Mathf.Max(1, Random.Range(_minActiveTime, _maxActiveTime + 1));
    }
}
