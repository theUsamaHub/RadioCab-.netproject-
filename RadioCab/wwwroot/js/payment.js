(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", initPage);

    const filterState = {
        status: "all",
        method: "all"
    };

    let allRows = [];

    function initPage() {
        try {
            cacheRows();
            bindEvents();
            updateCounter();
            // Set active navigation
            const paymentNav = document.querySelector('[data-page="payments"]');
            if (paymentNav) paymentNav.classList.add('active');
        } catch (e) {
            console.error("Initialization error:", e);
        }
    }

    function cacheRows() {
        const tbody = document.querySelector("tbody");
        if (!tbody) return;

        const rows = tbody.querySelectorAll("tr");
        allRows = Array.from(rows).filter(row => {
            // Check if this is a data row (has data-status attribute)
            return row.hasAttribute('data-status') &&
                row.hasAttribute('data-method') &&
                row.cells.length > 1;
        });

        console.log("Cached rows:", allRows.length);
    }

    function bindEvents() {
        const statusFilter = document.getElementById("statusFilter");
        const methodFilter = document.getElementById("methodFilter");
        const applyBtn = document.getElementById("applyFiltersBtn");
        const resetBtn = document.getElementById("resetFiltersBtn");

        if (statusFilter) {
            statusFilter.addEventListener("change", function () {
                filterState.status = this.value;
                applyFilters();
                updateActiveFilters();
            });
        }

        if (methodFilter) {
            methodFilter.addEventListener("change", function () {
                filterState.method = this.value;
                applyFilters();
                updateActiveFilters();
            });
        }

        if (applyBtn) {
            applyBtn.addEventListener("click", function () {
                filterState.status = statusFilter ? statusFilter.value : "all";
                filterState.method = methodFilter ? methodFilter.value : "all";
                applyFilters();
                updateActiveFilters();
            });
        }

        if (resetBtn) {
            resetBtn.addEventListener("click", resetFilters);
        }

        // Quick filter buttons
        document.querySelectorAll('.quick-filter-btn').forEach(btn => {
            btn.addEventListener('click', function () {
                const filterType = this.getAttribute('data-filter');
                const filterValue = this.getAttribute('data-value');

                if (filterType === 'status') {
                    if (statusFilter) statusFilter.value = filterValue;
                    filterState.status = filterValue;
                    applyFilters();
                    updateActiveFilters();
                }
                // Note: 'date' filter is not implemented as requested
            });
        });

        initDeleteButtons();
    }

    function applyFilters() {
        console.log("Applying filters:", filterState);

        let visibleCount = 0;

        allRows.forEach(row => {
            let show = true;

            // Get data from row attributes (these come from your Razor code)
            const rowStatus = row.getAttribute('data-status') || '';
            const rowMethod = row.getAttribute('data-method') || '';

            console.log("Row data:", {
                rowStatus,
                rowMethod,
                filterStatus: filterState.status,
                filterMethod: filterState.method
            });

            // Status filter
            if (filterState.status !== "all") {
                if (rowStatus !== filterState.status) {
                    show = false;
                }
            }

            // Method filter
            if (filterState.method !== "all") {
                if (rowMethod !== filterState.method) {
                    show = false;
                }
            }

            row.style.display = show ? "" : "none";
            if (show) visibleCount++;
        });

        console.log("Visible count after filtering:", visibleCount);
        updateCounter(visibleCount);
        toggleEmptyState(visibleCount === 0);
    }

    function updateActiveFilters() {
        const container = document.getElementById("activeFiltersContainer");
        const filtersElement = document.getElementById("activeFilters");

        if (!container || !filtersElement) return;

        filtersElement.innerHTML = '';

        const activeFilters = [];

        if (filterState.status !== "all") {
            activeFilters.push({
                type: "status",
                text: `Status: ${filterState.status}`,
                value: filterState.status
            });
        }

        if (filterState.method !== "all") {
            activeFilters.push({
                type: "method",
                text: `Method: ${filterState.method}`,
                value: filterState.method
            });
        }

        // Create filter tags
        activeFilters.forEach(filter => {
            const filterTag = document.createElement("span");
            filterTag.className = "badge bg-primary me-2 d-inline-flex align-items-center";
            filterTag.innerHTML = `
                ${filter.text}
                <button type="button" class="btn-close btn-close-white ms-1"
                    style="width: 8px; height: 8px; font-size: 0.5rem;"
                    data-type="${filter.type}"></button>
            `;

            // Add remove functionality
            const closeBtn = filterTag.querySelector("button");
            if (closeBtn) {
                closeBtn.addEventListener("click", function (e) {
                    e.stopPropagation();
                    removeFilter(filter.type);
                });
            }

            filtersElement.appendChild(filterTag);
        });

        // Show/hide container
        container.style.display = activeFilters.length > 0 ? "block" : "none";
    }

    function removeFilter(filterType) {
        const statusFilter = document.getElementById("statusFilter");
        const methodFilter = document.getElementById("methodFilter");

        switch (filterType) {
            case "status":
                if (statusFilter) statusFilter.value = "all";
                filterState.status = "all";
                break;
            case "method":
                if (methodFilter) methodFilter.value = "all";
                filterState.method = "all";
                break;
        }

        applyFilters();
        updateActiveFilters();
    }

    function resetFilters() {
        filterState.status = "all";
        filterState.method = "all";

        const statusFilter = document.getElementById("statusFilter");
        const methodFilter = document.getElementById("methodFilter");

        if (statusFilter) statusFilter.value = "all";
        if (methodFilter) methodFilter.value = "all";

        allRows.forEach(row => (row.style.display = ""));
        updateCounter(allRows.length);
        toggleEmptyState(false);
        updateActiveFilters();
    }

    function updateCounter(count) {
        const counter = document.querySelector(".text-muted.small");
        if (!counter) return;

        const visible = typeof count === "number"
            ? count
            : allRows.filter(r => r.style.display !== "none").length;

        counter.textContent = `Showing ${visible} of ${allRows.length} payments`;
    }

    function toggleEmptyState(show) {
        // Find empty state row
        const rows = document.querySelectorAll("tbody tr");
        let emptyRow = null;

        for (let row of rows) {
            if (row.querySelector(".empty-state")) {
                emptyRow = row;
                break;
            }
        }

        if (emptyRow) {
            emptyRow.style.display = show ? "" : "none";
        }
    }

    function initDeleteButtons() {
        const buttons = document.querySelectorAll(".js-delete-user");

        buttons.forEach(btn => {
            // Remove any existing listeners
            const newBtn = btn.cloneNode(true);
            btn.parentNode.replaceChild(newBtn, btn);

            newBtn.addEventListener("click", function (e) {
                e.preventDefault();

                const id = this.getAttribute("data-id");
                const tokenInput = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]');

                if (!id || !tokenInput) return;

                confirmDelete(id, tokenInput.value);
            });
        });
    }

    function confirmDelete(id, token) {
        Swal.fire({
            title: "Delete Payment?",
            text: "This action cannot be undone",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Delete",
            confirmButtonColor: "#e74c3c",
            cancelButtonText: "Cancel"
        }).then(result => {
            if (!result.isConfirmed) return;

            Swal.fire({
                title: "Deleting...",
                text: "Please wait",
                allowOutsideClick: false,
                didOpen: () => Swal.showLoading()
            });

            fetch("/Admin/DeletePaymentAjax", {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    "X-Requested-With": "XMLHttpRequest"
                },
                body: `id=${encodeURIComponent(id)}&__RequestVerificationToken=${encodeURIComponent(token)}`
            })
                .then(r => r.json())
                .then(data => {
                    if (data.success) {
                        removeRow(id);
                        Swal.fire({
                            icon: "success",
                            title: "Deleted!",
                            text: "Payment removed.",
                            timer: 1500,
                            showConfirmButton: false
                        });
                    } else {
                        Swal.fire("Error", data.message || "Delete failed", "error");
                    }
                })
                .catch(() => {
                    Swal.fire("Error", "Network error", "error");
                });
        });
    }

    function removeRow(id) {
        const btn = document.querySelector(`.js-delete-user[data-id="${id}"]`);
        if (!btn) return;

        const row = btn.closest("tr");
        if (!row) return;

        // Remove from cached rows
        const rowIndex = allRows.indexOf(row);
        if (rowIndex > -1) {
            allRows.splice(rowIndex, 1);
        }

        // Animate removal
        row.style.transition = "opacity 0.3s";
        row.style.opacity = "0";

        setTimeout(() => {
            row.remove();
            updateCounter();
            toggleEmptyState(allRows.length === 0);
        }, 300);
    }

})();