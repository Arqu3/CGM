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
    public Waypoint m_StartWaypoint;
    public AnimationCurve m_MovementCurve;
    [Range(0.1f, 100.0f)]
    public float m_MovementTime = 2.0f;
    [Range(0.0f, 100.0f)]
    public float m_ReachedDistance = 1.0f;

    [Header("Rotation variables")]
    public AnimationCurve m_RotationCurve;
    [Range(1.0f, 1000.0f)]
    public float m_RotationTime = 30.0f;
    [Range(0.5f, 10.0f)]
    public float m_LookAngle = 1.0f;
    public bool m_LookAtCurrent = false;

    //Private vars
    //Waypoint
    private Waypoint m_CurrentWaypoint = null;
    Vector3 m_LastWaypointPos;

    //Movement
    private bool m_IsMoving = false;
    private float m_CurrentMoveTime = 0.0f;

    //Rotation
    private bool m_IsRotating = false;
    private bool m_HasResetRot = false;
    private bool m_HasSetLook = false;
    private float m_RotationTimer = 0.0f;
    private float m_CurrentRot = 360.0f;
    private Vector3 m_LookAtRot;

    void Awake()
    {
        m_CurrentWaypoint = m_StartWaypoint;
        if (!m_CurrentWaypoint)
        {
            SetWaypoint(FindNearestWaypoint());
            Debug.Log("No starting waypoint set! found nearest: " + m_CurrentWaypoint.name);
        }
    }

    void OnEnable()
    {

    }

    void OnDisable()
    {

    }
	
	void Update()
    {
		if (m_IsMoving && m_CurrentWaypoint)
        {
            if (m_CurrentMoveTime <= m_MovementTime)
            {
                if (m_LookAtCurrent)
                {
                    if (!m_HasSetLook)
                    {
                        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(m_LookAtRot.x, m_LookAtRot.y, 0.0f), Time.deltaTime);
                        transform.rotation = Quaternion.Euler(Mathf.LerpAngle(transform.rotation.eulerAngles.x, m_LookAtRot.x, Time.deltaTime), Mathf.LerpAngle(transform.rotation.eulerAngles.y, m_LookAtRot.y, Time.deltaTime), 0.0f);

                        if (Quaternion.Angle(transform.rotation, Quaternion.Euler(m_LookAtRot.x, m_LookAtRot.y, 0.0f)) < m_LookAngle)
                            m_HasSetLook = true;
                    }
                }
                else
                {
                    if (!m_HasSetLook)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f), Time.deltaTime);

                        if (Quaternion.Angle(transform.rotation, Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f)) < m_LookAngle)
                            m_HasSetLook = true;
                    }
                }

                if (m_HasSetLook)
                {
                    m_CurrentMoveTime += Time.deltaTime;
                    transform.position = Vector3.Lerp(m_LastWaypointPos, m_CurrentWaypoint.transform.position, m_MovementCurve.Evaluate(m_CurrentMoveTime / m_MovementTime));

                    if (Vector3.Distance(transform.position, m_CurrentWaypoint.transform.position) <= m_ReachedDistance)
                    {
                        m_IsMoving = false;
                        m_IsRotating = true;
                    }
                }
            }
            else
                m_IsMoving = false;
        }
        else if (!m_IsMoving && m_CurrentWaypoint && !m_IsRotating)
        {
            Waypoint WP = m_CurrentWaypoint.GetCustomTargets()[0];
            if (WP)
                SetWaypoint(WP);
            else if (m_StartWaypoint)
                SetWaypoint(m_StartWaypoint);
        }
        else if (m_IsRotating)
            StartRotation();
	}

    public void SetWaypoint(Waypoint moveTo)
    {
        m_LastWaypointPos = transform.position;
        m_CurrentMoveTime = 0.0f;
        m_RotationTimer = 0.0f;
        m_HasResetRot = false;
        m_HasSetLook = false;
        m_CurrentRot = 360.0f;
        m_CurrentWaypoint = moveTo;
        m_IsMoving = true;
        m_LookAtRot = Vector3.zero;

        if (m_CurrentWaypoint && m_LookAtCurrent)
        {
            Vector3 dir = m_CurrentWaypoint.transform.position - transform.position;
            Vector3 dir2 = transform.forward;
            dir2.y = 0.0f;
            m_LookAtRot = Quaternion.FromToRotation(dir2.normalized, dir.normalized).eulerAngles;
        }
    }

    void StartRotation()
    {
        if (m_LookAtCurrent)
        {
            transform.rotation = Quaternion.Slerp(Quaternion.Euler(transform.rotation.eulerAngles), Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f), 2.0f * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f)) < 0.5f)
            {
                transform.rotation = Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f);
                m_HasResetRot = true;
            }
            if (m_HasResetRot)
                Rotate();
        }
        else
            Rotate();
    }

    void Rotate()
    {
        m_RotationTimer += Time.deltaTime;
        transform.Rotate(Vector3.up, m_RotationCurve.Evaluate(m_RotationTimer / m_RotationTime));

        m_CurrentRot -= m_RotationCurve.Evaluate(m_RotationTimer / m_RotationTime);
        if (m_CurrentRot <= 0.0f)
            m_IsRotating = false;
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
