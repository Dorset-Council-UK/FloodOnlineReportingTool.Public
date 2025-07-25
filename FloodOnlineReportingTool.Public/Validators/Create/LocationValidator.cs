﻿using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class LocationValidator : AbstractValidator<Location>
{
    public LocationValidator()
    {
        RuleFor(o => o.Easting)
            .NotEmpty()
            .WithMessage("Please select a location on the map using the 'Choose a location' button");

        RuleFor(o => o.Northing)
            .NotEmpty()
            .WithMessage("Please select a location on the map using the 'Choose a location' button")
            .When(o => o.Easting.HasValue);
    }
}
