let currentForceRow = null;
let $currentInput = null;

$(document).ready(function () {
    $(document).on('keydown', '#txtOrderNo', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            FindOrderToStore();
        }
    });

    $(document).on('keydown', '#txtGoToListNo', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            goToListNo();
        }
    });

    $(document).on('keydown', '#txtGoToListNoFloat', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            goToListNoFloat();
        }
    });

    $(document).on('click', '.btn-edit', function () {
        const $dataRow = $(this).closest('tr');
        const lotNo = $dataRow.data('lot-no');
        const $inputRow = $(`#tbl-sendToStore-body tr.input-row[data-lot-no="${lotNo}"]`);

        // Show edit buttons in input row
        $inputRow.find('.btn-edit-qty').removeClass('d-none');
        $inputRow.find('.btn-clear-qty').removeClass('d-none');

        // Toggle edit/save buttons in data row
        $(this).addClass('d-none');
        $dataRow.find('.btn-save').removeClass('d-none');
        $dataRow.find('.btn-cancel').removeClass('d-none');
    });


    $(document).on('click', '.btn-save', function () {
        const uid = $('#hddUserID').val();
        const $dataRow = $(this).closest('tr');
        const lotNo = $dataRow.data('lot-no');
        const $inputRow = $(`#tbl-sendToStore-body tr.input-row[data-lot-no="${lotNo}"]`);

        const store_qty = getDraftQty($dataRow, 'store');
        const melt_qty = getDraftQty($dataRow, 'melt');
        const export_qty = getDraftQty($dataRow, 'export');
        const lost_qty = getDraftQty($dataRow, 'lost');

        const store_wg = calcWgFromQty($dataRow, store_qty);
        const melt_wg = parseFloat($dataRow.attr('data-melt-wg')) || 0;
        const export_wg = calcWgFromQty($dataRow, export_qty);
        const lost_wg = calcWgFromQty($dataRow, lost_qty);

        const melt_des = parseInt($dataRow.attr('data-melt-des')) || 0;
        const force_send_by = parseInt($dataRow.attr('data-force-user')) || 0;

        const available = calculateAvailable($dataRow);

        // Hide edit buttons in both rows
        $(this).addClass('d-none');
        $dataRow.find('.btn-edit').removeClass('d-none');
        $dataRow.find('.btn-save, .btn-cancel').addClass('d-none');
        $inputRow.find('.btn-edit-qty').addClass('d-none');
        $inputRow.find('.btn-clear-qty').addClass('d-none');

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
                    swalToastSuccess(`บันทึกสำเร็จ`);
                } else {
                    swalToastWarning(
                        `เกิดข้อผิดพลาด (${res.code}) ${res.message})`,
                    );
                }
            },
            error: async function (xhr) {
                swalToastWarning(`เกิดข้อผิดพลาด (${xhr.status})`);
            }
        }).done(function (res) {
            if (res.isSuccess) {
                reloadRow(lotNo);
            }
        });
    });

    $(document).on('click', '.btn-edit-qty', function () {
        const $btn = $(this);
        const $inputRow = $btn.closest('tr');
        const type = $btn.data('type');
        const lotNo = $inputRow.data('lot-no');
        const $dataRow = $(`#tbl-sendToStore-body tr.data-row[data-lot-no="${lotNo}"]`);

        const currentQty = getDraftQty($dataRow, type);
        const maxQty = calculateMaxQty($dataRow, type);

        if (type === 'store') {
            $('#modalStoreLotNo').val(lotNo);
            $('#modalStoreQty').val(currentQty);
            $('#modalStoreMaxQty').val(maxQty);
            $('#maxStoreQtyLabel').text(maxQty);

            $currentInput = $inputRow.find('input.store_qty');
            $('#modal-edit-store').modal('show');
            return;
        }

        if (type === 'melt') {
            const meltWg = parseFloat($dataRow.data('melt-wg')) || 0;
            const meltDes = $dataRow.data('melt-des') || "";

            $('#modalMeltLotNo').val(lotNo);
            $('#modalMeltQty').val(currentQty);
            $('#modalMeltMaxQty').val(maxQty);
            $('#maxMeltQtyLabel').text(maxQty);

            $('#modalMeltWg').val(meltWg);
            $('#modalMeltReasonId').val(meltDes).trigger('change');

            $currentInput = $inputRow.find('input.melt_qty');
            $('#modal-edit-melt').modal('show');
            return;
        }

        if (type === 'lost') {
            currentForceRow = $dataRow;

            $('#modalLostLotNo').val(lotNo);
            $('#modalLostQty').val(currentQty);
            $('#modalLostMaxQty').val(maxQty);
            $('#maxLostQtyLabel').text(maxQty);

            $currentInput = $inputRow.find('input.lost_qty');
            $('#modal-edit-lost').modal('show');
            return;
        }

        if (type === 'export') {
            currentForceRow = $dataRow;

            $('#modalExportLotNo').val(lotNo);
            $('#modalExportQty').val(currentQty);
            $('#modalExportMaxQty').val(maxQty);
            $('#maxExportQtyLabel').text(maxQty);

            $currentInput = $inputRow.find('input.export_qty');
            $('#modal-edit-export').modal('show');
            return;
        }
    });

    $(document).on('input change', '#modalMeltQty', function () {
        const qty = parseFloat($(this).val()) || 0;
        const lotNo = $('#modalMeltLotNo').val();

        const $row = $(`#tbl-sendToStore-body tr.data-row[data-lot-no="${lotNo}"]`);

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
            swalWarning(`เกินยอดที่สามารถเก็บได้ (${max})`);
            return;
        }

        const $inputRow = $currentInput.closest('tr');
        const lotNo = $inputRow.data('lot-no');
        const $dataRow = $(`#tbl-sendToStore-body tr.data-row[data-lot-no="${lotNo}"]`);

        const fixed = getFixedQty($dataRow, 'store');

        $dataRow.data('store-draft-qty', draft);
        $inputRow.find('input.store_qty').val(fixed + draft);

        const wg = calcWgFromQty($dataRow, draft);
        $dataRow.attr('data-store-wg', wg);

        updateAvailableQty($dataRow);
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
            swalWarning(`เกินยอดที่สามารถหลอมได้ (${max})`);
            return;
        }
        if (draft > 0 && wg === 0) {
            swalWarning(`กรุณากรอกน้ำหนัก`);
            return;
        }
        if (draft > 0 && !reasonId) {
            swalWarning(`กรุณาเลือกสาเหตุ`);
            return;
        }

        const $inputRow = $currentInput.closest('tr');
        const lotNo = $inputRow.data('lot-no');
        const $dataRow = $(`#tbl-sendToStore-body tr.data-row[data-lot-no="${lotNo}"]`);

        const fixed = getFixedQty($dataRow, 'melt');

        $dataRow.data('melt-draft-qty', draft);
        $inputRow.find('input.melt_qty').val(fixed + draft);

        $dataRow.attr('data-melt-wg', wg);
        $dataRow.attr('data-melt-des', reasonId);

        updateAvailableQty($dataRow);
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
            swalWarning(`เกินยอดที่สามารถหายได้ (${max})`);
            return;
        }

        const $inputRow = $currentInput.closest('tr');
        const lotNo = $inputRow.data('lot-no');
        const $dataRow = $(`#tbl-sendToStore-body tr.data-row[data-lot-no="${lotNo}"]`);

        const fixed = getFixedQty($dataRow, 'lost');

        $dataRow.data('lost-draft-qty', draft);
        $inputRow.find('input.lost_qty').val(fixed + draft);

        const wg = calcWgFromQty($dataRow, draft);
        $dataRow.attr('data-lost-wg', wg);

        $dataRow.removeAttr('data-force-user');

        updateAvailableQty($dataRow);
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
            swalWarning(`เกินยอดที่สามารถส่งออกได้ (${max})`);
            return;
        }

        const $inputRow = $currentInput.closest('tr');
        const lotNo = $inputRow.data('lot-no');
        const $dataRow = $(`#tbl-sendToStore-body tr.data-row[data-lot-no="${lotNo}"]`);

        const fixed = getFixedQty($dataRow, 'export');

        $dataRow.data('export-draft-qty', draft);
        $inputRow.find('input.export_qty').val(fixed + draft);

        const wg = calcWgFromQty($dataRow, draft);
        $dataRow.attr('data-export-wg', wg);

        $dataRow.removeAttr('data-force-user');

        updateAvailableQty($dataRow);
        $('#modal-edit-export').modal('hide');
    });

    $(document).on('keydown', '#modalExportQty', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnExportQtyConfirm').click();
        }
    });


    $(document).on('click', '.btn-edit', function () {
        const $dataRow = $(this).closest('tr');
        const lotNo = $dataRow.data('lot-no');
        const $inputRow = $(`#tbl-sendToStore-body tr.input-row[data-lot-no="${lotNo}"]`);

        // Store old values for cancel
        $dataRow.data('old-store', getDraftQty($dataRow, 'store'));
        $dataRow.data('old-melt', getDraftQty($dataRow, 'melt'));
        $dataRow.data('old-export', getDraftQty($dataRow, 'export'));
        $dataRow.data('old-lost', getDraftQty($dataRow, 'lost'));

        // Show edit buttons in input row
        $inputRow.find('.btn-edit-qty').removeClass('d-none');
        $inputRow.find('.btn-clear-qty').removeClass('d-none');

        // Toggle buttons in data row
        $(this).addClass('d-none');
        $dataRow.find('.btn-save, .btn-cancel').removeClass('d-none');
    });


    $(document).on('click', '.btn-cancel', function () {
        const $dataRow = $(this).closest('tr');
        const lotNo = $dataRow.data('lot-no');
        const $inputRow = $(`#tbl-sendToStore-body tr.input-row[data-lot-no="${lotNo}"]`);

        // Restore old draft values
        const sDraft = $dataRow.data('old-store') || 0;
        const mDraft = $dataRow.data('old-melt') || 0;
        const eDraft = $dataRow.data('old-export') || 0;
        const lDraft = $dataRow.data('old-lost') || 0;

        $dataRow.data('store-draft-qty', sDraft);
        $dataRow.data('melt-draft-qty', mDraft);
        $dataRow.data('export-draft-qty', eDraft);
        $dataRow.data('lost-draft-qty', lDraft);

        const sFix = getFixedQty($dataRow, 'store');
        const mFix = getFixedQty($dataRow, 'melt');
        const eFix = getFixedQty($dataRow, 'export');
        const lFix = getFixedQty($dataRow, 'lost');

        // Update inputs in input-row
        $inputRow.find('input.store_qty').val(sFix + sDraft);
        $inputRow.find('input.melt_qty').val(mFix + mDraft);
        $inputRow.find('input.export_qty').val(eFix + eDraft);
        $inputRow.find('input.lost_qty').val(lFix + lDraft);

        // Hide edit buttons in input row
        $inputRow.find('.btn-edit-qty').addClass('d-none');
        $inputRow.find('.btn-clear-qty').addClass('d-none');

        // Toggle buttons in data row
        $dataRow.find('.btn-save, .btn-cancel').addClass('d-none');
        $dataRow.find('.btn-edit').removeClass('d-none');

        updateAvailableQty($dataRow);
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
        const $inputRow = $btn.closest('tr');
        const type = String($btn.data('type')).toLowerCase();
        const lotNo = $inputRow.data('lot-no');
        const $dataRow = $(`#tbl-sendToStore-body tr.data-row[data-lot-no="${lotNo}"]`);

        const draftKey = `${type}-draft-qty`;

        const fixed = getFixedQty($dataRow, type);

        $dataRow.data(draftKey, 0);

        $inputRow.find(`input.${type}_qty`).val(fixed);

        if (type === 'store') {
            $dataRow.attr('data-store-wg', 0);
        }

        if (type === 'melt') {
            $dataRow.attr('data-melt-wg', 0);
            $dataRow.attr('data-melt-des', '');
            $inputRow.find('.melt-reason-text').text('');
        }

        if (type === 'lost') {
            $dataRow.attr('data-lost-wg', 0);
            $dataRow.removeAttr('data-force-user');
        }

        if (type === 'export') {
            $dataRow.attr('data-export-wg', 0);
            $dataRow.removeAttr('data-force-user');
        }

        updateAvailableQty($dataRow);
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
            swalWarning('ไม่พบแถวที่จะอัปเดต');
            return;
        }

        const userId = $('#hddUserID').val();
        const qtyRaw = $('#txtforce-adjust-qty').val();
        const qty = parseFloat(qtyRaw);

        if (!userId) {
            swalWarning('กรุณาตรวจสอบสิทธิ์ก่อน (ตรวจสอบผู้ใช้และรหัสผ่าน)');
            return;
        }

        if (isNaN(qty) || qty < 0) {
            swalWarning('กรุณากรอกจำนวนที่ถูกต้อง');
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

    // Floating action buttons - simple scroll sync
    window.addEventListener('scroll', function () {
        var scrollTop = window.scrollY || document.documentElement.scrollTop;
        var floatingActions = document.getElementById('floatingActions');

        if (!floatingActions) return;

        var hasVisibleButton = document.querySelectorAll('.action-btn:not(.d-none)').length > 0;

        if (scrollTop > 200 && hasVisibleButton) {
            floatingActions.style.display = 'flex';
            syncFloatingBtn('btnSelect', 'btnSelectFloat');
            syncFloatingBtn('btnConfirmSendToStore', 'btnConfirmFloat');
            syncFloatingBtn('btnPrintSendToStore', 'btnPrintFloat');
            syncFloatingBtn('btnUnselect', 'btnUnselectFloat');
        } else {
            floatingActions.style.display = 'none';
        }
    });

    function syncFloatingBtn(originalId, floatId) {
        var original = document.getElementById(originalId);
        var floatBtn = document.getElementById(floatId);
        if (original && floatBtn) {
            if (original.classList.contains('d-none')) {
                floatBtn.classList.add('hidden');
            } else {
                floatBtn.classList.remove('hidden');
            }
        }
    }
});

