using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;
using UntitledJBTClone.Game.Components;
using UntitledJBTClone.Game.Models;
using UntitledJBTClone.Game.Services;

namespace UntitledJBTClone.Game.Screens
{
    public partial class ProfileEditScreen : Screen
    {
        private ProfileService profileService;
        private Profile profile;
        private SpriteText nameDisplay;
        private string currentName = "";
        private const int MaxNameLength = 20;
        private int cursorPosition = 0;
        private char[] nameBuffer = new char[MaxNameLength];
        private char previewCharacter = '\0';
        private VirtualKeyboard virtualKeyboard;

        public ProfileEditScreen(Profile profileToEdit)
        {
            profile = profileToEdit;
            currentName = profile.Name;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            profileService = new ProfileService();

            // Initialize name buffer with existing name
            for (int i = 0; i < MaxNameLength; i++)
            {
                if (i < currentName.Length)
                    nameBuffer[i] = currentName[i];
                else
                    nameBuffer[i] = '\0';
            }

            // Create and configure virtual keyboard
            virtualKeyboard = new VirtualKeyboard
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Y = -40,
            };

            // Set up keyboard events
            virtualKeyboard.OnCharacterSelected += OnCharacterSelected;
            virtualKeyboard.OnCharacterPreviewed += OnCharacterPreviewed;
            virtualKeyboard.OnCursorLeft += OnCursorLeft;
            virtualKeyboard.OnCursorRight += OnCursorRight;
            virtualKeyboard.OnConfirmPressed += OnConfirmPressed;
            virtualKeyboard.OnCancelPressed += OnCancelPressed;
            virtualKeyboard.OnDeletePressed += OnDeletePressed;

            InternalChildren = new Drawable[]
            {
                // Background
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                
                // Main container
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        // Header section
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 200,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                // Blue header background
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Blue,
                                },
                                
                                // Header content
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Vertical,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    Spacing = new Vector2(0, 20),
                                    Children = new Drawable[]
                                    {
                                        new SpriteText
                                        {
                                            Text = "Edit Profile:",
                                            Font = FontUsage.Default.With(size: 24),
                                            Colour = Color4.White,
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                        },
                                        
                                        // Name input display
                                        new Container
                                        {
                                            Size = new Vector2(300, 40),
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = Color4.White,
                                                },
                                                nameDisplay = new SpriteText
                                                {
                                                    Text = "",
                                                    Font = FontUsage.Default.With(size: 32),
                                                    Colour = Color4.Black,
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Margin = new MarginPadding { Left = 10 },
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        
                        // Virtual keyboard
                        virtualKeyboard
                    }
                }
            };

            UpdateNameDisplay();
        }

        // Keyboard event handlers
        private void OnCharacterSelected(char character)
        {
            SetCharacterAtCursor(character);
            previewCharacter = '\0'; // Clear preview
            MoveCursorRight();
        }

        private void OnCharacterPreviewed(char character)
        {
            previewCharacter = character;
            UpdateNameDisplay();
        }

        private void OnCursorLeft()
        {
            previewCharacter = '\0'; // Clear preview when moving cursor
            MoveCursorLeft();
        }

        private void OnCursorRight()
        {
            previewCharacter = '\0'; // Clear preview when moving cursor
            MoveCursorRight();
        }

        private void OnConfirmPressed()
        {
            SaveProfile();
        }

        private void OnCancelPressed()
        {
            this.Exit();
        }

        private void OnDeletePressed()
        {
            try
            {
                // Delete the profile from database
                profileService.DeleteProfile(profile.Id);

                // Navigate back to profile select screen
                this.Exit();
            }
            catch (Exception ex)
            {
                // Handle error (show message to user)
                Console.WriteLine($"Error deleting profile: {ex.Message}");
            }
        }

        // Name editing methods
        private void SetCharacterAtCursor(char character)
        {
            if (cursorPosition < MaxNameLength)
            {
                nameBuffer[cursorPosition] = character;
                UpdateNameFromBuffer();
            }
        }

        private void MoveCursorLeft()
        {
            if (cursorPosition > 0)
            {
                cursorPosition--;
                UpdateNameDisplay();
            }
        }

        private void MoveCursorRight()
        {
            if (cursorPosition < MaxNameLength - 1)
            {
                cursorPosition++;
                UpdateNameDisplay();
            }
        }

        private void UpdateNameFromBuffer()
        {
            currentName = new string(nameBuffer).TrimEnd('\0');
            UpdateNameDisplay();
        }

        private void UpdateNameDisplay()
        {
            // Build display string with cursor always shown in brackets
            var displayText = "";
            for (int i = 0; i < MaxNameLength; i++)
            {
                if (i == cursorPosition)
                {
                    // Always show brackets at cursor position
                    if (previewCharacter != '\0')
                    {
                        // Show preview character in brackets
                        displayText += $"[{previewCharacter}]";
                    }
                    else if (i < currentName.Length)
                    {
                        // Show existing character in brackets
                        displayText += $"[{currentName[i]}]";
                    }
                    else
                    {
                        // Show empty brackets for cursor
                        displayText += "[ ]";
                    }
                }
                else if (i < currentName.Length)
                {
                    displayText += currentName[i];
                }
                else
                {
                    displayText += " ";
                }
            }
            nameDisplay.Text = displayText;
        }

        private void SaveProfile()
        {
            if (string.IsNullOrWhiteSpace(currentName))
            {
                // Show error - name cannot be empty
                return;
            }

            try
            {
                // Check if name already exists (and it's not the current profile)
                if (profileService.ProfileExists(currentName) && currentName != profile.Name)
                {
                    // Show error - profile already exists
                    return;
                }

                // Update profile name in database
                profileService.UpdateProfileName(profile.Id, currentName);

                // Navigate back to profile select screen
                this.Exit();
            }
            catch (Exception ex)
            {
                // Handle error (show message to user)
                Console.WriteLine($"Error updating profile: {ex.Message}");
            }
        }
    }
}