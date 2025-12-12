## Contact Details Flow

At this point the user will be subscribed or logged in.


```mermaid
flowchart TB
    Start((Start)) --> Email{Email Verified?}
    Email -- Yes --> WhoAreYou[Who are you?]
    Email -- No --> End((End))
    WhoAreYou -- Home Owner --> ProvideOtherContactDetails
    WhoAreYou -- Renter --> ProvideOtherContactDetails
    WhoAreYou -- Non resident --> Summary[Check your answers]
    ProvideOtherContactDetails[Provide other contact details] --> Summary[Check your answers]
    Summary --> Save[(Save to database)]
    Save --> Confirmation2((( End )))
```

- Users can optionally provide phone contacts on the who are you page.
- They will also have the name and email prefilled from earlier in the process.