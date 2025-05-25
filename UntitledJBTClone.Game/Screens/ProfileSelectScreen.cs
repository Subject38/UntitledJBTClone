using System.Collections.Generic;
using System.Linq;
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
    public partial class ProfileSelectScreen : Screen
    {
        private ProfileService profileService;
        private List<Profile> profiles;
        private int selectedProfileIndex = -1; // -1 means no profile selected
        private int profilePageOffset = 0;
        private const int ProfilesPerPage = 12;
        private Container buttonContainer;
        private SpriteText selectedProfileDisplay;

        [BackgroundDependencyLoader]
        private void load()
        {
            profileService = new ProfileService();
            profiles = profileService.GetAllProfiles();

            InternalChildren = new Drawable[]
            {
                // Background
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                
                // Header
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 100,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Blue,
                        },
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(0, 10),
                            Children = new Drawable[]
                            {
                                new SpriteText
                                {
                                    Text = "Profile Select",
                                    Font = FontUsage.Default.With(size: 32),
                                    Colour = Color4.White,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                                selectedProfileDisplay = new SpriteText
                                {
                                    Text = "No profile selected",
                                    Font = FontUsage.Default.With(size: 20),
                                    Colour = Color4.LightGray,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                }
                            }
                        }
                    }
                },
                
                // Button grid with profiles
                buttonContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new ButtonGrid(CreateProfileSelectButton)
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Y = -40,
                    }
                }
            };
        }



        private void SelectProfileForDisplay(int profileIndex)
        {
            selectedProfileIndex = profileIndex;
            if (profileIndex < profiles.Count)
            {
                var profile = profiles[profileIndex];
                selectedProfileDisplay.Text = $"Selected: {profile.Name}";

                // Recreate button grid to update New/Edit button
                RefreshButtonGrid();
            }
        }

        private void SelectProfile()
        {
            if (selectedProfileIndex != -1 && selectedProfileIndex < profiles.Count)
            {
                var selectedProfile = profiles[selectedProfileIndex];
                profileService.UpdateLastPlayed(selectedProfile.Id);

                // Navigate to music select screen
                this.Push(new MusicSelectScreen(selectedProfile));
            }
        }

        private void CreateNewProfile()
        {
            this.Push(new ProfileCreationScreen());
        }

        private void NavigateProfiles(int direction)
        {
            if (profiles.Count == 0) return;

            selectedProfileIndex = (selectedProfileIndex + direction + profiles.Count) % profiles.Count;
            // Profile selection will be handled by button highlighting
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            // Refresh profile list when returning from profile creation/edit
            profiles = profileService.GetAllProfiles();

            // Reset selection since profiles may have changed
            selectedProfileIndex = -1;
            selectedProfileDisplay.Text = "No profile selected";

            // Refresh the button grid
            RefreshButtonGrid();
        }

        private GameButton CreateProfileSelectButton(int number)
        {
            // Display profiles on buttons 1-12
            if (number >= 1 && number <= 12)
            {
                int profileIndex = profilePageOffset + (number - 1);
                if (profileIndex < profiles.Count)
                {
                    var profile = profiles[profileIndex];
                    var profileButton = new GameButton(profile.Name);
                    profileButton.Action = () => SelectProfileForDisplay(profileIndex);
                    return profileButton;
                }
                else
                {
                    // Empty slot
                    return new GameButton("");
                }
            }

            switch (number)
            {
                case 13: // Left scroll
                    var leftButton = new GameButton("◄");
                    leftButton.Action = () => ScrollProfiles(-ProfilesPerPage);
                    return leftButton;

                case 14: // Right scroll
                    var rightButton = new GameButton("►");
                    rightButton.Action = () => ScrollProfiles(ProfilesPerPage);
                    return rightButton;

                case 15: // New/Edit button
                    if (selectedProfileIndex == -1)
                    {
                        var newButton = new GameButton("New");
                        newButton.Action = CreateNewProfile;
                        return newButton;
                    }
                    else
                    {
                        var editButton = new GameButton("Edit");
                        editButton.Action = EditSelectedProfile;
                        return editButton;
                    }

                case 16: // Select button
                    var selectButton = new GameButton("Select");
                    selectButton.Action = SelectProfile;
                    return selectButton;

                default:
                    return new GameButton("");
            }
        }

        private void SelectProfileByIndex(int index)
        {
            selectedProfileIndex = index;
            SelectProfile();
        }

        private void ScrollProfiles(int direction)
        {
            int newOffset = profilePageOffset + direction;
            if (newOffset >= 0 && newOffset < profiles.Count)
            {
                profilePageOffset = newOffset;

                // Reset selected profile when scrolling
                selectedProfileIndex = -1;
                selectedProfileDisplay.Text = "No profile selected";

                // Recreate button grid with new offset
                RefreshButtonGrid();
            }
        }

        private void EditSelectedProfile()
        {
            if (selectedProfileIndex != -1 && selectedProfileIndex < profiles.Count)
            {
                var profileToEdit = profiles[selectedProfileIndex];
                this.Push(new ProfileEditScreen(profileToEdit));
            }
        }

        private void RefreshButtonGrid()
        {
            buttonContainer.Clear();
            buttonContainer.Add(new ButtonGrid(CreateProfileSelectButton)
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Y = -40,
            });
        }
    }
}