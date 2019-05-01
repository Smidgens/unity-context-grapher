[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/Smidgens/UnityQuickBuild/master/LICENSE.md)

# Unity Context Grapher
Plugin for Unity that lets you customise the context menu in the project window. Useful for reducing workspace clutter for different types of workflow (example: hiding programmer centric options for animators or vice versa).

Inspired by [Shozo Hidaka](https://assetstore.unity.com/publishers/15224)'s [Menu Customizer](https://assetstore.unity.com/packages/tools/utilities/menu-customizer-44011) plugin.
(said plugin also supports customising scene context menu which this plugin currently does not, but this feature is compatible with Context Grapher as MC's scene context feature can be enabled by itself)

Context grapher aims for a similar but slightly different approach as Menu Customizer, mainly that of allowing multiple configurations to be created, and presenting customisation of the menu in a more intuitive way with respect to how a menu is structured (tree vs. flat list).

Features include:
* Graph editor for managing menu options.
* Different configurations for menus can be created and switched between.
* Options in menu can be removed without deleting them or their suboptions from the configuration entirely (e.g. by deleting edges connecting them to the tree root)
* The active menu configuration (the one currently enabled and overriding Unity's default) is cached and thus persists after Unity is closed and reopened. 
* Resulting menu configuration can be previewed from inspector before it's applied.


| Graph editor |
| ------------- |
| ![Graph](/Screenshots/01.png?raw=true "Graph") |

| Configuration asset       |
| ------------- |
| ![Inspector](/Screenshots/03.png?raw=true "Inspector") |


| Default Unity menu | Context Grapher menu |
| ------------- |:-------------:|
| ![Unity Menu](/Screenshots/05.png?raw=true "Graph")      | ![Menu](/Screenshots/02.png?raw=true "Menu") |

## FAQ

(conceivably frequent)

**Does the plugin bring any performance overhead?**

*Yes, but negligible. Because the active configuration is cached, and needs to override the default menu when it is opened, there is slight performance overhead tied to the operation of finding the instance and caching it statically in code. But considering that this occurs only once, or at the very least infrequently (and only when the menu is opened on right click), it should not produce easily discernible bearing on performance.*

**Why "Grapher" and not "Tree-er"?**

*a) it is technically possible to reuse parts of the graph, though this is unlikely to be needed (perhaps useful to store multiple types of menus within the same configuration file for multiple types of developers) and b) grammatical niceness*

## Current limitations
* Doesn't support customisation of scene context menu, [Menu Customizer](https://assetstore.unity.com/packages/tools/utilities/menu-customizer-44011) does. This feature might be added depending on feedback, and whether it can be reliably implemented (MC itself serves as a valuable comparison in this respect in its strengths and limitations).
* Currently when the menu is summoned on right click, its displayed list is regenerated from the current configuration. This regeneration has little to no performance impact, but is definitely needless, as the menu lists need only be regenerated when the configuration file changes, or when compilation occurs (and only then to the extent that the list of menu options registered as callbacks to the Unity Editor were affected).
* There isn't any validation currently, as opposed to Unity's default, for adding/removing options based on what type of object is right clicked in the window (folders vs files for example). It is uncertain to what extent this can be remedied to match that of Unity's or what approach is sensible while keeping the plugin lightweight, but ideally it would be added eventually to make it a proper alternative to the default. This also seems to be a limitation in Menu Customizer so perhaps similar assessments have been made by its developer towards some conclusion.
* There is currently a bug affecting retrieval of the cached active configuration. The exact cause is not pinned down yet, but when it occurs, the instance retrieved through Unity's editor API is of an unexpected type causing a type cast error (in this respect the current implementation is most likely derived from a flawed understanding of Unity's asset serialization/deserialization). This is a critical flaw as occurrences will require the user to re-apply the active menu configuration whenever Unity is re-opened.
Until this is fixed, the error outputted is show thusly : `Failed retrieving cached menu. Configuration needs to be reapplied.`


