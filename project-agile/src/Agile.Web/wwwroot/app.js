window.downloadFile = function (filename, content) {
    const blob = new Blob([content], { type: 'text/html;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

window.printAsPdf = function (content) {
    const win = window.open('', '_blank');
    win.document.write(content);
    win.document.close();
    win.onload = function () { win.print(); };
};

window.scrollToBottom = function (id) {
    const el = document.getElementById(id);
    if (el) el.scrollTop = el.scrollHeight;
};

window.downloadCsv = function (filename, content) {
    const blob = new Blob([content], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

window.downloadCsvFile = function (filename, base64Content) {
    const bytes = Uint8Array.from(atob(base64Content), c => c.charCodeAt(0));
    const blob = new Blob([bytes], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
