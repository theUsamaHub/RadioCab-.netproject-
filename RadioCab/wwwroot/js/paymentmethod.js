document.addEventListener('DOMContentLoaded', function () {
    // Set active navigation
    document.querySelector('[data-page="paymentmethod"]').classList.add('active');

    // Get anti-forgery token
    function getToken() {
        return document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]')?.value;
    }

    // Delete functionality (keep existing delete code)
    document.querySelectorAll('.js-delete-payment').forEach(btn => {
        btn.addEventListener('click', function () {
            const paymentMethodId = this.dataset.id;
            const token = getToken();

            Swal.fire({
                title: 'Are you sure?',
                text: 'This payment method will be permanently deleted!',
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

                fetch('/Admin/DeletePaymentMethodAjax', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded'
                    },
                    body: `id=${paymentMethodId}&__RequestVerificationToken=${encodeURIComponent(token)}`
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Deleted!',
                                text: 'Payment method has been deleted.',
                                timer: 1500,
                                showConfirmButton: false
                            });

                            // Remove row from table
                            btn.closest('tr').remove();

                            // Update table status display
                            updatePaymentMethodCount();

                            // Check if table is empty
                            if (document.querySelectorAll('tbody tr').length === 0) {
                                document.querySelector('#emptyState').style.display = 'block';
                                document.querySelector('.table-responsive').style.display = 'none';
                            }
                        } else {
                            Swal.fire('Error', data.message, 'error');
                        }
                    })
                    .catch(() => {
                        Swal.fire('Error', 'This is associated with transaction. You can DeActive.', 'error');
                    });
            });
        });
    });

    // Edit Payment Method Function
    function editPaymentMethod(paymentMethodId) {
        // Show loading state
        showLoading('Loading payment method data...');

        fetch('/Admin/GetPaymentMethod?id=' + paymentMethodId)
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                if (data) {
                    // Fill form with payment method data
                    document.getElementById('editPaymentMethodId').value = data.paymentMethodId;
                    document.getElementById('editMethodName').value = data.methodName;

                    // Set IsActive value
                    const isActiveSelect = document.getElementById('editIsActive');
                    isActiveSelect.value = data.isActive.toString();

                    // Clear previous errors
                    document.getElementById('editMethodNameError').textContent = '';

                    // Show modal
                    document.getElementById('editModal').style.display = 'flex';
                }
                hideLoading();
            })
            .catch(error => {
                hideLoading();
                Swal.fire('Error', 'Failed to load payment method data', 'error');
                console.error(error);
            });
    }

    // Save Edit Function
    function saveEdit() {
        const paymentMethodId = document.getElementById('editPaymentMethodId').value;
        const methodName = document.getElementById('editMethodName').value.trim();
        const isActive = document.getElementById('editIsActive').value === 'true';

        // Validate
        let isValid = true;

        // Clear previous errors
        document.getElementById('editMethodNameError').textContent = '';

        if (!methodName) {
            document.getElementById('editMethodNameError').textContent = 'Method name is required';
            isValid = false;
        }

        if (methodName.length < 3) {
            document.getElementById('editMethodNameError').textContent = 'Method name must be at least 3 characters';
            isValid = false;
        }

        if (methodName.length > 50) {
            document.getElementById('editMethodNameError').textContent = 'Method name must be less than 50 characters';
            isValid = false;
        }

        if (!isValid) return;

        // Show loading
        showLoading('Updating payment method...');

        // Prepare data
        const data = {
            PaymentMethodId: parseInt(paymentMethodId),
            MethodName: methodName,
            IsActive: isActive
        };

        // Send update request
        fetch('/Admin/UpdatePaymentMethod', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getToken()
            },
            body: JSON.stringify(data)
        })
            .then(response => response.json())
            .then(result => {
                hideLoading();

                if (result.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success!',
                        text: 'Payment method updated successfully.',
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        // Close modal
                        closeEditModal();
                        // Reload page to reflect changes
                        location.reload();
                    });
                } else {
                    // Show validation errors
                    if (result.errors) {
                        if (result.errors.MethodName) {
                            document.getElementById('editMethodNameError').textContent = result.errors.MethodName[0];
                        }
                    } else {
                        Swal.fire('Error', result.message || 'Failed to update payment method', 'error');
                    }
                }
            })
            .catch(error => {
                hideLoading();
                Swal.fire('Error', 'Network error occurred', 'error');
                console.error(error);
            });
    }

    // Close Edit Modal
    function closeEditModal() {
        document.getElementById('editModal').style.display = 'none';
        // Clear form
        document.getElementById('editPaymentMethodId').value = '';
        document.getElementById('editMethodName').value = '';
        document.getElementById('editIsActive').value = 'true';
        // Clear errors
        document.getElementById('editMethodNameError').textContent = '';
    }

    // Update payment method count
    function updatePaymentMethodCount() {
        const paymentMethodCount = document.querySelectorAll('tbody tr').length;
        document.getElementById('paymentMethodCount').textContent = `${paymentMethodCount} payment methods`;
    }

    // Loading helper functions
    function showLoading(message) {
        Swal.fire({
            title: message,
            allowOutsideClick: false,
            didOpen: () => Swal.showLoading()
        });
    }

    function hideLoading() {
        Swal.close();
    }

    // Close modal on ESC key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && document.getElementById('editModal').style.display === 'flex') {
            closeEditModal();
        }
    });

    // Close modal when clicking outside
    document.getElementById('editModal').addEventListener('click', function (e) {
        if (e.target === this) {
            closeEditModal();
        }
    });

    // Attach edit function to window for inline onclick
    window.editPaymentMethod = editPaymentMethod;
    window.saveEdit = saveEdit;
    window.closeEditModal = closeEditModal;

    // Form validation for add form
    const paymentMethodForm = document.getElementById('paymentMethodForm');
    if (paymentMethodForm) {
        paymentMethodForm.addEventListener('submit', function (e) {
            const methodName = document.getElementById('MethodName').value.trim();
            if (!methodName) {
                e.preventDefault();
                document.querySelector('[data-valmsg-for="paymentMethodValidateForm.MethodName"]').textContent = 'Method name is required';
                return false;
            }
            return true;
        });
    }

    // Reset form button
    const resetBtn = document.getElementById('resetBtn');
    if (resetBtn) {
        resetBtn.addEventListener('click', function () {
            // Clear validation messages
            document.querySelectorAll('.error-message').forEach(el => {
                el.textContent = '';
            });
        });
    }
});