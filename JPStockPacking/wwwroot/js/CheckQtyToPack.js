$(document).ready(function () {
    $(document).on('click', '#btnFindOrderToSend', async function () {
        $('#loadingIndicator').show();
        const orderNo = $('#txtFindOrderNo').val();

        if (orderNo != '') {
            $.ajax({
                url: urlGetOrderToSendQty,
                method: 'GET',
                data: { orderNo: orderNo },
                success: function (res) {
                    $('#loadingIndicator').hide();
                    const $tbody = $('#tblOrderToSend tbody');
                    $tbody.empty();

                    $('#txtCustCode').val(res.custCode);
                    $('#txtGrade').val(res.grade);
                    $('#txtSCountry').val(res.sCountry);

                    const lots = res?.lots || [];

                    if (lots.length > 0) {
                        let hasDefined = false;

                        lots.forEach((item, index) => {
                            const ttQty = item.ttQty || 0;
                            const qtySi = item.qtySi || 0;
                            const sendTtQty = item.sendTtQty || 0;
                            const isDefined = !!item.isDefined;
                            if (isDefined) hasDefined = true;

                            const hasSize = item.size && item.size.length > 0;
                            const readonlyAttr = hasSize ? 'readonly' : (isDefined ? 'readonly' : '');

                            let row = `
                                <tr data-lot-no="${item.lotNo}"
                                    data-list-no="${item.listNo || ''}"
                                    data-edes-fn="${item.edesFn || ''}"
                                    data-tdes-art="${item.tdesArt || ''}"
                                    data-ttqty="${item.tdesArt || ''}"
                                    data-valid-ttqty="${ttQty + qtySi}"
                                    ${item.size?.length > 0 ? 'data-widget="expandable-table" aria-expanded="false"' : ""}
                                    onclick="showImg(this, '${item.picture}')" style="cursor: pointer;">
                                        <td class="text-center">${index + 1}</td>
                                        <td class="text-left">${item.lotNo}</td>
                                        <td class="text-left">${item.article}</td>
                                        <td class="text-right">${ttQty}</td>
                                        <td class="text-right">${qtySi}</td>
                                        <td class="text-right">${sendTtQty}</td>
                                        <td class="d-flex align-items-center">
                                            <input type="number"
                                                   class="form-control form-control-sm me-2 qtyInput"
                                                   min="0"
                                                   max="99999999"
                                                   style="width:100px;"
                                                   name="TtQtyToPack_${item.lotNo}"
                                                   value="${item.ttQtyToPack}"
                                                   onchange="validateQty(this,${ttQty + qtySi})"
                                                   ${readonlyAttr} >
                                                   <button type="button" class="btn btn-warning btn-sm" onclick="ShowForceSendQtyModal()"><i class="fas fa-sliders-h"></i></button>
                                        </td>
                                </tr>`;

                            if (item.size && item.size.length > 0) {
                                let totalQtyToPack = 0;

                                const sizeRows = item.size.map((s, i) => {
                                    const q = s.q || 0;
                                    const ttQtyToPack = s.ttQtyToPack || 0;
                                    totalQtyToPack += ttQtyToPack;

                                    return `
                                        <tr>
                                            <td class="text-center">${i + 1}</td>
                                            <td class="text-right">${s.s || ''}</td>
                                            <td class="text-right">${s.cs || ''}</td>
                                            <td class="text-right">${q}</td>
                                            <td class="text-right">
                                                <input type="number"
                                                       class="form-control form-control-sm sizeQtyInput"
                                                       name="SizeQty_${item.lotNo}_${i + 1}"
                                                       data-lot-no="${item.lotNo}"
                                                       data-size-index="${i + 1}"
                                                       min="0"
                                                       max="99999999"
                                                       value="${ttQtyToPack}"
                                                       style="width: 100px;" />
                                            </td>
                                        </tr>
                                    `;}).join('');

                                row += `
                                    <tr class="expandable-body d-none">
                                        <td colspan="7">
                                            <div class="card d-flex">
                                                <div class="card-body table-responsive pt-0">
                                                    <div class="row justify-content-center">
                                                        <div class="col-12 col-md-10 col-lg-8">
                                                            <table class="table table-sm table-hover">
                                                                <thead>
                                                                    <tr>
                                                                        <tr>
                                                                            <th style="width: 5%" class="text-center">#</th>
                                                                            <th style="width: 20%" class="text-right">Size ลูกค้า</th>
                                                                            <th style="width: 20%" class="text-right">Size บริษัท</th>
                                                                            <th style="width: 15%" class="text-right">จำนวนสั่ง</th>
                                                                            <th style="width: 40%" class="text-right">แจ้งยอด</th>
                                                                        </tr>
                                                                    </tr>
                                                                </thead>
                                                                <tbody>
                                                                    ${sizeRows}
                                                                </tbody>
                                                            </table>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                `;
                            }

                            $tbody.append(row);
                        });

                        if (hasDefined) {
                            $('#actionButtons').addClass('d-none').removeClass('d-flex');
                            $('#reportButtons').removeClass('d-none').addClass('d-flex');
                            $('#tblOrderToSend tbody input.qtyInput').prop('disabled', true);
                        } else {
                            $('#actionButtons').removeClass('d-none').addClass('d-flex');
                            $('#reportButtons').addClass('d-none').removeClass('d-flex');
                        }
                    } else {
                        $('#actionButtons').addClass('d-none').removeClass('d-flex');
                        $('#reportButtons').addClass('d-none').removeClass('d-flex');
                        $tbody.append(`<tr><td colspan="7" class="text-center">ไม่พบข้อมูล</td></tr>`);
                    }
                },
                error: async function (xhr) {
                    $('#loadingIndicator').hide();
                    await showError(`เกิดข้อผิดพลาดในการค้นหา (${xhr.status})`);
                }
            });
        } else {
            $('#loadingIndicator').hide();
            await showWarning("กรุณากรอกเลขคำสั่งซื้อ");
        }
    });

    $(document).on('input', '.sizeQtyInput', function () {
        const $input = $(this);
        const lotNo = $input.data('lot-no');

        const $sizeInputs = $(`input.sizeQtyInput[data-lot-no="${lotNo}"]`);

        let sum = 0;
        $sizeInputs.each(function () {
            const val = parseFloat($(this).val()) || 0;
            sum += val;
        });

        const $qtyInput = $(`tr[data-lot-no="${lotNo}"] input.qtyInput`);
        if ($qtyInput.length) {
            $qtyInput.val(sum);
        }
    });


    $(document).on('keydown', '#txtFindOrderNo', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnFindOrderToSend').click();
        }
    });

    $(document).on('click', '#btnClearSearch', function () {
        resetPage();
    });

    $(document).on('click', '#btnSubmitOrderToSend', async function () {
        await showSaveConfirm(
            "ยืนยันการแจ้งยอดใช่หรือไม่?", "ยืนยันการแจ้งยอด", async () => {
                const orderNo = $('#txtFindOrderNo').val();
                const lots = [];
                let hasAnyQty = false;

                $('#tblOrderToSend tbody tr[data-lot-no]').each(function () {
                    const lotNo = $(this).data('lot-no');
                    const qty = Number($(this).find('input.qtyInput').val() || 0);

                    if (qty === 0) return;

                    hasAnyQty = true;

                    const sizes = [];
                    $(`input.sizeQtyInput[data-lot-no="${lotNo}"]`).each(function () {
                        const sizeIndex = $(this).data('size-index');
                        const sizeQty = Number($(this).val() || 0);

                        sizes.push({
                            SizeIndex: sizeIndex,
                            TtQty: sizeQty
                        });
                    });

                    lots.push({
                        LotNo: lotNo,
                        Qty: qty,
                        Sizes: sizes
                    });
                });

                if (!hasAnyQty) {
                    await showWarning("มีรายการที่ยังไม่แจ้งยอด");
                    return;
                }

                if (!orderNo || lots.length === 0) {
                    await showWarning("ไม่มีข้อมูลที่จะแจ้งยอด");
                    return;
                }

                const formData = new FormData();
                formData.append('OrderNo', orderNo);
                lots.forEach((lot, i) => {
                    formData.append(`Lots[${i}].LotNo`, lot.LotNo);
                    formData.append(`Lots[${i}].Qty`, lot.Qty);

                    lot.Sizes.forEach((s, j) => {
                        formData.append(`Lots[${i}].Sizes[${j}].SizeIndex`, s.SizeIndex);
                        formData.append(`Lots[${i}].Sizes[${j}].TtQty`, s.TtQty);
                    });
                });


                $.ajax({
                    url: urlDefineToPack,
                    method: 'POST',
                    contentType: false,
                    processData: false,
                    data: formData,
                    success: async function () {
                        await showSuccess("บันทึกข้อมูลเรียบร้อยแล้ว");
                        resetPage();
                    },
                    error: async function (xhr) {
                        if (xhr.status === 400 && xhr.responseText.includes("ไม่มีข้อมูลที่เปลี่ยนแปลง")) {
                            await showWarning("ไม่มีข้อมูลที่เปลี่ยนแปลง");
                        } else {
                            await showError(`เกิดข้อผิดพลาดในการบันทึก (${xhr.status})`);
                        }
                    }
                });
            }
        );
    });

    $(document).on('click', '#btnEditQty', function () {
        $('#tblOrderToSend tbody tr[data-lot-no]').each(function () {
            const $input = $(this).find('input.qtyInput');
            if ($input.length) {
                $input.prop('disabled', false);
            }
        });
        console.log('All qty inputs are now editable.');
    });

    $(document).on('click', '#tblOrderToSend tbody tr[data-lot-no]', function () {
        $('#tblOrderToSend tbody tr[data-lot-no]').removeClass('tr-highlight');

        $(this).addClass('tr-highlight');
    });


    $(document).on('click', '#btnClearQty', function () {
        $('#tblOrderToSend tbody tr[data-lot-no]').each(function () {
            const $input = $(this).find('input.qtyInput');
            const lotNo = $(this).data('lot-no');

            if (!$input.prop('readonly')) {
                $input.val(0);
            }

            const $sizeinputs = $(`input.sizeQtyInput[data-lot-no="${lotNo}"]`);
            $sizeinputs.each(function () {
                if (!$(this).prop('readonly')) {
                    $(this).val(0).trigger('input');
                }
            });
        });

        console.log('Cleared all editable qty inputs.');
    });



    $(document).on('click', '#btnCheckAuthForceSendQTY', function () {
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
                btn.removeClass('btn-primary btn-danger btn-warning').addClass('btn-success').text(`ยืนยันการแก้ไขโดยผู้ใช้ ${res.userid1}`).prop('disabled', true);
                $('#hddUserID').val(res.num)
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

    $(document).on('keydown', '#txtPassword', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnCheckAuthForceSendQTY').click();
        }
    });

    $(document).on('click', '#btnSubmitForceSendQTY', async function () {
        const $row = $('#modal-force-send-qty').data('row');
        if (!$row || $row.length === 0) {
            await showWarning('ไม่พบแถวที่เลือก');
            return;
        }

        if ($('#hddUserID').val() == '' || $('#hddUserID') === 0)
        {
            await showWarning('กรุณายืนยันสิทธิการแจ้งยอดแบบกำหนดเอง');
            return;
        }

        const newQty = Number($('#txtforce-adjust-qty').val()) || 0;
        $row.find('input.qtyInput').val(newQty);

        $('#modal-force-send-qty').removeData('row').modal('hide');
    });


});

