namespace EasyEnglish.Theme.Constants;

public static class ThemeConstants
{
    public static class Colors
    {
        // Primary colors
        public const string Primary = "#6366F1";
        public const string PrimaryLight = "#8B8CF8";
        public const string PrimaryDark = "#4F46E5";
        public const string PrimaryBg = "#1E1B4B";

        // Secondary colors
        public const string Secondary = "#EC4899";
        public const string SecondaryLight = "#F472B6";
        public const string SecondaryDark = "#DB2777";

        // Accent colors
        public const string Accent = "#06B6D4";
        public const string AccentLight = "#22D3EE";
        public const string AccentDark = "#0891B2";

        // Background colors
        public const string Background = "#0F0F23";
        public const string BackgroundSecondary = "#1A1B3A";
        public const string BackgroundTertiary = "#252653";
        public const string Surface = "#1E1F42";
        public const string SurfaceLight = "#2A2B5A";

        // Text colors
        public const string TextPrimary = "#F8FAFC";
        public const string TextSecondary = "#CBD5E1";
        public const string TextTertiary = "#94A3B8";
        public const string TextMuted = "#64748B";
        public const string TextDisabled = "#475569";

        // Semantic colors
        public const string Success = "#10B981";
        public const string SuccessLight = "#34D399";
        public const string SuccessDark = "#059669";
        public const string SuccessBg = "#064E3B";

        public const string Warning = "#F59E0B";
        public const string WarningLight = "#FBBF24";
        public const string WarningDark = "#D97706";
        public const string WarningBg = "#451A03";

        public const string Error = "#EF4444";
        public const string ErrorLight = "#F87171";
        public const string ErrorDark = "#DC2626";
        public const string ErrorBg = "#450A0A";

        public const string Info = "#3B82F6";
        public const string InfoLight = "#60A5FA";
        public const string InfoDark = "#2563EB";
        public const string InfoBg = "#1E3A8A";

        // Border colors
        public const string Border = "#334155";
        public const string BorderLight = "#475569";
        public const string BorderDark = "#1E293B";

        // Input colors
        public const string InputBackground = "#1E293B";
        public const string InputBorder = "#475569";
        public const string InputFocus = "#6366F1";
        public const string InputPlaceholder = "#64748B";

        // Level colors for language learning
        public const string LevelA1 = "#10B981";
        public const string LevelA2 = "#06B6D4";
        public const string LevelB1 = "#3B82F6";
        public const string LevelB2 = "#8B5CF6";
        public const string LevelC1 = "#F59E0B";
        public const string LevelC2 = "#EF4444";

        // Part of speech colors
        public const string Noun = "#6366F1";
        public const string Verb = "#10B981";
        public const string Adjective = "#F59E0B";
        public const string Adverb = "#8B5CF6";
        public const string Preposition = "#EC4899";
        public const string Phrase = "#06B6D4";
        public const string PhrasalVerb = "#84CC16";
        public const string Idiom = "#F97316";
        public const string Pronoun = "#EF4444";
        public const string Conjunction = "#14B8A6";
        public const string Interjection = "#A855F7";
        public const string Slang = "#F43F5E";
        public const string Abbreviation = "#64748B";
        public const string FixedExpression = "#0EA5E9";
    }

    public static class Spacing
    {
        public const int Xs = 4;
        public const int Sm = 8;
        public const int Md = 16;
        public const int Lg = 24;
        public const int Xl = 32;
        public const int Xxl = 48;
        public const int Xxxl = 64;
    }

    public static class BorderRadius
    {
        public const int Xs = 2;
        public const int Sm = 4;
        public const int Md = 8;
        public const int Lg = 12;
        public const int Xl = 16;
        public const int Xxl = 24;
        public const int Round = 999;
    }

    public static class FontSizes
    {
        public const int Xs = 12;
        public const int Sm = 14;
        public const int Base = 16;
        public const int Lg = 18;
        public const int Xl = 20;
        public const int Xl2 = 24;
        public const int Xl3 = 30;
        public const int Xl4 = 36;
        public const int Xl5 = 48;
    }

    public static class FontWeights
    {
        public const int Light = 300;
        public const int Normal = 400;
        public const int Medium = 500;
        public const int Semibold = 600;
        public const int Bold = 700;
        public const int Extrabold = 800;
    }

    public static class LineHeights
    {
        public const double Tight = 1.2;
        public const double Snug = 1.375;
        public const double Normal = 1.5;
        public const double Relaxed = 1.625;
        public const double Loose = 2.0;
    }

    public static class Breakpoints
    {
        public const int Mobile = 480;
        public const int Tablet = 768;
        public const int Desktop = 1024;
    }

    public static class CssClasses
    {
        // Layout
        public const string Flex = "flex";
        public const string Flex1 = "flex-1";
        public const string FlexRow = "flex-row";
        public const string FlexColumn = "flex-column";
        public const string FlexCenter = "flex-center";
        public const string FlexSpaceBetween = "flex-space-between";
        public const string FlexSpaceAround = "flex-space-around";
        public const string FlexSpaceEvenly = "flex-space-evenly";
        public const string FlexWrap = "flex-wrap";

        // Container
        public const string Container = "container";
        public const string ContainerPadded = "container-padded";
        public const string Screen = "screen";
        public const string ScreenCentered = "screen-centered";

        // Cards
        public const string Card = "card";
        public const string CardSmall = "card-small";
        public const string CardLarge = "card-large";

