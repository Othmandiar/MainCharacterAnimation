using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

using agora_gaming_rtc;

/// <summary>
/// this is an example of using ScreenSharing APIs for Desktops
/// </summary>
public class DesktopScreenShare : PlayerViewControllerBase
{

    Dropdown WindowOptionDropdown;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    readonly List<AgoraNativeBridge.RECT> WinDisplays = new List<AgoraNativeBridge.RECT>();
#else
    List<uint> MacDisplays;
#endif
    int CurrentDisplay = 0;

    public override void SetupUI()
    {
        base.SetupUI();
        if (SceneNames.temp)
        {

        
            Dropdown dropdown = StreamEventUIGetter.instance.dropdown;
        if (dropdown != null)
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            MacDisplays = AgoraNativeBridge.GetMacDisplayIds();
            WindowList list = AgoraNativeBridge.GetMacWindowList();
            if (list != null)
            {
                dropdown.options = list.windows.Select(w =>
                    new Dropdown.OptionData(w.kCGWindowOwnerName + " | " + w.kCGWindowNumber)).ToList();
            }
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // Monitor Display info
            var winDispInfoList = AgoraNativeBridge.GetWinDisplayInfo();
            if (winDispInfoList != null)
            {
                foreach (var dpInfo in winDispInfoList)
                {
                    WinDisplays.Add(dpInfo.MonitorInfo.monitor);
                }
            }

            // Window ID info
            Dictionary<string, System.IntPtr> winWinIdList;
            AgoraNativeBridge.GetDesktopWindowHandlesAndTitles(out winWinIdList);
            if (winWinIdList != null)
            {
                dropdown.options = (winWinIdList.Select(w =>
                    new Dropdown.OptionData(string.Format("{0, -20} | {1}",
                        w.Key.Substring(0, System.Math.Min(w.Key.Length, 20)), w.Value))).ToList());
            }
#endif
            WindowOptionDropdown = dropdown;
        }
        

        StreamEventUIGetter.instance.ShareWindowButton.GetComponent<Button>().onClick.AddListener(OnShareWindowClick);
        StreamEventUIGetter.instance.StopShareButton.GetComponent<Button>().onClick.AddListener(() => { mRtcEngine.StopScreenCapture(); });

        
         }
        GameObject screen = StreamEventUIGetter.instance.screen;
        screen.AddComponent<VideoSurface>();
    }

    int displayID0or1 = 0;
    void ShareDisplayScreen()
    {
        ScreenCaptureParameters sparams = new ScreenCaptureParameters
        {
            captureMouseCursor = true,
            frameRate = 15
        };

        mRtcEngine.StopScreenCapture();

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        mRtcEngine.StartScreenCaptureByDisplayId(MacDisplays[CurrentDisplay], default(Rectangle), sparams); 
        CurrentDisplay = (CurrentDisplay + 1) % MacDisplays.Count;
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        ShareWinDisplayScreen(CurrentDisplay);
        CurrentDisplay = (CurrentDisplay + 1) % WinDisplays.Count;
#endif
    }

    void ShareWinDisplayScreen(int index)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        var screenRect = new Rectangle
        {
            x = WinDisplays[index].left,
            y = WinDisplays[index].top,
            width = WinDisplays[index].right - WinDisplays[index].left,
            height = WinDisplays[index].bottom - WinDisplays[index].top
        };
        Debug.Log(string.Format(">>>>> Start sharing display {0}: {1} {2} {3} {4}", index, screenRect.x,
            screenRect.y, screenRect.width, screenRect.height));
        var ret = mRtcEngine.StartScreenCaptureByScreenRect(screenRect,
            new Rectangle { x = 0, y = 0, width = 0, height = 0 }, default(ScreenCaptureParameters));
#endif
    }

    void TestRectCrop(int order)
    {
        // Assuming you have two display monitors, each of 1920x1080, position left to right:
        Rectangle screenRect = new Rectangle() { x = 0, y = 0, width = 1920 * 2, height = 1080 };
        Rectangle regionRect = new Rectangle() { x = order * 1920, y = 0, width = 1920, height = 1080 };

        int rc = mRtcEngine.StartScreenCaptureByScreenRect(screenRect,
            regionRect,
            default(ScreenCaptureParameters)
            );
        if (rc != 0) Debug.LogWarning("rc = " + rc);
    }

    void OnShareWindowClick()
    {
        char[] delimiterChars = { '|' };
        if (WindowOptionDropdown == null) return;
        string option = WindowOptionDropdown.options[WindowOptionDropdown.value].text;
        if (string.IsNullOrEmpty(option))
        {
            return;
        }

        string wid = option.Split(delimiterChars, System.StringSplitOptions.RemoveEmptyEntries)[1];
        Debug.LogWarning(wid + " is chosen");
        mRtcEngine.StopScreenCapture();

        mRtcEngine.StartScreenCaptureByWindowId(int.Parse(wid), default(Rectangle), default(ScreenCaptureParameters));
    }
}
