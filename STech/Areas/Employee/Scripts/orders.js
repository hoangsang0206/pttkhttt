if (window.innerWidth < 768) {
    $('.sidebar').addClass('close');
}

$('.toggle').click(() => {
    $('.sidebar').toggleClass('close');
})

function hideLoading() {
    var interval = setInterval(() => {
        $('.loading').hide();
        clearInterval(interval);
    }, 600);
}

function showLoading() {
    $('.loading').css('display', 'grid');
}

function activeButton(button) {
    $('.page-btn').removeClass('active');
    button.addClass('active');
}

function getOrderCount() {
    $.ajax({
        type: 'GET',
        url: '/api/orders',
        data: {
            t: 'count'
        },
        success: (data) => {
            if (data > 0) {
                $('.new-order-count').css('display', 'grid');
                $('.new-order-count').text(data);
                console.log(data)
            }
            else {
                $('.new-order-count').hide();
            }
        }
    })
}

getOrderCount();

$(document).on('click', '.order-print-btn', function () {
    const orderId = $(this).data('order');
    if (orderId) {
        $.ajax({
            type: 'GET',
            url: `/employee/orders/printinvoice?orderID=${orderId}`,
            xhrFields: {
                responseType: 'blob'
            },
            success: (response) => {
                var link = document.createElement('a');
                link.href = window.URL.createObjectURL(response);
                link.download = orderId + ".pdf";
                link.click();
            },
            error: () => {
                console.log('Cannot download file');
            }
        })
    }
})


$('.dropdown-search-main').click(() => {
    $('.dropdown-search-list').toggleClass('show');
})

$('.order-search-value').click((e) => {
    var text = $(e.target).text();
    var value = $(e.target).data('search-select');

    $('.dropdown-search-selected').text(text);
    $('.dropdown-search-selected').data('search', value);
    $('.dropdown-search-list').removeClass('show');
})


function appendOrderList(res, parent_element) {
    parent_element.empty();
    const strHead = `<tr> <th>Mã ĐH</th><th>Tên khách hàng</th>
                    <th>Ngày đặt</th><th>Tổng tiền</th><th>Trạng thái thanh toán</th>
                    <th>Trạng thái</th><th></th></tr>`;
    parent_element.append(strHead);
    if (res.length > 0) {
        res.map(order => {
            const date = new Date(order.NgayDat);
            const dateFormat = date.toLocaleDateString('en-GB') + ' ' + date.toLocaleTimeString('en-US');

            let paymentStatusClass = "order-waiting";
            let paymentText = "Chưa thanh toán";
            if (order.TrangThaiThanhToan === "failed") {
                paymentStatusClass = "order-failed";
                paymentText = "T.toán thất bại";
            }
            else if (order.TrangThaiThanhToan === "paid") {
                paymentStatusClass = "order-success";
                paymentText = "Đã thanh toán";
            }

            let statusClass = "order-waiting";
            let statusText = "Chờ xác nhận";
            if (order.TrangThai === "cancelled") {
                statusClass = "order-failed";
                statusText = "Đã hủy";
            }
            else if (order.TrangThai === "confirmed") {
                statusClass = "order-success";
                statusText = "Đã xác nhận";
            }

            var str = `<tr>
                            <td><div class="order-id">${order.MaHD}</div></td>
                            <td><div class="cus-name">${order.KhachHang.HoTen}</div></td>
                            <td><div class="order-date">${dateFormat}</div></td>
                            <td><div class="total-payment">${order.TongTien.toLocaleString('vi-VN') + 'đ'}</div></td>
                            <td><div class="order-pstatus d-flex ${paymentStatusClass}">${paymentText}</td>`;
            if (order.TrangThai == "unconfirmed") {
                str += `<td>
                    <div class="order-status">
                        <button class="order-btn order-status-accept" data-order="${order.MaHD}">Xác nhận</button>
                        <button class="order-btn order-status-refuse" data-order="${order.MaHD}">Hủy ĐH</button>
                    </div>
                </td>`;
            }
            else {
                str += `<td><div class="order-status ${statusClass}">${statusText}</div></td>`;
            }
            str += `<td>
                <div class="order-button-box d-flex justify-content-end flex-wrap gap-2">
                    <button class="order-btn order-print-btn" data-order="${order.MaHD}">In HĐ</button>
                    <button class="order-btn order-detail-btn" data-order="${order.MaHD}">Chi tiết</button>
                </div>
            </td>`;

            parent_element.append(str);
        })
    }
}

