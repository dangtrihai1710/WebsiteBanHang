// Tạo file: wwwroot/js/dynamic-categories.js

$(document).ready(function () {
    // Initialize dynamic categories
    loadDynamicCategories();
    setupSearchAutocomplete();
    setupCategoryFilters();
});

// Load categories from database
function loadDynamicCategories() {
    $.get('/Category/GetCategories')
        .done(function (categories) {
            renderNavigationCategories(categories);
            renderFooterCategories(categories);
            renderProductFilters(categories);
        })
        .fail(function () {
            console.error('Could not load categories');
            renderFallbackCategories();
        });
}

// Render navigation dropdown categories
function renderNavigationCategories(categories) {
    let html = '';

    categories.forEach(function (category) {
        html += `
            <li>
                <a class="dropdown-item" href="/Product?categoryId=${category.id}">
                    <i class="bi bi-tag me-2"></i>${category.name}
                    <span class="badge bg-secondary ms-2">${category.productCount}</span>
                </a>
            </li>
        `;
    });

    html += `
        <li><hr class="dropdown-divider"></li>
        <li>
            <a class="dropdown-item" href="/Product">
                <i class="bi bi-list me-2"></i>Tất cả sản phẩm
            </a>
        </li>
    `;

    $('#dynamic-categories').html(html);
}

// Render footer categories
function renderFooterCategories(categories) {
    let html = '';

    // Chỉ hiển thị 6 categories đầu tiên trong footer
    categories.slice(0, 6).forEach(function (category) {
        html += `
            <li class="mb-2">
                <a href="/Product?categoryId=${category.id}" class="text-muted text-decoration-none">
                    <i class="bi bi-chevron-right me-1"></i>${category.name}
                </a>
            </li>
        `;
    });

    html += `
        <li class="mb-2">
            <a href="/ShoppingCart" class="text-muted text-decoration-none">
                <i class="bi bi-chevron-right me-1"></i>Giỏ hàng
            </a>
        </li>
    `;

    $('#footer-categories').html(html);
}

// Render product page filters
function renderProductFilters(categories) {
    if ($('#category-filters').length === 0) return;

    const currentCategoryId = getCurrentCategoryId();
    let html = `
        <a href="/Product" class="btn ${!currentCategoryId ? 'btn-primary' : 'btn-outline-secondary'} btn-sm">
            <i class="bi bi-grid me-1"></i>Tất cả
        </a>
    `;

    categories.forEach(function (category) {
        const isActive = currentCategoryId == category.id;
        const btnClass = isActive ? 'btn-primary' : 'btn-outline-secondary';
        html += `
            <a href="/Product?categoryId=${category.id}" class="btn ${btnClass} btn-sm" data-category-id="${category.id}">
                <i class="bi bi-tag me-1"></i>${category.name}
                <span class="badge bg-light text-dark ms-1">${category.productCount}</span>
            </a>
        `;
    });

    $('#category-filters').html(html);
}

// Get current category ID from URL or ViewBag
function getCurrentCategoryId() {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('categoryId') || window.currentCategoryId || null;
}

// Setup search autocomplete
function setupSearchAutocomplete() {
    const searchInputs = $('.search-input');

    searchInputs.each(function () {
        const $input = $(this);
        let searchTimeout;

        $input.on('input', function () {
            clearTimeout(searchTimeout);
            const searchTerm = $(this).val().trim();

            if (searchTerm.length >= 2) {
                searchTimeout = setTimeout(() => {
                    showSearchSuggestions($input, searchTerm);
                }, 300);
            } else {
                hideSearchSuggestions($input);
            }
        });

        // Hide suggestions when clicking outside
        $(document).on('click', function (e) {
            if (!$input.is(e.target) && !$input.closest('.search-suggestions').length) {
                hideSearchSuggestions($input);
            }
        });
    });
}

