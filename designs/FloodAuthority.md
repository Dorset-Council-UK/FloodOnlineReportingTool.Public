# Flood authority

## Purpose

The FloodAuthority are authorities and agencies responsible for managing, mitigating, and responding to flood risks and incidents.

## Simple relationships

```mermaid
erDiagram
  FloodAuthority {
    Guid Id PK
  }
  FloodAuthorityFloodProblem {
    Guid FloodAuthorityId PK
    Guid FloodProblemId PK
  }
  FloodAuthority ||--o{ FloodAuthorityFloodProblem : ""
  FloodAuthorityFloodProblem ||--|| FloodProblem : ""
```

## Where it is used

- [Organisation](Organisation.md)