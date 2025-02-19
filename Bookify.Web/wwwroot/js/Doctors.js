$(document).ready(function () {
    $('#DateRange').on('DOMSubtreeModified', function () {
        var selectedRange = $(this).html();

        if (selectedRange !== '') {
            var dateRange = selectedRange.split(' - ');
            $('#From').val(dateRange[0]);
            $('#To').val(dateRange[1]);
        }
    });
})
var start = moment().subtract(29, "days");
var end = moment();
function cb(start, end) {
    $("#DateRange").html(start.format("MMMM D, YYYY") + " - " + end.format("MMMM D, YYYY"));
}
$("#DateRange").daterangepicker({
    startDate: start,
    endDate: end
}, cb);

cb(start, end);