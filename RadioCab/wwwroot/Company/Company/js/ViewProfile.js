// Add animation to info sections on page load
document.addEventListener('DOMContentLoaded', function () {
    const infoSections = document.querySelectorAll('.info-section');
    infoSections.forEach((section, index) => {
        setTimeout(() => {
            section.style.opacity = '0';
            section.style.transform = 'translateY(20px)';
            section.style.transition = 'opacity 0.5s ease, transform 0.5s ease';

            setTimeout(() => {
                section.style.opacity = '1';
                section.style.transform = 'translateY(0)';
            }, 50);
        }, 300 + index * 100);
    });
});