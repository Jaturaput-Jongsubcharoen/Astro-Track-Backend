# Database Schema Analysis — Astro Track

Source of truth: [`Astro-Track-Oracle-SQL/sql/Astro_Track_Project.sql`](../../Astro-Track-Oracle-SQL/sql/Astro_Track_Project.sql) (read-only reference; not modified by this repo).

Oracle stores unquoted identifiers in **UPPERCASE**. All table/column names below are written as they appear in the DDL (mixed case), but the actual catalog objects are uppercase (e.g. `CelestialObjects` → `CELESTIALOBJECTS`, `object_id` → `OBJECT_ID`). Any ORM mapping must target the uppercase forms explicitly.

## 1. Entity tables

### CelestialObjects
PK: `object_id` (NUMBER)

| Column | Type | Constraints |
|---|---|---|
| object_id | NUMBER | PK |
| object_name | VARCHAR2(30) | NOT NULL |
| category | VARCHAR2(50) | NOT NULL, CHECK IN ('Planet','Exoplanet','Moon','Dwarf Planet','Asteroid','Comet','Black Hole','Neutron Star','Star') |
| distance_light_years | NUMBER(16,6) | DEFAULT 0 |
| discovery_date | DATE | DEFAULT NULL |
| in_solar_system | CHAR(1) | DEFAULT 'N', CHECK IN ('Y','N') |
| habitability_score | NUMBER(4,2) | DEFAULT 0, CHECK BETWEEN 0.00 AND 10.00 |
| surface_temperature | NUMBER(12,2) | DEFAULT NULL |
| gravity | NUMBER(5,2) | DEFAULT NULL, CHECK BETWEEN 0.0 AND 100.0 |
| nitrogen, oxygen, co2, sulfuric_acid, hydrogen, helium, methane, water_vapor, silicates, iron, nickel | CHAR(1) each | DEFAULT 'N', CHECK IN ('Y','N') — 11 gas/composition flags |

No FKs. Central table referenced by `Observations` and `Habitable_Planets`.

### Events
PK: `event_id` (NUMBER)

| Column | Type | Constraints |
|---|---|---|
| event_id | NUMBER | PK |
| event_name | VARCHAR2(50) | NOT NULL, UNIQUE |
| event_type | VARCHAR2(50) | NOT NULL |
| event_date | DATE | NOT NULL |
| visibility_score | NUMBER(3,1) | DEFAULT 5.0, CHECK BETWEEN 0.0 AND 10.0 |
| impact_on_habitability | VARCHAR2(19) | DEFAULT 'None', CHECK IN ('None','Mild Radiation','Severe Radiation','Climate Shift','Atmospheric Changes') |
| estimated_duration_days | NUMBER(5,2) | DEFAULT 1, CHECK > 0 |

No FKs. Referenced by `Habitable_Planets`.

### Affiliations
PK: `affiliation_id` (NUMBER)

| Column | Type | Constraints |
|---|---|---|
| affiliation_id | NUMBER | PK |
| affiliation_name | VARCHAR2(50) | NOT NULL, UNIQUE |

No FKs. Referenced by `Researchers` and `Missions`.

### Researchers
PK: `researcher_id` (NUMBER)

| Column | Type | Constraints |
|---|---|---|
| researcher_id | NUMBER | PK |
| researcher_name | VARCHAR2(30) | NOT NULL |
| contact_email | VARCHAR2(50) | UNIQUE, CHECK REGEXP_LIKE email format |
| phone_number | VARCHAR2(15) | UNIQUE, CHECK REGEXP_LIKE `^\+\d{1,3}-\d{1,4}-\d{4,10}$` |
| affiliation_id | NUMBER | NOT NULL, FK → `Affiliations(affiliation_id)` ON DELETE SET NULL |

⚠️ Constraint contradiction: `affiliation_id` is `NOT NULL` but its FK is `ON DELETE SET NULL`, which is only enforceable if the column is nullable. Flagged in risks-and-assumptions.md.

### ResearchPapers
PK: `paper_id` (NUMBER)

