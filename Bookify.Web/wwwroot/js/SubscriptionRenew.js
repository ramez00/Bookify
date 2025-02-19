$(document).ready(function () {
    $('.js-renew').on('click', function () {
        var subscriperKey = $(this).data('subkey');
        bootbox.confirm({
            message: 'Are you sure that you need to renew this subscription ?',
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
                        url: '/Subscribers/RenewSubscription?subKey=' + subscriperKey,
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (row) {
                            $('#subscribtionTbl').find('tbody').append(row);
                            var activeIcon = $('#SubscriberActiveIcon');
                            activeIcon.removeClass('d-none');
                            activeIcon.siblings('svg').remove();
                            activeIcon.parents('.card').removeClass('bg-warning').addClass('bg-success');

                            $('#AddRentalBtn').removeClass('d-none');

                            $('#subscriberStats').text('Active Subscriber');
                            var badge = $('#subscriberBadge');
                            badge.removeClass('badge-light-warning').addClass('badge-light-success');
                            badge.text('Active subscriber');
                            showSuccessMessage();
                        },
                        Error: function (err) {
                            showErrorMessage();
                        }
                    });
                }
            }
        });
    })

    console.log(currentRentals);
    $('.js-cancel-rental').on('click', function () {

        var btn = $(this);

        bootbox.confirm({
            message: 'Are you sure that you need to Cancel this Rental ?',
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
                        url: '/Rental/CancelRental/' + btn.data('id'),
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function () {
                            currentRentals = currentRentals - btn.parents('tr').find('.js-rentals-number').text();
                            console.log("new rental  =>" +currentRentals);
                            $('.js-total-number-rentals').text(currentRentals);

                            btn.parents('tr').remove();
                            var len = $('#rentalTbl tbody tr').length;

                            if (len === 0) {
                                $('#rentalTbl').fadeOut(function () {
                                    $('#alert').removeClass('d-none');
                                    $('#alert').fadeIn();
                                })
                            }
                            showSuccessMessage();

                                
                        },
                        Error: function (err) {
                            showErrorMessage();
                        }
                    });
                }
            }
        });
    })
})