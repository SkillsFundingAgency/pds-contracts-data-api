using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.Data.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pds.Contracts.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class DateTimeExtensionTests
    {
        [TestMethod]
        public void ToUtcTime_WithDaylightSavingTime()
        {
            // Arrange
            var utcdatetime = new DateTime(2000, 5, 2, 1, 2, 2, DateTimeKind.Utc);
            var localDatetime = new DateTime(2000, 5, 2, 2, 2, 2);

            // Act
            var actual = localDatetime.ToUtcTime();

            // Assert
            actual.Should().Be(utcdatetime);
        }

        [TestMethod]
        public void ToUtcTime_WithoutDaylightSavingTime()
        {
            // Arrange
            var utcdatetime = new DateTime(2000, 2, 2, 2, 2, 2, DateTimeKind.Utc);
            var localDatetime = new DateTime(2000, 2, 2, 2, 2, 2);

            // Act
            var actual = localDatetime.ToUtcTime();

            // Assert
            actual.Should().Be(utcdatetime);
        }

        [TestMethod]
        public void DateExtensions_GMTWhenDisplayFormatCalled_ReturnsDateStringAsExpected()
        {
            // Arrange
            var input = new DateTime(2024, 1, 5, 22, 6, 45);
            var expected = "5 January 2024 at 10:06pm";

            // Act
            var result = input.DisplayFormat();

            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void DateExtensions_BSTWhenDisplayFormatCalled_ReturnsDateStringAsExpected()
        {
            // Arrange
            var input = new DateTime(2024, 10, 5, 22, 6, 45);
            var expected = "5 October 2024 at 11:06pm";

            // Act
            var result = input.DisplayFormat();

            // Assert
            result.Should().Be(expected);
        }
    }
}
