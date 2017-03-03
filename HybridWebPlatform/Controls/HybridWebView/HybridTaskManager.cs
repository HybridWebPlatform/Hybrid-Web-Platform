using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HybridWebPlatform.HybridWeb.Utilites
{
	public class HybridTaskManager : IDisposable
	{
		private BlockingCollection<IHybridTask> tasks;
		private CancellationTokenSource cancellationTokenSource;

		public Task MainTask
		{
			get;
			private set;
		}

		public IHybridTask CurrentHybridTask
		{
			get;
			private set;
		}

		public HybridTaskManager()
		{
			tasks = new BlockingCollection<IHybridTask>();
			cancellationTokenSource = new CancellationTokenSource();
			MainTask = CreateMainTask();
		}

		public async Task<T> ExecuteTask<T>(HybridTask<T> thread)
		{
			tasks.Add(thread);

			return await thread.CurrentTask;
		}

		public async Task<ReturnT> ExecuteTask<InterActionT, ReturnT>(HybridTask<InterActionT, ReturnT> thread)
		{
			tasks.Add(thread);

			return await thread.CurrentTask;
		}

		public void Cancell()
		{
			cancellationTokenSource.Cancel();
		}

		public void ReleaseCurrentTask()
		{
			if (CurrentHybridTask != null)
			{
				CurrentHybridTask.ReleaseThread();
			}
		}

		public void FinishCollectionPopulation()
		{
			tasks.CompleteAdding();
		}

		private Task CreateMainTask()
		{
			return Task.Factory.StartNew(RepeatedTask);
		}

		private void RepeatedTask()
		{
			try
			{
				while (!tasks.IsCompleted)
				{
					IHybridTask item;
					if (tasks.TryTake(out item, Timeout.Infinite, cancellationTokenSource.Token))
					{
						CurrentHybridTask = item;
						Task currentTask = item.GetCurrentTask();
						currentTask.Start();
						currentTask.Wait(cancellationTokenSource.Token);
					}
				}
			}
			catch (TaskCanceledException)
			{
			}
		}

		public void Dispose()
		{
			foreach (var item in tasks)
			{
				item.Cancel();
				item.Dispose();
			}
		}
	}
}
