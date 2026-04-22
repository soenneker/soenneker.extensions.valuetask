using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;

namespace Soenneker.Extensions.ValueTask.Tests;

public class ValueTaskExtensionTests
{
    [Test]
    public void Default()
    {

    }

    [Test]
    public void AwaitSyncSafe_ShouldCompleteSuccessfully_WhenValueTaskCompletes()
    {
        // Arrange
        System.Threading.Tasks.ValueTask task = new System.Threading.Tasks.ValueTask(Task.Delay(50, CancellationToken.None));

        // Act
        Action act = () => task.AwaitSyncSafe();

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void AwaitSyncSafe_ShouldReturnResult_WhenValueTaskOfTCompletes()
    {
        // Arrange
        ValueTask<int> task = new ValueTask<int>(42);

        // Act
        int result = task.AwaitSyncSafe(CancellationToken.None);

        // Assert
        result.Should().Be(42);
    }

    [Test]
    public void AwaitSyncSafe_ShouldThrowException_WhenValueTaskThrows()
    {
        // Arrange
        System.Threading.Tasks.ValueTask task = new System.Threading.Tasks.ValueTask(Task.FromException(new InvalidOperationException("bad")));

        // Act
        Action act = () => task.AwaitSyncSafe();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("bad");
    }

    [Test]
    public void AwaitSyncSafe_ShouldThrowException_WhenValueTaskOfTThrows()
    {
        // Arrange
        ValueTask<string> task = new ValueTask<string>(Task.FromException<string>(new ArgumentNullException("param")));

        // Act
        Action act = () => task.AwaitSyncSafe();

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("param");
    }

    [Test]
    public void AwaitSyncSafe_ShouldHonorCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        System.Threading.Tasks.ValueTask task = new System.Threading.Tasks.ValueTask(Task.Delay(1000, cts.Token));

        // Act
        Action act = () => task.AwaitSyncSafe(cts.Token);

        // Assert
        act.Should().Throw<OperationCanceledException>();
    }

    [Test]
    public void AwaitSyncSafe_TOfT_ShouldHonorCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        ValueTask<string> task = new ValueTask<string>(Task.FromCanceled<string>(cts.Token));

        // Act
        Action act = () => task.AwaitSyncSafe(cts.Token);

        // Assert
        act.Should().Throw<OperationCanceledException>();
    }
}
