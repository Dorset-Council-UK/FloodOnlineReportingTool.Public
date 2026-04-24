# Flood Report Source Relationships

## Overview

Please use [Glossary of Terms](Glossary.md) for definitions of the main terms used in this application.

## Relationships
```mermaid
  flowchart LR
    FRS["FloodReportSource"]
    EC["EligibilityCheck<br>1..1"]
    INV["Investigation<br>0..1"]
    RO["Report Owner<br>linked by ReportOwnerId<br>0..1"]
    OC["Other Contacts<br>0..2"]

    TYPES["ContactRecordType enum<br>- Tenant<br>- HomeOwner<br>- NonResident"]

    FRS --> EC
    FRS --> INV
    FRS --> RO
    FRS --> OC

    RO --> TYPES
    OC --> TYPES
```

## Relationship Summary

A flood report source can link to:
- one **Eligibility Check**
- zero or one **Investigation**
- zero or one **Report Owner**
- zero to two **Other Contacts**

The contacts associated with a record are drawn from the available contact types and help define who reported the event and who may be contacted later.