﻿//3.EDIT MODAL
var uppy2; //init uppy2
var file_action;
//3.1.Init select2 for select element in edit modal
///3.2.1.THÔNG TIN CÁ NHÂN
$('#going_edit_officer_name').select2({
    placeholder: 'Họ và tên',
    allowClear: true,
    tags: true,
    ajax: {
        url: '/AcademicCollaboration/getPeople',
        delay: 250,
        cache: true,
        dataType: 'json',
        data: function (params) {
            return {
                person_name: params.term
            };
        },
        processResults: function (data) {
            data.obj.map(function (obj) {
                obj.id = obj.name + "/" + obj.people_id;
                obj.text = obj.name;
                return data.obj;
            });

            return {
                results: data.obj
            };
        }
    },
    templateResult: formatPersonInfo
}).on("select2:select", function () { //after select option
    checkPersonEdit();
}).on("select2:unselecting", function () {
    //after clear tag
    //clear email & office
    $('#going_edit_officer_email').val("");
    $('#going_edit_officer_facility').val(null).trigger('change');
    //enable email & office
    $('#going_edit_officer_email').prop('disabled', false);
    $('#going_edit_officer_facility').prop('disabled', false);
});

function checkPersonEdit() {
    person = $('#going_edit_officer_name').val();
    person_name = person.split('/')[0] === undefined ? '' : person.split('/')[0];
    person_id = person.split('/')[1] === undefined ? 0 : person.split('/')[1];

    $.ajax({
        url: "/AcademicCollaboration/checkPerson",
        type: "POST",
        data: {
            people_id: person_id,
            people_name: person_name
        },
        cache: false,
        dataType: "json",
        success: function (data) {
            if (data != null) {
                //clear content
                $('#going_edit_officer_email').val("");
                $('#going_edit_officer_facility').val(null).trigger('change');
                if (data.obj != null) {
                    //avaiable person data in DB
                    available_person = true; //using for Add||Edit
                    if (data.success) {
                        //auto fill data
                        let p = data.obj;
                        $('#going_edit_officer_email').val(p.email);
                        if (!(isEmptyOrNullOrUndefined(p.office_id))) {
                            $('#going_edit_officer_facility').append(new Option(p.office_name, p.office_id, false, true)).trigger('change');
                        }
                        //disable email & office
                        $('#going_edit_officer_email').prop('disabled', true);
                        $('#going_edit_officer_facility').prop('disabled', true);
                    } else {
                        toastr.error(data.content);
                    }
                } else {
                    //new person case
                    available_person = false;
                    ///enable email and office select
                    $('#going_edit_officer_email').prop('disabled', false);
                    $('#going_edit_officer_facility').prop('disabled', false);
                }
            } else {
                toastr.error("Kiểm tra cán bộ giảng viên có lỗi xảy ra.");
            }
        },
        error: function () {
            toastr.error("Kiểm tra cán bộ giảng viên có lỗi xảy ra.");
        }
    });
}

$('#going_edit_officer_facility').select2({
    placeholder: 'Đơn vị công tác',
    allowClear: true,
    ajax: {
        url: '/AcademicCollaboration/getOffices',
        delay: 250,
        cache: true,
        dataType: 'json',
        data: function (params) {
            return {
                office_name: params.term
            };
        },
        processResults: function (data) {
            data.obj.map(function (obj) {
                obj.id = obj.office_id;
                obj.text = obj.office_name;
                return data.obj;
            });

            return {
                results: data.obj
            };
        }
    },
    templateResult: formatOfficeInfo
});

//3.2.2.ĐƠN VỊ ĐÀO TẠO
$('#going_edit_officer_traning').select2({
    placeholder: 'Đơn vị đào tạo',
    allowClear: true,
    tags: true,
    ajax: {
        url: '/AcademicCollaboration/getPartners',
        delay: 250,
        cache: true,
        dataType: 'json',
        data: function (params) {
            return {
                partner_name: params.term
            };
        },
        processResults: function (data) {
            data.obj.map(function (obj) {
                obj.id = obj.partner_name + '/' + obj.partner_id;
                obj.text = obj.partner_name;
                return data.obj;
            });

            return {
                results: data.obj
            };
        }
    },
    templateResult: formatPartnerInfo
}).on("select2:select", function () { //after select partner
    //check partner
    checkPartnerEdit();
}).on("select2:unselecting", function () {
    //after clear tag
    //clear country select
    $('#going_edit_officer_nation').val(null).trigger('change');
    ///enable country
    $('#going_edit_officer_nation').prop('disabled', false);
});

