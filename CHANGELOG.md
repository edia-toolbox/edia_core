# Changelog
***
> High level changes overview for the eDIA framework 

## [unreleased] 
#### Added
- Pinch+Grip hand pose animation&poses
- Systemsettings Custom logfile path
- System Settings implemented and stored as `settings.json` file
- Control panel experiment progress updates + minor upgrades
- Samples/Templates/ to package

#### Changed
- Approach of `tasks` => `taskblocks`
- Several package dependencies -> updated in `package.json`
- `MessagepanelInVR` singleton as part of the basic XR Rig
- XR Rig overhauled to new version
- Switched to **snake case** convention for keys that are exposed to user 
- Moved all helper classes to `HelperClasses.cs`
- Made `Experiment.cs` the main class
- Updated auto documentation

#### Removed
- `TaskManager.cs` 


## [0.2.0] - 2022-03-29
#### Added
- edia.eye edia.lsl
- eDIA Control prefab + panels ‘experimentcontrol’ ‘messagebox’ ‘configselection’ ‘applicationsettings’
- Experimenter canvas PREFAB
- Generated automated documentation with largely dummy content
- Pause <action> defined in `eDIA default input actions`
- Implemented URP
- XR interaction toolkit pre.0.8

#### Changed
- Eyetracking extracted into separate package project
- Exp canvas > trigger listening on ExperimentManager level
- Merged StereoFEM sprint code changes. Fixed framerate + Eyetracking + copy configs to build folder
- Defined 'DocSRC' as docfx source and 'Documentation' folder as result (=website)

#### Removed
- Pico package
- Tobii package

## [0.1.0] - 2021-10-27
#### Changed
- Updated [readme](README.md)

