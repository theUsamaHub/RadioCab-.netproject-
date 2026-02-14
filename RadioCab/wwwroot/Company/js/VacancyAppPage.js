// Application filter functionality
document.addEventListener('DOMContentLoaded', function () {
    // Filter elements
    const jobTypeFilter = document.getElementById('jobTypeFilter');
    const statusFilter = document.getElementById('statusFilter');

    // Table rows
    const tableRows = document.querySelectorAll('tbody tr');

    // Function to filter table rows
    function filterTable() {
        const jobTypeValue = jobTypeFilter.value;
        const statusValue = statusFilter.value;

        tableRows.forEach(row => {
            const jobType = row.cells[1].textContent.toLowerCase();
            const status = row.cells[5].querySelector('.badge-status').textContent.toLowerCase();

            let showRow = true;

            if (jobTypeValue && !jobType.includes(jobTypeValue)) {
                showRow = false;
            }

            if (statusValue && !status.includes(statusValue)) {
                showRow = false;
            }

            if (showRow) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    }

    // Add event listeners to filters
    jobTypeFilter.addEventListener('change', filterTable);
    statusFilter.addEventListener('change', filterTable);

    // Search functionality
    const searchInput = document.querySelector('input[type="text"]');
    const searchButton = document.querySelector('.btn-outline-secondary');

    searchButton.addEventListener('click', function () {
        const searchTerm = searchInput.value.toLowerCase();
        if (searchTerm.trim() === '') {
            tableRows.forEach(row => row.style.display = '');
            return;
        }

        let visibleCount = 0;
        tableRows.forEach(row => {
            const rowText = row.textContent.toLowerCase();
            if (rowText.includes(searchTerm)) {
                row.style.display = '';
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });

       
    });

    // Enter key for search
    searchInput.addEventListener('keyup', function (event) {
        if (event.key === 'Enter') {
            searchButton.click();
        }
    });
});