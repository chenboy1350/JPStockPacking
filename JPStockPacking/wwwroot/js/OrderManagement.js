$(document).ready(function () {
    $(document).on('show.bs.collapse', '.accordion-collapse', function () {
        var accordionItem = $(this).closest('.accordion-item');

        var $newBadge = accordionItem.find('.badge.bg-danger.new');

        if ($newBadge.length) {
            $newBadge.remove();
        } else {
            return;
        }

        var orderNo = accordionItem.data('order-no');
        if (orderNo) {
            $.ajax({
                url: urlOrderMarkAsRead,
                method: 'GET',
                data: { orderNo: orderNo },
                error: function (xhr) {
                    console.error('Mark as read failed:', xhr.responseText);
                }
            });
        }
    });

    $(document).on('click', '#btnUpdateLot', function () {
        const lotNo = $('#hddUpdateLotNo').val();
        $('#loadingIndicator').show();

        const tbody = $('#tbl-received-body');
        const receivedNos = [];

        tbody.find('tr').each(function () {
            const chk = $(this).find('.chk-row');
            if (chk.is(':checked')) {
                const receivedNo = $(this).data('received-no');
                if (receivedNo) receivedNos.push(receivedNo);
            }
        });

        if (receivedNos.length === 0) {
            showWarning('กรุณาเลือกข้อมูลที่จะนำเข้า');
            $('#loadingIndicator').hide();
            return;
        }

        const formData = new FormData();
        formData.append("lotNo", lotNo);
        receivedNos.forEach(no => formData.append("receivedNo", no));

        $.ajax({
            url: urlUpdateLotItems,
            type: 'PATCH',
            processData: false,
            contentType: false,
            data: formData,
            success: function (lot) {
                $('#loadingIndicator').hide();
                CloseModal();
                showSuccess("อัปเดตสำเร็จ");
            },
            error: function (xhr) {
                $('#loadingIndicator').hide();
                showError(`เกิดข้อผิดพลาด (${xhr.status})`);
            }
        });
    });

    $(document).on("change", "#cbxTables", function () {
        const tableId = $(this).val();

        $.ajax({
            url: urlGetTableMember,
            type: 'GET',
            data: {
                tableID: tableId,
            },
            success: function (res) {
                let html = "";
                $.each(res, function (i, item) {
                    const cbxId = "checkboxTableMember" + item.id;
                    html += `
                    <tr data-member-id="${item.id}">
                        <td>${item.firstName} ${item.lastName} (${item.nickName})</td>
                        <td>
                            <div class="icheck-primary d-inline">
                                <input type="checkbox" id="${cbxId}" class="chk-row" value="${item.id}">
                                <label for="${cbxId}"></label>
                            </div>
                        </td>
                    </tr>
                    `;
                });

                $("#tblMembers").html(html);
            },
            error: function (xhr) {
                showError(`เกิดข้อผิดพลาด (${xhr.status})`);
            }
        });
    });

    $(document).on("click", "#btnConfirmAssign", function () {
        const lotNo = $('#hddAssignLotNo').val();
        const tableId = $("#cbxTables").val();

        const trevbody = $('#tbl-receivedToAssign-body');
        const receivedNos = [];

        trevbody.find('tr').each(function () {
            const chk = $(this).find('.chk-row');
            if (chk.is(':checked')) {
                const receivedNo = $(this).data('revtoassign-no');
                if (receivedNo) receivedNos.push(receivedNo);
            }
        });

        if (receivedNos.length === 0) {
            showWarning('กรุณาเลือกใบนำเข้าที่จะมอบหมาย');
            return;
        }

        const tmembody = $('#tblMembers');
        const memberIds = [];

        tmembody.find('tr').each(function () {
            const chk = $(this).find('.chk-row');
            if (chk.is(':checked')) {
                const memberId = $(this).data('member-id');
                if (memberId) memberIds.push(memberId);
            }
        });

        if (memberIds.length === 0) {
            showWarning('กรุณาเลือกสมาชิกที่จะมอบหมาย');
            return;
        }

        const formData = new FormData();
        formData.append("lotNo", lotNo);
        formData.append("tableId", tableId);
        receivedNos.forEach(no => formData.append("receivedNo", no));
        memberIds.forEach(no => formData.append("memberIds", no));

        $.ajax({
            url: urlAssignToTable,
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            beforeSend: () => $('#loadingIndicator').show(),
            success: (res) => {
                $('#loadingIndicator').hide();
                showSuccess("มอบหมายงานสำเร็จ");
            },
            error: (err) => {
                $('#loadingIndicator').hide();
                showError("เกิดข้อผิดพลาดในการมอบหมายงาน" + err);
            }
        });

    });


});

