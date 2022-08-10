# Changelog

## Unreleased

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