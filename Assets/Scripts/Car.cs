using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class Car : MonoBehaviour
{
    public SpriteRenderer Sprite;

    public Collider2D Body;
    public Collider2D Sensor;
    public Road Road;

    public float Speed;
    public bool IsVertical;

    private Vector2 _direction;
    private Rigidbody2D _rigidbody;

    private float _startTime;

    public Vector2 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public float Rotation
    {
        get { return transform.eulerAngles.z; }
        set
        {
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.y,
                value);
        }
    }

    public float TravelTime { get { return Time.time - _startTime; } }

    public LayerMask StopLayer { get { return GameManager.Instance.CarStopLayer; } }

    public void Initialize(Road road)
    {
        IsVertical = road.Data.IsVertical;
        Position = road.Data.Start;
        Road = road;
        _direction = new Vector2(
            Mathf.Cos(Road.Data.Angle * Mathf.Deg2Rad),
            Mathf.Sin(Road.Data.Angle * Mathf.Deg2Rad));

        _startTime = Time.time;
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        Body.OnCollisionEnter2DAsObservable().Subscribe(OnCollision);
        Sensor.OnTriggerEnter2DAsObservable().Subscribe(OnSensorEnter);
        Sensor.OnTriggerStay2DAsObservable().Subscribe(OnSensorStay);
        Sensor.OnTriggerExit2DAsObservable().Subscribe(OnSensorExit);
    }

    private void OnCollision(Collision2D collision2D)
    {
        if (IsVertical)
        {
            var crossing = Physics2D.OverlapCircleAll(transform.position, 0.5f)
                .Select(other => other.GetComponentInParent<Crossing>())
                .Where(other => other == true)
                .OrderBy(other => Vector2.Distance(transform.position, other.transform.position))
                .FirstOrDefault();
            if (crossing)
            {
                crossing.Collide();
                Destroy(gameObject);
                Destroy(collision2D.gameObject);
            }
        }
    }

    private void Update()
    {
        Rotation = Road.Data.Angle;

        if (_stoppers.Count > 0)
        {
            _rigidbody.velocity = Vector2.zero;
        }
        else
        {
            _rigidbody.velocity = _direction.normalized * Speed;
        }
    }

    public void Complete()
    {
        if (IsVertical)
            GameManager.Instance.NotifyTripDone(this);
        Destroy(gameObject, 1);
    }

    private readonly List<GameObject> _stoppers = new List<GameObject>();
    private readonly List<GameObject> _signalsToIgnore = new List<GameObject>();
    private void OnSensorEnter(Collider2D collider2D)
    {
        if(LayerMask.NameToLayer("Road End") == collider2D.gameObject.layer)
            Complete();

        var signal = collider2D.GetComponentInParent<Crossing>();
        if (signal)
        {
            if (_signalsToIgnore.Contains(signal.gameObject))
                return;

            if (IsVertical && signal.AllowingVertical)
                _signalsToIgnore.Add(signal.gameObject);

            return;
        }

        var car = collider2D.GetComponentInParent<Car>();
        if (car)
        {
            if (car.IsVertical == IsVertical)
                _stoppers.Add(car.gameObject);
            return;
        }

        var zombie = collider2D.GetComponentInParent<Enemy>();
        if (zombie)
            _stoppers.Add(zombie.gameObject);
    }

    private void OnSensorStay(Collider2D collider2D)
    {
        if (!IsVertical)
            return;

        var signal = collider2D.GetComponentInParent<Crossing>();
        if (signal)
        {
            if (_signalsToIgnore.Contains(signal.gameObject))
                return;

            if (!signal.AllowingVertical && !_stoppers.Contains(signal.gameObject))
            {
                _stoppers.Add(signal.gameObject);
            }
            else if (signal.AllowingVertical && _stoppers.Contains(signal.gameObject))
            {
                _stoppers.Remove(signal.gameObject);
                _signalsToIgnore.Add(signal.gameObject);
            }
                
            return;
        }
    }

    private void OnSensorExit(Collider2D collider2D)
    {
        var signal = collider2D.GetComponentInParent<Crossing>();
        if (signal)
        {
            _stoppers.Remove(signal.gameObject);
        }

        var car = collider2D.GetComponentInParent<Car>();
        if (car)
        {
            _stoppers.Remove(car.gameObject);
        }

        var zombie = collider2D.GetComponentInParent<Enemy>();
        if (zombie)
            _stoppers.Remove(zombie.gameObject);
    }

}
