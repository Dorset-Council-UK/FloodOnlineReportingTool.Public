using FloodOnlineReportingTool.Contracts.Topics;
using Messaging.Consumers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null, StringExtensions.MeasurementDisplayType.Centimetres, "")]
    [InlineData(-1, StringExtensions.MeasurementDisplayType.Centimetres, "cannot convert negative number")]
    [InlineData(0, StringExtensions.MeasurementDisplayType.Centimetres, "0 centimetres")]
    [InlineData(1, StringExtensions.MeasurementDisplayType.Centimetres, "1 centimetre")]
    [InlineData(2, StringExtensions.MeasurementDisplayType.Centimetres, "2 centimetres")]
    [InlineData(999, StringExtensions.MeasurementDisplayType.Centimetres, "999 centimetres")]
    [InlineData(null, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "")]
    [InlineData(-1, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "cannot convert negative number")]
    [InlineData(0, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "0 centimetres")]
    [InlineData(1, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "1 centimetre")]
    [InlineData(2, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "2 centimetres")]
    [InlineData(99, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "99 centimetres")]
    [InlineData(100, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "1 metre")]
    [InlineData(101, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "1 metre 1 centimetre")]
    [InlineData(199, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "1 metre 99 centimetres")]
    [InlineData(200, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "2 metres")]
    [InlineData(201, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "2 metres 1 centimetre")]
    [InlineData(299, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "2 metres 99 centimetres")]
    [InlineData(999, StringExtensions.MeasurementDisplayType.MetresAndCentimetres, "9 metres 99 centimetres")]
    [InlineData(null, StringExtensions.MeasurementDisplayType.Inches, "")]
    [InlineData(-1, StringExtensions.MeasurementDisplayType.Inches, "cannot convert negative number")]
    [InlineData(0, StringExtensions.MeasurementDisplayType.Inches, "0 inches")]
    [InlineData(1, StringExtensions.MeasurementDisplayType.Inches, "0 inches")]
    [InlineData(2, StringExtensions.MeasurementDisplayType.Inches, "1 inch")]
    [InlineData(3, StringExtensions.MeasurementDisplayType.Inches, "1 inch")]
    [InlineData(5, StringExtensions.MeasurementDisplayType.Inches, "2 inches")]
    [InlineData(999, StringExtensions.MeasurementDisplayType.Inches, "393 inches")]
    [InlineData(null, StringExtensions.MeasurementDisplayType.FeetAndInches, "")]
    [InlineData(-1, StringExtensions.MeasurementDisplayType.FeetAndInches, "cannot convert negative number")]
    [InlineData(0, StringExtensions.MeasurementDisplayType.FeetAndInches, "0 inches")]
    [InlineData(1, StringExtensions.MeasurementDisplayType.FeetAndInches, "0 inches")]
    [InlineData(2, StringExtensions.MeasurementDisplayType.FeetAndInches, "1 inch")]
    [InlineData(3, StringExtensions.MeasurementDisplayType.FeetAndInches, "1 inch")]
    [InlineData(5, StringExtensions.MeasurementDisplayType.FeetAndInches, "2 inches")]
    [InlineData(31, StringExtensions.MeasurementDisplayType.FeetAndInches, "1 foot")]
    [InlineData(33, StringExtensions.MeasurementDisplayType.FeetAndInches, "1 foot 1 inch")]
    [InlineData(58, StringExtensions.MeasurementDisplayType.FeetAndInches, "1 foot 11 inches")]
    [InlineData(61, StringExtensions.MeasurementDisplayType.FeetAndInches, "2 feet")]
    [InlineData(63, StringExtensions.MeasurementDisplayType.FeetAndInches, "2 feet 1 inch")]
    [InlineData(89, StringExtensions.MeasurementDisplayType.FeetAndInches, "2 feet 11 inches")]
    [InlineData(999, StringExtensions.MeasurementDisplayType.FeetAndInches, "32 feet 9 inches")]
    internal Task TopicName_Returns_ExpectedTopicName_ForConfiguredSuffix(int? cm, StringExtensions.MeasurementDisplayType mdt, string expectedResult)
    {
        var actualResult = cm.ConvertMeasurementToDisplayString(mdt);

        // Assert
        Assert.Equal(expectedResult, actualResult);
        return Task.CompletedTask;
    }
}
