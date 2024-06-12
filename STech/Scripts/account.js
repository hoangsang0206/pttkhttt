const showBtnLoading = (button) => {
    var btnText = button.text();
    var loadingStr = `<div class="loadingio-spinner-dual-ring-ekj0ol56kwc">
                        <div class="ldio-gmrbyawnrc">
                            <div></div><div><div></div></div>
                        </div>
                    </div>`;
    button.html(loadingStr);

    return btnText;
}

const resetBtn = (button, btnText) => {
    const timeout = setTimeout(() => {
        button.html(btnText);
        clearTimeout(timeout);
    }, 1000);
}

const showFormError = (form, message) => {
    $(form).find('.form-error').show();
    $(form).find('.form-error').html(message);
}

const closeFormError = (form) => {
    $(form).find('.form-error').hide();
    $(form).find('.form-error').empty();
}

const closeFormErrorWithTimeout = (form) => {
    const timeout = setTimeout(() => {
        $(form).find('.form-error').hide();
        $(form).find('.form-error').empty();
        clearTimeout(timeout);
    }, 5000)
}

$('.register form').submit(function(e) {
    e.preventDefault();
    const userName = $(this).find('#RegUsername').val();
    const password = $(this).find('#RegPassword').val();
    const confirmPassword = $(this).find('#ConfirmPassword').val();
    const email = $(this).find('#RegEmail').val();

    const submitBtn = $(e.target).find('.form-submit-btn');
    const btnText = showBtnLoading(submitBtn);
    $.ajax({
        type: 'POST',
        url: '/account/register',
        data: {
            RegUsername: userName,
            RegPassword: password,
            ConfirmPassword: confirmPassword,
            RegEmail: email
        },
        success: (response) => {
            if (response.status === 400) {
                const str = `<span>
                <i class="fa-solid fa-circle-exclamation"></i>`
                    + response.message + `</span>`;
                showFormError(this, str);
                closeFormErrorWithTimeout(this);
                resetBtn(submitBtn, btnText);
            } else {
                closeFormError(this);
                location.reload();
            }
        },
        error: (jqXHR) => { }
    })
})


$('.login form').submit(function(e) {
    e.preventDefault();
    var userName = $(this).find('#Username').val();
    var password = $(this).find('#Password').val();

    var submitBtn = $(this).find('.form-submit-btn');
    var btnText = showBtnLoading(submitBtn);

    $.ajax({
        type: 'POST',
        url: '/account/login',
        data: {
            Username: userName,
            Password: password
        },
        success: (response) => {
            if (response.status === 200) {
                closeFormError(this);
                if (response.redirectUrl) {
                    window.location.href = response.redirectUrl;
                } else {
                    location.reload();
                }

            } else if(response.status === 400) {
                const str = `<span>
                <i class="fa-solid fa-circle-exclamation"></i>`
                    + response.message + `</span>`;
                showFormError(this, str);
                closeFormErrorWithTimeout(this);
                resetBtn(submitBtn, btnText);
            }
        },
        error: (jqXHR) => { }
    })
})


const showUpdateMessage = (message) => {
    const element = $('.account-right-box.current').find('.update-error');
    element.empty();
    element.html(message);
    const timeout = setTimeout(() => {
        element.empty();
        clearTimeout(timeout);
    }, 5000);
}

const closeUpdateMessage = () => {
    const parent = $('.account-right-box.current');
    parent.find('.update-error').empty();
}

