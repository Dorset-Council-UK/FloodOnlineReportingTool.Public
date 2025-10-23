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
  }
  Investigation ||--|| FloodProblem : BeginId
  Investigation ||--|| FloodProblem : WaterSpeedId
  Investigation ||--|| FloodProblem : AppearanceId
  Investigation ||--|| RecordStatus : WereVehiclesDamagedId
  Investigation ||--o| RecordStatus : WhenWaterEnteredKnownId
  Investigation ||--|| RecordStatus : IsPeakDepthKnownId
  Investigation ||--|| RecordStatus : FloodlineId
  Investigation ||--|| RecordStatus : WarningReceivedId
  Investigation ||--o| RecordStatus : WarningTimelyId
  Investigation ||--o| RecordStatus : WarningAppropriateId
  Investigation ||--|| RecordStatus : HistoryOfFloodingId
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