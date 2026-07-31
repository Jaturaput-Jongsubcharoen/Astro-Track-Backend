# Backend Architecture Plan — Astro Track

This document proposes the ASP.NET Core backend architecture for Astro Track. It is a design plan only — no project has been created yet (see risks-and-assumptions.md for open questions that should be resolved first).

## 1. Oracle-specific Entity Framework Core considerations

### 1.1 Identifier casing
Oracle stores unquoted identifiers uppercase. The DDL uses mixed case (`CelestialObjects`, `object_id`), but the real catalog objects are `CELESTIALOBJECTS`, `OBJECT_ID`, etc. Every entity mapping must be explicit:
- `entity.ToTable("CELESTIALOBJECTS")`
- `entity.Property(e => e.ObjectId).HasColumnName("OBJECT_ID")`

EF Core's default convention-based name matching (PascalCase → as-is) will not match Oracle's uppercase catalog, so relying on conventions alone will fail at runtime.

### 1.2 Numeric precision
Columns declared as plain `NUMBER` (no precision/scale), e.g. `object_id`, `mission_id`, need an explicit .NET type decision — candidates are `int`, `long`, or `decimal`. Columns with declared precision/scale (e.g. `NUMBER(4,2)`, `NUMBER(16,6)`) should map to `decimal` with `HasPrecision(p, s)` matching the DDL exactly, to avoid silent rounding differences between the app and the database CHECK constraints.

### 1.3 CHAR(1) Y/N flags
There are ~20 `CHAR(1)` boolean-like columns across the schema (11 on `CelestialObjects` alone, plus flags on `Telescopes`, `Missions`-adjacent tables, etc.). Oracle has no native boolean type. Two viable approaches:
- Map the entity property as `string` (`"Y"`/`"N"`) and expose a computed `bool` on the DTO.
- Map as `bool` in the entity using a `ValueConverter<bool, string>` (`true` ↔ `"Y"`, `false` ↔ `"N"`), keeping DTOs simple.

Recommendation: use the `ValueConverter` approach so both entities and DTOs work naturally with `bool` in C#, while persistence still round-trips correctly to `'Y'`/`'N'`.

### 1.4 DATE handling
Oracle `DATE` always includes a time component, even for date-only business fields (`event_date`, `start_date`, `discovery_date`). Map to `DateTime`, not `DateOnly`, to avoid truncation surprises, and document that time components should generally be ignored/zeroed by convention rather than by column type.

### 1.5 No migrations — Database First only
The Oracle schema is the pre-existing source of truth (owned by `Astro-Track-Oracle-SQL`). EF Core in this project must:
- Never run `Add-Migration` / `dotnet ef migrations` against this schema.
- Use Fluent API configuration to describe the *existing* structure, not to generate or alter it.
- Disable any auto-migrate-on-startup behavior.

### 1.6 Invoking existing PL/SQL logic
EF Core's LINQ layer has no native support for calling Oracle packaged functions/procedures, especially ones returning `SYS_REFCURSOR` (e.g. `MISSION_ANALYSIS_PKG.FIND_MISSIONS_BY_OBJECT`) or using `OUT` parameters. These must be called via raw ADO.NET, using `context.Database.GetDbConnection()` with `Oracle.ManagedDataAccess`/`Oracle.EntityFrameworkCore`'s underlying `OracleCommand`, `CommandType.StoredProcedure`, and explicit `OracleParameter`s (including an `OracleDbType.RefCursor` output parameter where applicable). Scalar/function calls without ref cursors (e.g. `SIMPLE_HABITABILITY_PKG.IS_POTENTIALLY_HABITABLE`) can potentially use `FromSqlRaw`/`ExecuteScalar`-style calls, but should still go through a dedicated infrastructure helper rather than being scattered across services.

This is a deliberate design choice: **business logic already implemented in PL/SQL (habitability, mission efficiency/status) is called, not reimplemented in C#**, to keep the database as the single source of truth for those rules.

### 1.7 The `CelestialUpdater` PK-reassignment quirk
`CelestialUpdater.UpdateLatestCelestialObject` doesn't do a normal `UPDATE ... WHERE id = x`; it reassigns the row's primary key to a new sequence-generated value as part of the "update". This is unusual and should **not** be silently replicated by a REST `PUT`/`PATCH` endpoint without an explicit decision (see risks-and-assumptions.md) — a normal EF Core update (same PK, changed columns) is the expected default REST semantic unless told otherwise.

