$(document).ready(function () {
    // Cập nhật số lượng giỏ hàng khi trang load
    updateCartBadge();

    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        $('.alert').alert('close');
    }, 5000);

    // Xử lý sự kiện đăng nhập/đăng xuất
    handleAuthEvents();
});

// Hàm cập nhật badge giỏ hàng (global function)
function updateCartBadge() {
    $.get('/ShoppingCart/GetCartCount', function (data) {
        const badge = $('#cart-badge');
        badge.text(data.count);

        if (data.count > 0) {
            badge.removeClass('d-none');
            // Animation effect
            badge.addClass('animate__animated animate__pulse');
            setTimeout(() => {
                badge.removeClass('animate__animated animate__pulse');
            }, 1000);
        } else {
            badge.addClass('d-none');
        }
    }).fail(function () {
        $('#cart-badge').text('0').addClass('d-none');
    });
}

// Xử lý sự kiện đăng nhập/đăng xuất
function handleAuthEvents() {
    // Xử lý khi đăng nhập thành công
    if (sessionStorage.getItem('justLoggedIn') === 'true') {
        // Merge giỏ hàng và cập nhật badge
        setTimeout(() => {
            updateCartBadge();
            sessionStorage.removeItem('justLoggedIn');
        }, 500);
    }

    // Xử lý khi đăng xuất
    $('button[type="submit"]').filter(function () {
        return $(this).text().includes('Đăng xuất');
    }).click(function () {
        // Đánh dấu đã đăng xuất để reset giỏ hàng
        sessionStorage.setItem('justLoggedOut', 'true');
    });

    // Xử lý sau khi đăng xuất
    if (sessionStorage.getItem('justLoggedOut') === 'true') {
        // Reset giỏ hàng về trạng thái guest
        setTimeout(() => {
            updateCartBadge();
            sessionStorage.removeItem('justLoggedOut');
        }, 500);
    }
}

// Hàm thêm sản phẩm vào giỏ hàng với xử lý user-specific
function addToCart(productId, quantity = 1) {
    return $.ajax({
        url: '/ShoppingCart/AddToCart',
        type: 'POST',
        data: {
            productId: productId,
            quantity: quantity
        },
        success: function (response) {
            if (response.success) {
                updateCartBadge();
                showToast('Thành công!', response.message, 'success');
            } else {
                showToast('Lỗi!', response.message, 'error');
            }
        },
        error: function () {
            showToast('Lỗi!', 'Không thể thêm sản phẩm vào giỏ hàng', 'error');
        }
    });
}

// Export global function
window.updateCartBadge = updateCartBadge;
window.addToCart = addToCart;

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

// Global toast function with enhanced styling
function showToast(title, message, type) {
    const toastClass = type === 'success' ? 'bg-success' : 'bg-danger';
    const icon = type === 'success' ? 'bi-check-circle' : 'bi-exclamation-triangle';

    const toastHtml = `
        <div class="toast ${toastClass} text-white border-0 shadow" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="4000">
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

    // Tạo container nếu chưa có
    if (!$('#toast-container').length) {
        $('body').append('<div id="toast-container" class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 1055;"></div>');
    }

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

// Enhanced cart management functions
window.CartManager = {
    // Thêm sản phẩm vào giỏ hàng
    addProduct: function (productId, quantity = 1) {
        return addToCart(productId, quantity);
    },

    // Cập nhật số lượng sản phẩm
    updateQuantity: function (productId, quantity) {
        return $.ajax({
            url: '/ShoppingCart/UpdateQuantity',
            type: 'POST',
            data: {
                productId: productId,
                quantity: quantity
            },
            success: function (response) {
                if (response.success) {
                    updateCartBadge();
                    showToast('Thành công!', 'Đã cập nhật giỏ hàng', 'success');
                } else {
                    showToast('Lỗi!', response.message, 'error');
                }
            },
            error: function () {
                showToast('Lỗi!', 'Không thể cập nhật giỏ hàng', 'error');
            }
        });
    },

    // Xóa sản phẩm khỏi giỏ hàng
    removeProduct: function (productId) {
        if (confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?')) {
            window.location.href = `/ShoppingCart/RemoveFromCart?productId=${productId}`;
        }
    },

    // Xóa toàn bộ giỏ hàng
    clearCart: function () {
        if (confirm('Bạn có chắc muốn xóa toàn bộ giỏ hàng?')) {
            window.location.href = '/ShoppingCart/ClearCart';
        }
    },

    // Lấy thông tin giỏ hàng
    getCartInfo: function () {
        return $.get('/ShoppingCart/GetCartCount');
    }
};

// Debug function (chỉ trong development)
if (window.location.hostname === 'localhost') {
    window.debugCart = function () {
        console.log('=== Cart Debug Info ===');
        console.log('Current User:', $('[data-user-id]').data('user-id') || 'Guest');
        console.log('Session ID:', document.cookie.match(/AspNetCore\.Session=([^;]+)/)?.[1] || 'Unknown');

        CartManager.getCartInfo().done(function (data) {
            console.log('Cart Count:', data.count);
        });
    };
}