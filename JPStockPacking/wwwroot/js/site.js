var ToastThemes = {
    success: { background: "#10b981", color: "#ffffff", iconColor: "#ffffff" },
    info: { background: "#1e40af", color: "#ffffff", iconColor: "#ffffff" },
    error: { background: "#ef4444", color: "#ffffff", iconColor: "#ffffff" },
    warning: { background: "#f59e0b", color: "#ffffff", iconColor: "#ffffff" },
    question: { background: "#f59e0b", color: "#ffffff", iconColor: "#ffffff" },
};

var ToastBase = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true,
    padding: '1.5em',
    width: '380px',
    customClass: {
        popup: 'shadow-lg rounded-3'
    },
    didOpen: function (toast) {
        var title = toast.querySelector('.swal2-title');
        if (title) title.style.color = 'inherit';
    }
});

var Toast = {
    fire: function (opts) {
        var theme = ToastThemes[opts.icon] || {};
        return ToastBase.fire(Object.assign({}, theme, opts));
    }
};

// ===== SweetAlert2 Toast Functions =====
function swalToastSuccess(message) {
    return Toast.fire({ icon: 'success', title: message || 'Operation completed successfully!' });
}

function swalToastError(message) {
    return Toast.fire({ icon: 'error', title: message || 'An error occurred.' });
}

function swalToastInfo(message) {
    return Toast.fire({ icon: 'info', title: message || 'Here is some information.' });
}

function swalToastWarning(message) {
    return Toast.fire({ icon: 'warning', title: message || 'Please be aware of this warning.' });
}

// ===== SweetAlert2 Alert Functions (styled like custom modal) =====
var swalBase = {
    customClass: {
        popup: 'swal-custom-popup',
        title: 'swal-custom-title',
        htmlContainer: 'swal-custom-body',
        confirmButton: 'swal-custom-btn',
        cancelButton: 'swal-custom-btn swal-custom-btn-cancel',
        closeButton: 'swal-custom-close'
    },
    showCloseButton: true,
    buttonsStyling: false
};

function swalSuccess(message, title) {
    return Swal.fire(Object.assign({}, swalBase, {
        icon: 'success', title: title || 'Success', text: message || 'Operation completed successfully!',
        customClass: Object.assign({}, swalBase.customClass, { confirmButton: 'swal-custom-btn swal-custom-btn-success' })
    }));
}

function swalError(message, title) {
    return Swal.fire(Object.assign({}, swalBase, {
        icon: 'error', title: title || 'Error', text: message || 'An error occurred. Please try again.',
        customClass: Object.assign({}, swalBase.customClass, { confirmButton: 'swal-custom-btn swal-custom-btn-danger' })
    }));
}

function swalInfo(message, title) {
    return Swal.fire(Object.assign({}, swalBase, {
        icon: 'info', title: title || 'Information', text: message || 'Here is some important information.',
        customClass: Object.assign({}, swalBase.customClass, { confirmButton: 'swal-custom-btn swal-custom-btn-info' })
    }));
}

function swalWarning(message, title) {
    return Swal.fire(Object.assign({}, swalBase, {
        icon: 'warning', title: title || 'Warning', text: message || 'Please be aware of this warning.',
        customClass: Object.assign({}, swalBase.customClass, { confirmButton: 'swal-custom-btn swal-custom-btn-warning' })
    }));
}

async function swalConfirm(message, title, onConfirm) {
    var result = await Swal.fire(
        Object.assign({}, swalBase, {
            icon: "question",
            title: title || "Confirm Action",
            text: message || "Are you sure you want to proceed?",
            showCancelButton: true,
            confirmButtonText: "Confirm",
            cancelButtonText: "Cancel",
            customClass: Object.assign({}, swalBase.customClass, {
                confirmButton: "swal-custom-btn swal-custom-btn-success",
            }),
        }),
    );
    if (result.isConfirmed && typeof onConfirm === 'function') {
        await onConfirm();
    }
    return result.isConfirmed;
}

async function swalDeleteConfirm(message, onConfirm, title) {
    var result = await Swal.fire(
        Object.assign({}, swalBase, {
            icon: "warning",
            title: title || "Confirm Delete",
            text:
                message || "Are you sure you want to delete? This cannot be undone.",
            showCancelButton: true,
            confirmButtonText: "Confirm",
            cancelButtonText: "Cancel",
            customClass: Object.assign({}, swalBase.customClass, {
                confirmButton: "swal-custom-btn swal-custom-btn-delete",
            }),
        }),
    );
    if (result.isConfirmed && typeof onConfirm === 'function') {
        await onConfirm();
    }
    return result.isConfirmed;
}

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

// Scroll handler for btnTop only - uses addEventListener to coexist with other scroll handlers
window.addEventListener('scroll', function () {
    var scrollTop = document.documentElement.scrollTop;
    var btn = document.getElementById("btnTop");
    if (btn) {
        btn.style.display = (scrollTop > 200) ? "flex" : "none";
    }
});

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