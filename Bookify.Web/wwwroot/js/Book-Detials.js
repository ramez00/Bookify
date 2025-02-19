function OnAddRowCopySucess(item) {
    showSuccessMessage();
    $('#modal').modal('hide');

    $('#copyTableBody').prepend(item);
    var count = $('#TotalCopies');
    var newCount = parseInt(count.text()) + 1;
    count.text(newCount);

    $('.js-alert').addClass('d-none');
    $('#copyTable').removeClass('d-none');
}

function onEditCopySuccess(row) {
    showSuccessMessage();
    $('#modal').modal('hide');
    $(updatedRow).replaceWith(row);
}