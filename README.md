# Flood Online Reporting Tool

The Flood Online Reporting Tool is a .NET based web application targeted at members of the public who need to report flooding to lead local flood authorities and other responsible risk authorities. 

It is built using the GDS Design System and is Open Source Software licenced under the MIT licence. It is intended to be used by local authorities and government departments.

## Dependencies

To run the Flood Online Reporting Tool with minimal modification, you will need:

- A web server capable of running .NET applications, such as IIS, Kestrel or Azure
- PostgreSQL 13+ with PostGIS extension

The Flood Online Reporting Tool uses Entity Framework Core. Postgres has been set up for this project, but with some modifications, any Entity Framework Core compatible provider should work. For a full list of providers, check the Entity Framework docs.

## Bugs

Please report any bugs via the issue tracker in Azure DevOps.

## Contributing

Please see our guide on [contributing](CONTRIBUTING.md) if you're interested in getting involved.

## Reporting security issues

Security issues should be reported privately, to the project team via email at [fort@dorsetcouncil.gov.uk](mailto:fort@dorsetcouncil.gov.uk). You should receive a response within 24 hours during business days.

## Core developers

The Flood Online Reporting tool is a Dorset Council Open Source project. The core developers are currently Dorset Council staff. If you want to become a core developer, Please see our guide on [contributing](CONTRIBUTING.md).

## Licence

Unless stated otherwise, the codebase is released under the MIT License. This covers both the codebase and any sample code in the documentation.