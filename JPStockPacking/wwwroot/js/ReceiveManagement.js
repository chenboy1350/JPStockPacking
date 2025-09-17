$(document).ready(function () {
    $(document).on('click', '#btnUpdateLot', async function () {
        $('#loadingIndicator').show();

        const tbody = $('#tbl-received-body');
        const hddReceiveNo = $('#hddReceiveNo').val();
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
            await showWarning('กรุณาเลือกข้อมูลที่จะนำเข้า');
            $('#loadingIndicator').hide();
            return;
        }

        const formData = new FormData();
        formData.append("receiveNo", hddReceiveNo);
        orderNos.forEach(no => formData.append("orderNos", no));
        receiveIds.forEach(no => formData.append("receiveIds", no));

        $.ajax({
            url: urlUpdateLotItems,
            type: 'PATCH',
            processData: false,
            contentType: false,
            data: formData,
            success: async function (lot) {
                $('#loadingIndicator').hide();
                await showSuccess("อัปเดตสำเร็จ");
                $('#modal-update').modal('hide');
                refreshReceiveRow(hddReceiveNo); 
            },
            error: async function (xhr) {
                $('#loadingIndicator').hide();
                await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${xhr.statusText})`);
            }
        });
    });

    $(document).on('change', '#chkSelectAll', function () {
        const isChecked = $(this).is(':checked');
        $('#tbl-received-body .chk-row:enabled')
            .prop('checked', isChecked)
            .trigger('change');
    });

    $(document).on('change', '#tbl-received-body .chk-row', function () {
        const allEnabled = $('#tbl-received-body .chk-row:enabled').length;
        const allChecked = $('#tbl-received-body .chk-row:enabled:checked').length;

        $('#chkSelectAll')
            .prop('checked', allEnabled > 0 && allChecked === allEnabled)
            .prop('indeterminate', allChecked > 0 && allChecked < allEnabled);
    });

});

function showModalUpdateLot(receiveNo) {
    const modal = $('#modal-update');
    const tbody = modal.find('#tbl-received-body');

    tbody.empty().append('<tr><td colspan="9" class="text-center text-muted">กำลังโหลด...</td></tr>');

    modal.find('#txtTitleUpdate').html(
        "<i class='fas fa-folder-plus'></i> รายการนำ: " + html(receiveNo)
    );

    modal.modal('show');
    console.log('typeof html =', typeof html);


    $.ajax({
        url: urlImportReceiveNo,
        method: 'GET',
        data: { receiveNo: receiveNo },
        dataType: 'json',
        cache: false
    })
    .done(function (items) {
        tbody.empty();

        $('#hddReceiveNo').val(receiveNo);

        if (!items || items.length === 0) {
            tbody.append('<tr><td colspan="9" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
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
                    <td>${html(x.receiveNo)}</td>
                    <td>${lotNoDisplay}</td>
                    <td>${orderNoDisplay}</td>
                    <td class="text-center">${html(x.listNo)}</td>
                    <td>${barcodeDisplay}</td>
                    <td>${articleDisplay}</td>
                    <td class="text-end">${qtyDisplay}</td>
                    <td class="text-end">${wgDisplay}</td>
                    <td class="text-center">
                        <div class="icheck-primary d-inline">
                            <input type="checkbox" id="${safeId}_${i}" class="chk-row" ${checkedAttr} ${disabledAttr}>
                            <label for="${safeId}_${i}"></label>
                        </div>
                    </td>
                </tr>`;
          }).join('');

        tbody.append(rows);

        tbody.append(`
            <tr class="table-secondary fw-bold" id="totalRow">
                <td colspan="6" class="text-end">รวม</td>
                <td class="text-end" id="sumTtQty">0</td>
                <td class="text-end" id="sumTtWg">0</td>
                <td></td>
            </tr>
        `);


        calcTotal();

        const allEnabled = tbody.find('.chk-row:enabled').length;
        const allChecked = tbody.find('.chk-row:enabled:checked').length;

        $('#chkSelectAll')
            .prop('checked', allEnabled > 0 && allChecked === allEnabled)
            .prop('indeterminate', allChecked > 0 && allChecked < allEnabled);

        tbody.on('change', '.chk-row', function () {
            calcTotal();

            const allEnabled = tbody.find('.chk-row:enabled').length;
            const allChecked = tbody.find('.chk-row:enabled:checked').length;

            $('#chkSelectAll')
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

async function findReceive() {
    const keyword = $('#txtFindReceivedNo').val().trim();
    const tbody = $('#tbl-main tbody');

    console.log('typeof html =', typeof html);


    tbody.html('<tr><td colspan="3" class="text-center text-muted">กำลังค้นหา...</td></tr>');

    $.ajax({
        url: urlGetReceiveList,
        method: 'GET',
        data: { receiveNo: keyword },
        success: function (rows) {
            if (!rows || rows.length === 0) {
                tbody.html('<tr><td colspan="3" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
                return;
            }

            const body = rows.map(r => {
                const status = r.isReceived && !r.hasRevButNotAll
                    ? '<span class="badge badge-success">รับเข้าครบแล้ว</span>'
                    : r.hasRevButNotAll && !r.isReceived
                        ? '<span class="badge badge-warning">รับเข้ายังไม่ครบ</span>'
                        : '<span class="badge badge-secondary">รอรับเข้า</span>';

                const action = r.isReceived && !r.hasRevButNotAll
                    ? ''
                    : `<button class="btn btn-warning btn-sm" onclick="showModalUpdateLot('${r.receiveNo}')">
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
            tbody.html(`<tr><td colspan="3" class="text-danger text-center">เกิดข้อผิดพลาด (${xhr.status} ${xhr.statusText})</td></tr>`);
        }
    });
}


function refreshReceiveRow(receiveNo) {
    $.ajax({
        url: urlGetReceiveRow,
        type: 'GET',
        data: { receiveNo: receiveNo },
        success: function (row) {
            console.log(row)
            if (!row) return;
            const tr = $(`#tbl-main tr[data-receive-no="${row.receiveNo}"]`);
            if (tr.length) {
                tr.find('.col-mdate').text(formatDate(row.mdate));

                if (row.isReceived && !row.hasRevButNotAll) {
                    tr.find('.col-status').html('<span class="badge badge-success">รับเข้าครบแล้ว</span>');
                    tr.find('.col-action').empty();
                } else if (row.hasRevButNotAll && !row.isReceived) {
                    tr.find('.col-status').html('<span class="badge badge-warning">รับเข้ายังไม่ครบ</span>');
                    tr.find('.col-action').html(`
                        <button class="btn btn-warning btn-sm" onclick="showModalUpdateLot('${row.receiveNo}')">
                            <i class="fas fa-folder"></i> ตรวจสอบ
                        </button>
                    `);
                } else {
                    tr.find('.col-status').html('<span class="badge badge-secondary">รอรับเข้า</span>');
                    tr.find('.col-action').html(`
                        <button class="btn btn-warning btn-sm" onclick="showModalUpdateLot('${row.receiveNo}')">
                            <i class="fas fa-folder"></i> ตรวจสอบ
                        </button>
                    `);
                }
            }
        }
    });
}

function ClearFindBy() {
    $("#txtFindReceivedNo").val("");

    findReceive()
}