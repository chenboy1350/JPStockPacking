let currentPage = 1;
let currentPageSize = 10;
let totalItems = 0;

$(document).ready(function () {
    $(document).on('keydown', '#txtOrderNo, #txtCustCode', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            fetchOrdersByDateRange();
        }
    });

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

    $(document).on('change', '#pageSize', function () {
        currentPageSize = parseInt($(this).val());
        currentPage = 1;
        fetchOrdersByDateRange();
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
                $("#chxHasPartTime").prop('disabled', false)
            },
            error: async function (xhr) {
                await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${xhr.statusText})`);
            }
        });
    });

    $(document).on("click", "#btnConfirmAssign", async function () {
        const lotNo = $('#hddAssignLotNo').val();
        const tableId = $("#cbxTables").val();

        const txtWorkerNumber = $("#txtWorkerNumber").val();
        const chxHasPartTime = $("#chxHasPartTime").is(":checked");

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
        formData.append("workerNumber", txtWorkerNumber);
        formData.append("hasPartTime", chxHasPartTime);
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
                await updateLotRow(lotNo);
                CloseModal();
            },
            error: async (err) => {
                $('#loadingIndicator').hide();
                await showWarning("เกิดข้อผิดพลาดในการมอบหมายงาน" + err);
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
                tbody.html(`<tr><td colspan="9" class="text-danger text-center">เกิดข้อผิดพลาด (${xhr.status} ${xhr.statusText})</td></tr>`);
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
        }
    });

    $(document).on('click', '#btnConfirmReturn', async function () {
        const lotNo = $('#hddReturnLotNo').val();

        const txtReurnQty = $('#txtReurnQty').val();

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
        formData.append("returnQty", txtReurnQty);
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
                await updateLotRow(lotNo);
                CloseModal();
            },
            error: async (xhr) => {
                $('#loadingIndicator').hide();
                await showWarning(`เกิดข้อผิดพลาดในการคืนงาน (${xhr.status} ${xhr.statusText})`);
            }
        });
    });

    $(document).on('change', '#chkSelectAllMembers', function () {
        const isChecked = $(this).is(':checked');
        $('#tblMembers .chk-row:enabled').prop('checked', isChecked);
    });

    $(document).on("click", "#btnAddBreakDes", async function () {
        let txtAddBreakDes = $('#txtAddBreakDes').val();
        if (txtAddBreakDes == '') {
            $('#txtAddBreakDes').show(); 
        } else {
            await showSaveConfirm(
                `ต้องการเพิ่มอาการ "${txtAddBreakDes}" ใช่หรือไม่`, "ยืนยันการเพิ่มอาการใหม่", async () => {
                    const formData = new FormData();
                    formData.append("breakDescription", txtAddBreakDes);

                    $.ajax({
                        url: urlAddNewBreakDescription,
                        type: 'POST',
                        processData: false,
                        contentType: false,
                        data: formData,
                        success: async (res) => {
                            $('#loadingIndicator').hide();
                            $('#txtAddBreakDes').val('').hide();

                            $('#ddlBreakDes').empty();
                            $('#ddlBreakDes').append(new Option('-- เลือกอาการ --', ''));
                            res.forEach(item => {
                                $('#ddlBreakDes').append(new Option(item.name, item.breakDescriptionId));
                            });
                            $('#ddlBreakDes').val(res[res.length - 1].breakDescriptionId);

                            await showSuccess("เพิ่มอาการเรียบร้อย");
                        },
                        error: async (xhr) => {
                            $('#loadingIndicator').hide();
                            await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${xhr.statusText})`);
                        }
                    });
                }
            );
        }
    });

    $(document).on("change", "#chxHasPartTime", async function () {
        if ($(this).is(":checked")) {
            $('#txtWorkerNumber').prop('disabled', false)
        } else {
            $('#txtWorkerNumber').prop('disabled', true)
            $('#txtWorkerNumber').val(0)
        }
    });

    $(document).on('click', '#btnValidateApprover', function () {
        const txtUsername = $('#txtUsername');
        const txtPassword = $('#txtPassword');
        const btn = $(this);

        if ((txtUsername.val() == '' || txtUsername.val() == null) || (txtPassword.val() == '' || txtPassword.val() == null)) {
            txtUsername.removeClass('is-invalid is-valid').addClass('is-warning').prop('disabled', false);
            txtPassword.removeClass('is-invalid is-valid').addClass('is-warning').prop('disabled', false);
            btn.removeClass('btn-primary btn-danger btn-success').addClass('btn-warning').text(`กรุณากรอกชื่อผู้ใช้และรหัสให้ครบถ้วน`).prop('disabled', false);
            setTimeout(() => {
                btn.text('ตรวจสอบสิทธิ').removeClass('btn-danger btn-success btn-warning').addClass('btn-primary').prop('disabled', false);
            }, 2000);
            return;
        }

        const formData = new FormData();
        formData.append('username', txtUsername.val());
        formData.append('password', txtPassword.val());

        $.ajax({
            url: urlCheckUser,
            method: 'POST',
            contentType: false,
            processData: false,
            data: formData,
            success: async function (res) {
                if (res) {
                    txtUsername.removeClass('is-invalid is-warning').addClass('is-valid').prop('disabled', true);
                    txtPassword.removeClass('is-invalid is-warning').addClass('is-valid').prop('disabled', true);
                    btn.removeClass('btn-primary btn-danger btn-warning').addClass('btn-success').text(`ยืนยันการพิมพ์โดย ${res.username}`).prop('disabled', true);
                    $('#btnPrintReport').prop('disabled', false)

                    $('#hddUserID').val(res.username)
                    $('#btnPrintReport').focus();
                }
                else {
                    txtUsername.removeClass('is-valid is-warning').addClass('is-invalid').prop('disabled', false);
                    txtPassword.removeClass('is-valid is-warning').addClass('is-invalid').prop('disabled', false);
                    btn.removeClass('btn-primary btn-success btn-warning').addClass('btn-danger').text(`ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง กรุณาลองใหม่อีกครั้ง`).prop('disabled', false);

                    setTimeout(() => {
                        btn.text('ตรวจสอบสิทธิ').removeClass('btn-danger btn-success btn-warning').addClass('btn-primary').prop('disabled', false);
                    }, 2000);
                }
            },
            error: async function (xhr) {
                txtUsername.removeClass('is-valid is-warning').addClass('is-invalid').prop('disabled', false);
                txtPassword.removeClass('is-valid is-warning').addClass('is-invalid').prop('disabled', false);
                btn.removeClass('btn-primary btn-success btn-warning').addClass('btn-danger').text(`ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง กรุณาลองใหม่อีกครั้ง`).prop('disabled', false);
                $('#btnPrintReport').prop('disabled', true)

                setTimeout(() => {
                    btn.text('ตรวจสอบสิทธิ').removeClass('btn-danger btn-success btn-warning').addClass('btn-primary').prop('disabled', false);
                }, 2000);
            }
        });
    });

    $(document).on('keydown', '#txtUsername', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnValidateApprover').click();
        }
    });

    $(document).on('keydown', '#txtPassword', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnValidateApprover').click();
        }
    });

    $(document).on('click', '#btnPrintReport', async function () {
        printLostToPDF();
    });

    $(document).on('click', '#btnAddLost', async function () {
        const lostQty = $('#txtLostQty').val();
        const lotNo = $('#hddLotNo').val();

        if (lostQty == '' || lostQty == 0)
        {
            await showWarning('กรุณากรอกจำนวน Lost');
            return;
        }
        await showSaveConfirm(
            `ต้องการเพิ่ม Lost จำนวน ${lostQty} ชิ้น ใช่หรือไม่`, "ยืนยันการเพิ่ม Lost", async () => {
                const formData = new FormData();
                formData.append("lotNo", lotNo);
                formData.append("lostQty", lostQty);
                $.ajax({
                    url: urlAddLost,
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    beforeSend: () => $('#loadingIndicator').show(),
                    success: async () => {
                        $('#loadingIndicator').hide();
                        await showSuccess("เพิ่ม Lost เรียบร้อย");
                        await updateLotRow(lotNo);
                        CloseModal();
                    },
                    error: async (xhr) => {
                        $('#loadingIndicator').hide();
                        await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${xhr.responseText})`);
                    }
                });
            }
        );
        
    });

    $(document).on('click', '#btnAddBreak', async function () {
        const breakQty = $('#txtBreakQty').val();
        const breakDes = $('#ddlBreakDes').val();
        const lotNo = $('#hddLotNo').val();

        if (breakQty == '' || breakQty == 0) {
            await showWarning('กรุณากรอกจำนวน break');
            return;
        }

        if (breakDes == '' || breakDes == 0) {
            await showWarning('กรุณาเลือกอาการ');
            return;
        }

        await showSaveConfirm(
            `ต้องการเพิ่ม break จำนวน ${breakQty} ชิ้น ใช่หรือไม่`, "ยืนยันการเพิ่ม break", async () => {
                const formData = new FormData();
                formData.append("lotNo", lotNo);
                formData.append("breakQty", breakQty);
                formData.append("breakDes", breakDes);
                $.ajax({
                    url: urlAddBreak,
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    beforeSend: () => $('#loadingIndicator').show(),
                    success: async () => {
                        $('#loadingIndicator').hide();
                        await showSuccess("เพิ่ม break เรียบร้อย");
                        await updateLotRow(lotNo);
                        CloseModal();
                    },
                    error: async (xhr) => {
                        $('#loadingIndicator').hide();
                        await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${xhr.responseText})`);
                    }
                });
            }
        );
    });

    $(document).on('change', '#chkSelectAllLost', function () {
        const isChecked = $(this).is(':checked');
        $('#tbl-body-lost .chk-row:enabled').prop('checked', isChecked);
    });

    $(document).on('change', '#chkSelectAllBreak', function () {
        const isChecked = $(this).is(':checked');
        $('#tbl-body-break .chk-row:enabled').prop('checked', isChecked);
    });
});

