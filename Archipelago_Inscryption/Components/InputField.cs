using DiskCardGame;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Archipelago_Inscryption.Components
{
    internal class InputField : MainInputInteractable
    {
        public static bool IsAnySelected 
        {
            get
            {
                foreach (InputField inputField in allInputFields)
                {
                    if (inputField.keyboardInput.enabled) return true;
                }

                return false;
            }
        }

        private static List<InputField> allInputFields = new List<InputField>();

        internal string Label
        {
            get { return label.text; }
            set { label.text = value; }
        }

        internal string Text
        {
            get { return realText; }
            set 
            { 
                realText = value; 
                DisplayText(realText);
                keyboardInput.KeyboardInput = value;
            }
        }
        internal void DisplayText(string original, bool withCursor = false)
        {
            var str = (censor ? new string('*', original.Length) : original) + (withCursor ? "|" : " ");
            text.text = str;
            Canvas.ForceUpdateCanvases(); // update text sizing
            var maxWidth = withCursor ? 174 : 176;
            if (text.preferredWidth > maxWidth)
            {
                text.alignment = TextAnchor.MiddleRight;
                if (withCursor)
                {
                    text.transform.localPosition = new(-2, 0);
                }
                else
                {
                    text.transform.localPosition = Vector2.zero;
                }
            }
            else
            {
                text.alignment = TextAnchor.MiddleLeft;
            }
            while (text.preferredWidth > maxWidth)
            {
                str = str.Substring(1);
                text.text = "..." + str;
                Canvas.ForceUpdateCanvases(); // update text sizing
            }
        }

        internal int CharacterLimit
        {
            get { return keyboardInput.maxInputLength; }
            set { keyboardInput.maxInputLength = value; }
        }

        internal bool Censor
        {
            get { return censor; }
            set { censor = value; }
        }

        public override bool CollisionIs2D => true;

        [SerializeField]
        private Text label;

        [SerializeField]
        private Text text;

        private KeyboardInputHandler keyboardInput;
        private string realText;
        private bool censor;

        private bool isPointerInside = false;
        public Action<string> OnSubmit;

        private void Awake()
        {
            keyboardInput = GetComponent<KeyboardInputHandler>();

            if (keyboardInput == null )
                keyboardInput = gameObject.AddComponent<KeyboardInputHandler>();

            keyboardInput.allowPasteClipboard = true;
            keyboardInput.maxInputLength = 30;
            keyboardInput.enabled = false;
            keyboardInput.EnterPressed += OnEnterPressed;

            if (label == null)
                label = transform.Find("Title/Text").GetComponent<Text>();

            if (text == null)
                text = transform.Find("TextFrame/Text/Text").GetComponent<Text>();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            allInputFields.Add(this);
        }

        private void OnDisable()
        {
            allInputFields.Remove(this);
        }

        private void OnEnterPressed()
        {
            if (OnSubmit is not null)
            {
                OnSubmit(Text);
            }
            else
            {
                keyboardInput.enabled = false;
            }
        }

        public override void ManagedUpdate()
        {
            base.ManagedUpdate();

            if (InputButtons.GetButtonDown(Button.Select))
            {
                if (isPointerInside)
                {
                    keyboardInput.enabled = true;
                }
                else
                {
                    keyboardInput.enabled = false;
                }
            }

            if (!keyboardInput.enabled)
            {
                DisplayText(realText);
                return;
            }

            realText = keyboardInput.KeyboardInput;

            bool showTextCursor = ((int)(Time.timeSinceLevelLoad * 2)) % 2 > 0;

            DisplayText(realText, showTextCursor);
        }

        public override void OnCursorEnter()
        {
            isPointerInside = true;
        }

        public override void OnCursorExit()
        {
            isPointerInside = false;
        }
    }
}
