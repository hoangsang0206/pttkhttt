const showPaymentBtnLoader = (button) => {
    const loader_element = `<div class="loader-box">
        <svg class="button-loader" viewBox="0 0 40 40" height="20" width="20">
            <circle class="track" cx="20" cy="20" r="17.5" pathlength="100" stroke-width="4px" fill="none" />
            <circle class="car" cx="20" cy="20" r="17.5" pathlength="100" stroke-width="4px" fill="none" />
        </svg></span></div>`;
    const btnHeight = button.outerHeight();
    const btnElement = button.html();
    button.css('height', btnHeight);
    button.css('position', 'relative');
    button.html(loader_element);
    return btnElement;
}

const hidePaymentBtnLoader = (button, html) => {
    const timeout = setTimeout(() => {
        button.html(html);
        clearTimeout(timeout);
    }, 1000);
};

//---------------------------------
$('.order-info-summary').click(function() {
    var gender = $('input[name="gender"]:checked').val();
    var name = $('#UserFullName').val();
    var phone = $('#PhoneNumber').val();
    var shipMethod = $('input[name="shipmethod"]:checked').val();
    var address = $('#ship-address').val();
    var note = $('#cart-note').val();

    if (shipMethod != "COD") {
        address = "Store";
    }

    const btnElement = showPaymentBtnLoader($(this));

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
            hidePaymentBtnLoader($(this), btnElement);
            if (res.status === 200) {       
                window.location.href = res.redirectUrl;
            } else if (res.status === 400) {
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
                        + res.message + `</span>`;
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


$('.payment-action').click(function() {
    const paymentMethod = $('input[name="payment-method"]:checked').val();

    if (paymentMethod.length > 0) {
        showPaymentBtnLoader($(this));
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

$('.cancel-order').click(function() {
    Swal.fire({
        title: "Hủy đơn hàng?",
        text: "Bạn chắc chắn muốn hủy đơn hàng này?",
        icon: "question",
        showCancelButton: true,
        showConfirmButton: true,
        focusConfirm: false,
        cancelButtonText: "Thoát",
        confirmButtonText: "Xác nhận",
    }).then((result) => {
        if (result.isConfirmed) {
            const id = $(this).data('order');
            window.location.href = `/order/cancelorder?orderId=${id}`
        }
    });
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