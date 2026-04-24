# Glossary of Terms

## Overview

This glossary summarises the main terms used in the flood reporting data model.

This module collects and manages flood information submitted by individuals. More than one individual may report the same flood event, so the records created in this module are treated as **flood report source** records.

A flood report source may later be linked to a single **flood report** record in another module. For end users, the system may refer to this simply as a "flood report", but in the data model it remains a source record until linked.

## Terms

### Flood Report Source

A **Flood Report Source** is an overview of a reported flood event. It brings together the main eligibility information, status, investigation details, and related contact records.

A flood report source may be:
- anonymous
- linked to a user account
- linked to contact details only

This affects whether the record can be edited later or whether further information can be requested.

### Eligibility Check

An **Eligibility Check** is the information captured when someone completes the online flood report form. It represents an assessment of whether a person may qualify for assistance related to flood damage.

It includes key details about the flood event, including whether vulnerable people are involved.

### Investigation

An **Investigation** represents follow-up work carried out by the relevant authority, for example in relation to grants or formal review of a flood event.

It captures more detailed information about the incident, including flood behaviour, impacts, actions taken, and other supporting details.

### Contact Record

A **Contact Record** represents a real person associated with a flood report source or flood report.

It is used to:
- keep in touch with a person
- request more information where appropriate
- allow editing of some report details in some cases

Contact details may be provided directly by the public or on behalf of someone else, so email verification is important.

### Report Owner

The **Report Owner** is the main contact identified from a flood report's contact records, where the related `SubscribeRecord` is marked with `IsRecordOwner` within `FloodReport.ContactRecords`.

This identified contact represents the person with primary ownership of the submitted record. Depending on how the record was created, this may allow future access, updates, or requests for more information.

### Other Contacts

**Other Contacts** are additional contacts linked to the same flood report source.

These represent other people connected to the flood event but who are not the primary report owner.

### Contact Types

Contact records may be categorised using the following contact types:

- Tenant
- Home owner
- Non resident

These help describe the person's relationship to the affected property or flood event.

### Subscribe Records

We are using subscription records linked to a contact entity to manage the contact details associated with a flood report source. This allows us to create a little separation from the individual (contact) and the updates they want to receive (subscription).

## Other Related Terms

### Status

**Status** is used to describe the current state of a record and to help determine whether an investigation can be completed.

### Organisation / Authority

An **Organisation** is the flood risk management **Authority** responsible for handling or responding to the report.

### Flood Causes

**Flood Causes** describe the source or cause of the flood water. This is used to determine the correct **Authority** to be responsible for this report.

### Flood Problems / Impacts

**Flood Problems** and **Flood Impacts** describe the effects of the flood event. These are used to help determine severity and classify internal or external flooding.

