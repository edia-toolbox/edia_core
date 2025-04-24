<p align="center">
  <img src="Media/Logos/EDIA_header_gray.png">
</p>

# EDIA – A Unity XR Toolbox for Research

`EDIA` provides you with a set of modules (Unity packages) to facilitate the design and conduction of experimental research in XR using Unity.  

`EDIA` is heavily inspired by and based upon [UXF — Unity Experiment Framework](https://github.com/immersivecognition/unity-experiment-framework/) (Brookes et al., [2020](https://link.springer.com/article/10.3758/s13428-019-01242-0)). 

## Core features of `EDIA`:
:diamond_with_a_dot: Structure your experiment on multiple levels.  
:card_index_dividers: Manage it using Config Files  
:eye_in_speech_bubble: Unified eye tracking integration  
:mobile_phone_with_arrow: Remotly control mobile XR experiments  
:pencil: Automatically log relevant data  
:on_arrow: Synchronize with external data  

# `EDIA` is for you if ...
- you want to build experiments which use different tasks in the same experimental session.
- you need high temporal precision to, for example, synchronize your experiment with EEG.
- you want an easy intgration of eye tracking in your experiment.
- you want to use configuration files (JSON) to manage your experiment. 
- ...

# Getting started
For newcomers to EDIA, we recommend to follow our "[Getting started](https://mind-body-emotion.notion.site/EDIA-Core-Tutorial-Website-1ca03dd4773f80eb87c9f5f0806f4ece)" guide. This also has more detailled instructions on how to install `EDIA` as a package and how to set up your Unity Editor. 


# INSTALLATION

### As a package (Unity package manager)
- In Unity open the package manager window -> **Window** -> **Package manager**
- Use `install from GIT url` 
- Enter the URL: [git@gitlab.gwdg.de:edia/edia_core.git?path=Assets/com.edia.core](git@gitlab.gwdg.de:edia/edia_core.git?path=Assets/com.edia.core)  
(The path parameter is used to point directly to the package files.)
- Hit `ADD`

Unity now starts to download and install the `com.edia.core` package. 

### For development
Clone this repository. 


## ThirdParty

The following assets and packages are incorporated in edia.core:

- [UXF framework](https://github.com/immersivecognition/unity-experiment-framework)

## Documentation
[HTML API REFERENCE](https://gitlab.gwdg.de/edia/edia_core/-/tree/fix/%2328/doxyGenUpdate/Assets/com.edia.core/Documentation~/index.html)