async function fetchOrdersByDateRange() {
    const orderNo = $('#txtOrderNo').val();
    const lotNo = $('#txtLotNo').val();
    const custCode = $('#txtCustCode').val();
    const fdate = $('#fromDate').val();
    const edate = $('#toDate').val();
    const groupMode = $('input[name="groupMode"]:checked').val() || 0;

    $.ajax({
        url: urlGetOrder,
        type: 'GET',
        data: {
            orderNo: orderNo,
            lotNo: lotNo,
            custCode: custCode,
            fdate: fdate,
            edate: edate,
            groupMode: groupMode,
            page: currentPage,
            pageSize: currentPageSize
        },
        beforeSend: () => $('#loadingIndicator').show(),
        success: async function (data) {
            if (data) {
                totalItems = data.totalItems || 0;
                renderOrderList(data.data || data);
                renderPagination();

                if (lotNo != "") {
                    goToOrderLot(lotNo);
                }
            }
        },
        error: function (error) {
            console.error('Error loading order data:', error);
            $('#loadingIndicator').hide();
        }
    });
}

function renderOrderList(data) {
    const container = $('#accordionOrder');
    container.empty();

    if (!data || !data.days) {
        $('#loadingIndicator').hide();
        return;
    };

    data.days.forEach(day => {
        day.orders.forEach(order => {
            const lotsHtml = order.customLot.map((lot, index) => renderLotRow(order, lot, index + 1)).join('');

            const itemHtml = `
                <div class="accordion-item mt-2" data-order-no="${order.orderNo}">
                    <h2 class="accordion-header" id="heading${order.orderNo}">
                        <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse"
                            data-bs-target="#collapse${order.orderNo}" aria-expanded="false"
                            aria-controls="collapse${order.orderNo}">
                            <div class="d-flex justify-content-between w-100">
                                <div class="col-md-3">
                                    <strong>
                                        ${order.custCode}/${order.orderNo}
                                        ${order.isReceivedLate ? "<i class='fas fa-fire-alt' style='color: #e85700;'></i>" : ""}
                                        ${order.isPackingLate ? "<i class='fas fa-fire-alt' style='color: red;'></i>" : ""}
                                        ${order.isNew ? "<span class='badge bg-danger new'>ใหม่</span>" : ""}
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
                                    <i class="far fa-gem"></i> ${order.sumTtQty}
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
                    <div id="collapse${order.orderNo}" class="accordion-collapse collapse"
                        aria-labelledby="heading${order.orderNo}" data-bs-parent="#accordionOrder">
                        <div class="accordion-body table-responsive" style="overflow: auto;">
                            <table class="table table-striped projects">
                                <thead>
                                    <tr>
                                       <th style="width: 1%">#</th>
                                       <th style="width: 20%">หมายเลขล็อต</th>
                                       <th style="width: 10%">ลำดับที่</th>
                                       <th style="width: 15%">การมอบหมาย</th>
                                       <th>ความคืบหน้า</th>
                                       <th style="width: 8%" class="text-center">สถานะ</th>
                                       <th style="width: 30%"></th>
                                    </tr>
                                </thead>
                                <tbody>${lotsHtml}</tbody>
                            </table>
                        </div>
                    </div>
                </div>
            `;

            container.append(itemHtml);
        });
    });

    $('#loadingIndicator').hide();
}

