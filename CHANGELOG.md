
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

### Added

- Add git-based build variables

### Fixed

- Implement WebGL target path construction

## 1.2.1 - 2025-01-28

### Changed

- Add "Compact" changelog mode #2

### Security

- WebGL target path construction not implemented

## 1.2.0 - 2025-01-27

### Changed

- Remove custom Smart format, use com.unity.localization instead

### Security

- WebGL target path construction not implemented

## 1.1.5 - 2021-02-19

### Fixed

- Fix

### Security

- WebGL target path construction not implemented

## 1.1.4 - 2021-02-19 [YANKED]

### Fixed

- Fix

### Security

- WebGL target path construction not implemented

## 1.1.3 - 2021-02-19

### Fixed

- Delete unsupported symbols from path

### Security

- WebGL target path construction not implemented

## 1.1.2 - 2020-07-19 [YANKED]

### Changed

- Higher reflection speed

### Fixed

- Fix adding parameter instead of key

### Security

- WebGL target path construction not implemented

## 1.1.1 - 2020-07-18

### Added

- Add ability to write custom build path parameters

### Changed

- Change build path parameters collection using BuildPathAttribute

### Security

- WebGL target path construction not implemented

## 1.0.12 - 2020-07-16

### Fixed

- Implement Android target path construction
- Implement iOS target path construction

### Security

- WebGL target path construction not implemented

## 1.0.11 - 2020-07-14

### Added

- Insert new release version if there's any unreleased

### Security

- Android target path construction not implemented
- iOS target path construction not implemented
- WebGL target path construction not implemented

## 1.0.10 - 2020-07-08

### Added

- Add REMOVED note type
- Add YANKED to version
- Add unreleased versions
- Add DEPRECATED note type
- Save to CHANGELOG.md

### Security

- Android target path construction not implemented
- iOS target path construction not implemented
- WebGL target path construction not implemented

## 1.0.9 - 2020-07-04

### Fixed

- Fix smartFormat error while parsing uncommon symbols

## 1.0.8 - 2020-07-03

### Added

- Build path editor

### Changed

- Sort notes after adding new one
- Add trismegistus.unity.smartformat dependency

### Fixed

- Clone Known Issues from last version instead of copying references
- Handle package browsing empty path

## 1.0.7 - 2020-06-16

## 1.0.6 - 2020-06-15

### Added

- Note type buttons

## 1.0.5 - 2020-06-15

### Added

- Update version in package file

### Changed

- Colored category labels

## 1.0.4 - 2020-06-15

### Changed

- Update readme

## 1.0.3 - 2020-06-12

### Changed

- Build location with version

### Fixed

- Release notes build location

## 1.0.2 - 2020-06-12

### Added

- Copy known issues from last version

## 1.0.1 - 2020-06-08

### Changed

- Load build info from Asset if Xml is deleted
- Move to Packages

## 1.0.0 - 2020-06-06

### Changed

- Obtained

## 0.0.5 - 2013-05-15

### Added

- Added Export button for saving release notes to disk separately

### Changed

- XML is now serialized w/ indents, so that it is easier to merge
- Latest version's date/time is updated automatically when building
- Save now calls AssetDatabase.SaveAssets() so that _BuildPlus.asset is saved

### Fixed

- Fixed compilation warning in BuildPlusEditor
- Fixed release notes not writing correctly for iOS builds

## 0.0.4 - 2012-06-26

### Changed

- Added ReadMe.txt and ReleaseNotes.txt for Asset Store submission

### Fixed

- Release notes weren't be written to txt file correctly

## 0.0.3 - 2012-06-22

### Added

- ReleaseNotes are now saved into the serialized version

### Changed

- CurrentVersion is now expanded by default

### Fixed

- Reload issue in the editor w/ dates

## 0.0.2 - 2012-06-22

### Changed

- Cleaned up release notes output

## 0.0.1 - 2012-06-22

### Added

- Initial version
