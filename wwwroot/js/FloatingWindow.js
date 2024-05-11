const { ipcRenderer } = require('electron');


ipcRenderer.on('enable-clickthrough', (event, windowId) => {
    alert("test");
    const win = electron.BrowserWindow.fromId(windowId);
    win.setIgnoreMouseEvents(true);
});