function renderLotRow(order, lot, index = "#") {
    const percent = lot.ttQty > 0 ? (lot.receivedQty * 100 / lot.ttQty) : 0;
    const returnPercent = lot.ttQty > 0 ? (lot.returnedQty * 100 / lot.ttQty) : 0;

    // Progress Bar
    let progressHtml = '';
    if (!lot.isAllReceived) {
        progressHtml = `<div class='progress-bar bg-orange' role='progressbar' style='width: ${percent}%'></div>`;
    } else if (lot.isAllReceived && !lot.isPacking) {
        progressHtml = `<div class='progress-bar bg-light-orange' role='progressbar' style='width: ${percent}%'></div>`;
    } else if (lot.isPacking && !lot.isAllReturned) {
        progressHtml = `<div class='progress-bar bg-warning' role='progressbar' style='width: ${returnPercent}%'></div>`;
    } else if (lot.isAllReturned) {
        progressHtml = `<div class='progress-bar bg-info' role='progressbar' style='width: 100%'></div>`;
    }

    // Progress Text
    let progressText = '';
    if (!lot.isAllReceived) {
        progressText = `${lot.receivedQty.toFixed(1)} / ${lot.ttQty.toFixed(1)}`;
    } else if (lot.isAllReceived && !lot.isPacking) {
        progressText = `${lot.receivedQty.toFixed(1)} / ${lot.ttQty.toFixed(1)}`;
    } else if (lot.isPacking && !lot.isAllReturned) {
        progressText = `${lot.returnedQty.toFixed(1)} / ${lot.ttQty.toFixed(1)}`;
    } else if (lot.isAllReturned) {
        progressText = `${lot.ttQty.toFixed(1)} / ${lot.ttQty.toFixed(1)}`;
    }

    // Status Badges
    let statusHtml = '';
    if (lot.receivedQty < lot.ttQty)
        statusHtml += `<span class="badge badge-orange">รอนำส่ง</span>`;
    if (lot.ttQty > 0 && lot.receivedQty >= lot.ttQty)
        statusHtml += `<span class="badge badge-orange">รับครบแล้ว</span>`;
    if (lot.isAllReceived && !lot.isPacking && !lot.isAllAssigned)
        statusHtml += `<span class="badge badge-light-orange">รอจ่ายงาน</span>`;
    if (lot.isPacking && !lot.isAllReturned)
        statusHtml += `<span class="badge badge-warning">กำลังบรรจุ</span>`;
    if (lot.isAllReturned)
        statusHtml += `<span class="badge badge-info">บรรจุครบแล้ว</span>`;
    if (lot.hasRepair)
        statusHtml += `<span class="badge badge-danger">ส่งซ่อม</span>`;
    if (lot.hasLost)
        statusHtml += `<span class="badge badge-danger">สูญหาย</span>`;

    // Actions
    let actionsHtml = '';
    if (lot.isPacking || lot.isAllReturned)
        actionsHtml += `<button class='btn btn-warning btn-sm' onclick='showModalLost("${lot.lotNo}")'><i class='far fa-eye-slash'></i> หาย</button> `;
    if (lot.isPacking || lot.isAllReturned)
        actionsHtml += ` <button class='btn btn-warning btn-sm' onclick='showModalBreak("${lot.lotNo}")'><i class='fas fa-hammer'></i> ซ่อม</button> `;
    if (!lot.isAllReturned && lot.ttQty !== 0 && !lot.isAllReturned)
        actionsHtml += ` <button class='btn btn-primary btn-sm' onclick='showModalAssign("${lot.lotNo}")'><i class='fas fa-folder'></i> จ่ายงาน</button> `;
    if (lot.isPacking && !lot.isAllReturned)
        actionsHtml += ` <button class='btn btn-danger btn-sm' onclick='showModalReturn("${lot.lotNo}")'><i class='fas fa-folder'></i> รับคืน</button> `;

    // Assigned tables
    const assignHtml = lot.assignTo?.map(a =>
        `<a><span class="badge badge-secondary" onclick="showModalTableMember(${a.assignmentId})">${a.tableName}</span></a>`
    ).join(' ') || '-';

    return `
        <tr data-lot-no="${lot.lotNo}">
            <td>${index}</td>
            <td>
                <a><strong>${lot.lotNo}</strong>
                    ${order.isReceivedLate && !lot.isAllReturned ? "<i class='fas fa-fire-alt' style='color: #e85700;'></i>" : ""}
                    ${order.isPackingLate && !lot.isAllReturned ? "<i class='fas fa-fire-alt' style='color: red;'></i>" : ""}
                </a><br/>
                <small>ปรับปรุงล่าสุด : ${lot.updateDate ?? "N/A"}</small>
            </td>
            <td>${lot.listNo}</td>
            <td>${assignHtml}</td>
            <td class="project_progress">
                <div class="progress progress-sm">${progressHtml}</div>
                <small>${progressText}</small>
            </td>
            <td class="project-state">${statusHtml}</td>
            <td class="project-actions text-right">${actionsHtml}</td>
        </tr>
    `;
}

