using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class MenuCreator : MonoBehaviour
{
    public UIDocument uiDocument;
    protected  VisualElement m_Root;
    protected VisualElement m_MenuPanel;
    protected VisualElement m_MenuHolder;
    protected VisualElement m_alertmanagerpanel;
    protected VisualElement m_ProgressBarBG;
    protected VisualElement m_ProgressBar;
    protected VisualElement m_InteractWindow,m_LoadingIcon;
    protected Label m_InteractLabel,m_DebugText;
    [HideInInspector]
    public Label m_MenuTitle,m_LoadingLabel;

    public virtual void Awake()
    {
        if (uiDocument == null) uiDocument = gameObject.GetComponent<UIDocument>();
        m_Root = uiDocument.rootVisualElement;
        m_MenuHolder = m_Root.Q<VisualElement>("MenuRoot");
        m_MenuHolder.SetDisplayBasedOnBool(false);
        m_ProgressBarBG = m_Root.Q<VisualElement>("LoadingProgressBG");
        m_ProgressBar = m_Root.Q<VisualElement>("LoadingProgressBar");
        m_MenuPanel = m_Root.Q<VisualElement>("MenuPanel");
        m_MenuTitle = m_Root.Q<Label>("MenuTitle");
        m_LoadingLabel = m_MenuHolder.Q<Label>("LoadingLabel");
        m_LoadingIcon = m_MenuHolder.Q<VisualElement>("LoadingIcon");
    }

    public bool menuFullyLoaded {
        get { return m_MenuHolder.style.opacity.value > 0.999f; }
    }

    public void AddTextField(string Label, EventCallback<ChangeEvent<string>> ChangedCallback, string defaultText = "", int maxLength = 25, bool isPassword = false)
    {
        UnityEngine.UIElements.TextField worldName = new TextField(Label, maxLength, false, isPassword, '*');
        worldName.labelElement.AddToClassList(textFieldLabel);
        worldName.RegisterValueChangedCallback(ChangedCallback);
        worldName.value = defaultText;
        //worldName.focusable = false;
        m_MenuPanel.Add(worldName);
        createdVisualElements.Add(worldName);
    }
    
    const string textFieldLabel = "textfieldLabel";

    public void AddSpacer()
    {
        VisualElement spacer2 = new VisualElement();
        spacer2.AddToClassList("Spacer");
        m_MenuPanel.Add(spacer2);
        createdVisualElements.Add(spacer2);
    }
    
    public void ChangeToMenu(Action newMenu)
    {
        HideMenu(newMenu);
    }
    protected void CreateMenu(params MenuButton[] menuButtons)
    {
        m_MenuHolder.SetDisplayBasedOnBool(true);
        m_MenuHolder.style.opacity = 0;
        DOTween.To(()=> m_MenuHolder.style.opacity.value, x=> m_MenuHolder.style.opacity = x, 1, 0.33f).SetEase(Ease.InOutQuad);
        ClearButtons();
        for (int i = 0; i < menuButtons.Length; i++)
            AddButton(menuButtons[i],out var b);
    }
    

    public static int desiredLoadingProgress = 0;
    public static int lastChangedLoadingPercentage = 0;
    public static string desiredLoadingText;
    public static void ChangeLoadingProgress(int progress)
    {
        lastChangedLoadingPercentage++;
        desiredLoadingProgress = progress;
    } 
    public static void ChangeLoadingText(string newString)
    {
        lastChangedLoadingText++;
        desiredLoadingText = newString;
    }

    public Label AddBigLabel(string labelContent)
    {
        Label nweLabel = new Label(labelContent);
        nweLabel.AddToClassList("ExtraText");
        m_MenuPanel.Add(nweLabel);
        createdVisualElements.Add(nweLabel);
        return nweLabel;
    }

    public void StartBasicMenu(string menuTitle)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ClearButtons();
        m_MenuTitle.text = menuTitle;

        if (m_MenuHolder.style.display.value != DisplayStyle.Flex || m_MenuHolder.style.opacity== 0)
        {
            m_MenuHolder.SetDisplayBasedOnBool(true);
            m_MenuHolder.style.opacity = 0;
             DOTween.To(() => m_MenuHolder.style.opacity.value, x => m_MenuHolder.style.opacity = x, 1, 0.33f).SetEase(Ease.InOutQuad);
        }
    }
    
    private static bool isShowingProgressBar = false; 
    protected void SetupLoadMenuStart()
    {
        ClearButtons();
        m_MenuHolder.SetDisplayBasedOnBool(true);
        m_ProgressBar.style.width = new StyleLength(Length.Percent(0));
        m_ProgressBarBG.SetDisplayBasedOnBool(false); 
        isShowingProgressBar = false; 

        m_MenuHolder.style.opacity = 0;
        DOTween.To(() => m_MenuHolder.style.opacity.value, x => m_MenuHolder.style.opacity = x, 1, 0.33f).SetEase(Ease.InOutQuad);
        m_MenuTitle.text = "Loading";
        m_LoadingIcon.SetDisplayBasedOnBool(true);
        m_LoadingLabel.SetDisplayBasedOnBool(true);
    }
    
    public void ShowLoading(string loadingText,Func<bool> WhileFalse, Action Afterwards)
    {
        
        m_LoadingLabel.text = loadingText;
        ChangeLoadingText(loadingText);
        StartCoroutine(ShowLoadingMenu(WhileFalse,Afterwards));
    }

    private float lastLoadingIconRotationAngle = 0;

    private void AnimateLoadingCircle(int currentlastChangedLoadingText, int currentlastChangedLoadingPercentage)
    {
        lastLoadingIconRotationAngle =
            Mathf.Lerp(lastLoadingIconRotationAngle, lastLoadingIconRotationAngle + 45, Time.deltaTime * 3);
        if (lastLoadingIconRotationAngle > 360) lastLoadingIconRotationAngle = lastLoadingIconRotationAngle % 360;
        m_LoadingIcon.style.scale = new Scale(Vector2.one * (1.3f + (0.05f * Mathf.Sin(Mathf.Abs(Time.time * 4.5f)))));
        m_LoadingIcon.style.rotate = new StyleRotate(new Rotate(new Angle(lastLoadingIconRotationAngle, AngleUnit.Degree)));
        
        isShowingLoadingMenu = true;
        if (currentlastChangedLoadingText != lastChangedLoadingText)
        {
            currentlastChangedLoadingText = lastChangedLoadingText;
            m_LoadingLabel.text = desiredLoadingText;
        }

        if (lastChangedLoadingPercentage != currentlastChangedLoadingPercentage)
        {
            if (!isShowingProgressBar) {
                m_ProgressBarBG.SetDisplayBasedOnBool(true);
                isShowingProgressBar = true;
            }
            currentlastChangedLoadingPercentage = lastChangedLoadingPercentage;
            m_ProgressBar.style.width = new StyleLength(Length.Percent(desiredLoadingProgress));
        }
    }

    
    public static bool isShowingLoadingMenu = false;
    public static int lastChangedLoadingText = 0;
    public IEnumerator ShowLoadingMenu(Func<bool> WhileFalse, Action Afterwards = null)
    {
        isShowingLoadingMenu = true;
        SetupLoadMenuStart();
        int currentlastChangedLoadingText = lastChangedLoadingText;
        int currentlastChangedLoadingPercentage = lastChangedLoadingPercentage;
        int extraFramesLoading = 10;
        while (extraFramesLoading>0)
        {
            if (WhileFalse.Invoke()) extraFramesLoading--;
            AnimateLoadingCircle(currentlastChangedLoadingText,currentlastChangedLoadingPercentage);
            yield return null;
        }
        isShowingLoadingMenu = false;
        HideMenu(Afterwards);
    }
    
    public void ShowLoadingWithFailPossibility(string loadingText,Func<bool> WhileFalse, Action Afterwards, Func<bool> CheckFailed, Action Failed)
    {
        m_LoadingLabel.text = loadingText;
        if (WhileFalse.Invoke() && !CheckFailed.Invoke()) {
            Afterwards?.Invoke();
            return;
        }
        StartCoroutine(ShowLoadingWithFailPossibilityCoroutine(WhileFalse,Afterwards,CheckFailed,Failed));
    }
    
    public IEnumerator ShowLoadingWithFailPossibilityCoroutine(Func<bool> WhileFalse, Action Afterwards, Func<bool> CheckFailed, Action Failed)
    {
        int currentlastChangedLoadingText = lastChangedLoadingText;
        int currentlastChangedLoadingPercentage = lastChangedLoadingPercentage;
        
        SetupLoadMenuStart();
        int extraFramesLoading = 16;
        while (extraFramesLoading > 0)
        {
            if (WhileFalse.Invoke() || CheckFailed.Invoke()) extraFramesLoading--;
            AnimateLoadingCircle(currentlastChangedLoadingText,currentlastChangedLoadingPercentage);
            yield return null;
        }

        isShowingLoadingMenu = false;

        if (CheckFailed.Invoke())
            HideMenu(Failed);
        else HideMenu(Afterwards);
    }
    
    protected void AddButton(MenuButton menuButton, out Button newButton)
    {
        newButton = new Button(menuButton.ButtonAction);
        newButton.text = menuButton.ButtonText;
        m_MenuPanel.Add(newButton);
        createdVisualElements.Add(newButton);
        menuButton.SetButtonColor(newButton);
    }
    
    public void AddButton(MenuButton menuButton)
    {
        var newButton = new Button(menuButton.ButtonAction);
        newButton.text = menuButton.ButtonText;
        m_MenuPanel.Add(newButton);
        createdVisualElements.Add(newButton);
        menuButton.SetButtonColor(newButton);
    }
    public const string TwoButtonHolderClass = "TwoButtonHolder";
    public const string TwoColorHolderClass = "TwoColorHolder";
    protected  void AddTwoButton(MenuButton menuButton,MenuButton menuButton2)
    {
        VisualElement TwoButtonHolder = new VisualElement();
        m_MenuPanel.Add(TwoButtonHolder);
        TwoButtonHolder.AddToClassList(TwoButtonHolderClass);
        createdVisualElements.Add(TwoButtonHolder);
        //Button 1
        var newButton = new Button(menuButton.ButtonAction);
        newButton.text = menuButton.ButtonText;
        TwoButtonHolder.Add(newButton);
        createdVisualElements.Add(newButton);
        menuButton.SetButtonColor(newButton);
        //Button 2
        var newButton2 = new Button(menuButton2.ButtonAction);
        newButton2.text = menuButton2.ButtonText;
        TwoButtonHolder.Add(newButton2);
        createdVisualElements.Add(newButton2);
        menuButton2.SetButtonColor(newButton2);
    }
    
    public const string menuSliderClass = "MenuSlider";
    public void AddSlider(ScrollView scrollView, string str, float defaultValue, EventCallback<ChangeEvent<float>> changedValue)
    {
        Slider SensitivitySlider = new Slider($"{str} : {defaultValue}", 0.001f, 2, SliderDirection.Horizontal);
        SensitivitySlider.AddToClassList(menuSliderClass);
        SensitivitySlider.value = defaultValue;
        SensitivitySlider.RegisterValueChangedCallback((evt) =>
        {
            
            SensitivitySlider.label = $"{str} : {evt.newValue}";
            changedValue?.Invoke(evt);
        });
        scrollView.contentContainer.Add(SensitivitySlider);
        createdVisualElements.Add(SensitivitySlider);
    }
    public Slider AddSlider(string str, float defaultValue,float startValue,float endValue, EventCallback<ChangeEvent<float>> changedValue)
    {
        Slider SensitivitySlider = new Slider($"{str} : {defaultValue}", startValue, endValue, SliderDirection.Horizontal);
        SensitivitySlider.value = defaultValue;
        SensitivitySlider.RegisterValueChangedCallback((evt) =>
        {
            
            SensitivitySlider.label = $"{str} : {evt.newValue}";
            changedValue?.Invoke(evt);
        });
        m_MenuPanel.Add(SensitivitySlider);
        createdVisualElements.Add(SensitivitySlider);
        return SensitivitySlider;
    }
    public SliderInt AddSliderInt(string str, int defaultValue,int startValue,int endValue, EventCallback<ChangeEvent<int>> changedValue)
    {
        SliderInt SensitivitySlider = new SliderInt($"{str} : {defaultValue}", startValue, endValue, SliderDirection.Horizontal);
        SensitivitySlider.value = defaultValue;
        SensitivitySlider.RegisterValueChangedCallback((evt) =>
        {
            
            SensitivitySlider.label = $"{str} : {evt.newValue}";
            changedValue?.Invoke(evt);
        });
        m_MenuPanel.Add(SensitivitySlider);
        createdVisualElements.Add(SensitivitySlider);
        return SensitivitySlider;
    }
    
    public static float lastTimeHidMenu;
    public void HideMenu(Action WhenFinished = null, float duration = 0.2f)
    {
//        Debug.Log("Called hide menu");
        lastTimeHidMenu = Time.realtimeSinceStartup;
        if (hasHideMenuTweener)
        {
            HideMenuTweener.Kill(false);
            hasHideMenuTweener = false;
            HideMenuTweener = null;
        }
        HideMenuTweener = DOTween.To(()=> m_MenuHolder.style.opacity.value, x=> m_MenuHolder.style.opacity = x, 0, duration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            hasHideMenuTweener = false;
            HideMenuTweener = null;
            m_MenuHolder.SetDisplayBasedOnBool(false);
            ClearButtons();
            m_LoadingIcon.SetDisplayBasedOnBool(false);
            m_ProgressBarBG.SetDisplayBasedOnBool(false);
            m_LoadingLabel.SetDisplayBasedOnBool(false);
            WhenFinished?.Invoke();
        });
        hasHideMenuTweener = true;
    }

    public void AbortHideMenu()
    {
        if (hasHideMenuTweener)
        {
            HideMenuTweener.Kill(false);
            hasHideMenuTweener = false;
            HideMenuTweener = null;
        }
    }
    private bool hasHideMenuTweener;
    private TweenerCore<float, float,FloatOptions> HideMenuTweener;
    
    public void InstantHideMenu()
    {
        m_MenuHolder.SetDisplayBasedOnBool(false);
        ClearButtons();
        m_LoadingIcon.SetDisplayBasedOnBool(false);
        m_ProgressBarBG.SetDisplayBasedOnBool(false);
        m_LoadingLabel.SetDisplayBasedOnBool(false);
    }

    public void AddTwoButtonsWhenPossible(params MenuButton[] menuButtons)
    {
        bool skipNext = false;
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (skipNext)
            {
                skipNext = false;
                continue;
            }
            if (i + 1 < menuButtons.Length)
            {
                AddTwoButton(menuButtons[i], menuButtons[i+1]);
                skipNext = true;
            }
            else
            {
                //Debug.Log($"Adding menu button {i} i+1 is {i+1} menubuttonsLength  - 1is {menuButtons.Length -1}");
                AddButton(menuButtons[i]);
            }
        }
    }
    
    public void CreateMenuWithTwoButtonWhenPossible(params MenuButton[] menuButtons)
    {
        m_MenuHolder.SetDisplayBasedOnBool(true);
        m_MenuHolder.style.opacity = 0;
        DOTween.To(()=> m_MenuHolder.style.opacity.value, x=> m_MenuHolder.style.opacity = x, 1, 0.33f).SetEase(Ease.InOutQuad);
        ClearButtons();
        bool skipNext = false;
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (skipNext)
            {
                skipNext = false;
                continue;
            }
            if (i + 1 < menuButtons.Length)
            {
                AddTwoButton(menuButtons[i], menuButtons[i+1]);
                skipNext = true;
            }
            else
            {
                //Debug.Log($"Adding menu button {i} i+1 is {i+1} menubuttonsLength  - 1is {menuButtons.Length -1}");
                AddButton(menuButtons[i]);
            }
        }
    }
    
    protected List<VisualElement> createdVisualElements = new List<VisualElement>();
    public const string scrollviewTwoButtonClass = "TwoButtonHolderScrollview";
    protected bool hasCurrentScrollview = false;
    protected ScrollView currentScrollView;
    public void ClearButtons()
    {
        //Debug.Log($"Clear Buttons");
        if (hasCurrentScrollview)
        {
            currentScrollView.RemoveFromHierarchy();
            hasCurrentScrollview = false;
            currentScrollView = null;
        }
        //Debug.Log("Cleared buttons");
        for (int i = createdVisualElements.Count - 1; i >= 0; i--)
            createdVisualElements[i].RemoveFromHierarchy();
        createdVisualElements.Clear();
    }
}


