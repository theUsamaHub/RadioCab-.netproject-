// Advertisement Management JavaScript

class AdvertisementManager {
    constructor() {
        this.init();
    }

    init() {
        this.setupImagePreviews();
        this.setupFormValidation();
        this.setupStatusToggle();
        this.setupTooltips();
        this.setupFileUploadValidation();
    }

    setupImagePreviews() {
        // Create Advertisement Image Preview
        const adImageInput = document.getElementById('adImageInput');
        const imagePreview = document.getElementById('imagePreview');
        
        if (adImageInput && imagePreview) {
            adImageInput.addEventListener('change', (e) => {
                this.handleImagePreview(e.target.files[0], imagePreview);
            });
        }

        // Edit Advertisement Image Preview
        const newImageInput = document.getElementById('newImageInput');
        const newImagePreview = document.getElementById('newImagePreview');
        
        if (newImageInput && newImagePreview) {
            newImageInput.addEventListener('change', (e) => {
                this.handleImagePreview(e.target.files[0], newImagePreview);
            });
        }
    }

    handleImagePreview(file, previewContainer) {
        if (file && file.type.startsWith('image/')) {
            const reader = new FileReader();
            reader.onload = (e) => {
                previewContainer.innerHTML = `
                    <div class="image-preview">
                        <img src="${e.target.result}" alt="Preview" />
                        <small class="text-muted d-block mt-1">Image preview</small>
                    </div>
                `;
            };
            reader.readAsDataURL(file);
        } else {
            previewContainer.innerHTML = '';
        }
    }

    setupFormValidation() {
        const advertisementForm = document.getElementById('advertisementForm');
        const editAdvertisementForm = document.getElementById('editAdvertisementForm');

        if (advertisementForm) {
            advertisementForm.addEventListener('submit', (e) => {
                this.validateAdvertisementForm(e, advertisementForm);
            });
        }

        if (editAdvertisementForm) {
            editAdvertisementForm.addEventListener('submit', (e) => {
                this.validateEditAdvertisementForm(e, editAdvertisementForm);
            });
        }

        // Real-time validation
        this.setupRealTimeValidation();
    }