function FindOrderToStore() {
    $("#btnConfirmSendToStore").addClass("d-none");
    $("#btnConfirmFloat").addClass("d-none");
    $("#btnPrintSendToStore").addClass("d-none");
    $("#btnSelect").addClass("d-none");
    $("#divGoToListNo").addClass("d-none");
    $("#selectAllLotToStore").addClass("d-none");
    $("#btnUnselect").addClass("d-none");

    $('#loadingIndicator').show();
    const txtOrderNo = $('#txtOrderNo').val();
    const tbody = $('#tbl-sendToStore-body');
    tbody.empty().append('<tr><td colspan="10" class="text-center text-muted">กำลังโหลด...</td></tr>');

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
                tbody.append('<tr><td colspan="10" class="text-center text-muted">ไม่พบข้อมูล</td></tr>');
                return;
            }

            $("#btnSelect").removeClass("d-none");
            $("#divGoToListNo").removeClass("d-none");

            const BATCH_SIZE = 50;
            let currentIndex = 0;

            function renderBatch() {
                const fragment = document.createDocumentFragment();
                const endIndex = Math.min(currentIndex + BATCH_SIZE, response.length);

                for (let i = currentIndex; i < endIndex; i++) {
                    const item = response[i];
                    const available_qty = item.packed_Qty - (item.store_FixedQty + item.store_Qty) - (item.melt_FixedQty + item.melt_Qty) - (item.lost_FixedQty + item.lost_Qty) - (item.export_FixedQty + item.export_Qty);

                    // First row - data row (basic info + buttons)
                    const tr1 = document.createElement('tr');
                    tr1.className = 'data-row';
                    tr1.setAttribute('data-lot-no', item.lotNo || '');
                    tr1.setAttribute('data-ttwg', item.ttWg || 0);
                    tr1.setAttribute('data-percentage', item.percentage || 0);
                    tr1.setAttribute('data-sendtopack-qty', item.sendToPack_Qty || 0);

                    tr1.setAttribute('data-store-fixed-qty', item.store_FixedQty || 0);
                    tr1.setAttribute('data-store-fixed-wg', item.store_FixedWg || 0);
                    tr1.setAttribute('data-store-draft-qty', item.store_Qty || 0);
                    tr1.setAttribute('data-store-wg', item.store_Wg || 0);

                    tr1.setAttribute('data-melt-fixed-qty', item.melt_FixedQty || 0);
                    tr1.setAttribute('data-melt-fixed-wg', item.melt_FixedWg || 0);
                    tr1.setAttribute('data-melt-draft-qty', item.melt_Qty || 0);
                    tr1.setAttribute('data-melt-wg', item.melt_Wg || 0);
                    tr1.setAttribute('data-melt-des', item.breakDescriptionId || '');

                    tr1.setAttribute('data-export-fixed-qty', item.export_FixedQty || 0);
                    tr1.setAttribute('data-export-fixed-wg', item.export_FixedWg || 0);
                    tr1.setAttribute('data-export-draft-qty', item.export_Qty || 0);
                    tr1.setAttribute('data-export-wg', item.export_Wg || 0);

                    tr1.setAttribute('data-lost-fixed-qty', item.lost_FixedQty || 0);
                    tr1.setAttribute('data-lost-fixed-wg', item.lost_FixedWg || 0);
                    tr1.setAttribute('data-lost-draft-qty', item.lost_Qty || 0);
                    tr1.setAttribute('data-lost-wg', item.lost_Wg || 0);

                    tr1.setAttribute('data-available-qty', available_qty);

                    tr1.innerHTML = `
                    <td class="text-center" rowspan="2">${i + 1}</td>
                    <td class="text-center"><strong>${html(item.article)}</strong></br><small>${html(item.custCode)}/${html(item.orderNo)}</small></td>
                    <td class="text-center"><strong>${html(item.listNo)}</strong></td>
                    <td class="text-center"><strong>${item.ttQty != 0 ? numRaw(item.ttQty) : '-'}</strong></td>
                    <td class="text-center"><strong>${item.si != 0 ? numRaw(item.si) : '-'}</strong></td>
                    <td class="text-center"><strong>${item.sendPack_Qty != 0 ? numRaw(item.sendPack_Qty) : '-'}</strong></td>
                    <td class="text-center"><strong>${item.sendToPack_Qty != 0 ? numRaw(item.sendToPack_Qty) : '-'}</strong></td>
                    <td class="text-center col-packed"><strong>${item.packed_Qty != 0 ? numRaw(item.packed_Qty) : '-'}</strong></td>
                    <td class="text-center col-available fs-5"><strong>${available_qty != 0 ? numRaw(available_qty) : '-'}</strong></td>
                    <td class="text-center" rowspan="2">
                        <div class="d-flex gap-1 justify-content-center">
                            <button class="btn btn-sm btn-warning btn-edit"><i class="fas fa-edit"></i></button>
                            <button class="btn btn-sm btn-success btn-save d-none"><i class="fas fa-save"></i></button>
                            <button class="btn btn-sm btn-danger btn-cancel d-none"><i class="fas fa-times"></i></button>
                            <div id="${item.lotNo}_ss${i}" class="chk-wrapper d-none">
                                <input type="checkbox" id="${item.lotNo}_rt${i}" class="chk-row custom-checkbox">
                                <label for="${item.lotNo}_rt${i}" class="d-none"></label>
                            </div>
                        </div>
                    </td>
                `;

                    // Second row - input row (inputs only, no labels)
                    const tr2 = document.createElement('tr');
                    tr2.className = 'input-row';
                    tr2.setAttribute('data-lot-no', item.lotNo || '');
                    tr2.setAttribute('data-parent-row', 'true');

                    tr2.innerHTML = `
                    <td colspan="8" class="py-2">
                        <div class="d-flex justify-content-around align-items-center gap-3 flex-wrap">
                            <!-- เก็บ -->
                            <div class="input-group-col text-center">
                                <div class="d-flex align-items-center gap-1 justify-content-center">
                                    <span class="badge bg-primary"><i class="fas fa-warehouse"></i> เก็บ</span>
                                    <input class="form-control form-control-sm text-center qty-input store_qty fw-bold"
                                           type="number" min="0" step="any" style="width: 180px;"
                                           value="${numRaw(item.store_Qty + item.store_FixedQty)}" readonly />
                                    <button class="btn btn-sm btn-outline-primary btn-edit-qty d-none" data-type="store">
                                        <i class="fas fa-plus"></i>
                                    </button>
                                    <button class="btn btn-sm btn-outline-secondary btn-clear-qty d-none" data-type="store">
                                        <i class="fas fa-eraser"></i>
                                    </button>
                                </div>
                                <small class="text-muted"><i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.store_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.store_Qty)}</strong></small>
                            </div>

                            <!-- หลอม -->
                            <div class="input-group-col text-center">
                                <div class="d-flex align-items-center gap-1 justify-content-center">
                                    <span class="badge bg-warning text-dark"><i class="fas fa-fire"></i> หลอม</span>
                                    <input class="form-control form-control-sm text-center qty-input melt_qty fw-bold"
                                           type="number" min="0" step="any" style="width: 180px;"
                                           value="${numRaw(item.melt_Qty + item.melt_FixedQty)}" readonly />
                                    <button class="btn btn-sm btn-outline-warning btn-edit-qty d-none" data-type="melt">
                                        <i class="fas fa-plus"></i>
                                    </button>
                                    <button class="btn btn-sm btn-outline-secondary btn-clear-qty d-none" data-type="melt">
                                        <i class="fas fa-eraser"></i>
                                    </button>
                                </div>
                                <small class="text-muted"><i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.melt_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.melt_Qty)}</strong></small>
                            </div>

                            <!-- หาย -->
                            <div class="input-group-col text-center">
                                <div class="d-flex align-items-center gap-1 justify-content-center">
                                    <span class="badge bg-danger"><i class="fas fa-question-circle"></i> หาย</span>
                                    <input class="form-control form-control-sm text-center qty-input lost_qty fw-bold"
                                           type="number" min="0" step="any" style="width: 180px;"
                                           value="${numRaw(item.lost_Qty + item.lost_FixedQty)}" readonly />
                                    <button class="btn btn-sm btn-outline-danger btn-edit-qty d-none" data-type="lost">
                                        <i class="fas fa-plus"></i>
                                    </button>
                                    <button class="btn btn-sm btn-outline-secondary btn-clear-qty d-none" data-type="lost">
                                        <i class="fas fa-eraser"></i>
                                    </button>
                                </div>
                                <small class="text-muted"><i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.lost_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.lost_Qty)}</strong></small>
                            </div>

                            <!-- ส่งออก -->
                            <div class="input-group-col text-center">
                                <div class="d-flex align-items-center gap-1 justify-content-center">
                                    <span class="badge bg-success"><i class="fas fa-truck"></i> ส่งออก</span>
                                    <input class="form-control form-control-sm text-center qty-input export_qty fw-bold"
                                           type="number" min="0" step="any" style="width: 180px;"
                                           value="${numRaw(item.export_Qty + item.export_FixedQty)}" readonly />
                                    <button class="btn btn-sm btn-outline-success btn-edit-qty d-none" data-type="export">
                                        <i class="fas fa-plus"></i>
                                    </button>
                                    <button class="btn btn-sm btn-outline-secondary btn-clear-qty d-none" data-type="export">
                                        <i class="fas fa-eraser"></i>
                                    </button>
                                </div>
                                <small class="text-muted"><i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.export_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.export_Qty)}</strong></small>
                            </div>
                        </div>
                    </td>
                `;

                    fragment.appendChild(tr1);
                    fragment.appendChild(tr2);
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
            tbody.empty().append('<tr><td colspan="10" class="text-center text-danger">เกิดข้อผิดพลาดในการโหลดข้อมูล</td></tr>');
        });
}

