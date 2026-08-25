using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class HelpReceivedValidator : AbstractValidator<HelpReceived>
{
    public HelpReceivedValidator()
    {
        RuleFor(o => o.HelpReceivedOptions)
            .NotEmpty()
            .WithMessage("Select what help you received or select 'No help'");
    }
}
