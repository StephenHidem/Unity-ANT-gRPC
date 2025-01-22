<p align="center">
  <img src="./SmallEarthTech.png" />
</p>

# Unity ANT gRPC Example
This project is a simple example of integrating the AntPlus libraries and hosting
extensions in a Unity game.
## Overview
The UI consists of ANT radio server info, a ListView to display ANT devices in the AntCollection, and
a details view that displays the selected ANT device image and device number.

The example uses dependency injection as illustrated in AntPlus.cs. The Microsoft host builder adds a
logger for the Windows event system, which is not supported by Unity; so logging providers are cleared
and the custom logger UnityLogger.cs is added to the container. The SmallEarthTech.AntPlus.Extensions.Hosting
extension are added to the container by invoking the UseAntPlus() static method.

AntGrpcShared.dll is a .NET library that provides the gRPC service for the ANT+ network. The AntRadioService
class is a singleton that manages the ANT radio service. It uses the AntChannelService to provide supporting
services for the ANT network.

You will need to clone the VS2022 [AntPlus] solution and build the solution in order to run this example. This
solution provides the AntGrpcShared.dll necessary for gRPC support and the AntGrpcService console app/windows
service that acts as server on the network.

The AntGrpcService uses commercially available ANT USB sticks for ANT networks. I use two for testing; one to
support the service and the second to run simulated ANT devices. The service needs to be running so the Unity
example can communicate with ANT devices.
### Basic Setup for Unity Projects
You can start with these steps if you are not using this cloned solution from github.
1. Create a Unity project
2. Open Unity Package Manager window and install NuGetForUnity. See instructions [here][NuGetForUnity].
3. Open Unity Package Manager window and install YetAnotherHttpHandler. See instructions [here][YetAnotherHttpHandler].
4. Select **_Manage NuGet Packages_** from the NuGet menu and install the following packages -
    - Google.Protobuf 
    - Grpc.Client.Net 
    - SmallEarthTech.AntPlus.Extensions.Hosting 
    - System.Runtime.CompilerServices.Unsafe
    - System.IO.Pipelines
5. Copy AntGrpcShared.dll (netstandard2.1\debug or netstandard2.1\release build) from the cloned AntPlus solution to the **_Assets_** folder or a subfolder in the **_Project_** window.
6. Implement a MonoBehavior that supports dependency injection similar what I've done in AntPlus.cs.

That completes the initial steps required.
## Links
- [ANT+ Class Library Repository][AntPlus]
- [ANT+ Class Library Help](http://stephenhidem.github.io/AntPlus)

[NuGetForUnity]: https://github.com/GlitchEnzo/NuGetForUnity
[YetAnotherHttpHandler]: https://github.com/Cysharp/YetAnotherHttpHandler
[AntPlus]: https://github.com/StephenHidem/AntPlus