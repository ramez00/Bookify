$(document).ready(function(){

    $('.page-link').on('click', function () {
        var btn = $(this);
        var pagNumber = btn.data('page-number');

        if (btn.parent().hasClass('active'))
            return;
        $('#pageNumber').val(pagNumber);
        $('#reportForm').submit();
    })
})