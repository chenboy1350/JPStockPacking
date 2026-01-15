let currentForceRow = null;
let $currentInput = null;

$(document).ready(function () {
    $(document).on('keydown', '#txtOrderNo', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            FindOrderToStore();
        }
    });

    $(document).on('click', '.btn-edit', function () {
        const $row = $(this).closest('tr');
        $row.find('.btn-edit-qty').removeClass('d-none');
        $(this).addClass('d-none');
        $row.find('.btn-save').removeClass('d-none');
    });


    $(document).on('click', '.btn-save', function () {
        const uid = $('#hddUserID').val();
        const $row = $(this).closest('tr');

        const lotNo = $row.data('lot-no');

        const store_qty = getDraftQty($row, 'store');
        const melt_qty = getDraftQty($row, 'melt');
        const export_qty = getDraftQty($row, 'export');
        const lost_qty = getDraftQty($row, 'lost');

        const store_wg = calcWgFromQty($row, store_qty);
        const melt_wg = parseFloat($row.attr('data-melt-wg')) || 0;
        const export_wg = calcWgFromQty($row, export_qty);
        const lost_wg = calcWgFromQty($row, lost_qty);

        const melt_des = parseInt($row.attr('data-melt-des')) || 0;
        const force_send_by = parseInt($row.attr('data-force-user')) || 0;

        const available = calculateAvailable($row);

        $(this).addClass('d-none');
        $row.find('.btn-edit').removeClass('d-none');
        $row.find('.btn-edit-qty').addClass('d-none');
        $row.find('.btn-clear-qty').addClass('d-none');
        $row.find('.btn-save, .btn-cancel').addClass('d-none');

        const model = {
            LotNo: String(lotNo),
            KsQty: store_qty,
            KsWg: store_wg,
            KmQty: melt_qty,
            KmWg: melt_wg,
            KmDes: melt_des,
            KlQty: lost_qty,
            KlWg: lost_wg,
            KxQty: export_qty,
            KxWg: export_wg,
            Approver: force_send_by,
            Unallocated: available,
            UserId: String(uid)
        };

        $.ajax({
            url: urlSendToStore,
            type: "POST",
            data: JSON.stringify(model),
            contentType: "application/json; charset=utf-8",
            success: async function (res) {

                if (res.isSuccess) {
                    FindOrderToStore();
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

        const currentQty = getDraftQty($row, type);
        const maxQty = calculateMaxQty($row, type);

        if (type === 'store') {
            $('#modalStoreLotNo').val(lotNo);
            $('#modalStoreQty').val(currentQty);
            $('#modalStoreMaxQty').val(maxQty);
            $('#maxStoreQtyLabel').text(maxQty);

            $currentInput = $row.find('input.store_qty');
            $('#modal-edit-store').modal('show');
            return;
        }

        if (type === 'melt') {
            const meltWg = parseFloat($row.data('melt-wg')) || 0;
            const meltDes = $row.data('melt-des') || "";

            $('#modalMeltLotNo').val(lotNo);
            $('#modalMeltQty').val(currentQty);
            $('#modalMeltMaxQty').val(maxQty);
            $('#maxMeltQtyLabel').text(maxQty);

            $('#modalMeltWg').val(meltWg);
            $('#modalMeltReasonId').val(meltDes).trigger('change');

            $currentInput = $row.find('input.melt_qty');
            $('#modal-edit-melt').modal('show');
            return;
        }

        if (type === 'lost') {
            currentForceRow = $row;

            $('#modalLostLotNo').val(lotNo);
            $('#modalLostQty').val(currentQty);
            $('#modalLostMaxQty').val(maxQty);
            $('#maxLostQtyLabel').text(maxQty);

            $currentInput = $row.find('input.lost_qty');
            $('#modal-edit-lost').modal('show');
            return;
        }

        if (type === 'export') {
            currentForceRow = $row;

            $('#modalExportLotNo').val(lotNo);
            $('#modalExportQty').val(currentQty);
            $('#modalExportMaxQty').val(maxQty);
            $('#maxExportQtyLabel').text(maxQty);

            $currentInput = $row.find('input.export_qty');
            $('#modal-edit-export').modal('show');
            return;
        }
    });

    $(document).on('input change', '#modalMeltQty', function () {
        const qty = parseFloat($(this).val()) || 0;
        const lotNo = $('#modalMeltLotNo').val();

        const $row = $(`#tbl-sendToStore-body tr[data-lot-no="${lotNo}"]`);

        const baseWg = calcWgFromQty($row, qty);
        const percentage = parseFloat($row.data('percentage')) || 0;

        const maxWg = +(baseWg + (percentage / 100 * baseWg)).toFixed(2);

        $('#modalMeltWg').val(baseWg.toFixed(2)).attr('max', maxWg);
        $('#modalMeltWg').siblings('label').html(`น้ำหนัก (กรัม) <span class="text-muted">(สูงสุด ${maxWg.toLocaleString(undefined, { maximumFractionDigits: 2 })})</span>`);
    });

    $(document).on('click', '#btnStoreQtyConfirm', function () {
        const draft = parseFloat($('#modalStoreQty').val()) || 0;
        const max = parseFloat($('#modalStoreMaxQty').val()) || 0;

        if (draft > max) {
            showWarning(`เกินยอดที่สามารถเก็บได้ (${max})`);
            return;
        }

        const $row = $currentInput.closest('tr');
        const fixed = getFixedQty($row, 'store');

        $row.data('store-draft-qty', draft);
        $row.find('input.store_qty').val(fixed + draft);

        const wg = calcWgFromQty($row, draft);
        $row.attr('data-store-wg', wg);

        updateAvailableQty($row);
        $('#modal-edit-store').modal('hide');
    });

    $(document).on('keydown', '#modalStoreQty', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnStoreQtyConfirm').click();
        }
    });

    $(document).on('click', '#btnMeltQtyConfirm', function () {
        const draft = parseFloat($('#modalMeltQty').val()) || 0;
        const max = parseFloat($('#modalMeltMaxQty').val()) || 0;
        const wg = parseFloat($('#modalMeltWg').val()) || 0;
        const reasonId = $('#modalMeltReasonId').val();

        if (draft > max) {
            showWarning(`เกินยอดที่สามารถหลอมได้ (${max})`);
            return;
        }
        if (draft > 0 && wg === 0) {
            showWarning(`กรุณากรอกน้ำหนัก`);
            return;
        }
        if (draft > 0 && !reasonId) {
            showWarning(`กรุณาเลือกสาเหตุ`);
            return;
        }

        const $row = $currentInput.closest('tr');
        const fixed = getFixedQty($row, 'melt');

        $row.data('melt-draft-qty', draft);
        $row.find('input.melt_qty').val(fixed + draft);

        $row.attr('data-melt-wg', wg);
        $row.attr('data-melt-des', reasonId);

        updateAvailableQty($row);
        $('#modal-edit-melt').modal('hide');
    });

    $(document).on('keydown', '#modalMeltQty', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnMeltQtyConfirm').click();
        }
    });

    $(document).on('keydown', '#modalMeltWg', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnMeltQtyConfirm').click();
        }
    });

    $(document).on('click', '#btnLostQtyConfirm', function () {
        const draft = parseFloat($('#modalLostQty').val()) || 0;
        const max = parseFloat($('#modalLostMaxQty').val()) || 0;

        if (draft > max) {
            showWarning(`เกินยอดที่สามารถหายได้ (${max})`);
            return;
        }

        const $row = $currentInput.closest('tr');
        const fixed = getFixedQty($row, 'lost');

        $row.data('lost-draft-qty', draft);
        $row.find('input.lost_qty').val(fixed + draft);

        const wg = calcWgFromQty($row, draft);
        $row.attr('data-lost-wg', wg);

        $row.removeAttr('data-force-user');

        updateAvailableQty($row);
        $('#modal-edit-lost').modal('hide');
    });

    $(document).on('keydown', '#modalLostQty', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnLostQtyConfirm').click();
        }
    });

    $(document).on('click', '#btnExportQtyConfirm', function () {
        const draft = parseFloat($('#modalExportQty').val()) || 0;
        const max = parseFloat($('#modalExportMaxQty').val()) || 0;

        if (draft > max) {
            showWarning(`เกินยอดที่สามารถส่งออกได้ (${max})`);
            return;
        }

        const $row = $currentInput.closest('tr');
        const fixed = getFixedQty($row, 'export');

        $row.data('export-draft-qty', draft);
        $row.find('input.export_qty').val(fixed + draft);

        const wg = calcWgFromQty($row, draft);
        $row.attr('data-export-wg', wg);

        $row.removeAttr('data-force-user');

        updateAvailableQty($row);
        $('#modal-edit-export').modal('hide');
    });

    $(document).on('keydown', '#modalExportQty', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnExportQtyConfirm').click();
        }
    });


    $(document).on('click', '.btn-edit', function () {
        const $row = $(this).closest('tr');

        $row.data('old-store', getDraftQty($row, 'store'));
        $row.data('old-melt', getDraftQty($row, 'melt'));
        $row.data('old-export', getDraftQty($row, 'export'));
        $row.data('old-lost', getDraftQty($row, 'lost'));

        $row.find('.btn-edit-qty').removeClass('d-none');
        $row.find('.btn-clear-qty').removeClass('d-none');

        $(this).addClass('d-none');
        $row.find('.btn-save, .btn-cancel').removeClass('d-none');
    });


    $(document).on('click', '.btn-cancel', function () {
        const $row = $(this).closest('tr');

        const sDraft = $row.data('old-store') || 0;
        const mDraft = $row.data('old-melt') || 0;
        const eDraft = $row.data('old-export') || 0;
        const lDraft = $row.data('old-lost') || 0;

        $row.data('store-draft-qty', sDraft);
        $row.data('melt-draft-qty', mDraft);
        $row.data('export-draft-qty', eDraft);
        $row.data('lost-draft-qty', lDraft);

        const sFix = getFixedQty($row, 'store');
        const mFix = getFixedQty($row, 'melt');
        const eFix = getFixedQty($row, 'export');
        const lFix = getFixedQty($row, 'lost');

        $row.find('input.store_qty').val(sFix + sDraft);
        $row.find('input.melt_qty').val(mFix + mDraft);
        $row.find('input.export_qty').val(eFix + eDraft);
        $row.find('input.lost_qty').val(lFix + lDraft);

        $row.find('.btn-edit-qty').addClass('d-none');
        $row.find('.btn-clear-qty').addClass('d-none');
        $row.find('.btn-save, .btn-cancel').addClass('d-none');
        $row.find('.btn-edit').removeClass('d-none');

        updateAvailableQty($row);
    });

    $(document).on("change", ".chk-row", function () {
        toggleConfirmButton();
    });

    $(document).on("change", "#chkSelectAllLotToStore", function () {
        const checked = $(this).is(":checked");
        $(".chk-row").prop("checked", checked);
        toggleConfirmButton();
    });

    $(document).off('click', '.btn-clear-qty').on('click', '.btn-clear-qty', function () {
        const $btn = $(this);
        const $row = $btn.closest('tr');
        const type = String($btn.data('type')).toLowerCase();

        const draftKey = `${type}-draft-qty`;

        const fixed = getFixedQty($row, type);

        $row.data(draftKey, 0);

        $row.find(`input.${type}_qty`).val(fixed);

        if (type === 'store') {
            $row.attr('data-store-wg', 0);
        }

        if (type === 'melt') {
            $row.attr('data-melt-wg', 0);
            $row.attr('data-melt-des', '');
            $row.find('.melt-reason-text').text('');
        }

        if (type === 'lost') {
            $row.attr('data-lost-wg', 0);
            $row.removeAttr('data-force-user');
        }

        if (type === 'export') {
            $row.attr('data-export-wg', 0);
            $row.removeAttr('data-force-user');
        }

        updateAvailableQty($row);
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

        const $exportInput = currentForceRow.find('input.export_qty');
        $exportInput.val(qty).trigger('change');

        currentForceRow.attr('data-force-user', userId);

        const exportWg = calcWgFromQty(currentForceRow, qty);
        currentForceRow.attr('data-export-wg', exportWg);

        if (typeof updateAvailableQty === 'function') {
            updateAvailableQty(currentForceRow);
        }

        $('#modal-force-send-to-export').modal('hide');
        $('#modal-edit-export').modal('hide');
    });

    $(document).on('hidden.bs.modal', '#modal-force-send-to-export', function () {
        $('#txtUsername').val('').removeClass('is-valid is-invalid is-warning').prop('disabled', false);
        $('#txtPassword').val('').removeClass('is-valid is-invalid is-warning').prop('disabled', false);
        $('#btnCheckAuthForceSendToExport').text('ตรวจสอบสิทธิ์').removeClass('btn-success btn-danger btn-warning').addClass('btn-primary').prop('disabled', false);
    });

    $(document).on('hidden.bs.modal', '#modal-edit-export', function () {
        currentForceRow = null;
    });

});

