using System;
using System.Threading;
using System.Threading.Tasks;

namespace HybridWebPlatform.HybridWeb.Utilites
{
	public interface IHybridTask : IDisposable
	{
		Task GetCurrentTask();
		void ReleaseThread();
		void Cancel();
	}

	public class HybridTask<T> : IHybridTask
	{
		public Task<T> CurrentTask
		{
			get;
			private set;
		}

		private EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
		private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

		private Action initialAction;
		private Func<T> collectingResults;

		public HybridTask(Action initialAction, Func<T> collectingResults)
		{
			this.initialAction = initialAction;
			this.collectingResults = collectingResults;

			CurrentTask = Init();
		}

		private Task<T> Init()
		{
			waitHandle.Reset();

			Task<T> task = new Task<T>(
				() =>
					{
						//Here we execute work
						initialAction();

						//Here we wait for work execution finish called;
						waitHandle.WaitOne();

						//Here we call to wrap the results
						return collectingResults();
					}, cancelTokenSource.Token
			);
			return task;
		}

		public void ReleaseThread()
		{
			waitHandle.Set();
		}

		public void Cancel()
		{
			cancelTokenSource.Cancel();
		}

		public void Dispose()
		{
			waitHandle.Dispose();
		}

		public Task GetCurrentTask()
		{
			return CurrentTask;
		}
	}


	public class HybridTask<InterActionT, ReturnT> : IHybridTask
	{
		public Task<ReturnT> CurrentTask
		{
			get;
			private set;
		}

		private EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
		private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

		private Func<InterActionT> initialAction;
		private Func<InterActionT, ReturnT> collectingResults;

		public HybridTask(Func<InterActionT> initialAction, Func<InterActionT, ReturnT> collectingResults)
		{
			this.initialAction = initialAction;
			this.collectingResults = collectingResults;

			CurrentTask = Init();
		}

		private Task<ReturnT> Init()
		{
			waitHandle.Reset();

			Task<ReturnT> task = new Task<ReturnT>(
				() =>
					{
						//Here we execute work
						InterActionT result = initialAction();

						//Here we wait for work execution finish called;
						waitHandle.WaitOne();

						//Here we call to wrap the results
						return collectingResults(result);
					}, cancelTokenSource.Token
			);
			return task;
		}

		public void ReleaseThread()
		{
			waitHandle.Set();
		}

		public void Cancel()
		{
			cancelTokenSource.Cancel();
		}

		public void Dispose()
		{
			waitHandle.Dispose();
		}

		public Task GetCurrentTask()
		{
			return CurrentTask;
		}
	}
}
