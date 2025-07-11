# EasyEnglish.Theme

[![NuGet Version](https://img.shields.io/nuget/v/EasyEnglish.Theme.svg)](https://www.nuget.org/packages/EasyEnglish.Theme/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EasyEnglish.Theme.svg)](https://www.nuget.org/packages/EasyEnglish.Theme/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Comprehensive theme system for .NET MAUI Blazor applications with modern dark UI and comprehensive component library.**

Адаптована з React Native системи стилів, ця тема надає потужні інструменти для створення консистентного і красивого користувацького інтерфейсу в MAUI Blazor додатках.

## ✨ Features

- 🎨 **Modern Dark Theme** - Професійна темна тема з акцентними кольорами
- 📱 **Responsive Design** - Автоматична адаптація під різні розміри екранів
- 🧩 **Rich Component Library** - Готові компоненти для швидкої розробки
- 🌐 **Language Learning Support** - Спеціальні кольори для рівнів (A1-C2) та частин мови
- ⚡ **Performance Optimized** - CSS змінні та ефективний rendering
- 🔧 **Highly Customizable** - Легка кастомізація через configuration
- 📦 **TypeScript-like Constants** - Type-safe константи для C#
- 🎯 **Utility Classes** - Extensive utility classes для spacing, layout, typography

## 🚀 Quick Start

### Installation

```bash
dotnet add package EasyEnglish.Theme
```

### Basic Setup

1. **Configure in MauiProgram.cs:**

```csharp
using EasyEnglish.Theme.Extensions;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        // ... other MAUI configuration
        
        // Add theme
        builder.Services.AddYourAppTheme();
        
        return builder.Build();
    }
}
```

2. **Update wwwroot/index.html:**

```html
<head>
    <!-- Add theme CSS -->
    <link href="_content/EasyEnglish.Theme/css/AppTheme.css" rel="stylesheet" />
</head>
<body>
    <!-- Add theme JavaScript -->
    <script src="_content/EasyEnglish.Theme/js/theme-utils.js"></script>
</body>
```

3. **Wrap your app with ThemeProvider:**

```razor
@* Components/App.razor *@
<EasyEnglish.Theme.Components.ThemeProvider>
    <Router AppAssembly="@typeof(App).Assembly">
        <!-- Your routing content -->
    </Router>
</EasyEnglish.Theme.Components.ThemeProvider>
```

4. **Add imports:**

```razor
@* Components/_Imports.razor *@
@using EasyEnglish.Theme.Components
@using EasyEnglish.Theme.Constants
@using EasyEnglish.Theme.Services
```

## 🎨 Components

### Buttons

```razor
<!-- Different button variants -->
<Button Variant="ButtonVariant.Primary">Primary Button</Button>
<Button Variant="ButtonVariant.Secondary">Secondary Button</Button>
<Button Variant="ButtonVariant.Outline">Outline Button</Button>
<Button Variant="ButtonVariant.Success">Success Button</Button>

<!-- With icons and click handlers -->
<Button Variant="ButtonVariant.Primary" 
        Icon="fas fa-save" 
        OnClick="HandleSave">
    Save Changes
</Button>
```

### Cards

```razor
<!-- Simple card -->
<Card Title="My Card">
    <p>Card content goes here</p>
</Card>

<!-- Different sizes -->
<Card Size="CardSize.Large" Title="Large Card">
    <p>Large card with more padding</p>
</Card>
```

### Input Fields

```razor
<!-- Text input -->
<Input Label="Name" 
       Placeholder="Enter your name"
       @bind-Value="name" />

<!-- Multiline input -->
<Input Label="Description" 
       Multiline="true"
       Rows="4"
       @bind-Value="description" />

<!-- With validation -->
<Input Label="Email" 
       Type="email"
       @bind-Value="email"
       HasError="@(!string.IsNullOrEmpty(emailError))"
       ErrorMessage="@emailError" />
```

### Badges

```razor
<!-- Language level badges -->
<Badge Variant="BadgeVariant.Level" Level="A1">A1</Badge>
<Badge Variant="BadgeVariant.Level" Level="B2">B2</Badge>
<Badge Variant="BadgeVariant.Level" Level="C1">C1</Badge>

<!-- Part of speech badges -->
<Badge PartOfSpeech="noun">Noun</Badge>
<Badge PartOfSpeech="verb">Verb</Badge>
<Badge PartOfSpeech="adjective">Adjective</Badge>
```

### Typography

```razor
<H1>Main Heading</H1>
<H2>Secondary Heading</H2>
<H3>Tertiary Heading</H3>

<!-- With custom CSS classes -->
<H1 CssClass="@ThemeConstants.CssClasses.TextCenter">
    Centered Heading
</H1>
```

### Layout

```razor
<!-- Container types -->
<Container Type="ContainerType.Screen">
    <p>Full screen container</p>
</Container>

<Container Type="ContainerType.ScreenCentered">
    <p>Centered content</p>
</Container>
```

### Progress & Loading

```razor
<!-- Progress bar -->
<ProgressBar Progress="75" />

<!-- Loading spinner -->
<Loading Message="Loading your data..." />
```

## 🎛️ Advanced Configuration

### Custom Theme Options

```csharp
builder.Services.AddYourAppTheme(options =>
{
    // Enable debug mode
    options.EnableDebugMode = true;
    
    // Custom colors
    options.CustomColors.Add("color-primary", "#FF6B35");
    options.CustomColors.Add("color-secondary", "#004E89");
    
    // Custom breakpoints
    options.Breakpoints = new CustomBreakpoints
    {
        Mobile = 480,
        Tablet = 768,
        Desktop = 1200,
        LargeDesktop = 1600
    };
    
    // CSS prefix (to avoid conflicts)
    options.CssPrefix = "myapp-";
});
```

### Using Services

```razor
@page "/responsive-demo"
@inject IResponsiveService ResponsiveService
@inject IColorService ColorService

<div>
    <p>Current screen: @ResponsiveService.CurrentScreenSize</p>
    <p>Is mobile: @ResponsiveService.IsMobile</p>
</div>

@code {
    protected override void OnInitialized()
    {
        // Subscribe to screen size changes
        ResponsiveService.ScreenSizeChanged += OnScreenSizeChanged;
    }
    
    private void OnScreenSizeChanged(ScreenSize newSize)
    {
        InvokeAsync(StateHasChanged);
    }
    
    private void GetPartOfSpeechColor()
    {
        var color = ColorService.GetPartOfSpeechColor("noun");
        // Returns: var(--color-noun)
    }
}
```

## 📐 Utility Classes

### Layout

```razor
<!-- Flexbox utilities -->
<div class="@ThemeConstants.CssClasses.Flex @ThemeConstants.CssClasses.FlexCenter">
    Centered content
</div>

<div class="@ThemeConstants.CssClasses.FlexRow @ThemeConstants.CssClasses.FlexSpaceBetween">
    <span>Left</span>
    <span>Right</span>
</div>
```

### Spacing

```razor
<!-- Margin utilities -->
<div class="@ThemeConstants.CssClasses.Margin.BottomLg">
    Large bottom margin
</div>

<!-- Padding utilities -->
<div class="@ThemeConstants.CssClasses.Padding.HorizontalMd">
    Medium horizontal padding
</div>
```

### Typography

```razor
<!-- Text colors -->
<p class="@ThemeConstants.CssClasses.TextPrimary">Primary text</p>
<p class="@ThemeConstants.CssClasses.TextSecondary">Secondary text</p>
<p class="@ThemeConstants.CssClasses.TextSuccess">Success text</p>

<!-- Text alignment -->
<p class="@ThemeConstants.CssClasses.TextCenter">Centered text</p>
```

## 🎨 Color System

### Semantic Colors

- **Primary**: `#6366F1` (Indigo)
- **Secondary**: `#EC4899` (Pink)  
- **Accent**: `#06B6D4` (Cyan)
- **Success**: `#10B981` (Green)
- **Warning**: `#F59E0B` (Amber)
- **Error**: `#EF4444` (Red)
- **Info**: `#3B82F6` (Blue)

### Language Learning Colors

- **A1**: `#10B981` (Green)
- **A2**: `#06B6D4` (Cyan)
- **B1**: `#3B82F6` (Blue)
- **B2**: `#8B5CF6` (Purple)
- **C1**: `#F59E0B` (Orange)
- **C2**: `#EF4444` (Red)

### Part of Speech Colors

- **Noun**: `#6366F1` (Indigo)
- **Verb**: `#10B981` (Green)
- **Adjective**: `#F59E0B` (Amber)
- **Adverb**: `#8B5CF6` (Purple)
- **Phrase**: `#06B6D4` (Cyan)
- **Idiom**: `#F97316` (Orange)

## 📱 Responsive Design

The theme includes responsive breakpoints:

- **Mobile**: < 768px
- **Tablet**: 768px - 1024px
- **Desktop**: 1024px - 1440px
- **Large Desktop**: > 1440px

### Responsive Utilities

```razor
<!-- Responsive classes -->
<div class="flex-column tablet:flex-row">
    Column on mobile, row on tablet+
</div>

<!-- Programmatic responsive -->
<div class="@ResponsiveService.GetResponsiveClass("text-sm", "text-base", "text-lg")">
    Responsive text size
</div>
```

## 🔧 Customization

### CSS Variables

All colors and spacing use CSS variables, making customization easy:

```css
:root {
    --color-primary: #your-color;
    --spacing-md: 20px;
    --border-radius-lg: 16px;
}
```

### Custom Components

Create your own components using the theme system:

```razor
@inherits ComponentBase
@using EasyEnglish.Theme.Constants

<div class="@GetCardClasses()">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public bool IsHighlighted { get; set; }
    
    private string GetCardClasses()
    {
        var classes = new List<string> { ThemeConstants.CssClasses.Card };
        
        if (IsHighlighted)
            classes.Add("highlighted-card");
            
        return string.Join(" ", classes);
    }
}
```

## 🛠️ Development

### Building from Source

```bash
git clone https://github.com/yourorg/easyenglish-theme.git
cd easyenglish-theme
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Creating NuGet Package

```bash
dotnet pack --configuration Release
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📞 Support

- 🐛 **Bug Reports**: [GitHub Issues](https://github.com/yourorg/easyenglish-theme/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/yourorg/easyenglish-theme/discussions)
- 📖 **Documentation**: [Wiki](https://github.com/yourorg/easyenglish-theme/wiki)

## 🗺️ Roadmap

- [ ] Light theme support
- [ ] More component variants
- [ ] Animation system
- [ ] RTL language support
- [ ] Accessibility improvements
- [ ] Storybook integration

## 📋 Changelog

### v1.0.0
- Initial release
- Complete dark theme
- Responsive design system
- Language learning color scheme
- 15+ ready-to-use components
- TypeScript-like constants for C#

---

**Made with ❤️ for the .NET MAUI community**