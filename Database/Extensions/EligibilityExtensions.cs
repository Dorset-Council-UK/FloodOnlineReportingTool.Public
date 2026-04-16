using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
using FloodOnlineReportingTool.Database.Models.Responsibilities;
using System.Globalization;

namespace FloodOnlineReportingTool.Database.Models.Eligibility;

public static class EligibilityExtensions
{
    extension(EligibilityCheck eligibilityCheck)
    {
        internal EligibilityCheckRecord ToMessageCreated(string reference, IList<Organisation> organisations, IList<FloodProblem> floodProblems)
        {
            return new(
                eligibilityCheck.Id,
                eligibilityCheck.Uprn,
                eligibilityCheck.Usrn,
                eligibilityCheck.Easting,
                eligibilityCheck.Northing,
                eligibilityCheck.ImpactStart,
                eligibilityCheck.ImpactDuration,
                eligibilityCheck.OnGoing,
                eligibilityCheck.Uninhabitable,
                eligibilityCheck.VulnerableCount,
                eligibilityCheck.LocationDesc,
                [..
                    organisations.Select(o => new EligibilityCheckOrganisation(
                        o.Id,
                        o.Name,
                        o.FloodAuthorityId,
                        o.FloodAuthority.AuthorityName
                    )),
                ], [..
                    floodProblems.Select(p => new EligibilityCheckFloodSource(
                        p.Id,
                        p.TypeName
                    )),
                ]
            );
        }

        internal EligibilityCheckUpdated ToMessageUpdated(IList<Organisation> organisations, IList<FloodProblem> floodProblems)
        {
            return new(
                eligibilityCheck.Id,
                eligibilityCheck.UpdatedUtc ?? DateTimeOffset.UtcNow,
                eligibilityCheck.Uprn,
                eligibilityCheck.Usrn,
                eligibilityCheck.Easting,
                eligibilityCheck.Northing,
                eligibilityCheck.ImpactStart,
                eligibilityCheck.ImpactDuration,
                eligibilityCheck.OnGoing,
                eligibilityCheck.Uninhabitable,
                eligibilityCheck.VulnerableCount,
                eligibilityCheck.LocationDesc,
                [..
                    organisations.Select(o => new EligibilityCheckOrganisation(
                        o.Id,
                        o.Name,
                        o.FloodAuthorityId,
                        o.FloodAuthority.AuthorityName
                    )),
                ], [..
                    floodProblems.Select(p => new EligibilityCheckFloodSource(
                        p.Id,
                        p.TypeName
                    )),
                ]
            );
        }

        public EligibilityCheckDto ToDto()
        {
            return new()
            {
                IsAddress = eligibilityCheck.IsAddress,
                Uprn = eligibilityCheck.Uprn,
                Usrn = eligibilityCheck.Usrn,
                Easting = eligibilityCheck.Easting,
                Northing = eligibilityCheck.Northing,
                LocationDesc = eligibilityCheck.LocationDesc,
                ImpactStart = eligibilityCheck.ImpactStart,
                ImpactDuration = eligibilityCheck.ImpactDuration,
                OnGoing = eligibilityCheck.OnGoing,
                Uninhabitable = eligibilityCheck.Uninhabitable,
                VulnerableCount = eligibilityCheck.VulnerableCount,
                Sources = [.. eligibilityCheck.Sources.Select(o => o.FloodProblemId)],
                Residentials = [.. eligibilityCheck.Residentials.Select(o => o.FloodImpactId)],
                Commercials = [.. eligibilityCheck.Commercials.Select(o => o.FloodImpactId)],
            };
        }

        /// <summary>
        /// Determines whether the eligibility check includes any internal flood impacts.
        /// </summary>
        /// <returns>
        /// True if any residential or commercial flood impacts have a category priority of internal; otherwise, false.
        /// </returns>
        public bool IsInternal
        {
            get
            {
                return
                    eligibilityCheck.Residentials.Any(o => o.FloodImpact.IsInternal) ||
                    eligibilityCheck.Commercials.Any(o => o.FloodImpact.IsInternal);
            }
        }

        /// <summary>
        /// Determines whether the eligibility check includes any external flood impacts.
        /// </summary>
        /// <returns>
        /// True if any residential or commercial flood impacts have a category priority of external; otherwise, false.
        /// </returns>
        public bool IsExternal
        {
            get
            {
                return
                    eligibilityCheck.Residentials.Any(o => o.FloodImpact.IsExternal) ||
                    eligibilityCheck.Commercials.Any(o => o.FloodImpact.IsExternal);
            }
        }

        // TODO: make a method for get score
        internal int GetScore()
        {
            throw new NotImplementedException();
        }
    }

