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

- [SIL.Chorus.LibChorus] Add webm as additional audio file type
- [SIL.Chorus] Add ability to clone project without direct user interaction

### Changed

- Use UTF-8 in conflict details view
- [SIL.Chorus.LibChorus] Add ChorusStorage (the bundle cache) as an Excluded folder
- [SIL.Chorus.LibChorus] Changed HgResumeTransport LastKnownCommonBases to use Json serialization instead of BinaryFormatter
- Update SIL.Chorus.Mercurial dependency to version 6.5.1 which uses for Python 3

### Fixed

- Prevent S&R to Internet without full URL

## [5.1.0] - 2023-03-07

### Changed

- [SIL.Chorus.ChorusMerge] Additionally build with .net 6
- [SIL.Chorus.LibChorus] Add netstandard 2.0

### Deprecated

- Chorus.FileTypeHandlers.lift.LiftUtils.LiftTimeFormatWithTimeZone (Use SIL.Extensions.DateTimeExtensions.ToLiftDateTimeFormat)

## [5.0.0] - 2022-09-13

### Added

- Add static bool ServerSettingsModel.IsPrivateServer to allow clients to select the private LanguageForge

### Fixed

- On Unauthorized Access of ChorusNotes, retry, then ignore; don't crash
- Use the correct password in request URL's
- Send/Receive settings dialog is now big enough to display its widgets without cutting them off

### Changed

- Speed up Send/Receive operations by caching hashes
- Update to the latest version of Palaso libraries (10.0.0)
- Use CrossPlatformSettingsProvider for settings (Requires migration to retain old settings; client's responsibility)
- When there is only one available Project, populate the Project ID combobox
- Update SIL.Chorus.Mercurial dependency to the latest version which looks for python2

## [4.0.0] - 2021-04-30

### Changed

- Create nuget packages

## [3.0.0] - non-nuget version

[Unreleased]: https://github.com/sillsdev/libpalaso/compare/v5.1.0...master

[5.1.0]: https://github.com/sillsdev/libpalaso/compare/v5.0.0...v5.1.0
[5.0.0]: https://github.com/sillsdev/libpalaso/compare/v4.0.0...v5.0.0
[4.0.0]: https://github.com/sillsdev/libpalaso/compare/v3.0.0...v4.0.0
