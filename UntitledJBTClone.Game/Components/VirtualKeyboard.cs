using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace UntitledJBTClone.Game.Components
{
    public partial class VirtualKeyboard : Container
    {
        public event Action<char> OnCharacterSelected;
        public event Action<char> OnCharacterPreviewed;
        public event Action OnCursorLeft;
        public event Action OnCursorRight;
        public event Action OnConfirmPressed;
        public event Action OnCancelPressed;
        public event Action OnDeletePressed;

        private readonly string[][] keyboardLayout = new[]
        {
            new[] { "-", "*", "&", "1" }, new[] { "A", "B", "C", "2" }, new[] { "D", "E", "F", "3" }, new[] { "Cancel" },
            new[] { "G", "H", "I", "4" }, new[] { "J", "K", "L", "5" }, new[] { "M", "N", "O", "6" }, new[] { "Delete" },
            new[] { "P", "Q", "R", "S", "7" }, new[] { "T", "U", "V", "8" }, new[] { "W", "X", "Y", "Z", "9" }, new[] { "Select" },
            new[] { "Left" }, new[] { "Right" }, new[] { "0" }, new[] { "Confirm" }
        };

        private readonly int[] buttonStates = new int[16]; // Track current character index for each button
        private int lastPressedButton = -1;
        private readonly GameButton[] buttons = new GameButton[16];

        public VirtualKeyboard()
        {
            AutoSizeAxes = Axes.Both;
            Child = new ButtonGrid(CreateKeyboardButton);
        }

        private GameButton CreateKeyboardButton(int number)
        {
            int index = number - 1; // Convert to 0-based index
            string[] keyOptions = keyboardLayout[index];

            // Create display text based on button type
            string displayText;
            if (keyOptions.Length == 1 && keyOptions[0] == "")
            {
                // Empty buttons
                displayText = "";
            }
            else if (keyOptions.Length == 1)
            {
                // Special buttons (arrows, select, confirm, single numbers)
                displayText = keyOptions[0];
            }
            else
            {
                // Character buttons - show all options together
                displayText = string.Join("", keyOptions);
            }

            var button = new GameButton(displayText);
            button.Action = () => HandleKeyPress(index);
            buttons[index] = button; // Store reference for updating display

            return button;
        }

        private void HandleKeyPress(int buttonIndex)
        {
            string[] keyOptions = keyboardLayout[buttonIndex];

            // Skip empty buttons
            if (keyOptions.Length == 1 && keyOptions[0] == "")
                return;

            switch (buttonIndex)
            {
                case 3: // Cancel button (top right)
                    OnCancelPressed?.Invoke();
                    break;
                case 7: // Delete button (middle right)
                    OnDeletePressed?.Invoke();
                    break;
                case 12: // Left arrow (bottom left)
                    OnCursorLeft?.Invoke();
                    break;
                case 13: // Right arrow
                    OnCursorRight?.Invoke();
                    break;
                case 11: // Select
                    SelectCurrentCharacter();
                    break;
                case 15: // Confirm
                    OnConfirmPressed?.Invoke();
                    break;
                default:
                    // Character buttons - cycle through options
                    CycleButtonCharacter(buttonIndex);
                    break;
            }
        }

        private void CycleButtonCharacter(int buttonIndex)
        {
            string[] keyOptions = keyboardLayout[buttonIndex];
            if (keyOptions.Length > 0)
            {
                // If this is a different button than last pressed, reset its state
                if (lastPressedButton != buttonIndex)
                {
                    buttonStates[buttonIndex] = 0;
                    lastPressedButton = buttonIndex;
                }
                else
                {
                    // Same button, cycle to next character
                    buttonStates[buttonIndex] = (buttonStates[buttonIndex] + 1) % keyOptions.Length;
                }

                // Show preview of current character
                char previewChar = keyOptions[buttonStates[buttonIndex]][0];
                OnCharacterPreviewed?.Invoke(previewChar);

                // Update button display to show current selection
                UpdateButtonDisplay(buttonIndex);
            }
        }

        private void SelectCurrentCharacter()
        {
            if (lastPressedButton >= 0 && lastPressedButton < 16)
            {
                string[] keyOptions = keyboardLayout[lastPressedButton];
                if (keyOptions.Length > 0)
                {
                    char selectedChar = keyOptions[buttonStates[lastPressedButton]][0];
                    OnCharacterSelected?.Invoke(selectedChar);

                    // Reset button display after selection
                    ResetButtonDisplay(lastPressedButton);
                    buttonStates[lastPressedButton] = 0;
                }
            }
        }

        private void UpdateButtonDisplay(int buttonIndex)
        {
            if (buttons[buttonIndex] != null)
            {
                string[] keyOptions = keyboardLayout[buttonIndex];
                string currentChar = keyOptions[buttonStates[buttonIndex]];

                // Show current character highlighted, with other options dimmed
                string displayText = $"[{currentChar}]";
                for (int i = 0; i < keyOptions.Length; i++)
                {
                    if (i != buttonStates[buttonIndex])
                    {
                        displayText += keyOptions[i];
                    }
                }

                // Update button text (this is a simplified approach)
                // In a real implementation, you'd want to update the button's visual state
            }
        }

        private void ResetButtonDisplay(int buttonIndex)
        {
            if (buttons[buttonIndex] != null)
            {
                string[] keyOptions = keyboardLayout[buttonIndex];
                string displayText = string.Join("", keyOptions);
                // Reset to show all options normally
            }
        }
    }
}