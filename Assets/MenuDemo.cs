using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//EXTEND FROM MENU CREATOR
public class MenuDemo : MenuCreator
{
    // Start is called before the first frame update
    void Start()
    {
        CreateFirstDemoMenu();
    }

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

    public void CreateOtherMenu()
    {
        StartBasicMenu("Other Menu");
        AddBigLabel($"The loading screen will wait until you press <color=white>Spacebar</color>.{System.Environment.NewLine}If you press <color=red>Backspace</color> it will fail.");
        AddSpacer();
        AddSpacer();
        AddButton(new MenuButton("Loading Screen", () =>
        {
            ShowLoadingWithFailPossibility("Waiting for you to press <color=white>Spacebar</color>. Don't press <color=red>Backspace</color>!",PressingSpacebar,() => { ChangeToMenu(AfterLoading); },PressingBackspace,() =>{ChangeToMenu(FailedLoading);});
        },ButtonColors.ButtonYellow));
        AddSpacer();
        AddButton(new MenuButton("First Menu", () => {ChangeToMenu(CreateFirstDemoMenu);}));

        AddButton(new MenuButton("Hide Menu", () => {HideMenu(() =>
        {
            Debug.Log("This debug log will be triggered when menu is fully hidden.");
            StartCoroutine(RestartDemo());
        },0.5f);}, ButtonColors.ButtonOrange));
    }

    public void AfterLoading()
    {
        StartBasicMenu("Good Job");
        AddBigLabel("Way to press that spacebar!");
        AddSpacer();
        AddButton(new MenuButton("Back",() =>{ChangeToMenu(CreateOtherMenu);}));
    }
    
    public void FailedLoading()
    {
        StartBasicMenu("Damn!");
        AddBigLabel("You had to press backspace didn't you? :(");
        AddSpacer();
        AddButton(new MenuButton("Back",() =>{ChangeToMenu(CreateOtherMenu);}, ButtonColors.ButtonRed));
    }

    public bool PressingSpacebar()
    {
        return Input.GetKey(KeyCode.Space);
    }
    
    public bool PressingBackspace()
    {
        return Input.GetKey(KeyCode.Backspace);
    }

    IEnumerator RestartDemo()
    {
        yield return new WaitForSeconds(3);
        CreateFirstDemoMenu();
    }
}
