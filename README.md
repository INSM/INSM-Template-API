# INSM-Template-API
Documentation and examples of the INSM Template API

An Instoremedia template is an zip file containing files to render dynamic content on a screen. The zip file must contain a descriptor file called start.xml. A template may currenlty be implemented using HTML/JS or .NET/C#


## start.xml

The start.xml file has three parts:

* _Manifest_ - Standard meta data about this template file
* _Template_ - Standard meta data about the template
* _Content_ - Custom defined meta data about the dynamic parameters the template expect from a user

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
       colorpicker  - Choose a color


### Attributes

_DisplayName_ - Name of parameter to be displayd to the user

_Required_ - True if this attribute is required from the user

_RequiredMessage_ - Messages displayed if the user did not specify a value

_AvailableValues_ - Options for VisualType dropdown

_FilePattern_ - File extension options for VisualType browse 

_Advanced_ - True if parameter is an advanced parameter

_Hidden_ - True if the parameter is hidden from the user
