# Infonetica – Configurable Workflow Engine (State‑Machine API)

Minimal take‑home implementation in .NET 8 / C# Minimal API fulfilling the required capabilities: define workflow state machines, start instances, execute actions with validation, inspect definitions & instances. Uses in‑memory persistence (optionally JSON snapshot export/import). Clean structure, small surface area, easy to extend.

---

## Environment

- Requires: .NET 8 SDK or later
- Tested on: Windows 10

---

## Quick Start

1. Create folder & clone (after you push this code)
2. Create solution + project
3. Remove default Program.cs and add the files below (or copy repo contents once published)
4. Build & run

```
dotnet run
```

API listens on http://localhost:5000 (HTTP) & https://localhost:7000 (HTTPS) by default

---

## Project Structure

Infonetica.Workflow/
 ├─ Infonetica.Workflow.csproj
 ├─ Program.cs
 ├─ Domain/
 │   ├─ StateDef.cs
 │   ├─ ActionDef.cs
 │   ├─ WorkflowDefinition.cs
 │   ├─ WorkflowInstance.cs
 │   └─ InstanceHistoryEntry.cs
 ├─ Dtos/
 │   ├─ CreateDefinitionRequest.cs
 │   ├─ DefinitionDto.cs
 │   ├─ InstanceDtos.cs
 │   └─ ApiError.cs
 ├─ Services/
 │   └─ WorkflowService.cs
 ├─ Persistence/
 │   └─ JsonSnapshotStore.cs   # optional, lightweight import/export
 └─ README.md  (this file)

---

## Minimal API Surface

Definitions

* POST   /workflow-definitions – create new definition (states + actions in one request).
* GET    /workflow-definitions – list definitions.
* GET    /workflow-definitions/{id} – get definition.

Instances

* POST   /workflow-instances – start new instance from definition.
* GET    /workflow-instances – list instances.
* GET    /workflow-instances/{id} – get instance (current state + history).
* POST   /workflow-instances/{id}/actions/{actionId} – execute action on instance.

Persistence (optional)

* POST   /_admin/export – returns JSON snapshot of definitions + instances.
* POST   /_admin/import – replace in‑memory store from posted snapshot JSON.

---

## Validation Rules Implemented

* Definition must contain >=1 state and exactly one IsInitial == true state.
* All IDs unique across states in same definition; actions unique.
* Action ToState must exist; all FromStates must exist.
* States & actions default Enabled = true when omitted.
* Instance starts in definition's initial state.
* Executing action checks:
  * Definition exists & instance belongs to it.
  * Action exists in that definition.
  * Action.Enabled == true.
  * Current state is enabled and in action.FromStates.
  * Current state is not final.
  * Target state exists & is enabled.

Errors return 400 with { code, message, details } JSON.

---

## Sample JSON – Create Definition

Example: basic document review flow.

```
{
  "id": "docflow",
  "name": "Document Review",
  "states": [
    {"id":"draft","name":"Draft","isInitial":true},
    {"id":"review","name":"In Review"},
    {"id":"approved","name":"Approved","isFinal":true},
    {"id":"rejected","name":"Rejected","isFinal":true}
  ],
  "actions": [
    {"id":"submit","name":"Submit for review","fromStates":["draft"],"toState":"review"},
    {"id":"approve","name":"Approve","fromStates":["review"],"toState":"approved"},
    {"id":"reject","name":"Reject","fromStates":["review"],"toState":"rejected"},
    {"id":"revise","name":"Send back to draft","fromStates":["review"],"toState":"draft"}
  ]
}
```

---

## Sample Usage (curl)

```
# create definition
curl -s -X POST http://localhost:5000/workflow-definitions \
  -H 'content-type: application/json' \
  -d '{"id":"docflow","name":"Document Review","states":[{"id":"draft","name":"Draft","isInitial":true},{"id":"review","name":"In Review"},{"id":"approved","name":"Approved","isFinal":true},{"id":"rejected","name":"Rejected","isFinal":true}],"actions":[{"id":"submit","name":"Submit","fromStates":["draft"],"toState":"review"},{"id":"approve","name":"Approve","fromStates":["review"],"toState":"approved"},{"id":"reject","name":"Reject","fromStates":["review"],"toState":"rejected"},{"id":"revise","name":"Revise","fromStates":["review"],"toState":"draft"}]}'

# start instance
curl -s -X POST http://localhost:5000/workflow-instances -H 'content-type: application/json' -d '{"definitionId":"docflow"}'
# => {"id":"<guid>","definitionId":"docflow","currentState":"draft",...}

# execute submit
curl -s -X POST http://localhost:5000/workflow-instances/<guid>/actions/submit

# approve
curl -s -X POST http://localhost:5000/workflow-instances/<guid>/actions/approve

# inspect instance
curl -s http://localhost:5000/workflow-instances/<guid>
```

---

## Assumptions & Shortcuts

- Workflow definitions are created in a single POST (not incrementally).
- IDs can be supplied by the client or auto-generated as GUIDs.
- States and actions default to enabled if not specified.
- Data is stored in-memory; use export/import for persistence.
- No authentication or user management.
- No pagination or filtering on list endpoints.
- No concurrency control beyond atomic dictionary operations.
- History is not bounded or paginated.

---

## Known Limitations

- No PATCH/PUT endpoints for incremental workflow editing (all states/actions must be provided at creation).
- Data is lost on server restart unless exported/imported.
- No user/audit tracking.
- No OpenAPI/Swagger UI.
- No pagination or filtering for large lists.
- No advanced graph validation (e.g., unreachable states, dead transitions).

---

## Extensibility Notes (TODO markers you might add later)

* PATCH endpoints to add/disable states or actions.
* Graph validation (unreachable states, dead transitions).
* Bulk history export, audit user info.
* Pluggable persistence (database, redis, etc.) behind interface.
* Swagger/OpenAPI generation.