$('.user-update-form').submit(function(e) {
    e.preventDefault();
    const fullName = $(this).find('#Item2_UserFullName').val();
    const gender = $(this).find('input[name="Gender"]:checked').val();
    const phone = $(this).find('#Item2_PhoneNumber').val();
    const email = $(this).find('#Item2_Email').val();
    const dob = $(this).find('#Item2_DOB').val();
    const address = $(this).find('#Item2_Address').val();

    const submitBtn = $(this).find('.user-form-submit');
    const btnText = showBtnLoading(submitBtn);
    $.ajax({
        type: 'PUT',
        url: '/account/update',
        data: {
            UserFullName: fullName,
            Gender: gender,
            PhoneNumber: phone,
            Email: email,
            DOB: dob,
            Address: address
        },
        success: (response) => {
            resetBtn(submitBtn, btnText);
            if (response.status === 200) {
                const str = '<span style="color: #44bd32">Cập nhật thành công.</span>';
                showUpdateMessage(str);
            }
            else if(response.status === 400) {
                $('.update-error').empty();
                const str = '<span style="color: #e30019">' + response.message + '</span>'
                showUpdateMessage(str)
            }
        },
        error: (err) => { resetBtn(submitBtn, btnText); }
    })
})


$('.change-password-form').submit(function(e) {
    e.preventDefault();
    const oldPassword = $('#OldPassword').val();
    const newPassword = $('#NewPassword').val();
    const confirmNewPassword = $('#ConfirmNewPassword').val();

    const submitBtn = $(this).find('.user-form-submit');
    const btnText = showBtnLoading(submitBtn);
    $.ajax({
        type: 'POST',
        url: '/account/changepassword',
        data: {
            OldPassword: oldPassword,
            NewPassword: newPassword,
            ConfirmNewPassword: confirmNewPassword
        },
        success: (response) => {
            resetBtn(submitBtn, btnText);
            if (response.status === 200) {
                const str = '<span style="color: #44bd32">Đổi mật khẩu thành công.</span>';
                showUpdateMessage(str);
                
                $('#OldPassword').val('');
                $('#NewPassword').val('');
                $('#ConfirmNewPassword').val('');
            }
            else if (response.status === 400) {
                $('.update-error').empty();
                const str = `<span style="color: #e30019">${response.message}</span>`;

                showUpdateMessage(str)
            }

        },
        error: () => { resetBtn(submitBtn, btnText); }
    })
})

//--
$('.login-info-logout, .account-logout').click(() => {
    Swal.fire({
        title: "Đăng xuất?",
        text: "Bạn chắc chắn muốn đăng xuất?",
        icon: "question",
        showCancelButton: true,
        showConfirmButton: true,
        focusConfirm: false,
        cancelButtonText: "Hủy",
        confirmButtonText: "Đăng xuất",
    }).then((result) => {
        if (result.isConfirmed) {
            window.location.href = '/account/logout'
        }
    });
})


$('.upload-img-btn').click(() => {
    $('.upload-user-image').addClass('show');
    $('.upload-form-box').addClass('show');

})

$('.close-upload-frm').click(() => {
    $('.upload-user-image').removeClass('show');
    $('.upload-form-box').removeClass('show');

    $('.upload-user-img').empty();
    $('.upload-user-img').removeClass('img-uploaded');
    $('.upload-user-img').append(`<i class="fa-solid fa-cloud-arrow-up"></i><span>Tải tên hoặc kéo thả hình ảnh vào đây</span>`);
    $('.upload-frm-notice').empty();
    $('.upload-frm-notice').removeClass('success')
    $('.upload-frm-notice').removeClass('failed')
    $('.upload-frm-notice').css('display', 'none !important');
})

function updateUploadFormNotice(string, status) {
    $('.upload-frm-notice').empty();
    $('.upload-frm-notice').show();
  
    if (status) {
        $('.upload-frm-notice').addClass('success')
        $('.upload-frm-notice').removeClass('failed')
    }
    else {
        $('.upload-frm-notice').addClass('failed')
        $('.upload-frm-notice').removeClass('success')
    }

    $('.upload-frm-notice').append(string);
}


