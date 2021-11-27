document.addEventListener('click', function (event) {
    //let elem = event.target;
    let x = event.pageX;
    let y = event.pageY;
    let jsonObject =
    {
        Key: 'click',
        Value:
        {
            X: event.screenX + (window.outerWidth - window.innerWidth) / 2 - window.scrollX,
            Y: event.screenY + (window.outerHeight - window.innerHeight) - window.scrollY
        }
    };
    window.chrome.webview.postMessage(jsonObject);
});

document.addEventListener('mousemove', function (event) {
    //let elem = event.target;
    let x = event.pageX;
    let y = event.pageY;
    let jsonObject =
    {
        Key: 'mousemove',
        Value:
        {
            X: event.screenX,
            Y: event.screenY
        }
    };
    window.chrome.webview.postMessage(jsonObject);
});

//document.addEventListener('mousemove', onMouseUpdate, false);
//document.addEventListener('mouseenter', onMouseUpdate, false);

//function onMouseUpdate(e) {
//    x = e.pageX;
//    y = e.pageY;


//    let jsonObject =
//    {
//        Key: 'click',
//        Value: elem.name || elem.id || elem.tagName || "Unkown"
//    };
//    window.chrome.webview.postMessage(jsonObject);
//}