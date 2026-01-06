
async function showModalAsync(modalId, message = null, title = null) {
    return new Promise((resolve) => {
        const modal = document.getElementById(modalId);
        if (!modal) return resolve(false);

        if (message) {
            const el = modal.querySelector(`#${modalId.replace('Modal', 'Message')}`);
            if (el) el.textContent = message;
        }
        if (title) {
            const t = modal.querySelector('.custom-modal-title');
            if (t) t.textContent = title;
        }

        modal.classList.add('show');
        document.body.style.overflow = 'hidden';

        const clear = () => {
            modal.classList.remove('show');
            document.body.style.overflow = '';
        };

        const btnConfirm = modal.querySelector('#saveConfirmBtn');
        const btnCancel = modal.querySelector('.custom-btn-modal.cancel');

        const onConfirm = () => {
            cleanup();
            resolve(true);
        };
        const onCancel = () => {
            cleanup();
            resolve(false);
        };

        const cleanup = () => {
            btnConfirm?.removeEventListener('click', onConfirm);
            btnCancel?.removeEventListener('click', onCancel);
            clear();
        };

        btnConfirm?.addEventListener('click', onConfirm);
        btnCancel?.addEventListener('click', onCancel);
    });
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (!modal) return;

    modal.classList.remove('show');
    document.body.style.overflow = '';

    if (modalId === 'confirmModal') {
        document.getElementById('confirmBtn').onclick = null;
    }

    window.dispatchEvent(new CustomEvent('modalClosed', { detail: { modalId } }));
}

function showSuccess(message = 'Operation completed successfully!', title = 'Success') {
    return showModalAsync('successModal', message, title);
}

function showError(message = 'An error occurred. Please try again.', title = 'Error') {
    return showModalAsync('errorModal', message, title);
}

function showInfo(message = 'Here is some important information.', title = 'Information') {
    return showModalAsync('infoModal', message, title);
}

function showWarning(message = 'Please be aware of this warning.', title = 'Warning') {
    return showModalAsync('warningModal', message, title);
}

async function showSaveConfirm(
    message = "Are you sure you want to proceed?",
    title = "Confirm Action",
    onConfirm = null
) {
    const confirmed = await showModalAsync("saveConfirmModal", message, title);

    if (confirmed && typeof onConfirm === "function") {
        await onConfirm();
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

window.onscroll = function () {
    const btn = document.getElementById("btnTop");
    btn.style.display = (document.documentElement.scrollTop > 200) ? "block" : "none";
};

function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ==========================================
// Image Zoom on Hover Functionality (Fixed)
// ==========================================
let zoomPreview = null;

function createZoomPreview() {
    if (!zoomPreview) {
        zoomPreview = $('<div class="image-zoom-preview"><img src="" alt="Preview"></div>');
        $('body').append(zoomPreview);
    }
    return zoomPreview;
}

$(document).on('mouseenter', '.image-zoom-container', function (e) {
    const $container = $(this);
    const $img = $container.find('img');
    const imgSrc = $img.attr('src');

    if (!imgSrc || imgSrc.includes('blankimg.jpg')) {
        return;
    }

    const preview = createZoomPreview();
    const $previewImg = preview.find('img');

    $previewImg.attr('src', imgSrc);

    preview.show();

    preview.css({
        left: e.clientX + 20,
        top: e.clientY + 20
    });
});

$(document).on('mousemove', '.image-zoom-container', function (e) {
    if (!zoomPreview || !zoomPreview.is(':visible')) return;

    const offsetX = 20;
    const offsetY = 20;
    const previewWidth = zoomPreview.outerWidth();
    const previewHeight = zoomPreview.outerHeight();
    const windowWidth = $(window).width();
    const windowHeight = $(window).height();

    let left = e.clientX + offsetX;
    let top = e.clientY + offsetY;

    if (left + previewWidth > windowWidth) {
        left = e.clientX - previewWidth - offsetX;
    }

    if (top + previewHeight > windowHeight) {
        top = e.clientY - previewHeight - offsetY;
    }

    if (left < 0) {
        left = offsetX;
    }

    if (top < 0) {
        top = offsetY;
    }

    zoomPreview.css({ left: left, top: top });
});

$(document).on('mouseleave', '.image-zoom-container', function () {
    if (zoomPreview) {
        zoomPreview.hide();
    }
});

async function copyToClipboard(element, text) {
    try {
        await navigator.clipboard.writeText(text);

        var tooltip = document.createElement('span');
        tooltip.className = 'copy-tooltip';
        tooltip.textContent = 'คัดลอกแล้ว!';

        element.style.position = 'relative';
        element.appendChild(tooltip);

        setTimeout(function () {
            element.removeChild(tooltip);
        }, 2000);

    } catch (err) {
        console.error('ไม่สามารถคัดลอกได้:', err);
    }
}