using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct TimedPosition
{
    public Vector3 pos;
    public DateTime timestamp;
}

public class TimedPositions:IEnumerable<TimedPosition> {
    private List<TimedPosition> positions;

    public TimedPositions(int _dataBufferSize) {
        dataBufferSize = _dataBufferSize;
        positions = new List<TimedPosition>();
    }

    public int Count {
        get {
            return positions.Count;
        }
    }

    const int IGNORE_AFTER_MS = 1000;

    private int dataBufferSize = 2;

    public IEnumerator<TimedPosition> GetEnumerator()
    {
        foreach (TimedPosition position in positions)
        {
            yield return position;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(TimedPosition _pos) {
        positions.RemoveAll(x=> x.timestamp.AddMilliseconds(IGNORE_AFTER_MS)<DateTime.Now);
        if ( positions.Count==0 || positions.Last<TimedPosition>().timestamp <= _pos.timestamp)
        {
            positions.Add(_pos);
        }
        else {
            positions.Add(_pos);
            positions.OrderBy(x => x.timestamp);
        }
        if (positions.Count > dataBufferSize) {
            positions.RemoveAt(0);
        }
    }

    public Vector3 GetPos(Vector3 _currentPos,bool _extrapolate) {
        int count = positions.Count;
        if (count == 1)
        {
            return positions[0].pos;
        }
        else {
            TimedPosition lastPos = positions[count - 1];
            TimedPosition preLastPos = positions[count - 2];
            if (lastPos.pos==preLastPos.pos)
            {
                return lastPos.pos;
            }
            else if (_extrapolate)
            {
                float deltaT = (float)(lastPos.timestamp- preLastPos.timestamp).TotalMilliseconds;
                if (deltaT == 0) return lastPos.pos;

                Vector3 increment = ((float)(DateTime.Now - lastPos.timestamp).TotalMilliseconds / deltaT)*(lastPos.pos-preLastPos.pos);
                return lastPos.pos + increment;
            }
            else {
                
                return lastPos.pos;
            }
        }
    }

    public Vector3 GetPosAtTime(DateTime _time) {
        int i = 0;
        foreach (TimedPosition tp in positions)
        {
            if (tp.timestamp >= _time)
            {
                Debug.Log($"Exited loop after {i} iterations");
                return tp.pos;
            }
            i++;
        }
        if (positions.Count > 0)
        {
            return positions[positions.Count - 1].pos;
        }
        else 
        {
            Debug.LogWarning("Positions are empty");
            return Vector3.zero;
        }
    }

    public void MoveData(Vector3 _direction,DateTime _time)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            positions[i] = new TimedPosition 
            { 
                pos = positions[i].pos + _direction, 
                timestamp = positions[i].timestamp 
            };
        }
    }
}

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth = 100f;
    public int itemCount = 0;
    public SpriteRenderer model;

    public float distanceTolerance = 0.1f;

    /// <summary>
    /// For local player contains local positions, for other players contains positions send by server
    /// </summary>
    private TimedPositions positions;

    void Awake() {
        positions = new TimedPositions(30);
        distanceTolerance *= distanceTolerance;
    }

    public void SetPosition(Vector3 _pos, DateTime _timestamp) {
        if (id == Client.instance.myId)
        {
            Vector3 localPos = positions.GetPosAtTime(_timestamp);
            if ((DateTime.Now - _timestamp).TotalMilliseconds > 1000) {
                Vector3 error = _pos-transform.position;
                Debug.Log("Major Desync");
                Debug.DrawLine(transform.position, _pos,Color.red);
                transform.position = _pos;
                positions.MoveData(error, _timestamp);

                transform.position = localPos;
            }
            if ((localPos - _pos).sqrMagnitude > distanceTolerance) {
                Vector3 error = _pos - localPos;
                Debug.Log("Desync");
                Debug.DrawLine(localPos,_pos);
                transform.position += error;
                positions.MoveData(error, _timestamp);
            }
        }
        else
        {
            positions.Add(new TimedPosition { pos = _pos, timestamp = _timestamp });
        }
    }

    public void FixedUpdate() {
        if (id == Client.instance.myId) 
        {
            positions.Add(new TimedPosition {pos=transform.position,timestamp=DateTime.Now });
        }
        else if (positions.Count > 0) 
        {
            transform.position = positions.GetPos(transform.position,true);
        }
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }
}