async function updateLotRow(lotNo) {
    try {
        const res = await $.ajax({
            url: urlGetCustomLot, // สร้างตัวแปร url นี้ mapping ไปที่ Home/GetCustomLot
            type: 'GET',
            data: { lotNo: lotNo }
        });

        if (!res) return;

        // res คือข้อมูล lot เดี่ยว → ต้องหา order ที่ lot นี้อยู่
        const $row = $(`tr[data-lot-no="${lotNo}"]`);
        if ($row.length > 0) {
            const $accordionItem = $row.closest(".accordion-item");
            const orderNo = $accordionItem.data("order-no");

            // หา order object จาก DOM (เก็บ orderNo ไว้ใน data-order-no)
            const orderData = { orderNo: orderNo }; // dummy object
            const newRowHtml = renderLotRow(orderData, res);

            $row.replaceWith(newRowHtml);
        }
    } catch (err) {
        console.error("Update row failed:", err);
    }
}

function renderPagination() {
    const totalPages = Math.ceil(totalItems / currentPageSize);

    // Update info text
    const startItem = (currentPage - 1) * currentPageSize + 1;
    const endItem = Math.min(currentPage * currentPageSize, totalItems);
    $('.dataTables_info').html(`แสดง ${startItem} ถึง ${endItem} จาก ${totalItems} รายการ`);

    // Update page size dropdown
    $('#pageSize').val(currentPageSize);

    // Build pagination HTML
    let paginationHtml = '';

    // First page button
    paginationHtml += `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="goToPage(1); return false;">
                <i class="fas fa-angle-double-left"></i>
            </a>
        </li>
    `;

    // Previous page button
    paginationHtml += `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="goToPage(${currentPage - 1}); return false;">
                <i class="fas fa-angle-left"></i>
            </a>
        </li>
    `;

    // Page numbers
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);

    if (startPage > 1) {
        paginationHtml += '<li class="page-item disabled"><span class="page-link">...</span></li>';
    }

    for (let i = startPage; i <= endPage; i++) {
        paginationHtml += `
            <li class="page-item ${currentPage === i ? 'active' : ''}">
                <a class="page-link" href="#" onclick="goToPage(${i}); return false;">${i}</a>
            </li>
        `;
    }

    if (endPage < totalPages) {
        paginationHtml += '<li class="page-item disabled"><span class="page-link">...</span></li>';
    }

    // Next page button
    paginationHtml += `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="goToPage(${currentPage + 1}); return false;">
                <i class="fas fa-angle-right"></i>
            </a>
        </li>
    `;

    // Last page button
    paginationHtml += `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="goToPage(${totalPages}); return false;">
                <i class="fas fa-angle-double-right"></i>
            </a>
        </li>
    `;

    $('.pagination').html(paginationHtml);

    // Scroll to top after pagination
    $('html, body').animate({ scrollTop: 0 }, 300);
}

