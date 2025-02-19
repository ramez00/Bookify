
var selectedCopies = [];
var currentCopies = [];
var isEditMode = false;

$(document).ready(function () {

    if ($('.js-copy').length > 0) {
        isEditMode = true;
        prepareInput();
        currentCopies = selectedCopies;
    }

    $('.js-search').on('click', function (e) {
        e.preventDefault();

        var serialNumb = $('#Value').val();

        if (selectedCopies.find(c => c.serial == serialNumb)) {
            showErrorMessage(`You can not add same copy`);
            return;
        }

        if (selectedCopies.length >= maxSelectedCopies) {
            showErrorMessage(`You can not add more than ${maxSelectedCopies} book(s)`);
            return;
        }

        $('#SerchForm').submit();
    })


    $('body').delegate('.js-remove-btn', 'click', function () {

        var btn = $(this);
        var container = btn.parents('.js-row-copy');

        if (isEditMode) {

            btn.toggleClass('btn-danger btn-success js-remove-btn js-readd').text('Readd');
            container.find('img').css('opacity', '0.5');
            container.find('h4').css('text-decoration', 'line-through');
            container.find('.js-copy').toggleClass('js-copy js-removed').removeAttr('name');

        } else {
            container.remove();
        }

        prepareInput();

        if ($.isEmptyObject(selectedCopies) || JSON.stringify(currentCopies) == JSON.stringify(selectedCopies))
            $('#CopiesForm').find(':submit').addClass('d-none');
        else
            $('#CopiesForm').find(':submit').removeClass('d-none');
    })


    $('body').delegate('.js-readd', 'click', function () {

        var btn = $(this);
        var container = btn.parents('.js-row-copy');

        if (isEditMode) {

            btn.toggleClass('btn-success btn-danger js-readd js-remove-btn').text('Remove');
            container.find('img').css('opacity', '1');
            container.find('h4').css('text-decoration', 'none');
            container.find('.js-removed').toggleClass('js-removed js-copy');

        } else {
            container.remove();
        }

        prepareInput();

        if (JSON.stringify(currentCopies) == JSON.stringify(selectedCopies))
            $('#CopiesForm').find(':submit').addClass('d-none');
        else
            $('#CopiesForm').find(':submit').removeClass('d-none');
    })
})

function OnAddCopySuccess(copy) {
    $('#Value').val('');

    var bookId = $(copy).find('.js-copy').data('book-id');

    if (selectedCopies.find(c => c.bookId == bookId)) {
        showErrorMessage("You can not add same book");
        return;
    }

    $('#CopiesForm').prepend(copy);
    $('#CopiesForm').find(':submit').removeClass('d-none');
    prepareInput();
}

function prepareInput() {

    var copies = $('.js-copy');

    selectedCopies = [];
    $.each(copies, function (i, inpt) {
        var $input = $(inpt);
        selectedCopies.push({ serial: $input.val(), bookId: $(inpt).data('book-id') });
        $input.attr('name', `SelectedCopies[${i}]`);
    })

    console.log(selectedCopies);
}