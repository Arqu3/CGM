using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Author: Ludvig Grönborg
    Email: ludviggronborg@hotmail.com
    Phone: 0730654281  
    Last Edited: 2017/02/10
*/

/*
    SUMMARY
    Placed on each waypoint
    The script representation of a waypoint
*/

[RequireComponent(typeof(MeshRenderer))]
public class Waypoint : MonoBehaviour {
    // PUBLIC
    public delegate void WaypointEvent();
    public event WaypointEvent OnArrive;
    public event WaypointEvent OnDeparture;

    // SERALIZE PRIVATE
    [SerializeField]
    private List<Waypoint> m_customTargets;

    // PRIVATE
    private MeshRenderer m_meshRenderer;
    private List<Waypoint> m_reachableTargets;

    void Awake(){
        if ((m_meshRenderer = GetComponent<MeshRenderer>()) == null)
            print("Error(Waypoint): Waypoint has no MeshRenderer");
    }

    // For changing waypoint mode
    public void SetReachableTargets(List<Waypoint> reachableWaypoints){
        m_reachableTargets = reachableWaypoints;
    }

    // Use when unlocking new waypoints
    public void AddCustomTarget(Waypoint target){
        if(!m_customTargets.Contains(target))
            m_customTargets.Add(target);
    }

    public void Arrive(){
        if (OnArrive != null)
            OnArrive();
    }

    public void Depart(){
        if (OnDeparture != null)
            OnDeparture();
    }

    public List<Waypoint> GetReachableTargets(){
        return m_reachableTargets;
    }

    public List<Waypoint> GetCustomTargets(){
        return m_customTargets;
    }

    public void SetMeshRenderedState(bool state){
        if(m_meshRenderer.enabled != state)
            m_meshRenderer.enabled = state;
    }
}
