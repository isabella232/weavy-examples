var wvy = wvy || {};
wvy.asanaApp = (function ($) {

    var asanaBaseUrl = "https://app.asana.com/api/1.0";
    var authWin = null;
    var init = function (appId, clientId) {

        new Vue({
            el: '#app',

            data: {
                authenticated: false,
                client: null,
                token: null,
                me: null,
                tasks: []                
            },

            filters: {
                section: function (name) {
                    if (name.endsWith(":")) {
                        name = name.substring(0, name.length - 1);
                    }
                    return name;
                }
            },

            methods: {
                auth: function () {                 
                    authWin = window.open(
                        "https://app.asana.com/-/oauth_authorize?response_type=token&client_id=" + clientId + "&redirect_uri=https%3A%2F%2Flocalhost%3A44300%2Fapps%2FB309F192-9F22-4E39-AAFC-DD59589227C7%2Fauth&state=somerandomstate",
                        "authWindow",
                        "width=500px,height=700px"
                    );
                    
                },

                toggleCompleted: function (task) {

                    var app = this;
                    var id = task.id;
                    task.completed = !task.completed;

                    $.ajax({
                        url: asanaBaseUrl + "/tasks/" + id,
                        contentType: "application/json",
                        data: JSON.stringify({ data: { completed: task.completed } }),
                        method: "PUT",
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader('Authorization', 'Bearer ' + app.token);
                        }
                    });
                },

                getData: function () {
                    var app = this;

                    // very simplified implementation for demo purposes only
                    if (!app.authenticated)
                        return;

                    
                    // get the user info and then all the tasks                    
                    $.ajax({
                        url: asanaBaseUrl + "/users/me/",
                        contentType: "application/json",
                        method: "GET",
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader('Authorization', 'Bearer ' + app.token);
                        }
                    }).then(function (me) {                         
                        app.me = me.data;

                        return $.ajax({
                            url: asanaBaseUrl + "/tasks?workspace=" + me.data.workspaces[0].id + "&assignee=" + me.data.id + "&opt_fields=id,name,assignee_status,completed",
                            contentType: "application/json",
                            method: "GET",
                            beforeSend: function (xhr) {
                                xhr.setRequestHeader('Authorization', 'Bearer ' + app.token);
                            }
                        });
                    }).then(function (response) { 
                        app.tasks = _.map(response.data, function (t) {
                            return { id: t.id, name: t.name, is_section: t.name.endsWith(":"), completed: t.completed };
                        });
                    }).fail(function () { 
                        app.authenticated = false;
                    });

                }
            },

            created: function () {
                
                this.token = getCookie("asana_access_token_" + appId);

                if (this.token) {
                    this.authenticated = true;
                }
            },

            mounted: function () {
                var app = this;

                 window.addEventListener("message", function (e) {                     
                    switch (e.data.name) {
                        case "auth":
                            app.token = e.data.token;
                            setCookie("asana_access_token_" + appId, app.token, 1);
                            app.authenticated = true;
                            app.getData();

                            if (authWin) {
                                authWin.close();
                            }                                
                            break;
                    }
                });

                app.getData();
            }
        });


    };

    function setCookie(cname, cvalue, expireHours) {
        var d = new Date();
        d.setTime(d.getTime() + (expireHours* 60 * 60 * 1000));
        var expires = "expires=" + d.toUTCString();
        document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
    }

    function getCookie(cname) {
        var name = cname + "=";
        var decodedCookie = decodeURIComponent(document.cookie);
        var ca = decodedCookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(name) == 0) {
                return c.substring(name.length, c.length);
            }
        }
        return "";
    }

    var destroy = function () {

    };

    return {
        init: init,
        destroy: destroy
    };

})(jQuery);