$('.search-orders').submit((e) => {
    e.preventDefault();
    var searchType = $('.dropdown-search-selected').data('search');
    var searchVal = $('#search-orders').val();
    if (searchType.length > 0 && searchVal.length > 0) {
        showLoading();
        $('.order-list table tbody').empty();
        $.ajax({
            type: 'get',
            url: '/api/orders',
            data: {
                searchType: searchType,
                searchValue: searchVal
            },
            success: (res) => {
                hideLoading();
                appendOrderList(res);
            },
            error: (err) => { console.log(err) }
        })
    }
})

$('.search-by-date-btn').click(() => {
    var dateFrom = $('#date-from').val();
    var dateTo = $('#date-to').val();

    if (dateFrom.length > 0 && dateTo.length > 0) {
        showLoading();
        $('.order-list table tbody').empty();
        $.ajax({
            type: 'get',
            url: '/api/orders',
            data: {
                dateFrom: dateFrom,
                dateTo: dateTo
            },
            success: (res) => {
                hideLoading();
                appendOrderList(res);
            },
            error: (err) => {
                console.log(err);
            }
        })
    }
})

function reloadOrders() {
    $.ajax({
        type: 'GET',
        url: '/api/orders',
        success: (res) => {
            hideLoading();
            appendOrderList(res, $('.order-waiting-list table tbody'));
        },
        error: () => { }
    })
}


$('.reload-orders').click(function() {
    $('.page-btn').removeClass('active');
    showLoading();
    reloadOrders();
})

$('.get-confirmed').click(function() {
    activeButton($(this));
    showLoading();
    $.ajax({
        type: 'GET',
        url: '/api/orders',
        data: {
            status: 'confirmed'
        },
        success: (res) => {
            hideLoading();
            appendOrderList(res, $('.order-waiting-list table tbody'));
        },
        error: () => { }
    })
})

$('.get-cancelled').click(function() {
    activeButton($(this));
    showLoading();
    $.ajax({
        type: 'GET',
        url: '/api/orders',
        data: {
            status: 'cancelled'
        },
        success: (res) => {
            hideLoading();
            appendOrderList(res, $('.order-waiting-list table tbody'));
        },
        error: () => { }
    })
})

$('.get-paid').click(function() {
    activeButton($(this));
    showLoading();
    $.ajax({
        type: 'GET',
        url: '/api/orders',
        data: {
            pstatus: 'paid'
        },
        success: (res) => {
            hideLoading();
            appendOrderList(res, $('.order-waiting-list table tbody'));
        },
        error: () => { }
    })
})

$('.get-unpaid').click(function() {
    activeButton($(this));
    showLoading();
    $.ajax({
        type: 'GET',
        url: '/api/orders',
        data: {
            pstatus: 'unpaid'
        },
        success: (res) => {
            hideLoading();
            appendOrderList(res, $('.order-waiting-list table tbody'));
        },
        error: () => { }
    })
})

$('.get-today-unconfirmed').click(function () {
    activeButton($(this));
    showLoading();
    $.ajax({
        type: 'GET',
        url: '/api/orders',
        data: {
            todayStatus: 'unconfirmed'
        },
        success: (res) => {
            hideLoading();
            appendOrderList(res, $('.order-waiting-list table tbody'));
        },
        error: () => { }
    })
})

$('.get-unconfirmed').click(function () {
    activeButton($(this));
    showLoading();
    $.ajax({
        type: 'GET',
        url: '/api/orders',
        data: {
            status: 'unconfirmed'
        },
        success: (res) => {
            hideLoading();
            appendOrderList(res, $('.order-waiting-list table tbody'));
        },
        error: () => { }
    })
})

//Change order status ------------------
$(document).on('click', '.order-status-accept', function() {
    const orderID = $(this).data('order');
    if (orderID.length > 0) {
        Swal.fire({
            title: "Xác nhận đơn hàng?",
            icon: "question",
            showCancelButton: true,
            showConfirmButton: true,
            focusConfirm: false,
            cancelButtonText: "Thoát",
            confirmButtonText: "Xác nhận"
        }).then((result) => {
            showLoading();
            if (result.isConfirmed) {
                $.ajax({
                    type: 'PUT',
                    url: `/api/orders?orderId=${orderID}&status=confirmed`,
                    success: () => {
                        reloadOrders();
                        getOrderCount();
                    },
                    error: () => { }
                })
            }
        });
    }
})

