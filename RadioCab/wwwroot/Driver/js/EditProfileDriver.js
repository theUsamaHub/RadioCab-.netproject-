// Initialize animations
    document.addEventListener('DOMContentLoaded', function() {
            // Add animation delays to form groups
            const formGroups = document.querySelectorAll('.form-group');
            formGroups.forEach((group, index) => {
        group.style.animationDelay = `${(index + 1) * 0.1}s`;
            });

    // Add animation to info items
    const infoItems = document.querySelectorAll('.info-item');
            infoItems.forEach((item, index) => {
        item.style.animationDelay = `${(index + 1) * 0.1}s`;
            });

            // Trigger animations
            setTimeout(() => {
        document.querySelectorAll('.form-group, .info-item, .section-header').forEach(el => {
            el.style.animation = 'fadeInUp 0.5s ease forwards';
        });
            }, 100);
        });