        // Typography
        public const string H1 = "h1";
        public const string H2 = "h2";
        public const string H3 = "h3";
        public const string H4 = "h4";
        public const string H5 = "h5";
        public const string H6 = "h6";
        public const string BodyLarge = "body-large";
        public const string BodyMedium = "body-medium";
        public const string BodySmall = "body-small";
        public const string Caption = "caption";

        // Text colors
        public const string TextPrimary = "text-primary";
        public const string TextSecondary = "text-secondary";
        public const string TextTertiary = "text-tertiary";
        public const string TextMuted = "text-muted";
        public const string TextDisabled = "text-disabled";
        public const string TextSuccess = "text-success";
        public const string TextWarning = "text-warning";
        public const string TextError = "text-error";
        public const string TextInfo = "text-info";
        public const string TextAccent = "text-accent";

        // Text alignment
        public const string TextCenter = "text-center";
        public const string TextLeft = "text-left";
        public const string TextRight = "text-right";

        // Buttons
        public const string Button = "button";
        public const string ButtonPrimary = "button-primary";
        public const string ButtonSecondary = "button-secondary";
        public const string ButtonAccent = "button-accent";
        public const string ButtonSuccess = "button-success";
        public const string ButtonWarning = "button-warning";
        public const string ButtonError = "button-error";
        public const string ButtonOutline = "button-outline";
        public const string ButtonGhost = "button-ghost";
        public const string ButtonDisabled = "button-disabled";

        // Inputs
        public const string Input = "input";
        public const string InputMultiline = "input-multiline";
        public const string TextArea = "text-area";
        public const string InputError = "input-error";
        public const string InputDisabled = "input-disabled";

        // Lists
        public const string ListContainer = "list-container";
        public const string ListItem = "list-item";

        // Utilities
        public const string Separator = "separator";
        public const string SeparatorVertical = "separator-vertical";
        public const string LoadingContainer = "loading-container";
        public const string LoadingOverlay = "loading-overlay";
        public const string ProgressBarContainer = "progress-bar-container";
        public const string ProgressBarFill = "progress-bar-fill";
        public const string Badge = "badge";
        public const string Hidden = "hidden";
        public const string Absolute = "absolute";
        public const string Relative = "relative";
        public const string FullWidth = "full-width";
        public const string FullHeight = "full-height";
        public const string Rounded = "rounded";
        public const string RoundedLg = "rounded-lg";
        public const string RoundedXl = "rounded-xl";
        public const string Opacity50 = "opacity-50";
        public const string Opacity75 = "opacity-75";

        // Level classes
        public const string LevelA1 = "level-a1";
        public const string LevelA2 = "level-a2";
        public const string LevelB1 = "level-b1";
        public const string LevelB2 = "level-b2";
        public const string LevelC1 = "level-c1";
        public const string LevelC2 = "level-c2";

        public const string LevelBadgeA1 = "level-badge-a1";
        public const string LevelBadgeA2 = "level-badge-a2";
        public const string LevelBadgeB1 = "level-badge-b1";
        public const string LevelBadgeB2 = "level-badge-b2";
        public const string LevelBadgeC1 = "level-badge-c1";
        public const string LevelBadgeC2 = "level-badge-c2";

        // Spacing classes
        public static class Margin
        {
            public const string Xs = "m-xs";
            public const string Sm = "m-sm";
            public const string Md = "m-md";
            public const string Lg = "m-lg";
            public const string Xl = "m-xl";

            public const string TopXs = "mt-xs";
            public const string TopSm = "mt-sm";
            public const string TopMd = "mt-md";
            public const string TopLg = "mt-lg";
            public const string TopXl = "mt-xl";

            public const string BottomXs = "mb-xs";
            public const string BottomSm = "mb-sm";
            public const string BottomMd = "mb-md";
            public const string BottomLg = "mb-lg";
            public const string BottomXl = "mb-xl";

            public const string HorizontalXs = "mx-xs";
            public const string HorizontalSm = "mx-sm";
            public const string HorizontalMd = "mx-md";
            public const string HorizontalLg = "mx-lg";
            public const string HorizontalXl = "mx-xl";

            public const string VerticalXs = "my-xs";
            public const string VerticalSm = "my-sm";
            public const string VerticalMd = "my-md";
            public const string VerticalLg = "my-lg";
            public const string VerticalXl = "my-xl";
        }

        public static class Padding
        {
            public const string Xs = "p-xs";
            public const string Sm = "p-sm";
            public const string Md = "p-md";
            public const string Lg = "p-lg";
            public const string Xl = "p-xl";

            public const string TopXs = "pt-xs";
            public const string TopSm = "pt-sm";
            public const string TopMd = "pt-md";
            public const string TopLg = "pt-lg";
            public const string TopXl = "pt-xl";

            public const string BottomXs = "pb-xs";
            public const string BottomSm = "pb-sm";
            public const string BottomMd = "pb-md";
            public const string BottomLg = "pb-lg";
            public const string BottomXl = "pb-xl";

            public const string HorizontalXs = "px-xs";
            public const string HorizontalSm = "px-sm";
            public const string HorizontalMd = "px-md";
            public const string HorizontalLg = "px-lg";
            public const string HorizontalXl = "px-xl";

            public const string VerticalXs = "py-xs";
            public const string VerticalSm = "py-sm";
            public const string VerticalMd = "py-md";
            public const string VerticalLg = "py-lg";
            public const string VerticalXl = "py-xl";
        }
    }
}