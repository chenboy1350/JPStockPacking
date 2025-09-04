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

    $(document).on('click', '#btnImportOrder', async function () {
        $('#loadingIndicator').show();
        const orderNo = $('#txtImportOrderNo').val();

        const formData = new FormData();
        formData.append("orderNo", orderNo);

        $.ajax({
            url: urlImportOrder,
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            success: async function () {
                $('#loadingIndicator').hide();
                await showSuccess("นำเข้าสำเร็จ");
                $("#txtImportOrderNo").val("");
                fetchOrdersByDateRange()
            },
            error: async function (xhr) {
                $('#loadingIndicator').hide();
                await showError(`เกิดข้อผิดพลาด (${xhr.status})`);
            }
        });
    });

    $(document).on('click', '#btnAllUpdateLot', async function () {
        const receiveNo = $('#hddUpdateReceiveNo').val();

        if (receiveNo == "" && receiveNo == null) {
            await showWarning('กรุณาเลือกข้อมูลที่จะนำเข้า');
            $('#loadingIndicator').hide();
            return;
        }

        const formData = new FormData();
        formData.append("receiveNo", receiveNo);

        $.ajax({
            url: urlUpdateLotByRevNoItems,
            type: 'PATCH',
            processData: false,
            contentType: false,
            data: formData,
            success: async function (lot) {
                $('#loadingIndicator').hide();
                await showSuccess("อัปเดตสำเร็จ");
                $("#txtImportReceivedNo").val("");
                fetchOrdersByDateRange()
                
            },
            error: async function (xhr) {
                $('#loadingIndicator').hide();
                await showError(`เกิดข้อผิดพลาด (${xhr.status})`);
            }
        });
    });

    $(document).on('click', '#btnUpdateLot', async function () {
        const lotNo = $('#hddUpdateLotNo').val();
        $('#loadingIndicator').show();

        const tbody = $('#tbl-received-body');
        const receivedIDs = [];

        tbody.find('tr').each(function () {
            const chk = $(this).find('.chk-row');
            if (chk.is(':checked')) {
                const receivedID = $(this).data('received-no');
                if (receivedID) receivedIDs.push(receivedID);
            }
        });

        if (receivedIDs.length === 0) {
            await showWarning('กรุณาเลือกข้อมูลที่จะนำเข้า');
            $('#loadingIndicator').hide();
            return;
        }

        const formData = new FormData();
        formData.append("lotNo", lotNo);
        receivedIDs.forEach(no => formData.append("receivedIDs", no));

        $.ajax({
            url: urlUpdateLotItems,
            type: 'PATCH',
            processData: false,
            contentType: false,
            data: formData,
            success: async function (lot) {
                $('#loadingIndicator').hide();
                await showSuccess("อัปเดตสำเร็จ");

                const tr = $(`tr[data-lot-no="${lot.lotNo}"]`);
                const table = tr.closest("table");
                const tbody = tr.closest("tbody");
                tr.remove();

                const orderNo = table.closest(".accordion-item").data("order-no");
                const res = await $.get(urlGetOrder, {
                    orderNo: orderNo,
                    custCode: '',
                    fdate: '',
                    edate: '',
                    groupMode: 'Day'
                });

                const order = res.days?.flatMap(day => day.orders)?.find(o => o.orderNo === orderNo);
                const updatedLot = order?.customLot?.find(l => l.lotNo === lot.lotNo);

                if (order && updatedLot) {
                    const newTrHtml = renderLotRow(order, updatedLot);
                    tbody.append(newTrHtml);
                }
            },
            error: async function (xhr) {
                $('#loadingIndicator').hide();
                await showError(`เกิดข้อผิดพลาด (${xhr.status})`);
            }
        });
    });

    $(document).on('click', '#btnImportReceivedNo', async function () {
        showModalUpdateAllLot()
    });

    $(document).on("change", "#cbxTables", async function () {
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
            error: async function (xhr) {
                await showError(`เกิดข้อผิดพลาด (${xhr.status})`);
            }
        });
    });

    $(document).on("click", "#btnConfirmAssign", async function () {
        const lotNo = $('#hddAssignLotNo').val();
        const tableId = $("#cbxTables").val();

        const trevbody = $('#tbl-receivedToAssign-body');
        const receivedIDs = [];

        trevbody.find('tr').each(function () {
            const chk = $(this).find('.chk-row');
            if (chk.is(':checked')) {
                const receivedID = $(this).data('revtoassign-no');
                if (receivedID) receivedIDs.push(receivedID);
            }
        });

        if (receivedIDs.length === 0) {
            await showWarning('กรุณาเลือกใบนำเข้าที่จะมอบหมาย');
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
            await showWarning('กรุณาเลือกสมาชิกที่จะมอบหมาย');
            return;
        }

        const formData = new FormData();
        formData.append("lotNo", lotNo);
        formData.append("tableId", tableId);
        receivedIDs.forEach(no => formData.append("receivedIDs", no));
        memberIds.forEach(no => formData.append("memberIds", no));

        $.ajax({
            url: urlAssignToTable,
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            beforeSend: () => $('#loadingIndicator').show(),
            success: async () => {
                $('#loadingIndicator').hide();
                await showSuccess("มอบหมายงานสำเร็จ");
                location.reload();
            },
            error: async (err) => {
                $('#loadingIndicator').hide();
                await showError("เกิดข้อผิดพลาดในการมอบหมายงาน" + err);
            }
        });
    });

    $(document).on("change", "#cbxTableToReturn", function () {
        const tableId = $(this).val();
        const lotNo = $("#hddReturnLotNo").val();
        const tbody = $("#tbl-receivedReturn-body");

        if (!tableId) { tbody.empty(); return; }

        tbody.html('<tr><td colspan="9" class="text-center text-muted">กำลังโหลด...</td></tr>');

        $.ajax({
            url: urlGetRecievedToReturn,
            type: 'GET',
            data: { lotNo: lotNo, tableId: tableId },
            success: function (items) {
                tbody.empty();
                if (!items || items.length === 0) {
                    tbody.append('<tr><td colspan="9" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
                    return;
                }
                const rows = items.map((x, i) => {
                    const safeId = ('chk_' + String(x.receivedID ?? ('row' + i))).replace(/[^A-Za-z0-9_-]/g, '_');
                    return `
                <tr data-assignment-id="${numRaw(x.assignmentID)}" data-received-id="${numRaw(x.receivedID)}" data-ttqty="${numRaw(x.ttQty)}" data-ttwg="${numRaw(x.ttWg)}">
                    <td><strong>${html(x.receiveNo)}</strong></td>
                    <td>${html(x.lotNo)}</td>
                    <td>${html(x.orderNo ?? '')}</td>
                    <td class="text-center">${html(x.listNo ?? '')}</td>
                    <td>${html(x.barcode ?? '')}</td>
                    <td>${html(x.article ?? '')}</td>
                    <td class="text-end">${num(x.ttQty)}</td>
                    <td class="text-end">${num(x.ttWg)}</td>
                    <td class="text-center">
                        <div class="icheck-primary d-inline">
                            <input type="checkbox" id="${safeId}_rt${i}" class="chk-row" checked>
                            <label for="${safeId}_rt${i}"></label>
                        </div>
                    </td>
                </tr>`;
                }).join('');

                tbody.append(rows);

                tbody.append(`
                  <tr class="table-secondary fw-bold">
                    <td colspan="6" class="text-end">รวม</td>
                    <td class="text-end" id="sumRTQty">0</td>
                    <td class="text-end" id="sumRTWg">0</td>
                    <td></td>
                  </tr>
                `);

                calcReturnTotal();

                tbody.on('change', '.chk-row', function () {
                    calcReturnTotal();
                });
            },
            error: function (xhr) {
                tbody.html(`<tr><td colspan="9" class="text-danger text-center">เกิดข้อผิดพลาด (${xhr.status})</td></tr>`);
            }
        });

        function calcReturnTotal() {
            let sumQty = 0;
            let sumWg = 0;

            tbody.find('tr').each(function () {
                const $tr = $(this);
                const $chk = $tr.find('.chk-row');
                if ($chk.length && $chk.is(':checked')) {
                    sumQty += Number($tr.data('ttqty')) || 0;
                    sumWg += Number($tr.data('ttwg')) || 0;
                }
            });

            $('#sumRTQty').text(num(sumQty));
            $('#sumRTWg').text(num(sumWg));

            $('#txtReurnQty').val(sumQty);

            calcTotalReturnQty();
        }
    });


    $(document).on('click', '#btnConfirmReturn', async function () {
        const lotNo = $('#hddReturnLotNo').val();

        const txtLostQty = $('#txtLostQty').val();
        const txtBreakQty = $('#txtBreakQty').val();
        const txtTotalReurnQty = $('#txtTotalReurnQty').val();

        const trebody = $('#tbl-receivedReturn-body');
        const assignmentIDs = [];

        trebody.find('tr').each(function () {
            const chk = $(this).find('.chk-row');
            if (chk.is(':checked')) {
                const assignmentID = $(this).data('assignment-id');
                if (assignmentID) assignmentIDs.push(assignmentID);
            }
        });

        if (assignmentIDs.length === 0) {
            await showWarning('กรุณาเลือกใบนำเข้าที่จะรับคืน');
            return;
        }

        const formData = new FormData();
        formData.append("lotNo", lotNo);
        formData.append("lostQty", txtLostQty);
        formData.append("breakQty", txtBreakQty);
        formData.append("returnQty", txtTotalReurnQty);
        assignmentIDs.forEach(no => formData.append("assignmentIDs", no));

        $.ajax({
            url: urlReturnAssignment,
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            beforeSend: async () => $('#loadingIndicator').show(),
            success: async () => {
                $('#loadingIndicator').hide();
                await showSuccess("คืนงานสำเร็จ");
                location.reload();
            },
            error: async (xhr) => {
                $('#loadingIndicator').hide();
                await showError("เกิดข้อผิดพลาดในการคืนงาน (" + xhr.status + ")");
            }
        });
    });

    $(document).on('click', '#btnLostAndRepair', async function () {
        const lotNo = $('#hddReturnLotNo').val();
        const txtReurnQty = $('#txtReurnQty').val();
        const txtLostQty = $('#txtLostQty').val();
        const txtBreakQty = $('#txtBreakQty').val();
        const txtTotalReurnQty = $('#txtTotalReurnQty').val();

        const trebody = $('#tbl-receivedReturn-body');
        const assignmentIDs = [];

        trebody.find('tr').each(function () {
            const chk = $(this).find('.chk-row');
            if (chk.is(':checked')) {
                const assignmentID = $(this).data('assignment-id');
                if (assignmentID) assignmentIDs.push(assignmentID);
            }
        });

        if (assignmentIDs.length === 0) {
            await showWarning('กรุณาเลือกใบนำเข้าที่จะรับคืน');
            return;
        }

        if (txtBreakQty > txtReurnQty) {
            await showWarning('จำนวนสินค้าที่ชำรุดมากกว่าจำนวนสินค้าที่รับคืน');
            return;
        }

        const formData = new FormData();
        formData.append("lotNo", lotNo);
        formData.append("lostQty", txtLostQty);
        formData.append("breakQty", txtBreakQty);
        formData.append("returnQty", txtTotalReurnQty);
        assignmentIDs.forEach(no => formData.append("assignmentIDs", no));

        $.ajax({
            url: urlLostAndRepair,
            type: 'PATCH',
            processData: false,
            contentType: false,
            data: formData,
            beforeSend: async () => $('#loadingIndicator').show(),
            success: async () => {
                $('#loadingIndicator').hide();
                await showSuccess("คืนงานสำเร็จ");
                location.reload();
            },
            error: async (xhr) => {
                $('#loadingIndicator').hide();
                await showError("เกิดข้อผิดพลาดในการคืนงาน (" + xhr.status + ")");
            }
        });
    });

    $(document).on("change", "#txtBreakQty, #txtLostQty", function () {
        let total = 0;
        $("#txtBreakQty, #txtLostQty, #txtOtherQty").each(function () {
            total += parseInt($(this).val()) || 0;
        });

        if (total > 0) {
            $("#btnConfirmReturn").hide();
            $("#btnLostAndRepair").show();
        } else {
            $("#btnConfirmReturn").show();
            $("#btnLostAndRepair").hide();
        }
    });

    function calcTotalReturnQty() {
        const lostQty = parseFloat($("#txtLostQty").val()) || 0;
        const breakQty = parseFloat($("#txtBreakQty").val()) || 0;
        const reurnQty = parseFloat($("#txtReurnQty").val()) || 0;

        const totalReturn = reurnQty - (breakQty + lostQty);

        $("#txtTotalReurnQty").val(totalReturn);
    }

    $(document).on("input", "#txtLostQty, #txtBreakQty, #txtReurnQty", function () {
        calcTotalReturnQty();
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

    const today = new Date();

    data.days.forEach(day => {
        day.orders.forEach(order => {
            const lotsHtml = order.customLot.map(lot => {
                const percent = lot.ttQty > 0 ? (lot.receivedQty * 100 / lot.ttQty) : 0;
                const packPercent = lot.ttQty > 0 ? (lot.packedQty * 100 / lot.ttQty) : 0;

                let progressHtml = '';
                if (!lot.isAllReceived) {
                    progressHtml = `<div class='progress-bar bg-orange' role='progressbar' style='width: ${percent}%' aria-valuenow='${percent}'></div>`;
                } else if (lot.isAllReceived && !lot.isPacking) {
                    progressHtml = `<div class='progress-bar bg-light-orange' role='progressbar' style='width: ${percent}%' aria-valuenow='${percent}'></div>`;
                } else if (lot.isPacking && !lot.isAllPacking) {
                    progressHtml = `<div class='progress-bar bg-green' role='progressbar' style='width: ${packPercent}%' aria-valuenow='${packPercent}'></div>`;
                } else if (lot.isAllPacking) {
                    progressHtml = `<div class='progress-bar bg-info' role='progressbar' style='width: 100%' aria-valuenow='100'></div>`;
                }

                let progressText = '';
                if (!lot.isAllReceived) {
                    progressText = `${lot.receivedQty} / ${lot.ttQty}`;
                } else if (lot.isAllReceived && !lot.isPacking) {
                    progressText = `${lot.receivedQty} / ${lot.ttQty}`;
                } else if (lot.isPacking && !lot.isAllPacking) {
                    progressText = `${lot.packedQty} / ${lot.ttQty}`;
                } else if (lot.isAllPacking) {
                    progressText = `${lot.ttQty} / ${lot.ttQty}`;
                }

                let statusHtml = '';
                if (lot.receivedQty < lot.ttQty) {
                    statusHtml += `<span class="badge badge-orange">รอนำส่ง</span>`;
                }
                if (lot.ttQty > 0 && lot.receivedQty >= lot.ttQty) {
                    statusHtml += `<span class="badge badge-orange">รับครบแล้ว</span>`;
                }
                if (lot.isAllReceived && !lot.isPacking) {
                    statusHtml += `<span class="badge badge-light-orange">รอจ่ายงาน</span>`;
                }
                if (lot.isPacking && !lot.isAllPacking) {
                    statusHtml += `<span class="badge badge-warning">กำลังบรรจุ</span>`;
                }
                if (lot.isAllPacking) {
                    statusHtml += `<span class="badge badge-info">บรรจุครบแล้ว</span>`;
                }

                let actionsHtml = '';
                if (lot.isUpdate) {
                    actionsHtml += `<button class='btn btn-warning btn-sm' onclick='showModalUpdateLot("${lot.lotNo}")'><i class='fas fa-folder-plus'></i> ตรวจสอบ</button>`;
                }
                if ((order.isReceivedLate || lot.isAllReceived) && !lot.isAllPacking && lot.ttQty != 0) {
                    actionsHtml += `<button class='btn btn-primary btn-sm' onclick='showModalAssign("${lot.lotNo}")'><i class='fas fa-folder'></i> จ่ายงาน</button>`;
                }
                if (lot.isPacking && !lot.isAllPacking) {
                    actionsHtml += `<button class='btn btn-danger btn-sm' onclick='showModalReturn("${lot.lotNo}")'><i class='fas fa-folder'></i> รับคืน</button>`;
                }

                return `
                    <tr data-lot-no="${lot.lotNo}">
                        <td>#</td>
                        <td>
                            <a><strong>${lot.lotNo}</strong>
                            ${order.isReceivedLate ? "<i class='fas fa-fire-alt' style='color: #e85700;'></i>" : ""}
                            ${order.isPackingLate ? "<i class='fas fa-fire-alt' style='color: red;'></i>" : ""}
                            ${lot.isUpdate ? "<span class='badge bg-warning update'>มีการนำส่ง</span>" : ""}
                            </a><br/>
                            <small>ปรับปรุงล่าสุด : ${lot.updateDate}</small>
                        </td>
                        <td>${lot.listNo}</td>
                        <td>${lot.assignTo || '-'}</td>
                        <td class="project_progress">
                            <div class="progress progress-sm">${progressHtml}</div>
                            <small>${progressText}</small>
                        </td>
                        <td class="project-state">${statusHtml}</td>
                        <td class="project-actions text-right">${actionsHtml}</td>
                    </tr>`;
            }).join('');

            const itemHtml = `
            <div class="accordion-item mt-2" data-order-no="${order.orderNo}">
                <h2 class="accordion-header" id="heading${order.orderNo}">
                    <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapse${order.orderNo}" aria-expanded="false" aria-controls="collapse${order.orderNo}">
                        <div class="d-flex justify-content-between w-100">
                            <div class="col-md-3">
                                <strong>
                                    ${order.custCode}/${order.orderNo}
                                    ${order.isReceivedLate ? "<i class='fas fa-fire-alt' style='color: #e85700;'></i>" : ""}
                                    ${order.isPackingLate ? "<i class='fas fa-fire-alt' style='color: red;'></i>" : ""}
                                    ${order.isNew ? "<span class='badge bg-danger new'>ใหม่</span>" : ""}
                                    ${order.isUpdate ? "<span class='badge bg-warning'>มีการนำส่ง</span>" : ""}
                                </strong>
                            </div>
                            <div class="col-md-3">
                                <i class="fas fa-clipboard-check"></i> ${order.orderDate}
                                <i class="fas fa-box-open"></i> ${order.seldDate1}
                            </div>
                            <div class="col-md-2">
                                <i class="fas fa-business-time"></i> ${order.startDateTH}
                            </div>
                            <div class="col-md-1">
                                <i class="fas fa-boxes"></i> ${order.completeLot}/${order.totalLot}
                            </div>
                            <div class="col-md-1">
                                <i class="fas fa-boxes"></i> ${order.sumTtQty}
                            </div>
                            <div class="col-md-1">
                                <i class="fas fa-clipboard-check"></i> ${order.packDaysRemain} วัน
                            </div>
                            <div class="col-md-1">
                                <i class="fas fa-box-open"></i> ${order.exportDaysRemain} วัน
                            </div>
                        </div>
                    </button>
                </h2>
                <div id="collapse${order.orderNo}" class="accordion-collapse collapse" aria-labelledby="heading${order.orderNo}" data-bs-parent="#accordionOrder">
                    <div class="accordion-body" style="overflow: auto;">
                        <table class="table table-striped projects">
                            <thead>
                                <tr>
                                    <th>#</th>
                                    <th>หมายเลขล็อต</th>
                                    <th>ลำดับที่</th>
                                    <th>การมอบหมาย</th>
                                    <th>ความคืบหน้า</th>
                                    <th class="text-center">สถานะ</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>${lotsHtml}</tbody>
                        </table>
                    </div>
                </div>
            </div>`;

            container.append(itemHtml);
        });
    });
}

function renderLotRow(order, lot) {
    const percent = lot.ttQty > 0 ? (lot.receivedQty * 100 / lot.ttQty) : 0;
    const packPercent = lot.ttQty > 0 ? (lot.packedQty * 100 / lot.ttQty) : 0;

    let progressHtml = '';
    if (!lot.isAllReceived) {
        progressHtml = `<div class='progress-bar bg-orange' role='progressbar' style='width: ${percent}%' aria-valuenow='${percent}'></div>`;
    } else if (lot.isAllReceived && !lot.isPacking) {
        progressHtml = `<div class='progress-bar bg-light-orange' role='progressbar' style='width: ${percent}%' aria-valuenow='${percent}'></div>`;
    } else if (lot.isPacking && !lot.isAllPacking) {
        progressHtml = `<div class='progress-bar bg-green' role='progressbar' style='width: ${packPercent}%' aria-valuenow='${packPercent}'></div>`;
    } else if (lot.isAllPacking) {
        progressHtml = `<div class='progress-bar bg-info' role='progressbar' style='width: 100%' aria-valuenow='100'></div>`;
    }

    let progressText = '';
    if (!lot.isAllReceived) {
        progressText = `${lot.receivedQty} / ${lot.ttQty}`;
    } else if (lot.isAllReceived && !lot.isPacking) {
        progressText = `${lot.receivedQty} / ${lot.ttQty}`;
    } else if (lot.isPacking && !lot.isAllPacking) {
        progressText = `${lot.packedQty} / ${lot.ttQty}`;
    } else if (lot.isAllPacking) {
        progressText = `${lot.ttQty} / ${lot.ttQty}`;
    }

    let statusHtml = '';
    if (lot.receivedQty < lot.ttQty) statusHtml += `<span class="badge badge-orange">รอนำส่ง</span>`;
    if (lot.ttQty > 0 && lot.receivedQty >= lot.ttQty) statusHtml += `<span class="badge badge-orange">รับครบแล้ว</span>`;
    if (lot.isAllReceived && !lot.isPacking) statusHtml += `<span class="badge badge-light-orange">รอจ่ายงาน</span>`;
    if (lot.isPacking && !lot.isAllPacking) statusHtml += `<span class="badge badge-warning">กำลังบรรจุ</span>`;
    if (lot.isAllPacking) statusHtml += `<span class="badge badge-info">บรรจุครบแล้ว</span>`;

    let actionsHtml = '';
    if (lot.isUpdate) actionsHtml += `<button class='btn btn-warning btn-sm' onclick='showModalUpdateLot("${lot.lotNo}")'><i class='fas fa-folder-plus'></i> ตรวจสอบ</button>`;
    if ((order.isReceivedLate || lot.isAllReceived) && !lot.isAllPacking && lot.ttQty != 0) actionsHtml += `<button class='btn btn-primary btn-sm' onclick='showModalAssign("${lot.lotNo}")'><i class='fas fa-folder'></i> จ่ายงาน</button>`;
    if (lot.isPacking && !lot.isAllPacking) actionsHtml += `<button class='btn btn-danger btn-sm' onclick='showModalReturn("${lot.lotNo}")'><i class='fas fa-folder'></i> รับคืน</button>`;

    return `
        <tr data-lot-no="${lot.lotNo}">
            <td>#</td>
            <td>
                <a><strong>${lot.lotNo}</strong>
                ${order.isReceivedLate ? "<i class='fas fa-fire-alt' style='color: #e85700;'></i>" : ""}
                ${order.isPackingLate ? "<i class='fas fa-fire-alt' style='color: red;'></i>" : ""}
                ${lot.isUpdate ? "<span class='badge bg-warning update'>มีการนำส่ง</span>" : ""}
                </a><br/>
                <small>ปรับปรุงล่าสุด : ${lot.updateDate ?? "N/A"}</small>
            </td>
            <td>${lot.listNo}</td>
            <td>${lot.assignTo || '-'}</td>
            <td class="project_progress">
                <div class="progress progress-sm">${progressHtml}</div>
                <small>${progressText}</small>
            </td>
            <td class="project-state">${statusHtml}</td>
            <td class="project-actions text-right">${actionsHtml}</td>
        </tr>`;
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
            <tr data-received-no="${html(x.receivedID)}" data-ttqty="${numRaw(x.ttQty)}" data-ttwg="${numRaw(x.ttWg)}">
                <td><strong>${html(x.receiveNo)}</strong></td>
                <td>${html(x.lotNo)}</td>
                <td>${html(x.orderNo)}</td>
                <td class="text-center">${html(x.listNo)}</td>
                <td>${html(x.barcode)}</td>
                <td>${html(x.article)}</td>
                <td class="text-end">${num(x.ttQty)}</td>
                <td class="text-end">${num(x.ttWg)}</td>
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

async function showModalUpdateAllLot() {
    const receiveNo = $('#txtImportReceivedNo').val().trim();
    const modal = $('#modal-update-all');
    const tbody = modal.find('#tbl-all-received');

    if (!receiveNo) {
        showWarning("กรุณาระบุเลขที่ใบนำเข้า (ReceiveNo)");
        return;
    }

    tbody.empty().append('<tr><td colspan="9" class="text-center text-muted">กำลังโหลด...</td></tr>');

    modal.find('#txtTitleUpdateAll').html(
        "<i class='fas fa-folder-plus'></i> เลขที่ใบนำเข้า : " + html(receiveNo)
    );

    $.ajax({
        url: urlImportReceiveNo,
        method: 'GET',
        data: { receiveNo },
        dataType: 'json',
        cache: false
    })
    .done(async function (items) {
        tbody.empty();

        $('#hddUpdateReceiveNo').val(receiveNo);

        if (!items || items.length === 0) {
            tbody.append('<tr><td colspan="9" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
            return;
        }

        const allReceived = items.every(x => x.isReceived === true);
        if (allReceived) {
            await showInfo("รายการนี้รับเข้าทั้งหมดแล้ว");
            tbody.append('<tr><td colspan="9" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
            return;
        }

        modal.modal('show');

        const rows = items.map(function (x, i) {
            const safeId = `chk_${x.lotNo}_${i}`.replace(/[^A-Za-z0-9_-]/g, '_');
            const rowClass = x.isReceived ? 'text-decoration-line-through text-muted' : '';
            const checkedAttr = x.isReceived ? '' : 'checked';
            const disabledAttr = x.isReceived ? 'disabled' : '';

            return `
            <tr class="${rowClass}" data-lot-no="${html(x.lotNo)}" data-ttqty="${numRaw(x.ttQty)}" data-ttwg="${numRaw(x.ttWg)}">
                <td><strong>${html(x.receiveNo)}</strong></td>
                <td>${html(x.lotNo)}</td>
                <td>${html(x.orderNo)}</td>
                <td class="text-center">${html(x.listNo)}</td>
                <td>${html(x.barcode)}</td>
                <td>${html(x.article)}</td>
                <td class="text-end">${num(x.ttQty)}</td>
                <td class="text-end">${num(x.ttWg)}</td>
                <td class="text-center">
                    <div class="icheck-primary d-inline">
                        <input type="checkbox" id="${safeId}" class="chk-row" ${checkedAttr} ${disabledAttr}>
                        <label for="${safeId}"></label>
                    </div>
                </td>
            </tr>`;
        }).join('');

        tbody.append(rows);
    })
    .fail(function (xhr) {
        tbody.empty().append(
            `<tr><td colspan="10" class="text-danger text-center">
            เกิดข้อผิดพลาดในการโหลดข้อมูล (${xhr.status} ${xhr.statusText})
        </td></tr>`
        );
    });
}


function showModalAssign(lotNo) {
    clearModalAssignValues()
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
            const safeId = ('chk_' + String(x.receivedID ?? ('row' + i))).replace(/[^A-Za-z0-9_-]/g, '_');

            return `
            <tr data-revtoassign-no="${html(x.receivedID)}" data-ttqty="${numRaw(x.ttQty)}" data-ttwg="${numRaw(x.ttWg)}">
                <td><strong>${html(x.receiveNo)}</strong></td>
                <td>${html(x.lotNo)}</td>
                <td>${html(x.orderNo)}</td>
                <td class="text-center">${html(x.listNo)}</td>
                <td>${html(x.barcode)}</td>
                <td>${html(x.article)}</td>
                <td class="text-end">${num(x.ttQty)}</td>
                <td class="text-end">${num(x.ttWg)}</td>
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

async function showModalReturn(lotNo) {
    clearModalReturnValues()
    const modal = $('#modal-return');
    const select = modal.find('#cbxTableToReturn');

    modal.find('#txtTitleReturn').html(
        "<i class='fas fa-undo'></i> รายการรับงานคืน : " + html(lotNo)
    );
    $('#hddReturnLotNo').val(lotNo);

    select.empty().append('<option value="">กำลังโหลด...</option>').prop('disabled', true);

    modal.modal('show');

    $.ajax({
        url: urlGetTableToReturn,
        type: 'GET',
        data: { lotNo: lotNo },
        success: function (tables) {
            select.empty();
            if (!tables || tables.length === 0) {
                select.append('<option value="">ไม่พบโต๊ะ</option>');
                return;
            }
            select.append('<option value="">-- เลือกโต๊ะ --</option>');
            $.each(tables, function (i, rev) {
                select.append(`<option value="${rev.id}">${html(rev.name)}</option>`);
            });
        },
        error: async function (xhr) {
            select.empty().append('<option value="">โหลดข้อมูลไม่สำเร็จ</option>');
            showError(`เกิดข้อผิดพลาด (${xhr.status})`);
        },
        complete: function () {
            select.prop('disabled', false);
        }
    });
}

function clearModalAssignValues() {
    $("#cbxTables").val("");
    $("#tblMembers").empty();
}

function clearModalReturnValues() {
    $("#cbxTableToReturn").val("");
    $("#tbl-receivedReturn-body").empty();
    $("#txtReurnQty").val(0);
    $("#txtLostQty").val(0);
    $("#txtBreakQty").val(0);
    $("#txtTotalReurnQty").val(0);
}

function showModalSendTo() {
    $('#modal-Sendto').modal('show');
}

function CloseModal() {
    $('.modal').modal('hide');
}

function ClearFindBy() {
    $("#txtImportOrderNo").val("");
    $('#txtImportReceivedNo').val("")
    $("#fromDate").val("");
    $("#toDate").val("");
    $("#txtOrderNo").val("");
    $("#txtCustCode").val("");
}