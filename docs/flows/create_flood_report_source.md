# Flow diagram for creating a flood report source

```mermaid
flowchart TB
    Index((Start)) --> PostcodeOrLocation{Postcode <br>or location?}
    subgraph FloodLocation[Location of the flood]
    PostcodeOrLocation -- Postcode --> Postcode
    Postcode --> PostcodeKnown{Postcode<br>known?}
    PostcodeKnown -- Yes --> Address
    PostcodeKnown -- No --> SelectLocation[Select location on map]
    SelectLocation -- Select Property --> Address
    PostcodeOrLocation -- Location --> Location
    Location --> PropertyType
    Address --> PropertyType[Property type]
    end
    PropertyType --> FloodAreas[Flood areas]
    FloodAreas --> TemporaryAddress{Evacuated to <br>a temporary <br>address?}
    TemporaryAddress -- Yes --> TempAddress[Temporary address postcode - optional]
    TemporaryAddress -- No --> Vulnerability
    TempAddress -- Yes --> ChooseTempAddress[Choose temporary address]
    TempAddress -- No --> Vulnerability
    ChooseTempAddress --> Vulnerability
    Vulnerability --> FloodStarted[Flood started]
    FloodStarted --> OngoingFlooding{Flooding<br>on going?}
    OngoingFlooding -- Yes --> FloodCause[Flood causes]
    OngoingFlooding -- No --> FloodDuration[Flood duration]
    FloodDuration --> FloodCause
    FloodCause --> SecondaryFloodCauseRainwater{Rainwater<br>flood cause<br>chosen?}
    SecondaryFloodCauseRainwater -- Yes --> SecondaryFloodCauseType[Secondary flood cause]
    SecondaryFloodCauseRainwater -- No --> Summary[Check your answers]
    SecondaryFloodCauseType --> Summary
    FloodCause --> Summary[Check your answers]
    Summary --> Save[(Save to database)]
    Save --> Notifications{Choose Contacts}
    Notifications -- Subscribe --> SubscribeFlow[/Subscribe for notifications/]
    Notifications -- Register/Login --> RegistrationFlow[/Register for an account/]
    Notifications -- No thanks --> Confirmation((( End )))

```
## After creating a flood report
- The user can optionally choose to subscribe to notifications or more.
- See the following for more details:
    - [Subscribing for notifications flow](subscribing_for_notifications.md)
    - [Registration flow](registration.md)