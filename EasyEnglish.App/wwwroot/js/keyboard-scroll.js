/**
 * Автоматичне прокручування до активного поля при появі клавіатури
 */
window.KeyboardScroll = {
    init: function() {
        // Обробник для всіх input, textarea, select
        document.addEventListener('focusin', function(e) {
            const element = e.target;
            
            // Перевіряємо чи це поле вводу
            if (element.tagName === 'INPUT' || 
                element.tagName === 'TEXTAREA' || 
                element.tagName === 'SELECT') {
                
                // Затримка для появи клавіатури
                setTimeout(function() {
                    // Знаходимо скролюваний контейнер
                    const scrollContainer = element.closest('.scroll-area');
                    
                    if (scrollContainer) {
                        // Отримуємо позицію елемента відносно контейнера
                        const elementRect = element.getBoundingClientRect();
                        const containerRect = scrollContainer.getBoundingClientRect();
                        
                        // Розраховуємо потрібну позицію скролу
                        // Елемент буде в центрі видимої області
                        const elementTop = element.offsetTop;
                        const scrollTop = elementTop - (containerRect.height / 2) + (elementRect.height / 2);
                        
                        // Плавне прокручування
                        scrollContainer.scrollTo({
                            top: scrollTop,
                            behavior: 'smooth'
                        });
                    }
                }, 300); // 300ms - час для появи клавіатури
            }
        });
        
        console.log('KeyboardScroll initialized');
    }
};

// Автоматична ініціалізація
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        window.KeyboardScroll.init();
    });
} else {
    window.KeyboardScroll.init();
}
