
document.addEventListener('DOMContentLoaded', function () {
    // Set active navigation
    document.querySelector('[data-page="global-services"]').classList.add('active');
});

// platform-services-fixed.js
document.addEventListener('DOMContentLoaded', function () {

    // ==================== MODAL MANAGEMENT ====================

    // Store modal instances
    const addServiceModalEl = document.getElementById('addServiceModal');
    const editServiceModalEl = document.getElementById('editServiceModal');
    const viewServiceModalEl = document.getElementById('viewServiceModal');

    const addServiceModal = addServiceModalEl ? new bootstrap.Modal(addServiceModalEl) : null;
    const editServiceModal = editServiceModalEl ? new bootstrap.Modal(editServiceModalEl) : null;
    const viewServiceModal = viewServiceModalEl ? new bootstrap.Modal(viewServiceModalEl) : null;

    // ==================== ADD SERVICE MODAL FIX ====================

    // Force close Add Service Modal on Cancel/Close - even with validation errors
    if (addServiceModalEl) {
        // Handle Cancel button
        addServiceModalEl.querySelector('.modal-footer .btn-outline-secondary')?.addEventListener('click', function (e) {
            e.preventDefault();
            forceCloseModal(addServiceModal);
        });

        // Handle Close (X) button
        addServiceModalEl.querySelector('.modal-header .btn-close')?.addEventListener('click', function (e) {
            e.preventDefault();
            forceCloseModal(addServiceModal);
        });

        // Handle modal hidden event to clean up
        addServiceModalEl.addEventListener('hidden.bs.modal', function () {
            cleanUpModal('addServiceForm');

            // Force remove backdrop if still present
            const backdrop = document.querySelector('.modal-backdrop');
            if (backdrop) {
                backdrop.remove();
            }

            // Re-enable body scrolling
            document.body.classList.remove('modal-open');
            document.body.style.overflow = 'auto';
            document.body.style.paddingRight = '0';
        });

        // Prevent form submission from interfering with modal close
        const addForm = document.getElementById('addServiceForm');
        if (addForm) {
            addForm.addEventListener('submit', function (e) {
                // If form is invalid, let validation show errors but don't prevent modal behavior
                if (!this.checkValidity()) {
                    e.preventDefault();
                    e.stopPropagation();
                    this.classList.add('was-validated');
                }
            });
        }
    }

    // ==================== EDIT SERVICE MODAL ====================

    if (editServiceModalEl) {
        // Handle Cancel button for edit modal
        editServiceModalEl.querySelector('.modal-footer .btn-outline-secondary')?.addEventListener('click', function (e) {
            e.preventDefault();
            forceCloseModal(editServiceModal);
        });

        editServiceModalEl.querySelector('.modal-header .btn-close')?.addEventListener('click', function (e) {
            e.preventDefault();
            forceCloseModal(editServiceModal);
        });

        editServiceModalEl.addEventListener('hidden.bs.modal', function () {
            cleanUpModal('editServiceForm');
        });
    }

    // ==================== HELPER FUNCTIONS ====================

    function forceCloseModal(modalInstance) {
        if (!modalInstance) return;

        // Hide the modal
        modalInstance.hide();

        // Remove backdrop if Bootstrap didn't
        setTimeout(() => {
            const backdrop = document.querySelector('.modal-backdrop');
            if (backdrop) {
                backdrop.remove();
            }

            // Reset body classes
            document.body.classList.remove('modal-open');
            document.body.style.overflow = 'auto';
            document.body.style.paddingRight = '0';
        }, 150);
    }

    function cleanUpModal(formId) {
        const form = document.getElementById(formId);
        if (!form) return;

        // Reset form
        form.reset();

        // Clear validation errors
        form.querySelectorAll('.text-danger').forEach(el => el.textContent = '');
        form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
        form.querySelectorAll('.field-validation-error').forEach(el => el.remove());

        // Remove Bootstrap validation classes
        form.classList.remove('was-validated');

        // Clear any server-side validation summary
        const validationSummary = form.querySelector('.validation-summary-errors');
        if (validationSummary) validationSummary.remove();
    }

    // ==================== DELETE FUNCTIONALITY ====================

    document.querySelectorAll('.js-delete-user').forEach(btn => {
        btn.addEventListener('click', function () {
            const serviceId = this.dataset.id;
            const token = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]')?.value;

            Swal.fire({
                title: 'Are you sure?',
                text: 'This service will be permanently deleted!',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#e74c3c',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, delete'
            }).then((result) => {
                if (!result.isConfirmed) return;

                Swal.fire({
                    title: 'Deleting...',
                    text: 'Please wait',
                    allowOutsideClick: false,
                    didOpen: () => Swal.showLoading()
                });

                fetch('/Admin/DeletePlatformAjax', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded'
                    },
                    body: `id=${serviceId}&__RequestVerificationToken=${encodeURIComponent(token)}`
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Deleted!',
                                text: 'Service deleted successfully.'
                            });
                            this.closest('tr').remove();
                        } else {
                            Swal.fire('Error', data.message || 'Deletion failed', 'error');
                        }
                    })
                    .catch(() => Swal.fire('Error', 'Something went wrong.', 'error'));
            });
        });
    });

    // ==================== VIEW SERVICE MODAL ====================

    document.querySelectorAll('.view-service').forEach(btn => {
        btn.addEventListener('click', function () {
            document.getElementById('modalServiceId').textContent = this.dataset.serviceId;
            document.getElementById('modalServiceName').textContent = this.dataset.serviceName;
            document.getElementById('modalServiceDescription').textContent = this.dataset.serviceDescription;

            const statusBadge = document.getElementById('modalServiceStatus');
            statusBadge.textContent = this.dataset.serviceStatus;
            statusBadge.className = 'badge me-2 ' + (this.dataset.serviceStatus === 'Active' ? 'bg-success' : 'bg-warning');

            if (viewServiceModal) viewServiceModal.show();
        });
    });

    // ==================== EDIT SERVICE MODAL SETUP ====================

    document.querySelectorAll('.edit-service').forEach(btn => {
        btn.addEventListener('click', function () {
            // Clear previous validation
            cleanUpModal('editServiceForm');

            // Set form values
            document.getElementById('editServiceId').value = this.dataset.id;
            document.getElementById('editServiceName').value = this.dataset.name;
            document.getElementById('editServiceDescription').value = this.dataset.description;
            document.getElementById('editServiceStatus').value = this.dataset.status === 'Active' ? 'true' : 'false';

            if (editServiceModal) editServiceModal.show();
        });
    });

    // ==================== CLEAR ADD FORM WHEN OPENING ====================

    document.querySelector('[data-bs-target="#addServiceModal"]')?.addEventListener('click', function () {
        cleanUpModal('addServiceForm');
    });

    // ==================== AUTO-SHOW MODALS ON PAGE LOAD ====================

    // Show Add Modal if validation failed
    if (addServiceModal && '@(ViewBag.OpenAddModal?.ToString()?.ToLower() ?? "false")' === 'true') {
        setTimeout(() => {
            addServiceModal.show();
        }, 300);
    }

    // Show Edit Modal if validation failed
    if (editServiceModal && '@(ViewBag.OpenEditModal?.ToString()?.ToLower() ?? "false")' === 'true') {
        setTimeout(() => {
            editServiceModal.show();
        }, 300);
    }

    // ==================== GLOBAL ESC KEY TO CLOSE MODALS ====================

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            // Close any open modal
            if (addServiceModal && addServiceModal._isShown) {
                forceCloseModal(addServiceModal);
            }
            if (editServiceModal && editServiceModal._isShown) {
                forceCloseModal(editServiceModal);
            }
            if (viewServiceModal && viewServiceModal._isShown) {
                viewServiceModal.hide();
            }
        }
    });

    // ==================== CLICK OUTSIDE TO CLOSE (for backdrop) ====================

    document.addEventListener('click', function (e) {
        // If clicking on modal backdrop
        if (e.target.classList.contains('modal-backdrop')) {
            if (addServiceModal && addServiceModal._isShown) {
                forceCloseModal(addServiceModal);
            }
            if (editServiceModal && editServiceModal._isShown) {
                forceCloseModal(editServiceModal);
            }
            if (viewServiceModal && viewServiceModal._isShown) {
                viewServiceModal.hide();
            }
        }
    });
});