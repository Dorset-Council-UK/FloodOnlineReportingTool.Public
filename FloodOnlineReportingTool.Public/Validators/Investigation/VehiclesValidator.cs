using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class VehiclesValidator : AbstractValidator<Vehicles>
{
    public VehiclesValidator()
    {
        RuleFor(o => o.WereVehiclesDamagedId)
            .NotEmpty()
            .WithMessage("Select if vehicles were damaged or select 'Not sure'");

        When(o => o.WereVehiclesDamagedId == RecordStatusIds.Yes, () =>
        {
            RuleFor(o => o.NumberOfVehiclesDamagedText)
                .NotEmpty()
                .WithMessage("Enter the number of vehicles damaged");

            RuleFor(o => o.NumberOfVehiclesDamagedNumber)
                .NotEmpty()
                .WithMessage("The number of vehicles damaged must be a whole number like 3")
                .InclusiveBetween(1, 25)
                .WithMessage("The number of vehicles damaged must be between {From} and {To}")
                .OverridePropertyName(o => o.NumberOfVehiclesDamagedText)
                .When(o => !string.IsNullOrWhiteSpace(o.NumberOfVehiclesDamagedText));
        });
    }
}
