using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class BlockagesValidator : AbstractValidator<Blockages>
{
    public BlockagesValidator()
    {
        RuleFor(o => o.HasKnownProblemsId)
            .NotEmpty()
            .WithMessage("Select if there are any problems, blockages, or recent repair works");

        RuleFor(o => o.KnownProblemDetails)
            .NotEmpty()
            .WithMessage("Enter the problems, blockages, or recent repair works")
            .MaximumLength(200)
            .WithMessage("Details of the problems, blockages, or recent repair works must be {MaxLength} characters or less")
            .When(o => o.HasKnownProblemsId == RecordStatusIds.Yes);
    }
}
