using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Public.Models.Order;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators;

public class EligibilityCheckDtoValidator : AbstractValidator<EligibilityCheckDto>
{
    public EligibilityCheckDtoValidator()
    {
        /*
         * example
         * RuleFor(dto => dto.WaterSpeedId)
         *   .NotEmpty()
         *   .WithMessage("Select how fast the water was moving.")
         *   .WithState(dto => FloodReportCreatePages.Speed);
         */

        // Uprn
        //.Uprn

        // Usrn
        //.Usrn

        // Easting
        //Easting but it is double

        // Northing
        //Northing but it is double

        //.TemporaryUprn ??
        //.Uninhabitable ??
        //.Residentials ??
        //.Commercials ??

        // Is address / Is postal address
        // IsAddress is a boolean and can't be null, nothing to validate
        RuleFor(dto => dto.IsAddress)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.Home);

        // Location description
        RuleFor(dto => dto.LocationDesc)
            .NotEmpty()
            .WithState(dto => FloodReportCreatePages.Location)
            .When(dto => !dto.IsAddress);

        // Property type


        // Flooded areas

        // Is uninhabitable

        // Temporary location description
        //.TemporaryLocationDesc

        // Impact start
        //.ImpactStart

        // Is on going
        //.DurationKnownId
        //.OnGoing
        // OnGoing is a boolean and can't be null, nothing to validate

        // Flooding lasted
        //.ImpactDuration ??

        // Vulnerable people
        //.VulnerablePeopleId its a Guid though
        //.VulnerableCount

        // Sources
        //.Sources

        // Secondary sources
        //.SecondarySources
    }
}
