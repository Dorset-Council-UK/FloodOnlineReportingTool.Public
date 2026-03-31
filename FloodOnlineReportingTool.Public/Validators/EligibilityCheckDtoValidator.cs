using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Public.Models.Order;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators;

#pragma warning disable MA0051 // Method is too long
public class EligibilityCheckDtoValidator : AbstractValidator<EligibilityCheckDto>
{
    public EligibilityCheckDtoValidator()
    {
        // Uprn
        RuleFor(dto => dto.Uprn)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.Address)
            .When(dto => dto.IsAddress);

        // Usrn
        // Not validated at the moment, USRN to be added later

        // Easting
        // Easting is a non nullable double, but we can validate it's not zero
        RuleFor(dto => dto.Easting)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.Location);

        // Northing
        // Northing is a non nullable double, but we can validate it's not zero
        RuleFor(dto => dto.Northing)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.Location);

        // Is address / Is postal address
        // IsAddress is a non nullable bool, so we can't validate it

        // Location description
        RuleFor(dto => dto.LocationDesc)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.Location)
            .When(dto => !dto.IsAddress);

        // Temporary Uprn
        RuleFor(dto => dto.TemporaryUprn)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.TemporaryAddress);

        // Temporary location description
        RuleFor(dto => dto.TemporaryLocationDesc)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.TemporaryAddress);

        // Impact start / Flooding started
        RuleFor(dto => dto.ImpactStart)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.FloodStarted);

        // Duration known
        RuleFor(dto => dto.DurationKnownId)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.FloodDuration);

        // Impact duration / Flooding lasted
        RuleFor(dto => dto.ImpactDuration)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.FloodDuration)
            .When(dto => dto.OnGoing && dto.DurationKnownId == FloodDurationIds.DurationKnown);

        // On going / Is the flodding still happening
        // OnGoing is a non nullable bool, so we can't validate it

        // Is uninhabitable
        RuleFor(dto => dto.Uninhabitable)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.FloodAreas);

        // Vulnerable people
        // VulnerablePeopleId is a non nullable Guid, can we validate it?
        RuleFor(dto => dto.VulnerablePeopleId)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.Vulnerability);

        // Number of vulnerable people
        RuleFor(dto => dto.VulnerableCount)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.Vulnerability)
            .When(dto => dto.VulnerablePeopleId == RecordStatusIds.Yes);

        // Residentials / FloodImpact's
        RuleFor(dto => dto.Residentials)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.FloodAreas)
            .When(IsResidentialOrOther);

        // Commercials / FloodImpact's
        RuleFor(dto => dto.Commercials)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.FloodAreas)
            .When(IsCommercialOrOther);

        // Sources / FloodProblem's
        RuleFor(dto => dto.Sources)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.FloodSource);

        // Secondary sources / FloodProblem's
        RuleFor(dto => dto.SecondarySources)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.FloodSecondarySource);
    }

    private bool IsCommercialOrOther(EligibilityCheckDto dto)
    {
        var isOnlyCommercial = dto.Residentials.Count == 0 && dto.Commercials.Count > 0;
        var isOtherOrNotSpecified = dto.Residentials.Count > 0 && dto.Commercials.Count > 0;

        return isOnlyCommercial || isOtherOrNotSpecified;
    }

    private bool IsResidentialOrOther(EligibilityCheckDto dto)
    {
        var isOnlyResidential = dto.Residentials.Count > 0 && dto.Commercials.Count == 0;
        var isOtherOrNotSpecified = dto.Residentials.Count > 0 && dto.Commercials.Count > 0;

        return isOnlyResidential || isOtherOrNotSpecified;
    }
}
