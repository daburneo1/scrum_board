using Application.Tasks.Ordering;

using Application.Common.Exceptions;

namespace Application.Tests.Boards.Ordering;

public sealed class TaskOrderCalculatorTests
{
    private readonly TaskOrderCalculator _calculator = new();

    [Test]
    public void Calculate_ShouldMoveTaskToLastPosition_InSameColumn()
    {
        var columnId = Guid.NewGuid();

        var firstTaskId = Guid.NewGuid();
        var secondTaskId = Guid.NewGuid();
        var thirdTaskId = Guid.NewGuid();

        var result = _calculator.Calculate(
            firstTaskId,
            columnId,
            columnId,
            targetIndex: 2,
            sourceOrderedTaskIds:
            [
                firstTaskId,
                secondTaskId,
                thirdTaskId
            ],
            targetOrderedTaskIds:
            [
                firstTaskId,
                secondTaskId,
                thirdTaskId
            ]);

        var movedTask = result.Changes
            .Single(change =>
                change.TaskId == firstTaskId);

        Assert.That(
            movedTask.ColumnId,
            Is.EqualTo(columnId));

        Assert.That(
            movedTask.SortOrder,
            Is.EqualTo(3000));

        Assert.That(
            result.Changes.Single(change =>
                change.TaskId == secondTaskId).SortOrder,
            Is.EqualTo(1000));

        Assert.That(
            result.Changes.Single(change =>
                change.TaskId == thirdTaskId).SortOrder,
            Is.EqualTo(2000));
    }

    [Test]
    public void Calculate_ShouldInsertTaskAtRequestedIndex_InAnotherColumn()
    {
        var sourceColumnId = Guid.NewGuid();
        var targetColumnId = Guid.NewGuid();

        var movedTaskId = Guid.NewGuid();
        var sourceTaskId = Guid.NewGuid();

        var firstTargetTaskId = Guid.NewGuid();
        var secondTargetTaskId = Guid.NewGuid();

        var result = _calculator.Calculate(
            movedTaskId,
            sourceColumnId,
            targetColumnId,
            targetIndex: 1,
            sourceOrderedTaskIds:
            [
                movedTaskId,
                sourceTaskId
            ],
            targetOrderedTaskIds:
            [
                firstTargetTaskId,
                secondTargetTaskId
            ]);

        var movedTask = result.Changes
            .Single(change =>
                change.TaskId == movedTaskId);

        Assert.That(
            movedTask.ColumnId,
            Is.EqualTo(targetColumnId));

        Assert.That(
            movedTask.SortOrder,
            Is.EqualTo(2000));

        Assert.That(
            result.Changes.Single(change =>
                change.TaskId ==
                sourceTaskId).SortOrder,
            Is.EqualTo(1000));
    }

    [Test]
    public void Calculate_ShouldAllowMovingToEmptyColumn()
    {
        var sourceColumnId = Guid.NewGuid();
        var targetColumnId = Guid.NewGuid();

        var taskId = Guid.NewGuid();

        var result = _calculator.Calculate(
            taskId,
            sourceColumnId,
            targetColumnId,
            targetIndex: 0,
            sourceOrderedTaskIds:
            [
                taskId
            ],
            targetOrderedTaskIds:
            []);

        Assert.That(
            result.Changes,
            Has.Count.EqualTo(1));

        var movedTask = result.Changes.Single();

        Assert.That(
            movedTask.TaskId,
            Is.EqualTo(taskId));

        Assert.That(
            movedTask.ColumnId,
            Is.EqualTo(targetColumnId));

        Assert.That(
            movedTask.SortOrder,
            Is.EqualTo(1000));
    }

    [Test]
    public void Calculate_ShouldRejectOutOfRangeTargetIndex()
    {
        var columnId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        Assert.Throws<ValidationException>(() =>
            _calculator.Calculate(
                taskId,
                columnId,
                columnId,
                targetIndex: 10,
                sourceOrderedTaskIds:
                [
                    taskId
                ],
                targetOrderedTaskIds:
                [
                    taskId
                ]));
    }

    [Test]
    public void Calculate_ShouldRejectDuplicateTaskIdentifiers()
    {
        var columnId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        Assert.Throws<ValidationException>(() =>
            _calculator.Calculate(
                taskId,
                columnId,
                columnId,
                targetIndex: 0,
                sourceOrderedTaskIds:
                [
                    taskId,
                    taskId
                ],
                targetOrderedTaskIds:
                [
                    taskId,
                    taskId
                ]));
    }
}
