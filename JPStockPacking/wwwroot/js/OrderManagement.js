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
                await showWarning(`เกิดข้อผิดพลาด (${xhr.status})`);
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

                $('#txtLostQty').prop('readonly', false);
                $('#txtBreakQty').prop('readonly', false);

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

            $("#txtBreakQty").attr("max", sumQty);
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
                await updateLotRow(lotNo);
                CloseModal();
            },
            error: async (xhr) => {
                $('#loadingIndicator').hide();
                await showWarning("เกิดข้อผิดพลาดในการคืนงาน (" + xhr.status + ")");
            }
        });
    });

    $(document).on('click', '#btnLostAndRepair', async function () {
        const lotNo = $('#hddReturnLotNo').val();
        const txtReurnQty = $('#txtReurnQty').val();
        const txtLostQty = $('#txtLostQty').val();
        const txtBreakQty = $('#txtBreakQty').val();
        const ddlBreakDes = $('#ddlBreakDes').val();
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

        if (txtBreakQty > 0 && ddlBreakDes == '') {
            await showWarning('กรุณาเลือกอาการที่ชำรุด');
            return;
        }

        if (assignmentIDs.length === 0) {
            await showWarning('กรุณาเลือกใบนำเข้าที่จะรับคืน');
            return;
        }

        if (Number(txtBreakQty) > Number(txtReurnQty)) {
            await showWarning('จำนวนสินค้าที่ชำรุดมากกว่าจำนวนสินค้าที่รับคืน');
            return;
        }

        const formData = new FormData();
        formData.append("lotNo", lotNo);
        formData.append("lostQty", txtLostQty);
        formData.append("breakQty", txtBreakQty);
        formData.append("returnQty", txtTotalReurnQty);
        formData.append("breakDescriptionID", ddlBreakDes);
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
                await updateLotRow(lotNo);
                CloseModal();
            },
            error: async (xhr) => {
                $('#loadingIndicator').hide();
                await showWarning("เกิดข้อผิดพลาดในการคืนงาน (" + xhr.status + ")");
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

    $(document).on("input", "#txtLostQty, #txtBreakQty, #txtReurnQty", function () {
        calcTotalReturnQty();
    });

    $(document).on('change', '#chkSelectAllMembers', function () {
        const isChecked = $(this).is(':checked');
        $('#tblMembers .chk-row:enabled').prop('checked', isChecked);
    });

    $(document).on("click", "#btnGoTo", async function () {
        const keyword = $("#txtGoTo").val().trim().toLowerCase();
        $("#txtGoTo").val('')

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
    });


    $(document).on("keydown", "#txtGoTo", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            $("#btnGoTo").click();
        }
    });

    $(document).on("change", "#txtBreakQty", function () {
        console.log('asdasdasd')
        let val = parseInt($(this).val()) || 0;

        console.log('val', val)
        if (val > 0) {
            $("#breakDes").show();
            $("#breakDes2").show();
        } else {
            $("#breakDes").hide();
            $("#breakDes2").hide();
        }
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
                            await showWarning("เกิดข้อผิดพลาด (" + xhr.status + ")");
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

                console.log(res);
                txtUsername.removeClass('is-invalid is-warning').addClass('is-valid').prop('disabled', true);
                txtPassword.removeClass('is-invalid is-warning').addClass('is-valid').prop('disabled', true);
                btn.removeClass('btn-primary btn-danger btn-warning').addClass('btn-success').text(`ยืนยันการแก้ไขโดยผู้ใช้ ${res.username}`).prop('disabled', true);
                $('#btnPrintReport').prop('disabled', false)
                
                $('#hddUserID').val(res)
                $('#btnPrintReport').focus();
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
        printBreakToPDF($('#hddLotNo').val());
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
                                       <th style="width: 25%">การมอบหมาย</th>
                                       <th>ความคืบหน้า</th>
                                       <th style="width: 8%" class="text-center">สถานะ</th>
                                       <th style="width: 20%"></th>
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
    if (lot.hasRepair)
        actionsHtml += `<button class='btn btn-warning btn-sm' onclick='ShowApproveToPrintModal("${lot.lotNo}")'><i class='fas fa-user-lock'></i> ส่งซ่อม</button>`;
    if (lot.hasLost)
        actionsHtml += `<button class='btn btn-info btn-sm' onclick='printLostToPDF("${lot.lotNo}")'><i class='fas fa-folder'></i> สูญหาย</button>`;
    if ((order.isReceivedLate || lot.isAllReceived) && !lot.isAllReturned && lot.ttQty !== 0)
        actionsHtml += `<button class='btn btn-primary btn-sm' onclick='showModalAssign("${lot.lotNo}")'><i class='fas fa-folder'></i> จ่ายงาน</button>`;
    if (lot.isPacking && !lot.isAllReturned)
        actionsHtml += `<button class='btn btn-danger btn-sm' onclick='showModalReturn("${lot.lotNo}")'><i class='fas fa-folder'></i> รับคืน</button>`;

    // Assigned tables
    const assignHtml = lot.assignTo?.map(a =>
        `<a><span class="badge badge-secondary" onclick="showModalTableMember(${a.assignmentId})">${a.tableName}</span></a>`
    ).join(' ') || '-';

    return `
        <tr data-lot-no="${lot.lotNo}">
            <td>${index}</td>
            <td>
                <a><strong>${lot.lotNo}</strong>
                    ${order.isReceivedLate && !lot.isPacked ? "<i class='fas fa-fire-alt' style='color: #e85700;'></i>" : ""}
                    ${order.isPackingLate && !lot.isPacked ? "<i class='fas fa-fire-alt' style='color: red;'></i>" : ""}
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


async function showModalTableMember(assignedID) {
    console.log(assignedID)

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
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status})`);
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

    $("#breakDes").hide();
    $("#breakDes2").hide();
    $("#txtAddBreakDes").val('').hide();
    $('#txtLostQty').prop('readonly', true);
    $('#txtBreakQty').prop('readonly', true);
    $('#ddlBreakDes').val(null)
    $("#btnConfirmReturn").show();
    $("#btnLostAndRepair").hide();

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

function ShowApproveToPrintModal(lotNo) {

    const txtUsername = $('#txtUsername');
    const txtPassword = $('#txtPassword');
    const btn = $('#btnValidateApprover');

    txtUsername.val('').removeClass('is-invalid is-warning is-valid').prop('disabled', false);
    txtPassword.val('').removeClass('is-invalid is-warning is-valid').prop('disabled', false);
    btn.removeClass('btn-success').addClass('btn-primary').text('ตรวจสอบสิทธิ์').prop('disabled', false);
    $('#btnPrintReport').prop('disabled', true)

    $('#hddUserID').val('');

    $('#hddLotNo').val(lotNo);

    $('#modal-approve-to-print').modal('show');
}

async function printBreakToPDF(lotNo) {
    const username = $('#txtUsername').val();
    const password = $('#txtPassword').val();

    if (!lotNo || lotNo == '') {
        await showWarning('กรุณาเลือก lotNo');
        return;
    }

    const printUrl = urlBreakReport + '?lotNo=' + encodeURIComponent(lotNo) + '&username=' + encodeURIComponent(username) + '&password=' + encodeURIComponent(password);
    const printWindow = window.open(printUrl, '_blank');

    printWindow.onload = function () {
        // setTimeout(function() {
        //     printWindow.print();
        //     //printWindow.close();
        // }, 1000);
    };
}

async function printLostToPDF(lotNo) {
    if (!lotNo || lotNo == '') {
        await showWarning('กรุณาเลือก lotNo');
        return;
    }

    const printUrl = urlLostReport + '?lotNo=' + encodeURIComponent(lotNo);
    const printWindow = window.open(printUrl, '_blank');

    printWindow.onload = function () {
        // setTimeout(function() {
        //     printWindow.print();
        //     //printWindow.close();
        // }, 1000);
    };
}

function calcTotalReturnQty() {
    const lostQty = parseFloat($("#txtLostQty").val()) || 0;
    const breakQty = parseFloat($("#txtBreakQty").val()) || 0;
    const reurnQty = parseFloat($("#txtReurnQty").val()) || 0;

    const totalReturn = reurnQty - (breakQty + lostQty);

    $("#txtTotalReurnQty").val(totalReturn);
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
    fetchOrdersByDateRange()
}