| Column | Type | Constraints |
|---|---|---|
| paper_id | NUMBER | PK |
| title | VARCHAR2(30) | NOT NULL |
| publication_date | DATE | DEFAULT NULL |
| focus_area | VARCHAR2(50) | DEFAULT 'General Astronomy', CHECK IN ('Exoplanets','Space Radiation','Terraforming','Astrobiology','General Astronomy','Unknown') |
| journal | VARCHAR2(50) | DEFAULT 'Unknown Journal', CHECK IN (10 named journals + 'Unknown Journal') |
| doi | VARCHAR2(50) | UNIQUE |
| paper_score | NUMBER(3,2) | DEFAULT 0 |
| researcher_id | NUMBER | nullable, FK → `Researchers(researcher_id)` ON DELETE SET NULL |

### Telescopes
PK: `telescope_id` (NUMBER)

| Column | Type | Constraints |
|---|---|---|
| telescope_id | NUMBER | PK |
| telescope_name | VARCHAR2(40) | NOT NULL, UNIQUE |
| location | VARCHAR2(50) | DEFAULT 'Unknown Location' |
| type | VARCHAR2(16) | NOT NULL (Optical / Radio / Infrared / Ultraviolet) |
| aperture_size | NUMBER(5,2) | DEFAULT 1.0, CHECK > 0 |
| observation_range_ly | NUMBER(11) | DEFAULT NULL, CHECK >= 0 |
| optical, infrared, ultraviolet | CHAR(1) each | DEFAULT 'N', CHECK IN ('Y','N') |

No FKs. Referenced by `Observations`.

### Missions
PK: `mission_id` (NUMBER)

| Column | Type | Constraints |
|---|---|---|
| mission_id | NUMBER | PK |
| mission_name | VARCHAR2(30) | NOT NULL, UNIQUE |
| mission_purpose | VARCHAR2(100) | DEFAULT 'Unknown', CHECK LENGTH(TRIM(...)) > 0 |
| start_date | DATE | NOT NULL |
| end_date | DATE | DEFAULT NULL, CHECK (NULL OR end_date >= start_date) |
| lead_researcher_id | NUMBER | NOT NULL, FK → `Researchers(researcher_id)` ON DELETE SET NULL |
| affiliation_id | NUMBER | NOT NULL, FK → `Affiliations(affiliation_id)` ON DELETE SET NULL |

Same NOT NULL / ON DELETE SET NULL contradiction as `Researchers.affiliation_id` (both FKs here).

`end_date IS NULL` is the signal for "mission still active" — used by triggers and `MISSION_ANALYSIS_PKG`.

## 2. Bridge tables

### Observations
PK: `observation_id` (NUMBER)

| Column | Type | Constraints |
|---|---|---|
| observation_id | NUMBER | PK |
| object_id | NUMBER | NOT NULL, FK → `CelestialObjects(object_id)` ON DELETE CASCADE |
| telescope_id | NUMBER | NOT NULL, FK → `Telescopes(telescope_id)` ON DELETE SET NULL |
| researcher_id | NUMBER | NOT NULL, FK → `Researchers(researcher_id)` ON DELETE SET NULL |
| observation_date | DATE | NOT NULL (also force-set by trigger, see below) |
| xray_flux | NUMBER(10,3) | DEFAULT 0, CHECK >= 0 |
| redshift | NUMBER(6,5) | DEFAULT 0, CHECK BETWEEN -1 AND 10 |

Links `CelestialObjects`, `Telescopes`, `Researchers` — a true many-to-many junction between objects and telescopes/researchers, but modeled as its own entity with a surrogate PK (not a composite FK-only bridge) because it carries its own measurement data (xray_flux, redshift).

### Mission_Observations (bridge)
Composite PK: (`mission_id`, `observation_id`)

| Column | Type | Constraints |
|---|---|---|
| mission_id | NUMBER | PK part, FK → `Missions(mission_id)` ON DELETE CASCADE |
| observation_id | NUMBER | PK part, FK → `Observations(observation_id)` ON DELETE CASCADE |
| mission_name | VARCHAR2(20) | NOT NULL (denormalized copy of Missions.mission_name) |
| observation_role | VARCHAR2(50) | DEFAULT 'General Observation' |
| data_collected_size | NUMBER(10,2) | DEFAULT 0, CHECK >= 0 |
| observation_success | CHAR(1) | DEFAULT 'Y', CHECK IN ('Y','N') |
| last_updated | DATE | DEFAULT SYSDATE |