function fetchOrdersByDateRange() {
    const orderNo = $('#txtOrderNo').val();
    const custCode = $('#txtCustCode').val();
    const fdate = $('#fromDate').val();
    const edate = $('#toDate').val();
    const groupMode = $('input[name="groupMode"]:checked').val();

    $.ajax({
        url: urlGetOrder,
        type: 'GET',
        data: {
            orderNo: orderNo,
            custCode: custCode,
            fdate: fdate,
            edate: edate,
            groupMode: groupMode,
        },
        success: function (data) {
            renderOrderList(data);
        },
        error: function (xhr, status, error) {
            console.error('Error loading order data:', error);
        }
    });
}

function renderOrderList(data) {
    const container = $('#accordionOrder');
    container.empty();

    if (!data || !data.days) return;

    const todayStr = new Date().toISOString().split('T')[0];

    data.days.forEach(day => {
        day.orders.forEach(order => {
            const percent = order.customLot.reduce((acc, lot) => acc + (lot.ttQty > 0 ? (lot.receivedQty * 100 / lot.ttQty) : 0), 0) / order.customLot.length;

            const lotsHtml = order.customLot.map(lot => {
                const lotPercent = lot.ttQty > 0 ? Math.floor((lot.receivedQty * 100) / lot.ttQty) : 0;

                return `
                    <tr>
                        <td>#</td>
                        <td>
                            <a><strong>${lot.lotNo}</strong> ${lot.isUpdate ? "<span class='badge bg-warning update'>มีการนำส่ง</span>" : ""}</a><br />
                            <small>ปรับปรุงล่าสุด : ${formatDate(lot.updateDate)}</small>
                        </td>
                        <td>${lot.listNo}</td>
                        <td>-</td>
                        <td class="project_progress">
                            <div class="progress progress-sm">
                                <div class="progress-bar bg-green" role="progressbar" style="width: ${lotPercent}%" aria-valuenow="${lotPercent}" aria-valuemin="0" aria-valuemax="100"></div>
                            </div>
                            <small>${lot.receivedQty} / ${lot.ttQty} (${lotPercent}%)</small>
                        </td>
                        <td class="project-state">
                            <span class="badge badge-secondary">รอนำส่ง</span>
                        </td>
                        <td class="project-actions text-right">
                            ${lot.isUpdate ? `<button class='btn btn-warning btn-sm' onclick='showModalUpdateLot("${lot.lotNo}")'><i class='fas fa-folder-plus'></i> ตรวจสอบ</button>` : ""}
                        </td>
                    </tr>
                `;
            }).join('');

            const itemHtml = `
                <div class="accordion-item mt-2" data-order-no="${order.orderNo}">
                    <h2 class="accordion-header" id="heading${order.orderNo}">
                        <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapse${order.orderNo}" aria-expanded="false" aria-controls="collapse${order.orderNo}">
                            <div class="d-flex justify-content-between w-100">
                                <div class="col-md-3">
                                    <strong>
                                        ${order.custCode}/${order.orderNo}
                                        ${order.orderDate <= todayStr ? "<i class='fas fa-fire-alt' style='color: red;'></i>" : ""}
                                        ${order.isNew ? "<span class='badge bg-danger new'>ใหม่</span>" : ""}
                                        ${order.isUpdate ? "<span class='badge bg-warning'>มีการนำส่ง</span>" : ""}
                                    </strong>
                                </div>
                                <div class="col-md-3">
                                    <i class="fas fa-clipboard-check"></i> ${formatDate(order.orderDate)}
                                    <i class="fas fa-truck"></i> ${formatDate(order.seldDate1)}
                                </div>
                                <div class="col-md-2">
                                    <i class="fas fa-business-time"></i> ${formatDate(order.startDate)}
                                </div>
                                <div class="col-md-1">
                                    <i class="fas fa-boxes"></i> ${order.completeLot}/${order.totalLot}
                                </div>
                                <div class="col-md-1">
                                    <i class="fas fa-boxes"></i> ${order.sumTtQty$}
                                </div>
                                <div class="col-md-1">
                                    <i class="fas fa-box-open"></i> ${order.packDaysRemain} วัน
                                </div>
                                <div class="col-md-1">
                                    <i class="fas fa-truck-moving"></i> ${order.exportDaysRemain} วัน
                                </div>
                            </div>
                        </button>
                    </h2>
                    <div id="collapse${order.orderNo}" class="accordion-collapse collapse" aria-labelledby="heading${order.orderNo}" data-bs-parent="#accordionOrder">
                        <div class="accordion-body" style="overflow: auto;">
                            <table class="table table-striped projects">
                                <thead>
                                    <tr>
                                        <th style="width: 1%">#</th>
                                        <th style="width: 20%">หมายเลขล็อต</th>
                                        <th style="width: 10%">ลำดับที่</th>
                                        <th style="width: 25%">การมอบหมาย</th>
                                        <th>ความคืบหน้า</th>
                                        <th style="width: 8%" class="text-center">สถานะ</th>
                                        <th style="width: 20%"></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${lotsHtml}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            `;

            container.append(itemHtml);
        });
    });
}



