using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class HelpReceivedValidator : AbstractValidator<HelpReceived>
{
    public HelpReceivedValidator()
    {
        RuleFor(o => o.HelpReceivedOptions)
            .Must(o => o.Any(x => x.Selected))
            .WithMessage("Select what help you received or select 'No help'");
    }
}
