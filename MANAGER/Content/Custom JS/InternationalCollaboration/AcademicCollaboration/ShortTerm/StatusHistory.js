$(document).on('show.bs.modal', '#status_history_modal', function (e) {
    let collab_id = $(e.relatedTarget).data('id');
    //load datatable
    $("#status_history_table").DataTable({
        oLanguage: {
            oPaginate: {
                sPrevious: "Trang trước",
                sNext: "Trang sau"
            },
            sEmptyTable: "Không có dữ liệu",
            sInfo: "Đang hiển thị từ _START_ đến _END_ của _TOTAL_ bản ghi",
        },
        ajax: {
            url: "/AcademicCollaboration/getStatusHistories",
            type: "GET",
            datatype: "json",
            data: {
                collab_id: collab_id
            },
            cache: "false"
        },
        destroy: true, //destroy datatable when re-load
        searching: false,
        lengthChange: false,
        serverSide: true,
        order: [1, "desc"],
        columns: [
            {
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                },
                orderable: false
            },
            {
                data: 'change_date',
                name: 'change_date',
                render: function (data, type) {
                    if (type === "sort" || type === "") {
                        return data;
                    }
                    return moment(data).format('DD-MM-YYYY hh:mm:ss A');
                }
            },
            {
                data: 'collab_status_id',
                name: 'collab_status_id',
                orderable: false
            },
            {
                data: 'full_name',
                name: 'full_name',
                orderable: false
            },
            {
                render: function (data, type, row) {
                    return `<a href=` + row.file_link + ` target="_blank">` + row.file_name + `</a>`;
                },
                orderable: false,
                className: 'text-center'
            },
            {
                data: 'note',
                name: 'note',
                orderable: false
            }
        ],
        columnDefs: [
            {
                targets: [0, 1, 2, 3],
                className: 'text-nowrap text-center',
            },
            {
                targets: 2,
                render: function (data) {
                    var status = {
                        1: {
                            'title': 'Đề xuất',
                            'class': 'label-inline'
                        },
                        2: {
                            'title': 'Đang thực hiện',
                            'class': 'label-warning'
                        },
                        3: {
                            'title': 'Không hoàn thành',
                            'class': 'label-danger'
                        },
                        4: {
                            'title': 'Đã hoàn thành',
                            'class': 'label-success'
                        }
                    };
                    if (typeof status[data] === 'undefined') {
                        return data;
                    }
                    return '<span class="label label-pill font-weight-bold ' + status[data].class + ' label-inline">' + status[data].title + '</span>';
                }
            }
        ],
        initComplete: function () {
            $(this).parent().css('overflow-x', 'auto');
        }
    });

});

$(document).on('hide.bs.modal', '#status_history_modal', function () {

});
