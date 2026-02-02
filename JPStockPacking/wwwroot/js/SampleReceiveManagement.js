$(document).ready(function () {
    $(document).on('keydown', '#txtFindSampleReceivedNo, #txtFindSampleOrderNo, #txtFindSampleLotNo', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            findSampleReceive();
        }
    });

    $(document).on('click', '#btnUpdateSampleLot', async function () {
        $('#loadingIndicator').show();

        const tbody = $('#tbl-sample-received-body');
        const hddReceiveNo = $('#hddSampleReceiveNo').val();
        const orderNos = [];
        const receiveIds = [];

        tbody.find('tr').each(function () {
            const chk = $(this).find('.chk-row');
            if (chk.is(':checked')) {
                const orderNo = $(this).data('order-no');
                if (orderNo && !orderNos.includes(orderNo)) {
                    orderNos.push(orderNo);
                }
                const receiveId = $(this).data('receive-id');
                if (receiveId) {
                    receiveIds.push(receiveId);
                }
            }
        });

        if (receiveIds.length === 0 || orderNos.length === 0) {
            $('#loadingIndicator').hide();
            await swalWarning('กรุณาเลือกข้อมูลที่จะนำเข้า');
            return;
        }

        const formData = new FormData();
        formData.append("receiveNo", hddReceiveNo);
        orderNos.forEach(no => formData.append("orderNos", no));
        receiveIds.forEach(no => formData.append("receiveIds", no));

        $.ajax({
            url: urlUpdateSampleLotItems,
            type: 'PATCH',
            processData: false,
            contentType: false,
            data: formData,
            success: async function (lot) {
                $('#loadingIndicator').hide();
                $('#modal-sample-update').modal('hide');
                refreshSampleReceiveRow(hddReceiveNo);
                swalToastSuccess(`นำเข้าแล้ว ${receiveIds.length} รายการ`);
            },
            error: async function (xhr) {
                $('#loadingIndicator').hide();
                let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
            }
        });
    });

    $(document).on('click', '#btnCancelUpdateSampleLot', async function () {
        $('#loadingIndicator').show();

        const tbody = $('#tbl-sample-cancel-received-body');
        const hddReceiveNo = $('#hddSampleCancelReceiveNo').val();
        const orderNos = [];
        const receiveIds = [];

        tbody.find('tr').each(function () {
            const chk = $(this).find('.chk-row');
            if (chk.is(':checked')) {
                const orderNo = $(this).data('order-no');
                if (orderNo && !orderNos.includes(orderNo)) {
                    orderNos.push(orderNo);
                }
                const receiveId = $(this).data('receive-id');
                if (receiveId) {
                    receiveIds.push(receiveId);
                }
            }
        });

        if (receiveIds.length === 0 || orderNos.length === 0) {
            $('#loadingIndicator').hide();
            await swalWarning('กรุณาเลือกข้อมูลที่จะยกเลิก');
            return;
        }

        const formData = new FormData();
        formData.append("receiveNo", hddReceiveNo);
        orderNos.forEach(no => formData.append("orderNos", no));
        receiveIds.forEach(no => formData.append("receiveIds", no));

        $.ajax({
            url: urlCancelUpdateSampleLotItems,
            type: 'PATCH',
            processData: false,
            contentType: false,
            data: formData,
            success: async function (lot) {
                $('#loadingIndicator').hide();
                $('#modal-sample-cancel-update').modal('hide');
                refreshSampleReceiveRow(hddReceiveNo);
                swalToastSuccess(`ยกเลิกการนำเข้าแล้ว ${receiveIds.length} รายการ`);
            },
            error: async function (xhr) {
                $('#loadingIndicator').hide();
                let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
            }
        });
    });

    $(document).on('change', '#chkSampleSelectAll', function () {
        const isChecked = $(this).is(':checked');
        $('#tbl-sample-received-body .chk-row:enabled')
            .prop('checked', isChecked)
            .trigger('change');
    });

    $(document).on('change', '#tbl-sample-received-body .chk-row', function () {
        const allEnabled = $('#tbl-sample-received-body .chk-row:enabled').length;
        const allChecked = $('#tbl-sample-received-body .chk-row:enabled:checked').length;

        $('#chkSampleSelectAll')
            .prop('checked', allEnabled > 0 && allChecked === allEnabled)
            .prop('indeterminate', allChecked > 0 && allChecked < allEnabled);
    });

});

