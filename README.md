# UnityAssist

---

### Palette Window
![Alt text](/Screenshots/shot_0.png?raw=true "Optional Title")
> If you register scene files or frequently visited folders, you can easily access them from palette
* Open palette from menu [Window/File Palette]
* Add/Remove palette item with [+], [-] button
* Drag and drop file from project window
* One click -> focus item on project window
* Double click -> open target (scene, folder, source code, etc.)
 
### Hierarchy Reference Backtracer
![Alt text](/Screenshots/shot_1.png?raw=true "Optional Title")
> When you need to know some object whether linked in others, drag that and find it
* Open backtracer from menu [Window/Hierarchy Reference Backtracer]
  * option 1) use shortcut [Shift+F12] with a seleted object in the hierarchy window
  * option 2) right click target object and use 'Backtrace Reference' menu to work.
* Reference objects are displayed by listing each component type
* You can move to reference object with click object field

### Hierarchy Move Shortcuts
> This might be interesting if you've ever been annoyed with moving objects in hierarchy with mouse drag.
* This makes selected hierarchy objects can be moved by keyboard shortcuts [Ctrl or Cmd + Shift + H, J, K, L]
  * Unfortunately, the arrow hotkeys are blocked in the hierarchy window so assigned a beam style key.
  * If there is a more familiar hotkey, please refer to the [documentation](https://docs.unity3d.com/ScriptReference/MenuItem.html) and customize it.
* When used with the default hotkeys below, you can select objects and move them to the desired hierarchy without the mouse.
  * [Ctrl + 4] focus hierarchy view
  * [Shift or Ctrl + ↑, ↓] mutiple select
  * [→, ←] fold or unfold selected objects
  * [Shift + C] unfold all child objects and select them
