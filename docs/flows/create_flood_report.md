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