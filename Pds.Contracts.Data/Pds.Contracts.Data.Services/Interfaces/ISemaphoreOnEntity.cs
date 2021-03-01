using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Implementations
{
    /// <summary>
    /// A Semaphore based on the SemaphoreSlim to all locking based on a given identifier.
    /// </summary>
    /// <typeparam name="T">The Id type to lock on.</typeparam>
    public interface ISemaphoreOnEntity<T>
    {
        /// <summary>
        /// Releases the lock (according to the given ID).
        /// </summary>
        /// <param name="idToUnlock">the unique ID to unlock.</param>
        void Release(T idToUnlock);

        /// <summary>
        /// Blocks the current thread (according to the given ID) until it can enter the LockProvider.
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock.</param>
        void Wait(T idToLock);

        /// <summary>
        /// Asynchronously puts thread to wait (according to the given ID) until it can enter the LockProvider.
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock.</param>
        /// <returns>Task representing the async operation.</returns>
        Task WaitAsync(T idToLock);
    }
}