// Only hamburger menu functionality
document.getElementById('hamburgerMenu').addEventListener('click', () => {
    document.getElementById('sidebar').classList.toggle('active');
});

// Close sidebar when clicking outside on mobile
document.addEventListener('click', (e) => {
    const sidebar = document.getElementById('sidebar');
    const hamburgerMenu = document.getElementById('hamburgerMenu');

    if (window.innerWidth <= 1024 &&
        !sidebar.contains(e.target) &&
        !hamburgerMenu.contains(e.target)) {
        sidebar.classList.remove('active');
    }
});