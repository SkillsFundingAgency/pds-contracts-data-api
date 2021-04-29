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
        public void ToDateDisplay_WithDaylightSavingTime()
        {
            // Arrange
            var expectedDateTimeText = "24 February 2021 at 06:55PM";
            var localDatetime = new DateTime(2021, 2, 24, 18, 55, 0, DateTimeKind.Utc);

            // Act
            var actual = localDatetime.ToDateDisplay();

            // Assert
            actual.Should().Be(expectedDateTimeText);
        }
    }
}
