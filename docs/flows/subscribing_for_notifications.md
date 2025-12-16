## Notifications flow

After reporting flooding we encourage users to subscribe to notifications about their report. 
They can choose two routes:
- Subscribe to recieve notification emails only
- Register for an account to allow them to login and manage the current (and other linked) record and receive notification emails

See [registration](registration.md) for details on the registration flow.

## Subscription flow
```mermaid
flowchart TB
    Start((Start)) --> CollectDetails[Collect Details]
    CollectDetails -- Email & Name --> CollectDetails2[Initial Check] 
    CollectDetails2 -- Create verification code / expiry date --> VerifyEmail[Verify Email]
    VerifyEmail -- Remove code and verify --> EmailSubscription
    EmailSubscription(Subscription Created) --> Confirmation[/ Add Contacts /]
```

## Verification email
Gov.Notify template email will send the user a URL containing the verification endpoint and code.

## Post subscription
After submitting the subscription form, users are directed to the [contact flow](contact_flow.md).