// Show search suggestions
function showSearchSuggestions($input, searchTerm) {
    $.get('/Product/SearchProducts', { searchTerm: searchTerm })
        .done(function (response) {
            if (response.success && response.products.length > 0) {
                renderSearchSuggestions($input, response.products);
            } else {
                hideSearchSuggestions($input);
            }
        })
        .fail(function () {
            hideSearchSuggestions($input);
        });
}

// Render search suggestions dropdown
function renderSearchSuggestions($input, products) {
    // Remove existing suggestions
    $('.search-suggestions').remove();

    let html = '<div class="search-suggestions position-absolute bg-white border rounded shadow-sm w-100" style="top: 100%; z-index: 1000; max-height: 300px; overflow-y: auto;">';

    products.forEach(function (product) {
        html += `
            <div class="search-suggestion-item p-2 border-bottom" style="cursor: pointer;" data-product-id="${product.id}">
                <div class="d-flex align-items-center">
                    <img src="${product.imageUrl || '/images/placeholder.jpg'}" alt="${product.name}" 
                         class="me-2 rounded" style="width: 40px; height: 40px; object-fit: cover;">
                    <div class="flex-grow-1">
                        <div class="fw-bold">${product.name}</div>
                        <div class="text-muted small">${formatPrice(product.price)} VNĐ • ${product.categoryName || 'Chưa phân loại'}</div>
                    </div>
                </div>
            </div>
        `;
    });

    html += '</div>';

    const $wrapper = $input.closest('.input-group').length ? $input.closest('.input-group') : $input;
    $wrapper.css('position', 'relative').append(html);

    // Handle suggestion clicks
    $('.search-suggestion-item').on('click', function () {
        const productId = $(this).data('product-id');
        window.location.href = `/Product/Display/${productId}`;
    });

    // Highlight on hover
    $('.search-suggestion-item').on('mouseenter', function () {
        $(this).addClass('bg-light');
    }).on('mouseleave', function () {
        $(this).removeClass('bg-light');
    });
}

// Hide search suggestions
function hideSearchSuggestions($input) {
    $('.search-suggestions').remove();
}

// Setup category filter interactions
function setupCategoryFilters() {
    $(document).on('click', '[data-category-id]', function (e) {
        e.preventDefault();

        const categoryId = $(this).data('category-id');
        const currentSearch = new URLSearchParams(window.location.search).get('searchTerm');

        let url = '/Product';
        const params = new URLSearchParams();

        if (categoryId) {
            params.append('categoryId', categoryId);
        }

        if (currentSearch) {
            params.append('searchTerm', currentSearch);
        }

        if (params.toString()) {
            url += '?' + params.toString();
        }

        window.location.href = url;
    });
}

// Render fallback categories if API fails
function renderFallbackCategories() {
    const fallbackHtml = `
        <li>
            <a class="dropdown-item" href="/Product">
                <i class="bi bi-list me-2"></i>Tất cả sản phẩm
            </a>
        </li>
    `;

    $('#dynamic-categories').html(fallbackHtml);
    $('#footer-categories').html(`
        <li class="mb-2">
            <a href="/Product" class="text-muted text-decoration-none">
                <i class="bi bi-chevron-right me-1"></i>Tất cả sản phẩm
            </a>
        </li>
    `);
}

// Format price utility
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price);
}

// Enhanced category management
window.CategoryManager = {
    // Reload categories
    reload: function () {
        loadDynamicCategories();
    },

    // Get categories
    getCategories: function () {
        return $.get('/Category/GetCategories');
    },

    // Filter products by category
    filterByCategory: function (categoryId) {
        const url = categoryId ? `/Product?categoryId=${categoryId}` : '/Product';
        window.location.href = url;
    },

    // Search products
    searchProducts: function (searchTerm) {
        const url = `/Product?searchTerm=${encodeURIComponent(searchTerm)}`;
        window.location.href = url;
    }
};

// Auto refresh categories every 5 minutes (để đồng bộ khi admin thêm/sửa/xóa categories)
setInterval(function () {
    loadDynamicCategories();
}, 5 * 60 * 1000);

// Export for global use
window.loadDynamicCategories = loadDynamicCategories;