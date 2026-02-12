// เก็บสถานะว่า tab ไหนโหลดข้อมูลแล้ว
let tabDataLoaded = {
    store: false,
    melt: false,
    lost: false,
    export: false
};

// ตั้ง event listeners
$(document).ready(function () {
    // Event listener สำหรับเปลี่ยน tab
    $(document).on('shown.bs.tab', '#cancelReceiveTabs button[data-bs-toggle="tab"]', function (e) {
        const targetId = $(e.target).attr('id');

        switch (targetId) {
            case 'tab-cancel-store':
                loadCancelStoreData();
                break;
            case 'tab-cancel-melt':
                loadCancelMeltData();
                break;
            case 'tab-cancel-lost':
                loadCancelLostData();
                break;
            case 'tab-cancel-export':
                loadCancelExportData();
                break;
        }
    });

    $(document).on('change', '#chkSelectAllCancel', function () {
        const isChecked = $(this).is(':checked');
        $('#tbl-cancel-detail-body .chk-row:enabled')
            .prop('checked', isChecked)
            .trigger('change');
    });
});

// ฟังก์ชันค้นหาใบส่ง
function FindReceiveToCancel() {
    const activeTab = $('#cancelReceiveTabs .nav-link.active').attr('id');

    switch (activeTab) {
        case 'tab-cancel-store':
            loadCancelStoreData();
            break;
        case 'tab-cancel-melt':
            loadCancelMeltData();
            break;
        case 'tab-cancel-lost':
            loadCancelLostData();
            break;
        case 'tab-cancel-export':
            loadCancelExportData();
            break;
    }
}

// ฟังก์ชัน Clear - เคลียร์ input และตาราง
function ClearFindByReceiveToCancel() {
    // เคลียร์ค่าใน input
    $('#txtFindReceivedNoToCancel').val('');
    $('#txtFindLotNoCancel').val('');

    // เคลียร์ตารางทุก tab
    $('#tbl-cancel-store-body').html('');
    $('#tbl-cancel-melt-body').html('');
    $('#tbl-cancel-lost-body').html('');
    $('#tbl-cancel-export-body').html('');

    // รีเซ็ตสถานะการโหลดข้อมูล
    tabDataLoaded = {
        store: false,
        melt: false,
        lost: false,
        export: false
    };

    // โหลดข้อมูลใหม่สำหรับ tab ที่ active
    const activeTab = $('#cancelReceiveTabs .nav-link.active').attr('id');
    switch (activeTab) {
        case 'tab-cancel-store':
            loadCancelStoreData();
            break;
        case 'tab-cancel-melt':
            loadCancelMeltData();
            break;
        case 'tab-cancel-lost':
            loadCancelLostData();
            break;
        case 'tab-cancel-export':
            loadCancelExportData();
            break;
    }
}

// โหลดข้อมูล ส่งเก็บ
function loadCancelStoreData() {
    const receiveNo = $('#txtFindReceivedNoToCancel').val();
    const lotNo = $('#txtFindLotNoCancel').val();
    const tbody = $('#tbl-cancel-store-body');

    tbody.html('<tr><td colspan="5" class="text-center text-muted">กำลังโหลดข้อมูล...</td></tr>');

    $.ajax({
        url: urlGetCancelStoreList,
        method: 'GET',
        data: { receiveNo: receiveNo, lotNo: lotNo },
        success: function (rows) {
            renderCancelTable(tbody, rows, 'store');
            tabDataLoaded.store = true;
        },
        error: function (xhr) {
            tbody.html(`<tr><td colspan="5" class="text-danger text-center">เกิดข้อผิดพลาด (${xhr.status} ${xhr.statusText})</td></tr>`);
        }
    });
}

