# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

<!-- Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
-->

## [Unreleased]

### Added

- Add static bool ServerSettingsModel.IsPrivateServer to allow clients to select the private LanguageForge

### Deprecated

- Chorus.FileTypeHandlers.lift.LiftUtils.LiftTimeFormatWithTimeZone (Use SIL.Extensions.DateTimeExtensions.ToLiftDateTimeFormat)

## [5.0.0] - 2021-10-28

### Fixed

- On Unauthorized Access of ChorusNotes, retry, then ignore; don't crash
- Use the correct password in request URL's
- Send/Receive settings dialog is now big enough to display its widgets without cutting them off

### Changed

- Speed up Send/Receive operations by caching hashes
- Update to the latest version of Palaso libraries (9.0.0)
- Use CrossPlatformSettingsProvider for settings (Requires migration to retain old settings; client's responsibility)
- When there is only one available Project, populate the Project ID combobox

## [4.0.0] - 2021-04-30

### Changed

- Create nuget packages

## [3.0.0] - non-nuget version

[Unreleased]: https://github.com/sillsdev/libpalaso/compare/v4.0.0...master

[4.0.0]: https://github.com/sillsdev/libpalaso/compare/v3.0.0...v4.0.0
