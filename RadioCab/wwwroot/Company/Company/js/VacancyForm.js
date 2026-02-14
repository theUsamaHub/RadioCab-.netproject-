
    document.addEventListener('DOMContentLoaded', function() {
            // Character counter for job description
            const jobDescription = document.getElementById('jobDescription');
    const charCount = document.getElementById('charCount');

    jobDescription.addEventListener('input', function() {
                const currentLength = this.value.length;
    charCount.textContent = `${currentLength}/2000 characters`;

                // Change color when approaching limit
                if (currentLength > 1800) {
        charCount.style.color = 'var(--accent2)';
                } else if (currentLength > 1500) {
        charCount.style.color = 'var(--secondary)';
                } else {
        charCount.style.color = 'var(--neutral-light)';
                }
            });

    // Add focus effect to form controls
    const formControls = document.querySelectorAll('.form-control, .form-select');
            
            formControls.forEach(control => {
        // Add focus effect
        control.addEventListener('focus', function () {
            this.parentElement.style.transform = 'translateY(-2px)';
        });

    // Remove focus effect
    control.addEventListener('blur', function() {
        this.parentElement.style.transform = 'translateY(0)';
                });

    // Add input effect for text fields
    if (control.type !== 'select-one') {
        control.addEventListener('input', function () {
            if (this.value.length > 0) {
                this.style.backgroundColor = 'white';
            } else {
                this.style.backgroundColor = 'rgba(249, 250, 251, 0.7)';
            }
        });
                }
            });

    // Reset button functionality
    const resetButton = document.getElementById('resetButton');
    const form = document.getElementById('jobPostingForm');
    const resetConfirmation = document.getElementById('resetConfirmation');

    resetButton.addEventListener('click', function() {
        // Reset the form
        form.reset();

    // Reset character count
    charCount.textContent = '0/2000 characters';
    charCount.style.color = 'var(--neutral-light)';

                // Reset background colors
                formControls.forEach(control => {
                    if (control.type !== 'select-one') {
        control.style.backgroundColor = 'rgba(249, 250, 251, 0.7)';
                    }
                });

                // Reset any focus indicators
                document.querySelectorAll('.form-control:focus, .form-select:focus').forEach(el => {
        el.blur();
                });

    // Show reset confirmation
    resetConfirmation.classList.add('show');

                // Hide confirmation after 3 seconds
                setTimeout(() => {
        resetConfirmation.classList.remove('show');
                }, 3000);
            });

    // Add subtle hover effect to form container
    const formContainer = document.querySelector('.form-container');
    formContainer.addEventListener('mouseenter', function() {
        this.style.boxShadow = '0 15px 50px rgba(33, 42, 39, 0.12)';
            });

    formContainer.addEventListener('mouseleave', function() {
        this.style.boxShadow = '0 12px 40px rgba(33, 42, 39, 0.08)';
            });
        });
