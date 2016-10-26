$(function () {
    var authForm = "<form class='auth-form' style='display:none' title='Fill basic auth credentials if needed.'>" +
            "<label style='margin-left: 10%'>basic auth: </label>" +
            "<input placeholder='id' style='width: 25%;margin: 5px 0 0 10px;' name='cred_id' class='cred-id' />" +
            "<input placeholder='password' type='password' style='width: 25%;margin: 5px 0 0 15px;' name='cred_pwd' class='cred-pwd' />" +
        "</form>";

    var placeToInsert = ".endpoint .heading > h3";
    var url = ".endpoint .heading .path > a";
    var headings = ".endpoint .heading";
    var credentials = {
        id: null,
        password: null,
        valid: function () {
            return this.id != null && this.password != null;
        }
    };

    insertCredentialsForms(placeToInsert);
    setTimeout(showAuthForms, 1000);

    function insertCredentialsForms(selector) {
        if ($(selector) == null)
            throw "could not insert basic auth form. element not exist:" + selector;

        $(selector).each(function(i, heading) {
            var responseCodes = $(heading).parents(".endpoint").find(".code");

            $(responseCodes).each(function (j, el) {
                var code = $(el);
                if (code.text() === "401" ||
                    code.text() === "403") {
                    $(authForm).insertAfter(heading);
                    return false;
                }
            });
        });
    }

    function authorizeHeader() {
        if (credentials.valid()) {
            var value = "Basic " + window.btoa(credentials.id + ":" + credentials.password);
            var authKeyHeader = new SwaggerClient.ApiKeyAuthorization("Authorization", value, "header");
            window.swaggerUi.api.clientAuthorizations.add("Authorization", authKeyHeader);
        }
    }

    function showAuthForms() {
        $(headings).each(function (i, heading) {
            if ($(heading).next(".content").css('display') === 'block')
                $(heading).children(".auth-form:first").show();
        });
    }

    function updateCredentials(form) {
        credentials.id = $(form).children("input[name=cred_id]:first").val();
        credentials.password = $(form).children("input[name=cred_pwd]:first").val();
        authorizeHeader();
    }

    function toggleAuthForm(heading) {
        var form = $(heading).children(".auth-form:first");
        if (!form.length) return;

        if ($(heading).next(".content").css('display') === 'block') {
            form.hide();
        } else {
            form.show();
            updateCredentials(form);
        }
    }

    $(url).on("click", function () {
        var heading = $(this).closest("div");
        toggleAuthForm(heading);
    });

    $("input[name=cred_id]").on("change paste keyup", function () {
        updateCredentials($(this).closest('.auth-form'));
    });

    $("input[name=cred_pwd]").on("change paste keyup", function () {
        updateCredentials($(this).closest('.auth-form'));
    });
});