function showModalUpdateSampleLot(receiveNo) {
    const txtFindLotNo = $('#txtFindSampleLotNo').val().trim();

    const modal = $('#modal-sample-update');
    const tbody = modal.find('#tbl-sample-received-body');

    tbody.empty().append('<tr><td colspan="10" class="text-center text-muted">กำลังโหลด...</td></tr>');

    modal.find('#txtTitleSampleUpdate').html(
        "<i class='fas fa-folder-plus'></i> รายการนำเข้าใบรับ (Sample): " + html(receiveNo)
    );

    modal.modal('show');

    $.ajax({
        url: urlImportSampleReceiveNo,
        method: 'GET',
        data: {
            receiveNo: receiveNo,
            lotNo: txtFindLotNo,
        },
        dataType: 'json',
    })
        .done(function (items) {
            tbody.empty();

            $('#hddSampleReceiveNo').val(receiveNo);

            if (!items || items.length === 0) {
                tbody.append('<tr><td colspan="10" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
                return;
            }

            const rows = items.map(function (x, i) {
                const safeId = ('chk_' + String(x.receiveNo ?? ('row' + i))).replace(/[^A-Za-z0-9_-]/g, '_');

                const isReceived = x.isReceived === true;
                const checkedAttr = isReceived ? '' : 'checked';
                const disabledAttr = isReceived ? 'disabled' : '';
                const rowClass = isReceived ? 'text-muted' : '';

                const lotNoDisplay = isReceived ? `<del>${html(x.lotNo)}</del>` : `<strong>${html(x.lotNo)}</strong>`;
                const orderNoDisplay = isReceived ? `<del>${html(x.orderNo)}</del>` : html(x.orderNo);
                const cusCodeDisplay = isReceived ? `<del>${html(x.custCode)}</del>` : html(x.custCode);
                const barcodeDisplay = isReceived ? `<del>${html(x.barcode)}</del>` : html(x.barcode);
                const articleDisplay = isReceived ? `<del>${html(x.article)}</del>` : html(x.article);
                const qtyDisplay = isReceived ? `<del>${num(x.ttQty)}</del>` : num(x.ttQty);
                const wgDisplay = isReceived ? `<del>${num(x.ttWg)}</del>` : num(x.ttWg);

                return `
                <tr class="${rowClass}"
                        data-receive-id="${html(x.receivedID)}"
                        data-order-no="${html(x.orderNo)}"
                        data-ttqty="${numRaw(x.ttQty)}"
                        data-ttwg="${numRaw(x.ttWg)}">
                    <td class="text-center">${i + 1}</td>
                    <td class="text-center">${cusCodeDisplay}</td>
                    <td>${orderNoDisplay}</td>
                    <td>${lotNoDisplay}</td>
                    <td class="text-center">${html(x.listNo)}</td>
                    <td>${barcodeDisplay}</td>
                    <td>${articleDisplay}</td>
                    <td class="text-end">${qtyDisplay}</td>
                    <td class="text-end">${wgDisplay}</td>
                    <td class="text-center">
                        <div class="chk-wrapper">
                            <input type="checkbox" id="${safeId}_${i}" class="chk-row custom-checkbox" ${checkedAttr} ${disabledAttr}>
                            <label for="${safeId}_${i}" class="d-none"></label>
                        </div>
                    </td>
                </tr>`;
            }).join('');

            tbody.append(rows);

            tbody.append(`
            <tr class="table-secondary fw-bold" id="totalRow">
                <td colspan="7" class="text-end">รวม</td>
                <td class="text-end" id="sumTtQty">0</td>
                <td class="text-end" id="sumTtWg">0</td>
                <td></td>
            </tr>
        `);


            calcTotal();

            const allEnabled = tbody.find('.chk-row:enabled').length;
            const allChecked = tbody.find('.chk-row:enabled:checked').length;

            $('#chkSampleSelectAll')
                .prop('checked', allEnabled > 0 && allChecked === allEnabled)
                .prop('indeterminate', allChecked > 0 && allChecked < allEnabled);

            tbody.on('change', '.chk-row', function () {
                calcTotal();

                const allEnabled = tbody.find('.chk-row:enabled').length;
                const allChecked = tbody.find('.chk-row:enabled:checked').length;

                $('#chkSampleSelectAll')
                    .prop('checked', allEnabled > 0 && allChecked === allEnabled)
                    .prop('indeterminate', allChecked > 0 && allChecked < allEnabled);
            });
        })
        .fail(function (xhr) {
            tbody.empty().append(
                `<tr><td colspan="10" class="text-danger text-center">
            เกิดข้อผิดพลาดในการโหลดข้อมูล (${xhr.status} ${xhr.statusText})
        </td></tr>`
            );
        });

    function calcTotal() {
        let sumQty = 0;
        let sumWg = 0;
        tbody.find('tr').each(function () {
            const tr = $(this);
            const chk = tr.find('.chk-row');
            if (chk.length && chk.is(':checked')) {
                sumQty += Number(tr.data('ttqty')) || 0;
                sumWg += Number(tr.data('ttwg')) || 0;
            }
        });
        $('#sumTtQty').text(num(sumQty));
        $('#sumTtWg').text(num(sumWg));
    }
}

function showModalCancelSampleLot(receiveNo) {
    const txtFindLotNo = $('#txtFindSampleLotNo').val().trim();

    const modal = $('#modal-sample-cancel-update');
    const tbody = modal.find('#tbl-sample-cancel-received-body');

    tbody.empty().append('<tr><td colspan="10" class="text-center text-muted">กำลังโหลด...</td></tr>');

    modal.find('#txtTitleSampleCancelUpdate').html(
        "<i class='fas fa-folder-plus'></i> รายการนำเข้าใบรับ (Sample): " + html(receiveNo)
    );

    modal.modal('show');

    $.ajax({
        url: urlCancelImportSampleReceiveNo,
        method: 'GET',
        data: {
            receiveNo: receiveNo,
            lotNo: txtFindLotNo,
        },
        dataType: 'json',
    })
        .done(function (items) {
            tbody.empty();

            $('#hddSampleCancelReceiveNo').val(receiveNo);

            if (!items || items.length === 0) {
                tbody.append('<tr><td colspan="10" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
                return;
            }

            const rows = items.map(function (x, i) {
                const safeId = ('chk_' + String(x.receiveNo ?? ('row' + i))).replace(/[^A-Za-z0-9_-]/g, '_');

                const isReceived = x.isReceived === false;
                const checkedAttr = isReceived ? '' : 'checked';
                const disabledAttr = isReceived ? 'disabled' : '';
                const rowClass = isReceived ? 'text-muted' : '';

                const lotNoDisplay = isReceived ? `<del>${html(x.lotNo)}</del>` : `<strong>${html(x.lotNo)}</strong>`;
                const orderNoDisplay = isReceived ? `<del>${html(x.orderNo)}</del>` : html(x.orderNo);
                const cusCodeDisplay = isReceived ? `<del>${html(x.custCode)}</del>` : html(x.custCode);
                const barcodeDisplay = isReceived ? `<del>${html(x.barcode)}</del>` : html(x.barcode);
                const articleDisplay = isReceived ? `<del>${html(x.article)}</del>` : html(x.article);
                const qtyDisplay = isReceived ? `<del>${num(x.ttQty)}</del>` : num(x.ttQty);
                const wgDisplay = isReceived ? `<del>${num(x.ttWg)}</del>` : num(x.ttWg);

                return `
            <tr class="${rowClass}"
                    data-receive-id="${html(x.receivedID)}"
                    data-order-no="${html(x.orderNo)}"
                    data-ttqty="${numRaw(x.ttQty)}"
                    data-ttwg="${numRaw(x.ttWg)}">
                <td class="text-center">${i + 1}</td>
                <td class="text-center">${cusCodeDisplay}</td>
                <td>${orderNoDisplay}</td>
                <td>${lotNoDisplay}</td>
                <td class="text-center">${html(x.listNo)}</td>
                <td>${barcodeDisplay}</td>
                <td>${articleDisplay}</td>
                <td class="text-end">${qtyDisplay}</td>
                <td class="text-end">${wgDisplay}</td>
                <td class="text-center">
                    <div class="chk-wrapper">
                        <input type="checkbox" id="${safeId}_${i}" class="chk-row custom-checkbox" ${checkedAttr} ${disabledAttr}>
                        <label for="${safeId}_${i}" class="d-none"></label>
                    </div>
                </td>
            </tr>`;
            }).join('');

            tbody.append(rows);

            tbody.append(`
            <tr class="table-secondary fw-bold" id="totalRow">
                <td colspan="7" class="text-end">รวม</td>
                <td class="text-end" id="sumTtQty">0</td>
                <td class="text-end" id="sumTtWg">0</td>
                <td></td>
            </tr>
        `);


            calcTotal();

            const allEnabled = tbody.find('.chk-row:enabled').length;
            const allChecked = tbody.find('.chk-row:enabled:checked').length;

            $('#chkSampleSelectAll')
                .prop('checked', allEnabled > 0 && allChecked === allEnabled)
                .prop('indeterminate', allChecked > 0 && allChecked < allEnabled);

            tbody.on('change', '.chk-row', function () {
                calcTotal();

                const allEnabled = tbody.find('.chk-row:enabled').length;
                const allChecked = tbody.find('.chk-row:enabled:checked').length;

                $('#chkSampleSelectAll')
                    .prop('checked', allEnabled > 0 && allChecked === allEnabled)
                    .prop('indeterminate', allChecked > 0 && allChecked < allEnabled);
            });
        })
        .fail(function (xhr) {
            tbody.empty().append(
                `<tr><td colspan="10" class="text-danger text-center">
                เกิดข้อผิดพลาดในการโหลดข้อมูล (${xhr.status} ${xhr.statusText})
            </td></tr>`
            );
        });

    function calcTotal() {
        let sumQty = 0;
        let sumWg = 0;
        tbody.find('tr').each(function () {
            const tr = $(this);
            const chk = tr.find('.chk-row');
            if (chk.length && chk.is(':checked')) {
                sumQty += Number(tr.data('ttqty')) || 0;
                sumWg += Number(tr.data('ttwg')) || 0;
            }
        });
        $('#sumTtQty').text(num(sumQty));
        $('#sumTtWg').text(num(sumWg));
    }
}

async function findSampleReceive() {
    const txtFindReceivedNo = $('#txtFindSampleReceivedNo').val().trim();
    const txtFindLotNo = $('#txtFindSampleLotNo').val().trim();

    const tbody = $('#tbl-sample-main tbody');

    tbody.html('<tr><td colspan="5" class="text-center text-muted">กำลังค้นหา...</td></tr>');

    $.ajax({
        url: urlGetSampleReceiveList,
        method: 'GET',
        data: {
            receiveNo: txtFindReceivedNo,
            lotNo: txtFindLotNo,
        },
        success: function (rows) {
            if (!rows || rows.length === 0) {
                tbody.html('<tr><td colspan="5" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
                return;
            }

            const body = rows.map(r => {
                const status = r.isReceived && !r.hasRevButNotAll
                    ? '<span class="badge badge-success">รับเข้าครบแล้ว</span>'
                    : r.hasRevButNotAll && !r.isReceived
                        ? '<span class="badge badge-warning">รับเข้ายังไม่ครบ</span>'
                        : '<span class="badge badge-secondary">รอรับเข้า</span>';

                const action = r.isReceived && !r.hasRevButNotAll
                    ? `<button class="btn btn-danger btn-sm" onclick="showModalCancelSampleLot('${r.receiveNo}')">
                            <i class="fas fa-folder"></i> ตรวจสอบ
                       </button>`
                    : `<button class="btn btn-warning btn-sm" onclick="showModalUpdateSampleLot('${r.receiveNo}')">
                            <i class="fas fa-folder"></i> ตรวจสอบ
                       </button>
                       <button class="btn btn-danger btn-sm" onclick="showModalCancelSampleLot('${r.receiveNo}')">
                            <i class="fas fa-folder"></i> ตรวจสอบ
                       </button>`;

                return `
                    <tr data-receive-no="${html(r.receiveNo)}">
                        <td>#</td>
                        <td class="col-receiveno"><strong>${html(r.receiveNo)}</strong></td>
                        <td class="col-mdate">${html(r.mdate)}</td>
                        <td class="col-status">${status}</td>
                        <td class="col-action">${action}</td>
                    </tr>
                `;
            }).join('');

            tbody.html(body);
        },
        error: function (xhr) {
            tbody.html(`<tr><td colspan="5" class="text-danger text-center">เกิดข้อผิดพลาด (${xhr.status} ${xhr.statusText})</td></tr>`);
        }
    });
}


function refreshSampleReceiveRow(receiveNo) {
    $.ajax({
        url: urlGetSampleReceiveRow,
        type: 'GET',
        data: { receiveNo: receiveNo },
        success: function (row) {
            if (!row) return;
            const tr = $(`#tbl-sample-main tr[data-receive-no="${row.receiveNo}"]`);
            if (tr.length) {
                tr.find('.col-mdate').text(row.mdat);

                if (row.isReceived && !row.hasRevButNotAll) {
                    tr.find('.col-status').html('<span class="badge badge-success">รับเข้าครบแล้ว</span>');
                    tr.find('.col-action').html(`
                        <button class="btn btn-danger btn-sm" onclick="showModalCancelSampleLot('${row.receiveNo}')">
                            <i class="fas fa-folder"></i> ตรวจสอบ
                        </button>
                    `);
                } else if (row.hasRevButNotAll && !row.isReceived) {
                    tr.find('.col-status').html('<span class="badge badge-warning">รับเข้ายังไม่ครบ</span>');
                    tr.find('.col-action').html(`
                        <button class="btn btn-warning btn-sm" onclick="showModalUpdateSampleLot('${row.receiveNo}')">
                            <i class="fas fa-folder"></i> ตรวจสอบ
                        </button>
                        <button class="btn btn-danger btn-sm" onclick="showModalCancelSampleLot('${row.receiveNo}')">
                            <i class="fas fa-folder"></i> ตรวจสอบ
                        </button>
                    `);
                } else {
                    tr.find('.col-status').html('<span class="badge badge-secondary">รอรับเข้า</span>');
                    tr.find('.col-action').html(`
                        <button class="btn btn-warning btn-sm" onclick="showModalUpdateSampleLot('${row.receiveNo}')">
                            <i class="fas fa-folder"></i> ตรวจสอบ
                        </button>
                        <button class="btn btn-danger btn-sm" onclick="showModalCancelSampleLot('${row.receiveNo}')">
                            <i class="fas fa-folder"></i> ตรวจสอบ
                        </button>
                    `);
                }
            }
        }
    });
}

function ClearFindBySampleReceive() {
    $("#txtFindSampleReceivedNo").val("");
    $("#txtFindSampleLotNo").val("");

    findSampleReceive()
}
