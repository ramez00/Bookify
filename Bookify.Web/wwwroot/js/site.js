
var updatedRow;
var table;
var datatable;
var ExportedCol = [];

function DisableBtnWhenProcessing() {
    $('.js-submit-btn').attr('disabled', 'disabled').attr('data-kt-indicator', 'on');
}
function OnModalBegin() {
    DisableBtnWhenProcessing(); 
}
function showSuccessMessage() {
    toastr.success("Saved Success");
}

function showErrorMessage(msg) {
    var msgText = typeof msg == 'object' && msg != undefined ? msg.responseText : msg != undefined ? msg : "Error Occured";
    console.log(msgText);
    toastr.error(msgText);
}

function OnModalSucess(item) {
    showSuccessMessage();
    $('#modal').modal('hide');

    if (updatedRow !== undefined) {
        datatable.row(updatedRow).remove().draw();
        updatedRow = undefined;
    }

    var newRow = $(item);
    datatable.row.add(newRow).draw();

}

function OnModalComplete() {
    $('.js-submit-btn').removeAttr('disabled').removeAttr('data-kt-indicator');
}


function OnResetPasswordSucess(obj) {
    showSuccessMessage();
    $('#modal').modal('hide');
    updatedRow.removeClass('animate__animated animate__flash');
    var row = updatedRow;
    var status = row.find('.js-status');
    var newStatus = status.text().trim() === 'Deleted' ? 'Avliable' : 'Avliable' ;
    var changedStatus  = status.text(newStatus);
    if (changedStatus.hasClass('badge-light-danger')) {
        changedStatus.removeClass('badge-light-danger');
    }
    changedStatus.addClass('badge-light-success');
    row.find('.js-updated-on').html(obj);
    if (row.hasClass('animate__animated animate__flash')) {
        row.removeClass('animate__animated animate__flash');
    }
    row.addClass('animate__animated animate__flash');
}

function HandelSelect2() {
    $('.js-select').select2({
        placeholder: "Select..."
    });
    $('.js-select').on('select2:select', function (e) {
        $('form').not('#signOut').validate().element('#' + $(this).attr('id'));
    })
}

var headers = $('th');
$.each(headers, function (i) {
    if (!$(this).hasClass('js-exclude-export'))
        ExportedCol.push(i);
})
var KTDatatables = function () {
    // Shared variables


    // Private functions
    var initDatatable = function () {
        // Set date data order
        const tableRows = table.querySelectorAll('tbody tr');

        tableRows.forEach(row => {
            const dateRow = row.querySelectorAll('td');
            const realDate = moment(dateRow[3].innerHTML, "DD MMM YYYY, LT").format(); // select date from 4th column in table
            dateRow[3].setAttribute('data-order', realDate);
        });

        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'pageLength': 10,
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $('.js-datatable').data('document-title');
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: ExportedCol
                    }
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                    columns: ExportedCol
                    }
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: ExportedCol
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: ExportedCol
                    },
                    customize: function (doc) {
                        pdfMake.fonts = {
                            Arial: {
                                normal: 'arial',
                                bold: 'arial',
                                italics: 'arial',
                                bolditalics: 'arial'
                            }
                        }
                        doc.defaultStyle.font = 'Arial';
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Public methods
    return {
        init: function () {
            table = document.querySelector('.js-datatable');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            //handleSearchDatatable();
        }
    };
}();

$(document).ready(function () {

    $('form').not('#signOut').not('.js-exclude-validation').on('submit', function () {
        var isValid = $(this).valid();
        if (isValid) DisableBtnWhenProcessing();
    })

    HandelSelect2();

    $('.js-datepicker').daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        drops: "up",
        maxDate: new Date()
    })
    KTUtil.onDOMContentLoaded(function () {
        KTDatatables.init();
    });

    $('body').delegate('.js-render-modal','click', function () {
        var btn = $(this);
        var modal = $('#modal');
        var title = btn.data('title');

        if (title == 'Rental History')
            modal.find('.modal-dialog').addClass('modal-xl');


        modal.find('#modalLabl').text(btn.data('title'));

        if (btn.data('update') !== undefined) {
            updatedRow = btn.parents('tr');
        }

        $.get({
            url: btn.data('url'),
            success: function (form) {
                modal.find('#modalBody').html(form);
                $.validator.unobtrusive.parse(modal);
                HandelSelect2();
            },
            error: function () {
                toastr.error("asd");
            }
        })
        modal.modal('show');
    })

    // Unlock User
    $('body').delegate('.js-unlock-user', 'click', function () {
        var btn = $(this);
        btn.parents('tr').removeClass('animate__animated animate__flash');

        bootbox.confirm({
            size: 'small',
            message: btn.data('message'),
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: btn.data('url'),
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (obj) {
                            var row = btn.parents('tr');
                            row.find('.js-updated-on').html(obj);
                            row.addClass('animate__animated animate__flash');
                            toastr.success('User Unlocked successfuly');
                        }
                    });
                }
            }
        });
    })

    //Toggle Status
    $('body').delegate('.js-toggle-status', 'click', function () {
        var btn = $(this);
        btn.parents('tr').removeClass('animate__animated animate__flash');

        bootbox.confirm({
            size: 'small',
            message: 'Are you sure to change toggle status?',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: btn.data('url'),
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (obj) {
                            var row = btn.parents('tr');
                            var status = row.find('.js-status');
                            var newStatus = status.text().trim() === 'Deleted' ? 'Avliable' : 'Deleted';
                            status.text(newStatus).toggleClass('badge-light-success badge-light-danger');
                            row.find('.js-updated-on').html(obj);
                            row.addClass('animate__animated animate__flash');
                            toastr.success('Toggle status Changed successfuly');
                        }
                    });
                }
            }
        });
    })
})