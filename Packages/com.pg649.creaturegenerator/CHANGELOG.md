# Changelog


## Unreleased
### Added
- Default joint limits can now be overridden using an optional JointLimitOverrides object (see wiki https://github.com/PG649-3D-RPG/Creature-Generation/wiki/Joint-Limit-Overrides)
- Parameter for number of torso segments
- Parameter for number of leg segments (only quadruped, must be between 1 and 4)

### Fixed
- Quadruped's feet rotation not locked anymore (added paw subcategory to differentiate)

### Changed
- Added FalloffFunction class for easy customization of metaball functions

## [3.6.0] - 2022-10-12
### Changed
- Moved quadruped legs slightly down

## [3.5.0] - 2022-09-21
### Changed
- Set category of lowest quadruped leg part to foot
- Offset root bone of biped and quadruped so that creature is standing on ground (height of base transform)
- Ordering of skeleton observations is now fixed

### Fixed
- Quadruped feet are on same level by default
- Seed of quadruped creature is now nullable

## [3.4.0] - 2022-08-31
### Added
- ISettingsInstance field to Skeleton. Exposes raw skeleton data as list of floats.

## [3.3.1] - 2022-08-24
### Fixed
- Broken generator seeding

## [3.3.0] - 2022-08-24
### Added
- Debug setting to make all bones kinematic
- Made bone densities configurable. Allows for both a default setting and per bone category overrides
- Extra logging to CreatureGenerator
- Skeleton linter. Warns about problems and auto fixes some issues
- Warning: Overlapping colliders
- Autofix: Overlapping colliders. May produce non symmetric creatures
- Autofix: Bones with identical positions
- Pairwise iterators for Skeleton

### Fixed
- Reversed biped leg angle limits

### Removed
- Broken samples

## [3.2.0] - 2022-08-11
### Added
- Joint angle limits for quadruped creatures
- MeshGenerator determines its bounding box by itself

### Changed
- CreatureGenerator now creates an empty GameObject above the root bone

### Fixed
- Removed unneeded import that broke project build

## [3.1.0] - 2022-08-09
### Added
- Tooltips to most fields in CreatureGeneratorSettings and ParametricCreatureSettings
- Quadruped creatures are rotated, so that their feet are level

### Changed
- Change field names in ParametricCreatureSettings to better reflect intended meaning

### Fixed
- Limbs are not attached correctly, even if parent bone radius has been clamped
- Quadruped creatures no longer explode once they hit the ground
- Settings objects can be initialized via ScriptableObject.CreateInstance without throwing a NPE