function updateRowData(tr, item, keepChecked) {
    const available_qty = item.packed_Qty - (item.store_FixedQty + item.store_Qty) - (item.melt_FixedQty + item.melt_Qty) - (item.lost_FixedQty + item.lost_Qty) - (item.export_FixedQty + item.export_Qty);
    const rowIndex = $(tr).find('td:first').text();
    const lotNo = item.lotNo;

    // Update data-row attributes
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

    // Update data-row HTML (basic info only)
    tr.innerHTML = `
        <td class="text-center" rowspan="2">${rowIndex}</td>
        <td class="text-center"><strong>${html(item.article)}</strong></br><small>${html(item.custCode)}/${html(item.orderNo)}</small></td>
        <td class="text-center"><strong>${html(item.listNo)}</strong></td>
        <td class="text-center"><strong>${item.ttQty != 0 ? numRaw(item.ttQty) : '-'}</strong></td>
        <td class="text-center"><strong>${item.si != 0 ? numRaw(item.si) : '-'}</strong></td>
        <td class="text-center"><strong>${item.sendPack_Qty != 0 ? numRaw(item.sendPack_Qty) : '-'}</strong></td>
        <td class="text-center"><strong>${item.sendToPack_Qty != 0 ? numRaw(item.sendToPack_Qty) : '-'}</strong></td>
        <td class="text-center col-packed"><strong>${item.packed_Qty != 0 ? numRaw(item.packed_Qty) : '-'}</strong></td>
        <td class="text-center col-available fs-5"><strong>${available_qty != 0 ? numRaw(available_qty) : '-'}</strong></td>
        <td class="text-center" rowspan="2">
            <div class="d-flex gap-1 justify-content-center">
                <button class="btn btn-sm btn-warning btn-edit${keepChecked ? ' d-none' : ''}"><i class="fas fa-edit"></i></button>
                <button class="btn btn-sm btn-success btn-save d-none"><i class="fas fa-save"></i></button>
                <button class="btn btn-sm btn-danger btn-cancel d-none"><i class="fas fa-times"></i></button>
                <div id="${item.lotNo}_ss${rowIndex}" class="chk-wrapper${keepChecked ? '' : ' d-none'}">
                    <input type="checkbox" id="${item.lotNo}_rt${rowIndex}" class="chk-row custom-checkbox"${keepChecked ? ' checked' : ''}>
                    <label for="${item.lotNo}_rt${rowIndex}" class="d-none"></label>
                </div>
            </div>
        </td>
    `;

    // Find and update input-row
    const inputRow = document.querySelector(`#tbl-sendToStore-body tr.input-row[data-lot-no="${lotNo}"]`);
    if (inputRow) {
        inputRow.innerHTML = `
            <td colspan="8" class="py-2">
                <div class="d-flex justify-content-around align-items-center gap-3 flex-wrap">
                    <!-- เก็บ -->
                    <div class="input-group-col text-center">
                        <div class="d-flex align-items-center gap-1 justify-content-center">
                            <span class="badge bg-primary"><i class="fas fa-warehouse"></i> เก็บ</span>
                            <input class="form-control form-control-sm text-center qty-input store_qty fw-bold"
                                   type="number" min="0" step="any" style="width: 180px;"
                                   value="${numRaw(item.store_Qty + item.store_FixedQty)}" readonly />
                            <button class="btn btn-sm btn-outline-primary btn-edit-qty d-none" data-type="store">
                                <i class="fas fa-plus"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-secondary btn-clear-qty d-none" data-type="store">
                                <i class="fas fa-eraser"></i>
                            </button>
                        </div>
                        <small class="text-muted"><i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.store_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.store_Qty)}</strong></small>
                    </div>

                    <!-- หลอม -->
                    <div class="input-group-col text-center">
                        <div class="d-flex align-items-center gap-1 justify-content-center">
                            <span class="badge bg-warning text-dark"><i class="fas fa-fire"></i> หลอม</span>
                            <input class="form-control form-control-sm text-center qty-input melt_qty fw-bold"
                                   type="number" min="0" step="any" style="width: 180px;"
                                   value="${numRaw(item.melt_Qty + item.melt_FixedQty)}" readonly />
                            <button class="btn btn-sm btn-outline-warning btn-edit-qty d-none" data-type="melt">
                                <i class="fas fa-plus"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-secondary btn-clear-qty d-none" data-type="melt">
                                <i class="fas fa-eraser"></i>
                            </button>
                        </div>
                        <small class="text-muted"><i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.melt_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.melt_Qty)}</strong></small>
                    </div>

                    <!-- หาย -->
                    <div class="input-group-col text-center">
                        <div class="d-flex align-items-center gap-1 justify-content-center">
                            <span class="badge bg-danger"><i class="fas fa-question-circle"></i> หาย</span>
                            <input class="form-control form-control-sm text-center qty-input lost_qty fw-bold"
                                   type="number" min="0" step="any" style="width: 180px;"
                                   value="${numRaw(item.lost_Qty + item.lost_FixedQty)}" readonly />
                            <button class="btn btn-sm btn-outline-danger btn-edit-qty d-none" data-type="lost">
                                <i class="fas fa-plus"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-secondary btn-clear-qty d-none" data-type="lost">
                                <i class="fas fa-eraser"></i>
                            </button>
                        </div>
                        <small class="text-muted"><i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.lost_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.lost_Qty)}</strong></small>
                    </div>

                    <!-- ส่งออก -->
                    <div class="input-group-col text-center">
                        <div class="d-flex align-items-center gap-1 justify-content-center">
                            <span class="badge bg-success"><i class="fas fa-truck"></i> ส่งออก</span>
                            <input class="form-control form-control-sm text-center qty-input export_qty fw-bold"
                                   type="number" min="0" step="any" style="width: 180px;"
                                   value="${numRaw(item.export_Qty + item.export_FixedQty)}" readonly />
                            <button class="btn btn-sm btn-outline-success btn-edit-qty d-none" data-type="export">
                                <i class="fas fa-plus"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-secondary btn-clear-qty d-none" data-type="export">
                                <i class="fas fa-eraser"></i>
                            </button>
                        </div>
                        <small class="text-muted"><i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.export_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.export_Qty)}</strong></small>
                    </div>
                </div>
            </td>
        `;
    }
}

