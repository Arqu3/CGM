using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Placed on a camera, this script moves between waypoints using an animation-curve
/// </summary>

public class IdleMovement : MonoBehaviour
{
    //Public vars
    [Header("Movement variables")]
    public AnimationCurve m_MovementCurve;
    [Range(0.1f, 100.0f)]
    public float m_MovementTime = 2.0f;
    [Range(0.0f, 100.0f)]
    public float m_ReachedDistance = 1.0f;
    [Range(1.0f, 1000.0f)]
    public float m_RotationSpeed = 30.0f;

    //Private vars
    private bool m_IsMoving = false;
    private bool m_IsRotating = false;
    private Waypoint m_CurrentWaypoint = null;
    private float m_CurrentDistance = 0.0f;
    private float m_CurrentMoveTime = 0.0f;
    Vector3 m_CurrentPosition;

	void Start()
    {
        if (!m_CurrentWaypoint)
        {
            SetWaypoint(FindNearestWaypoint());
            Debug.Log("No starting waypoint set! finding nearest: " + m_CurrentWaypoint.name);
        }
    }

    void OnEnable()
    {

    }
	
	void Update()
    {
		if (m_IsMoving && m_CurrentWaypoint)
        {
            if (m_CurrentMoveTime <= m_MovementTime)
            {
                m_CurrentMoveTime += Time.deltaTime;
                transform.position = Vector3.Lerp(m_CurrentPosition, m_CurrentWaypoint.transform.position, m_MovementCurve.Evaluate(m_CurrentMoveTime / m_MovementTime));

                if (Vector3.Distance(transform.position, m_CurrentWaypoint.transform.position) <= m_ReachedDistance)
                {
                    m_IsMoving = false;
                    m_IsRotating = true;
                }
            }
            else
                m_IsMoving = false;
        }
        else if (!m_IsMoving && m_CurrentWaypoint && !m_IsRotating)
            SetWaypoint(m_CurrentWaypoint.GetCustomTargets()[0]);
        else if (m_IsRotating)
        {
            Rotate();
        }
	}

    public void SetWaypoint(Waypoint moveTo)
    {
        m_CurrentPosition = transform.position;
        m_CurrentMoveTime = 0.0f;
        m_CurrentWaypoint = moveTo;
        m_IsMoving = true;
        m_CurrentDistance = Vector3.Distance(transform.position, m_CurrentWaypoint.transform.position);
    }

    void Rotate()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * m_RotationSpeed);
    }

    public Waypoint FindNearestWaypoint()
    {
        var waypoints = FindObjectsOfType<Waypoint>();
        Waypoint near = null;
        float dist = Mathf.Infinity;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float currentDist = Vector3.Distance(transform.position, waypoints[i].transform.position);
            if (currentDist < dist)
            {
                near = waypoints[i];
                dist = currentDist;
            }
        }

        return near;
    }
}
