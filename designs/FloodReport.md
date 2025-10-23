# Flood report

## Purpose

The FloodReport represents an overview of a flood event reported by a user.
It captures essential information about the flood within the EligibilityCheck, as well as its status, any investigation, and contact records.

## Simple relationships

```mermaid
erDiagram
  FloodReport {
    Guid Id PK
  }
  FloodReport ||--|| RecordStatus : StatusId
  FloodReport ||--o| EligibilityCheck : EligibilityCheckId
  FloodReport ||--o| Investigation : InvestigationId
  FloodReport ||--o| ContactRecord : ReportedByUserId
  FloodReport ||--o{ ContactRecord : ContactRecords
```