async function showModalTableMember(assignedID) {
    const modal = $('#modal-table-member');
    modal.find('#txtTitleTableMember').html(
        "<i class='fas fa-folder-plus'></i> โต๊ะทำงาน : " + html(assignedID)
    );
    modal.modal('show');

    $.ajax({
        url: urlGetTableMemberByAssignedID,
        type: 'GET',
        data: { assignedID: assignedID },
        success: function (res) {
            console.log(res)
        },
        error: async function (xhr) {
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${xhr.statusText})`);
        },
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

    $("#txtWorkerNumber").val(0);
    $("#chxHasPartTime").prop("checked", false);
    $("#txtWorkerNumber").prop('disabled', true)
    $("#chxHasPartTime").prop('disabled', true)

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
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status})`);
        },
        complete: function () {
            select.prop('disabled', false);
        }
    });
}

async function showModalAddLost() {
    $('#txtLostQty').val(0)
    const modal = $('#modal-add-Lost');
    modal.modal('show');
}

async function showModalLost(lotNo) {
    $('#hddLotNo').val(lotNo);

    const modal = $('#modal-lost');
    const tbody = modal.find('#tbl-body-lost');
    tbody.empty().append('<tr><td colspan="11" class="text-center text-muted">กำลังโหลด...</td></tr>');

    if (lotNo) {
        $('#btnAddLostList').removeClass('d-none');
        modal.find('#txtTitleLost').html("<i class='fas fa-eye-slash'></i> รายการแจ้งหาย : " + html(lotNo));
    }
    else {
        $('#btnAddLostList').addClass('d-none');
        modal.find('#txtTitleLost').html("<i class='fas fa-eye-slash'></i> รายการแจ้งหายทั้งหมด");
    }

    modal.modal('show');

    let model = {
        LotNo: String(lotNo),
    };

    $.ajax({
        url: urlGetLost,
        type: 'POST',
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            tbody.empty();
            if (!data || data.length === 0) {
                tbody.append('<tr><td colspan="11" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
                return;
            }
            const rows = data.map(function (x, i) {
                return `
            <tr data-lost-id="${html(x.lostID)}">
                <td>#</td>
                <td><strong>${html(x.custCode)}</strong></td>
                <td>${html(x.orderNo)}</td>
                <td>${html(x.lotNo)}</td>
                <td>${html(x.listNo)}</td>
                <td class="text-center">${html(x.article)}</td>
                <td>${html(x.ttQty)}</td>
                <td>${html(x.lostQty)}</td>
                <td>${html(x.createDate)}</td>
                <td>${x.isReported ? '✔️ รายงานแล้ว' : '❌ ยังไม่รายงาน'}</td>
                <td class="text-center">
                    <div class="icheck-primary d-inline">
                        <input type="checkbox" id="${x.lostID}_as${i}" class="chk-row" ${!x.isReported ? 'checked' : ''}>
                        <label for="${x.lostID}_as${i}"></label>
                    </div>
                </td>
            </tr>`;
            }).join('');
            tbody.append(rows);
        },
        error: function (xhr, status, error) {
            console.error('Error loading order data:', error, xhr, status);
            tbody.empty().append('<tr><td colspan="11" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
        }
    });
}

async function showModalAddBreak() {
    $('#txtBreakQty').val(0)
    $('#ddlBreakDes').val(null)
    $('#txtAddBreakDes').val('')
    $('#txtAddBreakDes').hide();

    $('#ddlBreakDes').select2({
        dropdownParent: $('#modal-add-break'),
    });

    $('#txtBreakQty').val(0)
    const modal = $('#modal-add-break');
    modal.modal('show');
}

async function showModalBreak(lotNo) {
    $('#hddLotNo').val(lotNo);

    const modal = $('#modal-break');
    const tbody = modal.find('#tbl-body-break');
    tbody.empty().append('<tr><td colspan="12" class="text-center text-muted">กำลังโหลด...</td></tr>');

    if (lotNo) {
        $('#btnAddBreakList').removeClass('d-none');
        modal.find('#txtTitleBreak').html("<i class='fas fa-hammer'></i> รายการแจ้งซ่อม : " + html(lotNo));
    }
    else {
        $('#btnAddBreakList').addClass('d-none');
        modal.find('#txtTitleBreak').html("<i class='fas fa-hammer'></i> รายการแจ้งซ่อมทั้งหมด");
    }

    modal.modal('show');

    let model = {
        LotNo: String(lotNo),
    };

    $.ajax({
        url: urlGetBreak,
        type: 'POST',
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            tbody.empty();
            if (!data || data.length === 0) {
                tbody.append('<tr><td colspan="12" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
                return;
            }
            const rows = data.map(function (x, i) {
                return `
            <tr data-break-id="${html(x.breakID)}">
                <td>#</td>
                <td><strong>${html(x.receiveNo)}</strong></td>
                <td>${html(x.custCode)}</td>
                <td>${html(x.orderNo)}</td>
                <td>${html(x.lotNo)}</td>
                <td>${html(x.listNo)}</td>
                <td>${html(x.previousQty)}</td>
                <td>${html(x.breakQty)}</td>
                <td>${html(x.breakDescription)}</td>
                <td>${html(x.createDate)}</td>
                <td>${x.isReported ? '✔️ รายงานแล้ว' : '❌ ยังไม่รายงาน'}</td>
                <td class="text-center">
                    <div class="icheck-primary d-inline">
                        <input type="checkbox" id="${x.breakID}_as${i}" class="chk-row" ${!x.isReported ? 'checked' : ''}>
                        <label for="${x.breakID}_as${i}"></label>
                    </div>
                </td>
            </tr>`;
            }).join('');
            tbody.append(rows);
        },
        error: function (xhr, status, error) {
            console.error('Error loading order data:', error, xhr, status);
            tbody.empty().append('<tr><td colspan="12" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
        }
    });
}

function showApproveToPrintModal() {

    const txtUsername = $('#txtUsername');
    const txtPassword = $('#txtPassword');
    const btn = $('#btnValidateApprover');

    txtUsername.val('').removeClass('is-invalid is-warning is-valid').prop('disabled', false);
    txtPassword.val('').removeClass('is-invalid is-warning is-valid').prop('disabled', false);
    btn.removeClass('btn-success').addClass('btn-primary').text('ตรวจสอบสิทธิ์').prop('disabled', false);
    $('#btnPrintReport').prop('disabled', true)

    $('#hddUserID').val('');

    $('#modal-approve-to-print').modal('show');
}

async function printBreakToPDF() {
    let lotNo = $('#hddLotNo').val();

    const tbekbody = $('#tbl-body-break');
    const breakIDs = [];

    tbekbody.find('tr').each(function () {
        const chk = $(this).find('.chk-row');
        if (chk.is(':checked')) {
            const breakID = $(this).data('break-id');
            if (breakID) breakIDs.push(breakID);
        }
    });

    if (breakIDs.length === 0) {
        await showWarning('กรุณาเลือก break ที่ต้องการพิมพ์');
        return;
    }

    let model = {
        LotNo: lotNo,
        BreakIDs: breakIDs
    };

    let pdfWindow = window.open('', '_blank');

    $.ajax({
        url: urlBreakReport,
        type: 'POST',
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data) {
            const blob = new Blob([data], { type: 'application/pdf' });
            const blobUrl = URL.createObjectURL(blob);

            if (pdfWindow) {
                pdfWindow.location = blobUrl;
            }
        },
        error: function (xhr) {
            if (pdfWindow) {
                pdfWindow.close();
            }

            showError("ไม่สามารถดึงรายงานได้ " + xhr.statusText);
        }
    });
}

async function printLostToPDF() {
    let lotNo = $('#hddLotNo').val();

    const username = $('#txtUsername').val();
    const password = $('#txtPassword').val();

    const tlosbody = $('#tbl-body-lost');
    const lostIDs = [];

    tlosbody.find('tr').each(function () {
        const chk = $(this).find('.chk-row');
        if (chk.is(':checked')) {
            const lostID = $(this).data('lost-id');
            if (lostID) lostIDs.push(lostID);
        }
    });

    if (lostIDs.length === 0) {
        await showWarning('กรุณาเลือก Lost ที่ต้องการพิมพ์');
        return;
    }

    let model = {
        LotNo: lotNo,
        LostIDs: lostIDs,
        Username: username,
        Password: password
    };

    let pdfWindow = window.open('', '_blank');

    $.ajax({
        url: urlLostReport,
        type: 'POST',
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data) {
            const blob = new Blob([data], { type: 'application/pdf' });
            const blobUrl = URL.createObjectURL(blob);

            if (pdfWindow) {
                pdfWindow.location = blobUrl;
            }
        },
        error: function (xhr) {
            if (pdfWindow) {
                pdfWindow.close();
            }

            showError("ไม่สามารถดึงรายงานได้ " + xhr.statusText);
        }
    });
}

