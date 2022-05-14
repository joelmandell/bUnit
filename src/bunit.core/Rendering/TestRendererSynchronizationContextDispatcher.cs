// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Bunit.Rendering;

internal class TestRendererSynchronizationContextDispatcher : Dispatcher
{
	private readonly TestRendererSynchronizationContext context;

	public TestRendererSynchronizationContextDispatcher()
	{
		context = new TestRendererSynchronizationContext();
		context.UnhandledException += (sender, e) =>
		{
			OnUnhandledException(e);
		};
	}

	public override bool CheckAccess()
		=> SynchronizationContext.Current == context;

	public override Task InvokeAsync(Action workItem)
	{
		if (CheckAccess())
		{
			workItem();
			return Task.CompletedTask;
		}

		return context.InvokeAsync(workItem);
	}

	public override Task InvokeAsync(Func<Task> workItem)
	{
		if (CheckAccess())
		{
			return workItem();
		}

		return context.InvokeAsync(workItem);
	}

	public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
	{
		if (CheckAccess())
		{
			return Task.FromResult(workItem());
		}

		return context.InvokeAsync<TResult>(workItem);
	}

	public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
	{
		if (CheckAccess())
		{
			return workItem();
		}

		return context.InvokeAsync<TResult>(workItem);
	}
}