function FindOrderToStore() {
    $("#btnConfirmSendToStore").addClass("d-none");
    $("#btnPrintSendToStore").addClass("d-none");
    $("#btnSelect").addClass("d-none");
    $("#selectAllLotToStore").addClass("d-none");
    $("#btnUnselect").addClass("d-none");

    $('#loadingIndicator').show();
    const txtOrderNo = $('#txtOrderNo').val();
    const tbody = $('#tbl-sendToStore-body');
    tbody.empty().append('<tr><td colspan="14" class="text-center text-muted">กำลังโหลด...</td></tr>');

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
            tbody.append('<tr><td colspan="14" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
            return;
        }

        $("#btnSelect").removeClass("d-none");

        const BATCH_SIZE = 50;
        let currentIndex = 0;

        function renderBatch() {
            const fragment = document.createDocumentFragment();
            const endIndex = Math.min(currentIndex + BATCH_SIZE, response.length);

            for (let i = currentIndex; i < endIndex; i++) {
                const item = response[i];
                const available_qty = item.packed_Qty - (item.store_FixedQty + item.store_Qty) - (item.melt_FixedQty + item.melt_Qty) - (item.lost_FixedQty + item.lost_Qty) - (item.export_FixedQty + item.export_Qty);

                const tr = document.createElement('tr');
                tr.setAttribute('data-lot-no', item.lotNo || '');
                tr.setAttribute('data-ttwg', item.ttWg || 0);
                tr.setAttribute('data-percentage', item.percentage || 0);
                tr.setAttribute('data-sendtopack-qty', item.sendToPack_Qty || 0);

                tr.setAttribute('data-store-fixed-qty', item.store_FixedQty || 0);
                tr.setAttribute('data-store-fixed-wg', item.store_FixedWg || 0);
                tr.setAttribute('data-store-draft-qty', item.store_Qty || 0);
                tr.setAttribute('data-store-wg', item.store_Wg || 0);

                tr.setAttribute('data-melt-fixed-qty', item.melt_FixedQty || 0);
                tr.setAttribute('data-melt-fixed-wg', item.melt_FixedWg || 0);
                tr.setAttribute('data-melt-draft-qty', item.melt_Qty || 0);
                tr.setAttribute('data-melt-wg', item.melt_Wg || 0);
                tr.setAttribute('data-melt-des', item.breakDescriptionId || '');

                tr.setAttribute('data-export-fixed-qty', item.export_FixedQty || 0);
                tr.setAttribute('data-export-fixed-wg', item.export_FixedWg || 0);
                tr.setAttribute('data-export-draft-qty', item.export_Qty || 0);
                tr.setAttribute('data-export-wg', item.export_Wg || 0);

                tr.setAttribute('data-lost-fixed-qty', item.lost_FixedQty || 0);
                tr.setAttribute('data-lost-fixed-wg', item.lost_FixedWg || 0);
                tr.setAttribute('data-lost-draft-qty', item.lost_Qty || 0);
                tr.setAttribute('data-lost-wg', item.lost_Wg || 0);

                tr.setAttribute('data-available-qty', available_qty);

                tr.innerHTML = `
                    <td class="text-center">${i + 1}</td>
                    <td class="text-start"><strong>${html(item.article)}</strong></br><small>${html(item.custCode)}/${html(item.orderNo)}</small></td>
                    <td class="text-center">${html(item.listNo)}</td>
                    <td class="text-end">${item.ttQty != 0 ? numRaw(item.ttQty) : '-'}</td>
                    <td class="text-end">${item.si != 0 ? numRaw(item.si) : '-'}</td>
                    <td class="text-end">${item.sendPack_Qty != 0 ? numRaw(item.sendPack_Qty) : '-'}</td>
                    <td class="text-end">${item.sendToPack_Qty != 0 ? numRaw(item.sendToPack_Qty) : '-'}</td>
                    <td class="text-end col-packed">${ item.packed_Qty != 0 ? numRaw(item.packed_Qty) : '-'}</td>

                    <td class="text-center col-available fs-5"><strong>${available_qty != 0 ? numRaw(available_qty) : '-'}</strong></td>

                    <td class="text-end">
                        <div class="d-flex flex-column align-items-end">
                            <div class="d-flex justify-content-between align-content-center gap-2 w-100">
                                <input class="form-control text-center qty-input store_qty fw-bold fs-5"
                                       type="number"
                                       min="0"
                                       step="any"
                                       value="${numRaw(item.store_Qty + item.store_FixedQty)}"
                                       readonly />

                                <button class="btn btn-default btn-edit-qty d-none" data-type="store">
                                    <i class="fas fa-plus"></i>
                                </button>

                            </div>
                            <small class="text-muted pe-1"><i class="fas fa-download"></i> : <strong class="text-info">${numRaw(item.store_FixedQty)}</strong> / <i class="fas fa-marker"></i> : <strong class="text-warning">${numRaw(item.store_Qty)}</strong></small>
                        </div>
                    </td>

                    <td class="text-end">
                        <div class="d-flex flex-column align-items-end">
                            <div class="d-flex justify-content-between align-content-center gap-2 w-100">
                                <input class="form-control text-center qty-input melt_qty fw-bold fs-5"
                                       type="number"
                                       min="0"
                                       step="any"
                                       value="${numRaw(item.melt_Qty + item.melt_FixedQty)}"
                                       readonly />

                                <button class="btn btn-default btn-edit-qty d-none" data-type="melt">
                                    <i class="fas fa-plus"></i>
                                </button>

                            </div>
                            <small class="text-muted pe-1"><i class="fas fa-download"></i> : <strong class="text-info">${numRaw(item.melt_FixedQty)}</strong> / <i class="fas fa-marker"></i> : <strong class="text-warning">${numRaw(item.melt_Qty)}</strong></small>
                        </div>
                    </td>

                    <td class="text-end">
                        <div class="d-flex flex-column align-items-end">
                            <div class="d-flex justify-content-between align-content-center gap-2 w-100">
                                <input class="form-control text-center qty-input lost_qty fw-bold fs-5"
                                       type="number"
                                       min="0"
                                       step="any"
                                       value="${numRaw(item.lost_Qty + item.lost_FixedQty)}"
                                       readonly />

                                <button class="btn btn-default btn-edit-qty d-none" data-type="lost">
                                    <i class="fas fa-plus"></i>
                                </button>

                            </div>
                            <small class="text-muted pe-1"><i class="fas fa-download"></i> : <strong class="text-info">${numRaw(item.lost_FixedQty)}</strong> / <i class="fas fa-marker"></i> : <strong class="text-warning">${numRaw(item.lost_Qty)}</strong></small>
                        </div>
                    </td>

                    <td class="text-end">
                        <div class="d-flex flex-column align-items-end">
                            <div class="d-flex justify-content-between align-content-center gap-2 w-100">
                                <input class="form-control text-center qty-input export_qty fw-bold fs-5"
                                       type="number"
                                       min="0"
                                       step="any"
                                       value="${numRaw(item.export_Qty + item.export_FixedQty)}"
                                       readonly />

                                <button class="btn btn-default btn-edit-qty d-none" data-type="export">
                                    <i class="fas fa-plus"></i>
                                </button>

                            </div>
                            <small class="text-muted pe-1"><i class="fas fa-download"></i> : <strong class="text-info">${numRaw(item.export_FixedQty)}</strong> / <i class="fas fa-marker"></i> : <strong class="text-warning">${numRaw(item.export_Qty)}</strong></small>
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
                `;

                fragment.appendChild(tr);
            }

            tbody[0].appendChild(fragment);
            currentIndex = endIndex;

            if (currentIndex < response.length) {
                requestAnimationFrame(renderBatch);
            }
        }

        renderBatch();
    })
    .fail(function (error) {
        $('#loadingIndicator').hide();
        console.error('Error fetching data:', error);
        tbody.empty().append('<tr><td colspan="14" class="text-center text-danger">เกิดข้อผิดพลาดในการโหลดข้อมูล</td></tr>');
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
            FindOrderToStore();
            await showSuccess(`${res.message}`);
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

    let pdfWindow = window.open('', '_blank');

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

function toggleConfirmButton() {
    const anyChecked = $(".chk-row:checked").length > 0;
    $("#btnConfirmSendToStore").toggleClass("d-none", !anyChecked);
    $("#btnPrintSendToStore").toggleClass("d-none", !anyChecked);
}

function showSelection() {
    $("#selectAllLotToStore").removeClass("d-none");
    $(".chk-wrapper").removeClass("d-none");
    $("#btnUnselect").removeClass("d-none");
    $("#btnSelect").addClass("d-none");
    $(".btn-edit").addClass("d-none");
    $(".btn-save").addClass("d-none");
    $(".btn-cancel").addClass("d-none");
}

function hideSelection() {
    $("#selectAllLotToStore").addClass("d-none");
    $(".chk-wrapper").addClass("d-none");
    $(".chk-row").prop("checked", false);
    $("#chkSelectAllLotToStore").prop("checked", false);
    $("#btnConfirmSendToStore").addClass("d-none");
    $("#btnPrintSendToStore").addClass("d-none");
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
    const available = calculateAvailable($row);

    const formatted = available.toLocaleString(undefined, {
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
    });

    $row.find('.col-available').text(formatted == 0 ? '-' : formatted);
    $row.attr('data-available-qty', available);


    //if (available < 0) {
    //    $row.find('.col-available').addClass('text-danger fw-bold fs-5');
    //} else {$row
    //    $row.find('.col-available').removeClass('text-danger fw-bold fs-5');
    //}
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

    txtUsername
        .val('')
        .removeClass('is-invalid is-warning is-valid')
        .prop('disabled', false);
    txtPassword
        .val('')
        .removeClass('is-invalid is-warning is-valid')
        .prop('disabled', false);

    btn
        .removeClass('btn-success')
        .addClass('btn-primary')
        .text('ตรวจสอบสิทธิ์')
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

function getDraftQty($row, type) {
    const key = `${type}-draft-qty`;
    const storedDraft = $row.data(key);

    if (storedDraft !== undefined && storedDraft !== null) {
        return parseFloat(storedDraft) || 0;
    }

    const total = parseFloat($row.find(`input.${type}_qty`).val()) || 0;
    const fixed = getFixedQty($row, type);

    const draft = total - fixed;
    return draft > 0 ? draft : 0;
}


function getFixedQty($row, type) {
    if (type === 'store') return parseFloat($row.data('store-fixed-qty')) || 0;
    if (type === 'melt') return parseFloat($row.data('melt-fixed-qty')) || 0;
    if (type === 'export') return parseFloat($row.data('export-fixed-qty')) || 0;
    if (type === 'lost') return parseFloat($row.data('lost-fixed-qty')) || 0;
    return 0;
}

function getPackedQty($row) {
    return parseFloat($row.find('.col-packed').text().replace(/,/g, '')) || 0;
}

function calculateAvailable($row) {
    const packed = getPackedQty($row);

    const sFix = getFixedQty($row, 'store');
    const mFix = getFixedQty($row, 'melt');
    const eFix = getFixedQty($row, 'export');
    const lFix = getFixedQty($row, 'lost');

    const sDrf = getDraftQty($row, 'store');
    const mDrf = getDraftQty($row, 'melt');
    const eDrf = getDraftQty($row, 'export');
    const lDrf = getDraftQty($row, 'lost');

    const available = packed - (sFix + mFix + eFix + lFix + sDrf + mDrf + eDrf + lDrf);
    return available;
}

function calculateMaxQty($row, type) {

    const packed = getPackedQty($row);

    const sFix = getFixedQty($row, 'store');
    const mFix = getFixedQty($row, 'melt');
    const eFix = getFixedQty($row, 'export');
    const lFix = getFixedQty($row, 'lost');

    const sDrf = getDraftQty($row, 'store');
    const mDrf = getDraftQty($row, 'melt');
    const eDrf = getDraftQty($row, 'export');
    const lDrf = getDraftQty($row, 'lost');

    const currentDraft = getDraftQty($row, type);

    const available = packed - (sFix + mFix + eFix + lFix + sDrf + mDrf + eDrf + lDrf) + currentDraft;

    if (type === 'export') {

        const sendToPack = parseFloat($row.data('sendtopack-qty')) || 0;

        if (sendToPack !== 0) {
            const exportRoom = sendToPack - eFix + currentDraft;
            return Math.max(Math.min(available, exportRoom), 0);
        }

        return Math.max(available, 0);
    }

    return Math.max(available, 0);
}