function checkPartnerEdit() {
    //process partner_name
    partner = $('#going_edit_officer_traning').val();
    partner_name = partner.split('/')[0] === undefined ? '' : partner.split('/')[0];
    partner_id = partner.split('/')[1] === undefined ? 0 : partner.split('/')[1];

    $.ajax({
        url: "/AcademicCollaboration/checkPartner",
        type: "POST",
        data: {
            partner_name: partner_name,
            partner_id: partner_id
        },
        cache: false,
        dataType: "json",
        success: function (data) {
            if (data != null) {
                //clear content
                $('#going_edit_officer_nation').val(null).trigger('change');
                if (data.obj != null) {
                    //available partner data in DB
                    available_partner = true; //using for Add||Edit
                    if (data.success) {
                        //auto fill data
                        let p = data.obj;
                        $('#going_edit_officer_nation').append(new Option(p.country_name, p.country_id, false, true)).trigger('change');
                        //disable country
                        $('#going_edit_officer_nation').prop('disabled', true);
                    } else {
                        toastr.error(data.content);
                    }
                } else {
                    //new partner case
                    available_partner = false;
                    ///enable country
                    $('#going_edit_officer_nation').prop('disabled', false);
                }
            } else {
                toastr.error("Kiểm tra đối tác có lỗi xảy ra.");
            }
        },
        error: function () {
            toastr.error("Kiểm tra đối tác có lỗi xảy ra.");
        }
    });
}

$('#going_edit_officer_nation').select2({
    placeholder: 'Quốc gia',
    allowClear: true,
    ajax: {
        url: '/AcademicCollaboration/getCountries',
        delay: 250,
        cache: true,
        dataType: 'json',
        data: function (params) {
            return {
                country_name: params.term
            };
        },
        processResults: function (data) {
            data.obj.map(function (obj) {
                obj.id = obj.country_id;
                obj.text = obj.country_name;
                return data.obj;
            });

            return {
                results: data.obj
            };
        }
    },
    templateResult: formatCountryInfo
});

$('#going_edit_officer_coop_scope').select2({
    placeholder: 'Phạm vi hợp tác',
    allowClear: true,
    minimumResultsForSearch: -1, //hide search box
    ajax: {
        url: '/AcademicCollaboration/getCollabScopes',
        delay: 250,
        cache: true,
        dataType: 'json',
        data: function () {
            return {
                collab_abbreviation_name: 'FE' //faculty exchange
            };
        },
        processResults: function (data) {
            data.obj.map(function (obj) {
                obj.id = obj.scope_id;
                obj.text = obj.scope_name;
                return data.obj;
            });

            return {
                results: data.obj
            };
        }
    },
    templateResult: formatCollabScope
});

function formatCollabScope(scope) {
    return scope.scope_abbreviation + " - " + scope.scope_name;
}

///3.2.3.CHI TIẾT
$('#going_edit_officer_status').select2({
    placeholder: 'Trạng thái',
    allowClear: true,
    minimumResultsForSearch: -1, //hide search box
    tags: true,
    ajax: {
        url: '/AcademicCollaboration/getAcadCollabStatus',
        delay: 250,
        cache: true,
        dataType: 'json',
        data: function () {
            return {
                status_type_specific: 1 //short-term
            };
        },
        processResults: function (data) {
            data.obj.map(function (obj) {
                obj.id = obj.collab_status_id;
                obj.text = obj.collab_status_name;
                return data.obj;
            });

            return {
                results: data.obj
            };
        }
    },
    templateResult: formatAcadCollabStatus
});

