function addStyle(url) {
    return new Promise((resolve, reject) => {
        var link = document.createElement('link');
        link.href = url;
        link.rel = 'stylesheet';
        link.type = 'text/css';

        link.onload = resolve;
        link.onerror = reject;

        document.head.appendChild(link);
    });
}

function addScript(url) {
    return new Promise((resolve, reject) => {
        var script = document.createElement('script');
        script.src = url;

        script.onload = resolve;
        script.onerror = reject;

        document.body.appendChild(script);  // Ajout du script à la fin du body
    });
}