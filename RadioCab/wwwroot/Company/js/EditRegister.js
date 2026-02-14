// Add focus effects to form inputs
document.querySelectorAll('.form-control, .form-select').forEach(element => {
    element.addEventListener('focus', function () {
        this.parentElement.style.transform = 'translateY(-2px)';
    });

    element.addEventListener('blur', function () {
        this.parentElement.style.transform = 'translateY(0)';
    });
});

// Add hover effect to contact info items
document.querySelectorAll('.info-item').forEach(item => {
    item.addEventListener('mouseenter', function () {
        this.style.transform = 'translateY(-5px) scale(1.02)';
    });

    item.addEventListener('mouseleave', function () {
        this.style.transform = 'translateY(0) scale(1)';
    });
});

// Initialize animations
document.addEventListener('DOMContentLoaded', function () {
    // Animate form groups with staggered delay
    const formGroups = document.querySelectorAll('.form-group');
    formGroups.forEach((group, index) => {
        group.style.animationDelay = `${0.1 + (index * 0.1)}s`;
    });

    // Animate contact info items
    const infoItems = document.querySelectorAll('.info-item');
    infoItems.forEach((item, index) => {
        item.style.animationDelay = `${0.1 + (index * 0.1)}s`;
    });
});
  