// Generic function to handle confirm send requests
async function confirmSendGeneric(url, typeName) {
    const uid = $('#hddUserID').val();
    const selectedLots = [];
    $(".chk-row:checked").each(function () {
        const lotNo = $(this).closest("tr").data("lot-no");
        if (lotNo) selectedLots.push(lotNo);
    });

    if (selectedLots.length === 0) {
        await swalWarning('กรุณาเลือกรายการที่ต้องการยืนยัน');
        return;
    }

    const formData = new FormData();
    selectedLots.forEach(no => formData.append("lotNos", no));
    formData.append("userId", uid);

    $.ajax({
        url: url,
        type: 'POST',
        processData: false,
        contentType: false,
        data: formData,
        beforeSend: async () => $('#loadingIndicator').show(),
        success: async (res) => {
            $('#loadingIndicator').hide();

            if (res.data && res.data.length > 0) {
                res.data.forEach(item => {
                    const tr = $(`#tbl-sendToStore-body tr.data-row[data-lot-no="${item.lotNo}"]`);
                    if (tr.length) {
                        updateRowData(tr[0], item, true);
                    }
                });
            }

            await swalSuccess(`${res.message}`);
        },
        error: async (xhr) => {
            $('#loadingIndicator').hide();
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

// ยืนยันทั้งหมด (Store, Melt, Export, Lost)
async function confirmSendAll() {
    await confirmSendGeneric(urlConfirmToSendAll, 'All');
}

// ยืนยันส่งคลัง (Store)
async function confirmSendStore() {
    await confirmSendGeneric(urlConfirmToSendStore, 'Store');
}

// ยืนยันหลอม (Melt)
async function confirmSendMelt() {
    await confirmSendGeneric(urlConfirmToSendMelt, 'Melt');
}

// ยืนยันส่งออก (Export)
async function confirmSendExport() {
    await confirmSendGeneric(urlConfirmToSendExport, 'Export');
}

// ยืนยันหาย (Lost)
async function confirmSendLost() {
    await confirmSendGeneric(urlConfirmToSendLost, 'Lost');
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
            await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

function toggleConfirmButton() {
    const anyChecked = $(".chk-row:checked").length > 0;
    $("#btnConfirmSendToStore").toggleClass("d-none", !anyChecked);
    $("#btnConfirmFloat").toggleClass("d-none", !anyChecked);
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
    // Hide edit qty and clear qty buttons in input-rows
    $(".btn-edit-qty").addClass("d-none");
    $(".btn-clear-qty").addClass("d-none");

    // Clear/restore values like btn-cancel for all rows
    $("#tbl-sendToStore-body tr.data-row").each(function () {
        const $dataRow = $(this);
        const lotNo = $dataRow.data('lot-no');
        const $inputRow = $(`#tbl-sendToStore-body tr.input-row[data-lot-no="${lotNo}"]`);

        // Restore old draft values
        const sDraft = $dataRow.data('old-store') || 0;
        const mDraft = $dataRow.data('old-melt') || 0;
        const eDraft = $dataRow.data('old-export') || 0;
        const lDraft = $dataRow.data('old-lost') || 0;

        $dataRow.data('store-draft-qty', sDraft);
        $dataRow.data('melt-draft-qty', mDraft);
        $dataRow.data('export-draft-qty', eDraft);
        $dataRow.data('lost-draft-qty', lDraft);

        const sFix = getFixedQty($dataRow, 'store');
        const mFix = getFixedQty($dataRow, 'melt');
        const eFix = getFixedQty($dataRow, 'export');
        const lFix = getFixedQty($dataRow, 'lost');

        // Update inputs in input-row
        $inputRow.find('input.store_qty').val(sFix + sDraft);
        $inputRow.find('input.melt_qty').val(mFix + mDraft);
        $inputRow.find('input.export_qty').val(eFix + eDraft);
        $inputRow.find('input.lost_qty').val(lFix + lDraft);

        updateAvailableQty($dataRow);
    });
}

function hideSelection() {
    $("#selectAllLotToStore").addClass("d-none");
    $(".chk-wrapper").addClass("d-none");
    $(".chk-row").prop("checked", false);
    $("#chkSelectAllLotToStore").prop("checked", false);
    $("#btnConfirmSendToStore").addClass("d-none");
    $("#btnConfirmFloat").addClass("d-none");
    $("#btnPrintSendToStore").addClass("d-none");
    $("#btnSelect").removeClass("d-none");
    $("#btnUnselect").addClass("d-none");
    $(".btn-edit").removeClass("d-none");
    $(".btn-edit-qty").addClass("d-none");
    $(".btn-clear-qty").addClass("d-none");
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
        swalWarning('ไม่พบแถวที่จะทำการส่งออกแบบกำหนดเอง กรุณาเปิด modal จากแถวที่ต้องการก่อน');
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

function ClearSearch() {
    $('#txtOrderNo').val('');
    const tbody = $('#tbl-sendToStore-body');
    tbody.empty().append('<tr><td colspan="14" class="text-center text-muted">ค้นหาคำสั่งซื้อ</td></tr>');

    $("#btnConfirmSendToStore").addClass("d-none");
    $("#btnConfirmFloat").addClass("d-none");
    $("#btnPrintSendToStore").addClass("d-none");
    $("#btnSelect").addClass("d-none");
    $("#divGoToListNo").addClass("d-none");
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

function reloadRow(lotNo) {
    $.ajax({
        url: urlGetOrderToStoreByLot,
        method: 'GET',
        data: { lotNo: lotNo },
        dataType: 'json',
        success: function (item) {
            if (!item) return;

            const $dataRow = $(`#tbl-sendToStore-body tr.data-row[data-lot-no="${lotNo}"]`);
            if ($dataRow.length === 0) return;

            const $inputRow = $(`#tbl-sendToStore-body tr.input-row[data-lot-no="${lotNo}"]`);

            const available_qty = item.packed_Qty
                - (item.store_FixedQty + item.store_Qty)
                - (item.melt_FixedQty + item.melt_Qty)
                - (item.lost_FixedQty + item.lost_Qty)
                - (item.export_FixedQty + item.export_Qty);

            // Update data-row attributes
            $dataRow.attr('data-ttwg', item.ttWg || 0);
            $dataRow.attr('data-percentage', item.percentage || 0);
            $dataRow.attr('data-sendtopack-qty', item.sendToPack_Qty || 0);

            $dataRow.attr('data-store-fixed-qty', item.store_FixedQty || 0);
            $dataRow.attr('data-store-fixed-wg', item.store_FixedWg || 0);
            $dataRow.attr('data-store-draft-qty', item.store_Qty || 0);
            $dataRow.attr('data-store-wg', item.store_Wg || 0);

            $dataRow.attr('data-melt-fixed-qty', item.melt_FixedQty || 0);
            $dataRow.attr('data-melt-fixed-wg', item.melt_FixedWg || 0);
            $dataRow.attr('data-melt-draft-qty', item.melt_Qty || 0);
            $dataRow.attr('data-melt-wg', item.melt_Wg || 0);
            $dataRow.attr('data-melt-des', item.breakDescriptionId || '');

            $dataRow.attr('data-export-fixed-qty', item.export_FixedQty || 0);
            $dataRow.attr('data-export-fixed-wg', item.export_FixedWg || 0);
            $dataRow.attr('data-export-draft-qty', item.export_Qty || 0);
            $dataRow.attr('data-export-wg', item.export_Wg || 0);

            $dataRow.attr('data-lost-fixed-qty', item.lost_FixedQty || 0);
            $dataRow.attr('data-lost-fixed-wg', item.lost_FixedWg || 0);
            $dataRow.attr('data-lost-draft-qty', item.lost_Qty || 0);
            $dataRow.attr('data-lost-wg', item.lost_Wg || 0);

            $dataRow.attr('data-available-qty', available_qty);

            // Update jQuery .data() cache
            $dataRow.data('store-draft-qty', item.store_Qty || 0);
            $dataRow.data('melt-draft-qty', item.melt_Qty || 0);
            $dataRow.data('export-draft-qty', item.export_Qty || 0);
            $dataRow.data('lost-draft-qty', item.lost_Qty || 0);

            // Update display values in data-row
            $dataRow.find('td:eq(3)').html(item.ttQty != 0 ? numRaw(item.ttQty) : '-');
            $dataRow.find('td:eq(4)').html(item.si != 0 ? numRaw(item.si) : '-');
            $dataRow.find('td:eq(5)').html(item.sendPack_Qty != 0 ? numRaw(item.sendPack_Qty) : '-');
            $dataRow.find('td:eq(6)').html(item.sendToPack_Qty != 0 ? numRaw(item.sendToPack_Qty) : '-');
            $dataRow.find('.col-packed').html(item.packed_Qty != 0 ? numRaw(item.packed_Qty) : '-');
            $dataRow.find('.col-available strong').html(available_qty != 0 ? numRaw(available_qty) : '-');

            // Update qty inputs in input-row
            if ($inputRow.length > 0) {
                $inputRow.find('input.store_qty').val(numRaw(item.store_Qty + item.store_FixedQty));
                $inputRow.find('input.melt_qty').val(numRaw(item.melt_Qty + item.melt_FixedQty));
                $inputRow.find('input.lost_qty').val(numRaw(item.lost_Qty + item.lost_FixedQty));
                $inputRow.find('input.export_qty').val(numRaw(item.export_Qty + item.export_FixedQty));

                // Update fixed/draft labels in input-row
                $inputRow.find('input.store_qty').closest('.input-group-col').find('small').html(
                    `<i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.store_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.store_Qty)}</strong>`
                );
                $inputRow.find('input.melt_qty').closest('.input-group-col').find('small').html(
                    `<i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.melt_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.melt_Qty)}</strong>`
                );
                $inputRow.find('input.lost_qty').closest('.input-group-col').find('small').html(
                    `<i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.lost_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.lost_Qty)}</strong>`
                );
                $inputRow.find('input.export_qty').closest('.input-group-col').find('small').html(
                    `<i class="fas fa-download"></i>: <strong class="text-info">${numRaw(item.export_FixedQty)}</strong> / <i class="fas fa-marker"></i>: <strong class="text-warning">${numRaw(item.export_Qty)}</strong>`
                );
            }
        }
    });
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

async function goToListNo() {
    const keyword = $('#txtGoToListNo').val().trim();
    await searchAndScrollToListNo(keyword);
    $('#txtGoToListNo').val('');
}

async function goToListNoFloat() {
    const keyword = $('#txtGoToListNoFloat').val().trim();
    await searchAndScrollToListNo(keyword);
    $('#txtGoToListNoFloat').val('');
}

async function searchAndScrollToListNo(keyword) {
    if (!keyword) return;

    const $rows = $('#tbl-sendToStore-body tr');
    let $target = null;

    $rows.each(function () {
        const listNoText = $(this).find('td:eq(2)').text().trim();
        if (listNoText === keyword) {
            $target = $(this);
            return false;
        }
    });

    if ($target && $target.length > 0) {
        $("html, body").animate({
            scrollTop: $target.offset().top - 100
        }, 200);

        $target.addClass("table-warning");
        setTimeout(() => $target.removeClass("table-warning"), 2000);
        return;
    }

    await swalToastWarning("ไม่พบข้อมูลลำดับที่ค้นหา");
}

