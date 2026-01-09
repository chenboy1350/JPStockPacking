$(document).ready(function () {
    $(document).on('keydown', '#txtInvOrderNo, #txtInvoice', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            FindInvoice();
        }
    });

    $(document).on('change', '#txtInvoice', function (e) {
        $('#txtInvOrderNo').val('');
        CheckIsMarked();
    });

    $(document).on('keydown', '#txtInvoice', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            CheckIsMarked();
        }
    });
});

function FindInvoice() {
    var txtInvoice = $('#txtInvoice').val().trim();
    var txtInvOrderNo = $('#txtInvOrderNo').val().trim();

    var dtInvFromDate = $('#dtInvFromDate').val();
    var dtInvToDate = $('#dtInvToDate').val();

    const selected = $('input[name="r1"]:checked').val();

    let model = {
        FromDate: dtInvFromDate ? new Date(dtInvFromDate).toISOString() : null,
        ToDate: dtInvToDate ? new Date(dtInvToDate).toISOString() : null,
        InvoiceNo: txtInvoice,
        OrderNo: txtInvOrderNo,
        InvoiceType: parseInt(selected) || 0
    };

    let pdfWindow = window.open('', '_blank');

    $.ajax({
        url: urlGetComparedInvoice,
        type: 'POST',
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        xhrFields: {
            responseType: 'blob'
        },
        beforeSend: async () => $('#loadingIndicator').show(),
        success: async (data) => {
            const blob = new Blob([data], { type: 'application/pdf' });
            const blobUrl = URL.createObjectURL(blob);

            if (pdfWindow) {
                pdfWindow.location = blobUrl;
            }

            $('#loadingIndicator').hide();
        },
        error: async (xhr) => {
            $('#loadingIndicator').hide();

            if (pdfWindow) {
                pdfWindow.close();
            }

            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

async function FindConfirmedInvoice() {
    var txtInvoice = $('#txtInvoice').val().trim();

    if (!txtInvoice) return await showWarning('กรุณาใส่เลขที่ใบแจ้งหนี้');

    let model = {
        InvoiceNo: txtInvoice,
    };

    let pdfWindow = window.open('', '_blank');

    $.ajax({
        url: urlGetConfirmedInvoice,
        type: 'POST',
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        xhrFields: {
            responseType: 'blob'
        },
        beforeSend: async () => $('#loadingIndicator').show(),
        success: async (data) => {
            const blob = new Blob([data], { type: 'application/pdf' });
            const blobUrl = URL.createObjectURL(blob);

            if (pdfWindow) {
                pdfWindow.location = blobUrl;
            }

            $('#loadingIndicator').hide();
        },
        error: async (xhr) => {
            $('#loadingIndicator').hide();

            if (pdfWindow) {
                pdfWindow.close();
            }

            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

async function MarkInvoiceAsRead() {
    var txtInvoice = $('#txtInvoice').val().trim();
    const uid = $('#hddUserID').val();

    if (!txtInvoice) return await showWarning('กรุณาใส่เลขที่ใบแจ้งหนี้');

    const formData = new FormData();
    formData.append("InvoiceNo", txtInvoice);
    formData.append("userId", Number(uid));

    $.ajax({
        url: urlMarkInvoiceAsRead,
        type: 'POST',
        processData: false,
        contentType: false,
        data: formData,
        beforeSend: async () => $('#loadingIndicator').show(),
        success: async (res) => {
            $('#loadingIndicator').hide();
            CloseModal();
            CheckIsMarked();
            if (res.isSuccess) {
                await showSuccess(res.message);
            }
            else {
                await showWarning(res.message);
            }
        },
        error: async (xhr) => {
            $('#loadingIndicator').hide();
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

async function CheckIsMarked() {
    var txtInvoice = $('#txtInvoice').val().trim();

    $('#btnMarkInvoiceAsRead').addClass('d-none');
    $('#btnFindConfirmedInvoice').addClass('d-none');

    if (!txtInvoice) return;

    $.ajax({
        url: urlGetIsMarked,
        type: 'GET',
        data: { invoiceNo: txtInvoice },
        success: function (response) {
            if (response.isSuccess) {
                $('#btnFindConfirmedInvoice').removeClass('d-none');
                $('#btnMarkInvoiceAsRead').addClass('d-none');
            } else {
                $('#btnMarkInvoiceAsRead').removeClass('d-none');
                $('#btnFindConfirmedInvoice').addClass('d-none');
            }
        },
        error: function (xhr) {
            console.error('Error checking marked status:', xhr);
        }
    });
}

function FindUnalocateLot() {
    var txtInvOrderNo = $('#txtLocOrderNo').val().trim();

    var dtInvFromDate = $('#dtLocFromDate').val();
    var dtInvToDate = $('#dtLocToDate').val();

    if (!dtInvFromDate || !dtInvToDate) return showWarning('กรุณาเลือกวันที่ให้ครบถ้วน');

    let model = {
        FromDate: dtInvFromDate ? new Date(dtInvFromDate).toISOString() : null,
        ToDate: dtInvToDate ? new Date(dtInvToDate).toISOString() : null,
        OrderNo: txtInvOrderNo,
    };

    let pdfWindow = window.open('', '_blank');

    $.ajax({
        url: urlGetUnallocatedQuentityToStore,
        type: 'POST',
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        xhrFields: {
            responseType: 'blob'
        },
        beforeSend: async () => $('#loadingIndicator').show(),
        success: async (data) => {
            const blob = new Blob([data], { type: 'application/pdf' });
            const blobUrl = URL.createObjectURL(blob);

            if (pdfWindow) {
                pdfWindow.location = blobUrl;
            }

            $('#loadingIndicator').hide();
        },
        error: async (xhr) => {
            $('#loadingIndicator').hide();

            if (pdfWindow) {
                pdfWindow.close();
            }

            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });

}


function ClearInvoice(){
    $('#txtInvoice').val('');
    $('#txtInvOrderNo').val('');

    $('#dtInvFromDate').val(null);
    $('#dtInvToDate').val(null);

    $('#btnMarkInvoiceAsRead').addClass('d-none');
    $('#btnFindConfirmedInvoice').addClass('d-none');
}
