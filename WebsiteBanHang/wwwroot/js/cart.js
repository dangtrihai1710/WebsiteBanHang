$(document).ready(function () {
    // Cập nhật số lượng giỏ hàng khi trang load
    updateCartBadge();

    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        $('.alert').alert('close');
    }, 5000);
});

// Hàm cập nhật badge giỏ hàng (global function)
function updateCartBadge() {
    $.get('/ShoppingCart/GetCartCount', function (data) {
        const badge = $('#cart-badge');
        badge.text(data.count);

        if (data.count > 0) {
            badge.removeClass('d-none');
        } else {
            badge.addClass('d-none');
        }
    }).fail(function () {
        $('#cart-badge').text('0').addClass('d-none');
    });
}

// Export global function
window.updateCartBadge = updateCartBadge;

// Search form enhancement
$('.search-input').on('keypress', function (e) {
    if (e.which === 13) { // Enter key
        $(this).closest('form').submit();
    }
});

// Smooth scroll for footer links
$('a[href^="#"]').click(function (e) {
    e.preventDefault();
    const target = $(this.getAttribute('href'));
    if (target.length) {
        $('html, body').animate({
            scrollTop: target.offset().top - 100
        }, 800);
    }
});

// Newsletter subscription
$('.input-group button[type="button"]').click(function () {
    const email = $(this).siblings('input[type="email"]').val();
    if (email && isValidEmail(email)) {
        // Simulate subscription
        $(this).html('<i class="bi bi-check"></i>').addClass('btn-success').removeClass('btn-primary');
        setTimeout(() => {
            $(this).html('<i class="bi bi-send"></i>').removeClass('btn-success').addClass('btn-primary');
            $(this).siblings('input').val('');
        }, 2000);

        // Show success message
        showToast('Thành công!', 'Đã đăng ký nhận tin thành công!', 'success');
    } else {
        showToast('Lỗi!', 'Vui lòng nhập email hợp lệ!', 'error');
    }
});

// Email validation
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Global toast function
function showToast(title, message, type) {
    const toastClass = type === 'success' ? 'bg-success' : 'bg-danger';
    const icon = type === 'success' ? 'bi-check-circle' : 'bi-exclamation-triangle';

    const toastHtml = `
                <div class="toast ${toastClass} text-white" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="4000">
                    <div class="toast-header ${toastClass} text-white border-0">
                        <i class="bi ${icon} me-2"></i>
                        <strong class="me-auto">${title}</strong>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
                    </div>
                    <div class="toast-body">
                        ${message}
                    </div>
                </div>
            `;

    const $toast = $(toastHtml);
    $('#toast-container').append($toast);

    const bootstrap_toast = new bootstrap.Toast($toast[0]);
    bootstrap_toast.show();

    $toast.on('hidden.bs.toast', function () {
        $(this).remove();
    });
}

// Export global toast function
window.showToast = showToast;