# API Resource Plan — Astro Track

Planned REST surface for the Astro Track backend. No controllers exist yet — this is the design for how each database entity becomes an API resource, added incrementally per the backend-architecture.md structure.

## Conventions

- Base path versioned: `/api/v1/...`
- Resource names are kebab-case plurals of the entity (e.g. `celestial-objects`).
- Errors returned as RFC 7807 `ProblemDetails` (validation errors, not-found, and PL/SQL-raised business errors such as `ORA-20001` translated to a 400/409 with a clear message).
- List endpoints support pagination (`?page=`, `?pageSize=`) and basic filtering by indexed/CHECK-constrained fields (e.g. `category`, `event_type`) once implemented — not required for the first vertical slice.
- Composite-key bridge resources expose both key parts explicitly in the URL (e.g. `/api/v1/missions/{missionId}/observations/{observationId}`), not a synthetic id.

## Standard CRUD resources

| Resource | Base route | Verbs |
|---|---|---|
| Celestial Objects | `/api/v1/celestial-objects` | GET (list), GET `{id}`, POST, PUT `{id}`, DELETE `{id}` |
| Events | `/api/v1/events` | GET (list), GET `{id}`, POST, PUT `{id}`, DELETE `{id}` |
| Affiliations | `/api/v1/affiliations` | GET (list), GET `{id}`, POST, PUT `{id}`, DELETE `{id}` |
| Researchers | `/api/v1/researchers` | GET (list), GET `{id}`, POST, PUT `{id}`, DELETE `{id}` |
| Research Papers | `/api/v1/research-papers` | GET (list), GET `{id}`, POST, PUT `{id}`, DELETE `{id}` |
| Telescopes | `/api/v1/telescopes` | GET (list), GET `{id}`, POST, PUT `{id}`, DELETE `{id}` |
| Observations | `/api/v1/observations` | GET (list), GET `{id}`, POST, DELETE `{id}` — no PUT: `observation_date` is server-set by DB triggers, and other fields are effectively immutable measurement data by convention |
| Missions | `/api/v1/missions` | GET (list), GET `{id}`, POST, PUT `{id}`, DELETE `{id}` |

## Bridge resources

| Resource | Base route | Verbs |
|---|---|---|
| Mission Observations | `/api/v1/missions/{missionId}/observations` | GET (list for mission), POST (link an observation to a mission), GET `/{observationId}`, PUT `/{observationId}` (update role/success/data size), DELETE `/{observationId}` (unlink) |
| Habitable Planets | `/api/v1/celestial-objects/{objectId}/habitability-assessments` | GET (list assessments for an object), POST, GET `/{eventId}/{researchId}`, PUT `/{eventId}/{researchId}`, DELETE `/{eventId}/{researchId}` |

## Endpoints wrapping existing PL/SQL business logic

These do not reimplement logic — they invoke the corresponding Oracle package via the `Infrastructure` layer (see backend-architecture.md §1.6):

| Endpoint | Wraps | Notes |
|---|---|---|
| `GET /api/v1/celestial-objects/{id}/habitability` | `SIMPLE_HABITABILITY_PKG.IS_POTENTIALLY_HABITABLE` | Returns `{ objectId, isPotentiallyHabitable: bool }` |
| `GET /api/v1/missions/{id}/status` | `MISSION_ANALYSIS_PKG.GET_MISSION_STATUS` | Returns the formatted status report as structured JSON fields, not a raw string, where practical |
| `GET /api/v1/missions/{id}/efficiency` | `MISSION_ANALYSIS_PKG.CALCULATE_MISSION_EFFICIENCY` | Returns `{ missionId, efficiencyScore: number }` |
| `GET /api/v1/celestial-objects/{id}/missions` | `MISSION_ANALYSIS_PKG.FIND_MISSIONS_BY_OBJECT` | Returns the list of missions that have observed the given object (via the ref cursor) |

## Notes on mutability

- `Observations`: create-only (plus delete) at the API level, reflecting that `observation_date` is trigger-controlled and the record represents an immutable measurement.
- `Mission_Observations`: blocked server-side (mirroring `mission_observation_trg`) from linking new observations to a mission whose `end_date` is already set — the API should surface the resulting Oracle error as a 409 Conflict with a clear message, not swallow it.
- `Habitable_Planets`: `recommended_population` is always trigger-normalized (forced to 0 when not habitable, defaulted to 0 when null) — the API should treat this as a read-only derived field on the response even though it's client-suppliable on create/update.
- Whether `PUT /celestial-objects/{id}` should mirror `CelestialUpdater`'s odd "reassign the primary key" update semantics, or perform a normal same-key update, is an open question — see risks-and-assumptions.md. Default assumption for this plan: normal same-key update (standard REST semantics), since replicating a PK-changing "update" via HTTP PUT would violate normal REST expectations for that route.
