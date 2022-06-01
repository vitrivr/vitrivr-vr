# vitrivr-VR
This repository contains the source code for vitrivr-VR, a Unity Engine based VR interface for multimedia retrieval using the Cineast retrieval engine.

| Speech-to-text | Precision Keyboard | Immersive Results View | Intuitive Results Exploration |
| --- | --- | --- | --- |
| ![vitrivr-VR-speech-to-text](https://user-images.githubusercontent.com/9721543/154439403-07f6edab-33b7-41ea-8741-3fcb86ca77e4.gif) | ![vitrivr-VR-type](https://user-images.githubusercontent.com/9721543/154436198-0ed0a346-1cec-4e0b-b983-f9fc5639ff08.gif) | ![vitrivr-VR-view-results](https://user-images.githubusercontent.com/9721543/154439439-20fa8a17-4b6d-4110-bbb3-021ff4fee482.gif) | ![vitrivr-VR-drawer](https://user-images.githubusercontent.com/9721543/154434477-5397c3f5-5d2a-4874-84d8-1f1ea62b2d43.gif) |

## Setup
Setup is very easy and should not involve much more than having a working OpenXR runtime and a compatible version of the Unity engine installed.
There are a few things to be aware of:
- **Errors on first import:** During first import there may be errors because Unity incorrectly loads the different versions of certain libraries included in itself and different packages. Simply close the editor and reopen the project to fix this issue.
- **MapBox:** To use the map query formulation method using MapBox, follow the MapBox popup instructions to acquire an API-key. If you do not intend to use the map this step is not required.
- **DeepSpeech:** To use the DeepSpeech speech-to-text functionality, follow the instructions on the [DeepSpeech UPM](https://github.com/Spiess/deep-speech-upm) repository to download and correctly place the required model file.

## Usage

### Configuration
vitrivr-VR relies on an instance of [Cineast](https://github.com/vitrivr/cineast) for feature transformation and retrieval.
To configure this connection, create a JSON configuration file at `Assets/cineastapi.json`.
A documentation of the parameters is available in the [Cineast Unity Interface package](https://github.com/vitrivr/CineastUnityInterface/blob/master/Runtime/Vitrivr/UnityInterface/CineastApi/Model/Config/CineastConfig.cs).

Configuration of vitrivr-VR itself is done through `Assets/vitrivr-vr.json`, which is documented in the [respective class](Assets/Scripts/VitrivrVR/Config/VitrivrVrConfig.cs).

### Interaction
All UI objects (other than the cylindrical results displays) can be grabbed and moved using the grip button on a standard XR controller.
Other interactions typically use the trigger on a standard XR controller.
To open the settings menu within vitrivr-VR use the menu button (small button above trackpad on VIVE wands) on the left-hand XR controller.
To use speech-to-text, have a text-field selected and press and hold the menu button on the right-hand XR controller.

## VR Input
Due to the still rapidly evolving landscape of OpenXR plugins, libraries and backends, this project attempts to separate input logic from interaction logic wherever possible.

Currently, the following input and interaction setup is used:

- **Unity OpenXR Plugin** for VR input from any OpenXR compliant backend
- **Custom Interaction System** for direct interaction consisting of [Interactors](Assets/Scripts/VitrivrVR/Interaction/System/Interactor.cs) and [Interactables](Assets/Scripts/VitrivrVR/Interaction/System/Interactable.cs)

### Contributing
Basic interactions should be implemented with the custom interaction system, conventional 2D UI interactions through the Unity UI.

Raw device input should be implemented using input actions from the new input system.

## System Structure & Data Flow
For increased flexibility, vitrivr-VR is structured to allow easy switching of individual components.

### Control Flow
At the core of vitrivr-VR is the [QueryController](Assets/Scripts/VitrivrVR/Query/QueryController.cs), which sends a query to [Cineast](https://github.com/vitrivr/cineast) when the asynchronous function `RunQuery` is invoked.
[QueryTermProvider](Assets/Scripts/VitrivrVR/Query/Term/QueryTermProvider.cs)s are required to provide the query terms for the query.
Once the query results arrive, the QueryController will instantiate the provided type of [QueryDisplay](Assets/Scripts/VitrivrVR/Query/Display/QueryDisplay.cs) with the scored result list.
The QueryDisplay will then instantiate the results in the form of [MediaItemDisplays](Assets/Scripts/VitrivrVR/Media/Display/MediaItemDisplay.cs).
Ultimately, individual MediaItemDisplays should also provide functionality for a detailed media view, but this has not yet been formalized into an interface.

Once a new query is started or the current query should be cleared, the QueryController initiates the required changes in the scene.

### Component Responsibilities
- **QueryController:** Sends queries to Cineast, instantiates QueryDisplays from query results and manages QueryDisplay instances.
- **QueryTermProvider:** Provides the QueryController with query terms and UI components (or the user directly) with methods to specify these query terms.
- **QueryDisplay:** Instantiates and arranges MediaItemDisplays in 3D space. May (or should) provide functionality to explore / browse query results.
- **MediaItemDisplay:** Displays and allows detailed inspection of a scored media item.
