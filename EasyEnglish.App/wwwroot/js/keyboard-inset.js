/**
 * Keeps the CSS variable --keyboard-inset equal to the on-screen keyboard height.
 *
 * The page adds that inset at the bottom (see .content-wrapper in MainLayout) and fixed
 * bottom bars lift by the same amount — so the keyboard never covers input fields
 * or buttons.
 *
 * If the platform already shrinks the WebView for the keyboard (Android + adjustResize),
 * visualViewport.height equals window.innerHeight, the difference is 0, and no double
 * inset is added.
 */
(function () {
    var root = document.documentElement;

    // Below this threshold the height difference is browser chrome or rounding, not a keyboard
    var MIN_KEYBOARD_HEIGHT = 120;

    function apply(height) {
        root.style.setProperty('--keyboard-inset', height + 'px');
        root.classList.toggle('keyboard-open', height > 0);
    }

    function update() {
        var vv = window.visualViewport;
        if (!vv) return;

        var overlap = window.innerHeight - (vv.height + vv.offsetTop);
        apply(overlap > MIN_KEYBOARD_HEIGHT ? Math.round(overlap) : 0);
    }

    function scrollFocusedIntoView() {
        var el = document.activeElement;
        if (!el) return;

        var tag = el.tagName;
        if (tag !== 'INPUT' && tag !== 'TEXTAREA' && tag !== 'SELECT' && !el.isContentEditable) return;

        el.scrollIntoView({ block: 'center', behavior: 'smooth' });
    }

    window.keyboardInset = {
        get: function () {
            return parseInt(root.style.getPropertyValue('--keyboard-inset'), 10) || 0;
        }
    };

    apply(0);

    if (window.visualViewport) {
        window.visualViewport.addEventListener('resize', update);
        window.visualViewport.addEventListener('scroll', update);
    }

    // The keyboard does not appear instantly — refresh after the animation and scroll the field into view
    document.addEventListener('focusin', function () {
        setTimeout(function () {
            update();
            scrollFocusedIntoView();
        }, 300);
    });

    document.addEventListener('focusout', function () {
        setTimeout(update, 300);
    });
})();
