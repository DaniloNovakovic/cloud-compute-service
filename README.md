# Simplified Cloud Compute Service 

- a school project from Cloud Computing course in Applied Software Engineering department (Faculty of Technical Sciences Novi Sad).

## Table of Contents

- [Client](#Client)
  - [Configuration](#Client-Configuration)
- [Common](#Common)
- [Compute](#Compute)
  - [Configuration](#Compute-Configuration)
- [Container](#Container)
- [JobWorker](#JobWorker)
- [RoleEnvironmentLibrary](#RoleEnvironmentLibrary)
- [_JobWorkerDllsForTesting](#_JobWorkerDllsForTesting)

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

```xml
<appSettings>
  <add key="ContainerRelativeFilePath" value="..\..\..\Container\Bin\Debug\Container.exe" />
  <add key="MaxPort" value="10050" />
  <add key="MinPort" value="10010" />
  <add key="NumberOfContainersToStart" value="4" />
  <add key="PackageConfigFileName" value="JobWorker.xml" />
  <add key="PackageRelativeFolderPath" value="..\..\..\packages\_JobWorker\" />
  <add key="PackageTempRelativeFolderPath" value="..\..\..\packages\_JobWorker\Temp\" />
  <add key="PackageAcquisitionIntervalMilliseconds" value="2000" />
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

## Container

Console application that implements `IContainerManagement` interface and provides two methods as WCF Service:

- `Load(string assemblyName)`
  
  - Checks if the assembly on provided path implements interface `IWorker`
  - Calls `Start` method from loaded .dll

- `CheckHealth()`
  - Returns `'Healthy'`.