$(document).on('click', '.order-status-refuse', function() {
    const orderID = $(this).data('order');
    if (orderID.length > 0) {
        Swal.fire({
            title: "Hủy đơn hàng?",
            icon: "question",
            showCancelButton: true,
            showConfirmButton: true,
            focusConfirm: false,
            cancelButtonText: "Thoát",
            confirmButtonText: "Xác nhận"
        }).then((result) => {
            showLoading();
            if (result.isConfirmed) {
                $.ajax({
                    type: 'PUT',
                    url: `/api/orders?orderId=${orderID}&status=cancelled`,
                    success: () => {
                        reloadOrders();
                        getOrderCount();
                    },
                    error: () => { }
                })
            }
        });
    }
})

//--Change order status to "Thanh toán thành công"
$(document).on('click', '.accept-paid', (e) => {
    $('.payment-acception-confirm').css('visibility', 'visible');
    $('.payment-acception').addClass('show');

    $('.cancel-acception').off('click').click(() => {
        $('.payment-acception-confirm').css('visibility', 'hidden');
        $('.payment-acception').removeClass('show');
    })

    $('.confirm-acception').off('click').click(() => {
        var orderID = $(e.target).data('accept-paid');
        if (orderID.length > 0) {
            console.log(orderID);
            $.ajax({
                type: 'post',
                url: '/admin/orders/acceptpaid',
                data: { orderID: orderID },
                success: (res) => {
                    if (res.success) {
                        $('.payment-acception-confirm').css('visibility', 'hidden');
                        $('.payment-acception').removeClass('show');
                    }
                }
            })
        }
    })
})

//--Get order detail ---------------------------------------
$('.close-order-info').click(() => {
    $('.order-infomation-wrapper').css('visibility', 'hidden');
    $('.order-infomation-box').removeClass('show');
})

$(document).on('click', '.order-detail-btn', (e) => {
    var orderID = $(e.target).data('detail-order');
    if (orderID.length > 0) {
        showLoading();
        $.ajax({
            tpe: 'get',
            url: '/api/orders',
            data: { id: orderID },
            success: (data) => {
                var date = new Date(data.OrderDate);
                var dateFormat = date.toLocaleDateString('en-GB') + ' ' + date.toLocaleTimeString('en-US');

                $('.order-info-header').text('Đơn hàng - ' + data.OrderID)
                $('.order-info-date').text(dateFormat);
                $('.order-info-payment').text(data.PaymentMethod);
                $('.order-info-ship').text(data.ShipMethod);
                $('.order-info-note').text(data.Note);
                $('.order-info-total').text(data.TotalPrice.toLocaleString('vi-VN') + 'đ');
                $('.order-info-ship-total').text(data.DeliveryFee.toLocaleString('vi-VN') + 'đ');
                $('.order-info-totalpay').text(data.TotalPaymentAmout.toLocaleString('vi-VN') + 'đ');
                $('.order-info-pstatus').text(data.PaymentStatus);
                $('.order-info-status').text(data.Status);

                //Get list product in order
                $.ajax({
                    type: 'get',
                    url: '/api/orders',
                    data: { orderID: data.OrderID },
                    success: (data1) => {
                        hideLoading();
                        $('.order-products-info table tbody').empty();
                        var strH = ` <tr>
                                    <th>Mã sản phẩm</th>
                                    <th>Tên sản phẩm</th>
                                    <th>Giá bán</th>
                                    <th>Số lượng</th>
                                    <th>Thành tiền</th>
                                </tr>`;
                        var str = ``;
                        if (data1.length > 0) {
                            for (var i = 0; i < data1.length; i++) {
                                str += `<tr>
                                            <td>${data1[i].Product.MaSP}</td>
                                            <td>${data1[i].Product.ProductName}</td>
                                            <td>${data1[i].Product.Price.toLocaleString('vi-VN') + 'đ'}</td>
                                             <td>${data1[i].Quantity}</td>
                                            <td class="fw-bold">${(data1[i].Product.Price * data1[i].Quantity).toLocaleString('vi-VN') + 'đ'}</td>
                                        </tr>`;
                            }

                            $('.order-info-cnt').text('Số sản phẩm - ' + data1.length);
                            $('.order-products-info table tbody').append(strH + str);
                            setTimeout(() => {
                                $('.order-infomation-wrapper').css('visibility', 'visible');
                                $('.order-infomation-box').addClass('show');
                            }, 500)
                        }
                    }
                })

                //Get customer info
                $.ajax({
                    type: 'get',
                    url: '/api/customers',
                    data: { customerID: data.CustomerID },
                    success: (data2) => {
                        $('.order-cus-id').text(data2.CustomerID);
                        $('.order-cus-name').text(data2.CustomerName);
                        $('.order-cus-phone').text(data2.Phone);
                        $('.order-cus-email').text(data2.Email);
                        $('.order-cus-address').text(data2.Address);
                    }
                })
            },
            error: () => { console.log('Error') }
        })
    }
})

