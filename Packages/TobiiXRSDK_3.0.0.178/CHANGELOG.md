Changes for 3.0.0

* Required Unity version changed from 2018.2 to 2019.4
* Removed support for legacy VR and now requires Unity XR Management package
* Added support for HP Reverb G2 Omnicept Edition
* Changed release format from an Asset Package (.unitypackage) to a Unity Package Manager (UPM) release
* Added Pico XR provider to support Pico XR SDK 1.22
* Removed Pico VR provider
* Changed Vive Provider to be reflection-based
* Samples are now imported using Unity Package Manager
* Social sample rewritten to use avatar from ReadyPlayerMe
* Ensured materials and shaders can be upgraded to work in URP

Changes for 2.1.1

* Fixed a bug where Tobii provider stopped processing data after a reconnect

Changes for 2.1.0

* Tobii Provider now uses a background thread to process new data to avoid blocking main thread
* Pico provider updated for PicoVR SDK 2.8.8. This change breaks compatibility with earlier versions of PicoVR SDK.

Changes for 2.0.0

* Mouse provider no longer creates an extra camera
* Added a new advanced API that requires an Ocumen license to access
* TobiiXR Updater is now DontDestroyOnLoad instead of being recreated when needed