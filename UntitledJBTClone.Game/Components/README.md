# Customizable Button System

This document explains how to use the flexible button system in UntitledJBTClone.

## GameButton Component

The `GameButton` class provides multiple ways to create buttons with custom content.

### Constructor Options

```csharp
// 1. Custom drawable content (most flexible)
new GameButton(myCustomDrawable)

// 2. Simple text button
new GameButton("Click Me")

// 3. Numbered button (backward compatibility)
new GameButton(1)
```

### Examples

#### Simple Text Button
```csharp
var playButton = new GameButton("Play");
var optionsButton = new GameButton("Options");
```

#### Animated Button
```csharp
var animatedContent = new Circle
{
    Anchor = Anchor.Centre,
    Origin = Anchor.Centre,
    Size = new Vector2(60, 60),
    Colour = Color4.Red,
};
animatedContent.Loop(c => c.ScaleTo(1.2f, 1000).Then().ScaleTo(1.0f, 1000));

var animatedButton = new GameButton(animatedContent);
```

#### Complex Multi-Element Button
```csharp
var complexContent = new FillFlowContainer
{
    Anchor = Anchor.Centre,
    Origin = Anchor.Centre,
    Direction = FillDirection.Vertical,
    AutoSizeAxes = Axes.Both,
    Children = new Drawable[]
    {
        new SpriteText { Text = "Level 1", Colour = Color4.White },
        new Circle { Size = new Vector2(20, 20), Colour = Color4.Gold }
    }
};

var complexButton = new GameButton(complexContent);
```

## ButtonGrid Component

The `ButtonGrid` class creates a 4x4 grid of buttons with customizable content.

### Constructor Options

```csharp
// 1. Default numbered buttons (1-16)
new ButtonGrid()

// 2. Custom button factory
new ButtonGrid(buttonNumber => CreateMyCustomButton(buttonNumber))
```

### Examples

#### Default Grid
```csharp
var defaultGrid = new ButtonGrid();
```

#### Custom Text Grid
```csharp
var textGrid = new ButtonGrid(number => new GameButton($"Button {number}"));
```

#### Mixed Content Grid
```csharp
var mixedGrid = new ButtonGrid(number => 
{
    if (number <= 4)
        return new GameButton($"Level {number}");
    else if (number <= 8)
        return new GameButton(CreateAnimatedContent(number));
    else
        return new GameButton(number); // Default numbered
});
```

#### Game-Specific Grid
```csharp
var gameGrid = new ButtonGrid(number =>
{
    var gameData = GetGameDataForButton(number);
    return new GameButton(CreateGameSpecificContent(gameData));
});
```

## Grid Specifications

- **Grid Size**: 4x4 (16 buttons total)
- **Button Size**: 160x160 pixels each
- **Button Gaps**: 37px horizontally and vertically
- **Edge Margins**: 8px horizontal, 6px vertical
- **Total Grid Size**: 767x763 pixels

## Animation Support

Buttons can contain any drawable content, including:

- **Pulsing effects**: `ScaleTo()` animations
- **Rotation**: `RotateTo()` animations
- **Color changes**: `FadeColour()` animations
- **Movement**: `MoveTo()` and `MoveToOffset()` animations
- **Complex sequences**: Chained animations with `Then()`
- **Looping**: `Loop()` for continuous animations

## Best Practices

1. **Performance**: Avoid too many complex animations running simultaneously
2. **Consistency**: Use similar animation timings across buttons for cohesive feel
3. **Accessibility**: Ensure text remains readable with animations
4. **Responsiveness**: Keep button content within the 160x160 pixel bounds
5. **State Management**: Consider how button content should change based on game state

## Future Extensions

The system is designed to support:

- **Sprite-based animations**: Loading and displaying sprite sheets
- **Particle effects**: Adding particle systems to buttons
- **Dynamic content**: Updating button content based on real-time data
- **Custom themes**: Swapping entire button styles
- **Sound integration**: Adding audio feedback to button interactions 