## 2. Proposed solution structure

Given the portfolio/course scope, a **single ASP.NET Core Web API project**, internally layered by folder, is recommended over a multi-project Clean Architecture split — it is simpler to build incrementally and still keeps concerns separated. (Open decision point — see below.)

```
Astro-Track-Backend/
├── AstroTrack.Api/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Controllers/
│   ├── Models/            # EF Core entity classes (one per table/bridge)
│   ├── Data/
│   │   ├── AstroTrackDbContext.cs
│   │   └── Configurations/ # IEntityTypeConfiguration<T> per entity
│   ├── DTOs/               # Read/Create/Update DTOs per resource
│   ├── Repositories/       # Interfaces + Oracle-backed implementations
│   ├── Services/           # Business/orchestration layer, calls PL/SQL where applicable
│   └── Infrastructure/     # Raw ADO.NET helpers for packaged functions/procedures, ref cursor mapping
├── AstroTrack.Api.Tests/    # (future issue) xUnit test project
├── Dockerfile               # (future issue)
└── .github/workflows/       # (future issue) CI pipeline
```

**Decision point for the team:** single project (above) vs. multi-project (`AstroTrack.Domain`, `AstroTrack.Infrastructure`, `AstroTrack.Application`, `AstroTrack.Api`). Multi-project adds ceremony that may not pay off at this scope; single project can still be refactored into multiple projects later if needed. Recommend starting single-project unless directed otherwise.

## 3. Planned models (EF Core entities)

One entity class per table, in `Models/`, each mapped via a dedicated `IEntityTypeConfiguration<T>` in `Data/Configurations/` (rather than one giant `OnModelCreating`), for maintainability as all 10 tables are added incrementally:

`CelestialObject`, `Event`, `Affiliation`, `Researcher`, `ResearchPaper`, `Telescope`, `Observation`, `Mission`, `MissionObservation` (bridge, composite key), `HabitablePlanet` (bridge, composite key).

Each configuration explicitly sets: `ToTable`, every `HasColumnName`, PK (`HasKey`, including composite keys for the two bridge tables), FK relationships with the correct delete behavior mirrored from the DDL (`Cascade`, `SetNull`, or `Restrict`/`NoAction` for `Habitable_Planets`), and CHECK-constraint-backed value restrictions where practical (e.g., enum-like string fields).

## 4. Planned DTOs

Per top-level resource, three DTOs: `{Entity}Dto` (read), `Create{Entity}Dto`, `Update{Entity}Dto`. Fields that are server-controlled (e.g. `Observation.observation_date`, forced by triggers) are excluded from `Create`/`Update` DTOs and only appear in the read DTO. Bridge-table DTOs (`MissionObservationDto`, `HabitablePlanetDto`) expose the composite key as explicit id fields rather than a synthetic single id.

## 5. Planned repositories

One repository interface + implementation per aggregate root (`ICelestialObjectRepository`, `IEventRepository`, `IAffiliationRepository`, `IResearcherRepository`, `IResearchPaperRepository`, `ITelescopeRepository`, `IObservationRepository`, `IMissionRepository`, `IMissionObservationRepository`, `IHabitablePlanetRepository`), each wrapping standard EF Core CRUD against `AstroTrackDbContext`. No repository re-implements PL/SQL logic — that lives in `Infrastructure`/`Services`.

## 6. Planned services

One service per resource, composing its repository plus (where relevant) the `Infrastructure` PL/SQL helpers:
- `CelestialObjectService` — CRUD + `CheckHabitabilityAsync` (calls `SIMPLE_HABITABILITY_PKG.IS_POTENTIALLY_HABITABLE`/`GET_HABITABILITY_INFO`).
- `MissionService` — CRUD + `GetStatusAsync`, `GetEfficiencyAsync`, `FindByObjectAsync` (calls `MISSION_ANALYSIS_PKG`).
- Remaining services (`EventService`, `AffiliationService`, `ResearcherService`, `ResearchPaperService`, `TelescopeService`, `ObservationService`, `MissionObservationService`, `HabitablePlanetService`) start as plain CRUD wrappers; extended only if a specific PL/SQL routine needs surfacing.

## 7. Planned controllers

One controller per top-level resource plus the two bridge resources, following standard REST verbs (see api-resource-plan.md for the full endpoint list), added incrementally alongside their model/DTO/repository/service counterparts — not all at once.
