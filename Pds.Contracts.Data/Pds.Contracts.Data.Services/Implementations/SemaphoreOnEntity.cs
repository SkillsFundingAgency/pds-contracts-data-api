using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Implementations
{
    /// <summary>
    /// A Semaphore based on the SemaphoreSlim to all locking based on a given identifier.
    /// </summary>
    /// <typeparam name="T">The Id type to lock on.</typeparam>
    public class SemaphoreOnEntity<T> : ISemaphoreOnEntity<T>
    {
        private static readonly ConcurrentDictionary<T, SemaphoreSlim> _lockDictionary
            = new ConcurrentDictionary<T, SemaphoreSlim>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SemaphoreOnEntity{T}"/> class.
        /// </summary>
        public SemaphoreOnEntity()
        {
        }

        /// <inheritdoc/>
        public void Wait(T idToLock)
        {
            _lockDictionary.GetOrAdd(idToLock, new SemaphoreSlim(1, 1)).Wait();
        }

        /// <inheritdoc/>
        public async Task WaitAsync(T idToLock)
        {
            System.Diagnostics.Debug.WriteLine($"Locking on {idToLock}");
            await _lockDictionary.GetOrAdd(idToLock, new SemaphoreSlim(1, 1)).WaitAsync();
        }

        /// <inheritdoc/>
        public void Release(T idToUnlock)
        {
            if (_lockDictionary.TryRemove(idToUnlock, out SemaphoreSlim semaphore))
            {
                semaphore.Release();
            }
        }
    }
}