    extension(EligibilityCheckDto eligibilityCheckDto)
    {
        /// <summary>
        ///     <para>Calculates the impact duration hours.</para>
        ///     <para>If the flood is still happening this will be zero.</para>
        ///     <para>If the flood duration is known, it will return the hours provided by the user.</para>
        ///     <para>Otherwise, it will try to get the duration hours from the flood problem in the database.</para>
        /// </summary>
        public async Task<int> CalculateImpactDurationHours(PublicDbContext context, CancellationToken ct = default)
        {
            // Flood is ongoing, so impact duration is not known yet
            if (eligibilityCheckDto.OnGoing)
            {
                return 0;
            }

            // Impact duration is not known, and no duration known Id was provided
            if (eligibilityCheckDto.DurationKnownId is null)
            {
                return 0;
            }

            // Impact duration is known, using provided impact duration hours
            if (eligibilityCheckDto.DurationKnownId == FloodDurationIds.DurationKnown)
            {
                return eligibilityCheckDto.ImpactDuration ?? 0;
            }

            // Get the duration from the flood problem
            var floodProblem = await context.FloodProblems.FindAsync([eligibilityCheckDto.DurationKnownId], ct);
            if (floodProblem is null)
            {
                return 0;
            }

            if (!string.IsNullOrWhiteSpace(floodProblem.TypeName) && int.TryParse(floodProblem.TypeName, CultureInfo.InvariantCulture, out var durationHours))
            {
                return durationHours;
            }

            // Could not parse impact duration from flood problem type name
            return 0;
        }

        /// <summary>
        /// Creates a new EligibilityCheck entity with the values from the DTO.
        /// Only the fields that are allowed to be set on creation will be populated.
        /// </summary>
        /// <remarks>UpdatedUtc property is not set.</remarks>
        /// <returns>A new EligibilityCheck populated with the values from the DTO.</returns>
        internal EligibilityCheck ToCreatedEntity(Guid id, DateTimeOffset createdUtc, DateTimeOffset termsAgreed, int impactDuration)
        {
            return new()
            {
                Id = id,
                CreatedUtc = createdUtc,
                //UpdatedUtc
                TermsAgreed = termsAgreed,

                Uprn = eligibilityCheckDto.Uprn,
                Usrn = eligibilityCheckDto.Usrn,
                Easting = eligibilityCheckDto.Easting,
                Northing = eligibilityCheckDto.Northing,
                IsAddress = eligibilityCheckDto.IsAddress,
                LocationDesc = eligibilityCheckDto.LocationDesc,
                TemporaryUprn = eligibilityCheckDto.TemporaryUprn,
                TemporaryLocationDesc = eligibilityCheckDto.TemporaryLocationDesc,
                ImpactStart = eligibilityCheckDto.ImpactStart,
                ImpactDuration = impactDuration,
                OnGoing = eligibilityCheckDto.OnGoing,
                Uninhabitable = eligibilityCheckDto.Uninhabitable == true,
                VulnerablePeopleId = eligibilityCheckDto.VulnerablePeopleId,
                VulnerableCount = eligibilityCheckDto.VulnerableCount,
                Residentials = [.. eligibilityCheckDto.Residentials.Select(floodImpactId => new EligibilityCheckResidential(id, floodImpactId))],
                Commercials = [.. eligibilityCheckDto.Commercials.Select(floodImpactId => new EligibilityCheckCommercial(id, floodImpactId))],
                Sources = [.. eligibilityCheckDto.Sources.Select(floodProblemId => new EligibilityCheckSource(id, floodProblemId))],
                SecondarySources = [.. eligibilityCheckDto.SecondarySources.Select(floodProblemId => new EligibilityCheckRunoffSource(id, floodProblemId))],
            };
        }

        /// <summary>
        /// Update the eligibility check entity with the values from the DTO.
        /// Only the fields that are allowed to be updated will be changed.
        /// </summary>
        /// <remarks>Id, CreatedUtc and TermsAgreed properties will not be updated.</remarks>
        /// <returns>An updated eligibility check entity with the new values from the DTO.</returns>
        public EligibilityCheck ToUpdatedEntity(EligibilityCheck eligibilityCheck, DateTimeOffset updatedUtc, int impactDuration)
        {
            var id = eligibilityCheck.Id;

            return eligibilityCheck with
            {
                //Id
                //CreatedUtc
                UpdatedUtc = updatedUtc,
                //TermsAgreed = termsAgreed,

                Uprn = eligibilityCheckDto.Uprn,
                Usrn = eligibilityCheckDto.Usrn,
                Easting = eligibilityCheckDto.Easting,
                Northing = eligibilityCheckDto.Northing,
                IsAddress = eligibilityCheckDto.IsAddress,
                LocationDesc = eligibilityCheckDto.LocationDesc,
                TemporaryUprn = eligibilityCheckDto.TemporaryUprn,
                TemporaryLocationDesc = eligibilityCheckDto.TemporaryLocationDesc,
                ImpactStart = eligibilityCheckDto.ImpactStart,
                ImpactDuration = impactDuration,
                OnGoing = eligibilityCheckDto.OnGoing,
                Uninhabitable = eligibilityCheckDto.Uninhabitable == true,
                VulnerablePeopleId = eligibilityCheckDto.VulnerablePeopleId,
                VulnerableCount = eligibilityCheckDto.VulnerableCount,
                Residentials = [.. eligibilityCheckDto.Residentials.Select(floodImpactId => new EligibilityCheckResidential(id, floodImpactId))],
                Commercials = [.. eligibilityCheckDto.Commercials.Select(floodImpactId => new EligibilityCheckCommercial(id, floodImpactId))],
                Sources = [.. eligibilityCheckDto.Sources.Select(floodProblemId => new EligibilityCheckSource(id, floodProblemId))],
                SecondarySources = [.. eligibilityCheckDto.SecondarySources.Select(floodProblemId => new EligibilityCheckRunoffSource(id, floodProblemId))],
            };
        }
    }
}
