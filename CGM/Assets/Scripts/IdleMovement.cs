using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Ludwig Gustavsson
/// Email: ludwiggustavsson3@gmail.com
/// Last edited: 23/02/17
/// 
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
    [Range(0.1f, 100.0f)]
    public float m_Speed = 1.0f;
    [Range(0.0f, 100.0f)]
    public float m_ReachedDistance = 1.0f;

    [Header("Rotation variables")]
    public AnimationCurve m_RotationCurve;
    [Range(1.0f, 1000.0f)]
    public float m_RotationTime = 30.0f;
    [Range(0.5f, 10.0f)]
    public float m_LookAngle = 1.0f;
    public bool m_LookAtCurrent = false;

    [Header("Control variables")]
    public KeyCode m_DisableKey = KeyCode.Return;

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
    private Quaternion m_LookAtRot;
    private Transform m_CurrentLookAt;

    //Reset
    private bool m_Enabled = true;
    private bool m_Active = true;

    void Awake()
    {
        OnStart();
    }

    void OnStart()
    {
        m_CurrentWaypoint = m_StartWaypoint;
        if (!m_CurrentWaypoint)
        {
            SetWaypoint(FindNearestWaypoint());
            Debug.Log("No starting waypoint set! found nearest: " + m_CurrentWaypoint.name);
        }
        //transform.position = m_CurrentWaypoint.transform.position;
        //m_LastWaypointPos = transform.position;
    }

    void OnEnable()
    {
        if (!m_Enabled)
        {
            OnStart();
            m_Enabled = true;
            m_Active = true;
        }
    }

    void OnDisable()
    {
        m_Enabled = false;
        m_Active = false;
    }

    void OnDrawGizmos()
    {
        if (m_CurrentLookAt)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(m_CurrentLookAt.position, new Vector3(1.2f, 1.2f, 1.2f));
        }
    }
	
	void Update()
    {
        if (Input.GetKeyDown(m_DisableKey))
        {
            m_Active = !m_Active;
            if (m_Active)
                OnStart();
        }

        if (m_Enabled && m_Active)
            MovementUpdate();
	}

    void MovementUpdate()
    {
        if (m_IsMoving && m_CurrentWaypoint)
        {
            if (m_CurrentMoveTime <= m_MovementTime)
            {
                if (m_LookAtCurrent)
                {
                    if (!m_HasSetLook)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, m_LookAtRot, Time.deltaTime);

                        if (Quaternion.Angle(transform.rotation, m_LookAtRot) < m_LookAngle)
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
                    transform.LookAt(m_CurrentLookAt);

                    m_CurrentMoveTime += Time.deltaTime;
                    transform.position = Vector3.Lerp(m_LastWaypointPos, m_CurrentWaypoint.transform.position, m_Speed * m_MovementCurve.Evaluate(m_CurrentMoveTime / m_MovementTime));

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
        //m_LookAtRot = Vector3.zero;

        if (m_CurrentWaypoint && m_LookAtCurrent)
        {
            m_CurrentLookAt = m_CurrentWaypoint.GetCustomTargets()[0].transform;
            Vector3 dir = m_CurrentLookAt.position - transform.position;
            m_LookAtRot = Quaternion.LookRotation(dir.normalized);
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
