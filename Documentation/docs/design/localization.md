Multi-Language Suport
=====================

<!-- @import "[TOC]" {cmd="toc" depthFrom=1 depthTo=6 orderedList=false} -->

<!-- code_chunk_output -->

- [Requirements](#requirements)
- [Current multi-language support in Monaco](#current-multi-language-support-in-monaco)
- [Solution](#solution)
- [LocalizationService](#localizationservice)
  - [Configuration](#configuration)
- [CultureProvider](#cultureprovider)
  - [Examples](#examples)
- [CultureScope](#culturescope)
  - [Examples](#examples-1)
- [XAML](#xaml)
  - [Examples](#examples-2)
  - [WPFLocalizeExtensions](#wpflocalizeextensions)
- [Custom RESX Localization Provider](#custom-resx-localization-provider)
  - [Examples](#examples-3)
- [Resources](#resources)
  - [Resolving Duplicates](#resolving-duplicates)
  - [ResXManager Tool](#resxmanager-tool)
    - [Using The ResXManager Tool](#using-the-resxmanager-tool)
- [Jurisdictional Overrides](#jurisdictional-overrides)
  - [Building the Satellite Assemblies](#building-the-satellite-assemblies)

<!-- /code_chunk_output -->

# Requirements

This feature has the following requirements:
  - support English, French, Spanish, and Simple Chinese. 
  - support adding additional languages
  - all Platform messages a player could see need translations into
    these languages
  - support the operator setting the language for Platform messages
  - support for changing languages at run time
  - support for persisting the language selection
  - support for jurisdiction specific overrides of messages
  - support independent language selections for Players and Operators
  - support languages a game must implement to be enabled for play
  - support language selection for Tickets
  - support the operator setting the language for Tickets

Not included as part of this epic:
  - Providing/showing Game translations or graphics. 

# Current multi-language support in Monaco

Currently Monaco has a couple of methods for supporting multiple
languages and jurisdiction specific messages. There isn't a common
localization process.
  - Multiple Resource files embedded in projects as well as satellite
    DLLs.
  - A Localization.xml file in the Config directory that is similar to
    the Linux translation xml file.
  - Jurisdiction specific configuration xml files that provide messages
    using properties

# Solution
  - Consolidate resources files into one project. When the project is
    built, a template assembly (Aristocrat.Monaco.Application) and the
    associated satellite assemblies
    (Aristocrat.Monaco.Application.resources.dll) will be created in the
    Platform\\bin directory. The satellite assemblies will be in the
    culture sub-folders, i.e. Platform\\bin\\fr-CA.
  - Create a Resources.{culture}.resx file in the jurisdiction config
    folder for overrides. A Post-Event MsBuild script will create the
    resource assemblies in the associated
    Platform\\bin\\Config\\Jurisdiction folders. The resx files can
    include both text and images.
  - Create a culture provider that will manage multi-language
    requirements for a specific area. For example, a culture provider
    will be created for the player and operator multi-language
    requirements.
  - Resources will be referenced by a string key.
  - **WPFLocalizationExtensions** library will be used to retrieve the
    localized resource for a particular key in XAML. A custom service
    provider will extend the **WPFLocalizationExtensions** RESX provider
    for jurisdiction overrides.

# LocalizationService

The **LocalizationService** will manage the localization services
initialization logic for the Platform. This service will load in the
default resources and jurisdiction resources. It will also act as a
bridge between the culture providers and the RESX localization provider.
The **LocalizationService** will be created and initialized following
the properties providers. During initialization, the directory will be
scanned for culture providers defined in add-ins. When a culture
provider is discovered, it will be created and registered. The default
culture will also be added to the provider's list of available cultures.

**ILocalizationService Interface**

CultureInfo DefaultCulture { get; }

CultureInfo CurrentCulture { get; set; }

void Register(params ICultureProvider\[\] providers);

ICultureProvider GetProvider(string name);

bool IsCultureSupported(CultureInfo culture);

## Configuration

The initial configuration information for the **LocalizationService**
will be retrieved from the Locale.config.xml file located in the
selected jurisdiction folder. Before a Jurisdiction is selected, all
resource look-ups will target the default language which is "en-US".

# CultureProvider

Culture providers are used to manage the localization requirements for a
specific area. Initially, there will be a player and operator culture
provider. These two culture providers should cover the majority of the
multi-language requirements for the Platform. If there are specific
requirements that are outside of the purview of the existing culture
providers, a new culture provider can be created to implement the logic
for these requirements. Providers can be defined in add-in files or
instantiated by the container for layers that have container services
available. When created by the container, the provider will need to be
registered with the **LocalizationService**.

**ICultureProvider Interface**

string ProviderName { get; }

CultureInfo CurrentCulture { get; set; }

IReadOnlyCollection\<CultureInfo\> AvailableCultures { get; }

CultureInfo\[\] AddCultures(params CultureInfo\[\] cultures);

CultureInfo\[\] RemoveCultures(params CultureInfo\[\] cultures);

CultureInfo CoerceCulture(object parameter, CultureInfo culture);

bool IsCultureSupported(CultureInfo culture);

void SwitchTo();

TResource GetObject\<TResource\>(string key, object parameter = null);

## Examples

**Defining a CultureProvider**

\<Extension path="/Application/Localization/CultureProviders"\>

\<Provider
type="Aristocrat.Monaco.Application.Localization.OperatorCultureProvider"
/\>

\</Extension\>

**Using a Container**

var localization = \_container.GetInstance\<ILocalizationService\>();

var provider = \_container.GetInstance\<IPlayerCultureProvider\>();

localization.Register(provider);

# CultureScope

A **CultureScope** is used to wrap retrievals for localized resources. A
**CultureScope** can be created with a new statement passing in the
provider name. The **CultureScope** instance should always be disposed.
When a **CultureScope** is instantiated, the thread culture will change
to the current culture of the provider. When it is disposed, the
previous thread culture will be restored.

**ICultureScope interface**

TResource GetObject\<TResource\>(string key, object parameter = null);

string GetString(string key, object parameter = null);

string FormatString(string key, object parameter = null, params
object\[\] args);

## Examples

**Using a CultureScope**
```csharp
using (var scope = new CultureScope("OperatorTicket", valueForTheProvider))
{
  var title = scope.GetString("TicketTitle");
}
```
**Formatting**

```csharp
using (var scope = new CultureScope("Player"))
{
  var coinIn = scope.FormatString("CoinInFormat", args: 100);
}
```

**Using Localizer Static Class**

```csharp
var billIn = Localizer.For("Operator").FormatString("BillInFormat",
args: 100);
```

# XAML

**WPFLocalizationExtensions** markup extensions library was installed in
the UI projects to retrieve localized strings from XAML. This is a
powerful open source library that contains several built-in markup
extensions and other localization utilities. The localization strategy
for the Platform was built on top of this library. The preferred method
to retrieve localized resources for XAML is using the markup extension
contained in this library. Custom markup extensions can also be created
using the markup extension in this library as a guide.

## Examples

**Using WPFLocalizationExtensions in XAML**

```xml
<UserControl xmlns:lex="http://wpflocalizeextension.codeplex.com">
  <Grid>
    <Button Content="{x:Static loc:Resources.ReplayText}" />
    <!-- should be replace with -->
    <Button Content="{lex:Loc Key=ReplayText}" />
  </Grid>
</UserControl>
```

**Localized Image**

```xml
<Window xmlns:lex="http://wpflocalizeextension.codeplex.com">
  <Button x:Name="btnCashOut">
    <Image>
      <Image.Style>
        <Style TargetType="Image">
          <Setter Property="Source" Value="{lex:BLoc Key=CashOutNormal, Converter={StaticResource BitmapToImageConverter}}" />
          <Style.Triggers>
            <DataTrigger Binding="{Binding ElementName=btnCashOut, Path=IsEnabled}"
                         Value="False"/>
              <Setter Property="Source"
                      Value="{lex:BLoc Key=CashOutDisabled, Converter={StaticResource BitmapToImageConverter}}" />
            </DataTrigger>
        </Style.Triggers>
        </Style>
      </Image.Style>
    </Image>
  </Button>
</Window>
```

## WPFLocalizeExtensions

<https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension>

**Installing**

Install-Package WpfLocalizeExtension -Version 3.3.1

*Note: Make sure to select the target project before executing the
command.*

# Custom RESX Localization Provider

The custom RESX localization provider extends the base provider found in
the **WPFLocalizationExtensions** library. The purpose of this provider
is to provide the capability for jurisdictional overrides, connect the
Platform to the **WPFLocalizationExtensions** base provider, and define
attachable properties that can be included in XAML to target a specific
culture provider. This custom provider is used by the
**LocalizationService** and should not be referenced directly from
anywhere else in the Platform. The custom provider and attachable
properties for localization are located in the
Infrastructure/Aristocrat.Monaco.Localization project.

## Examples

**Using Attachable Properties**

```xml
<UserControl xmlns:loc="http://monaco.aristocrat.com/localization" loc:Localization.CultureFor="Player">
  <StackPanel>
    <!-- these controls will target the Player culture provider -->
    <Button Content="{lex:Loc Key=OkText}" />
    <TextBlock Text="{lex:Loc Key=CancelText}" />
    <!-- this control will target the Operator culture provider -->
    <Button Content="{lex:Loc Key=OpenText}" 
            loc:Localization.CultureFor="Operator" 
            loc:Localization.CultureForParameter="{Binding SomeParameter}" />
  </StackPanel>
</UserControl>
```

Note: The attachable properties are bindable.*

# Resources

The resources form all the solution projects have been consolidated into
one project. The consolidation was done approximately on November 6th.
Hence, any resources added after that date will not be included in the
centralized resources. Any resources added after that date, should be
moved to the centralized resources. All other resources located in other
projects will be removed at a determined time. The centralized resources
can be found in the Aristocrat.Monaco.Application project under the
Localization folder. Use the **ResXManager Tool** to view project
resources in the solution.

## Resolving Duplicates

In order to resolve the conflicts when consolidating the resource files,
some keys have been renamed. You will need to check the centralized
resource files to see if the key has been changed. For example, if
**AcceptText** was defined in three different projects, there will be
three keys named **AcceptText**,  **AcceptText2**, and **AcceptText3**.
Wherever possible, we need to try to eliminate duplicates. For instance,
when the localized string is the same for all three in all supported
languages, you can remove the duplicates and just use the **AcceptText**
key. Another instance may be when one is lowercase and the other is
uppercase. The case of text can be controlled in the XAML. Yet another
instance, maybe when one has a colon at the end of the text. You can use
Inline elements in the XAML to append a colon.

## ResXManager Tool

[ResXManager Visual Studio Extension](https://marketplace.visualstudio.com/items?itemName=TomEnglert.ResXManager)

### Using The ResXManager Tool

  - Install the ResXManger VS extension tool from the "Tools \>
    Extensions and Updates..." dialog.
  - Open the ResXManager tool from the Tools menu.
  - Initially, all the projects in the solution containing resource
    files will be selected. Deselect all of the resources.
  - Under the Aristocrat.Monaco.Application project, select the
    "Properties\\Resources" resources. This will show you all the keys
    and localized strings from each language side-by-side.
  - You can filter the list by clicking the filter icon in the Keys
    header. The comment column will contain the original project where
    the key was defined.

# Jurisdictional Overrides

Jurisdictional overrides are defined in the folder for the jurisdiction
that the override will apply to. The overrides will be defined in resx
files in the jurisdiction config folder. A resx file will exist for each
language that defines overrides. For instance, if an override needs to
be defined for French-Canada and Spanish-Mexico, there will be two resx
files in the jurisdiction folder named Resources.fr-CA.resx and
Resources.es-MX.resx, respectively. These files can be edited in Visual
Studio and can include both text strings and images. References to
external files in the resx file (i.e. images) should be contained in the
jurisdiction folder or sub-folders. To target the Invariant culture, a
Resources.resx file should be created.

## Building the Satellite Assemblies

The satellite assemblies will be built by an MsBuild script that will be
triggered during the Post-Build event of the Bootstrap project. This
script will run after the configs are copied to the jurisdiction folders
located under the Platform/bin/Config/Jurisdiction directory. After the
script completes, there should be an
**Aristocrat.Monaco.Jurisdiction.dll** in the jurisdiction folder and a
**Aristocrat.Monaco.Jurisdiction.resources.dll** satellite assembly in
culture sub-folders for each of the cultures where overrides exists. The
script should only build when a resource file has been changed for the
particular jurisdiction. If no resx file are include in the jurisdiction
config folder, then no assemblies will be built for that jurisdiction.
The MsBuild script is located in the Build\\Localization folder.
