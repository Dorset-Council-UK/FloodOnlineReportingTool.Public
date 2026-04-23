```mermaid
flowchart LR 
    subgraph ANON["Anonymous"]
        A["Can create Eligibility Check"]
        B[/"Can login or add contacts"/]
        Z["No further ability to edit<br>or do anything with the record once saved"]
        A --> B
        B --> Z
    end

    subgraph CONTACT["Contact Details Only"]
        C["Provide email"]
        D["Provide phone only"]
        E["Receive calls<br>(unlikely)"]
        D --> E
    end

    subgraph MAGIC["Via Magic Link"]
        ML{{"Via magic link"}}
    end

    subgraph FULL["Full Account"]
        F["Logged in"]
        G["Can:<br>Edit reports<br>Have: Multiple reports"]
        H["Receive request<br>for further info"]
        I["Can get status updates"]
        J["Request other services<br>(if offered)"]
        F --> G
        F --> H
        F --> I
        F --> J
    end

    E --> Z

    B -->|"User Choice"| D
    B -->|"User Choice"| C
    B -->|"User Choice"| F

    C -->|"Process via Email"| ML
    C -->|"Process via Email"| I
    ML --> H

    style ML fill:gold,color:#000
    style ANON fill:#BAC8D3,opacity:0.5
    style CONTACT fill:#BAC8D3,opacity:0.25
    style MAGIC fill:#fff9c4,opacity:0.5
    style FULL fill:#BAC8D3,opacity:0.5
    linkStyle 1,2,7 stroke:orange,stroke-width:3px
    linkStyle 3,4,5,6 stroke:blue,stroke-width:3px
    linkStyle 11,12,13 stroke:red,stroke-width:3px

    %% Invisible spine to force column order
    ANON ~~~ CONTACT ~~~ MAGIC ~~~ FULL
