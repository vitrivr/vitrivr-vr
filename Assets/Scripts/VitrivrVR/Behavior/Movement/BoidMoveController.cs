using System;
using System.Collections.Generic;
using UnityEngine;

namespace VitrivrVR.Behavior.Movement
{
  public class BoidMoveController : MonoBehaviour
  {
    public float neighborForceWeight = 1;
    public float radiusForceWeight = 1;
    public float heightForceWeight = 1;
    public float minimumHeight = 0.5f;
    public float maximumHeight = 2f;
    public float maximumNeighborForce = 1;

    private Rigidbody _rb;
    private readonly List<Transform> _neighbors = new List<Transform>();
    private Transform _radiusTarget;
    private float _targetRadius;
    private float _squaredMaxForce;

    private void Awake()
    {
      _rb = GetComponent<Rigidbody>();
      InitializeTarget(transform.parent);
      _squaredMaxForce = maximumNeighborForce * maximumNeighborForce;
    }

    private void Update()
    {
      transform.LookAt(_radiusTarget);
    }

    private void FixedUpdate()
    {
      var position = transform.position;
      
      var neighborForce = Vector3.zero;

      // Neighbor forces
      foreach (var neighbor in _neighbors)
      {
        var separation = position - neighbor.position;
        var sqrDistance = separation.sqrMagnitude;
        neighborForce += neighborForceWeight / sqrDistance * separation;
      }

      if (neighborForce.sqrMagnitude > _squaredMaxForce)
      {
        neighborForce = neighborForce.normalized * maximumNeighborForce;
      }
      
      _rb.AddForce(neighborForce);

      // Radius force
      var radiusTargetPosition = _radiusTarget.position;
      var radiusSeparation = position - radiusTargetPosition;
      var radius = radiusSeparation.magnitude;
      var radiusDiff = _targetRadius - radius;
      _rb.AddForce(radiusForceWeight * radiusDiff * radiusSeparation.normalized);
      
      // Height force
      if (position.y < minimumHeight)
      {
        var diff = minimumHeight - position.y;
        _rb.AddForce(heightForceWeight * diff * Vector3.up);
      } else if (position.y > maximumHeight)
      {
        var diff = position.y - maximumHeight;
        _rb.AddForce(heightForceWeight * diff * Vector3.down);
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      _neighbors.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
      _neighbors.Remove(other.transform);
    }

    /// <summary>
    /// Initialize the radius target for this Boid.
    /// </summary>
    /// <param name="radiusTarget">Target to preserve current radius to.</param>
    private void InitializeTarget(Transform radiusTarget)
    {
      _radiusTarget = radiusTarget;
      _targetRadius = Vector3.Distance(transform.position, radiusTarget.position);
    }
  }
}