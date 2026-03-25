window.blazorCulture = {
    get: () => localStorage['BlazorCulture'],
    set: (value) => {
        localStorage['BlazorCulture'] = value;
    }
};

window.theme = {
    get: () => localStorage['theme'] === 'dark',
    set: (isDark) => {
        localStorage['theme'] = isDark ? 'dark' : 'light';
        theme.apply(isDark);
    },
    apply: (isDark) => {
        document.documentElement.classList.toggle('dark-theme', isDark);
    }
};

window.initializeSortable = (dotNetHelper, elementId) => {
    const el = document.getElementById(elementId);
    if (el) {
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
    if (el) {
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
    const isDark = localStorage['theme'] === 'dark';
    theme.apply(isDark);
});