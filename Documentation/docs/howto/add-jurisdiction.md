How to add a jurisdiction
=========================

<!-- @import "[TOC]" {cmd="toc" depthFrom=1 depthTo=6 orderedList=false} -->

<!-- code_chunk_output -->

- [Overview](#overview)
- [Step 1: Create a Jurisdiction Folder](#step-1-create-a-jurisdiction-folder)
- [Step 2: Create Jurisdiction Addin](#step-2-create-jurisdiction-addin)
- [Step 3: Add Jurisdiction Content](#step-3-add-jurisdiction-content)
  - [Hardware Config](#hardware-config)
  - [Operator Menu Config](#operator-menu-config)
  - [Tilt Logger Config](#tilt-logger-config)
  - [Lobby Config](#lobby-config)
  - [Sound](#sound)

<!-- /code_chunk_output -->

## Overview

This document describes how to add a new jurisdiction to the Monaco
platform to vary platform behavior and configurable limits and other
data values.  This document does not describe specific jurisdictions or
their regulations.

## Step 1: Create a Jurisdiction Folder

Configuration for all jurisdictions resides in the source tree at the
following location:

> \<SolutionDir\>\Jurisdiction\\\<Jurisdiction\>\\

There is a subdirectory at the above path for each jurisdiction.  The
jurisdiction name should be specific enough to identify the unique
jurisdiction.  For example, don’t use just “Oklahoma”, as that could be
either class 2 or class 3 which are very different jurisdictions.

Create a subdirectory for the new jurisdiction.

## Step 2: Create Jurisdiction Addin

Inside each jurisdiction folder there must reside a file with the
extension *.addin.xml* that defines a Mono.Addins addin for the
jurisdiction.  Refer to the document **MonacoAddinsConfiguration.docx**
for more information on Mono.Addins and how jurisdictions are
supported.  By convention, the addin file is named
**Jurisdiction.addin.xml**.  Create this file or copy it from another
jurisdiction’s folder.  At a minimum, the file will look like the
following sample where the highlighted items should be named after the
jurisdiction.

As you will see, the configuration calls out separate configuration
files as extensions.  We have a user story to replace this with a single
jurisdiction configuration file extension path where these other config
file paths would be listed within the jurisdiction configuration file. 
This will eliminate the need for everything to be an extension.  While
swapping out implementation can be handled through extension point
filtering, varying simple data values (e.g. meter limits, file paths,
etc.) can be handled through the jurisdiction configuration file.

**Jurisdiction Example**
```xml
<Addin id="QuebecJurisdiction" namespace="Client12Addins"version="1.0"/>
  <Dependencies/>
    <Addin id="Monaco.Kernel" version="1.0" />
    <Addin id="Monaco.Application" version="1.0" />
    <Addin id="Monaco.Application.UI" version="1.0" />
    <Addin id="Monaco.Hardware.Discovery" version="1.0" />
    <Addin id="SelectableJurisdiction" version="1.0" />
  </Dependencies/>

  <Runtime>
  <Import assembly="..\\..\\..\\Aristocrat.Monaco.Kernel.dll" />
  </Runtime/>

  <Extension path="/Kernel/SelectableAddinConfiguration/Jurisdiction"/>
    <SelectableJurisdiction name="Quebec VLT" description="Top level Quebec jurisdiction group">
      <AddinConfigurationGroupReference name="BaseWpfOperatorMenus"/>
      <AddinConfigurationGroupReference name="BaseConfigWizardPages"/>
      <AddinConfigurationGroupReference name="BaseLoggerPage"/>

      <ExtensionPointConfiguration extensionPath="/Platform/Discovery/Configuration">
        <NodeSpecification addinId="Client12Addins.QuebecJurisdiction" filterid="QuebecHardwareConfig" />
      </ExtensionPointConfiguration>

      <ExtensionPointConfiguration extensionPath="/TiltLogger/Configuration">
        <NodeSpecification addinId="Client12Addins.QuebecJurisdiction" filterid="QuebecTiltLoggerConfig" />
      </ExtensionPointConfiguration>

      <ExtensionPointConfiguration extensionPath="/OperatorMenu/Configuration">
        <NodeSpecification addinId="Client12Addins.QuebecJurisdiction" filterid="QuebecOperatorMenuConfig" />
      </ExtensionPointConfiguration>

      <ExtensionPointConfiguration extensionPath="/Lobby/Configuration">
        <NodeSpecification addinId="Client12Addins.QuebecJurisdiction" filterid="QuebecLobbyConfig" />
      </ExtensionPointConfiguration>
    </SelectableJurisdiction>
  </Extension>

  <Extension path="/Platform/Discovery/Configuration">
    <FilePath FilePath=".\\Config\\Jurisdiction\\Quebec-VLT\\PlatformDiscovery.config.xml" filterid="QuebecHardwareConfig" />
  </Extension>

  <Extension path="/TiltLogger/Configuration">
    <TiltLoggerConfiguration>
      <FilePath FilePath=".\\Config\\Jurisdiction\\Quebec-VLT\\TiltLogger.config.xml" filterid="QuebecTiltLoggerConfig" />
    </TiltLoggerConfiguration>
  </Extension>

  <DoorOpenAlarm RepeatSeconds="0" OperatorCanCancel="true">
    <FilePath FilePath=".\\Config\\Jurisdiction\\Quebec-VLT\\alarm.ogg" filterid="QuebecTiltLoggerConfig" />
  </DoorOpenAlarm>

  <Extension path="/OperatorMenu/Configuration">
    <FilePath FilePath=".\\Config\\Jurisdiction\\Quebec-VLT\\OperatorMenu.config.xml" filterid="QuebecOperatorMenuConfig" />
  </Extension>

  <Extension path="/Lobby/Configuration">
    <FilePath FilePath=".\\Config\\Jurisdiction\\Quebec-VLT\\Lobby.config.xml" filterid="QuebecLobbyConfig" />
  </Extension>
</Addin>
```

## Step 3: Add Jurisdiction Content

Each jurisdiction needs to specify hardware, operator menu, tilt logger,
and lobby configuration as well as several sound files.

### Hardware Config

Configuration for peripherals, I/O and other system hardware is
specified in an XML configuration file.  Because different jurisdictions
support different makes and models of hardware, the configuration file
resides in each jurisdiction folder.  As seen above in the
jurisdiction’s addin, the path to the file is provided as an
extension.  This document will not go into details about the schema or
content of the hardware configuration XML file.  Copy a
**PlatformDiscovery.config.xml** from another jurisdiction and edit as
needed to add/remove/modify hardware configuration.

### Operator Menu Config

Because different jurisdictions use different types of hardware, not all
hardware related operator menus are applicable to a given jurisdiction. 
The operator menu configuration file path is also as an extension within
the jurisdiction addin.  Copy **OperatorMenu.config.xml** from another
jurisdiction and edit it as needed.

### Tilt Logger Config

Each jurisdiction has different regulations for which events must be
logged and accessible in an operator menu.  In Monaco, the component
responsible for such a log is the TiltLogger, which uses a configuration
file to determine which system events to capture.  The tilt logger
configuration file path is also as an extension within the jurisdiction
addin.  Copy **TiltLogger.config.xml** from another jurisdiction and
edit it as needed.  The file is basically just a big list of event types
(fully qualified type names) with the name of the assembly in which they
are defined.  The log can group them as errors or tilts.  \<TBD –
difference???/>

### Lobby Config

The user interface of the Lobby can vary by jurisdiction; for example,
Quebec requires a multilanguage button but Oregon does not.  In
addition, the graphics and videos could change and be themed differently
for different jurisdictions.  The lobby is configured per jurisdiction
in the Lobby.Config.xml.  Below is a summary of some of the options:
  - MultiLanguageEnabled: True for a lobby UPI with a multilanguage
    button (required for Quebec).
  - ResponsibleGamingTimeLimitEnabled: True for a lobby with a
    responsible gaming time limit (required for Quebec).
  - LargeGameIconsEnabled: True for a lobby that displays large game
    icons (required for Quebec).
  - DaysAsNew: The number of days a newly installed game will be flagged
    as "new."  
  - ResponsibleGamingTimeLimits: Four time limit options for responsible
    gaming.  
  - LobbyUiDirectoryPath: The root path lobby asset files (e.g., images
    and videos).  By convention, graphical lobby assets that can vary by
    jurisdiction are placed in
    the *Config/Jurisdiction/\<SpecificJurisdiction\>/ui/* directory.
  - SkinFilename: References a .xaml file that defines XAML resources to
    configure the lobby visuals (mostly images and videos, and some
    positioning).  These XAML resources references jurisdiction based
    asset files in the LobbyUiDirectoryPath.  Right now the skin .xaml
    files are defined in the platform
    Aristocrat.Monaco.Gaming.VideoLottery.UI project, and copied to the
    platform output Skins directory (e.g., bin\\Platform\\Debug\\Skins)
    when building, but they are just loose xaml files that are
    dynamically loaded.

The other options refer to additional graphical assets that are needed
by the lobby outside of XAML.    

### Sound

Monaco currently depends on several sounds played by the platform. 
These sounds can differ by jurisdiction.  The sound files reside in each
jurisdiction subdirectory.  Add the required sound files below, or copy
them from another jurisdiction’s folder.

| **File**               | **Played When** |
| ---------------------- | --------------- |
| alarm.ogg              | TBD             |
| coinin.ogg             | TBD             |
| coinout.ogg            | TBD             |
| collect.ogg            | TBD             |
| feature\_bell.og       | TBD             |
| silence.ogg            | TBD             |
| snd\_print\_ticket.ogg | TBD             |
| touch\_sound.ogg       | TBD             |
