using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class SummaryValidator : AbstractValidator<Summary>
{
    public SummaryValidator()
    {
        RuleFor(o => o.AddressPreview)
            .NotEmpty()
            .WithMessage("Address or location is empty.");

        RuleFor(o => o.PropertyTypeName)
            .NotEmpty()
            .WithMessage("Property type is empty.");

        RuleFor(o => o.IsUninhabitable)
            .NotEmpty()
            .WithMessage("Evacuation is empty.");

        RuleFor(o => o.FloodedAreas)
            .NotEmpty()
            .WithMessage("No flood areas were selected.");

        RuleFor(o => o.FloodSources)
            .NotEmpty()
            .WithMessage("No flood sources were selected.");

        RuleFor(o => o.StartDate)
            .NotEmpty()
            .WithMessage("Flood start date is empty.");

        RuleFor(o => o.IsOnGoing)
            .NotEmpty()
            .WithMessage("Flooding on-going is empty.");

        When(o => o.IsOnGoing == false, () =>
        {
            RuleFor(o => o.FloodDurationKnownId)
                .NotEmpty()
                .WithMessage("Flood duration is empty.");

            RuleFor(o => o.FloodingLasted)
                .NotEmpty()
                .WithMessage("Flood lasted is empty.")
                .When(o => o.FloodDurationKnownId != null);
        });

        RuleFor(o => o.VulnerablePeopleId)
            .NotEmpty()
            .WithMessage("Vulnerable people is empty.");

        RuleFor(o => o.NumberOfVulnerablePeople)
            .NotEmpty()
            .WithMessage("Number of vulnerable people is empty.")
            .When(o => o.VulnerablePeopleId == RecordStatusIds.Yes);
    }
}