    validateAdvertisementForm(e, form) {
        const adImageInput = document.getElementById('adImageInput');
        const titleInput = form.querySelector('#Title');
        const descriptionInput = form.querySelector('#Description');
        const startDateInput = form.querySelector('#StartDate');
        const paymentAmountSelect = document.getElementById('paymentAmountSelect');
        const paymentMethodSelect = form.querySelector('#PaymentMethodId');
        const transactionIdInput = form.querySelector('#TransactionId');

        let isValid = true;
        let errorMessage = '';

        // Validate image upload
        if (!adImageInput.files || adImageInput.files.length === 0) {
            errorMessage = 'Advertisement image is required.';
            isValid = false;
        }

        // Validate title
        if (!titleInput.value.trim()) {
            errorMessage = errorMessage || 'Advertisement title is required.';
            isValid = false;
        }

        // Validate description
        if (!descriptionInput.value.trim()) {
            errorMessage = errorMessage || 'Advertisement description is required.';
            isValid = false;
        }

        // Validate start date
        if (!startDateInput.value) {
            errorMessage = errorMessage || 'Start date is required.';
            isValid = false;
        } else {
            const startDate = new Date(startDateInput.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            
            if (startDate < today) {
                errorMessage = errorMessage || 'Start date cannot be in the past.';
                isValid = false;
            }
        }

        // Validate payment amount
        if (!paymentAmountSelect.value) {
            errorMessage = errorMessage || 'Please select a payment duration.';
            isValid = false;
        }

        // Validate payment method
        if (!paymentMethodSelect.value) {
            errorMessage = errorMessage || 'Please select a payment method.';
            isValid = false;
        }

        // Validate transaction ID
        if (!transactionIdInput.value.trim()) {
            errorMessage = errorMessage || 'Transaction ID is required.';
            isValid = false;
        }

        if (!isValid) {
            e.preventDefault();
            this.showToast(errorMessage, 'error');
            return false;
        }

        // Show loading state
        this.setFormLoading(form, true);
    }

    validateEditAdvertisementForm(e, form) {
        const titleInput = form.querySelector('#Title');
        const descriptionInput = form.querySelector('#Description');
        const startDateInput = form.querySelector('#StartDate');

        let isValid = true;
        let errorMessage = '';

        // Validate title
        if (!titleInput.value.trim()) {
            errorMessage = 'Advertisement title is required.';
            isValid = false;
        }

        // Validate description
        if (!descriptionInput.value.trim()) {
            errorMessage = errorMessage || 'Advertisement description is required.';
            isValid = false;
        }

        // Validate start date
        if (!startDateInput.value) {
            errorMessage = errorMessage || 'Start date is required.';
            isValid = false;
        } else {
            const startDate = new Date(startDateInput.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            
            if (startDate < today) {
                errorMessage = errorMessage || 'Start date cannot be in the past.';
                isValid = false;
            }
        }

        if (!isValid) {
            e.preventDefault();
            this.showToast(errorMessage, 'error');
            return false;
        }

        // Show loading state
        this.setFormLoading(form, true);
    }

    setupRealTimeValidation() {
        // Title validation
        const titleInputs = document.querySelectorAll('#Title');
        titleInputs.forEach(input => {
            input.addEventListener('input', (e) => {
                const value = e.target.value.trim();
                if (value.length > 0 && value.length < 3) {
                    this.showFieldError(e.target, 'Title must be at least 3 characters long.');
                } else {
                    this.clearFieldError(e.target);
                }
            });
        });

        // Description validation
        const descriptionInputs = document.querySelectorAll('#Description');
        descriptionInputs.forEach(input => {
            input.addEventListener('input', (e) => {
                const value = e.target.value.trim();
                if (value.length > 0 && value.length < 10) {
                    this.showFieldError(e.target, 'Description must be at least 10 characters long.');
                } else {
                    this.clearFieldError(e.target);
                }
            });
        });

        // Transaction ID validation
        const transactionInputs = document.querySelectorAll('#TransactionId');
        transactionInputs.forEach(input => {
            input.addEventListener('input', (e) => {
                const value = e.target.value.trim();
                const pattern = /^[A-Z0-9\-]+$/;
                if (value.length > 0 && !pattern.test(value)) {
                    this.showFieldError(e.target, 'Transaction ID can only contain uppercase letters, numbers, and hyphens.');
                } else {
                    this.clearFieldError(e.target);
                }
            });
        });
    }

    setupStatusToggle() {
        // This will be handled by the toggleStatus function
        // The actual toggle is done via AJAX call
    }

    setupTooltips() {
        // Initialize Bootstrap tooltips if available
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        }
    }

    setupFileUploadValidation() {
        const fileInputs = document.querySelectorAll('input[type="file"][accept="image/*"]');
        
        fileInputs.forEach(input => {
            input.addEventListener('change', (e) => {
                const file = e.target.files[0];
                if (file) {
                    // Check file size (max 5MB)
                    const maxSize = 5 * 1024 * 1024; // 5MB in bytes
                    if (file.size > maxSize) {
                        this.showToast('File size must be less than 5MB.', 'error');
                        e.target.value = '';
                        return;
                    }

                    // Check file type
                    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
                    if (!allowedTypes.includes(file.type)) {
                        this.showToast('Only JPG, PNG, and GIF files are allowed.', 'error');
                        e.target.value = '';
                        return;
                    }
                }
            });
        });
    }

    showFieldError(input, message) {
        this.clearFieldError(input);
        
        const errorDiv = document.createElement('div');
        errorDiv.className = 'text-danger small field-error';
        errorDiv.textContent = message;
        
        input.parentNode.appendChild(errorDiv);
        input.classList.add('is-invalid');
    }

    clearFieldError(input) {
        const existingError = input.parentNode.querySelector('.field-error');
        if (existingError) {
            existingError.remove();
        }
        input.classList.remove('is-invalid');
    }

    setFormLoading(form, loading) {
        const submitBtn = form.querySelector('button[type="submit"]');
        const formInputs = form.querySelectorAll('input, select, textarea');
        
        if (loading) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processing...';
            formInputs.forEach(input => input.disabled = true);
            form.classList.add('loading');
        } else {
            submitBtn.disabled = false;
            submitBtn.innerHTML = submitBtn.getAttribute('data-original-text') || '<i class="fas fa-save"></i> Save';
            formInputs.forEach(input => input.disabled = false);
            form.classList.remove('loading');
        }
    }

    showToast(message, type = 'info') {
        // Create toast container if it doesn't exist
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container';
            document.body.appendChild(toastContainer);
        }