public enum ButtonColors
{
    ButtonBlue,
    ButtonGrey,
    ButtonBlueAlt,
    ButtonGreen,
    ButtonLightGreen,
    ButtonNavyBlue,
    ButtonOrange,
    ButtonRed,
    ButtonYellow,
}
public struct MenuButton
{
    public string ButtonText;
    public Action ButtonAction;
    public ButtonColors buttonColors;
    
    public const string MMenuButton = "MenuButton";
    public const string MenuButtonGray = "MenuButtonGray";
    public const string ButtonBlueAlt = "ButtonBlueAlt";
    public const string ButtonGreen = "ButtonGreen";
    public const string ButtonLightGreen = "ButtonLightGreen";
    public const string ButtonNavyBlue = "ButtonNavyBlue";
    public const string ButtonOrange = "ButtonOrange";
    public const string ButtonRed = "ButtonRed";
    public const string ButtonYellow = "ButtonYellow";
    
    public MenuButton(string buttonText, Action buttonAction, ButtonColors colors = ButtonColors.ButtonBlue) {
        ButtonText = buttonText;
        ButtonAction = buttonAction;
        buttonColors = colors;
    }

    public void SetButtonColor(VisualElement ve)
    {
        switch (buttonColors)
        {
            case ButtonColors.ButtonBlue:
                ve.AddToClassList(MMenuButton);
                break;
            case ButtonColors.ButtonGrey:
                ve.AddToClassList(MenuButtonGray);
                break;
            case ButtonColors.ButtonBlueAlt:
                ve.AddToClassList(MMenuButton);
                ve.AddToClassList(ButtonBlueAlt);
                break;
            case ButtonColors.ButtonGreen:
                ve.AddToClassList(MMenuButton);
                ve.AddToClassList(ButtonGreen);
                break;
            case ButtonColors.ButtonLightGreen:
                ve.AddToClassList(MMenuButton);
                ve.AddToClassList(ButtonLightGreen);
                break;
            case ButtonColors.ButtonNavyBlue:
                ve.AddToClassList(MMenuButton);
                ve.AddToClassList(ButtonNavyBlue);
                break;
            case ButtonColors.ButtonOrange:
                ve.AddToClassList(MMenuButton);
                ve.AddToClassList(ButtonOrange);
                break;
            case ButtonColors.ButtonRed:
                ve.AddToClassList(MMenuButton);
                ve.AddToClassList(ButtonRed);
                break;
            case ButtonColors.ButtonYellow:
                ve.AddToClassList(MMenuButton);
                ve.AddToClassList(ButtonYellow);
                break;
        }
    }
}
public class ButtonCallbackHolder
{
    public Action callback = default;

