var weavy = weavy || {};

weavy.realtime = (function ($) {

    // attach an event handler for the specified server event, e.g. "presence", "typing" etc (see PushService for a list of built-in events)
    function on(event, handler, proxy) {
        proxy = proxy || "rtm";
        $(document).on(event + "." + proxy + ".weavy", null, null, handler);
    }

    // invoke a method on a server hub, e.g. "SetActive" on the RealTimeHub (rtm) or "Typing" on the MessengerHub (messenger).
    function invoke(hub, method, data) {
        var args = data ? [method, data] : [method];
        if (weavy.connection.connection.state === $.signalR.connectionState.connected) {
            var proxy = weavy.connection.proxies[hub];
            proxy.invoke.apply(proxy, args).fail(function (error) {
                console.error(error);
            });
        } else if (weavy.browser && weavy.browser.embedded) {
            // if embedded then execute invoke message from host page
            window.parent.postMessage({ name: "invoke", hub: hub, args: args }, "*")
        }
    }

    // handle cross frame events from rtm
    var onCrossMessageReceived = function (e) {

        switch (e.data.name) {
            case "cross-frame-event":
                var name = e.data.eventName;
                var event = $.Event(name);

                $(document).triggerHandler(event, e.data.data);
                break;
            case "alert":
                if (e.data.eventName === "show") {
                    weavy.alert.alert(e.data.data.type, e.data.data.title, null, e.data.data.id);
                } else {
                    weavy.alert.close(e.data.data);
                }
                break;
            default:
                return;
        }
    }

    window.addEventListener("message", onCrossMessageReceived, false);

    return {
        on: on,
        invoke: invoke
    };

})(jQuery);
