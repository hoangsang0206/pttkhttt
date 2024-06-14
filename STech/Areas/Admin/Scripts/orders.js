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
                    <th>Ngày đặt</th><th>Tổng tiền</th><th>PT t.toán</th><th>T.thái thanh toán</th>
                    <th>Trạng thái</th><th></th></tr>`;
    parent_element.append(strHead);
    if (res.length > 0) {
        res.map(order => {
            const date = new Date(order.NgayDat);
            const dateFormat = date.toLocaleDateString('en-GB') + ' ' + date.toLocaleTimeString('en-US');
            const buttonAccept = "&nbsp;<button class='mini-btn green accept-paid' data-order=" + order.MaHD + "><i class='bx bx-check'></i></button>";
            let isRender = order.TrangThai !== "cancelled" && order.TrangThaiThanhToan !== "paid";

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
                <td><div>${order.KhachHang.HoTen}</div></td>
                <td><div>${dateFormat}</div></td>
                <td><div class="total-payment">${order.TongTien.toLocaleString('vi-VN') + 'đ'}</div></td>
                <td><div>${order.PhuongThucThanhToan == "COD" ? "TT khi nhận hàng" : order.PhuongThucThanhToan}</div></td>
                <td><div class="d-flex align-items-center ${paymentStatusClass}">${paymentText} ${isRender ? buttonAccept : ""}</td>`;
            if (order.TrangThai == "unconfirmed") {
                str += `<td>
                    <div class="order-status">
                        <button class="mini-btn green order-status-accept" data-order="${order.MaHD}">Xác nhận</button>
                        <button class="mini-btn red order-status-refuse" data-order="${order.MaHD}">Hủy ĐH</button>
                    </div>
                </td>`;
            }
            else {
                str += `<td><div class="order-status ${statusClass}">${statusText}</div></td>`;
            }
            str += `<td>
                <div class="order-button-box d-flex justify-content-end flex-wrap gap-2">
                    <button class="mini-btn green order-print-btn" data-order="${order.MaHD}">In HĐ</button>
                    <button class="mini-btn blue order-detail-btn" data-order="${order.MaHD}">Chi tiết</button>
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
            appendOrderList(res, $('.order-list table tbody'));
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
            appendOrderList(res, $('.order-list table tbody'));
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
            appendOrderList(res, $('.order-list table tbody'));
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
            appendOrderList(res, $('.order-list table tbody'));
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
            appendOrderList(res, $('.order-list table tbody'));
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
            appendOrderList(res, $('.order-list table tbody'));
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
            appendOrderList(res, $('.order-list table tbody'));
        },
        error: () => { }
    })
})


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
            if (result.isConfirmed) {
                showLoading();
                $.ajax({
                    type: 'PUT',
                    url: `/api/orders?orderId=${orderID}&status=confirmed`,
                    success: (response) => {
                        if (response) {
                            if ($(this).closest('.order-infomation-wrapper').length > 0) {
                                $(this).removeAttr('data-order').hide();
                                $(this).siblings('.order-status-refuse').removeAttr('data-order').hide();
                            } else {
                                $(this).closest('.order-status').addClass('order-success').html('Đã xác nhận');
                            }
                            getOrderCount();
                            hideLoading();
                        } else {
                            setTimeout(() => {
                                Swal.fire({
                                    title: "Xác nhận đơn hàng thất bại?",
                                    icon: "errror",
                                    text: "Đã xảy ra lỗi trong quá trình xác nhận đơn hàng",
                                    confirmButtonText: "OK"
                                })
                            }, 600)
                        }
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
            if (result.isConfirmed) {
                showLoading();
                $.ajax({
                    type: 'PUT',
                    url: `/api/orders?orderId=${orderID}&status=cancelled`,
                    success: (response) => {
                        if (response) {
                            if ($(this).closest('.order-infomation-wrapper').length > 0) {
                                $(this).removeAttr('data-order').hide();
                                $(this).siblings('.order-status-accept').removeAttr('data-order').hide();
                            } else {
                                $(this).closest('.order-status').addClass('order-failed').html('Đã hủy');
                            }

                            hideLoading();
                            getOrderCount();
                        } else {
                            setTimeout(() => {
                                Swal.fire({
                                    title: "Hủy đơn hàng thất bại?",
                                    icon: "errror",
                                    text: "Đã xảy ra lỗi trong quá trình hủy đơn hàng",
                                    confirmButtonText: "OK"
                                })
                            }, 600)
                        }
                    },
                    error: () => { }
                })
            }
        });
    }
})


$(document).on('click', '.accept-paid', function () {
    const orderID = $(this).data('order');
    if (orderID) {
        Swal.fire({
            title: "Xác nhận đã thanh toán?",
            icon: "question",
            text: 'Xác nhận đã thanh toán cho đơn hàng này',
            showCancelButton: true,
            showConfirmButton: true,
            focusConfirm: false,
            cancelButtonText: "Thoát",
            confirmButtonText: "Xác nhận"
        }).then((result) => {
            if (result.isConfirmed) {
                showLoading();
                $.ajax({
                    type: 'PUT',
                    url: `/api/orders?orderId=${orderID}&pstatus=paid`,
                    success: (res) => {
                        if (res) {
                            if ($(this).closest('.order-infomation-wrapper').length > 0) {
                                $(this).removeAttr('data-order').hide();
                            } else {
                                $(this).parent().addClass('order-success').html('Đã thanh toán')
                                    .removeClass('order-waiting').removeClass('order-failed');
                            }
                            hideLoading();
                        } else {
                            setTimeout(() => {
                                Swal.fire({
                                    title: "Xác nhận thanh toán thất bại?",
                                    icon: "error",
                                    text: "Đã xảy ra lỗi",
                                    confirmButtonText: "OK"
                                })
                            }, 600)
                        }
                    },
                    error: () => {

                    }
                })
            }
        })
    }
})

//--Get order detail ---------------------------------------
$('.close-order-info').click(() => {
    $('.order-infomation-wrapper').removeClass('show');
})

$(document).on('click', '.order-detail-btn', function() {
    const orderID = $(this).data('order');
    if (orderID) {
        showLoading();
        $.ajax({
            tpe: 'GET',
            url: '/api/orders',
            data: { orderId: orderID },
            success: (data) => {
                const date = new Date(data.NgayDat);
                const dateFormat = date.toLocaleDateString('en-GB') + ' ' + date.toLocaleTimeString('en-US');

                $('.order-info-header').text('Đơn hàng - ' + data.MaHD)
                $('.order-info-date').text(dateFormat);
                $('.order-info-payment').text(data.PhuongThucThanhToan);
                $('.order-info-ship').text(data.DiaChiGiao != "COD" ? data.DiaChiGiao : "Nhận tại cửa hàng");
                $('.order-info-note').text(data.GhiChu);
                $('.order-info-totalpay').text(data.TongTien.toLocaleString('vi-VN') + 'đ');
                $('.order-info-pstatus').text(data.TrangThaiThanhToan == "paid"
                    ? "Đã thanh toán" : data.TrangThaiThanhToan == "unpaid" ? "Chưa thanh toán" : "Thanh toán thất bại");
                $('.order-info-status').text(data.Status == "confirmed"
                    ? "Đã xác nhận" : data.Status == "unconfirmed" ? "Chờ xác nhận" : "Đã hủy");

                const strH = ` <tr>
                            <th>Mã sản phẩm</th>
                            <th>Tên sản phẩm</th>
                            <th>Giá bán</th>
                            <th>Số lượng</th>
                            <th>Thành tiền</th>
                        </tr>`;
                $('.order-products-info table tbody').html(strH);

                if (data.ChiTietHD) {
                    data.ChiTietHD.map(item => {
                        const str = `<tr>
                                    <td>${item.MaSP}</td>
                                    <td>${item.SanPham.TenSP}</td>
                                    <td>${item.ThanhTien.toLocaleString('vi-VN') + 'đ'}</td>
                                    <td>${item.SoLuong}</td>
                                    <td class="fw-bold">${(item.ThanhTien * item.SoLuong).toLocaleString('vi-VN') + 'đ'}</td>
                                </tr>`;
                        $('.order-products-info table tbody').append(str);
                    })

                    $('.order-info-cnt').text('Số sản phẩm - ' + data.ChiTietHD.length);
                }

                $('.order-cus-id').text(data.KhachHang.MaKH);
                $('.order-cus-name').text(data.KhachHang.HoTen);
                $('.order-cus-phone').text(data.KhachHang.SDT);
                $('.order-cus-email').text(data.KhachHang.Email);
                $('.order-cus-address').text(data.KhachHang.DiaChi);

                const parent = $('.order-infomation-wrapper');

                if (data.TrangThai == 'unconfirmed') {
                    parent.find('.order-status-accept').show().data('order', data.MaHD);
                    parent.find('.order-status-refuse').show().data('order', data.MaHD);
                } else {
                    parent.find('.order-status-accept').hide().removeAttr('data-order');
                    parent.find('.order-status-refuse').hide().removeAttr('data-order');
                }

                if (data.TrangThaiThanhToan != 'paid' && data.TrangThai != 'cancelled') {
                    parent.find('.accept-paid').show().data('order', data.MaHD);
                } else {
                    parent.find('.accept-paid').hide().removeAttr('data-order');
                }

                hideLoading();
                setTimeout(() => {
                    $('.order-infomation-wrapper').addClass('show');
                }, 600)
            },
            error: () => { console.log('Error') }
        })
    }
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
                $('#cusID').val(data.MaKH);
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
                data: { query: productName },
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
    const currentQty = $(e.target).val();

    $(e.target).blur(() => {
        const qty = $(e.target).val();
        if (qty != currentQty) {
            const productId = $(e.target).data('order-pro');
            $.ajax({
                type: 'PUT',
                url: `/employee/orders/updateproductqty?productID=${productId}&qty=${qty}`,
                success: (data) => {
                    if (data.status === 200) {
                        $(e.target).val(data.quantity);
                        $(e.target).closest('tr').find('.one-p-total').text(data.total.toLocaleString('vi-VN') + 'đ');
                        updateTotal();
                    } else {

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
            $('#cusID').val(data.MaKH);
            $('#cusName').val(data.HoTen);
            $('#cusPhone').val(data.SDT);
            $('#cusEmail').val(data.Email);
            $('#cusAddress').val(data.DiaChi);
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
})

$('.close-create-customer').click(() => {
    $('.create-customer-wrapper').removeClass('show');
})

$('.create-customer-wrapper .form-box').on('reset', function (e) {
    $('.create-customer-wrapper').removeClass('show');
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
            hideLoading();
            if (response) {
                getCustomer(phone);
                $(this).removeClass('show');
                $(this).closest('.create-customer-wrapper').removeClass('show')
            } else {
                setTimeout(() => {
                    Swal.fire({
                        title: "Thêm khách hàng thất bại",
                        icon: "error",
                    });
                }, 600)
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
    const cusID = $('.create-order #cusID').val();
    const cusName = $('.create-order #cusName').val();
    const cusPhone = $('.create-order #cusPhone').val();
    const cusAddress = $('.create-order #cusAddress').val();
    const cusGender = $('.create-order input[name="cusGender"]:checked').val();
    const cusEmail = $('.create-order #cusEmail').val();
    const payment = $('input[name="paymentmethod"]:checked').val();
    const note = $('#order-note').val();

    let lstProduct = [];


    $('.order-create-products table tr:not(:first-child)').each((index, row) => {
        const MaSP = $(row).find('input[name="order-pro-id"]').val();
        const orderQty = $(row).find('input[name="order-pro-qty"]').val();

        lstProduct.push({ MaSP: MaSP, SoLuong: orderQty})
    })

    const order = {
        KhachHang: {
            MaKH: cusID,
            HoTen: cusName,
            SDT: cusPhone,
            DiaChi: cusAddress,
            GioiTinh: cusGender,
            Email: cusEmail,
        },
        PaymentMed: payment,
        Note: note,
        ChiTietHD: lstProduct
    }

    console.log(order)

    if (lstProduct) {
        showLoading();
        $.ajax({
            type: 'POST',
            url: '/api/orders',
            contentType: 'application/json',
            data: JSON.stringify(order),
            success: (res) => {
                hideLoading();
                setTimeout(() => {
                    if (res) {
                        Swal.fire({
                            icon: "success",
                            title: "Tạo đơn hàng thành công",
                            confirmButtonText: 'OK'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                window.location.href = '';
                            }
                        })
                    } else {
                        Swal.fire({
                            icon: "error",
                            title: "Không thể tạo đơn hàng",
                            text: "Đã xảy ra lỗi!"
                        })
                    }
                }, 600);
            },
            error: () => {
                setTimeout(() => {
                    Swal.fire({
                        icon: "error",
                        title: "Không thể tạo đơn hàng",
                        text: "Đã xảy ra lỗi!"
                    })
                }, 600)
            }
        })
    }
})