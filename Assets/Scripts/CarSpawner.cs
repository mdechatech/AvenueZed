using UnityEngine;
using System.Collections;

public class CarSpawner : MonoBehaviour
{
    public float ClearRadius;
    public float CheckInterval;
    public Road Road;

    private float _checkTimer;

    public Car CarPrefab { get { return GameManager.Instance.CarPrefab; } }
    public LayerMask CarLayer { get { return GameManager.Instance.CarLayer; } }

    public CarSpawner Initialize(Road road)
    {
        Road = road;
        transform.position = Road.Data.Start;
        return this;
    }

    private void Update()
    {
        if ((_checkTimer -= Time.deltaTime) < 0)
        {
            if (!Physics2D.OverlapBox(Road.Data.Start, new Vector2(ClearRadius * 2, 0), Road.Data.Angle, CarLayer))
            {
                SpawnCar();
            }

            _checkTimer = CheckInterval;
        }
    }

    private void SpawnCar()
    {
        var car = Instantiate(CarPrefab);
        car.Initialize(Road);
    }
}
