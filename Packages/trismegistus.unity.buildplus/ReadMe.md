Build+ Unity Extension
----------------------
Build+ can help you to version your app in a straight-forward way and avoid some of the headaches with maintaining
release notes outside of the development environment. With Build+ you can even include your release notes in the
app itself! As Build+ evolves, additional useful features will be added to automating your builds.

Launch Build+ by selecting File->Build+...

When you save you'll see a _BuildPlus.xml and a _BuildPlus.asset added to the root of your Assets/ folder. The xml is 
used only for easy merging. The asset file is a binary serialized version that you can use directly in your app for 
showing the version number/release notes. An example of this is shown in BuildPlus/Example/Example.unity.

Suggested workflow: 
1. All developers on the team can add their significant changes to the release notes and check in the 
_BuildPlus.xml/asset along with the changed files. This should be done at the current version shown in Build+.
2. When a build is made and released, the developer who makes the build should increment the version number and check
in _BuildPlus.xml/asset, so that other developers know where new notes should go.

ReleaseNotes.txt:
After you have made a single build using the standard Build menu, you can then use Build+ to make additional builds 
using the previously used settings. When you build this way, you will see a ReleaseNotes.txt file alongside your build.

Please send any feedback to buildplus@luminaryproductions.net.

Enjoy!
Luminary Productions