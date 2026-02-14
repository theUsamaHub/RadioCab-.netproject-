document.addEventListener('DOMContentLoaded', function () {
    // Set active navigation
    document.querySelector('[data-page="city"]').classList.add('active');
});

// Improved City Management JavaScript
document.addEventListener('DOMContentLoaded', function () {

    // Get anti-forgery token
    function getToken() {
        return document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]')?.value;
    }

    // Delete functionality (existing code)
    document.querySelectorAll('.js-delete-user').forEach(btn => {
        btn.addEventListener('click', function () {
            const userId = this.dataset.id;
            const token = getToken();

            Swal.fire({
                title: 'Are you sure?',
                text: 'This city will be permanently deleted!',
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

                fetch('/Admin/DeleteCityAjax', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded'
                    },
                    body: `id=${userId}&__RequestVerificationToken=${encodeURIComponent(token)}`
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Deleted!',
                                text: 'City has been deleted.'
                            });

                            // Remove row from table
                            btn.closest('tr').remove();

                            // Update city count
                            updateCityCount();

                        } else {
                            Swal.fire('Error', data.message, 'error');
                        }
                    })
                    .catch(() => {
                        Swal.fire('Error', 'This city is already associated with registered drivers and companies; therefore, it cannot be deleted.', 'error');
                    });
            });
        });
    });

    // Edit City Function
    function editCity(cityId) {
        // Show loading state
        showLoading('Loading city data...');

        fetch('/Admin/GetCity?id=' + cityId)
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                if (data) {
                    // Fill form with city data
                    document.getElementById('editCityId').value = data.cityId;
                    document.getElementById('editCityName').value = data.cityName;
                    document.getElementById('editZipCode').value = data.zipCode || '';

                    // Clear previous errors
                    document.getElementById('editCityNameError').textContent = '';
                    document.getElementById('editZipCodeError').textContent = '';

                    // Show modal
                    document.getElementById('editModal').style.display = 'flex';
                }
                hideLoading();
            })
            .catch(error => {
                hideLoading();
                Swal.fire('Error', 'Failed to load city data', 'error');
                console.error(error);
            });
    }

    // Save Edit Function
    function saveEdit() {
        const cityId = document.getElementById('editCityId').value;
        const cityName = document.getElementById('editCityName').value.trim();
        const zipCode = document.getElementById('editZipCode').value.trim();

        // Validate
        let isValid = true;

        // Clear previous errors
        document.getElementById('editCityNameError').textContent = '';
        document.getElementById('editZipCodeError').textContent = '';

        if (!cityName) {
            document.getElementById('editCityNameError').textContent = 'City name is required';
            isValid = false;
        }

        if (cityName.length < 2) {
            document.getElementById('editCityNameError').textContent = 'City name must be at least 2 characters';
            isValid = false;
        }

        if (!isValid) return;

        // Show loading
        showLoading('Updating city...');

        // Prepare data
        const data = {
            CityId: parseInt(cityId),
            CityName: cityName,
            ZipCode: zipCode
        };

        // Send update request
        fetch('/Admin/UpdateCity', {
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
                        text: 'City updated successfully.',
                        timer: 1500,
                        showConfirmButton: false
                    });

                    // Close modal
                    closeEditModal();

                    // Reload page after a short delay
                    setTimeout(() => {
                        location.reload();
                    }, 1000);

                } else {
                    // Show validation errors
                    if (result.errors) {
                        if (result.errors.CityName) {
                            document.getElementById('editCityNameError').textContent = result.errors.CityName[0];
                        }
                        if (result.errors.ZipCode) {
                            document.getElementById('editZipCodeError').textContent = result.errors.ZipCode[0];
                        }
                    } else {
                        Swal.fire('Error', result.message || 'Failed to update city', 'error');
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
        document.getElementById('editCityId').value = '';
        document.getElementById('editCityName').value = '';
        document.getElementById('editZipCode').value = '';
        // Clear errors
        document.getElementById('editCityNameError').textContent = '';
        document.getElementById('editZipCodeError').textContent = '';
    }

    // Update city count
    function updateCityCount() {
        const cityCount = document.querySelectorAll('tbody tr').length;
        document.getElementById('cityCount').textContent = `${cityCount} cities`;
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
    window.editCity = editCity;
    window.saveEdit = saveEdit;
    window.closeEditModal = closeEditModal;

    // Form validation
    const cityForm = document.getElementById('cityForm');
    if (cityForm) {
        cityForm.addEventListener('submit', function (e) {
            const cityName = document.getElementById('CityName').value.trim();
            if (!cityName) {
                e.preventDefault();
                // Show error
                document.querySelector('[data-valmsg-for="City_Validate.CityName"]').textContent = 'City name is required';
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