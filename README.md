# INSM-Template-API
Documentation and examples of the INSM Template API

## Introduction

An Instoremedia template is an zip file containing files to render dynamic content on a screen. The zip file must contain a descriptor file called start.xml. A template may currently be implemented using HTML/JS or .NET/C#


## Features

A templates main purpose it to render content. But the API provides many additional features for configuration and integration.


### Configuration

Template configuration is provided as the TemplateDataSet. The available data fields are  decalred statically in start.xml. When uploading and schduling the template on the AMS the configuration is defined, stored on the server,  transferred to players and stored locally on the player. Everytime a new dataset arrives to a player the template will receive the TemplateDataSetChanged event.

### Attributes

Read-only platform attributes are available as key/values and will describe the runtime environment. E.g. the current channel, region, opening hours etc.

### Properties

A template may generate and store data as key/values using TemplateProperties. This data is stored on the player to be used when a player restart. Properties can be defined to be shared accross multiple running instances of templates. All properties is also sent the the AMS when changed. If the property values are numeric, they will also be aggregated into statistics. The time segements for statistical data is defined by a configuration on the AMS.

### Statistics

Advanced statistics may be generated using the Statistics wrapper class. This may be used to measure both count and duration of generic actions by calling an API on begin and end. 

### Playlog

A template can record playback of individual files or datasets (assets). Multiple instances of the same file can be announced and are individually tracked. This is used for proof of play logging.

### Plugins

A plugin is a local running component. It works much the same way as a template but does not render anything and cannot be scheduled at times. It is used for separating integration and presentation. Normally plugins are used for integration with hardware. A plugin has a name and only on instance is allowed per plugin name. 

A template may connect to a plugin to send, receive of be notified by the plugin of data as key/Values.

### Commands

Commands are fast volatile in memory commands sent from the AMS to players. Commands are intended to be used for temporary modifications on a template. If persistent change is desired the configuration should instead be changed using the content dataset.

A command is a key/value pair that might return replay as a string or byte array.

### WarpSpace

Communication between templates locally or between players is done using warpspace. Each warpspace has a unique name and are isolated from each other. Currently a warpspace involving multiple players must be configured with a master IP address of one of the players but it does not matter which one. Any player can send a message to any other player, or broadcast to all players. The call can be synchronous or asynchronous. The call can have a reply if sent to a single peer. Broadcast messages cannot be replied.

## Previews

In most cases a template preview will be automatically generated from a headless preview player. But in some cases this will not be possible. In that case there are other options to avoid a black frame and display a relevant preview image.

Configure one input file to be full screen preview.

Embed an image called Preview.jpg to display a static preview image.


## start.xml

An Instoremedia template must contain a start.xml file. The start.xml file has three parts:

* _Manifest_ - Standard meta data about this file (template or plugin)
* _Template_ - Standard meta data about the template
* _Content_ - Custom defined meta data about the dynamic parameters the template expect from a user


## _Manifest_

This dataset contains defined information about the file. Only defined item ids are permitted.

### Item Ids (Mandatory)

_Title_ - Title of the template.

_DisplayName_ - Display fiendly name of the template.

_Description_ - Description of the template.

_Category_ - Value is the category title.

_Version_ - Version of the template.

_Type_ - Type should be "Template" for templates.

_Vendor_ - Vendor of the template.

_StartFile_ - Start file of the template.

_FileType_ - File Type of the template.

_FileHandler_ - FileHandler of the template.


## _Template_

This dataset contains defined information about the template. Only defined item ids are permitted.

### Item Ids (Optional)

_Orientation_ -	Value can be “Landscape” or “Portrait”

_Resolution_ - Value is pixels [width]x[height] e.g. “1920x1080”

_Identity_ - Identifier e.g. "Image" or "Movie". If specified templates with the same identity will be considered to have the same function. This is used as the TemplateName in the playlist data. If it is not present the manifest Title will be used instead. This may be used as an icon hint to override icons.

_UrlItem_ - Value is the content dataset item id with an url from which the template should be loaded. 

_Transition_ - Default transition effect like "Fade".

_IsTransitionsEnabled_ - Transitions enabled or not.

_Duration_ - Duration value of transition.

_IsDurationEnabled_ - Duration enabled or not.

_AutoDuration_ - Auto duration is enabled or not.

_IsAutoDurationOptionEnabled_ - Auto duration option enabled or not.

_IsVolumeControlEnabled_ - Volume control enabled or not.

_IsActiveXHost_ - Active XHost is enabled or not.


## _Content_

This dataset contains user defined configuration options for the template. The item Ids user defined but only defined attributes are permitted. 

### Type
Type of parameter

        Text        - Text string
        Numeric     - Number
        Boolean     - True/False
        MediaFile   - File reference
        Image       - File reference restricted to images
        Audio       - File reference restricted to audio
        Movie       - File reference restricted to movie
        Archive     - File reference restricted to zip
        Url         - Web link
        Html        - HTML text

### VisualType
Visual type of parameter

       text         - Text field
       checkbox     - True/False option
       dropdown     - Option list (requires AvailableValues)
       browse       - Select a file
	   slider		- For numeric values
       colorpicker  - Choose a color
	   playlist		- Choose a sub playlist
	   extendedtext - Rich text field
	   datagrid     - Display a table


### Attributes (Optional)

_DisplayName_ - Name of parameter to be displayd to the user

_DisplayGroupName_ - Name of visual grouping of items

_DisplayGroupId_ - Id of visual grouping of items (integer)

_Required_ - True if this attribute is required from the user

_RequiredMessage_ - Messages displayed if the user did not specify a value

_AvailableValues_ - Options for VisualType dropdown

_FilePattern_ - File extension options for VisualType browse 

_Advanced_ - True if parameter is an advanced parameter

_Hidden_ - True if the parameter is hidden from the user

_IsPreviewEnabled_ - True if automatic preview rendering should be attemted. If this is False and IsPreviewItemEnabled is also false the template can embedd a static file called Preview.jpg as the preview image.

_IsPreviewItemEnabled_ - True if an item should be used as preview for the whole template. In this case PreviewItem must be defined

_PreviewItem_ - Key of dataset item id to use as preview. This must be a file.

_Color_  -   Color as hex value with A,R,G and B.

_FileDescription_ - Kind of file with file extensions.

_Rows_  -  Number of rows.

_Font_  -   Name of the font.

_FontBold_  - Bold font is enabled or not.

_FontItalic_  - FontItalic is true or false.

_FontSize_ - Selected size of the font.

_TextAlign_  - Alignment of text left or right.

_AllowEdit_ - Allow edit true or false.

_Fonts_ - All fonts options.

_FontSizes_ - All font size options.

_DropShadow_ - enabled or not.

_DataColumnDisplayName.1_ - datagrid column display name.

_DataColumnKey.1_ - datagrid column display key.

_DataColumnRequired.1_ - datagrid column required or not.

_DataColumnRequiredMessage.1_ - datagrid column required message.





