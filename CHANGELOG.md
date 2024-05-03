
## [Unreleased]
-

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
