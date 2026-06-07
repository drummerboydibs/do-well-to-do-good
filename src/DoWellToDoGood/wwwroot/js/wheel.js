// Keyboard support for the Emotion Wheel (roving tabindex).
// Blazor manages which wedge is "current"; this focuses it and stops the
// arrow/space keys from scrolling the page while a wedge is focused.
window.dwtdgWheel = {
    focus: function (i) {
        var el = document.querySelector('.wheel-wedge[data-i="' + i + '"]');
        if (el) { el.focus(); }
    }
};

document.addEventListener('keydown', function (e) {
    var el = document.activeElement;
    if (el && el.classList && el.classList.contains('wheel-wedge')) {
        if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Home', 'End', ' ', 'Spacebar'].indexOf(e.key) !== -1) {
            e.preventDefault();
        }
    }
});
