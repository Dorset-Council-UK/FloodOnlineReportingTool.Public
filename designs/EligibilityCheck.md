# Eligibility check

## Purpose

The EligibilityCheck represents an assessment to determine if a person qualifies for assistance, related to flood damage.
This is the information saved when someone completes the online flood report form.

## Vulnerable people

The VulnerablePeopleId field indicates whether there are vulnerable individuals involved in the eligibility check.
It uses the RecordStatus entity to capture responses of:
- Yes
- No
- Not Sure

## Simple relationships

```mermaid
---
config:
  layout: elk
---
erDiagram
  EligibilityCheck {
    Guid Id PK
    Guid VulnerablePeopleId "RecordStatus"
  }
  FloodReport ||--o| EligibilityCheck : EligibilityCheckId
  EligibilityCheck ||--o{ EligibilityCheckCommercial : Commercials
  EligibilityCheck ||--o{ EligibilityCheckResidential : Residentials
  EligibilityCheck ||--o{ EligibilityCheckRunoffSource : SecondarySources
  EligibilityCheck ||--o{ EligibilityCheckSource : Sources

  EligibilityCheckCommercial ||--|| FloodImpact : FloodImpactId
  EligibilityCheckResidential ||--|| FloodImpact : FloodImpactId
  EligibilityCheckRunoffSource ||--|| FloodProblem : FloodProblemId
  EligibilityCheckSource ||--|| FloodProblem : FloodProblemId
```

## Where it is used

- [FloodReport](FloodReport.md)