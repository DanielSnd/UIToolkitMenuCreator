# UIToolkitMenuCreator
UI Toolkit Menu Creator is a class and UXML/Stylesheet you can extend from to easily create Unity Menus from C# code without having to edit UXML.

# Dependencies
UI Toolkit Menu Creator uses DOTween for fading menus:
### DOTween [License here](http://dotween.demigiant.com/license.php)

# Demo
![Demo](https://github.com/DanielSnd/UIToolkitMenuCreator/blob/main/ReadmeImages/MenuDemo.gif?raw=true)

The demo scene is in **Assets\MenuCreatorDemo\MenuCreatorDemo.scene**.
Its main file is ****Assets\MenuCreatorDemo\MenuDemo.cs** and shows how to use Menu Creator.

This is how the first demo menu is set up in code:

```csharp
    public void CreateFirstDemoMenu()
    {
        StartBasicMenu("Main Menu");
        AddTwoButtonsWhenPossible(
            new MenuButton("First Button", () => {Debug.Log("Pressed First Button");}, ButtonColors.ButtonGrey),
            new MenuButton("Second Button", () => {Debug.Log("Pressed Second Button");}, ButtonColors.ButtonBlueAlt)
            );
        
        AddSpacer();
        var addedLabel = AddBigLabel("This label changes with the text field/slider values!");
        AddSpacer();
        AddTextField("Text Field", evt => { addedLabel.text = $"Text Field is: {evt.newValue}";},"Default Text Field Text");
        AddSpacer();
        AddSliderInt("Slider",0,0,10, evt => { addedLabel.text = $"Slider is set to {evt.newValue}";});
        AddSpacer();
        AddButton(new MenuButton("Other Menu", () =>
        {
            ChangeToMenu(CreateOtherMenu);
        }));
    }
```

# Usage

- Extend your menu file from **MenuCreator**
- Add a UIDocument to the scene with **MenuCreatorUXML** as its source asset.
- Add your menu file as a component to the same gameobject.
- Refer to the Menu Demo on what methods to call to setup your menu.
