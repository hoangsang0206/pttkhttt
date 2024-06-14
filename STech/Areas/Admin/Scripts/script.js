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

$('.admin-logout').click(() => {
    Swal.fire({
        title: "Đăng xuất tài khoản?",
        icon: "question",
        showCancelButton: true,
        showConfirmButton: true,
        cancelButtonText: "Hủy",
        confirmButtonText: "Đăng xuất"
    }).then((result) => {
        if (result.isConfirmed) {
            window.location.href = '/admin/account/logout'
        }
    });
})