        // Create toast element
        const toastId = 'toast-' + Date.now();
        const toastHTML = `
            <div id="${toastId}" class="toast toast-${type}" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="toast-header">
                    <strong class="me-auto">
                        ${type === 'success' ? '✓ Success' : 
                          type === 'error' ? '✗ Error' : 
                          type === 'warning' ? '⚠ Warning' : 'ℹ Info'}
                    </strong>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;

        toastContainer.insertAdjacentHTML('beforeend', toastHTML);

        // Initialize and show toast
        const toastElement = document.getElementById(toastId);
        if (typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastElement, {
                autohide: true,
                delay: 5000
            });
            toast.show();

            // Remove toast element after it's hidden
            toastElement.addEventListener('hidden.bs.toast', () => {
                toastElement.remove();
            });
        } else {
            // Fallback if Bootstrap is not available
            setTimeout(() => {
                toastElement.remove();
            }, 5000);
        }
    }
}

// Global function for status toggle (called from onclick)
function toggleStatus(advertisementId) {
    const checkbox = document.getElementById(`status-${advertisementId}`);
    const label = checkbox.nextElementSibling;
    
    // Show loading state
    checkbox.disabled = true;
    label.textContent = 'Updating...';
    
    // Make AJAX call to toggle status
    fetch(`/Company/ToggleAdvertisementStatus/${advertisementId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            checkbox.checked = data.isActive;
            label.textContent = data.isActive ? 'Active' : 'Inactive';
            
            // Show success message
            const adManager = new AdvertisementManager();
            adManager.showToast(data.message, 'success');
            
            // Reload page after a short delay to show updated status
            setTimeout(() => {
                window.location.reload();
            }, 1500);
        } else {
            // Revert checkbox state
            checkbox.checked = !checkbox.checked;
            label.textContent = checkbox.checked ? 'Active' : 'Inactive';
            
            // Show error message
            const adManager = new AdvertisementManager();
            adManager.showToast(data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Error toggling advertisement status:', error);
        
        // Revert checkbox state
        checkbox.checked = !checkbox.checked;
        label.textContent = checkbox.checked ? 'Active' : 'Inactive';
        
        // Show error message
        const adManager = new AdvertisementManager();
        adManager.showToast('An error occurred while updating status.', 'error');
    })
    .finally(() => {
        checkbox.disabled = false;
    });
}

// Global function for delete advertisement (called from onclick)
function deleteAdvertisement(advertisementId) {
    if (!confirm('Are you sure you want to delete this advertisement? This action cannot be undone.')) {
        return;
    }
    
    // Show loading state
    const deleteButton = event.target;
    const originalContent = deleteButton.innerHTML;
    deleteButton.disabled = true;
    deleteButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Deleting...';
    
    // Make AJAX call to delete advertisement
    fetch(`/Company/DeleteAdvertisement/${advertisementId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Show success message
            const adManager = new AdvertisementManager();
            adManager.showToast(data.message, 'success');
            
            // Remove the deleted row from table
            const row = deleteButton.closest('tr');
            if (row) {
                row.style.transition = 'opacity 0.3s';
                row.style.opacity = '0';
                
                setTimeout(() => {
                    row.remove();
                }, 300);
            }
        } else {
            // Show error message
            const adManager = new AdvertisementManager();
            adManager.showToast(data.message, 'error');
            
            // Restore button state
            deleteButton.disabled = false;
            deleteButton.innerHTML = originalContent;
        }
    })
    .catch(error => {
        console.error('Error deleting advertisement:', error);
        
        // Show error message and restore button
        const adManager = new AdvertisementManager();
        adManager.showToast('An error occurred while deleting advertisement.', 'error');
        
        deleteButton.disabled = false;
        deleteButton.innerHTML = originalContent;
    });
}

// Payment amount selection handler
function updatePaymentAmount() {
    const paymentAmountSelect = document.getElementById('paymentAmountSelect');
    if (paymentAmountSelect) {
        const selectedOption = paymentAmountSelect.options[paymentAmountSelect.selectedIndex];
        if (selectedOption) {
            const amount = selectedOption.getAttribute('data-amount');
            const duration = selectedOption.getAttribute('data-duration');
            
            // Update display if element exists
            const amountDisplay = document.getElementById('selectedAmount');
            if (amountDisplay) {
                amountDisplay.textContent = `$${amount} (${duration} months)`;
            }
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    const adManager = new AdvertisementManager();
    
    // Setup payment amount change handler
    const paymentAmountSelect = document.getElementById('paymentAmountSelect');
    if (paymentAmountSelect) {
        paymentAmountSelect.addEventListener('change', updatePaymentAmount);
    }
    
    // Store original button text
    const submitBtns = document.querySelectorAll('button[type="submit"]');
    submitBtns.forEach(btn => {
        btn.setAttribute('data-original-text', btn.innerHTML);
    });
});

// Export for global access
window.AdvertisementManager = AdvertisementManager;
window.toggleStatus = toggleStatus;
window.updatePaymentAmount = updatePaymentAmount;
