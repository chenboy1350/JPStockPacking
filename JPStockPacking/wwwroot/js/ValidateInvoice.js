$(document).ready(function () {
    $(document).on('keydown', '#txtInvOrderNo, #txtInvoice', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            FindInvoice();
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

function ClearInvoice(){
    $('#txtInvoice').val('');
    $('#txtInvOrderNo').val('');

    $('#dtInvFromDate').val(null);
    $('#dtInvToDate').val(null);
}