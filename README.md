# Simplified Cloud Compute Service (DEPRECATED)

[![No Maintenance Intended](http://unmaintained.tech/badge.svg)](http://unmaintained.tech/)

School project from Cloud Computing course in Applied Software Engineering department (Faculty of Technical Sciences Novi Sad).

## Table of Contents

- [Getting Started](#Getting-Started)
- [Client](#Client)
  - [Configuration](#Client-Configuration)
- [Common](#Common)
- [Compute](#Compute)
  - [Configuration](#Compute-Configuration)
  - [Expected Package Configuration](#Expected-Package-Configuration)
- [Container](#Container)
- [JobWorker](#JobWorker)
- [RoleEnvironmentLibrary](#RoleEnvironmentLibrary)
- [_JobWorkerDllsForTesting](#_JobWorkerDllsForTesting)

## Getting Started

To start application:

1. Copy files from `./_JobWorkerDllsForTesting/` into `./_PlacePackageDllsHere/` folder (Check [Compute Configuration](#Compute-Configuration) section if you want to change path)
1. Build solution
1. (Optional) Set solution to multiple startup projects ( *Right Click Solution > Properties > Multiple Startup Perojects*) where Compute project will be loaded first and then Client
1. Run solution

> This project is made using .NET Framework 4.7.2

Note: It is important that you **build entire solution** first instead of starting `Compute` right away because it relies on `Container`'s `.exe` file generated when you build solution.

## Client

### Client Configuration

Expected `address` for `Common.IComputeManagement`'s endpoint to match the address of [Compute](#Compute)'s WCF Service that is implementing the `Common.IComputeManagement` interface

```xml
<system.serviceModel>
  <client>
    <endpoint name="Common.IComputeManagement"
              address ="net.tcp://localhost:10200/IComputeManagement"
              binding="netTcpBinding"
              contract="Common.IComputeManagement" />
  </client>
</system.serviceModel>
```

## Common

Holds the interfaces that are used through the solution for WCF Communication.

## Compute

### Compute Configuration

`appSettings` holds static values that are used loaded and stored inside `ComputeConfiguration` singleton class and used through the program.

Values of interest for change:

- `PackageRelativeFolderPath` - relative path to the folder in which the `.xml` with its `.dll` pair (and it's dependencies) are expected.

- `PackageTempRelativeFolderPath` - relative path to the folder which will hold `NumberOfContainersToStart` copies of `.dll`s with it's dependencies from `PackageRelativeFolderPath`. (Names of these copies will are assigned at runtime)

- `PackageConfigFileName` - name of package configuration (.xml) file that is expected to be located inside `PackageRelativeFolderPath`

```xml
<appSettings>
    <add key="ContainerRelativeFilePath" value="..\..\..\Container\Bin\Debug\Container.exe" />
    <add key="MaxPort" value="10050" />
    <add key="MinPort" value="10010" />
    <add key="NumberOfContainersToStart" value="4" />
    <add key="PackageConfigFileName" value="JobWorker.xml" />
    <add key="PackageRelativeFolderPath" value="..\..\..\_PlacePackageDllsHere\" />
    <add key="PackageTempRelativeFolderPath" value="..\..\..\_PlacePackageDllsHere\Temp\" />
  </appSettings>
```

`system.serviceModel` defines endpoints for WCF Services that `Compute` provides. It is advised not to modify these since `RoleEnvironmentLibrary` as well as `Client` rely on them.

```xml
<system.serviceModel>
  <services>
    <service name="Compute.RoleEnvironment">
      <endpoint address="" binding="netTcpBinding" contract="Common.IRoleEnvironment" />
      <host>
        <baseAddresses>
          <add baseAddress="net.tcp://localhost:10100/IRoleEnvironment" />
        </baseAddresses>
      </host>
    </service>
    <service name="Compute.ComputeManagement">
      <endpoint address="" binding="netTcpBinding" contract="Common.IComputeManagement" />
      <host>
        <baseAddresses>
          <add baseAddress="net.tcp://localhost:10200/IComputeManagement" />
        </baseAddresses>
      </host>
    </service>
  </services>
</system.serviceModel>
```

### Expected Package Configuration

While reading a package `Compute` will expect `PackageConfigFileName` file located in `PackageRelativeFolderPath` to have following structure:

```xml
<?xml version="1.0"?>
<doc>
    <assembly>
        <name>JobWorker</name>
    </assembly>
    <numberOfInstances value="2"/>
</doc>
```

Where `<assembly><name>` specifies the name of the assembly (worker role), and `<numberOfInstances value="">` defines the number of `Containers` that `Compute` will call `Load` method to.

> If the `.xml` configuration is invalid (ex. `numberOfInstances`'s `value` is bigger then `NumberOfContainersToStart`, `.xml` is in incorrect format, or `name`.dll (ex. `JobWorker`.dll) from `<assembly><name>` does not exist), then `Compute` will delete all files and folders located inside `PackageRelativeFolderPath`.

## Container

Console application that implements `IContainerManagement` interface and provides two methods as WCF Service:

- `Load(string assemblyName)`
  
  - Checks if the assembly on provided path implements interface `IWorker`
  - Calls `Start` method from loaded .dll

- `CheckHealth()`
  - Returns `'Healthy'`.

## JobWorker

Example of valid implementation of `.dll` that `Container` would load.

## RoleEnvironmentLibrary

Contains client version of `RoleEnvironment` class that is meant for `JobWorker` to use. It connects to `Common`'s WCF service to acquire information about current and brother role instances.

## _JobWorkerDllsForTesting

A folder that holds valid implemented package (`.xml` and `.dll`) with its dependencies. For testing purpose it is advised to copy these files into the `PackageRelativeFolderPath` specified in [Compute Configuration](#Compute-Configuration) 
