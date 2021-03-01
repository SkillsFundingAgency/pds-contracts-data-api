using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.Data.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Tests.Unit
{
    [TestClass]
    public class SempaphoreOnEntityIdTests
    {
        private const int LongSleepInterval = 1000;
        private const int ShortSleepInterval = 100;

        private readonly ISemaphoreOnEntity<string> _semaphoreOnEntity
            = new SemaphoreOnEntity<string>();

        [TestMethod]
        public void Wait_SecondEntryWithTheSameID_Is_BlockedUntilTheFirstResolvesTest()
        {
            // Arrange
            List<Task> tasks = new List<Task>();
            string id = "test";
            bool firstRunning = false;

            DateTime completionTimeOfFirstTask = DateTime.MinValue;
            DateTime completionTimeOfSecondTask = DateTime.MinValue;

            void FirstLock()
            {
                firstRunning = true;
                _semaphoreOnEntity.Wait(id);
                Thread.Sleep(LongSleepInterval);
                _semaphoreOnEntity.Release(id);
                completionTimeOfFirstTask = DateTime.UtcNow;
            }

            void SecondLock()
            {
                do
                {
                    // Wait for first to start
                    Thread.Sleep(ShortSleepInterval);
                }
                while (!firstRunning);

                _semaphoreOnEntity.Wait(id);
                _semaphoreOnEntity.Release(id);
                completionTimeOfSecondTask = DateTime.UtcNow;
            }

            // Act
            Task firstTask = Task.Run(FirstLock);
            Task secondTask = Task.Run(SecondLock);

            Task.WaitAll(new Task[] { firstTask, secondTask });

            // Assert
            completionTimeOfFirstTask.Should().BeBefore(completionTimeOfSecondTask);
        }

        [TestMethod]
        public void Wait_SecondEntryWithDifferentID_IsNot_BlockedTest()
        {
            // Arrange
            List<Task> tasks = new List<Task>();
            string id1 = "first-test";
            string id2 = "other-test";
            bool firstRunning = false;

            DateTime completionTimeOfFirstTask = DateTime.MinValue;
            DateTime completionTimeOfSecondTask = DateTime.MinValue;

            void FirstLock()
            {
                firstRunning = true;

                _semaphoreOnEntity.Wait(id1);
                Thread.Sleep(LongSleepInterval);
                _semaphoreOnEntity.Release(id1);
                completionTimeOfFirstTask = DateTime.UtcNow;
            }

            void SecondLock()
            {
                do
                {
                    // Wait for first to start
                    Thread.Sleep(ShortSleepInterval);
                }
                while (!firstRunning);

                _semaphoreOnEntity.Wait(id2);
                _semaphoreOnEntity.Release(id2);
                completionTimeOfSecondTask = DateTime.UtcNow;
            }

            // Act
            Task firstTask = Task.Run(FirstLock);
            Task secondTask = Task.Run(SecondLock);

            Task.WaitAll(new Task[] { firstTask, secondTask });

            // Assert
            completionTimeOfFirstTask.Should().BeAfter(completionTimeOfSecondTask);
        }

        [TestMethod]
        public async Task WaitAsync_SecondEntryWithTheSameID_Is_BlockedUntilTheFirstResolvesTest()
        {
            // Arrange
            List<Task> tasks = new List<Task>();
            string id = "test";
            bool firstRunning = false;

            DateTime completionTimeOfFirstTask = DateTime.MinValue;
            DateTime completionTimeOfSecondTask = DateTime.MinValue;

            async Task FirstLock()
            {
                firstRunning = true;
                await _semaphoreOnEntity.WaitAsync(id);

                Thread.Sleep(LongSleepInterval);
                _semaphoreOnEntity.Release(id);
                completionTimeOfFirstTask = DateTime.UtcNow;
            }

            async Task SecondLock()
            {
                do
                {
                    // Wait for first to start
                    Thread.Sleep(ShortSleepInterval);
                }
                while (!firstRunning);

                await _semaphoreOnEntity.WaitAsync(id);

                _semaphoreOnEntity.Release(id);
                completionTimeOfSecondTask = DateTime.UtcNow;
            }


            // Act
            Task firstTask = Task.Run(FirstLock);
            Task secondTask = Task.Run(SecondLock);

            await Task.WhenAll(new Task[] { firstTask, secondTask });

            // Assert
            completionTimeOfFirstTask.Should().BeBefore(completionTimeOfSecondTask);
        }

        [TestMethod]
        public async Task WaitAsync_SecondEntryWithDifferentID_IsNot_BlockedTest()
        {
            // Arrange
            List<Task> tasks = new List<Task>();
            string id1 = "test";
            string id2 = "otherTest";
            bool firstRunning = false;

            //bool secondRunning = false;
            DateTime completionTimeOfFirstTask = DateTime.MinValue;
            DateTime completionTimeOfSecondTask = DateTime.MinValue;

            async Task FirstLock()
            {
                firstRunning = true;
                await _semaphoreOnEntity.WaitAsync(id1);

                Thread.Sleep(LongSleepInterval);
                _semaphoreOnEntity.Release(id1);
                completionTimeOfFirstTask = DateTime.UtcNow;
            }

            async Task SecondLock()
            {
                do
                {
                    // Wait for first to start
                    Thread.Sleep(ShortSleepInterval);
                }
                while (!firstRunning);

                await _semaphoreOnEntity.WaitAsync(id2);

                _semaphoreOnEntity.Release(id2);
                completionTimeOfSecondTask = DateTime.UtcNow;
            }

            // Act
            Task firstTask = Task.Run(FirstLock);
            Task secondTask = Task.Run(SecondLock);

            await Task.WhenAll(new Task[] { firstTask, secondTask });

            // Assert
            completionTimeOfFirstTask.Should().BeAfter(completionTimeOfSecondTask);
        }

        [TestMethod]
        public void Release_AttemptingToReleaseNonExistantLock_DoesNotCauseExceptionTest()
        {
            // Arrange
            string id = "test";

            // Act
            Action act = () => _semaphoreOnEntity.Release(id);

            // Assert
            act.Should().NotThrow();
        }
    }
}
