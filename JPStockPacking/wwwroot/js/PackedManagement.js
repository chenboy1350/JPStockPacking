let currentForceRow = null;

$(document).ready(function () {
    $(document).on('keydown', '#txtOrderNo', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            FindOrderToStore();
        }
    });

    $(document).on('click', '.btn-edit', function () {
        const $row = $(this).closest('tr');

        // แสดงปุ่ม +
        $row.find('.btn-edit-qty').removeClass('d-none');

        // ซ่อนปุ่ม edit
        $(this).addClass('d-none');

        // แสดงปุ่ม save
        $row.find('.btn-save').removeClass('d-none');
    });


    $(document).on('click', '.btn-save', function () {
        const uid = $('#hddUserID').val();

        const $row = $(this).closest('tr');

        const lotNo = $row.data('lot-no');
        const store_qty = parseFloat($row.find('input.store_qty').val()) || 0;
        const melt_qty = parseFloat($row.find('input.melt_qty').val()) || 0;
        const export_qty = parseFloat($row.find('input.export_qty').val()) || 0;

        const melt_wg = parseFloat($row.attr('data-melt-wg')) || 0;
        const melt_des = parseInt($row.attr('data-melt-des')) || 0;
        const store_wg = parseFloat($row.attr('data-store-wg')) || 0;
        const export_wg = parseFloat($row.attr('data-export-wg')) || 0;

        const force_send_by = parseInt($row.attr('data-force-user')) || 0;

        // ซ่อนปุ่มบันทึก
        $(this).addClass('d-none');

        // แสดงปุ่มแก้ไข
        $row.find('.btn-edit').removeClass('d-none');

        // ซ่อนปุ่ม + ทุกช่อง
        $row.find('.btn-edit-qty').addClass('d-none');
        $row.find('.btn-clear-qty').addClass('d-none');

        $row.find('.btn-save, .btn-cancel').addClass('d-none');

        let model = {
            LotNo: String(lotNo),
            KsQty: store_qty,
            KsWg: store_wg,
            KmQty: melt_qty,
            KmWg: melt_wg,
            KmDes: melt_des,
            KxQty: export_qty,
            KxWg: export_wg,
            Approver: force_send_by,
            UserId: String(uid),
        };

        $.ajax({
            url: urlSendToStore,
            type: "POST",
            data: JSON.stringify(model),
            contentType: "application/json; charset=utf-8",
            success: async function (res) {
                if (res.isSuccess) {
                    await showSuccess(`บันทึกสำเร็จ`);
                } else {
                    await showWarning(`เกิดข้อผิดพลาด (${res.code}) ${res.message})`);
                }
            },
            error: async function (xhr) {
                await showWarning(`เกิดข้อผิดพลาด (${xhr.status})`);
            }
        });
    });

    $(document).on('click', '.btn-edit-qty', function () {
        const $btn = $(this);
        const $row = $btn.closest('tr');
        const type = $btn.data('type');
        const lotNo = $row.data('lot-no');

        const storeQty = parseFloat($row.find('input.store_qty').val()) || 0;
        const meltQty = parseFloat($row.find('input.melt_qty').val()) || 0;
        const exportQty = parseFloat($row.find('input.export_qty').val()) || 0;
        const packedQty = parseFloat($row.find('.col-packed').text().replace(/,/g, '')) || 0;

        const sendtopack_qty = parseFloat($row.data('sendtopack-qty')) || 0;

        const store_Wg = parseFloat($row.data('store-wg')) || 0;
        const export_Wg = parseFloat($row.data('export-wg')) || 0;

        const melt_Wg = parseFloat($row.data('melt-wg')) || 0;
        const melt_Des = parseInt($row.data('melt-des')) || 0;

        let currentQty = 0;
        if (type === 'store') currentQty = storeQty;
        if (type === 'melt') currentQty = meltQty;
        if (type === 'export') currentQty = exportQty == 0 ? sendtopack_qty : exportQty;

        const maxQty = packedQty - storeQty - meltQty - exportQty + currentQty;

        if (type === 'store') {
            $('#modalStoreLotNo').val(lotNo);
            $('#modalStoreQty').val(currentQty);
            $('#modalStoreMaxQty').val(maxQty);
            $('#maxStoreQtyLabel').text(maxQty);
            $currentInput = $row.find('input.store_qty');
            $('#modal-edit-store').modal('show');
        } else if (type === 'melt') {
            $('#modalMeltLotNo').val(lotNo);
            $('#modalMeltMaxQty').val(maxQty);
            $('#maxMeltQtyLabel').text(maxQty);
            $('#modalMeltQty').val(currentQty).trigger('change');
            $('#modalMeltWg').val(melt_Wg);
            $('#modalMeltReasonId').val(melt_Des).trigger('change');
            $currentInput = $row.find('input.melt_qty');
            $('#modal-edit-melt').modal('show');
        } else if (type === 'export') {
            currentForceRow = $row;
            $('#modalExportLotNo').val(lotNo);
            $('#modalExportQty').val(currentQty);

            // อ่านค่า sendToPack_Qty จากแถว (สมมติชื่อ data attribute คือ data-sendtopack-qty)
            const sendToPackQty = parseFloat($row.data('sendtopack-qty')) || 0;

            // limit ด้วย sendToPack_Qty ถ้ามีค่า > 0
            let maxQty = packedQty - storeQty - meltQty - exportQty + currentQty;
            if (sendToPackQty > 0) {
                maxQty = Math.min(maxQty, sendToPackQty);
            }

            $('#modalExportMaxQty').val(maxQty);
            $('#maxExportQtyLabel').text(maxQty);
            $currentInput = $row.find('input.export_qty');
            $('#modal-edit-export').modal('show');
        }
    });

    $(document).on('input change', '#modalMeltQty', function () {
        const qty = parseFloat($(this).val()) || 0;
        const lotNo = $('#modalMeltLotNo').val();

        // หาแถวในตารางที่ตรงกับ lotNo
        const $row = $(`#tbl-sendToStore-body tr[data-lot-no="${lotNo}"]`);

        const baseWg = calcWgFromQty($row, qty);
        const percentage = parseFloat($row.data('percentage')) || 0;

        const maxWg = +(baseWg + (percentage / 100 * baseWg)).toFixed(2);

        $('#modalMeltWg').val(baseWg.toFixed(2)).attr('max', maxWg);
        $('#modalMeltWg').siblings('label').html(`น้ำหนัก (กรัม) <span class="text-muted">(สูงสุด ${maxWg.toLocaleString(undefined, { maximumFractionDigits: 2 })})</span>`);
    });

    $(document).on('click', '#btnStoreQtyConfirm', function () {
        const newVal = parseFloat($('#modalStoreQty').val()) || 0;
        const max = parseFloat($('#modalStoreMaxQty').val()) || 0;

        if (newVal > max) {
            showWarning(`เกินยอดที่สามารถเก็บได้ (${max})`);
            return;
        }
        $currentInput.val(newVal).trigger('change');

        const $row = $currentInput.closest('tr');
        const storeWg = calcWgFromQty($row, newVal);
        $row.attr('data-store-wg', storeWg);

        updateAvailableQty($currentInput.closest('tr'));

        $('#modal-edit-store').modal('hide');
    });

    $(document).on('click', '#btnMeltQtyConfirm', function () {
        const qty = parseFloat($('#modalMeltQty').val()) || 0;
        const max = parseFloat($('#modalMeltMaxQty').val()) || 0;
        const wg = parseFloat($('#modalMeltWg').val()) || 0;
        const reasonId = $('#modalMeltReasonId').val();

        if (qty > max) {
            showWarning(`เกินยอดที่สามารถเก็บได้ (${max})`);
            return;
        }

        if (qty > 0 && wg == 0) {
            showWarning(`กรุณากรอกน้ำหนัก`);
            return;
        }

        if (qty > 0 && !reasonId) {
            showWarning(`กรุณาเลือกสาเหตุ`);
            return;
        }

        // อัปเดตเฉพาะ input melt_qty
        $currentInput.val(qty).trigger('change');

        // เก็บค่า wg และ reason ไว้ใน attribute (หรือนำไปใช้ทันที)
        const $row = $currentInput.closest('tr');
        $row.attr('data-melt-wg', wg);
        $row.attr('data-melt-des', reasonId);

        updateAvailableQty($currentInput.closest('tr'));

        const maxAllowedWg = parseFloat($('#modalMeltWg').attr('max')) || 0;

        if (wg > maxAllowedWg) {
            showWarning(`น้ำหนักที่กรอกเกินที่อนุญาต (${maxAllowedWg} กก.)`);
            return;
        }


        $('#modal-edit-melt').modal('hide');
    });

    $(document).on('click', '#btnExportQtyConfirm', function () {
        const newVal = parseFloat($('#modalExportQty').val()) || 0;
        const max = parseFloat($('#modalExportMaxQty').val()) || 0;
        if (newVal > max) {
            showWarning(`เกินยอดที่สามารถเก็บได้ (${max})`);
            return;
        }
        $currentInput.val(newVal).trigger('change');

        const $row = $currentInput.closest('tr');
        const exportWg = calcWgFromQty($row, newVal);
        $row.attr('data-export-wg', exportWg);

        $row.removeAttr('data-force-user');

        updateAvailableQty($currentInput.closest('tr'));

        $('#modal-edit-export').modal('hide');
    });

    $(document).on('click', '.btn-edit', function () {
        const $row = $(this).closest('tr');

        // เก็บค่าปัจจุบันไว้ใน data
        $row.data('old-store', $row.find('input.store_qty').val());
        $row.data('old-melt', $row.find('input.melt_qty').val());
        $row.data('old-export', $row.find('input.export_qty').val());

        $row.find('.btn-edit-qty').removeClass('d-none');
        $row.find('.btn-clear-qty').removeClass('d-none');

        // สลับปุ่ม
        $(this).addClass('d-none');
        $row.find('.btn-save, .btn-cancel').removeClass('d-none');
    });

    $(document).on('click', '.btn-cancel', function () {
        const $row = $(this).closest('tr');

        // คืนค่าที่เคยเก็บไว้
        $row.find('input.store_qty').val($row.data('old-store'));
        $row.find('input.melt_qty').val($row.data('old-melt'));
        $row.find('input.export_qty').val($row.data('old-export'));

        $row.find('.btn-edit-qty').addClass('d-none');
        $row.find('.btn-clear-qty').addClass('d-none');

        // สลับปุ่ม
        $row.find('.btn-save, .btn-cancel').addClass('d-none');
        $row.find('.btn-edit').removeClass('d-none');

        // อัปเดต available ใหม่ตามค่าที่คืน
        updateAvailableQty($row);
    });

    // เมื่อ checkbox รายแถวเปลี่ยนค่า
    $(document).on("change", ".chk-row", function () {
        toggleConfirmButton();
    });

    // เมื่อ checkbox เลือกทั้งหมดเปลี่ยนค่า
    $(document).on("change", "#chkSelectAllLotToStore", function () {
        const checked = $(this).is(":checked");
        $(".chk-row").prop("checked", checked);
        toggleConfirmButton();
    });

    //  เช็ค / ยกเลิกเช็ค ทุกแถวเมื่อคลิกที่ checkbox บนหัวตาราง
    $(document).on('change', '#chkSelectAllLotToStore', function () {
        const checked = $(this).is(':checked');
        $(".chk-row").prop("checked", checked);
    });

    $(document).off('click', '.btn-clear-qty').on('click', '.btn-clear-qty', function () {
        const $btn = $(this);
        const $row = $btn.closest('tr');
        const type = String($btn.data('type') || '').toLowerCase();

        // หาอินพุตของแต่ละคอลัมน์
        const $storeInput = $row.find('input.store_qty');
        const $meltInput = $row.find('input.melt_qty');
        const $exportInput = $row.find('input.export_qty');

        if (type === 'store') {
            $storeInput.val(0).trigger('change');
            // ถ้าเคยคำนวณ/เก็บน้ำหนัก store ให้รีเซ็ตด้วย
            $row.data('store-wg', 0);
        } else if (type === 'melt') {
            $meltInput.val(0).trigger('change');
            // รีเซ็ตน้ำหนักและเหตุผลของ melt
            $row.data('melt-wg', 0);
            $row.data('melt-des', '');
            // ถ้ามี badge/label แสดงเหตุผลในแถว ก็ควรเคลียร์ด้วย (ตัวอย่าง)
            $row.find('.melt-reason-text').text('');
        } else if (type === 'export') {
            $exportInput.val(0).trigger('change');
            // รีเซ็ตน้ำหนัก export ถ้ามีเก็บไว้
            $row.data('export-wg', 0);
            $row.removeAttr('data-force-user');
        } else {
            // กันพลาด: ถ้า data-type ไม่ตรงที่คาด ไม่ทำอะไร
            return;
        }

        // อัปเดตยอดคงเหลือ/แสดงผลรวมใหม่
        if (typeof updateAvailableQty === 'function') {
            updateAvailableQty($row);
        }
    });

    $(document).on('click', '#btnCheckAuthForceSendToExport', function () {
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
                    btn.removeClass('btn-primary btn-danger btn-warning').addClass('btn-success').text(`ยืนยันการแก้ไขโดยผู้ใช้ ${res.username}`).prop('disabled', true);
                    $('#hddUserID').val(res.userID)
                    $('#txtforce-adjust-qty').focus();
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

                setTimeout(() => {
                    btn.text('ตรวจสอบสิทธิ').removeClass('btn-danger btn-success btn-warning').addClass('btn-primary').prop('disabled', false);
                }, 2000);
            }
        });
    });

    $(document).on('keydown', '#txtUsername', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnCheckAuthForceSendToExport').click();
        }
    });

    $(document).on('keydown', '#txtPassword', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnCheckAuthForceSendToExport').click();
        }
    });

    $(document).on('click', '#btnSubmitForceSendToExport', function (e) {
        // ตรวจสอบว่ามีแถวหรือยัง
        if (!currentForceRow || currentForceRow.length === 0) {
            showWarning('ไม่พบแถวที่จะอัปเดต');
            return;
        }

        const userId = $('#hddUserID').val();
        const qtyRaw = $('#txtforce-adjust-qty').val();
        const qty = parseFloat(qtyRaw);

        if (!userId) {
            showWarning('กรุณาตรวจสอบสิทธิ์ก่อน (ตรวจสอบผู้ใช้และรหัสผ่าน)');
            return;
        }

        if (isNaN(qty) || qty < 0) {
            showWarning('กรุณากรอกจำนวนที่ถูกต้อง');
            return;
        }

        // อัปเดต input.export_qty ในแถว
        const $exportInput = currentForceRow.find('input.export_qty');
        $exportInput.val(qty).trigger('change');

        // เก็บ userId ลง attribute ของแถว (หรือจะเก็บลง input hidden ก็ได้)
        currentForceRow.attr('data-force-user', userId);
        // ถ้าต้องการเก็บเป็นค่าใน input hidden:
        // $currentForceRow.find('input.hddForceUser').val(userId);

        // คำนวณ wg ใหม่ (ถ้ามี)
        const exportWg = calcWgFromQty(currentForceRow, qty);
        currentForceRow.attr('data-export-wg', exportWg);

        // อัปเดตยอดคงเหลือ
        if (typeof updateAvailableQty === 'function') {
            updateAvailableQty(currentForceRow);
        }

        // ปิด modal
        $('#modal-force-send-to-export').modal('hide');
        $('#modal-edit-export').modal('hide');

        // ล้างข้อมูลชั่วคราว (ไม่จำเป็นแต่สะอาด)
        // ไม่ลบ $currentForceRow เพราะอาจต้องใช้ต่อ แต่ถ้าต้องการล้าง:
        // $currentForceRow = null;
    });

    // --- เมื่อ modal ปิดด้วยการกดปิด ให้ไม่ค้าง state ผิดพลาด (optional)
    $(document).on('hidden.bs.modal', '#modal-force-send-to-export', function () {
        // ไม่ล้าง $currentForceRow ทันที เผื่อยังต้องใช้ แต่ถ้าต้องการให้ล้าง:
        // $currentForceRow = null;

        // reset field
        $('#txtUsername').val('').removeClass('is-valid is-invalid is-warning').prop('disabled', false);
        $('#txtPassword').val('').removeClass('is-valid is-invalid is-warning').prop('disabled', false);
        $('#btnCheckAuthForceSendToExport').text('ตรวจสอบสิทธิ์').removeClass('btn-success btn-danger btn-warning').addClass('btn-primary').prop('disabled', false);
    });
});

