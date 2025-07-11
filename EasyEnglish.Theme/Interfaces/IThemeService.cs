﻿namespace EasyEnglish.Theme.Interfaces;

public interface IThemeService
{
    string GetColorByPartOfSpeech(string partOfSpeech);
    string GetLevelColor(string level);
    string GetResponsiveClass(string baseClass, string? tabletClass = null, string? desktopClass = null);
    bool IsTablet { get; }
    bool IsDesktop { get; }
    int GetSpacing(SpacingSize size);
    int GetFontSize(ThemeFontSize size); // Перейменовано для уникнення конфлікту
    string GetShadowClass(ShadowSize size);
    Task InitializeAsync();
}

public enum SpacingSize
{
    Xs, Sm, Md, Lg, Xl, Xxl, Xxxl
}

// Перейменовано enum для уникнення конфлікту з Microsoft.Maui.FontSize
public enum ThemeFontSize
{
    Xs, Sm, Base, Lg, Xl, Xl2, Xl3, Xl4, Xl5
}

public enum ShadowSize
{
    Sm, Md, Lg, Xl
}
