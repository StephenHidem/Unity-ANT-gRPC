# Unity ANT gRPC Example
This project is a simple example of integrating the AntPlus libraries and hosting
extensions in a Unity game.
## Overview
## Details
### Basic Setup for Unity Projects
1. Create a Unity project
2. Open Unity Package Manager window and install NuGetForUnity. See instructions [here][NuGetForUnity].
3. Open Unity Package Manager window and install YetAnotherHttpHandler. See instructions [here][YetAnotherHttpHandler].
4. Select **_Manage NuGet Packages_** from the NuGet menu and install the following packages -
    - Google.Protobuf 
    - Grpc.Client.Net 
    - SmallEarthTech.AntPlus.Extensions.Hosting 
    - System.Runtime.CompilerServices.Unsafe
    - System.IO.Pipelines
5. Add AntGrpcShared.dll (debug or release build) from the cloned AntPlus solution to the **_Assets_** folder or a subfolder in the **_Project_** window.

That completes the initial steps required.
## Links
- [ANT+ Class Library Help](http://stephenhidem.github.io/AntPlus)

[NuGetForUnity]: https://github.com/GlitchEnzo/NuGetForUnity
[YetAnotherHttpHandler]: https://github.com/Cysharp/YetAnotherHttpHandler
