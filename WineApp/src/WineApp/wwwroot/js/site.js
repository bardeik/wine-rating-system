// File download helper
window.downloadFile = function (fileName, contentType, base64Content) {
    const linkElement = document.createElement('a');
    linkElement.setAttribute('href', `data:${contentType};base64,${base64Content}`);
    linkElement.setAttribute('download', fileName);
    linkElement.click();
};

// Print helper
window.printPage = function () {
    window.print();
};