Implements the Missions ↔ Observations many-to-many relationship, with per-pairing metadata (role, success flag, data volume).

### Habitable_Planets (bridge)
Composite PK: (`object_id`, `event_id`, `research_id`)

| Column | Type | Constraints |
|---|---|---|
| object_id | NUMBER | PK part, FK → `CelestialObjects(object_id)` |
| event_id | NUMBER | PK part, FK → `Events(event_id)` |
| research_id | NUMBER | PK part, FK → `ResearchPapers(paper_id)` |
| is_habitable | CHAR(1) | DEFAULT 'N', CHECK IN ('Y','N') |
| habitability_reason | VARCHAR2(50) | DEFAULT 'Unknown' |
| recommended_population | NUMBER | DEFAULT NULL, CHECK (NULL OR >= 0) |
| last_evaluated | DATE | DEFAULT SYSDATE |

Three-way junction linking a celestial object, the astronomical event context, and the supporting research paper for a habitability assessment. No `ON DELETE` clause on its FKs — default is `NO ACTION` (deletes of a referenced row will be blocked while dependent rows exist here).

## 3. Relationship summary

- `Affiliations` (1) → (M) `Researchers`
- `Affiliations` (1) → (M) `Missions`
- `Researchers` (1) → (M) `ResearchPapers`
- `Researchers` (1) → (M) `Missions` (as `lead_researcher_id`)
- `Researchers` (1) → (M) `Observations`
- `CelestialObjects` (1) → (M) `Observations`
- `Telescopes` (1) → (M) `Observations`
- `Missions` (M) ↔ (M) `Observations` via `Mission_Observations`
- `CelestialObjects` (M) ↔ (M) `Events` ↔ (M) `ResearchPapers` via `Habitable_Planets` (3-way composite bridge)

## 4. Indexes

| Index | Table | Columns | Purpose |
|---|---|---|---|
| `idx_missions_lead_date` | Missions | (lead_researcher_id, start_date) | Supports the query matching researchers whose mission start date equals one of their observation dates (used by `ResearchMatchManager`) |
| `idx_r_id_affiliation` | Researchers | (researcher_id, affiliation_id) | Supports the query finding researchers affiliated with multiple habitable-planet assessments (used by `HabitableResearchAnalysis`) |

Supplementary indexes appear only in the alternate/draft script `sql/PL_SQL/sequence_indexes1.sql` (not the main script): `idx_rp_researcher_id ON researchpapers(researcher_id)` and `idx_hp_research_id ON habitable_planets(research_id)`. These are not present in the authoritative `Astro_Track_Project.sql` and should be treated as draft/optional (see risks-and-assumptions.md).

## 5. Sequences

| Sequence | Start | Increment | Used by |
|---|---|---|---|
| `mission_add_seq` | 11 | 1 | Demo INSERTs of new missions |
| `mission_update_seq` | 13 | 1 | Demo "update" of a mission (reassigns PK) |
| `celestial_add_seq` | 22 | 1 | `CelestialManager.AddDefaultCelestialObjects` |
| `celestial_update_seq` | 100 | 1 | `CelestialUpdater.UpdateLatestCelestialObject` |

All four sequences are created, demonstrated, and then `DROP SEQUENCE`'d within the same script (teaching pattern) — see risks-and-assumptions.md for why their real, final, deployed state is ambiguous.

## 6. Triggers

| Trigger | Table | Timing/Event | Behavior |
|---|---|---|---|
| `create_observation_trg` | Observations | BEFORE INSERT | Forces `observation_date := SYSDATE`, overriding any client-supplied value |
| `future_observations_trg` | Observations | BEFORE INSERT OR UPDATE | Raises `ORA-20001` if `observation_date > SYSDATE` |
| `mission_observation_trg` | Mission_Observations | BEFORE INSERT | Raises `ORA-20001` if the target mission's `end_date IS NOT NULL` (mission already completed) |
| `habplanets_null_trg` | Habitable_Planets | BEFORE INSERT OR UPDATE | Replaces a NULL `recommended_population` with 0 |
| `habplanets_habitable_trg` | Habitable_Planets | BEFORE INSERT OR UPDATE | Forces `recommended_population := 0` whenever `is_habitable = 'N'` |

