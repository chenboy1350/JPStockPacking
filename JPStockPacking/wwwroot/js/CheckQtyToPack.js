$(document).ready(function () {
    $(document).on('click', '#btnFindOrderToSend', async function () {
        $('#loadingIndicator').show();
        const orderNo = $('#txtFindOrderNo').val();

        if (orderNo) {
            $.ajax({
                url: urlGetOrderToSendQty,
                method: 'GET',
                data: { orderNo: orderNo },
                success: function (res) {
                    $('#loadingIndicator').hide();

                    const $tbody = $('#tblOrderToSend tbody');
                    $tbody.empty();

                    if (res && res.length > 0) {
                        $('#actionButtons').removeClass('d-none').addClass('d-flex');

                        res.forEach((item, index) => {
                            const ttQty = item.ttQty || 0;
                            const sendTtQty = item.sendTtQty || 0;
                            const maxAllowed = Math.ceil(ttQty * 1.02);
                            const isDefined = !!item.isDefined;
                            const initValue = isDefined ? (item.ttQtyToPack ?? 0) : 0;
                            const readonlyAttr = isDefined ? 'disabled' : '';
                            const lockBadge = isDefined
                                ? `<span class="badge bg-secondary ms-2 align-middle">Locked</span>`
                                : '';

                            const row = `
                                <tr data-lot-no="${item.lotNo}">
                                    <td class="text-center">${index + 1}</td>
                                    <td class="text-left">${item.lotNo}</td>
                                    <td class="text-right">${ttQty}</td>
                                    <td class="text-right">${sendTtQty}</td>
                                    <td class="d-flex align-items-center">
                                        <input type="number"
                                               class="form-control form-control-sm me-2 qtyInput"
                                               min="0"
                                               max="${maxAllowed}"
                                               style="width:100px;"
                                               name="TtQtyToPack_${item.lotNo}"
                                               value="${initValue}"
                                               ${readonlyAttr}
                                               onchange="validateQty(this, ${ttQty})" />
                                    </td>
                                </tr>
                            `;
                            $tbody.append(row);
                        });
                    } else {
                        $('#actionButtons').addClass('d-none').removeClass('d-flex');
                        $tbody.append(`<tr><td colspan="5" class="text-center">ไม่พบข้อมูล</td></tr>`);
                    }
                },
                error: function (xhr) {
                    $('#loadingIndicator').hide();
                    console.error('GetOrderToSendQty failed:', xhr.responseText);
                }
            });
        }
    });

    $(document).on('click', '#btnClearSearch', function () {
        resetPage();
    });

    $(document).on('click', '#btnSubmitOrderToSend', async function () {
        const orderNo = $('#txtFindOrderNo').val();
        const lots = [];

        $('#tblOrderToSend tbody tr[data-lot-no]').each(function () {
            const lotNo = $(this).data('lot-no');
            const qty = Number($(this).find('input.qtyInput').val() || 0);
            lots.push({ lotNo, qty });
        });

        if (!orderNo || lots.length === 0) {
            alert('ไม่มีข้อมูลที่จะส่ง');
            return;
        }

        const formData = new FormData();
        formData.append('OrderNo', orderNo);
        lots.forEach((x, i) => {
            formData.append(`Lots[${i}].LotNo`, x.lotNo);
            formData.append(`Lots[${i}].Qty`, x.qty);
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

    $(document).on('click', '#btnClearQty', function () {
        $('#tblOrderToSend tbody tr[data-lot-no]').each(function () {
            const $input = $(this).find('input.qtyInput');
            if (!$input.prop('disabled')) {
                $input.val(0);
            }
        });
        console.log('Cleared all editable qty inputs.');
    });

});

function resetPage() {
    $('#txtFindOrderNo').val('');
    $('#tblOrderToSend tbody').empty().append(`
        <tr>
            <td colspan="5" class="text-center">กรุณาค้นหา Order</td>
        </tr>
    `);
    $('#actionButtons').addClass('d-none').removeClass('d-flex');
}


function validateQty(input, ttQty) {
    const maxAllowed = Math.ceil(ttQty * 1.02);
    let val = parseInt(input.value, 10);

    if (isNaN(val) || val < 0) {
        val = 0;
    } else if (val > maxAllowed) {
        val = maxAllowed;
    }

    input.value = val.toString();
}

