using Application.RealTime.Boards;

namespace Application.Tests.Boards;

public sealed class InMemoryBoardPresenceTrackerTests
{
    [Test]
    public void Join_ShouldCountDistinctUsers_NotConnections()
    {
        var tracker = new InMemoryBoardPresenceTracker();
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var firstSnapshot = tracker.Join(
            projectId,
            userId,
            "Ana",
            "connection-1");

        var secondSnapshot = tracker.Join(
            projectId,
            userId,
            "Ana",
            "connection-2");

        Assert.That(
            firstSnapshot.ConnectedUserCount,
            Is.EqualTo(1));

        Assert.That(
            secondSnapshot.ConnectedUserCount,
            Is.EqualTo(1));
    }

    [Test]
    public void Leave_ShouldKeepUserPresent_UntilLastConnectionLeaves()
    {
        var tracker = new InMemoryBoardPresenceTracker();
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        tracker.Join(
            projectId,
            userId,
            "Ana",
            "connection-1");

        tracker.Join(
            projectId,
            userId,
            "Ana",
            "connection-2");

        var firstLeaveSnapshot = tracker.Leave(
            projectId,
            "connection-1");

        var secondLeaveSnapshot = tracker.Leave(
            projectId,
            "connection-2");

        Assert.That(
            firstLeaveSnapshot.ConnectedUserCount,
            Is.EqualTo(1));

        Assert.That(
            secondLeaveSnapshot.ConnectedUserCount,
            Is.EqualTo(0));
    }

    [Test]
    public void Join_ShouldKeepSameUserPresenceIndependentAcrossProjects()
    {
        var tracker = new InMemoryBoardPresenceTracker();
        var firstProjectId = Guid.NewGuid();
        var secondProjectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        tracker.Join(
            firstProjectId,
            userId,
            "Ana",
            "connection-1");

        tracker.Join(
            secondProjectId,
            userId,
            "Ana",
            "connection-1");

        tracker.Leave(
            firstProjectId,
            "connection-1");

        Assert.That(
            tracker.GetSnapshot(firstProjectId).ConnectedUserCount,
            Is.EqualTo(0));

        Assert.That(
            tracker.GetSnapshot(secondProjectId).ConnectedUserCount,
            Is.EqualTo(1));
    }

    [Test]
    public void RemoveConnection_ShouldCleanAllProjectMemberships()
    {
        var tracker = new InMemoryBoardPresenceTracker();
        var firstProjectId = Guid.NewGuid();
        var secondProjectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        tracker.Join(
            firstProjectId,
            userId,
            "Ana",
            "connection-1");

        tracker.Join(
            secondProjectId,
            userId,
            "Ana",
            "connection-1");

        var snapshots = tracker.RemoveConnection(
            "connection-1");

        Assert.That(
            snapshots.Select(snapshot => snapshot.ProjectId),
            Is.EquivalentTo(new[]
            {
                firstProjectId,
                secondProjectId
            }));

        Assert.That(
            snapshots,
            Has.All.Matches<BoardPresenceSnapshot>(
                snapshot => snapshot.ConnectedUserCount == 0));
    }
}
