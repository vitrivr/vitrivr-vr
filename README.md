# vitrivr-VR

This repository contains the source code for vitrivr-VR, a Unity Engine based VR interface for multimedia retrieval using the Cineast retrieval engine.

## VR Input

Due to the currently rapidly changing landscape VR/XR input libraries and interfaces this project uses a combination of custom, stable and preview packages and plugins to facilitate VR input and interaction.
To enable a smooth transition to a stable solution, an attempt is made to separate input logic from interaction logic wherever possible.

Currently, the following library/plugin setup is used:

- **Unity OpenXR Plugin** for VR input from any OpenXR compliant backend
- [**Unity XR Input**](https://docs.unity3d.com/Manual/xr_input.html) for device-based input through the vitrivr-VR [XRButtonObserver](Assets/Scripts/VitrivrVR/Input/Controller/XRButtonObserver.cs) (transition to new input system bindings through OpenXR soon)
- **Unity XR Interaction Toolkit Pre-release** for basic interaction (using input action based input through OpenXR)
- **Custom Interaction System** for direct interaction consisting of [Interactors](Assets/Scripts/VitrivrVR/Interaction/System/Interactor.cs) and [Interactables](Assets/Scripts/VitrivrVR/Interaction/System/Interactable.cs)

### Contributing

Basic interactions such as interacting with UI canvases should be (as far as possible) left to the XR Interaction Toolkit and any additional functionality should be implemented as generically and interaction system agnostic as possible.

Raw device input used to be gathered within the [XRButtonObserver](Assets/Scripts/VitrivrVR/Input/Controller/XRButtonObserver.cs) and dynamically connected to components using the provided Unity Events.
These will be transitioned to OpenXR based input actions soon and so should no longer be used.

## System Structure & Data Flow

For increased flexibility, vitrivr-VR is structured to allow easy switching of individual components.

### Control Flow

At the core of vitrivr-VR is the [QueryController](Assets/Scripts/VitrivrVR/Query/QueryController.cs), which sends a query to [Cineast](https://github.com/vitrivr/cineast) when the asynchronous function `RunQuery` is invoked.
An already instantiated [QueryTermProvider](Assets/Scripts/VitrivrVR/Query/Term/QueryTermProvider.cs) is required to provide the query terms for the query.
Once the query results arrive, the QueryController will instantiate the provided type of [QueryDisplay](Assets/Scripts/VitrivrVR/Query/Display/QueryDisplay.cs) with the scored result list.
The QueryDisplay will then instantiate the results in the form of [MediaItemDisplays](Assets/Scripts/VitrivrVR/Media/Display/MediaItemDisplay.cs).
Ultimately, individual MediaItemDisplays should also provide functionality for a detailed media view, but this has not yet been formalized into an interface.

Once a new query is launched or the current query should be cleared, the QueryController initiates the required changes in the scene.

### Component Responsibilities

- **QueryController:** Sends queries to Cineast, instantiates QueryDisplays from query results and manages QueryDisplay instances.
- **QueryTermProvider:** Provides the QueryController with query terms and UI components (or the user directly) with methods to specify these query terms.
- **QueryDisplay:** Instantiates and arranges MediaItemDisplays in 3D space. May (or should) provide functionality to explore / browse query results.
- **MediaItemDisplay:** Displays and allows detailed inspection of a scored media item.
