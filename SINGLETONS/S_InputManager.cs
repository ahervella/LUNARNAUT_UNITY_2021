using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_InputManager : Singleton<S_InputManager>
{
    //readonly allows for constants that need to be set at runtime
    private readonly KeyCode[] JUMP_INPUT_KEY = { KeyCode.UpArrow, KeyCode.W, KeyCode.Space };
    private readonly KeyCode[] RIGHT_INPUT_KEY = { KeyCode.RightArrow, KeyCode.D };
    private readonly KeyCode[] LEFT_INPUT_KEY = { KeyCode.LeftArrow, KeyCode.A };
    private readonly KeyCode[] TIME_TRAVEL_KEY = { KeyCode.Q };

    public event System.Action<bool> JumpPressChange = delegate { };
    //private InputWrapper jumpIW = new InputWrapper(new KeyCode[] { KeyCode.UpArrow, KeyCode.W, KeyCode.Space }, JumpPressChange);

    public event System.Action<bool> RightPressChange = delegate { };
    private InputWrapper rightIW;

    public event System.Action<bool> LeftPressChange = delegate { };
    private InputWrapper leftIW;

    public event System.Action<bool> TimeTravelPressChange = delegate { };
    private InputWrapper timeTravelIW;

    private bool aefawef;

    public void Blah()
    {

    }

    private class InputWrapper
    {
        private KeyCode[] keys;
        private bool inputActive = false;
        private System.Action<bool> inputEvent;

        public InputWrapper(KeyCode[] keys, System.Action<bool> inputEvent)
        {
            this.keys = keys;
            this.inputEvent = inputEvent;
        }

        public void UpdateState()
        {
            foreach (KeyCode kc in keys)
            {
                if (Input.GetKey(kc))
                {
                    if (!inputActive)
                    {
                        inputActive = true;
                        inputEvent.Invoke(true);
                    }
                    return;
                }
            }

            if (inputActive)
            {
                inputActive = false;
                inputEvent.Invoke(false);
            }
        }
    }

    private bool controlsEnabled = true;
    public event System.Action ControlsEnabledChanged = delegate { };
    public bool ControlsEnabled
    {
        get => controlsEnabled;
        set
        {
            bool prevVal = controlsEnabled;
            controlsEnabled = value;
            if (prevVal != value)
            {
                ControlsEnabledChanged();
                string logText = value ? "Astro controls enabled." : "Astro controls disabled.";
                Debug.Log(logText);
            }
        }
    }

    private void Update()
    {
       
    }

    private void blah(ref bool cachedState, KeyCode key)
    {
        //if (Input.GetKey(KeyCode.A) != )
        //{
          //  Debug.Log("Well I'll be damned");
        //}
    }
}
