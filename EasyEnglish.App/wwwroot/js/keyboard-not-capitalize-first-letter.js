/**
 * Глобальне виправлення клавіатури - вимикає автоматичну велику літеру
 * Файл: wwwroot/js/keyboard-global.js
 */

(function() {
    'use strict';
    
    /**
     * Налаштовує атрибути для input/textarea
     */
    function configureInput(element) {
        // Вимикаємо автоматичну велику літеру
        element.setAttribute('autocapitalize', 'none');
        
        // Опціонально: вимикаємо автокорекцію
        element.setAttribute('autocorrect', 'off');
        
        // Опціонально: вимикаємо автозаповнення
        // element.setAttribute('autocomplete', 'off');
        
        // Опціонально: вимикаємо перевірку орфографії
        // element.setAttribute('spellcheck', 'false');
    }
    
    /**
     * Обробляє всі існуючі поля на сторінці
     */
    function processExistingInputs() {
        // Знаходимо всі input та textarea
        const inputs = document.querySelectorAll('input[type="text"], input[type="search"], input[type="email"], input:not([type]), textarea');
        
        inputs.forEach(input => {
            configureInput(input);
        });
        
        console.log(`Keyboard Global: Processed ${inputs.length} input fields`);
    }
    
    /**
     * Спостерігає за новими полями (для динамічного Blazor контенту)
     */
    function observeNewInputs() {
        const observer = new MutationObserver(mutations => {
            mutations.forEach(mutation => {
                mutation.addedNodes.forEach(node => {
                    // Перевіряємо чи це input/textarea
                    if (node.nodeType === 1) { // Element node
                        if (node.tagName === 'INPUT' || node.tagName === 'TEXTAREA') {
                            configureInput(node);
                        }
                        
                        // Перевіряємо дочірні елементи
                        const inputs = node.querySelectorAll?.('input[type="text"], input[type="search"], input[type="email"], input:not([type]), textarea');
                        inputs?.forEach(input => {
                            configureInput(input);
                        });
                    }
                });
            });
        });
        
        // Спостерігаємо за змінами в DOM
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
        
        console.log('Keyboard Global: Observer started');
    }
    
    /**
     * Ініціалізація
     */
    function init() {
        console.log('Keyboard Global: Initializing...');
        
        // Обробляємо існуючі поля
        processExistingInputs();
        
        // Спостерігаємо за новими полями
        observeNewInputs();
        
        console.log('Keyboard Global: Ready');
    }
    
    // Запускаємо після завантаження DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
    
    // Також запускаємо при кожному Blazor ререндері
    if (window.Blazor) {
        window.Blazor.addEventListener('enhancedload', () => {
            console.log('Keyboard Global: Blazor enhanced load');
            processExistingInputs();
        });
    }
    
})();