$(document).ready(() => {
    function uploadUserImage(file) {
        if (file) {
            let formData = new FormData();
            formData.append('file', file);

            $('.upload-progress-bar').css('display', 'flex');
            $('.upload-progress-bar').empty();
            $.ajax({
                type: 'POST',
                url: '/account/uploadimage',
                data: formData,
                processData: false,
                contentType: false,
                xhr: () => {
                    var xhr = $.ajaxSettings.xhr();
                    xhr.upload.onprogress = (event) => { //Upload progress
                        var percentComplete = Math.floor((event.loaded / event.total) * 100);
                        var progressHtml = `<i class='bx bxs-file-image'></i>
                                <div class="progress-bar d-flex flex-row align-items-center justify-content-start">
                                    <div class="bar" style="width: ${percentComplete + '%'}"></div>
                                </div>
                                <div class="progress-percent">${percentComplete}%</div>`;
                        $('.upload-progress-bar').empty();
                        $('.upload-progress-bar').append(progressHtml);
                    }
                    return xhr;
                },
                success: (res) => {
                    $('.upload-progress-bar').empty();
                    $('.upload-progress-bar').hide();

                    if (res.success) {
                        var str = `<i class='bx bxs-check-circle'></i>
                            <span>Tải lên thành công.</span>`;
                        var strImg = `<img src="${res.src}" alt="" />`
                        $('.upload-user-img').empty();
                        $('.upload-user-img').addClass('img-uploaded');
                        $('.upload-user-img').append(strImg);

                        updateUploadFormNotice(str, res.success);
                        $('.user-img').attr('src', res.src);
                    }
                    else {
                        var str = `<i class='bx bxs-x-circle'></i>
                            <span>${res.error}</span>`;
                        var strElement = ` <i class="fa-solid fa-cloud-arrow-up"></i><span>Tải hình ảnh lên</span>`
                        $('.upload-user-img').empty();
                        $('.upload-user-img').removeClass('img-uploaded');
                        $('.upload-user-img').append(strElement);
                        updateUploadFormNotice(str, res.success);
                    }
                },
                error: (err) => {
                    var str = `<i class='bx bxs-x-circle'></i>
                            <span>Tải lên thất bại.</span>`;
                    updateUploadFormNotice(str, false);
                    var strElement = ` <i class="fa-solid fa-cloud-arrow-up"></i><span>Tải hình ảnh lên</span>`
                    $('.upload-user-img').empty();
                    $('.upload-user-img').removeClass('img-uploaded');
                    $('.upload-user-img').append(strElement);
                    console.log(err);
                }
            })
        }
    }

    $('.upload-user-img').on('dragover dragenter', () => {
        $('.upload-user-img').addClass('dragenter');
    })

    $('.upload-user-img').on('dragleave dragend drop', () => {
        $('.upload-user-img').removeClass('dragenter');
    })

    $('.upload-user-img').click(() => {
        $('.file-input').click();
    })

    $('.file-input').on('change', ({target}) => {
        let file = target.files[0];
        uploadUserImage(file);
    });

    $('.upload-user-img').on('dragover', (e) => {
        e.preventDefault();
    })

    $('.upload-user-img').on('drop', (e) => {
        e.preventDefault();
        var files = e.originalEvent.dataTransfer.files;
        uploadUserImage(files[0]);
    })
});


function setParentHeight() {
    const childHeight = $('.account-right-box.current').outerHeight(true);
    $('.account-right-side').css('height', childHeight + 'px');

    const leftSideHeight = $('.account-left-side').outerHeight(true);
    if (leftSideHeight > childHeight) {
        $('.account-right-box.current').css('height', leftSideHeight + 'px');
        $('.account-right-side').css('height', leftSideHeight + 'px');
    }
}

function showCard() {
    const idFromUrl = window.location.hash.substring(1);
    if (idFromUrl.length > 0) {
        $('.account-right-box').removeClass('current');
        $('.account-nav-list-item a').removeClass('activeNav');
        $('[data-account-side="' + idFromUrl + '"').addClass('current');
        $('a[href="#' + idFromUrl +'"').addClass('activeNav');
    } 
    setParentHeight();
}

showCard();

$(window).on('hashchange' ,() => {
    showCard();
})


$(document).ready(() => {
    const element = document.querySelector('.account-right-box.current')
    if (element) {
        const resizeObserver = new ResizeObserver((entries) => {
            entries.map(entry => {
                setParentHeight();
            })
        })

        resizeObserver.observe(element);
    }
})