async function goToOrderLot(keyword) {
    if (!keyword) return;

    let $target = $(`.accordion-item[data-order-no="${keyword}"]`);

    if ($target.length > 0) {
        const collapseId = $target.find(".accordion-collapse").attr("id");
        const $button = $target.find(`button[data-bs-target="#${collapseId}"]`);

        if (!$target.find(".accordion-collapse").hasClass("show")) {
            $button.click();
        }

        $("html, body").animate({
            scrollTop: $target.offset().top - 100
        }, 200);

        return;
    }

    $target = $(`tr[data-lot-no="${keyword}"]`);
    if ($target.length > 0) {
        const $accordionItem = $target.closest(".accordion-item");
        const collapseId = $accordionItem.find(".accordion-collapse").attr("id");
        const $button = $accordionItem.find(`button[data-bs-target="#${collapseId}"]`);

        if (!$accordionItem.find(".accordion-collapse").hasClass("show")) {
            $button.click();
        }

        setTimeout(() => {
            $("html, body").animate({
                scrollTop: $target.offset().top - 100
            }, 200);
            $target.addClass("table-warning");
            setTimeout(() => $target.removeClass("table-warning"), 2000);
        }, 400);

        return;
    }

    await showWarning("ไม่พบข้อมูล Lot หรือ Order ที่ค้นหา");
}

function goToPage(page) {
    if (page < 1) return;
    currentPage = page;
    fetchOrdersByDateRange();
}

function changePageSize(pageSize) {
    currentPageSize = parseInt(pageSize);
    currentPage = 1;
    fetchOrdersByDateRange();
}

function clearModalAssignValues() {
    $("#cbxTables").val("");
    $("#tblMembers").empty();
}

function clearModalReturnValues() {
    $("#cbxTableToReturn").val("");
    $("#tbl-receivedReturn-body").empty();
    $("#txtReurnQty").val(0);
}

function showModalSendTo() {
    $('#modal-Sendto').modal('show');
}

function CloseModal() {
    $('.modal').modal('hide');
}

function ClearFindBy() {
    $("#txtOrderNo").val("");
    $("#txtLotNo").val("");
    $("#txtCustCode").val("");
    $("#pageSize").val(10);
    currentPage = 1;
    currentPageSize = 10;
    fetchOrdersByDateRange()
}
