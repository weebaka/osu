﻿using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Audio.Sample;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Overlays.Pause
{
    public class PauseButton : ClickableContainer
    {
        private const float hoverWidth = 0.9f;
        private const float hoverDuration = 500;
        private const float glowFadeDuration = 250;
        private const float clickDuration = 200;

        private Color4 backgroundColour = OsuColour.Gray(34);

        private Color4 buttonColour;
        public Color4 ButtonColour
        {
            get
            {
                return buttonColour;
            }
            set
            {
                buttonColour = value;
                updateGlow();
                if (colourContainer == null) return;
                colourContainer.Colour = ButtonColour;
            }
        }

        private string text;
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                if (spriteText == null) return;
                spriteText.Text = Text;
            }
        }

        public AudioSample SampleClick, SampleHover;

        private Container backgroundContainer, colourContainer, glowContainer;
        private Box leftGlow, centerGlow, rightGlow;
        private SpriteText spriteText;

        private bool didClick; // Used for making sure that the OnMouseDown animation can call instead of OnHoverLost's when clicking

        public override bool Contains(Vector2 screenSpacePos) => backgroundContainer.Contains(screenSpacePos);

        protected override bool OnMouseDown(Framework.Input.InputState state, MouseDownEventArgs args)
        {
            didClick = true;
            colourContainer.ResizeTo(new Vector2(1.5f, 1f), clickDuration, EasingTypes.In);
            flash();
            SampleClick?.Play();
            Action?.Invoke();

            Delay(clickDuration);
            Schedule(delegate {
                colourContainer.ResizeTo(new Vector2(0.8f, 1f), 0, EasingTypes.None);
                spriteText.Spacing = Vector2.Zero;
                glowContainer.FadeOut();
            });

            return true;
        }

        protected override bool OnClick(Framework.Input.InputState state) => false;

        protected override bool OnHover(Framework.Input.InputState state)
        {
            colourContainer.ResizeTo(new Vector2(hoverWidth, 1f), hoverDuration, EasingTypes.OutElastic);
            spriteText.TransformSpacingTo(new Vector2(3f, 0f), hoverDuration, EasingTypes.OutElastic);
            glowContainer.FadeIn(glowFadeDuration, EasingTypes.Out);
            SampleHover?.Play();
            return true;
        }

        protected override void OnHoverLost(Framework.Input.InputState state)
        {
            if (!didClick)
            {
                colourContainer.ResizeTo(new Vector2(0.8f, 1f), hoverDuration, EasingTypes.OutElastic);
                spriteText.TransformSpacingTo(Vector2.Zero, hoverDuration, EasingTypes.OutElastic);
                glowContainer.FadeOut(glowFadeDuration, EasingTypes.Out);
            }

            didClick = false;
        }

        private void flash()
        {
            var flash = new Box
            {
                RelativeSizeAxes = Axes.Both
            };

            colourContainer.Add(flash);

            flash.Colour = ButtonColour;
            flash.BlendingMode = BlendingMode.Additive;
            flash.Alpha = 0.3f;
            flash.FadeOutFromOne(clickDuration);
            flash.Expire();
        }

        private void updateGlow()
        {
            leftGlow.ColourInfo = ColourInfo.GradientHorizontal(new Color4(ButtonColour.R, ButtonColour.G, ButtonColour.B, 0f), ButtonColour);
            centerGlow.Colour = ButtonColour;
            rightGlow.ColourInfo = ColourInfo.GradientHorizontal(ButtonColour, new Color4(ButtonColour.R, ButtonColour.G, ButtonColour.B, 0f));
        }

        public PauseButton()
        {
            Children = new Drawable[]
            {
                backgroundContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = backgroundColour
                        }
                    }
                },
                glowContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1f,
                    Alpha = 0f,
                    Children = new Drawable[]
                    {
                        leftGlow = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            Width = 0.125f
                        },
                        centerGlow = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Width = 0.75f
                        },
                        rightGlow = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Width = 0.125f
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        colourContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Width = 0.8f,
                            Masking = true,
                            MaskingSmoothness = 2,
                            EdgeEffect = new EdgeEffect
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.2f),
                                Radius = 5
                            },
                            Colour = ButtonColour,
                            Shear = new Vector2(0.2f, 0),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    EdgeSmoothness = new Vector2(2, 0),
                                    RelativeSizeAxes = Axes.Both
                                },
                                new Triangles
                                {
                                    BlendingMode = BlendingMode.Additive,
                                    RelativeSizeAxes = Axes.Both,
                                    TriangleScale = 4,
                                    Alpha = 0.05f,
                                    Shear = new Vector2(-0.2f, 0)
                                }
                            }
                        }
                    }
                },
                spriteText = new SpriteText
                {
                    Text = Text,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = 28,
                    Font = "Exo2.0-Bold",
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.1f),
                    Colour = Color4.White
                }
            };

            updateGlow();
        }
    }
}
