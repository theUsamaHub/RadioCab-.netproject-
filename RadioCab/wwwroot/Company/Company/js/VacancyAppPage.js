// VacancyAppPage.js
document.addEventListener('DOMContentLoaded', function () {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Filter functionality
    const applyFiltersBtn = document.querySelector('.filter-container .btn-primary');
    const jobTypeFilter = document.getElementById('jobTypeFilter');
    const statusFilter = document.getElementById('statusFilter');
    const searchInput = document.querySelector('.application-table-container input[type="text"]');
    const searchBtn = document.querySelector('.application-table-container .btn-outline-secondary');
    const tableRows = document.querySelectorAll('.application-table-container tbody tr');

    // Apply filters
    applyFiltersBtn.addEventListener('click', function () {
        applyFilters();
    });

    // Apply search
    searchBtn.addEventListener('click', function () {
        applySearch();
    });

    // Enter key for search
    searchInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            applySearch();
        }
    });

    function applyFilters() {
        const selectedJobType = jobTypeFilter.value.toLowerCase();
        const selectedStatus = statusFilter.value.toLowerCase();

        tableRows.forEach(row => {
            const jobType = row.querySelector('.job-type-badge').textContent.toLowerCase().trim();
            const status = row.querySelector('.badge-status').textContent.toLowerCase().trim();

            let showRow = true;

            // Apply job type filter
            if (selectedJobType && jobType !== selectedJobType) {
                showRow = false;
            }

            // Apply status filter
            if (selectedStatus && status !== selectedStatus) {
                showRow = false;
            }

            // Show/hide row
            row.style.display = showRow ? '' : 'none';
        });
    }

    function applySearch() {
        const searchTerm = searchInput.value.toLowerCase().trim();

        if (!searchTerm) {
            // Show all rows if search is empty
            tableRows.forEach(row => {
                row.style.display = '';
            });
            return;
        }

        tableRows.forEach(row => {
            const rowText = row.textContent.toLowerCase();
            row.style.display = rowText.includes(searchTerm) ? '' : 'none';
        });
    }

    // Quick status update (if you want inline updates)
    const statusBadges = document.querySelectorAll('.badge-status');
    statusBadges.forEach(badge => {
        badge.addEventListener('click', function (e) {
            e.stopPropagation();
            const currentStatus = this.textContent.trim();
            const row = this.closest('tr');
            const applicationId = row.querySelector('a[href*="ViewVacancyApp"]')?.href?.split('id=')[1];

            if (applicationId) {
                // Show a quick status update modal (optional)
                showQuickStatusModal(applicationId, currentStatus, row);
            }
        });
    });

    // Quick status update modal (optional feature)
    function showQuickStatusModal(applicationId, currentStatus, row) {
        const statusOptions = ['Applied', 'Reviewed', 'Shortlisted', 'Hired', 'Rejected'];

        const modalHTML = `
            <div class="modal fade" id="quickStatusModal" tabindex="-1">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">Update Application Status</h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label class="form-label">Select New Status</label>
                                <select class="form-select" id="quickStatusSelect">
                                    ${statusOptions.map(status =>
            `<option value="${status}" ${status === currentStatus ? 'selected' : ''}>
                                            ${status}
                                        </option>`
        ).join('')}
                                </select>
                            </div>
                            <div class="alert alert-info">
                                <small><i class="bi bi-info-circle"></i> This will update the status immediately.</small>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-primary" id="saveQuickStatus">Update Status</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Remove existing modal if any
        const existingModal = document.getElementById('quickStatusModal');
        if (existingModal) {
            existingModal.remove();
        }

        // Add modal to body
        document.body.insertAdjacentHTML('beforeend', modalHTML);

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('quickStatusModal'));
        modal.show();

        // Handle save
        document.getElementById('saveQuickStatus').addEventListener('click', function () {
            const newStatus = document.getElementById('quickStatusSelect').value;

            // Update via AJAX
            updateStatus(applicationId, newStatus, row);
            modal.hide();
        });

        // Remove modal from DOM when hidden
        document.getElementById('quickStatusModal').addEventListener('hidden.bs.modal', function () {
            this.remove();
        });
    }

    // AJAX status update function
    function updateStatus(applicationId, newStatus, row) {
        fetch(`/Company/UpdateApplicationStatus?id=${applicationId}&status=${newStatus}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Update the status badge
                    const statusBadge = row.querySelector('.badge-status');
                    statusBadge.textContent = newStatus;
                    statusBadge.className = 'badge-status ' + getStatusClass(newStatus);

                    // Show success message
                    showToast('Status updated successfully!', 'success');
                } else {
                    showToast('Failed to update status', 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showToast('An error occurred', 'error');
            });
    }

    // Helper function to get status CSS class
    function getStatusClass(status) {
        switch (status.toLowerCase()) {
            case 'applied': return 'badge-pending';
            case 'reviewed': return 'badge-reviewed';
            case 'shortlisted': return 'badge-shortlisted';
            case 'hired': return 'badge-hired';
            case 'rejected': return 'badge-rejected';
            default: return 'badge-pending';
        }
    }

    // Toast notification function
    function showToast(message, type = 'info') {
        const toastContainer = document.getElementById('toastContainer') || createToastContainer();

        const toastId = 'toast-' + Date.now();
        const toastHTML = `
            <div id="${toastId}" class="toast align-items-center text-white bg-${type === 'success' ? 'success' : type === 'error' ? 'danger' : 'info'} border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="bi bi-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'} me-2"></i>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;

        toastContainer.insertAdjacentHTML('beforeend', toastHTML);
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { delay: 3000 });
        toast.show();

        // Remove toast after it's hidden
        toastElement.addEventListener('hidden.bs.toast', function () {
            this.remove();
        });
    }

    function createToastContainer() {
        const container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        container.style.zIndex = '1055';
        document.body.appendChild(container);
        return container;
    }

    // Add data-type attribute to job type badges for filtering
    document.querySelectorAll('.job-type-badge').forEach(badge => {
        const jobType = badge.textContent.trim().toLowerCase();
        badge.setAttribute('data-type', jobType.replace(' ', '-'));
    });

    // Initialize filter buttons
    const filterButtons = document.querySelectorAll('.filter-btn');
    filterButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            filterButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            applyFilters();
        });
    });

    // Export functionality (optional)
    const exportBtn = document.querySelector('.export-btn');
    if (exportBtn) {
        exportBtn.addEventListener('click', function () {
            exportTableToCSV();
        });
    }

    function exportTableToCSV() {
        let csv = [];
        const headers = [];

        // Get headers
        document.querySelectorAll('thead th').forEach(th => {
            headers.push(th.textContent.trim());
        });
        csv.push(headers.join(','));

        // Get rows data
        document.querySelectorAll('tbody tr:not([style*="none"])').forEach(row => {
            const rowData = [];
            row.querySelectorAll('td').forEach(td => {
                let text = td.textContent.trim();
                // Remove action button text
                if (td.querySelector('.action-btn')) {
                    text = '';
                }
                // Handle commas in text
                text = text.replace(/"/g, '""');
                if (text.includes(',')) {
                    text = `"${text}"`;
                }
                rowData.push(text);
            });
            csv.push(rowData.join(','));
        });

        // Create and download CSV file
        const csvContent = csv.join('\n');
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = `vacancy-applications-${new Date().toISOString().split('T')[0]}.csv`;
        link.click();

        showToast('Applications exported successfully!', 'success');
    }
});