// โหลดข้อมูล ส่งหลอม
function loadCancelMeltData() {
    const receiveNo = $('#txtFindReceivedNoToCancel').val();
    const lotNo = $('#txtFindLotNoCancel').val();
    const tbody = $('#tbl-cancel-melt-body');

    tbody.html('<tr><td colspan="5" class="text-center text-muted">กำลังโหลดข้อมูล...</td></tr>');

    $.ajax({
        url: urlGetCancelMeltList,
        method: 'GET',
        data: { receiveNo: receiveNo, lotNo: lotNo },
        success: function (rows) {
            renderCancelTable(tbody, rows, 'melt');
            tabDataLoaded.melt = true;
        },
        error: function (xhr) {
            tbody.html(`<tr><td colspan="5" class="text-danger text-center">เกิดข้อผิดพลาด (${xhr.status} ${xhr.statusText})</td></tr>`);
        }
    });
}

// โหลดข้อมูล ส่งหาย
function loadCancelLostData() {
    const receiveNo = $('#txtFindReceivedNoToCancel').val();
    const lotNo = $('#txtFindLotNoCancel').val();
    const tbody = $('#tbl-cancel-lost-body');

    tbody.html('<tr><td colspan="5" class="text-center text-muted">กำลังโหลดข้อมูล...</td></tr>');

    $.ajax({
        url: urlGetCancelLostList,
        method: 'GET',
        data: { receiveNo: receiveNo, lotNo: lotNo },
        success: function (rows) {
            renderCancelTable(tbody, rows, 'lost');
            tabDataLoaded.lost = true;
        },
        error: function (xhr) {
            tbody.html(`<tr><td colspan="5" class="text-danger text-center">เกิดข้อผิดพลาด (${xhr.status} ${xhr.statusText})</td></tr>`);
        }
    });
}

// โหลดข้อมูล ส่งออก
function loadCancelExportData() {
    const receiveNo = $('#txtFindReceivedNoToCancel').val();
    const lotNo = $('#txtFindLotNoCancel').val();
    const tbody = $('#tbl-cancel-export-body');

    tbody.html('<tr><td colspan="5" class="text-center text-muted">กำลังโหลดข้อมูล...</td></tr>');

    $.ajax({
        url: urlGetCancelExportList,
        method: 'GET',
        data: { receiveNo: receiveNo, lotNo: lotNo },
        success: function (rows) {
            renderCancelTable(tbody, rows, 'export');
            tabDataLoaded.export = true;
        },
        error: function (xhr) {
            tbody.html(`<tr><td colspan="5" class="text-danger text-center">เกิดข้อผิดพลาด (${xhr.status} ${xhr.statusText})</td></tr>`);
        }
    });
}