//3.2.GET CORESSPONDING DATA
$('#going_edit_officer').on('show.bs.modal', function (e) {
    let acad_collab_id = $(e.relatedTarget).data('acad_collab_id');
    file_action = 'none';
    $.ajax({
        url: "/AcademicCollaboration/getAcademicCollaboration",
        type: "GET",
        data: {
            direction: 1, /*going*/
            collab_type_id: 1 /*short-term*/,
            acad_collab_id: acad_collab_id
        },
        cache: false,
        dataType: "json",
        success: function (data) {
            if (data != null) {
                if (data.success) {
                    let acadCollab = data.obj;
                    //fill data to correspoding select, input
                    available_person = true; //person had data in db
                    $("#going_edit_officer_name").append(new Option(acadCollab.people_name, acadCollab.people_name + '/' + acadCollab.people_id, false, true)).trigger('change');
                    $("#going_edit_officer_email").val(acadCollab.email);
                    $("#going_edit_officer_email").prop("disabled", true);
                    if (!(isEmptyOrNullOrUndefined(acadCollab.office_id))) {
                        $("#going_edit_officer_facility").append(new Option(acadCollab.office_name, acadCollab.office_id, false, true)).trigger('change');
                        $("#going_edit_officer_facility").prop("disabled", true);
                    }

                    available_partner = true; //partner had data in db
                    $("#going_edit_officer_traning").append(new Option(acadCollab.partner_name, acadCollab.partner_name + '/' + acadCollab.partner_id, false, true)).trigger('change');
                    $("#going_edit_officer_nation").append(new Option(acadCollab.country_name, acadCollab.country_id, false, true)).trigger('change');
                    $("#going_edit_officer_nation").prop("disabled", true);
                    $("#going_edit_officer_coop_scope").append(new Option(acadCollab.scope_name, acadCollab.scope_id, false, true)).trigger('change');

                    $("#going_edit_officer_status").append(new Option(acadCollab.collab_status_name, acadCollab.collab_status_id, false, true)).trigger('change');
                    $("#going_edit_officer_start_plan_date").val(moment(acadCollab.plan_study_start_date).format("DD/MM/YYYY"));
                    $("#going_edit_officer_end_plan_date").val(moment(acadCollab.plan_study_end_date).format("DD/MM/YYYY"));

                    $("#going_edit_officer_start_date").val(acadCollab.actual_study_start_date == null ? "" : moment(acadCollab.actual_study_start_date).format("DD/MM/YYYY"));
                    $("#going_edit_officer_end_date").val(acadCollab.actual_study_end_date == null ? "" : moment(acadCollab.actual_study_end_date).format("DD/MM/YYYY"));

                    
                    collab_id = acadCollab.collab_id;
                    //load available exist file
                    let elemId = 'going_edit_officer_upload';
                    let id = '#' + elemId;
                    let $statusBar = $(id + ' .uppy-status');
                    let $uploadedList = $(id + ' .uppy-list');
                    let uploadListHtml = '<div class="uppy-list-item" data-id="' + acadCollab.file_id + '"><div class="uppy-list-label"><a target="_blank" href=' + acadCollab.file_link + '>' + acadCollab.file_name + '</a></div><span class="uppy-list-remove" data-id="' + acadCollab.file_id + '"><i class="flaticon2-cancel-music"></i></span></div>';
                    if (acadCollab.file_id != null) {
                        $uploadedList.append(uploadListHtml);
                        $statusBar.addClass('uppy-status-hidden');
                        $statusBar.removeClass('uppy-status-ongoing');
                    }
                    $("#going_edit_officer_note").val(acadCollab.note);
                } else {
                    toastr.error(data.content);
                }
            }
        },
        error: function () {
            toastr.error("Có lỗi xảy ra.");
        }
    })
});