// Format datetime ASP.NET to "dd/MM/yyyy"
const formatDate = (inputDate) => {
    const date = new Date(inputDate);
    if (date != null) {
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return day + '/' + month + '/' + year;
    }
    return inputDate;
}


$('.order-search-form').submit(function(e) {
    e.preventDefault();
    const orderID = $(this).find('#order-search').val();

    showWebLoader();
    $.ajax({
        type: 'GET',
        url: '/api/orders',
        data: {
            searchId: orderID
        },
        success: (data) => {
            hideWebLoader();
            $('.account-right-box.current').css('height', 'auto');
            if (data != null && data.length > 0) {
                let headerStr = ` <tr>
                        <th>Mã ĐH</th>
                        <th>Ngày đặt</th>
                        <th>Tổng tiền</th>
                        <th>TT thanh toán</th>
                        <th>Trạng thái</th>
                        <th></th>
                        </tr>`;

                $('.order-list table tbody').empty();
                $('.order-list table tbody').append(headerStr)

                data.map(order => {
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
                    } else if (order.TrangThai === "confirmed") {
                        statusClass = "order-success";
                        statusText = "Đã xác nhận";
                    }

                    let str = `<tr>
                    <td class="order-id">${order.MaHD}</td>
                    <td class="order-date">${formatDate(order.NgayDat)}</td>
                    <td class="order-total">${order.TongTien.toLocaleString("vi-VN")}đ</td>
                    <td>
                        <div class="order-status ${paymentStatusClass}">${paymentText}</div>
                    </td>
                     <td>
                        <div class="order-status ${statusClass}">${statusText}</div>
                    </td>
                    <td> <a href="/order/detail/${order.MaHD}">Chi tiết</a></td>
                    </tr>`;

                    $('.order-list table tbody').append(str);

                })
            } else {
                $('.order-list table tbody').html(`<div class="my-5 text-center">Không tìm thấy đơn hàng nào</div>`);
            }

            setParentHeight();
        },
        error: () => { hideWebLoader(); }
    })
})


$('.order-header-list li').click(function() {
    $('.order-header-list li').removeClass('active');
    $(this).addClass('active');

    const value = $(this).data('get-order');
    if (value.length > 0) {
        showWebLoader();
        $.ajax({
            type: 'GET',
            url: '/api/orders',
            data: {
                type: value
            },
            success: (data) => {
                hideWebLoader();
                $('.account-right-box.current').css('height', 'auto');
                if (data != null && data.length > 0) {
                    let headerStr = ` <tr>
                        <th>Mã ĐH</th>
                        <th>Ngày đặt</th>
                        <th>Tổng tiền</th>
                        <th>TT thanh toán</th>
                        <th>Trạng thái</th>
                        <th></th>
                        </tr>`;

                    $('.order-list table tbody').empty();
                    $('.order-list table tbody').append(headerStr)

                    data.map(order => {
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
                        } else if (order.TrangThai === "confirmed") {
                            statusClass = "order-success";
                            statusText = "Đã xác nhận";
                        }

                        let str = `<tr>
                            <td class="order-id">${order.MaHD}</td>
                            <td class="order-date">${formatDate(order.NgayDat)}</td>
                            <td class="order-total">${order.TongTien.toLocaleString("vi-VN")}đ</td>
                            <td>
                                <div class="order-status ${paymentStatusClass}">${paymentText}</div>
                            </td>
                             <td>
                                <div class="order-status ${statusClass}">${statusText}</div>
                            </td>
                            <td> <a href="/order/detail/${order.MaHD}">Chi tiết</a></td>
                                </tr>`;

                        $('.order-list table tbody').append(str);
                    })

                } else {
                    $('.order-list table tbody').html(`<div class="my-5 text-center">Không tìm thấy đơn hàng nào</div>`);
                }


                setParentHeight();
            },
            error: () => { hideWebLoader(); }
        })
    }
})