using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class GlobalKeyboardHook
{
    [DllImport("user32.dll")]
    static extern int CallNextHookEx(IntPtr hhk, int code, int wParam, ref keyBoardHookStruct lParam);
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowsHookEx(int idHook, LLKeyboardHook callback, IntPtr hInstance, uint theardID);
    [DllImport("user32.dll")]
    static extern bool UnhookWindowsHookEx(IntPtr hInstance);
    [DllImport("kernel32.dll")]
    static extern IntPtr LoadLibrary(string lpFileName);

    public delegate int LLKeyboardHook(int Code, int wParam, ref keyBoardHookStruct lParam);

    public struct keyBoardHookStruct
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }

    public GlobalKeyboardHook(KeyEventHandler keyDown, KeyEventHandler keyUp = null)
    {
        llkh = HookProc;
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
            this.HookedKeys.Add(key);
        KeyDown = keyDown;
        KeyUp = keyUp;
    }

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    private LLKeyboardHook llkh;
    private List<Keys> HookedKeys = new List<Keys>();
    private IntPtr hook = IntPtr.Zero;
    private static event KeyEventHandler KeyDown;
    private static event KeyEventHandler KeyUp;

    public void Hook()
    {
        try
        {
            IntPtr hInstance = LoadLibrary("User32");
            hook = SetWindowsHookEx(WH_KEYBOARD_LL, llkh, hInstance, 0);
        }
        catch { }
    }

    public void Unhook()
    {
        try
        {
            UnhookWindowsHookEx(hook);
        }
        catch { }
    }

    public static void SignKeyDownHandler(KeyEventHandler KeyDownHandler)
    {
        KeyDown += KeyDownHandler;
    }

    public static void SignKeyUpHandler(KeyEventHandler KeyUpHandler)
    {
        KeyUp += KeyUpHandler;
    }

    public static void UnsignKeyDownHandler(KeyEventHandler KeyDownHandler)
    {
        KeyDown -= KeyDownHandler;
    }

    public static void UnsignKeyUpHandler(KeyEventHandler KeyUpHandler)
    {
        KeyUp -= KeyUpHandler;
    }

    public int HookProc(int Code, int wParam, ref keyBoardHookStruct lParam)
    {
        if (Code >= 0)
        {
            Keys key = (Keys)lParam.vkCode;
            if (HookedKeys.Contains(key))
            {
                KeyEventArgs kArg = new KeyEventArgs(key);
                if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
                    KeyDown(this, kArg);
                else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
                    KeyUp(this, kArg);
                if (kArg.Handled)
                    return 1;
            }
        }
        return CallNextHookEx(hook, Code, wParam, ref lParam);
    }

    ~GlobalKeyboardHook()
    { Unhook(); }
}