//--Delete order
$(document).on('click', '.delete-order-btn', (e) => {
    $('.delete-order-confirm').css('visibility', 'visible');
    $('.delete-order-confirm .delete-confirm-box').addClass('show');
    //----------
    $('.cancel-delete').off('click').click(() => {
        $('.delete-order-confirm').css('visibility', 'hidden');
        $('.delete-order-confirm .delete-confirm-box').removeClass('show');
    })

    $('.delete-order-confirm .confirm-delete-order').off('click').click(() => {
        var orderID = $(e.target).data('del-order');
        if (orderID.length > 0) {
            $.ajax({
                type: 'post',
                url: '/admin/orders/deleteorder',
                data: {
                    orderID: orderID
                },
                success: (response) => {
                    if (response.success) {
                        $('.complete-delete-notice').css('visibility', 'visible');
                        $('.complete-notice-box').addClass('showForm');
                        $('.delete-order-confirm').css('visibility', 'hidden');
                        $('.delete-order-confirm .delete-confirm-box').removeClass('show');
                    }
                },
                error: () => { console.log('Không thể xóa đơn hàng') }
            })
        }
    })
})


//--Create order -------------------------------------------------------
$('.add-order-btn').click(() => {
    window.location.href = '/admin/orders/create';
})

$('.close-create-order').click(() => {
    $('.create-order').css('visibility', 'hidden');
    $('.create-order-box').removeClass('show');
})

//---Search customer by phone
$('#search-cus-by-phone').keyup((e) => {
    var phone = $(e.target).val();
    if (phone.length > 0) {
        $.ajax({
            type: 'GET',
            url: '/api/customers',
            data: { phone: phone },
            success: (data) => {
                if (data.length > 0) {
                    $('.cus-search-auto-complete').empty();
                    $('.cus-search-auto-complete').show();
                    for (let i = 0; i < data.length; i++) {
                        const str = `<div class="cus-search-item">
                            <input type="radio" id="cus-search-cbx-${i + 1}" name="cus-search-cbx" class="d-none" value="${data[i].MaKH}" />
                            <label for="cus-search-cbx-${i + 1}" class="d-flex gap-3 align-items-center">
                                <span class="cus-search-phone">${data[i].HoTen}</span>
                                <span class="cus-search-name">${data[i].SDT}</span>
                            </label>
                        </div>`;

                        $('.cus-search-auto-complete').append(str);
                    }
                }
                else {
                    $('.cus-search-auto-complete').empty();
                    $('.cus-search-auto-complete').hide();
                }
            },
            error: () => {
                $('.cus-search-auto-complete').empty();
                $('.cus-search-auto-complete').hide();
            }
        })
    }
    else {
        $('.cus-search-auto-complete').empty();
        $('.cus-search-auto-complete').hide();
    }
})

$(document).on('change', 'input[name="cus-search-cbx"]', (e) => {
    if ($(e.target).prop('checked') == true) {
        $('#search-cus-by-phone').val('')
        $('.cus-search-auto-complete').empty();
        $('.cus-search-auto-complete').hide();
        showLoading();
        $.ajax({
            type: 'GET',
            url: '/api/customers',
            data: {
                id: $(e.target).val()
            },
            success: (data) => {
                hideLoading();
                $('#cusName').val(data.HoTen);
                $('#cusPhone').val(data.SDT);
                $('#cusAddress').val(data.DiaChi);
                $('#cusEmail').val(data.Email);
                data.GioiTinh === "Nam" ? $('#cusGender-Male').prop('checked', true) :
                    $('#cusGender-FeMale').prop('checked', true);
            },
            error: () => { }
        })
    }
})