async function printToPDF(printTo) {
    const orderNo = $('#txtFindOrderNo').val();
    if (!orderNo || orderNo == '') {
        await showWarning('กรุณาเลือก Order No');
        return;
    }

    const printUrl = urlSendQtyToPackReport + '?orderNo=' + encodeURIComponent(orderNo) + '&printTo=' + encodeURIComponent(printTo);
    const printWindow = window.open(printUrl, '_blank');

    printWindow.onload = function () {
        // setTimeout(function() {
        //     printWindow.print();
        //     //printWindow.close();
        // }, 1000);
    };
}

function resetPage() {
    $('#txtFindOrderNo').val('');

    $('#txtCustCode').val('');
    $('#txtGrade').val('');
    $('#txtSCountry').val('');
    $('#txtListNo').val('');
    $('#txtEdesFn').val('');
    $('#txtTdesArt').val('');

    $('#imgLot').attr('src', './img/blankimg.jpg');

    $('#tblOrderToSend tbody').empty().append(`
        <tr>
            <td colspan="7" class="text-center">กรุณาค้นหา Order</td>
        </tr>
    `);
    $('#actionButtons').addClass('d-none').removeClass('d-flex');
    $('#reportButtons').addClass('d-none').removeClass('d-flex');
}

function showImg(element, filename) {
    const url = urlGetImage + '?filename=' + encodeURIComponent(filename);
    $('#imgLot').attr('src', url);

    document.querySelectorAll('#tblOrderToSend tbody tr[data-lot-no]').forEach(function (row) {
        row.querySelectorAll('td').forEach(function (cell) {
            cell.style.backgroundColor = '';
            cell.style.color = '';
        });
    });

    var clickedCells = element.querySelectorAll('td');
    clickedCells.forEach(function (cell) {
        cell.style.backgroundColor = '#0D6EFD';
        cell.style.color = '#FFFFFF';
    });

    const $row = $(element);
    $('#txtListNo').val($row.data('list-no'));
    $('#txtEdesFn').val($row.data('edes-fn'));
    $('#txtTdesArt').val($row.data('tdes-art'));
}

function ShowForceSendQtyModal() {
    const $row = $(event.target).closest('tr[data-lot-no]');
    const oldQty = $row.find('input.qtyInput').val();

    const txtUsername = $('#txtUsername');
    const txtPassword = $('#txtPassword');
    const btn = $('#btnCheckAuthForceSendQTY');

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

    // preload ค่าเก่า
    $('#txtforce-adjust-qty').val(oldQty);

    // เก็บ $row ไว้ใน modal
    $('#modal-force-send-qty').data('row', $row);

    $('#modal-force-send-qty').modal('show');
}

function validateQty(input, ttQty) {
    const minAllowed = ttQty - Math.ceil(ttQty * 0.02);
    let val = parseInt(input.value, 10);

    if (isNaN(val) || val < 0) {
        val = 0;
    } else if (val < minAllowed) {
        val = minAllowed;
    }

    input.value = val.toString();
}

