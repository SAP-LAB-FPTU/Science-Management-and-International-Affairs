﻿$('#stg_describe_content').on('show.bs.modal', function () {
    getLTGContent();
});

var collab_type_direction_id;
function getLTGContent() {
    //get content
    $.ajax({
        url: "/AcademicCollaboration/getLTGCContent",
        type: "GET",
        cache: false,
        data: {
            direction_id: 1, //going
            collab_type_id: 1, //short-term
            language_id: function () { return $('#stg_describe_content_language').val(); }
        },
        dataType: "json",
        success: function (data) {
            if (data != null) {
                if (data.success) {
                    collab_type_direction_id = data.obj.collab_type_direction_id;
                    $('#stg_describe_content_description').val(data.obj.description);
                } else {
                    toastr.error(data.content);
                }
            }
        },
        error: function () {
            toastr.error("Có lỗi xảy ra.");
        }
    });
}

$('#stg_describe_content_language').select2({
    placeholder: 'Ngôn ngữ',
    allowClear: true,
    minimumResultsForSearch: -1, //hide search box
    tags: true,
    ajax: {
        url: "/AcademicActivityType/getLanguages",
        delay: 250,
        cache: true,
        dataType: 'json',
        processResults: function (data) {
            data.obj.map(function (obj) {
                obj.id = obj.language_id;
                obj.text = obj.language_name;
                return data.obj;
            });
            return {
                results: data.obj
            };
        }
    },
    templateResult: formatLanguageInfor
}).on('select2:select', function () {
    getLTGContent();
});

function formatLanguageInfor(language) {
    return language.language_name;
}

$('#stg_describe_content_save').on('click', function () {
    $.ajax({
        url: "/AcademicCollaboration/updateLTGCContent",
        type: "POST",
        cache: false,
        data: {
            collab_type_direction_id: collab_type_direction_id,
            language_id: function () { return $('#stg_describe_content_language').val(); },
            description: function () { return $('#stg_describe_content_description').val(); }
        },
        dataType: "json",
        success: function (data) {
            if (data != null) {
                if (data.success) {
                    toastr.success(data.content);
                } else {
                    toastr.error(data.content);
                }
            }
        },
        error: function () {
            toastr.error("Có lỗi xảy ra.");
        }
    });
});