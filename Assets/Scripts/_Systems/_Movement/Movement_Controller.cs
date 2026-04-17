using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField][Range(0, 10)] private float _defaultSpeed;

    private Coroutine _moveCoroutine;
    public Coroutine moveCoroutine => _moveCoroutine;

    private Vector2 _destination;
    
    
    // MonoBehaviour
    private void Awake()
    {
        
    }
    
    private void OnDestroy()
    {
        
    }

    private void Update()
    {
        Movement_Update();
    }


    // Move Update
    private void Movement_Update()
    {
        if (_moveCoroutine == null) return;

        transform.position = Vector2.MoveTowards(transform.position, _destination, _defaultSpeed * Time.deltaTime);
    }

    public bool At_Destination()
    {
        return Vector2.Distance(transform.position, _destination) <= 0.01f;
    }
    private IEnumerator MovementState_Update()
    {
        while (At_Destination() == false) yield return null;
        _moveCoroutine = null;
    }

    public void Stop()
    {
        if (_moveCoroutine == null) return;

        StopCoroutine(_moveCoroutine);
        _moveCoroutine = null;
    }

    public void Move(Vector2 customPosition)
    {
        _destination = customPosition;
        _moveCoroutine = StartCoroutine(MovementState_Update());
    }
    public void Move(Tile targetTile)
    {
        Move(targetTile.transform.position);
    }
}
