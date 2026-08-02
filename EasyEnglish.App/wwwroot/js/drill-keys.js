/**
 * Keyboard navigation between drilling cards: left = previous, right = next, Space = next.
 *
 * The listener sits on document because focus during a test can be anywhere.
 * While the learner types in a field, arrows and space belong to that field, not to us.
 */
window.drillKeys = {
    _ref: null,
    _handler: null,

    register: function (dotNetRef) {
        this.unregister();
        this._ref = dotNetRef;

        this._handler = function (e) {
            if (e.ctrlKey || e.altKey || e.metaKey) return;

            var el = document.activeElement;
            if (el) {
                var tag = el.tagName;
                if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || el.isContentEditable) return;
            }

            var key = null;
            if (e.key === 'ArrowLeft')  key = 'prev';
            if (e.key === 'ArrowRight') key = 'next';
            if (e.key === ' ' || e.key === 'Spacebar') key = 'next';
            if (e.key === 'Escape') key = 'close';
            if (key === null) return;

            // Otherwise space scrolls the page and arrows scroll it horizontally
            e.preventDefault();

            if (window.drillKeys._ref)
                window.drillKeys._ref.invokeMethodAsync('HandleKey', key);
        };

        document.addEventListener('keydown', this._handler);
    },

    unregister: function () {
        if (this._handler) {
            document.removeEventListener('keydown', this._handler);
            this._handler = null;
        }
        this._ref = null;
    }
};
