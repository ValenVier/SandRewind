using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode tests for RewindBuffer, written following the AAA
/// (Arrange-Act-Assert) pattern and MethodName_Scenario_ExpectedBehavior
/// naming convention from the Testing lesson.
///
/// Setup note: place this file under an EditMode test assembly (e.g.
/// Assets/Tests/EditMode/) with an .asmdef that references
/// "UnityEngine.TestRunner" and "UnityEditor.TestRunner", includes the
/// precompiled reference "nunit.framework.dll", and has "Test Assemblies"
/// checked. Run from Window > General > Test Runner.
/// </summary>
public class RewindBufferTests
{
    private static RewindKeyframe MakeFrame(float x)
    {
        return new RewindKeyframe(new Vector3(x, 0f, 0f), Quaternion.identity, Vector3.zero, Vector3.zero);
    }

    [Test]
    public void Record_SingleFrame_IncreasesCountToOne()
    {
        // ─────── ARRANGE ───────
        var buffer = new RewindBuffer(maxKeyframes: 10);
        RewindKeyframe frame = MakeFrame(1f);

        // ─────── ACT ───────
        buffer.Record(frame);

        // ─────── ASSERT ───────
        Assert.AreEqual(1, buffer.Count, "Recording one frame should bring count to 1");
    }

    [Test]
    public void Record_ExceedsMaxKeyframes_DropsOldestFrame()
    {
        // ─────── ARRANGE ───────
        var buffer = new RewindBuffer(maxKeyframes: 3);

        // ─────── ACT ───────
        buffer.Record(MakeFrame(1f));
        buffer.Record(MakeFrame(2f));
        buffer.Record(MakeFrame(3f));
        buffer.Record(MakeFrame(4f)); // buffer is full, should evict frame 1

        // ─────── ASSERT ───────
        Assert.AreEqual(3, buffer.Count, "Buffer should never exceed maxKeyframes");
    }

    [Test]
    public void PopPair_EmptyBuffer_ReturnsFalse()
    {
        // ─────── ARRANGE ───────
        var buffer = new RewindBuffer(maxKeyframes: 10);

        // ─────── ACT ───────
        bool result = buffer.PopPair(out _, out _);

        // ─────── ASSERT ───────
        Assert.IsFalse(result, "Popping an empty buffer should fail rather than throw");
    }

    [Test]
    public void PopPair_SingleFrame_CurrentAndPreviousAreSameFrame()
    {
        // ─────── ARRANGE ───────
        var buffer = new RewindBuffer(maxKeyframes: 10);
        RewindKeyframe expected = MakeFrame(5f);
        buffer.Record(expected);

        // ─────── ACT ───────
        buffer.PopPair(out RewindKeyframe current, out RewindKeyframe previous);

        // ─────── ASSERT ───────
        Assert.AreEqual(expected.position, current.position, "Current should be the only frame recorded");
        Assert.AreEqual(expected.position, previous.position,
            "With only one frame, previous should equal current (no earlier frame to interpolate from)");
    }

    [Test]
    public void PopPair_TwoFrames_RemovesOnlyTheLastOne()
    {
        // ─────── ARRANGE ───────
        var buffer = new RewindBuffer(maxKeyframes: 10);
        RewindKeyframe first = MakeFrame(1f);
        RewindKeyframe second = MakeFrame(2f);
        buffer.Record(first);
        buffer.Record(second);

        // ─────── ACT ───────
        buffer.PopPair(out RewindKeyframe current, out RewindKeyframe previous);

        // ─────── ASSERT ───────
        Assert.AreEqual(second.position, current.position, "Current should be the most recently recorded frame");
        Assert.AreEqual(first.position, previous.position, "Previous should be the frame recorded right before current");
        Assert.AreEqual(1, buffer.Count, "Only the last frame should have been removed, not both");
    }

    [Test]
    public void HasHistory_AfterPoppingLastFrame_ReturnsFalse()
    {
        // ─────── ARRANGE ───────
        var buffer = new RewindBuffer(maxKeyframes: 10);
        buffer.Record(MakeFrame(1f));

        // ─────── ACT ───────
        buffer.PopPair(out _, out _);

        // ─────── ASSERT ───────
        Assert.IsFalse(buffer.HasHistory, "Buffer should report no history once its only frame has been popped");
    }
}
