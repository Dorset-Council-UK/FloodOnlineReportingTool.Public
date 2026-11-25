namespace FluentValidation;

internal static class FluentValidationExtensions
{
    extension(IValidationContext context)
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current context is marked as internal.
        /// Set this from the eligibility check, which looks for internal flood impacts.
        /// <code>eligibilityCheck?.IsInternal == true</code>
        /// </summary>
        public bool IsInternal
        {
            get => context.RootContextData.TryGetValue("IsInternal", out var isInternal) && isInternal is bool b && b;
            set => context.RootContextData["IsInternal"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current context is marked as residential.
        /// </summary>
        public bool IsResidential
        {
            get => context.RootContextData.TryGetValue("IsResidential", out var isResidential) && isResidential is bool b && b;
            set => context.RootContextData["IsResidential"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current context is marked as commercial.
        /// </summary>
        public bool IsCommercial
        {
            get => context.RootContextData.TryGetValue("IsCommercial", out var isCommercial) && isCommercial is bool b && b;
            set => context.RootContextData["IsCommercial"] = value;
        }
    }
}
