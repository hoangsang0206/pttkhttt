//---------------------------------
$('.order-info-summary').click(() => {
    var gender = $('input[name="gender"]:checked').val();
    var name = $('#UserFullName').val();
    var phone = $('#PhoneNumber').val();
    var shipMethod = $('input[name="shipmethod"]:checked').val();
    var address = $('#ship-address').val();
    var note = $('#cart-note').val();

    if (shipMethod != "COD") {
        address = "Store";
    }

    $.ajax({
        type: 'POST',
        url: '/order/checkorderinfo',
        data: {
            gender: gender,
            customerName: name,
            customerPhone: phone,
            address: address,
            note: note
        },
        success: (res) => {
            if (res.status === 200) {       
                window.location.href = res.redirectUrl;
            } else if(res.status === 400) {
                if (res.error != null) {
                    let str = ``;
                    res.error.map(value => {
                        console.log(value)
                        str += `<li><i class="fa-solid fa-circle-exclamation"></i><span>${value}</span></li>`;
                    })

                    showOrderNotice(str);
                } else {
                    $('.cart-info .form-error').empty();
                    $('.cart-info .form-error').show();
                    var str = `<span>
                    <i class="fa-solid fa-circle-exclamation"></i>`
                        + res.error + `</span>`;
                    $('.cart-info .form-error').append(str);

                    setTimeout(function () {
                        $('.cart-info .form-error').hide();
                    }, 8000)
                }
            }
        },
        error: () => {  }
    })
})


$('.payment-action').click(() => {
    var paymentMethod = $('input[name="payment-method"]:checked').val();

    if (paymentMethod.length > 0) {
        $.ajax({
            type: 'POST',
            url: '/order/checkout',
            data: {
                paymentMethod: paymentMethod
            },
            success: (res) => {
                if (res.status === 200) {
                    window.location.href = res.redirectUrl;
                }
            },
            error: () => { console.log("Payment Error") }
        })
    }

})

//--------------
function showOrderNotice(string) {
    $('.notice-list').empty();
    $('.checkout-notice-wrapper').css('visibility', 'visible');
    $('.checkout-notice').addClass('showNotice');

    $('.notice-list').append(string);

    $('.close-notice').click(() => {
        $('.notice-list').empty();
        $('.checkout-notice-wrapper').css('visibility', 'hidden');
        $('.checkout-notice').removeClass('showNotice');
    })
}