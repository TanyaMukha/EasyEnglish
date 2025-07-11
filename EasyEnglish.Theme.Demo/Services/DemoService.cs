using EasyEnglish.Theme.Demo.Interfaces;
using EasyEnglish.Theme.Demo.Models;
using EasyEnglish.Theme.Types;

namespace EasyEnglish.Theme.Demo.Services;

public class DemoService : IDemoService
{
    public List<ComponentDemoModel> GetComponentDemos()
    {
        return new List<ComponentDemoModel>
        {
            new ComponentDemoModel
            {
                Id = "button",
                Name = "Button",
                Description = "Interactive button component with multiple variants and sizes",
                Category = "Interactive",
                Variants = new List<ComponentVariantModel>
                {
                    new ComponentVariantModel
                    {
                        Name = "Primary Button",
                        Code = """<Button Variant="ButtonVariant.Primary">Primary Button</Button>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Primary" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Secondary Button",
                        Code = """<Button Variant="ButtonVariant.Secondary">Secondary Button</Button>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Secondary" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Accent Button",
                        Code = """<Button Variant="ButtonVariant.Accent">Accent Button</Button>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Accent" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Success Button",
                        Code = """<Button Variant="ButtonVariant.Success">Success Button</Button>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Success" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Warning Button",
                        Code = """<Button Variant="ButtonVariant.Warning">Warning Button</Button>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Warning" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Error Button",
                        Code = """<Button Variant="ButtonVariant.Error">Error Button</Button>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Error" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Outline Button",
                        Code = """<Button Variant="ButtonVariant.Outline">Outline Button</Button>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Outline" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Ghost Button",
                        Code = """<Button Variant="ButtonVariant.Ghost">Ghost Button</Button>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Ghost" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Disabled Button",
                        Code = """<Button Variant="ButtonVariant.Primary" Disabled="true">Disabled Button</Button>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Primary", ["Disabled"] = true }
                    }
                }
            },
            new ComponentDemoModel
            {
                Id = "card",
                Name = "Card",
                Description = "Container component for grouping related content",
                Category = "Layout",
                Variants = new List<ComponentVariantModel>
                {
                    new ComponentVariantModel
                    {
                        Name = "Basic Card",
                        Code = """<Card Title="Basic Card"><p>Card content goes here</p></Card>""",
                        Props = new Dictionary<string, object> { ["Title"] = "Basic Card" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Small Card",
                        Code = """<Card Size="CardSize.Small" Title="Small Card"><p>Small card content</p></Card>""",
                        Props = new Dictionary<string, object> { ["Size"] = "Small", ["Title"] = "Small Card" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Large Card",
                        Code = """<Card Size="CardSize.Large" Title="Large Card"><p>Large card content</p></Card>""",
                        Props = new Dictionary<string, object> { ["Size"] = "Large", ["Title"] = "Large Card" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Card Without Title",
                        Code = """<Card><p>Card content without title</p></Card>""",
                        Props = new Dictionary<string, object>()
                    }
                }
            },
            new ComponentDemoModel
            {
                Id = "input",
                Name = "Input",
                Description = "Form input component with validation support",
                Category = "Forms",
                Variants = new List<ComponentVariantModel>
                {
                    new ComponentVariantModel
                    {
                        Name = "Basic Input",
                        Code = """<Input Label="Name" Placeholder="Enter your name" />""",
                        Props = new Dictionary<string, object> { ["Label"] = "Name", ["Placeholder"] = "Enter your name" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Email Input",
                        Code = """<Input Label="Email" Type="email" Placeholder="Enter your email" />""",
                        Props = new Dictionary<string, object> { ["Label"] = "Email", ["Type"] = "email", ["Placeholder"] = "Enter your email" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Multiline Input",
                        Code = """<Input Label="Description" Multiline="true" Rows="4" Placeholder="Enter description..." />""",
                        Props = new Dictionary<string, object> { ["Label"] = "Description", ["Multiline"] = true, ["Rows"] = 4, ["Placeholder"] = "Enter description..." }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Input with Error",
                        Code = """<Input Label="Email" HasError="true" ErrorMessage="Please enter a valid email" />""",
                        Props = new Dictionary<string, object> { ["Label"] = "Email", ["HasError"] = true, ["ErrorMessage"] = "Please enter a valid email" }
                    }
                }
            },
            new ComponentDemoModel
            {
                Id = "badge",
                Name = "Badge",
                Description = "Small status indicators and labels",
                Category = "Display",
                Variants = new List<ComponentVariantModel>
                {
                    new ComponentVariantModel
                    {
                        Name = "Level A1 Badge",
                        Code = """<Badge Variant="BadgeVariant.Level" Level="A1">A1</Badge>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Level", ["Level"] = "A1" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Level B2 Badge",
                        Code = """<Badge Variant="BadgeVariant.Level" Level="B2">B2</Badge>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Level", ["Level"] = "B2" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Level C1 Badge",
                        Code = """<Badge Variant="BadgeVariant.Level" Level="C1">C1</Badge>""",
                        Props = new Dictionary<string, object> { ["Variant"] = "Level", ["Level"] = "C1" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Noun Badge",
                        Code = """<Badge PartOfSpeech="noun">Noun</Badge>""",
                        Props = new Dictionary<string, object> { ["PartOfSpeech"] = "noun" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Verb Badge",
                        Code = """<Badge PartOfSpeech="verb">Verb</Badge>""",
                        Props = new Dictionary<string, object> { ["PartOfSpeech"] = "verb" }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Adjective Badge",
                        Code = """<Badge PartOfSpeech="adjective">Adjective</Badge>""",
                        Props = new Dictionary<string, object> { ["PartOfSpeech"] = "adjective" }
                    }
                }
            },
            new ComponentDemoModel
            {
                Id = "loading",
                Name = "Loading",
                Description = "Loading spinner component with optional message",
                Category = "Feedback",
                Variants = new List<ComponentVariantModel>
                {
                    new ComponentVariantModel
                    {
                        Name = "Basic Loading",
                        Code = """<Loading />""",
                        Props = new Dictionary<string, object>()
                    },
                    new ComponentVariantModel
                    {
                        Name = "Loading with Message",
                        Code = """<Loading Message="Loading your content..." />""",
                        Props = new Dictionary<string, object> { ["Message"] = "Loading your content..." }
                    }
                }
            },
            new ComponentDemoModel
            {
                Id = "progressbar",
                Name = "ProgressBar",
                Description = "Progress indicator component",
                Category = "Feedback",
                Variants = new List<ComponentVariantModel>
                {
                    new ComponentVariantModel
                    {
                        Name = "25% Progress",
                        Code = """<ProgressBar Progress="25" />""",
                        Props = new Dictionary<string, object> { ["Progress"] = 25.0 }
                    },
                    new ComponentVariantModel
                    {
                        Name = "50% Progress",
                        Code = """<ProgressBar Progress="50" />""",
                        Props = new Dictionary<string, object> { ["Progress"] = 50.0 }
                    },
                    new ComponentVariantModel
                    {
                        Name = "75% Progress",
                        Code = """<ProgressBar Progress="75" />""",
                        Props = new Dictionary<string, object> { ["Progress"] = 75.0 }
                    },
                    new ComponentVariantModel
                    {
                        Name = "Custom Range Progress",
                        Code = """<ProgressBar Progress="30" Min="0" Max="100" />""",
                        Props = new Dictionary<string, object> { ["Progress"] = 30.0, ["Min"] = 0.0, ["Max"] = 100.0 }
                    }
                }
            }
        };
    }

    public ComponentDemoModel? GetDemoById(string id)
    {
        return GetComponentDemos().FirstOrDefault(d => d.Id == id);
    }

    public List<ColorPaletteModel> GetColorPalettes()
    {
        return new List<ColorPaletteModel>
        {
            new ColorPaletteModel
            {
                Name = "Primary Colors",
                Description = "Main brand colors used throughout the application",
                Colors = new List<ColorItemModel>
                {
                    new ColorItemModel { Name = "Primary", Hex = "#6366F1", CssVar = "--color-primary", Usage = "Primary actions, links" },
                    new ColorItemModel { Name = "Primary Light", Hex = "#8B8CF8", CssVar = "--color-primary-light", Usage = "Hover states, light variants" },
                    new ColorItemModel { Name = "Primary Dark", Hex = "#4F46E5", CssVar = "--color-primary-dark", Usage = "Active states, dark variants" },
                    new ColorItemModel { Name = "Secondary", Hex = "#EC4899", CssVar = "--color-secondary", Usage = "Secondary actions" },
                    new ColorItemModel { Name = "Accent", Hex = "#06B6D4", CssVar = "--color-accent", Usage = "Accent elements" }
                }
            },
            new ColorPaletteModel
            {
                Name = "Semantic Colors",
                Description = "Colors with semantic meaning for status and feedback",
                Colors = new List<ColorItemModel>
                {
                    new ColorItemModel { Name = "Success", Hex = "#10B981", CssVar = "--color-success", Usage = "Success states, positive feedback" },
                    new ColorItemModel { Name = "Warning", Hex = "#F59E0B", CssVar = "--color-warning", Usage = "Warning states, caution" },
                    new ColorItemModel { Name = "Error", Hex = "#EF4444", CssVar = "--color-error", Usage = "Error states, destructive actions" },
                    new ColorItemModel { Name = "Info", Hex = "#3B82F6", CssVar = "--color-info", Usage = "Informational content" }
                }
            },
            new ColorPaletteModel
            {
                Name = "Language Levels",
                Description = "Colors for different language proficiency levels (CEFR)",
                Colors = new List<ColorItemModel>
                {
                    new ColorItemModel { Name = "A1", Hex = "#10B981", CssVar = "--color-level-a1", Usage = "Beginner level (A1)" },
                    new ColorItemModel { Name = "A2", Hex = "#06B6D4", CssVar = "--color-level-a2", Usage = "Elementary level (A2)" },
                    new ColorItemModel { Name = "B1", Hex = "#3B82F6", CssVar = "--color-level-b1", Usage = "Intermediate level (B1)" },
                    new ColorItemModel { Name = "B2", Hex = "#8B5CF6", CssVar = "--color-level-b2", Usage = "Upper-intermediate level (B2)" },
                    new ColorItemModel { Name = "C1", Hex = "#F59E0B", CssVar = "--color-level-c1", Usage = "Advanced level (C1)" },
                    new ColorItemModel { Name = "C2", Hex = "#EF4444", CssVar = "--color-level-c2", Usage = "Proficient level (C2)" }
                }
            },
            new ColorPaletteModel
            {
                Name = "Parts of Speech",
                Description = "Colors for different grammatical parts of speech",
                Colors = new List<ColorItemModel>
                {
                    new ColorItemModel { Name = "Noun", Hex = "#6366F1", CssVar = "--color-noun", Usage = "Nouns and noun phrases" },
                    new ColorItemModel { Name = "Verb", Hex = "#10B981", CssVar = "--color-verb", Usage = "Verbs and verb forms" },
                    new ColorItemModel { Name = "Adjective", Hex = "#F59E0B", CssVar = "--color-adjective", Usage = "Adjectives and descriptors" },
                    new ColorItemModel { Name = "Adverb", Hex = "#8B5CF6", CssVar = "--color-adverb", Usage = "Adverbs and modifiers" },
                    new ColorItemModel { Name = "Preposition", Hex = "#EC4899", CssVar = "--color-preposition", Usage = "Prepositions" },
                    new ColorItemModel { Name = "Phrase", Hex = "#06B6D4", CssVar = "--color-phrase", Usage = "Common phrases" }
                }
            }
        };
    }

    public List<TypographyExampleModel> GetTypographyExamples()
    {
        return new List<TypographyExampleModel>
        {
            new TypographyExampleModel { Name = "Heading 1", CssClass = "h1", FontSize = "36px (tablet) / 28px (mobile)", FontWeight = "700", Usage = "Page titles, main headings" },
            new TypographyExampleModel { Name = "Heading 2", CssClass = "h2", FontSize = "30px (tablet) / 24px (mobile)", FontWeight = "700", Usage = "Section headings" },
            new TypographyExampleModel { Name = "Heading 3", CssClass = "h3", FontSize = "24px (tablet) / 20px (mobile)", FontWeight = "600", Usage = "Subsection headings" },
            new TypographyExampleModel { Name = "Body Large", CssClass = "body-large", FontSize = "18px (tablet) / 16px (mobile)", FontWeight = "400", Usage = "Large body text, introductions" },
            new TypographyExampleModel { Name = "Body Medium", CssClass = "body-medium", FontSize = "16px (tablet) / 14px (mobile)", FontWeight = "400", Usage = "Regular body text" },
            new TypographyExampleModel { Name = "Body Small", CssClass = "body-small", FontSize = "14px (tablet) / 12px (mobile)", FontWeight = "400", Usage = "Secondary text, captions" },
            new TypographyExampleModel { Name = "Caption", CssClass = "caption", FontSize = "12px (tablet) / 10px (mobile)", FontWeight = "400", Usage = "Small text, metadata" }
        };
    }
}