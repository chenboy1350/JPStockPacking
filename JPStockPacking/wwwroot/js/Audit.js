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
            await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

async function FindConfirmedInvoice() {
    var txtInvoice = $('#txtInvoice').val().trim();

    if (!txtInvoice) return await swalWarning('กรุณาใส่เลขที่ใบแจ้งหนี้');

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
            await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

async function MarkInvoiceAsRead() {
    var txtInvoice = $('#txtInvoice').val().trim();
    const uid = $('#hddUserID').val();

    if (!txtInvoice) return await swalWarning('กรุณาใส่เลขที่ใบแจ้งหนี้');

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
                await swalSuccess(res.message);
            }
            else {
                await swalWarning(res.message);
            }
        },
        error: async (xhr) => {
            $('#loadingIndicator').hide();
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
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
    var isOver30Days = $('input[name="rdDays"]:checked').val() === "true";
    var isSample = $('input[name="rdIsSample"]:checked').val() === "true";

    if (!dtInvFromDate || !dtInvToDate) return swalWarning('กรุณาเลือกวันที่ให้ครบถ้วน');

    let model = {
        FromDate: dtInvFromDate ? new Date(dtInvFromDate).toISOString() : null,
        ToDate: dtInvToDate ? new Date(dtInvToDate).toISOString() : null,
        OrderNo: txtInvOrderNo,
        IsOver30Days: isOver30Days,
        IsSample: isSample
    };

    $.ajax({
        url: urlGetUnallocatedQuantityData,
        type: 'POST',
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        beforeSend: async () => $('#loadingIndicator').show(),
        success: async (data) => {
            $('#loadingIndicator').hide();

            let tbody = $('#tblUnallocated tbody');
            tbody.empty();

            if (data && data.length > 0) {
                data.forEach(item => {
                    let row = `<tr>
                        <td>${item.orderNo || ''}</td>
                        <td>${item.lotNo || ''}</td>
                        <td>${item.listNo || ''}</td>
                        <td>${item.article || ''}</td>
                        <td class="text-right">${(item.exportedQty || 0).toLocaleString()}</td>
                        <td class="text-right">${(item.storedQty || 0).toLocaleString()}</td>
                        <td class="text-right">${(item.meltedQty || 0).toLocaleString()}</td>
                        <td class="text-right font-weight-bold text-danger">${(item.unallocatedQty || 0).toLocaleString()}</td>
                    </tr>`;
                    tbody.append(row);
                });
            } else {
                tbody.append('<tr><td colspan="8" class="text-center">ไม่พบข้อมูล</td></tr>');
            }
        },
        error: async (xhr) => {
            $('#loadingIndicator').hide();
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });

}

function PrintUnalocateLot() {
    var txtInvOrderNo = $('#txtLocOrderNo').val().trim();

    var dtInvFromDate = $('#dtLocFromDate').val();
    var dtInvToDate = $('#dtLocToDate').val();
    var isOver30Days = $('input[name="rdDays"]:checked').val() === "true";
    var isSample = $('input[name="rdIsSample"]:checked').val() === "true";

    if (!dtInvFromDate || !dtInvToDate) return swalWarning('กรุณาเลือกวันที่ให้ครบถ้วน');

    let model = {
        FromDate: dtInvFromDate ? new Date(dtInvFromDate).toISOString() : null,
        ToDate: dtInvToDate ? new Date(dtInvToDate).toISOString() : null,
        OrderNo: txtInvOrderNo,
        IsOver30Days: isOver30Days,
        IsSample: isSample
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
            await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

function ClearLocInput() {
    $('#txtLocOrderNo').val('');

    $('#dtLocToDate').val(null);
    $('#dtLocFromDate').val(null);
    $('#tblUnallocated tbody').empty();
    $('#rdOver30').prop('checked', true);
}

function FindSendLostList() {
    var txtInvOrderNo = $('#txtSendLostOrderNo').val().trim();

    var dtInvFromDate = $('#dtSendLostFromDate').val();
    var dtInvToDate = $('#dtSendLostToDate').val();

    let model = {
        FromDate: dtInvFromDate ? new Date(dtInvFromDate).toISOString() : null,
        ToDate: dtInvToDate ? new Date(dtInvToDate).toISOString() : null,
        OrderNo: txtInvOrderNo
    };

    $.ajax({
        url: urlGetSendLostCheckList,
        type: 'POST',
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        beforeSend: async () => $('#loadingIndicator').show(),
        success: async (data) => {
            $('#loadingIndicator').hide();

            let tbody = $('#tblSendLostList tbody');
            tbody.empty();

            if (data && data.length > 0) {
                data.forEach(item => {
                    let row = `<tr>
                        <td>${item.customer || ''}</td>
                        <td>${item.orderNo || ''}</td>
                        <td>${item.lotNo || ''}</td>
                        <td>${item.listNo || ''}</td>
                        <td>${item.article || ''}</td>
                        <td class="text-right">${(item.qty || 0).toLocaleString()}</td>
                        <td class="text-right">${(item.wg || 0).toLocaleString()}</td>
                    </tr>`;
                    tbody.append(row);
                });
            } else {
                tbody.append('<tr><td colspan="7" class="text-center">ไม่พบข้อมูล</td></tr>');
            }
        },
        error: async (xhr) => {
            $('#loadingIndicator').hide();
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

function ClearSendLostInput() {
    $('#txtSendLostOrderNo').val('');
    $('#dtSendLostFromDate').val(null);
    $('#dtSendLostToDate').val(null);
    $('#tblSendLostList tbody').empty();
}

function ClearInvoice() {
    $('#txtInvoice').val('');
    $('#txtInvOrderNo').val('');

    $('#dtInvFromDate').val(null);
    $('#dtInvToDate').val(null);

    $('#btnMarkInvoiceAsRead').addClass('d-none');
    $('#btnFindConfirmedInvoice').addClass('d-none');
}
