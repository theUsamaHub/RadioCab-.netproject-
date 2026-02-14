/**
 * Driver Advertisement Management JavaScript
 * Handles advertisement creation, editing, status toggling, and deletion for drivers
 */

class DriverAdvertisementManager {
    constructor() {
        this.initializeEventListeners();
        this.initializeTooltips();
    }

    /**
     * Initialize all event listeners
     */
    initializeEventListeners() {
        // Image preview for advertisement image
        const adImageInput = document.getElementById('adImageInput');
        if (adImageInput) {
            adImageInput.addEventListener('change', (e) => this.handleImagePreview(e, 'adImagePreview'));
        }

        // Image preview for new image in edit form
        const newImageInput = document.getElementById('newImageInput');
        if (newImageInput) {
            newImageInput.addEventListener('change', (e) => this.handleImagePreview(e, 'newImagePreview'));
        }

        // Payment screenshot preview
        const paymentScreenshotInput = document.getElementById('paymentScreenshotInput');
        if (paymentScreenshotInput) {
            paymentScreenshotInput.addEventListener('change', (e) => this.handleImagePreview(e, 'paymentScreenshotPreview'));
        }

        // Form validation on input
        const form = document.getElementById('advertisementForm') || document.getElementById('editAdvertisementForm');
        if (form) {
            const inputs = form.querySelectorAll('input, textarea, select');
            inputs.forEach(input => {
                input.addEventListener('blur', () => this.validateField(input));
                input.addEventListener('input', () => this.clearFieldError(input));
            });
        }
    }

    /**
     * Initialize Bootstrap tooltips
     */
    initializeTooltips() {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    /**
     * Handle image preview
     */
    handleImagePreview(event, previewContainerId) {
        const file = event.target.files[0];
        const previewContainer = document.getElementById(previewContainerId);

        if (!file || !previewContainer) return;

        // Validate file
        if (!this.validateImageFile(file)) {
            event.target.value = '';
            previewContainer.innerHTML = '';
            return;
        }

        // Show preview
        const reader = new FileReader();
        reader.onload = (e) => {
            const isPdf = file.type === 'application/pdf';
            const isDoc = file.type.includes('document') || file.name.endsWith('.doc') || file.name.endsWith('.docx');

            if (isPdf) {
                previewContainer.innerHTML = `
                    <div class="alert alert-info small">
                        <i class="fas fa-file-pdf me-2"></i>
                        <strong>${file.name}</strong> (${this.formatFileSize(file.size)})
                    </div>
                `;
            } else if (isDoc) {
                previewContainer.innerHTML = `
                    <div class="alert alert-info small">
                        <i class="fas fa-file-word me-2"></i>
                        <strong>${file.name}</strong> (${this.formatFileSize(file.size)})
                    </div>
                `;
            } else {
                previewContainer.innerHTML = `
                    <div class="position-relative d-inline-block">
                        <img src="${e.target.result}" alt="Preview" 
                             class="rounded border" style="max-width: 200px; max-height: 150px; object-fit: cover;" />
                        <button type="button" class="btn btn-sm btn-danger position-absolute top-0 end-0 m-1" 
                                onclick="this.parentElement.parentElement.innerHTML=''" title="Remove preview">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                `;
            }
        };

        if (file.type.startsWith('image/')) {
            reader.readAsDataURL(file);
        } else {
            // For non-image files, just show file info
            reader.onload();
        }
    }

    /**
     * Validate image file
     */
    validateImageFile(file) {
        // Check file size (5MB)
        const maxSize = 5 * 1024 * 1024;
        if (file.size > maxSize) {
            this.showToast('File size exceeds 5MB limit', 'error');
            return false;
        }

        // Check file type
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 
                           'application/pdf', 'application/msword', 
                           'application/vnd.openxmlformats-officedocument.wordprocessingml.document'];
        const allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.pdf', '.doc', '.docx'];
        
        const fileExtension = '.' + file.name.split('.').pop().toLowerCase();
        
        if (!allowedTypes.includes(file.type) && !allowedExtensions.includes(fileExtension)) {
            this.showToast('Invalid file type. Allowed: JPG, PNG, GIF, PDF, DOC', 'error');
            return false;
        }

        return true;
    }

    /**
     * Handle form submission for create advertisement
     */
    handleFormSubmit(event) {
        event.preventDefault();
        
        const form = event.target;
        const submitBtn = document.getElementById('submitBtn');
        
        // Show loading state
        this.setLoadingState(submitBtn, true);
        
        // Validate form
        if (!this.validateForm(form)) {
            this.setLoadingState(submitBtn, false);
            return;
        }

        // Submit form
        form.submit();
    }

