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

        RuleFor(o => o.NumberOfVehiclesDamaged)
            .NotEmpty()
            .WithMessage("Enter the number of vehicles damaged")
            .InclusiveBetween(1, 255)
            .WithMessage("The number of vehicles damaged must be between {From} and {To}")
            .When(o => o.WereVehiclesDamagedId == RecordStatusIds.Yes);
    }
}
