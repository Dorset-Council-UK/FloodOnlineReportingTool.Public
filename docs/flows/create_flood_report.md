# Flow diagram for creating a flood report

```mermaid
flowchart TB
    Index((Start)) --> Postcode
    Postcode --> PostcodeKnown{Postcode<br>known?}
    PostcodeKnown -- Yes --> Address
    PostcodeKnown -- No --> Location
    Location --> Address
    Address --> PropertyType[Property type]
    PropertyType --> FloodAreas[Flood areas]
    FloodAreas --> Vulnerability
    Vulnerability --> FloodStarted[Flood started]
    FloodStarted --> OngoingFlooding{Flooding<br>on going?}
    OngoingFlooding -- Yes --> FloodSource[Flood source]
    OngoingFlooding -- No --> FloodDuration[Flood duration]
    FloodDuration --> FloodSource
    FloodSource --> Summary[Check your answers]
    Summary --> Save[(Save to database)]
    Save --> Confirmation((( End )))
```
## After creating a flood report
The user can choose to add contact details as:
 - an anonymous reporter (do nothing - report remains anonymous)
 - just providing the contact details
 - a logged in user

The flow for adding contact details is described below

```mermaid
flowchart TB
    Confirmation((Flood report created)) --> Start[Add contact details?]
    Start -- Explain the process and provide option to login --> WhoAreYou
    WhoAreYou -- Home Owner --> AddContact
    WhoAreYou -- Renter --> AddContact
    WhoAreYou -- Non resident --> ProvideOtherContactDetails
    AddContact[Add contact details] --> ProvideOtherContactDetails
    ProvideOtherContactDetails[Provide other contact details] --> Summary[Check your answers]
    Summary --> Save[(Save to database)]
    Save --> Confirmation2((( End )))
```
