# Aristocrat Gaming Platform (Monaco)
C#, Windows-based Aristocrat gaming platform (codename Monaco).

## Contributing
Refer to the [Monaco Getting Started Guide](https://confy.aristocrat.com/display/GTech/Monaco+%7C+Getting+Started) on how to set up your development environment to contribute to Monaco, involving the setup of a Personal Access Token, Git Commit Signing, NuGet package source setup, etc; as well as the general contribution workflow (ticketing, pull requests, etc).

See the [R2C Development Processes](https://confy.aristocrat.com/pages/viewpage.action?spaceKey=GTech&title=R2C+Development+Processes) Confluence page to familiarize yourself with the branching and release strategy introduced on September 2023.

## Builds
- [TeamCity Project](http://usan-abuild-01.dev.local/project/Monaco)
    - Root TeamCity project for the Monaco platform.
- [Trunk Builds](http://usan-abuild-01.dev.local/buildConfiguration/Monaco_PlatformDailyBuildTrunkGit)
    - Builds from the `main` branch whenever new commits are introduced.
- [Epic Branch Builds](http://usan-abuild-01.dev.local/buildConfiguration/Monaco_Arch_PlatformFeatureBuildTrunkVs2019Git?branch=&mode=builds#all-projects)
    - Any branch named with the `epic/` prefix will automatically be built in TeamCity.

---

## Building and Running
- Open the `Monaco.sln` solution in Visual Studio (2019 or 2022) as administrator.
    - Alternatively there is a `noanalysisdev.bat` and `noanalysisdev2022.bat` script which can improve performance in Visual Studio by disabling code analysis if you run into issues. Run this as admin. Use `noanalysisdev.bat` if you have VS2019, or `noanalysisdev2022.bat` if you have VS2022.
- Rebuild the project.
- Run/Debug the Bootstrap project, which is the entry-point for Monaco.
    - Enable Native Code Debugging
        - Without this, Monaco will simply exit while debugging when the GDK Runtime starts.
    - Use the following commandline arguments for a smoother development experience: 
    `display=windowed showTestTool=true showMouseCursor=true ignoreTouchCalibration DisplayFakePrinterTickets=true readonlymediaoptional=true`
        - See [Monaco Command Line Arguments](https://confy.aristocrat.com/display/MON/Monaco+Command+Line+Arguments) for a list of commandline arguments, and more info.
    
- The Status Display window will first appear, displaying the progress and any messages for the startup phase.
- The Configuration Wizard will appear on the initial launch, which is used to configure the EGM.
    - If the EGM doesn't have touchscreen displays, Monaco won't exit; it will just proceed to the next phases of startup.
- Once configuration is complete, the platform will exit. Relaunching the platform via the Bootstrap project will then 
result in the game Lobby to appear, where any installed and enabled games will be presented and playable.

### Installing the GDK Runtime + Games
On the initial launch of the game lobby, if no games have been set up, then the lobby will be empty. 

To install the GDK Runtime and games:
- Download the GDK Runtime from TeamCity by going to the [Composite (Arch + GDK 5.0 Games) builds](http://usan-abuild-01.dev.local/buildConfiguration/Monaco_Arch_CompositeGdk50), select the latest successful build > navigate to the Artifacts tab > Download `Platform/packages/ATI_Runtime_x.x.x.xxx.iso` and any games, such as `Platform/packages/ATI_WW_BuffaloGoldRevolution_x.xx.x.xx.iso`.
- Once the ISO downloads are finished, create the directory `bin/Debug/Platform/packages` (substitute `Debug` for whichever build configuration you're using), and place both the runtime and game ISOs there.
- When launching Monaco and entering the lobby, it will display a message `No Games Enabled`. To enable the games you just installed:
    - Open the Operator Menu (Press F9 > F1).
    - Navigate to the Games page, select the Game Type and Game that you wish to configure and enable, press K to open the Logic Door, select the denoms desired, then press Save.
    - Close the Logic Door by pressing L, then close the Operator Menu by pressing F1 again. 
    - You should now see games enabled in the lobby that you can press to play.
- Once a game is launched, you can easily add credits by pressing the top-row number keys 1-7 (ensure that keyboard shortcuts are toggled with F9). Press the spacebar to play.
- To return back to the lobby, press the Player Menu button in the game and press the More Games button.

## External Documentation
 - [Comprehensive Getting Started Guide](https://confy.aristocrat.com/display/GTech/Monaco+%7C+Getting+Started)
 - [Understanding and Getting Started with Monaco](https://confy.aristocrat.com/display/GTech/Understanding+and+Getting+Started+With+Monaco)
 - [Exhaustive guide on building and debugging Monaco](https://confy.aristocrat.com/display/MON/Building+and+Debugging+Monaco)
 - [GDK Runtime + Games Setup](https://confy.aristocrat.com/display/GTech/Level+2+-+Steps+for+Building+and+Running+the+Monaco+Platform#Level2StepsforBuildingandRunningtheMonacoPlatform-DownloadRuntimeandGame(s))
 - [Development guides for various facets of Monaco](https://confy.aristocrat.com/display/MON/Development)
 - [Keyboard Commands](https://confy.aristocrat.com/display/MON/Keyboard+Commands)
 - [Commandline Arguments](https://confy.aristocrat.com/display/MON/Monaco+Command+Line+Arguments)
 - [Fixing Unsigned Git Commits](https://confy.aristocrat.com/display/GTech/Monaco+%7C+Fixing+Unsigned+Git+Commits)
 - [Common Abbreviations/Acronyms](https://confy.aristocrat.com/pages/viewpage.action?spaceKey=STUD&title=Abbreviations)
 - [Persistent Storage, SQLite, Monaco Platform](https://confy.aristocrat.com/display/GT/Video+Lottery+Terminal#VideoLotteryTerminal-PersistentStorage,SQLite,MonacoPlatform)

---

# Quality Control Inspection Tool (QC Inspection Tool)
A slimmed-down, special-purpose version of Monaco for inspectors to validate the assembler's work during the EGM manufacturing process. The QC Inspection tool automates individual tests, and provides a summary of inspection results.

## Building and Running
- Open the Monaco repo's `Inspection.sln` solution in Visual Studio as administrator.
    - Alternatively, run the `noanalysisInspection.bat` script as admin, which opens the solution in VS with code analysis disabled.
- Rebuild the solution.
- Build the `Aristocrat.Monaco.Inspection` project under the `/Test/Integration` folder of the solution. This is required to overwrite certain files such as the addins to allow the Inspection Tool to run.
- Run the Bootstrap project just like running Monaco's Bootstrap.
- The experience is similar to Monaco's Bootstrap launch, except the UI will be presented in a gold theme to indicate that it's running in Inspection mode.

## Precautions
- Clear out the /bin folder if you want to build Monaco.sln after building Inspection.sln, since the extra Inspection test DLL will still be there otherwise.

## Implementation Overview
- The `Inspection.sln` solution includes the `Aristocrat.Monaco.Inspection` project in addition to the rest of the projects that are typically included in the Monaco solution. 
- To build off the existing Monaco solution and still load the QC Inspection Tool's required types, some of Monaco's `*.addin.xml` files are overwritten within the
 `Aristocrat.Monaco.Inspection` project. Specific base addin xml files are explicitly removed in the project's build steps, and the Inspection versions are copied to 
 the output directory instead – the Inspection's addin.xml files are set to Copy Always – the Inspection project takes precedence in the build order due to depending 
 on its targeted projects. These overwritten addin xml files register the InspectionService, InspectionWizard, and other required types.
- During the launch of Bootstrap, an InspectionService gets initialized, which sets a property via the IPropertiesManager for a key defined in `KernelConstant.IsInspectionOnly`. This property is checked throughout the codebase to check if the app is running in inspection-mode.
- The Inspection Wizard pages are defined within `<AddinConfigurationGroup name="InspectionWizardPages" ...`  in `WizardConfiguration.addin.xml`. To add new pages, this group must be updated and a new `PageLoader` class that inherits `OperatorMenuPageLoader` must be added to the `Aristocrat.Monaco.Application.UI.Loaders` folder.
    - All current inspection wizard pages are reused from existing Monaco pages, usually intended for tabs Audit Menu > Hardware page. They needed to have wizard behavior added to them to make them dual-use. (Monaco itself already had a few dual-use pages, such as Hardware Config (albeit with some derived classes)). This is more likely to be the case for new Inspection pages, as we would be testing hardware that Monaco should already know about (and know how to run/inspect).
- Automated tests and their instructions for the inspection tool live in the `AutomationInstructions.xml` file within the `Aristocrat.Monaco.Inspection` project.