Note: `create_observation_trg` and `future_observations_trg` together mean the API can never truly control `observation_date` on insert — it is always "now" at insert time. This must be reflected in the DTO/service design (the field is effectively server-generated on create).

## 7. PL/SQL packages, procedures, and functions

| Package | Members | Purpose |
|---|---|---|
| `SIMPLE_HABITABILITY_PKG` | `IS_POTENTIALLY_HABITABLE(p_object_id) RETURN VARCHAR2`, `GET_HABITABILITY_INFO(p_object_id) RETURN CELESTIALOBJECTS%ROWTYPE`, private `HAS_ESSENTIAL_ELEMENTS` | Habitability check: `habitability_score >= 5.0 AND (oxygen='Y' OR water_vapor='Y') AND surface_temperature BETWEEN -50 AND 100`. Constant `G_MIN_HABITABLE_SCORE = 5.0`. Raises `ORA-20001`/`ORA-20002` on errors. |
| `MISSION_ANALYSIS_PKG` | `CALCULATE_MISSION_EFFICIENCY(p_mission_id) RETURN NUMBER`, `GET_MISSION_STATUS(p_mission_id) RETURN VARCHAR2`, `FIND_MISSIONS_BY_OBJECT(p_object_id) RETURN SYS_REFCURSOR`, private `CALCULATE_SUCCESS_RATE`, `GET_MISSION_DURATION` | Efficiency = 60% success rate + 30% data collected (capped at 100MB) + 10% duration bonus (< 365 days = full bonus), capped at 100. Status report is a formatted multi-line string. `FIND_MISSIONS_BY_OBJECT` returns a ref cursor of missions that observed a given object (via `Mission_Observations`/`Observations` join). |
| `CelestialManager` | `AddDefaultCelestialObjects` | Inserts 2 hardcoded celestial objects using `celestial_add_seq`, prints results via `DBMS_OUTPUT`. |
| `CelestialUpdater` | `UpdateLatestCelestialObject(p_new_name, p_new_category, p_new_score, p_new_gravity)` | Updates the most recently added celestial object, but **reassigns its primary key** to a new value from `celestial_update_seq` rather than updating in place. |
| `ResearchMatchManager` | `ShowMatchingResearchers` | Self-healing: creates `idx_missions_lead_date` if missing (catches `ORA-00955`), then prints researchers whose mission start date matches one of their observation dates. |
| `HabitableResearchAnalysis` | `ShowMultiAffiliatedResearchers` | Self-healing: creates `idx_r_id_affiliation` if missing, then prints researchers linked to more than one habitable-planet assessment where their affiliation_id > 1. |
| `SpaceResearchPackage` | 12 procedures: Add/Update/Delete for `CelestialObject`, `Event`, `Researcher`, `Mission` | Generic CRUD helpers taking the full column set as parameters; thin wrappers around INSERT/UPDATE/DELETE. |

## 8. Consolidated business rules

1. `observation_date` is always server-set to the insert moment (two triggers enforce this) — clients cannot backdate or future-date observations.
2. A mission with a non-null `end_date` is "completed" and can no longer receive new `Mission_Observations` rows.
3. A celestial object is "potentially habitable" only if `habitability_score >= 5.0`, has oxygen or water vapor, and surface temperature is between -50°C and 100°C — this is the single source of truth for habitability, not to be reimplemented independently.
4. `Habitable_Planets.recommended_population` is always 0 when `is_habitable = 'N'`, and never NULL (defaulted to 0 by trigger).
5. Mission efficiency is a weighted score (60/30/10 split) capped at 100, derived from `Mission_Observations` success/data rows and the mission's date range.
6. Deleting a `CelestialObject` cascades to delete its `Observations` (and, transitively, its `Mission_Observations` rows). Deleting a `Telescope`, `Researcher`, or (in `Missions`/`Researchers`) an `Affiliation` sets the referencing FK to NULL instead of blocking — except where the FK column is also `NOT NULL`, which is contradictory (see risks-and-assumptions.md).
7. Category, focus area, journal, event type/impact, and telescope type are all constrained to fixed vocabularies via CHECK constraints — these should become enums or constrained value sets in the API layer, not free text.