//---Search product by name
function updateTotal() {
    var total = 0;
    $('.one-p-total').each((index, item) => {
        var price = $(item).text().replace('đ', '').replace(/\./g, '').trim();
        total += parseInt(price);
    })

    $('.order-totalprice span').text(total.toLocaleString('vi-VN') + 'đ');
}

let typingTimeOut;
$('#order-search-p').keyup((e) => {
    clearTimeout(typingTimeOut);
    typingTimeOut = setTimeout(() => {
        var productName = $(e.target).val();
        if (productName.length > 0) {
            $.ajax({
                type: 'get',
                url: '/api/products',
                data: { q: productName },
                success: (data) => {
                    if (data.length > 0) {
                        $('.pro-search-auto-complete').empty();
                        $('.pro-search-auto-complete').show();

                        for (let i = 0; i < data.length; i++) {
                            var str = ` <div class="pro-search-item d-flex align-items-center gap-2">
                            <input type="radio" class="d-none" name="pro-search-id" id="pro-search-id-${i + 1}" value="${data[i].MaSP}" />
                            <label for="pro-search-id-${i + 1}" class=" d-flex align-items-center justify-content-between gap-3">
                                <img src="${data[i].HinhAnh != null ? data[i].HinhAnh : '/Assets/Images/no-image.jpg'}" alt="" />
                                <span class="m-0 p-0 pro-search-name">${data[i].TenSP}</span>
                                <span class="pro-search-price">${data[i].GiaBan.toLocaleString('vi-VN') + 'đ'}</span>
                            </label>
                        </div>`;

                            $('.pro-search-auto-complete').append(str);
                        }
                    }
                    else {
                        $('.pro-search-auto-complete').empty();
                        $('.pro-search-auto-complete').hide();
                    }
                },
                error: () => {
                    $('.pro-search-auto-complete').empty();
                    $('.pro-search-auto-complete').hide();
                }
            })
        }
        else {
            $('.pro-search-auto-complete').empty();
            $('.pro-search-auto-complete').hide();
        }
    }, 500)
})

//--------------------
$(document).on('click', (e) => {
    const cusSearch = $('.cus-search-auto-complete');
    if (!$(e.target).closest('.cus-search-auto-complete').length) {
        cusSearch.hide();
        cusSearch.empty();
    }

    const proSearch = $('.pro-search-auto-complete');
    if (!$(e.target).closest('.pro-search-auto-complete').length) {
        proSearch.hide();
        proSearch.empty();
    }
})


//Add product to create order table
$(document).on('change', '.create-order input[name="pro-search-id"]', (e) => {
    if ($(e.target).prop('checked') == true) {
        var proID = $(e.target).val();
        $('.pro-search-auto-complete').empty();
        $('.pro-search-auto-complete').hide();
        $('#order-search-p').val('');
        if (proID.length > 0) {
            showLoading();
            $.ajax({
                type: 'GET',
                url: '/api/products',
                data: {
                    id: proID
                },
                success: (data) => {
                    hideLoading();
                    let currentPro = $('input[name="order-pro-qty"]').toArray();
                    let exist = currentPro.some(function (el) {
                        return $(el).data('order-pro') === data.MaSP;
                    });

                    if (exist === false) {
                        if (data.MaSP != null) {
                            let str = `<tr>
                                        <td>
                                            <input type="hidden" name="order-pro-id" value="${data.MaSP}" />
                                            ${data.MaSP}
                                        </td>
                                        <td>${data.TenSP}</td>
                                        <td>${data.GiaBan.toLocaleString('vi-VN')}đ</td>
                                        <td>
                                            <input type="number" name="order-pro-qty" value="1" min="1" data-order-pro="${data.MaSP}" required/>
                                        </td>
                                        <td class="one-p-total">
                                            ${data.GiaBan.toLocaleString('vi-VN')}đ
                                        </td>
                                        <td>
                                            <i class='bx bx-trash del-order-pro'></i>
                                        </td>
                                    </tr>`;

                            $('.order-create-products table tbody').append(str);
                            updateTotal();
                        }
                    }
                },
                error: () => { }
            })
        }
    }
})

