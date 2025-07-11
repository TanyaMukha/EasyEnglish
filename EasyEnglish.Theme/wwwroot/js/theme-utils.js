// theme-utils.js - Скопійовано з EasyEnglish.Theme для MAUI сумісності
// Utilities для роботи з темою в MAUI Blazor WebView

console.log('🎨 EasyEnglish Theme Utilities Loading...');

window.themeUtils = {
    /**
     * Встановлює тему додатку
     * @param {string} theme - Назва теми ('light' або 'dark')
     */
    setTheme: (theme) => {
        console.log(`🎨 Setting theme to: ${theme}`);

        // Встановлюємо атрибут для CSS селекторів
        document.documentElement.setAttribute('data-theme', theme);

        // Зберігаємо в localStorage якщо доступно
        try {
            localStorage.setItem('app-theme', theme);
            console.log(`💾 Theme ${theme} saved to localStorage`);
        } catch (e) {
            console.warn('⚠️ localStorage not available:', e.message);
        }

        // Диспатчимо подію для компонентів
        window.dispatchEvent(new CustomEvent('themeChanged', {
            detail: { theme }
        }));
    },

    /**
     * Отримує поточну тему
     * @returns {string} Назва поточної теми
     */
    getTheme: () => {
        try {
            const savedTheme = localStorage.getItem('app-theme');
            const currentTheme = savedTheme || 'dark'; // За замовчуванням темна тема
            console.log(`🔍 Current theme: ${currentTheme}`);
            return currentTheme;
        } catch (e) {
            console.warn('⚠️ localStorage read failed:', e.message);
            return 'dark';
        }
    },

    /**
     * Перемикає між світлою та темною темою
     */
    toggleTheme: () => {
        const currentTheme = window.themeUtils.getTheme();
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        window.themeUtils.setTheme(newTheme);
        console.log(`🔄 Theme toggled: ${currentTheme} → ${newTheme}`);
        return newTheme;
    },

    /**
     * Ініціалізує тему при завантаженні
     */
    initTheme: () => {
        const savedTheme = window.themeUtils.getTheme();
        window.themeUtils.setTheme(savedTheme);
        console.log(`🚀 Theme initialized: ${savedTheme}`);
    },

    /**
     * Отримує CSS змінну теми
     * @param {string} varName - Назва CSS змінної (без --)
     * @returns {string} Значення CSS змінної
     */
    getCSSVariable: (varName) => {
        const fullVarName = varName.startsWith('--') ? varName : `--${varName}`;
        const value = getComputedStyle(document.documentElement)
            .getPropertyValue(fullVarName)
            .trim();

        console.log(`🎨 CSS Variable ${fullVarName}: ${value}`);
        return value;
    },

    /**
     * Встановлює CSS змінну теми
     * @param {string} varName - Назва CSS змінної (без --)
     * @param {string} value - Значення змінної
     */
    setCSSVariable: (varName, value) => {
        const fullVarName = varName.startsWith('--') ? varName : `--${varName}`;
        document.documentElement.style.setProperty(fullVarName, value);
        console.log(`🎨 CSS Variable set: ${fullVarName} = ${value}`);
    },

    /**
     * Перевіряє чи тема завантажена
     * @returns {boolean} true якщо тема завантажена
     */
    isThemeLoaded: () => {
        const primaryColor = window.themeUtils.getCSSVariable('color-primary');
        const isLoaded = primaryColor !== '';
        console.log(`✅ Theme loaded: ${isLoaded}`);
        return isLoaded;
    },

    /**
     * Отримує всі доступні кольори теми
     * @returns {object} Об'єкт з кольорами теми
     */
    getThemeColors: () => {
        const colors = {
            primary: window.themeUtils.getCSSVariable('color-primary'),
            secondary: window.themeUtils.getCSSVariable('color-secondary'),
            accent: window.themeUtils.getCSSVariable('color-accent'),
            background: window.themeUtils.getCSSVariable('color-background'),
            surface: window.themeUtils.getCSSVariable('color-surface'),
            textPrimary: window.themeUtils.getCSSVariable('color-text-primary'),
            textSecondary: window.themeUtils.getCSSVariable('color-text-secondary'),
            success: window.themeUtils.getCSSVariable('color-success'),
            warning: window.themeUtils.getCSSVariable('color-warning'),
            error: window.themeUtils.getCSSVariable('color-error'),
            info: window.themeUtils.getCSSVariable('color-info')
        };

        console.log('🎨 Theme colors:', colors);
        return colors;
    },

    /**
     * Встановлює системну тему (автоматично відповідно до OS)
     */
    setSystemTheme: () => {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            window.themeUtils.setTheme('dark');
            console.log('🌙 System theme: dark');
        } else {
            window.themeUtils.setTheme('light');
            console.log('☀️ System theme: light');
        }
    },

    /**
     * Слухає зміни системної теми
     */
    watchSystemTheme: () => {
        if (window.matchMedia) {
            const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
            mediaQuery.addEventListener('change', (e) => {
                const newTheme = e.matches ? 'dark' : 'light';
                window.themeUtils.setTheme(newTheme);
                console.log(`🔄 System theme changed to: ${newTheme}`);
            });
            console.log('👁️ Watching system theme changes...');
        }
    }
};

// Автоматична ініціалізація при завантаженні
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.themeUtils.initTheme();
    });
} else {
    // DOM вже завантажений
    window.themeUtils.initTheme();
}

// Експорт для ES модулів (якщо потрібно)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = window.themeUtils;
}

console.log('✅ EasyEnglish Theme Utilities Ready!');