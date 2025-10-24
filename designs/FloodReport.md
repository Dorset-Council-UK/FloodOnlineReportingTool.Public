# Flood report

## Purpose

The FloodReport represents an overview of a flood event reported by a user.
It captures essential information about the flood within the EligibilityCheck, as well as its status, any investigation, and contact records.

## Status
The StatusId field indicates the current status of the flood report.
It uses the RecordStatus entity to capture responses of:
- Marked for deletion
- New
- Viewed
- Action needed
- Action completed
- Error

## Simple relationships

```mermaid
---
config:
  layout: elk
---
erDiagram
  FloodReport {
    Guid Id PK
    Guid StatusId "RecordStatus"
  }
  FloodReport ||--o| EligibilityCheck : EligibilityCheckId
  FloodReport ||--o| Investigation : InvestigationId
  FloodReport ||--o| ContactRecord : ReportedByUserId
  FloodReport ||--o{ ContactRecord : ContactRecords
```