var available_person;
var available_partner;
//3.3.EDIT SAVE BUTTON
var going_edit_officer_save = new LoaderBtn($('#going_edit_officer_save'));
$('#going_edit_officer_save').on('click', function () {
    //person
    let person = $('#going_edit_officer_name').val();

    let person_email = $('#going_edit_officer_email').val();
    let person_profile_office_id = $('#going_edit_officer_facility').val();

    //partner
    let partner = $('#going_edit_officer_traning').val();

    let partner_country_id = $('#going_edit_officer_nation').val();

    let collab_scope_id = $('#going_edit_officer_coop_scope').val();

    //detail
    let status_id = $('#going_edit_officer_status').val();

    let plan_start_date = $('#going_edit_officer_start_plan_date').val();
    let plan_end_date = $('#going_edit_officer_end_plan_date').val();

    let actual_start_date = $('#going_edit_officer_start_date').val();
    let actual_end_date = $('#going_edit_officer_end_date').val();

    let evidence = uppy2.getFiles();

    let note = $('#going_edit_officer_note').val();

    //check empty
    if (isEmptyOrNullOrUndefined(person) || isEmptyOrNullOrUndefined(person_email)
        || isEmptyOrNullOrUndefined(partner) || isEmptyOrNullOrUndefined(partner_country_id)
        || isEmptyOrNullOrUndefined(collab_scope_id) || isEmptyOrNullOrUndefined(status_id)
        || isEmptyOrNullOrUndefined(plan_start_date) || isEmptyOrNullOrUndefined(plan_end_date)) {
        return toastr.warning("Chưa chọn đủ trường thông tin bắt buộc.");
    } else {
        let person_name = person.split('/')[0];
        let person_id = person.split('/')[1]
        let partner_name = partner.split('/')[0];
        let partner_id = partner.split('/')[1];

        let obj_person = objPerson(available_person, person_name, person_id, person_email, person_profile_office_id);

        console.log(obj_person);

        let obj_partner = objPartner(available_partner, partner_name, partner_id, partner_country_id, collab_scope_id);

        let obj_academic_collab = objAcadCollab(collab_id, status_id, plan_start_date, plan_end_date, actual_start_date, actual_end_date, 'true', note);

        //validate datepicker from - to
        if (!datePickerFromToValidate(plan_start_date, plan_end_date) || !datePickerFromToValidate(actual_start_date, actual_end_date)) {
            return toastr.error("`TG đi học` không được vượt quá `TG kết thúc`.")
        } else {
            //start load
            going_edit_officer_save.startLoading();

            let formData = new FormData();
            formData.append("new_evidence", evidence.length == 0 ? null : evidence[0].data);
            formData.append("file_action", file_action);
            formData.append("folder_name", person_name + " - " + partner_name);

            formData.append("direction_id", 1); //going case
            formData.append("collab_type_id", 1); //short-term

            let obj_person_stringify = JSON.stringify(obj_person);
            let obj_partner_stringify = JSON.stringify(obj_partner);
            let obj_academic_collab_stringify = JSON.stringify(obj_academic_collab);

            formData.append("obj_person_stringify", obj_person_stringify);
            formData.append("obj_partner_stringify", obj_partner_stringify);
            formData.append("obj_academic_collab_stringify", obj_academic_collab_stringify);

            //Save actually
            $.ajax({
                url: "/AcademicCollaboration/updateAcademicCollaboration",
                type: "POST",
                data: formData,
                cache: false,
                dataType: "json",
                processData: false,
                contentType: false,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.content);
                        exchange_going_table.ajax.reload();
                        going_edit_officer_save.stopLoading();
                        $("#going_edit_officer_close").click();
                        clearContentEditModal();
                    } else {
                        toastr.warning(data.content);
                        going_edit_officer_save.stopLoading();
                    }
                },
                error: function () {
                    toastr.error("Có lỗi xảy ra khi upload file.");
                    going_edit_officer_save.stopLoading();
                }
            });
            for (var pair of formData.entries()) {
                console.log(pair[0] + ', ' + pair[1]);
            }
        }
    }
});

$('#going_edit_officer').on('hide.bs.modal', function () {
    clearContentEditModal();
});

function clearContentEditModal() {
    //enable input and select
    $('#going_edit_officer_email').prop('disabled', false);
    $("#going_edit_officer_facility").prop('disabled', false);
    $('#going_edit_officer_nation').prop('disabled', false);
    $('#going_edit_officer_coop_scope').prop('disabled', false);

    //clear data
    ///THÔNG TIN CÁ NHÂN
    $('#going_edit_officer_name').val(null).trigger('change');
    $('#going_edit_officer_email').val('');
    $('#going_edit_officer_facility').val(null).trigger('change');
    ///ĐƠN VỊ ĐÀO TẠO
    $('#going_edit_officer_traning').val(null).trigger('change');
    $('#going_edit_officer_nation').val(null).trigger('change');
    $('#going_edit_officer_coop_scope').val(null).trigger('change');
    ///CHI TIẾT
    $('#going_edit_officer_status').val(null).trigger('change');
    $('#going_edit_officer_start_plan_date').val('');
    $('#going_edit_officer_end_plan_date').val('');

    //clear upload file
    $('#going_edit_officer_upload .uppy-list').html('');
    uppy2.removeFile(uppy2.getFiles().length == 0 ? '' : uppy2.getFiles()[0].id);

    $('#going_edit_officer_start_date').val('');
    $('#going_edit_officer_end_date').val('');
    $('#going_edit_officer_note').val('');
}

