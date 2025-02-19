$(document).ready(function () {
    $('#GovernorateId').on('change', function () {
        var GovId = $(this).val();

        var AreaList = $('#AreaId');

        AreaList.empty();
        AreaList.append('<option></option>');

        if (GovId !== '') {
            $.ajax({
                url: '/Subscribers/GetAreas?GovId=' + GovId,
                success: function (areas) {
                    $.each(areas, function (i,area) {
                        var item = $('<option></option>').attr("value", area.value).text(area.text);
                        AreaList.append(item);
                    })
                },
                error: function () {
                    showErrorMessage();
                }
            })
        }
    })
}) 