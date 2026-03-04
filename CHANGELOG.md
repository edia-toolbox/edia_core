
## [Unreleased]

### Breaking Changes — UXF Dependency Removed
EDIA Core no longer depends on the `com.edia.uxf` package. All UXF functionality used by EDIA has been absorbed directly into the framework. This is a **breaking change** for existing XBlock code that references UXF types directly.

**Migration guide** (mechanical find-and-replace):

| Before | After |
|---|---|
| `using UXF;` | Remove (no replacement needed) |
| `Session.instance.CurrentTrial` | `Experiment.Instance.CurrentTrial` |
| `Session.instance.CurrentBlock` | `Experiment.Instance.CurrentBlock` |
| `Session.instance.CurrentTrial.result["x"]` | `Experiment.Instance.CurrentTrial.result["x"]` |
| `Session.instance.CurrentTrial.settings.GetString("key")` | `Experiment.Instance.CurrentTrial.settings.GetString("key")` |
| `UXFDataTable` | `Edia.Data.DataTable` |
| `UXFDataRow` | `Edia.Data.DataRow` |

The `Settings` API (GetString, GetFloat, GetBool, GetInt, etc.) remains identical — only the access path changes.

### Added
- `Edia.Data.Settings` — Cascading settings system (previously `UXF.Settings`). Same API, now with `SetParent()` instead of `ISettingsContainer`.
- `Edia.Data.DataTable` — Tabular data with CSV export (previously `UXF.UXFDataTable`).
- `Edia.Data.DataRow` — Named-column data row (previously `UXF.UXFDataRow`).
- `Edia.Data.ResultsDictionary` — Thread-safe results dictionary for trial data.
- `Edia.Data.BlockingQueue<T>` — Thread-safe producer-consumer queue for async I/O.
- `Edia.Block` — Block grouping class (previously `UXF.Block`). Plain C# class, not a MonoBehaviour.
- `Edia.Trial` — Trial lifecycle with worker thread pattern (previously `UXF.Trial`). Fires EDIA `EventManager` events instead of UXF UnityEvents.
- `Edia.IO.FileSaver` — Async file I/O with worker thread (previously `UXF.FileSaver`). Now a plain C# class (not MonoBehaviour), constructed by `Experiment.BeginSession()`.
- `Edia.Tracking.Tracker` — Abstract tracker base class (previously `UXF.Tracker`).
- `Edia.Tracking.PositionRotationTracker` — XR position/rotation tracker (previously `UXF.PositionRotationTracker`).
- `Experiment.Instance` now exposes session-level state: `blocks`, `settings`, `CurrentTrial`, `CurrentBlock`, `Trials`, `Headers`, `settingsToLog`, `customHeaders`, `trackedObjects`, `fileSaver`.
- `Experiment.Instance.CreateBlock()` — Creates a new block (previously `Session.instance.CreateBlock()`).
- `Experiment.Instance.BeginNextTrial()` / `EndCurrentTrial()` — Trial lifecycle management.
- `Experiment.Instance.SaveDataTable()` — Save arbitrary data tables.
- Unit test suite: 74 tests covering `Settings`, `DataTable`, `DataRow`, `ResultsDictionary`, and `BlockingQueue`.

### Changed
- `Experiment.cs` — Major refactor: absorbed all `UXF.Session` state and lifecycle methods. `Experiment.Instance` is now the single API for everything.
- `SessionGenerator.cs` — Retargeted from `Session.instance` to `Experiment.Instance`. Renamed `GenerateUxfSequence()` to `GenerateSequence()`.
- `SystemSettings.cs` — Removed `UXF.LocalFileDataHandler` references. Storage path is now managed by `Experiment.BeginSession()`.
- `XRManager.cs` — Updated tracker type references from `UXF.Tracker` / `UXF.PositionRotationTracker` to `Edia.Tracking.*`.
- `UXFSessionCheck.cs` — Renamed to `ExperimentSessionCheck`. Now inspects `Experiment` instead of `Session`. Menu item: `EDIA/Show session debugger`.
- `edia.core.Runtime.asmdef` — Removed UXF assembly reference.
- All sample XBlocks (`TaskStroop`, `TaskD2`, `Break`, `TaskStartersKitFinished`) updated to use `Experiment.Instance.*` instead of `Session.instance.*`.

### Removed
- `com.edia.uxf` package dependency. The UXF package is no longer required and can be removed from your project.
- All `using UXF;` references throughout the codebase.

## [0.4.0] - 12-04-2024
This release is based on the configs 2.0 approach and has all related framework logic to support that.

### Added
- New demotasks based on the Configs 2.0
- All task related calls towards the FW are going through `Experiment.instance.<methodname>`.
- Key entries starting with "_" are not logged into the trial_results.csv
- Fade to black funtionality hiding everything except the ` CamOverlay` layer (used for `MessagePanelInVR`)
- `MessagePanelInVR` component, which is collaborating with `Experiment.instance.ShowMessageToUser` 
- Helper component `XRControllerInputRemapper`
- Helper methods for String conversions
- `RayOverlayInteractor` for `MessagePanelInVR` interaction only (due to bug in Unity settings raycast layers via script)
- `Menu/eDIA/Configurator` panel with handy editor tools for setting up Unity editor correctly

### Changed
- Consistent class namings; Experiment, XRManager, ControlPanel, SessionGenerator, MessagePanelInVR
- Config File and folder name conventions [`folder-name` `file_name` ] (system auto lowercases all values on read)
- Main framework exists out of a `Controller` and a `Executer` part. These can be in the same Unity project for standalone, or be set up as separate applications using the `edia.RCAS module`.
- Dedicated class for generating session based on config files
- Configs 2.0; Task & Per participant optimized config file approach
- Xblock architecture; Meaning any type of block in any sequence = experiment
- Optional show/hide button on `MessagePanelInVR` to allow input from VR user.
- `Executer` needs to be set in `WaitOnProceed` mode in order to continue with `Proceed` call (from event or directly)
- Using Unity LTS version 2022.3.12

### Fixed
- `Controller` UI tweaks
- `Double clicks` on Proceed
- Correct status updates from `Executer` to `Controller`

### Removed
- RCAS references in code for independence
- HMD database (names & icons)
- Old config files and related code
- 


# [0.2.0]

- Pinch+Grip hand pose animation
- Systemsettings Custom logfile path, resolution.
- edia.eye edia.lsl
- System Settings implemented, stored on disk
- eDIA Control prefab + panels ‘experimentcontrol’ ‘messagebox’ ‘configselection’ ‘applicationsettings’
- Edia folder is now root eDIA framework package, including package.json
- Eyetracking extracted into separate package project
- Experimenter canvas PREFAB
- Exp canvas > trigger listening on ExperimentManager level
- Merged StereoFEM sprint code changes. Fixed framerate + Eyetracking + copy configs to build folder 
- Defined 'DocSRC' as docfx source and 'Documentation' folder as result (=website)
- Generated automated documentation with largely dummy content
- Pause <action> defined in `eDIA default input actions`
- Implemented URP
- XR interaction toolkit pre.0.8
- Updated README.md
- Pico package
- Tobii package
