# Changelog

## Unreleased

### Added
- Skeleton linter. Warns about problems and auto fixes some issues
- Warning: Overlapping colliders
- Autofix: Overlapping colliders. May produce non symmetric creatures
- Autofix: Bones with identical positions
- Pairwise iterators for Skeleton

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