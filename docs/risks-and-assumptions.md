# Risks and Assumptions — Astro Track Backend

Open questions and inconsistencies discovered while analyzing the existing Oracle schema, to be resolved (or explicitly accepted as-is) before implementation begins.

## Risks

1. **Ambiguous final state of sequences and indexes.** `Astro_Track_Project.sql` creates `idx_missions_lead_date`, `idx_r_id_affiliation`, and all four sequences multiple times across different "teaching" sections of the script, each followed by a `DROP`. Without running the script end-to-end against a live database and inspecting the resulting catalog, it's not possible to be certain which of these objects actually exist in the deployed database, or with what current values. **Assumption for now:** treat the two indexes as permanent (they appear in the final "Indexing and Query Optimization" section of the README) and the four sequences as *not* reliably present/live — do not depend on them existing for ID generation in the API; instead assume PKs are supplied by the application or via `RETURNING ... INTO` after insert, pending confirmation.

2. **No live Oracle database access.** None of the schema/business-logic assumptions in these documents have been validated against a running instance. All analysis is based solely on static reading of the DDL/PL_SQL scripts. Connection details, schema/owner name, and actual current data state are unknown.

3. **FK nullability contradictions.** Three foreign keys are declared `NOT NULL` but also `ON DELETE SET NULL`, which Oracle cannot actually enforce as written (`Researchers.affiliation_id`, `Missions.lead_researcher_id`, `Missions.affiliation_id`, and `Observations.telescope_id`/`Observations.researcher_id`). Either the `NOT NULL` or the `ON DELETE SET NULL` is wrong in the original design. **Assumption:** treat the columns as effectively non-nullable in the application layer (matching the stricter `NOT NULL` constraint) and do not rely on the `SET NULL` cascade behavior actually firing without deeper testing.

4. **`CelestialUpdater` PK-reassignment semantics.** This package's "update" operation changes the row's primary key value rather than updating in place. It's unclear whether this is intentional business behavior to be exposed via the API in some form, or purely a teaching-script artifact not meant to be replicated. **Assumption:** the REST API will *not* replicate this behavior; `PUT` will perform a normal same-key update. Needs confirmation from the project owner.

5. **`SYS_REFCURSOR` mapping from EF Core / ODP.NET.** `MISSION_ANALYSIS_PKG.FIND_MISSIONS_BY_OBJECT` returns a ref cursor. Calling this cleanly from .NET requires ADO.NET-level handling via `Oracle.ManagedDataAccess` (`OracleRefCursor`/`OracleDataReader`), which has not yet been prototyped in this codebase. This should be spiked/proof-of-concepted early, since it's the riskiest integration point technically.

6. **Draft/duplicate script variants.** The `sql/PL_SQL/` folder contains several sequence/index variants (`sequences.sql`, `sequences2.sql`, `sequences3-CelestialObjects.sql`, `sequence_indexes1.sql`, `sequence_indexes2.sql`, etc.) that overlap with and sometimes extend the main `Astro_Track_Project.sql` (e.g. extra indexes `idx_rp_researcher_id`, `idx_hp_research_id` appear only in `sequence_indexes1.sql`). It's unclear whether these supplementary indexes were ever applied to the real database. Treated as non-authoritative for this analysis; only `Astro_Track_Project.sql` is treated as the primary source of truth.

7. **Sample data quality.** Some `INSERT` comments in `Habitable_Planets` reference object names (e.g. "Proxima Centauri b") that don't match the actual `object_id` values used in the same statement (which correspond to different objects per the `CelestialObjects` sample data). This is a pre-existing sample-data/comment inconsistency, not something this repo will fix — flagged only so it isn't mistaken for a real business rule when writing seed/test data later.

8. **No authentication/authorization requirements defined.** Out of scope for Issue #1; assumed to be addressed in a future issue before any endpoint is exposed publicly.

## Assumptions carried into backend-architecture.md / api-resource-plan.md

- NUMBER columns without declared precision map to `long` (IDs) or `decimal` (unspecified numeric measures) — exact choice per-column to be finalized when each entity is implemented, not decided globally here.
- CHAR(1) Y/N columns map to `bool` in entities via a value converter, keeping DTOs simple.
- EF Core is used strictly Database First / no-migrations; the database schema is never generated or altered by the application.
- Single ASP.NET Core project (internally layered by folder) is used instead of a multi-project Clean Architecture split, given project scope — reversible later if needed.
- Existing PL/SQL business logic (habitability check, mission efficiency/status) is invoked from the API, never reimplemented in C#.

## Unresolved questions for the project owner

1. Should the four sequences (`mission_add_seq`, `mission_update_seq`, `celestial_add_seq`, `celestial_update_seq`) be treated as permanently deployed objects the API can rely on, or should the API manage its own ID strategy?
2. Is single-project or multi-project (Clean Architecture) solution structure preferred?
3. Should `CelestialUpdater`'s PK-reassignment update behavior be exposed anywhere in the API, or dropped entirely in favor of standard REST update semantics?
4. What connection/credentials strategy will be used for local development against Oracle (Docker container with Oracle XE, a shared dev instance, or something else)?
5. Are the supplementary indexes found only in `sequence_indexes1.sql` (`idx_rp_researcher_id`, `idx_hp_research_id`) actually present in the live database, and should the backend assume they exist for query performance?
