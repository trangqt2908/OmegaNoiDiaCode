function loadDropModules(control, idControl, type) {
    var id = null;
    var hfCountry = $(idControl);   

    if (control != null) {
        id = $(control).val();

        hfCountry.val(id);

        if ($(control).nextAll().length != 0) {
            $(control).nextAll().remove();
        }      

        if (id == 0) {
            var previous = $(control).prev();

            if (previous.length != 0) {
                id = previous.val();
                hfCountry.val(id);
            }
            return;
        }
    }

    $.ajax({
        type: "POST",
        async: false,
        url: "/contenttype/home/GetDestinations/" + id,
        data: {},
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.length == 0) {
                return;
            }

            var select = "<select class = 'select' rel='country' onchange = \"loadDropModules(this, '" + idControl + "', '" + type + "')\">";
            var option = "<option value = '$value'>$text</option>";

            select += option.replace("$value", 0).replace("$text", type);

            for (var i = 0; i < result.length; i++) {
                var module = result[i];
                select += option.replace("$value", module.ID).replace("$text", module.Title);
            }

            select += "</select>";

            var lastSelect = hfCountry.siblings(":last");

            if (lastSelect.length != 0) {
                lastSelect.after(select);
            } else {
                hfCountry.after(select);
            }

        },
        error: function () {
            alert("Đã có lỗi xảy ra bạn vui lòng thao tác lại");
        }
    });
}