function showModalUpdateLot(lotNo) {
    const modal = $('#modal-update');
    const tbody = modal.find('#tbl-received-body');

    tbody.empty().append('<tr><td colspan="9" class="text-center text-muted">กำลังโหลด...</td></tr>');

    modal.find('#txtTitleUpdate').html(
        "<i class='fas fa-folder-plus'></i> รายการนำเข้าของล็อต : " + html(lotNo)
    );

    modal.modal('show');

    $.ajax({
        url: urlGetReceived,
        method: 'GET',
        data: { lotNo: lotNo },
        dataType: 'json',
        cache: false
    })
    .done(function (items) {
        tbody.empty();

        $('#hddUpdateLotNo').val(lotNo);

        if (!items || items.length === 0) {
            tbody.append('<tr><td colspan="9" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
            return;
        }

        const rows = items.map(function (x, i) {
            const safeId = ('chk_' + String(x.receiveNo ?? ('row' + i))).replace(/[^A-Za-z0-9_-]/g, '_');

            return `
            <tr data-received-no="${html(x.receiveNo)}" data-ttqty="${numRaw(x.ttQty)}" data-ttwg="${numRaw(x.ttwg)}">
                <td><strong>${html(x.receiveNo)}</strong></td>
                <td>${html(x.lotNo)}</td>
                <td>${html(x.orderNo)}</td>
                <td class="text-center">${html(x.listNo)}</td>
                <td>${html(x.barcode)}</td>
                <td>${html(x.article)}</td>
                <td class="text-end">${num(x.ttQty)}</td>
                <td class="text-end">${num(x.ttwg)}</td>
                <td class="text-center">
                    <div class="icheck-primary d-inline">
                        <input type="checkbox" id="${safeId}_${i}" class="chk-row" checked>
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

        tbody.on('change', '.chk-row', function () {
            calcTotal();
        });

        modal.find('#txtTitleUpdate').html(
            "<i class='fas fa-folder-plus'></i> รายการนำเข้าล็อต : " + html(items[0].lotNo ?? lotNo)
        );
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

function showModalAssign(lotNo) {
    const modal = $('#modal-assign');
    const tbody = modal.find('#tbl-receivedToAssign-body');

    tbody.empty().append('<tr><td colspan="9" class="text-center text-muted">กำลังโหลด...</td></tr>');

    modal.find('#txtTitleAssign').html(
        "<i class='fas fa-folder-plus'></i> รายการมอบหมายงาน : " + html(lotNo)
    );

    modal.modal('show');

    $.ajax({
        url: urlGetReceivedToAssign,
        method: 'GET',
        data: { lotNo: lotNo },
        dataType: 'json',
        cache: false
    })
    .done(function (items) {
        tbody.empty();

        $('#hddAssignLotNo').val(lotNo);

        if (!items || items.length === 0) {
            tbody.append('<tr><td colspan="9" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
            return;
        }

        const rows = items.map(function (x, i) {
            const safeId = ('chk_' + String(x.receiveNo ?? ('row' + i))).replace(/[^A-Za-z0-9_-]/g, '_');

            return `
            <tr data-revtoassign-no="${html(x.receiveNo)}" data-ttqty="${numRaw(x.ttQty)}" data-ttwg="${numRaw(x.ttwg)}">
                <td><strong>${html(x.receiveNo)}</strong></td>
                <td>${html(x.lotNo)}</td>
                <td>${html(x.orderNo)}</td>
                <td class="text-center">${html(x.listNo)}</td>
                <td>${html(x.barcode)}</td>
                <td>${html(x.article)}</td>
                <td class="text-end">${num(x.ttQty)}</td>
                <td class="text-end">${num(x.ttwg)}</td>
                <td class="text-center">
                    <div class="icheck-primary d-inline">
                        <input type="checkbox" id="${safeId}_as${i}" class="chk-row" checked>
                        <label for="${safeId}_as${i}"></label>
                    </div>
                </td>
            </tr>`;
        }).join('');

        tbody.append(rows);

        tbody.append(`
        <tr class="table-secondary fw-bold" id="totalRow">
            <td colspan="6" class="text-end">รวม</td>
            <td class="text-end" id="sumASTtQty">0</td>
            <td class="text-end" id="sumASTtWg">0</td>
            <td></td>
        </tr>
    `);


        calcTotal();

        tbody.on('change', '.chk-row', function () {
            calcTotal();
        });

        modal.find('#txtTitleAssign').html(
            "<i class='fas fa-folder-plus'></i> รายการมอบหมายงาน : " + html(items[0].lotNo ?? lotNo)
        );
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
        $('#sumASTtQty').text(num(sumQty));
        $('#sumASTtWg').text(num(sumWg));
    }
}

function showModalReturn() {
    $('#modal-return').modal('show');
}

function showModalSendTo() {
    $('#modal-Sendto').modal('show');
}

function CloseModal() {
    $('.modal').modal('hide');
}