function FindOrderToStore() {
    $('#loadingIndicator').show();
    const txtOrderNo = $('#txtOrderNo').val();
    const tbody = $('#tbl-sendToStore-body');
    tbody.empty().append('<tr><td colspan="13" class="text-center text-muted">กำลังโหลด...</td></tr>');

    $('#modalMeltReasonId').select2({
        dropdownParent: $('#modal-edit-melt'),
    });


    $.ajax({
        url: urlGetOrderToStore,
        method: 'GET',
        data: { orderNo: txtOrderNo },
        dataType: 'json',
    })
    .done(function (response) {
        $('#loadingIndicator').hide();
        tbody.empty();

        if (!response || response.length === 0) {
            tbody.append('<tr><td colspan="13" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
            return;
        }

        $("#btnSelect").removeClass("d-none");

        $.each(response, function (i, item) {
            const rowHtml = `
                <tr data-lot-no="${html(item.lotNo)}"
                    data-ttwg="${html(item.ttWg)}"
                    data-percentage="${html(item.percentage)}"
                    data-store-wg="${html(item.store_Wg)}"
                    data-melt-wg="${html(item.melt_Wg)}"
                    data-melt-des="${html(item.breakDescriptionId)}"
                    data-export-wg="${html(item.export_Wg)}"
                    data-sendtopack-qty="${html(item.sendToPack_Qty)}"
                    >
                    <td class="text-center">${i + 1}</td>
                    <td class="text-start"><strong>${html(item.article)}</strong></br><small>${html(item.custCode)}/${html(item.orderNo)}</small></td>
                    <td class="text-center">${html(item.listNo)}</td>
                    <td class="text-end">${numRaw(item.ttQty)}</td>
                    <td class="text-end">${numRaw(item.si)}</td>
                    <td class="text-end">${numRaw(item.sendPack_Qty)}</td>
                    <td class="text-end">${numRaw(item.sendToPack_Qty)}</td>
                    <td class="text-end col-packed">${numRaw(item.packed_Qty)}</td>
                    <td class="text-center col-available"><strong>${numRaw(item.packed_Qty - item.store_Qty - item.melt_Qty - item.export_Qty)}</strong></td>

                    <td class="text-end">
                        <div class="d-flex justify-content-between align-content-center gap-2">
                            <input class="form-control text-center qty-input store_qty" type="number" min="0" step="any" value="${numRaw(item.store_Qty)}" readonly />
                            <button class="btn btn-default btn-edit-qty d-none" data-type="store"><i class="fas fa-plus"></i></button>
                            <button class="btn btn-default btn-clear-qty d-none" data-type="store"><i class="fas fa-trash"></i></button>
                        </div>
                    </td>

                    <td class="text-end">
                        <div class="d-flex justify-content-between align-content-center gap-2">
                            <input class="form-control text-center qty-input melt_qty" type="number" min="0" step="any" value="${numRaw(item.melt_Qty)}" readonly />
                            <button class="btn btn-default btn-edit-qty d-none" data-type="melt"><i class="fas fa-plus"></i></button>
                            <button class="btn btn-default btn-clear-qty d-none" data-type="melt"><i class="fas fa-trash"></i></button>
                        </div>
                    </td>

                    <td class="text-end">
                        <div class="d-flex justify-content-between align-content-center gap-2">
                            <input class="form-control text-center qty-input export_qty" type="number" min="0" step="any" value="${numRaw(item.export_Qty)}" readonly />
                            <button class="btn btn-default btn-edit-qty d-none" data-type="export"><i class="fas fa-plus"></i></button>
                            <button class="btn btn-default btn-clear-qty d-none" data-type="export"><i class="fas fa-trash"></i></button>
                        </div>
                    </td>

                    <td class="text-center">
                        <div class="d-flex gap-1 justify-content-center">
                            <button class="btn btn-sm btn-warning btn-edit"><i class="fas fa-edit"></i></button>
                            <button class="btn btn-sm btn-success btn-save d-none"><i class="fas fa-save"></i></button>
                            <button class="btn btn-sm btn-danger btn-cancel d-none"><i class="fas fa-times"></i></button>
                            <div id="${item.lotNo}_ss${i}" class="chk-wrapper d-none">
                                <div class="icheck-primary d-inline">
                                    <input type="checkbox" id="${item.lotNo}_rt${i}" class="chk-row">
                                    <label for="${item.lotNo}_rt${i}"></label>
                                </div>
                            </div>
                        </div>
                    </td>
                </tr>`;

            tbody.append(rowHtml);
        });
    })
    .fail(function (error) {
        $('#loadingIndicator').hide();
        console.error('Error fetching data:', error);
        tbody.empty().append('<tr><td colspan="13" class="text-center text-danger">เกิดข้อผิดพลาดในการโหลดข้อมูล</td></tr>');
    });
}

async function confirmSendToStore()
{
    const uid = $('#hddUserID').val();
    const selectedLots = [];
    $(".chk-row:checked").each(function () {
        const lotNo = $(this).closest("tr").data("lot-no");
        if (lotNo) selectedLots.push(lotNo);
    });

    const formData = new FormData();
    selectedLots.forEach(no => formData.append("lotNos", no));
    formData.append("userId", uid);

    $.ajax({
        url: urlConfirmToSendStore,
        type: 'POST',
        processData: false,
        contentType: false,
        data: formData,
        beforeSend: async () => $('#loadingIndicator').show(),
        success: async (res) => {
            $('#loadingIndicator').hide();
            await showSuccess(`${res.message}`);
        },
        error: async (xhr) => {
            $('#loadingIndicator').hide();
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

async function cancelSendToStore() {
    const uid = $('#hddUserID').val();
    const selectedLots = [];
    $(".chk-row:checked").each(function () {
        const lotNo = $(this).closest("tr").data("lot-no");
        if (lotNo) selectedLots.push(lotNo);
    });

    const formData = new FormData();
    selectedLots.forEach(no => formData.append("lotNos", no));
    formData.append("userId", uid);

    $.ajax({
        url: urlCancelToSendStore,
        type: 'POST',
        processData: false,
        contentType: false,
        data: formData,
        beforeSend: async () => $('#loadingIndicator').show(),
        success: async () => {
            $('#loadingIndicator').hide();
            await showSuccess("ยกเลิกส่งแล้ว");
        },
        error: async (xhr) => {
            $('#loadingIndicator').hide();
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

async function printSendToStore() {
    const uid = $('#hddUserID').val();
    const selectedLots = [];
    $(".chk-row:checked").each(function () {
        const lotNo = $(this).closest("tr").data("lot-no");
        if (lotNo) selectedLots.push(lotNo);
    });

    const formData = new FormData();
    selectedLots.forEach(no => formData.append("lotNos", no));
    formData.append("userId", uid);

    $.ajax({
        url: urlPrintSendToAllReport,
        type: 'POST',
        processData: false,
        contentType: false,
        data: formData,
        xhrFields: {
            responseType: 'blob'
        },
        beforeSend: async () => $('#loadingIndicator').show(),
        success: async (data) => {

            const blob = new Blob([data], { type: 'application/pdf' });
            const blobUrl = URL.createObjectURL(blob);
            window.open(blobUrl, '_blank');
            $('#loadingIndicator').hide();
        },
        error: async (xhr) => {
            $('#loadingIndicator').hide();
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

// ฟังก์ชันแสดง / ซ่อนปุ่ม "ยืนยันส่ง"
function toggleConfirmButton() {
    const anyChecked = $(".chk-row:checked").length > 0;
    $("#btnConfirmSendToStore").toggleClass("d-none", !anyChecked);
    $("#btnPrintSendToStore").toggleClass("d-none", !anyChecked);
}

// แสดง checkbox ทั้งหมด เมื่อกดปุ่ม "เลือก"
function showSelection() {
    // แสดงหัว checkbox
    $("#selectAllLotToStore").removeClass("d-none");

    // แสดง checkbox ของแต่ละแถว
    $(".chk-wrapper").removeClass("d-none");

    // แสดงปุ่มไม่เลือก
    $("#btnUnselect").removeClass("d-none");

    // ซ่อนปุ่มเลือก
    $("#btnSelect").addClass("d-none");

    $(".btn-edit").addClass("d-none");
    $(".btn-save").addClass("d-none");
    $(".btn-cancel").addClass("d-none");
}

// ซ่อน checkbox และ uncheck เมื่อกดปุ่ม "ไม่เลือก"
function hideSelection() {
    // ซ่อนหัว checkbox
    $("#selectAllLotToStore").addClass("d-none");

    // ซ่อน checkbox ของแต่ละแถว
    $(".chk-wrapper").addClass("d-none");

    // ยกเลิกการเลือกทุกแถว
    $(".chk-row").prop("checked", false);
    $("#chkSelectAllLotToStore").prop("checked", false);

    $("#btnConfirmSendToStore").addClass("d-none");
    $("#btnPrintSendToStore").addClass("d-none");

    // แสดงปุ่มเลือกอีกครั้ง
    $("#btnSelect").removeClass("d-none");
    $("#btnUnselect").addClass("d-none");

    $(".btn-edit").removeClass("d-none");
}

function calcWgFromQty($row, qty) {
    const ttWg = parseFloat($row.data('ttwg')) || 0;
    const ttQty = parseFloat($row.find('.col-packed').text().replace(/,/g, '')) || 0;

    if (ttQty === 0) return 0;
    const avg = ttWg / ttQty;
    return +(qty * avg).toFixed(2);
}

function updateAvailableQty($row) {
    const storeQty = parseFloat($row.find('input.store_qty').val()) || 0;
    const meltQty = parseFloat($row.find('input.melt_qty').val()) || 0;
    const exportQty = parseFloat($row.find('input.export_qty').val()) || 0;
    const packedQty = parseFloat($row.find('.col-packed').text().replace(/,/g, '')) || 0;

    const available = packedQty - storeQty - meltQty - exportQty;

    const formatted = available.toLocaleString(undefined, {
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
    });

    $row.find('.col-available').text(formatted);

    if (available < 0) {
        $row.find('.col-available').addClass('text-danger fw-bold');
    } else {
        $row.find('.col-available').removeClass('text-danger fw-bold');
    }
}

function ShowForceSendToModal(button) {
    if (!currentForceRow || currentForceRow.length === 0) {
        showWarning('ไม่พบแถวที่จะทำการส่งออกแบบกำหนดเอง กรุณาเปิด modal จากแถวที่ต้องการก่อน');
        return;
    }

    const oldQty = $('#modalExportQty').val();

    const txtUsername = $('#txtUsername');
    const txtPassword = $('#txtPassword');
    const btn = $('#btnCheckAuthForceSendToExport');

    // clear ค่า + reset border + enable
    txtUsername
        .val('')
        .removeClass('is-invalid is-warning is-valid')
        .prop('disabled', false);
    txtPassword
        .val('')
        .removeClass('is-invalid is-warning is-valid')
        .prop('disabled', false);

    // reset ปุ่ม
    btn
        .removeClass('btn-success')
        .addClass('btn-primary') // หรือ class เดิมที่คุณใช้
        .text('ตรวจสอบสิทธิ์') // ข้อความเดิมที่ต้องการ
        .prop('disabled', false);

    $('#hddUserID').val('')

    $('#txtforce-adjust-qty').val(oldQty);
    $('#modal-force-send-to-export').modal('show');
}

function ClearSearch()
{
    $('#txtOrderNo').val('');
    const tbody = $('#tbl-sendToStore-body');
    tbody.empty().append('<tr><td colspan="13" class="text-center text-muted">ค้นหาคำสั่งซื้อ</td></tr>');

    $("#btnConfirmSendToStore").addClass("d-none");
    $("#btnPrintSendToStore").addClass("d-none");
    $("#btnSelect").addClass("d-none");
    $("#selectAllLotToStore").addClass("d-none");
    $("#btnUnselect").addClass("d-none");
}