//Update quantity of product in order detail (create order) -------
$(document).on('focus', 'input[name="order-pro-qty"]', (e) => {
    var currentQty = $(e.target).val();

    $(e.target).blur(() => {
        var qty = $(e.target).val();
        if (qty != currentQty) {
            $.ajax({
                type: 'post',
                url: '/admin/orders/updateproductqty',
                data: {
                    MaSP: $(e.target).data('order-pro'),
                    qty: qty
                },
                success: (data) => {
                    if (data.success) {
                        $(e.target).val(data.quantity);
                        $(e.target).closest('tr').find('.one-p-total').text(data.total.toLocaleString('vi-VN') + 'đ');
                        updateTotal();
                    }
                },
                error: () => { }
            })
        }
    })
})

//Delete product in create order form
$(document).on('click', '.del-order-pro', (e) => {
    $(e.target).closest('tr').remove();
    updateTotal();
})


//-----------------------------------------------------------
const getCustomer = (sdt) => { 
    $.ajax({
        type: 'GET',
        url: '/api/customers',
        data: { sdt: sdt },
        success: (data) => {
            $('#cusName').text(data.HoTen);
            $('#cusPhone').text(data.SDT);
            $('#cusEmail').text(data.Email);
            $('#cusAddress').text(data.DiaChi);
            if (data.Gender == "Nam") {
                $('#cusGender-Male').prop('checked', true);
            } else {
                $('#cusGender-FeMale').prop('checked', true);
            }
        }
    })
}

$('.create-cus-btn').click(() => {
    $('.create-customer-wrapper').addClass('show');
    $('.create-customer-wrapper .form-box').addClass('show');
})

$('.close-create-customer').click(() => {
    $('.create-customer-wrapper').removeClass('show');
    $('.create-customer-wrapper .form-box').removeClass('show');
})

$('.create-customer-wrapper .form-box').on('reset', function (e) {
    $(this).removeClass('show');
    $(this).closest('.create-customer-wrapper').removeClass('show')
})

$('.create-customer-wrapper .form-box').submit(function (e) {
    e.preventDefault();
    let name = $(this).find('#HoTen').val();
    let phone = $(this).find('#SDT').val();
    let address = $(this).find('#DiaChi').val();
    let gender = $(this).find('input[name="Gender"]:checked').val();
    let dob = $(this).find('#NgaySinh').val();
    let email = $(this).find('#Email').val();
    showLoading();
    $.ajax({
        type: 'POST',
        url: '/api/customers',
        data: {
            'HoTen': name,
            'SDT': phone,
            'DiaChi': address,
            'GioiTinh': gender,
            'NgaySinh': dob,
            'Email': email
        },
        success: (response) => {
            if (response) {
                hideLoading();
                getCustomer(phone);
                $(this).removeClass('show');
                $(this).closest('.create-customer-wrapper').removeClass('show')
            }
        },
        error: () => {
            
        }
    })

    $(e.target).find('.form-submit-reset').click(() => {
        $(e.target).find('.form-error').hide();
        $(e.target).find('.form-error').empty();
    })
})

//Create order ---------------------------
$('.create-order-box').submit((e) => {
    e.preventDefault();
    var cusID = $('.create-order #cusID').val();
    var cusName = $('.create-order #cusName').val();
    var cusPhone = $('.create-order #cusPhone').val();
    var cusAddress = $('.create-order #cusAddress').val();
    var cusGender = $('.create-order input[name="cusGender"]:checked').val();
    var cusEmail = $('.create-order #cusEmail').val();
    var payment = $('input[name="paymentmethod"]:checked').val();
    var note = $('#order-note').val();
    var strProduct = '';

    $('.order-create-products table tr:not(:first-child)').each((index, row) => {
        const MaSP = $(row).find('input[name="order-pro-id"]').val();
        const orderQty = $(row).find('input[name="order-pro-qty"]').val();

        strProduct += MaSP + '++++++++' + orderQty + ';;;;;;;;';
    })

    if (strProduct.length > 0) {
        $.ajax({
            type: 'post',
            url: '/admin/orders/create',
            data: {
                'CustomerID': cusID,
                'CustomerName': cusName,
                'Phone': cusPhone,
                'Address': cusAddress,
                'Gender': cusGender,
                'Email': cusEmail,
                paymentMethod: payment,
                productStr: strProduct,
                note: note
            },
            success: (res) => {
                if (res) {
                    window.location.href = '';
                }
            },
            error: () => { }
        })
    }
})