
function showModal(modalId, message = null, title = null) {
    const modal = document.getElementById(modalId);
    if (!modal) return;

    if (message) {
        const messageElement = modal.querySelector(`#${modalId.replace('Modal', 'Message')}`);
        if (messageElement) {
            messageElement.textContent = message;
        }
    }

    if (title) {
        const titleElement = modal.querySelector('.custom-modal-title');
        if (titleElement) {
            titleElement.textContent = title;
        }
    }

    modal.classList.add('show');
    document.body.style.overflow = 'hidden';
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (!modal) return;

    modal.classList.remove('show');
    document.body.style.overflow = '';

    if (modalId === 'confirmModal') {
        document.getElementById('confirmBtn').onclick = null;
    }
}

function showSuccess(message = 'Operation completed successfully!', title = 'Success') {
    showModal('successModal', message, title);
}

function showError(message = 'An error occurred. Please try again.', title = 'Error') {
    showModal('errorModal', message, title);
}

function showInfo(message = 'Here is some important information.', title = 'Information') {
    showModal('infoModal', message, title);
}

function showWarning(message = 'Please be aware of this warning.', title = 'Warning') {
    showModal('warningModal', message, title);
}

function showSaveConfirm(
    message = "Are you sure you want to proceed?",
    onConfirm = null,
    title = "Confirm Action"
) {
    showModal("saveConfirmModal", message, title);

    if (onConfirm && typeof onConfirm === "function") {
        document.getElementById("saveConfirmBtn").onclick = function () {
            onConfirm();
            closeModal("saveConfirmModal");
        };
    }
}

function showDeleteConfirm(
    message = "Are you sure you want to proceed?",
    onConfirm = null,
    title = "Confirm Action"
) {
    showModal("deleteConfirmModal", message, title);

    if (onConfirm && typeof onConfirm === "function") {
        document.getElementById("deleteConfirmBtn").onclick = function () {
            onConfirm();
            closeModal("deleteConfirmModal");
        };
    }
}

document.addEventListener('click', function (e) {
    if (e.target.classList.contains('custom-modal-overlay')) {
        closeModal(e.target.id);
    }
});

document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const openModals = document.querySelectorAll('.custom-modal-overlay.show');
        openModals.forEach(modal => {
            closeModal(modal.id);
        });
    }
});

// ===== Helper Functions =====
function html(s) {
    if (s === null || s === undefined) return '';
    return String(s).replace(/[&<>"'`=\/]/g, function (c) {
        return {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;',
            '/': '&#x2F;',
            '`': '&#x60;',
            '=': '&#x3D;'
        }[c];
    });
}

function num(v) {
    if (v === null || v === undefined || v === '') return '';
    const n = Number(v);
    if (isNaN(n)) return html(v);
    return n.toLocaleString(undefined, { maximumFractionDigits: 2 });
}

function numRaw(v) {
    const n = Number(v);
    return isNaN(n) ? 0 : n;
}

function formatDate(dateString) {
    if (!dateString) return "-";
    const d = new Date(dateString);
    return d.toLocaleDateString('th-TH', { year: 'numeric', month: 'short', day: 'numeric' });
}

function addDays(date, days) {
    const result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
}
