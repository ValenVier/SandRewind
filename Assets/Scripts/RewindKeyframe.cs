using UnityEngine;

/// <summary>One recorded snapshot of a transform (and optionally velocity) at a point in time</summary>
[System.Serializable]
public struct RewindKeyframe
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public RewindKeyframe(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
    {
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.angularVelocity = angularVelocity;
    }
}