    /**
     * Handle form submission for edit advertisement
     */
    handleEditFormSubmit(event) {
        event.preventDefault();
        
        const form = event.target;
        const submitBtn = document.getElementById('editSubmitBtn');
        
        // Show loading state
        this.setLoadingState(submitBtn, true);
        
        // Validate form
        if (!this.validateForm(form)) {
            this.setLoadingState(submitBtn, false);
            return;
        }

        // Submit form
        form.submit();
    }

    /**
     * Validate form fields
     */
    validateForm(form) {
        let isValid = true;
        const requiredFields = form.querySelectorAll('[required]');

        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                this.showFieldError(field, 'This field is required');
                isValid = false;
            } else {
                this.clearFieldError(field);
            }
        });

        // Validate title
        const titleField = form.querySelector('input[name*="Title"]');
        if (titleField && titleField.value.trim()) {
            if (titleField.value.length < 3) {
                this.showFieldError(titleField, 'Title must be at least 3 characters');
                isValid = false;
            }
        }

        // Validate description
        const descriptionField = form.querySelector('textarea[name*="Description"]');
        if (descriptionField && descriptionField.value.trim()) {
            if (descriptionField.value.length < 10) {
                this.showFieldError(descriptionField, 'Description must be at least 10 characters');
                isValid = false;
            }
        }

        // Validate start date
        const startDateField = form.querySelector('input[name*="StartDate"]');
        if (startDateField && startDateField.value) {
            const startDate = new Date(startDateField.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            
            if (startDate < today) {
                this.showFieldError(startDateField, 'Start date cannot be in the past');
                isValid = false;
            }
        }

        // Validate transaction ID format
        const transactionIdField = form.querySelector('input[name*="transactionId"]');
        if (transactionIdField && transactionIdField.value.trim()) {
            const transactionIdPattern = /^[A-Z0-9\-]+$/;
            if (!transactionIdPattern.test(transactionIdField.value)) {
                this.showFieldError(transactionIdField, 'Transaction ID must contain only uppercase letters, numbers, and hyphens');
                isValid = false;
            }
        }

        return isValid;
    }

    /**
     * Validate individual field
     */
    validateField(field) {
        if (field.hasAttribute('required') && !field.value.trim()) {
            this.showFieldError(field, 'This field is required');
        } else {
            this.clearFieldError(field);
        }
    }

    /**
     * Show field error
     */
    showFieldError(field, message) {
        this.clearFieldError(field);
        
        field.classList.add('is-invalid');
        
        const errorDiv = document.createElement('div');
        errorDiv.className = 'invalid-feedback';
        errorDiv.textContent = message;
        
        field.parentNode.appendChild(errorDiv);
    }

    /**
     * Clear field error
     */
    clearFieldError(field) {
        field.classList.remove('is-invalid');
        
        const errorDiv = field.parentNode.querySelector('.invalid-feedback');
        if (errorDiv) {
            errorDiv.remove();
        }
    }

    /**
     * Set loading state for button
     */
    setLoadingState(button, isLoading) {
        if (isLoading) {
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processing...';
        } else {
            button.disabled = false;
            button.innerHTML = button.getAttribute('data-original-text') || 
                           (button.id === 'submitBtn' ? 
                            '<i class="fas fa-save me-2"></i>Create Advertisement' : 
                            '<i class="fas fa-save me-2"></i>Update Advertisement');
        }
    }

    /**
     * Show toast notification
     */
    showToast(message, type = 'info') {
        // Remove existing toasts
        const existingToasts = document.querySelectorAll('.custom-toast');
        existingToasts.forEach(toast => toast.remove());

        const toastHtml = `
            <div class="custom-toast toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} border-0" 
                 role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="fas fa-${type === 'error' ? 'exclamation-circle' : type === 'success' ? 'check-circle' : 'info-circle'} me-2"></i>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;

        // Create toast container if it doesn't exist
        let toastContainer = document.getElementById('toastContainer');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toastContainer';
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '1050';
            document.body.appendChild(toastContainer);
        }

        toastContainer.insertAdjacentHTML('beforeend', toastHtml);

        // Initialize and show toast
        const toastElement = toastContainer.lastElementChild;
        const toast = new bootstrap.Toast(toastElement, {
            autohide: true,
            delay: 5000
        });

        toast.show();

        // Remove toast element after hidden
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    }

    /**
     * Format file size
     */
    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }
}

// Export for global access
window.DriverAdvertisementManager = DriverAdvertisementManager;
