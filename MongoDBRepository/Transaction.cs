using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDBRepository
{
	public sealed class Transaction : IDisposable
	{
		private readonly IClientSessionHandle _session;

		private readonly Action<bool> _onCompleted;

		private bool _isCompleted = false;

		internal Transaction(IClientSessionHandle session, Action<bool> onCompleted)
		{
			_session = session;

			_onCompleted = onCompleted;
		}

		public async Task CommitAsync(CancellationToken cancellation = default)
		{
			await RetryHelper.DoWithRetryAsync(async () => await _session.CommitTransactionAsync(cancellation));

			Complete(true);
		}

		public async Task AbortAsync(CancellationToken cancellation = default)
		{
			await _session.AbortTransactionAsync(cancellation);

			Complete(false);
		}

		public void Dispose()
		{
			_session.Dispose();

			Complete(false);
		}

		private void Complete(bool committed)
        {
			if (!_isCompleted)
			{
				_onCompleted(committed);
			}

			_isCompleted = true;
		}
	}

	internal static class RetryHelper
	{
		public static async Task DoWithRetryAsync(Func<Task> action, int maxRetries = 5)
		{
			var tries = 0;

			while (true)
			{
				try
				{
					await action();
				}
				catch (MongoException ex) //when (ex.HasErrorLabel("TransientTransactionError"))
				{
					if (maxRetries != default && tries >= maxRetries)
					{
						throw ex;
					}

					tries++;

					await Task.Delay(10 * tries);
				}
				catch (Exception ex)
                {
					throw ex;
                }
			}
		}

		public static void DoWithRetry(Action action, int maxRetries = 5)
		{
			var tries = 0;

			while (true)
			{
				try
				{
					action();
				}
				catch (MongoException ex) when (ex.HasErrorLabel("TransientTransactionError"))
				{
					if (maxRetries != default && tries >= maxRetries)
					{
						throw;
					}

					tries++;

					Thread.Sleep(10 * tries);
				}
			}
		}
	}
}
