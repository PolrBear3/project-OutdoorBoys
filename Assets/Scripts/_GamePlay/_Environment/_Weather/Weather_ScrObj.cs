using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Weather Event")]
public class Weather_ScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField][TextArea(3, 10)] private string _upcomingInfoText;

    [SerializeField][TextArea(3, 10)] private string _descriptionText;
    public string descriptionText => _descriptionText;

    [Space(20)]
    [SerializeField] private Sprite _iconSprite;
    public Sprite iconSprite => _iconSprite;

    [SerializeField] private GameObject _activePrefab;
    public GameObject activePrefab => _activePrefab;

    [Space(40)]
    [SerializeField] private bool _persistingBackground;
    public bool persistingBackground => _persistingBackground;

    [Space(10)]
    [SerializeField][Range(0, 10)] private int _warpUpdateOrderNum;
    public int warpUpdateOrderNum => _warpUpdateOrderNum;

    [SerializeField] private WarpRenderer_Data _warpUpdateVisualData;
    public WarpRenderer_Data warpUpdateVisualData => _warpUpdateVisualData;

    [Space(10)]
    [SerializeField] private TileIndicator_VisualData _activateTileVisuals;
    public TileIndicator_VisualData activateTileVisuals => _activateTileVisuals;

    [Space(40)]
    [SerializeField][Range(0, 10)] private int _rateValue;
    public int rateValue => _rateValue;

    [Space(10)]
    [SerializeField][Range(0, 10)] private int _persistTimeCount;
    public int persistTimeCount => _persistTimeCount;

    [SerializeField][Range(0, 10)] private int _persistUpcomingTimeCount;
    public int persistUpcomingTimeCount => _persistUpcomingTimeCount;

    [Space(10)]
    [SerializeField] private TimeRange_Data _timeRangeData;
    public TimeRange_Data timeRangeData => _timeRangeData;

    [Space(10)]
    [SerializeField] private TileState[] _tileStatesToRemove;
    public TileState[] tileStatesToRemove => _tileStatesToRemove;

    [SerializeField] private TileState[] _tileStatesToAdd;
    public TileState[] tileStatesToAdd => _tileStatesToAdd;


    // Text Template
    public string UpcomingInfo(int upcomingTimeValue)
    {
        return _upcomingInfoText.Replace("{upcomingTimeValue}", upcomingTimeValue.ToString());
    }
}
