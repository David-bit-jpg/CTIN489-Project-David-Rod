                    Magic Lightmap Switcher

  What is it?
  -----------

  Magic Lightmap Switcher is a Unity editor extension that will allow you 
  to store baked lightmap data and then switch between its instantly or interpolate smoothly. 
  
  The following data is involved in switching and blending:
    * Lightmaps
    * Lightprobes
    * Reflection probes
    * Light sources settings
    * Skybox texture
    * Scene fog settings
    * Any custom data such as post-processing settings, Volume component settings in SRP, 
      or variable values ​​of any of your script
  
  System features:
    * Switching and blending all available baked data in the scene at runtime
    * Using shaders and multithreading for system operation, which allows you to maintain high performance
    * Support for multiple lighting scenarios for one scene
    * Automatic baking and storing process of lightmaps in turn order

  The Latest Version
  ------------------
    
  The latest version is always available in the asset store. Interim updates 
  not published in the asset store can be obtained by contacting the developer. 
  
  Documentation
  -------------
    
  Up-to-date documentation is located at
  https://motiongamesstudio.gitbook.io/magic-lightmap-switcher/

  CHANGELOG

  v 1.37.3
  -------------

  Bug Fixes:
    - Core: Lighting Scenario events are reset after editor restart.  

  v 1.37.2
  -------------

  Bug Fixes:
    - Preset Manager: Game Objects settings reset during baking with Bakery.
    - Lightmap Data Storing: Fixed a bug due to which Terrain data was saved with errors.
    - Lighting Scenario: Fixed a bug that caused Events to work incorrectly.

  Improvements:
    - Preset Manager: Added options to preserve the light source shadow type (None/Hard/Soft) and the Baked Shadow Angle.
    - Preset Manager: Added support for preserve Environment Lighting Settings.

  v 1.37.1
  -------------

  Improvements:
    - Added support for saving Environment Lighting settings to presets.

  v 1.37
  -------------

  Bug Fixes:
    - Fixed a bug due to which preset data was reset during baking with Bakery.
    - Fixed a bug due to which the instance of the main component was not deleted 
      after calling the "Clear All Data" option, which led to many errors.

  Improvements:
    - Added support for callbacks during blending.
    - Now the plugin can work in the lightmap switching only mode without the need for a shader patch.

  v 1.36
  -------------

  Bug Fixes:
    - Dynamic and static component on the same object caused "Null Reference Exeption" error.
    - Deleted Custom Blendable objects cause errors in the preset manager.
    - Terrain was incorrectly accounted for by the system.
    - Fog settings are overwritten for all presets.
    - Fixed other bugs in the preset manager.
    - Fixed a bug when working with a few lighting scenarios.
    - Missing light probes in the scene causes an error in UpdateOperationalData.
    - An error occurred while rewriting a scenario asset if the previous option did not contain Reflection Probes.
    - Fixed bug when working with meshes with multiple materials.

  Improvements:
    - Enviro support.

  v 1.35
  -------------

  Bug Fixes:
    - The preset parameters are interrupted if baking starts with the preset manager window open.
    - Rotation values ​​for light sources are sometimes incorrectly assigned.

  Improvements:
    - Access to settings and presets is now locked during baking.
    - New versions of the plugin can be downloaded immediately after release through the "About MLS..." window.

  v 1.34
  -------------

  Bug Fixes:
    - Fixed a bug due to which objects were sometimes assigned different UIDs when changing presets, which led to an error during blending.
    - Fixed a rare bug that caused the lightmap data asset to lose all settings.
    - Fixed a bug that occurred if the Terrain was not marked as static.
    - Fixed bugs in test scenes.

  Improvements:
    - Supports switching/blending for Bakery's RNM and SH maps.
    - The ability to save transforms of game objects to presets and, as a result, synchronize them with the Global Blend value.
    - Blending of the tint color for the skybox shader is now also supported.

  v 1.33
  -------------

  Bug Fixes:
    - Fixed a bug due to which reflections were incorrect for all bakes in the 
      queue, except for the first.

  Improvements:
    - Changed blending mechanics to support batching. (only works in play mode and build)
    - Added the ability to save the state of objects in presets (enablend/disabled)
    - Optimized lightmap blending code to eliminate unnecessary computations.

  v 1.32
  -------------

  Bug Fixes:
    - An error that caused blending to stop if the object has 
      "scaleInLightmap = 0" or "lightmapIndex = -1".
    - Texture format error for Android platform.
    - Light references are lost after reloading the editor.
    - A bug due to which, when creating a new preset, data in 
      all existing presets is overwritten.
    - A bug that caused the rotation values ​​of the light to reset 
      to zero during the first edit.

  Improvements:
    - Ability to move the plugin to any project folder.
    - Ability to specify the path to store data in the "Resources" folder.

  v 1.31
  -------------

  Bug Fixes:
    - Changed code to work around the situation when the range of a light 
      source of type Area increased infinitely in the editors of version 2019.
    - Fixed a bug that prevented editing scene lighting settings when the preset 
      manager window was active.
    - Fixed a bug that caused spam in the console when adding multiple empty objects 
      to the blending queue.

  Improvements:
    - Added the ability to select a scenario in which the data after re-baking 
      will be overwritten.
    - Added a warning in the console and in the interface that is shown when an error 
      is detected in the blending queue data.

  v 1.3
  -------------

  Bug Fixes:
    - An issue was found that is causing errors while building the project.
    - Fixed a bug that prevented adding multiple fields to the Custom Blendable.

  Improvements:
    - The Custom Blendable module has been completely rewritten.
    - Color fields are now recognized by Custom Blendable.

  v 1.2
  -------------

  Bug Fixes:
    - An error related to paths conflict due to which the interface does not work correctly.
    - Shader restoring is not possible on systems with limited user rights.
    - An issue has been encountered where the notification that all lightmaps have been 
      successfully stored will pop up indefinitely if you cancel the bake and store process.
    - An issue was found that is causing errors while building the project.
    - Reflections work with errors in deferred rendering. Implemented support for system 
      operation without support for deferred reflections.

  Improvements:
    - Added preset manager. Now you can manage all scene presets from one place and edit them quickly.
    - Added a new manager option. Clear Default Data Folder - Allows you to choose whether to delete 
      duplicate lightmap data in the default folder.
    - Added tooltips to manager options. To see this, hold the mouse cursor over the option name.

  v 1.1
  -------------

  Bug Fixes:
    - Fixed a bug due to which the Shadowmask was not taken into account by the system.
    - Fixed a bug due to which the Bakery could not bake light probes
    - Unexpected system behavior for objects with a 0 lightmap scale
    - Fixed an issue where lightmaps were not displayed after baking manually using the 
      standard Unity Lighting window.
    - Fixed a bug in the DayNightController (Day/Night Cycle example) script that caused 
      the MLS Light component to be infinitely added to the sun object.

  Canges:
   - Changed the mechanism of the shader patch. Now the patch is launched from the command 
     line with administrator rights (for Windows). This should fix the access violation issue 
     for most restricted users. For the rest, instructions are written in the documentation on 
     how to get around the limitation.

  Improvements:
    - Added a new blend option for CustomBlendable, now interpolation can occur between stored variable values.


  v 1.0
  -------------

  First release.

  Contacts
  -------------

  e-mail: evb45@bk.ru
  telegram: https://t.me/EVB45
  forum: https://forum.unity.com/threads/magic-lightmap-switcher-storing-switching-and-blending-lightmaps-in-real-time.966461/
  discord channel: https://discord.gg/p94azzE