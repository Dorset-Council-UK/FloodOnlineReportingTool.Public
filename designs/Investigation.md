# Investigation

## Purpose

The Investigation represents investigations carried out by the relevant authority, for example grants.

## Simple relationships

```mermaid
---
config:
  layout: elk
---
erDiagram
  Investigation {
    Guid Id PK
    Guid BeginId "FloodProblem"
    Guid WaterSpeedId "FloodProblem"
    Guid AppearanceId "FloodProblem"
    Guid WereVehiclesDamagedId "RecordStatus"
    Guid WhenWaterEnteredKnownId "RecordStatus"
    Guid IsPeakDepthKnownId "RecordStatus"
    Guid FloodlineId "RecordStatus"
    Guid WarningReceivedId "RecordStatus"
    Guid WarningTimelyId "RecordStatus"
    Guid WarningAppropriateId "RecordStatus"
    Guid HistoryOfFloodingId "RecordStatus"
  }
  Investigation ||--o{ InvestigationDestination : Destinations
  Investigation ||--o{ InvestigationEntry : "Entries (how the water entered)"
  Investigation ||--o{ InvestigationCommunityImpact : CommunityImpacts
  Investigation ||--o{ InvestigationActionsTaken : ActionsTaken
  Investigation ||--o{ InvestigationHelpReceived : HelpReceived
  Investigation ||--o{ InvestigationWarningSource : WarningSources

  InvestigationDestination ||--|| FloodProblem : FloodProblemId
  InvestigationEntry ||--|| FloodProblem : FloodProblemId
  InvestigationCommunityImpact ||--|| FloodImpact : FloodImpactId
  InvestigationActionsTaken ||--|| FloodMitigation : FloodMitigationId
  InvestigationHelpReceived ||--|| FloodMitigation : FloodMitigationId
  InvestigationWarningSource ||--|| FloodMitigation : FloodMitigationId
```

## Where it is used

- [FloodReport](FloodReport.md)