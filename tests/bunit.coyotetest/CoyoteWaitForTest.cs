using AngleSharp.Dom;
using Bunit.Extensions.WaitForHelpers;
using Bunit.TestAssets.BlazorE2E;
using Microsoft.Coyote.SystematicTesting;
using Xunit.Abstractions;

namespace Bunit;

public class CoyoteWaitForTest
{
	[Test]
	public void CanDoubleDispatchRenderToSyncContext()
	{
		using var ctx = new TestContext();

		var cut = ctx.RenderComponent<DispatchingComponent>();
		var result = cut.Find("#result");

		cut.Find("#run-with-double-dispatch").Click();

		cut.WaitForAssertion(() => Assert.Equal("Success (completed synchronously)", result.TextContent.Trim()));
	}

	[Test]
	public static void Test110()
	{
		using var ctx = new TestContext();
		
		// Initial state is stopped
		var cut = ctx.RenderComponent<TwoRendersTwoChanges>();
		var stateElement = cut.Find("#state");
		stateElement.TextContent.ShouldBe("Stopped");

		// Clicking 'tick' changes the state, and starts a task
		cut.Find("#tick").Click();
		cut.Find("#state").TextContent.ShouldBe("Started");

		// Clicking 'tock' completes the task, which updates the state
		// This click causes two renders, thus something is needed to await here.
		cut.Find("#tock").Click();
		cut.WaitForAssertion(() => cut.Find("#state").TextContent.ShouldBe("Stopped"));
	}

	[Test]
	public static void Test100()
	{
		using var ctx = new TestContext();

		// Initial state is stopped
		var cut = ctx.RenderComponent<TwoRendersTwoChanges>();

		// Clicking 'tick' changes the state, and starts a task
		cut.Find("#tick").Click();
		cut.Find("#state").TextContent.ShouldBe("Started");

		// Clicking 'tock' completes the task, which updates the state
		// This click causes two renders, thus something is needed to await here.
		cut.Find("#tock").Click();
		cut.WaitForState(() =>
		{
			var elm = cut.Nodes.QuerySelector("#state");
			return elm?.TextContent == "Stopped";
		});
	}

	[Test]
	public static void Test200()
	{
		using var ctx = new TestContext();

		var cut = ctx.RenderComponent<AsyncRenderChangesProperty>();
		cut.Instance.Counter.ShouldBe(0);

		// Clicking 'tick' changes the counter, and starts a task
		cut.Find("#tick").Click();
		cut.Instance.Counter.ShouldBe(1);

		// Clicking 'tock' completes the task, which updates the counter
		// This click causes two renders, thus something is needed to await here.
		cut.Find("#tock").Click();
		cut.WaitForState(() => cut.Instance.Counter == 2);

		cut.Instance.Counter.ShouldBe(2);
	}	

	internal class ThrowsAfterAsyncOperation : ComponentBase
	{
		protected override async Task OnInitializedAsync()
		{
			await InvokeAsync(async () =>
			{
				await Task.Delay(100);
				throw new ThrowsAfterAsyncOperationException();
			});
		}

		internal sealed class ThrowsAfterAsyncOperationException : Exception { }
	}
}
