using System.Collections.Generic;

/// <summary>Plain C# ring buffer of RewindKeyframes; no Unity deps, so it's unit-testable directly</summary>
public class RewindBuffer
{
    private readonly List<RewindKeyframe> keyframes = new List<RewindKeyframe>();
    private readonly int maxKeyframes;

    public int Count => keyframes.Count;
    public bool HasHistory => keyframes.Count > 0;

    public RewindBuffer(int maxKeyframes)
    {
        this.maxKeyframes = maxKeyframes;
    }

    /// <summary>Adds a keyframe, evicting the oldest one if over capacity</summary>
    public void Record(RewindKeyframe frame)
    {
        keyframes.Add(frame);

        if (keyframes.Count > maxKeyframes)
        {
            keyframes.RemoveAt(0);
        }
    }

    /// <summary>Pops the last keyframe as "current" plus the one before it as "previous"; false if empty</summary>
    public bool PopPair(out RewindKeyframe current, out RewindKeyframe previous)
    {
        int lastIndex = keyframes.Count - 1;

        if (lastIndex < 0)
        {
            current = default;
            previous = default;
            return false;
        }

        int secondToLastIndex = lastIndex - 1;

        current = keyframes[lastIndex];
        previous = secondToLastIndex >= 0 ? keyframes[secondToLastIndex] : keyframes[lastIndex];

        keyframes.RemoveAt(lastIndex);
        return true;
    }
}