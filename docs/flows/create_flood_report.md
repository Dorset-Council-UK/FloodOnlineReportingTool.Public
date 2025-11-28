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
The user can optionally choose to subscribe to notifications or more.
See [subscribing for notifications](subscribing_for_notifications.md) for more details.