window.blazorCulture = {
    get: () => localStorage.getItem('BlazorCulture'),
    set: (value) => {
        localStorage.setItem('BlazorCulture', value);
    }
};

window.theme = {
    get: () => localStorage.getItem('theme') === 'dark',
    set: (isDark) => {
        localStorage.setItem('theme', isDark ? 'dark' : 'light');
        window.theme.apply(isDark);
    },
    apply: (isDark) => {
        document.documentElement.classList.toggle('dark-theme', !!isDark);
    }
};

window.initializeSortable = (dotNetHelper, elementId) => {
    const el = document.getElementById(elementId);
    if (el && typeof Sortable !== 'undefined') {
        new Sortable(el, {
            animation: 150,
            onEnd: (evt) => {
                dotNetHelper.invokeMethodAsync('UpdateOrder', evt.oldIndex, evt.newIndex);
            }
        });
    }
};

window.initializeSortableFields = (dotNetHelper, elementId) => {
    const el = document.getElementById(elementId);
    if (el && typeof Sortable !== 'undefined') {
        new Sortable(el, {
            animation: 150,
            onEnd: (evt) => {
                dotNetHelper.invokeMethodAsync('UpdateFieldOrder', evt.oldIndex, evt.newIndex);
            }
        });
    }
};

window.downloadFile = (fileName, contentType, content) => {
    const blob = new Blob([content], { type: contentType });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(link.href);
};

document.addEventListener('DOMContentLoaded', () => {
    const isDark = localStorage.getItem('theme') === 'dark';
    window.theme.apply(isDark);
});