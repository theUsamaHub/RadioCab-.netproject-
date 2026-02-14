const step1 = document.getElementById('step1');
const step2 = document.getElementById('step2');
const step1Indicator = document.getElementById('step1-indicator');
const step2Indicator = document.getElementById('step2-indicator');
const nextBtn = document.getElementById('nextBtn');
const backBtn = document.getElementById('backBtn');
const resetBtn = document.getElementById('resetBtn');
const form = document.getElementById('registrationForm');

// Function to preview image
function previewImage(input, previewId) {
    const previewContainer = document.getElementById(previewId + 'Container');
    const preview = document.getElementById(previewId);
    const fileNameDisplay = document.getElementById(previewId.replace('Preview', 'FileName'));

    if (input.files && input.files[0]) {
        const file = input.files[0];

        // Check if file is an image
        if (!file.type.match('image.*')) {
            alert('Please select an image file (PNG, JPG, GIF)');
            input.value = '';
            previewContainer.style.display = 'none';
            return;
        }

        const reader = new FileReader();

        reader.onload = function (e) {
            preview.src = e.target.result;
            previewContainer.style.display = 'block';

            // Show file name
            if (fileNameDisplay) {
                fileNameDisplay.textContent = file.name;
                fileNameDisplay.style.display = 'inline-block';
            }
        }

        reader.readAsDataURL(file);
    } else {
        previewContainer.style.display = 'none';
        if (fileNameDisplay) {
            fileNameDisplay.style.display = 'none';
        }
    }
}

// Function to show file name for PDFs
function showFileName(input, fileNameId) {
    const fileNameDisplay = document.getElementById(fileNameId);

    if (input.files && input.files[0]) {
        const file = input.files[0];

        // Check if file is a PDF
        if (!file.type.match('application/pdf') && !file.name.toLowerCase().endsWith('.pdf')) {
            alert('Please select a PDF file');
            input.value = '';
            fileNameDisplay.style.display = 'none';
            return;
        }

        fileNameDisplay.textContent = file.name;
        fileNameDisplay.style.display = 'inline-block';
    } else {
        fileNameDisplay.style.display = 'none';
    }
}

// Next button click handler
nextBtn.addEventListener('click', function () {
    // Hide step 1, show step 2
    step1.classList.remove('active');
    step2.classList.add('active');

    // Update step indicators
    step1Indicator.classList.remove('active');
    step1Indicator.classList.add('completed');
    step2Indicator.classList.add('active');
});

// Back button click handler
backBtn.addEventListener('click', function () {
    // Hide step 2, show step 1
    step2.classList.remove('active');
    step1.classList.add('active');

    // Update step indicators
    step2Indicator.classList.remove('active');
    step1Indicator.classList.add('active');
});

// Reset button click handler
resetBtn.addEventListener('click', function () {
    // Confirm with user before resetting
    if (confirm('Are you sure you want to reset the entire form? All entered data will be lost.')) {
        // Reset the form
        form.reset();

        // Hide step 2, show step 1
        step2.classList.remove('active');
        step1.classList.add('active');

        // Reset step indicators
        step1Indicator.classList.remove('completed');
        step1Indicator.classList.add('active');
        step2Indicator.classList.remove('completed', 'active');

        // Clear file previews
        document.querySelectorAll('.image-preview-container').forEach(el => {
            el.style.display = 'none';
        });
        document.querySelectorAll('.file-name').forEach(el => {
            el.style.display = 'none';
        });

    }
});