// ฟังก์ชัน render ตาราง
function renderCancelTable(tbody, rows, type) {
    if (!rows || rows.length === 0) {
        tbody.html('<tr><td colspan="5" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
        return;
    }

    const body = rows.map((r, index) => {
        let status = '';
        let canCancel = false; // ตรวจสอบว่าแสดงปุ่มยกเลิกได้หรือไม่

        switch (type) {
            case 'store':
                canCancel = !r.isReceived; // ยังไม่รับเข้า → ยกเลิกได้
                status = r.isReceived
                    ? '<i class="fas fa-check text-success" title="รับเข้าแล้ว"></i>'
                    : '<i class="fas fa-times text-danger" title="ยังไม่รับเข้า"></i>';
                break;
            case 'melt':
                canCancel = !r.isReceived; // ยังไม่หลอม → ยกเลิกได้
                status = r.isReceived
                    ? '<i class="fas fa-check text-success" title="หลอมแล้ว"></i>'
                    : '<i class="fas fa-times text-danger" title="ยังไม่หลอม"></i>';
                break;
            case 'lost':
                canCancel = true; // แจ้งหายสามารถยกเลิกได้เสมอ
                status = '<i class="fas fa-times text-danger" title="แจ้งหาย"></i>';
                break;
            case 'export':
                canCancel = !r.isExported; // ยังไม่ส่งออก → ยกเลิกได้
                status = r.isExported
                    ? '<i class="fas fa-check text-success" title="ส่งออกแล้ว"></i>'
                    : '<i class="fas fa-times text-danger" title="ยังไม่ส่งออก"></i>';
                break;
        }

        const cancelBtn = canCancel
            ? `<button class="btn btn-danger btn-sm" onclick="cancelReceive('${html(r.receiveNo)}', '${type}')">
                   <i class="fas fa-times"></i> ยกเลิกใบส่ง
               </button>`
            : '';

        const action = `
            <button class="btn btn-warning btn-sm" onclick="showCancelDetail('${html(r.receiveNo)}', '${type}')">
                <i class="fas fa-folder"></i> ดูรายการ
            </button>
            ${cancelBtn}`;

        return `
            <tr data-receive-no="${html(r.receiveNo)}">
                <td>${index + 1}</td>
                <td class="col-receiveno"><strong>${html(r.receiveNo)}</strong></td>
                <td class="col-mdate">${html(r.mdate)}</td>
                <td class="col-status">${status}</td>
                <td class="col-action">${action}</td>
            </tr>
        `;
    }).join('');

    tbody.html(body);
}

function showCancelDetail(receiveNo, type) {
    const txtFindLotNo = $('#txtFindLotNoCancel').val();

    const modal = $('#modal-cancel-detail');
    const tbody = modal.find('#tbl-cancel-detail-body');

    tbody.empty().append('<tr><td colspan="10" class="text-center text-muted">กำลังโหลด...</td></tr>');

    modal.find('#txtTitleCancelDetail').html(
        "<i class='fas fa-folder-open'></i> รายการใบส่ง: " + html(receiveNo)
    );

    modal.modal('show');

    let url = '';
    switch (type) {
        case 'store': url = urlGetCancelStoreDetail; break;
        case 'melt': url = urlGetCancelMeltDetail; break;
        case 'lost': url = urlGetCancelLostDetail; break;
        case 'export': url = urlGetCancelExportDetail; break;
    }

    $.ajax({
        url: url,
        method: 'GET',
        data: {
            receiveNo: receiveNo,
            lotNo: txtFindLotNo,
        },
        dataType: 'json',
    })
        .done(function (items) {
            tbody.empty();

            $('#hddCancelReceiveNo').val(receiveNo);

            if (!items || items.length === 0) {
                tbody.append('<tr><td colspan="10" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
                return;
            }

            const rows = items.map(function (x, i) {
                const safeId = ('chk_' + String(x.receiveNo ?? ('row' + i))).replace(/[^A-Za-z0-9_-]/g, '_');

                const isReceived = x.isReceived === true;
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
                            <input type="checkbox" id="${safeId}_${i}" class="chk-row custom-checkbox" ${disabledAttr}>
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

            // ซ่อน/แสดงปุ่มยกเลิกและ checkbox ทั้งหมดตามจำนวน item ที่เลือกได้
            if (allEnabled === 0) {
                $('#btnCancelSelected').hide();
                $('#chkSelectAllCancel').parent().hide();
            } else {
                $('#btnCancelSelected').show();
                $('#chkSelectAllCancel').parent().show();
            }

            $('#chkSelectAllCancel')
                .prop('checked', allEnabled > 0 && allChecked === allEnabled)
                .prop('indeterminate', allChecked > 0 && allChecked < allEnabled);

            tbody.on('change', '.chk-row', function () {
                calcTotal();

                const allEnabled = tbody.find('.chk-row:enabled').length;
                const allChecked = tbody.find('.chk-row:enabled:checked').length;

                $('#chkSelectAllCancel')
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

// ยกเลิกรายการที่เลือก (สำหรับ Lost/Export tab)
async function cancelSelectedItems() {
    const tbody = $('#tbl-cancel-detail-body');
    const receiveNo = $('#hddCancelReceiveNo').val();
    const activeTab = $('#cancelReceiveTabs .nav-link.active').attr('id');
    const lotNos = [];

    // เก็บ lotNo ที่ถูกเลือก (checked)
    tbody.find('tr').each(function () {
        const chk = $(this).find('.chk-row');
        if (chk.is(':checked') && !chk.is(':disabled')) {
            const lotNo = $(this).find('td:eq(3)').text().trim(); // คอลัมน์ LotNo
            if (lotNo && !lotNos.includes(lotNo)) {
                lotNos.push(lotNo);
            }
        }
    });

    if (lotNos.length === 0) {
        await swalWarning('กรุณาเลือกรายการที่จะยกเลิก');
        return;
    }

    // เลือก URL ตาม tab ที่ active
    let url = '';
    switch (activeTab) {
        case 'tab-cancel-store': url = urlCancelStoreByLotNo; break;
        case 'tab-cancel-melt': url = urlCancelMeltByLotNo; break;
        case 'tab-cancel-lost': url = urlCancelLostByLotNo; break;
        case 'tab-cancel-export': url = urlCancelExportByLotNo; break;
        default:
            await swalWarning('ยังไม่รองรับการยกเลิกสำหรับประเภทนี้');
            return;
    }

    await swalDeleteConfirm(
        `ต้องการยกเลิก ${lotNos.length} รายการ ใช่หรือไม่?`,
        'ยืนยันการยกเลิก?',
        async () => {
            $('#loadingIndicator').show();

            const formData = new FormData();
            formData.append("receiveNo", receiveNo);
            lotNos.forEach(no => formData.append("lotNos", no));

            $.ajax({
                url: url,
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: async function (res) {
                    $('#loadingIndicator').hide();
                    $('#modal-cancel-detail').modal('hide');

                    // รีโหลดข้อมูล tab ที่ active
                    switch (activeTab) {
                        case 'tab-cancel-store': loadCancelStoreData(); break;
                        case 'tab-cancel-melt': loadCancelMeltData(); break;
                        case 'tab-cancel-lost': loadCancelLostData(); break;
                        case 'tab-cancel-export': loadCancelExportData(); break;
                    }

                    swalToastSuccess(`ยกเลิกแล้ว ${lotNos.length} รายการ`);
                },
                error: async function (xhr) {
                    $('#loadingIndicator').hide();
                    let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                    await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
                }
            });
        }
    );
}

// ยกเลิกทั้งใบส่ง
async function cancelReceive(receiveNo, type) {
    // เลือก URL ตาม type
    let url = '';
    switch (type) {
        case 'store': url = urlCancelStoreByReceiveNo; break;
        case 'melt': url = urlCancelMeltByReceiveNo; break;
        case 'lost': url = urlCancelLostByReceiveNo; break;
        case 'export': url = urlCancelExportByReceiveNo; break;
        default:
            await swalWarning('ยังไม่รองรับการยกเลิกสำหรับประเภทนี้');
            return;
    }

    await swalDeleteConfirm(
        `ต้องการยกเลิกใบส่ง ${receiveNo} ทั้งหมดใช่หรือไม่?`,
        'ยืนยันการยกเลิกใบส่ง?',
        async () => {
            $('#loadingIndicator').show();

            const formData = new FormData();
            formData.append("receiveNo", receiveNo);

            $.ajax({
                url: url,
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: async function (res) {
                    $('#loadingIndicator').hide();

                    // รีโหลดข้อมูล tab ที่ active
                    switch (type) {
                        case 'lost': loadCancelLostData(); break;
                        case 'store': loadCancelStoreData(); break;
                        case 'melt': loadCancelMeltData(); break;
                        case 'export': loadCancelExportData(); break;
                    }

                    swalToastSuccess(`ยกเลิกใบส่ง ${receiveNo} เรียบร้อย`);
                },
                error: async function (xhr) {
                    $('#loadingIndicator').hide();
                    let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                    await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
                }
            });
        }
    );
}