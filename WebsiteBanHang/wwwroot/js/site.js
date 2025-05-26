// Enhanced Website JavaScript

document.addEventListener('DOMContentLoaded', function () {

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Header scroll effect
    const header = document.querySelector('.header-modern');
    let lastScrollTop = 0;

    window.addEventListener('scroll', function () {
        let scrollTop = window.pageYOffset || document.documentElement.scrollTop;

        if (scrollTop > 100) {
            header.style.background = 'rgba(255, 255, 255, 0.95)';
            header.style.backdropFilter = 'blur(20px)';
            header.style.boxShadow = '0 4px 20px rgba(0, 0, 0, 0.1)';
        } else {
            header.style.background = 'rgba(255, 255, 255, 0.95)';
            header.style.backdropFilter = 'blur(10px)';
            header.style.boxShadow = '0 1px 3px rgba(0, 0, 0, 0.1)';
        }

        // Hide/show header on scroll
        if (scrollTop > lastScrollTop && scrollTop > 500) {
            header.style.transform = 'translateY(-100%)';
        } else {
            header.style.transform = 'translateY(0)';
        }
        lastScrollTop = scrollTop;
    });

    // Animate numbers in stats section
    const animateNumbers = () => {
        const statsNumbers = document.querySelectorAll('.stat-number');
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const target = entry.target;
                    const finalNumber = target.textContent.replace(/[^\d]/g, '');
                    let currentNumber = 0;
                    const increment = Math.ceil(finalNumber / 100);
                    const timer = setInterval(() => {
                        currentNumber += increment;
                        if (currentNumber >= finalNumber) {
                            currentNumber = finalNumber;
                            clearInterval(timer);
                        }
                        target.textContent = currentNumber.toLocaleString() + '+';
                        if (target.textContent.includes('%')) {
                            target.textContent = currentNumber + '%';
                        }
                    }, 20);
                    observer.unobserve(target);
                }
            });
        });

        statsNumbers.forEach(number => observer.observe(number));
    };

    // Initialize number animation
    animateNumbers();

    // Product card hover effects
    const productCards = document.querySelectorAll('.product-card');
    productCards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-10px) scale(1.02)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });

    // Add to cart animation
    const addToCartButtons = document.querySelectorAll('.btn-add-cart');
    addToCartButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();

            // Create flying animation to cart
            const rect = this.getBoundingClientRect();
            const cartIcon = document.querySelector('.cart-btn');
            const cartRect = cartIcon.getBoundingClientRect();

            // Create animated element
            const flyingElement = document.createElement('div');
            flyingElement.innerHTML = '<i class="bi bi-cart-plus"></i>';
            flyingElement.style.cssText = `
                position: fixed;
                top: ${rect.top + rect.height / 2}px;
                left: ${rect.left + rect.width / 2}px;
                z-index: 9999;
                color: #3b82f6;
                font-size: 20px;
                pointer-events: none;
                transition: all 0.8s cubic-bezier(0.2, 0, 0.2, 1);
            `;

            document.body.appendChild(flyingElement);

            // Animate to cart
            setTimeout(() => {
                flyingElement.style.top = cartRect.top + 'px';
                flyingElement.style.left = cartRect.left + 'px';
                flyingElement.style.transform = 'scale(0.5)';
                flyingElement.style.opacity = '0';
            }, 100);

            // Remove element after animation
            setTimeout(() => {
                flyingElement.remove();

                // Update cart badge
                const cartBadge = document.querySelector('.cart-btn .badge');
                const currentCount = parseInt(cartBadge.textContent) || 0;
                cartBadge.textContent = currentCount + 1;

                // Bounce effect on cart
                cartIcon.style.transform = 'scale(1.1)';
                setTimeout(() => {
                    cartIcon.style.transform = 'scale(1)';
                }, 200);

            }, 800);

            // Button feedback
            this.innerHTML = '<i class="bi bi-check me-2"></i>Đã thêm!';
            this.classList.remove('btn-primary');
            this.classList.add('btn-success');

            setTimeout(() => {
                this.innerHTML = '<i class="bi bi-cart-plus me-2"></i>Thêm vào giỏ';
                this.classList.remove('btn-success');
                this.classList.add('btn-primary');
            }, 2000);
        });
    });

    // Newsletter form handling
    const newsletterForm = document.querySelector('.newsletter-form');
    if (newsletterForm) {
        const emailInput = newsletterForm.querySelector('input[type="email"]');
        const submitButton = newsletterForm.querySelector('button');

        submitButton.addEventListener('click', function (e) {
            e.preventDefault();

            const email = emailInput.value.trim();
            if (!email) {
                showNotification('Vui lòng nhập email của bạn!', 'warning');
                return;
            }

            if (!isValidEmail(email)) {
                showNotification('Email không hợp lệ!', 'danger');
                return;
            }

            // Show loading
            const originalText = this.innerHTML;
            this.innerHTML = '<span class="loading"></span> Đang xử lý...';
            this.disabled = true;

            // Simulate API call
            setTimeout(() => {
                this.innerHTML = '<i class="bi bi-check me-2"></i>Đã đăng ký!';
                this.classList.remove('btn-light');
                this.classList.add('btn-success');
                emailInput.value = '';

                showNotification('Đăng ký thành công! Cảm ơn bạn đã đăng ký.', 'success');

                setTimeout(() => {
                    this.innerHTML = originalText;
                    this.classList.remove('btn-success');
                    this.classList.add('btn-light');
                    this.disabled = false;
                }, 3000);
            }, 2000);
        });
    }

    // Floating elements animation
    const floatingElements = document.querySelectorAll('.floating-element');
    floatingElements.forEach((element, index) => {
        element.style.animationDelay = (index * 2) + 's';
    });

    // Parallax effect for hero section
    window.addEventListener('scroll', function () {
        const scrolled = window.pageYOffset;
        const parallax = document.querySelector('.hero-bg');
        if (parallax) {
            const speed = scrolled * 0.5;
            parallax.style.transform = `translateY(${speed}px)`;
        }
    });

    // Search functionality
    const searchInput = document.querySelector('.search-input');
    const searchButton = document.querySelector('.search-btn');

    if (searchInput && searchButton) {
        searchButton.addEventListener('click', function () {
            const searchTerm = searchInput.value.trim();
            if (searchTerm) {
                // Redirect to search results or filter products
                window.location.href = `/Product?search=${encodeURIComponent(searchTerm)}`;
            }
        });

        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                searchButton.click();
            }
        });
    }

    // Lazy loading for images
    const images = document.querySelectorAll('img[data-src]');
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                imageObserver.unobserve(img);
            }
        });
    });

    images.forEach(img => imageObserver.observe(img));

    // Utility Functions
    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    function showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} notification-toast`;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            min-width: 300px;
            opacity: 0;
            transform: translateX(100%);
            transition: all 0.3s ease;
        `;
        notification.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="bi bi-info-circle me-2"></i>
                <span>${message}</span>
                <button type="button" class="btn-close ms-auto" aria-label="Close"></button>
            </div>
        `;

        document.body.appendChild(notification);

        // Show notification
        setTimeout(() => {
            notification.style.opacity = '1';
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Auto close after 5 seconds
        setTimeout(() => {
            hideNotification(notification);
        }, 5000);

        // Close button functionality
        notification.querySelector('.btn-close').addEventListener('click', () => {
            hideNotification(notification);
        });
    }

    function hideNotification(notification) {
        notification.style.opacity = '0';
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            notification.remove();
        }, 300);
    }

    // Initialize tooltips if Bootstrap is available
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // Page loading animation
    window.addEventListener('load', function () {
        document.body.classList.add('loaded');

        // Animate elements on page load
        const animateElements = document.querySelectorAll('.animate-on-load');
        animateElements.forEach((element, index) => {
            setTimeout(() => {
                element.classList.add('animated');
            }, index * 100);
        });
    });

    console.log('🚀 Enhanced website functionality loaded!');
});