
        // Create animated background with cab driver theme - More Visible
    function createBackgroundShapes() {
            const background = document.getElementById('animated-background');
    const shapes = ['taxi', 'taxi-sign', 'road-line', 'traffic-light', 'steering-wheel', 'map-pin', 'road-sign', 'speedometer'];

    // Create more shapes for better visibility
    for (let i = 0; i < 30; i++) {
                const shape = document.createElement('div');
    const shapeType = shapes[Math.floor(Math.random() * shapes.length)];
    shape.className = `${shapeType}`;

    // Set random position
    shape.style.left = `${Math.random() * 100}%`;
    shape.style.top = `${Math.random() * 100}%`;

    // Set random animation
    let animationName, animationDuration;

    if (shapeType === 'taxi') {
        animationName = Math.random() > 0.5 ? 'driveRight' : 'driveLeft';
    animationDuration = `${Math.random() * 40 + 30}s`;
                    // Make some taxis bigger
                    if (Math.random() > 0.7) {
                        const size = Math.random() * 60 + 100;
    shape.style.width = `${size}px`;
    shape.style.height = `${size * 0.4}px`;
                    } else {
                        const size = Math.random() * 50 + 80;
    shape.style.width = `${size}px`;
    shape.style.height = `${size * 0.4}px`;
                    }
                } else if (shapeType === 'road-line') {
        animationName = 'driveRight';
    animationDuration = `${Math.random() * 20 + 15}s`;
    // Make some road lines longer
    shape.style.width = `${Math.random() * 100 + 150}px`;
                } else if (shapeType === 'steering-wheel') {
        animationName = Math.random() > 0.5 ? 'rotateSlow' : 'floatUpDown';
    animationDuration = `${Math.random() * 40 + 30}s`;
    const size = Math.random() * 40 + 50;
    shape.style.width = `${size}px`;
    shape.style.height = `${size}px`;
                } else if (shapeType === 'map-pin') {
        animationName = 'bounce';
    animationDuration = `${Math.random() * 10 + 5}s`;
                } else if (shapeType === 'speedometer') {
        animationName = 'rotateSlow';
    animationDuration = `${Math.random() * 30 + 20}s`;
                } else if (shapeType === 'road-sign') {
        animationName = 'floatUpDown';
    animationDuration = `${Math.random() * 15 + 10}s`;
                } else {
        animationName = 'floatUpDown';
    animationDuration = `${Math.random() * 15 + 10}s`;
                }

    shape.style.animationName = animationName;
    shape.style.animationDuration = animationDuration;
    shape.style.animationDelay = `${Math.random() * 5}s`;
    shape.style.animationIterationCount = 'infinite';

    // Random z-index for depth
    shape.style.zIndex = Math.floor(Math.random() * 5);

    background.appendChild(shape);
            }
        }

    // Initialize background when page loads
    document.addEventListener('DOMContentLoaded', function() {
        createBackgroundShapes();

    // File upload preview for driver photo
    const driverPhotoInput = document.getElementById('driverPhoto');
    const driverPhotoPreview = document.getElementById('driverPhotoPreview');
    const driverPhotoName = document.getElementById('driverPhotoName');
    const driverPhotoUpload = document.getElementById('driverPhotoUpload');

    driverPhotoUpload.addEventListener('click', function() {
        driverPhotoInput.click();
            });

    driverPhotoInput.addEventListener('change', function() {
                if (driverPhotoInput.files && driverPhotoInput.files[0]) {
                    const fileName = driverPhotoInput.files[0].name;
    driverPhotoName.textContent = fileName;
    driverPhotoName.style.display = 'block';

    const reader = new FileReader();
    reader.onload = function(e) {
        driverPhotoPreview.src = e.target.result;
    driverPhotoPreview.style.display = 'block';
                    }
    reader.readAsDataURL(driverPhotoInput.files[0]);
                }
            });

    // License file upload
    const licenseFileInput = document.getElementById('licenseFile');
    const licenseFileName = document.getElementById('licenseFileName');
    const licenseFileUpload = document.getElementById('licenseFileUpload');

    licenseFileUpload.addEventListener('click', function() {
        licenseFileInput.click();
            });

    licenseFileInput.addEventListener('change', function() {
                if (licenseFileInput.files && licenseFileInput.files[0]) {
                    const fileName = licenseFileInput.files[0].name;
    licenseFileName.textContent = fileName;
    licenseFileName.style.display = 'block';
                }
            });

    // Reset button functionality
    const resetBtn = document.getElementById('resetBtn');
    const form = document.getElementById('driverRegistrationForm');

    resetBtn.addEventListener('click', function() {
        // Clear all form fields
        form.reset();

    // Clear file previews
    driverPhotoPreview.style.display = 'none';
    driverPhotoName.style.display = 'none';
    licenseFileName.style.display = 'none';

    // Clear file inputs (needed because form.reset() doesn't clear file inputs in some browsers)
    driverPhotoInput.value = '';
    licenseFileInput.value = '';

    // Show confirmation message
    alert('Form has been reset. All fields have been cleared.');
            });

    
        });