    public ButtonCallbackHolder(Action callback)
    {
        this.callback = callback;
    }

    public void Call()
    {
        callback?.Invoke();
    }
}

public static class VisualElementExtensionMethods
{
    
    public static Vector2 GetOffsetValues(this VisualElement sl,Vector2 OffsetAmount)
    {
        var OssetValues = sl.style.PosValues();
        OssetValues.x = OssetValues.x - OffsetAmount.x;
        OssetValues.y = OssetValues.y - OffsetAmount.y;
        return OssetValues;
    }

    public static void SetVisibleBasedOnBool(this VisualElement vs, bool b)
    {
        vs.style.visibility =
            b ? new StyleEnum<Visibility>(Visibility.Visible) : new StyleEnum<Visibility>(Visibility.Hidden);
    }
    public static void SetDisplayBasedOnBool(this VisualElement vs, bool b)
    {
        vs.style.display =
            b ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }
    public static void SetPosValues(this VisualElement sl, float newPosx,float newPosy)
    {
        sl.style.top = newPosy;
        sl.style.left = newPosx;
    }
    public static void SetPosValues(this VisualElement sl, Vector2 newPos)
    {
        sl.style.top = newPos.y;
        sl.style.left = newPos.x;
    }
    public static void SetPosValues(this IStyle sl, Vector2 newPos)
    {
        sl.top = newPos.y;
        sl.left = newPos.x;
    }
    public static Vector2 PosValues(this IStyle sl)
    {
        return new Vector2(sl.LeftValue(),sl.TopValue());
    }
    public static float TopValue(this IStyle sl)
    {
        return sl.top.value.value;
    }
    public static float LeftValue(this IStyle sl)
    {
        return sl.left.value.value;
    }
    public static float FloatValue(this StyleLength sl)
    {
        return sl.value.value;
    }
}