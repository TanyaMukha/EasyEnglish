<script>
window.themeUtils = {
    // Screen utilities
    getScreenWidth: () => window.innerWidth,
    getScreenHeight: () => window.innerHeight,
    
    // Device type detection
    isMobile: () => window.innerWidth < 768,
    isTablet: () => window.innerWidth >= 768 && window.innerWidth < 1024,
    isDesktop: () => window.innerWidth >= 1024,
    
    // Theme management
    setTheme: (theme) => {
        document.documentElement.setAttribute('data-theme', theme);
    },
    
    getTheme: () => {
        return document.documentElement.getAttribute('data-theme') || 'dark';
    },
    
    // CSS variable utilities
    setCssVariable: (name, value) => {
        document.documentElement.style.setProperty(`--${name}`, value);
    },
    
    getCssVariable: (name) => {
        return getComputedStyle(document.documentElement).getPropertyValue(`--${name}`);
    },
    
    // Responsive utilities
    subscribeToResize: (dotNetRef) => {
        window.themeUtils._dotNetRef = dotNetRef;
        window.addEventListener('resize', window.themeUtils._handleResize);
    },
    
    unsubscribeFromResize: () => {
        window.removeEventListener('resize', window.themeUtils._handleResize);
        window.themeUtils._dotNetRef = null;
    },
    
    _handleResize: () => {
        if (window.themeUtils._dotNetRef) {
            window.themeUtils._dotNetRef.invokeMethodAsync(
                'OnWindowResize', 
                window.innerWidth, 
                window.innerHeight
            );
        }
    },
    
    // Color utilities
    hexToRgb: (hex) => {
        const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : null;
    },
    
    rgbToHex: (r, g, b) => {
        return "#" + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
    },
    
    // Animation utilities
    fadeIn: (element, duration = 300) => {
        element.style.opacity = 0;
        element.style.display = 'block';
        
        let start = performance.now();
        
        function animate(time) {
            let progress = (time - start) / duration;
            if (progress > 1) progress = 1;
            
            element.style.opacity = progress;
            
            if (progress < 1) {
                requestAnimationFrame(animate);
            }
        }
        
        requestAnimationFrame(animate);
    },
    
    fadeOut: (element, duration = 300) => {
        let start = performance.now();
        
        function animate(time) {
            let progress = (time - start) / duration;
            if (progress > 1) progress = 1;
            
            element.style.opacity = 1 - progress;
            
            if (progress < 1) {
                requestAnimationFrame(animate);
            } else {
                element.style.display = 'none';
            }
        }
        
        requestAnimationFrame(animate);
    }
};

// Initialize theme on load
document.addEventListener('DOMContentLoaded', () => {
    // Set default theme if none exists
    if (!window.themeUtils.getTheme()) {
        window.themeUtils.setTheme('dark');
    }
});
</script>