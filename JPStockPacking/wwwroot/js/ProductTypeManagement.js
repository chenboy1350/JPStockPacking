$(document).ready(function () {
    $(document).on("click", "#btnConfirmAddProductType", async function () {
        await swalConfirm(
            "ยืนยันการเพิ่มประเภทสินค้า?", "ยืนยันการเพิ่มประเภทสินค้า", async () => {
                let name = $("#txtAddProductTypeName").val();
                let baseTime = $("#txtAddBaseTime").val();

                if (!name) {
                    await swalWarning("กรุณากรอกชื่อประเภทสินค้า");
                    return;
                }

                let model = {
                    Name: name,
                    BaseTime: baseTime ? parseFloat(baseTime) : null
                };

                $.ajax({
                    url: urlAddProductType,
                    type: "POST",
                    data: JSON.stringify(model),
                    contentType: "application/json; charset=utf-8",
                    success: async function (res) {
                        if (res.isSuccess) {
                            $('#modal-add-product-type').modal('hide');
                            reloadProductTypeList();
                            await swalSuccess(`เพิ่มประเภทสินค้าเรียบร้อยแล้ว (${res.code})`);
                        } else {
                            $('#modal-add-product-type').modal('hide');
                            await swalWarning(`เกิดข้อผิดพลาดในการเพิ่มประเภทสินค้า (${res.code}) ${res.message}`);
                        }
                    },
                    error: async function (xhr) {
                        $('#modal-add-product-type').modal('hide');
                        let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                        await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
                    }
                });
            }
        );
    });

    $(document).on("click", "#btnConfirmEditProductType", async function () {
        await swalConfirm(
            "ยืนยันการแก้ไขประเภทสินค้า?", "ยืนยันการแก้ไขประเภทสินค้า", async () => {
                let id = $("#hddProductTypeID").val();
                let name = $("#txtEditProductTypeName").val();
                let baseTime = $("#txtEditBaseTime").val();

                if (!name) {
                    await swalWarning("กรุณากรอกชื่อประเภทสินค้า");
                    return;
                }

                let model = {
                    ProductTypeId: parseInt(id),
                    Name: name,
                    BaseTime: baseTime ? parseFloat(baseTime) : null
                };

                $.ajax({
                    url: urlEditProductType,
                    type: "PATCH",
                    data: JSON.stringify(model),
                    contentType: "application/json; charset=utf-8",
                    success: async function (res) {
                        if (res.isSuccess) {
                            $('#modal-edit-product-type').modal('hide');
                            reloadProductTypeList();
                            await swalSuccess(`แก้ไขประเภทสินค้าเรียบร้อยแล้ว (${res.code})`);
                        } else {
                            $('#modal-edit-product-type').modal('hide');
                            await swalWarning(`เกิดข้อผิดพลาดในการแก้ไขประเภทสินค้า (${res.code}) ${res.message}`);
                        }
                    },
                    error: async function (xhr) {
                        $('#modal-edit-product-type').modal('hide');
                        let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                        await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
                    }
                });
            }
        );
    });
});

function reloadProductTypeList() {
    $("#tbl-product-type").empty();

    $.ajax({
        url: urlGetProductTypes,
        type: "GET",
        success: function (res) {
            let rows = "";
            let i = 1;

            res.forEach(pt => {
                rows += `
                    <tr>
                        <td>${i}</td>
                        <td>${pt.name ?? ''}</td>
                        <td>${pt.baseTime ?? ''}</td>
                        <td>${formatProductTypeDate(pt.createDate)}</td>
                        <td>${formatProductTypeDate(pt.updateDate)}</td>
                        <td>
                            ${pt.isActive
                        ? `<span class="badge bg-success">เปิดใช้งาน</span>`
                        : `<span class="badge bg-danger">ปิดใช้งาน</span>`}
                        </td>
                        <td>
                            <button class="btn btn-info btn-sm" onclick="showEditProductTypeModal(${pt.productTypeId})">
                                <i class="fas fa-edit"></i> แก้ไข
                            </button>
                            ${pt.isActive
                        ? `<button class="btn btn-danger btn-sm" onclick="toggleProductTypeStatus(${pt.productTypeId}, false)"><i class="fas fa-ban"></i> ปิดใช้งาน</button>`
                        : `<button class="btn btn-success btn-sm" onclick="toggleProductTypeStatus(${pt.productTypeId}, true)"><i class="fas fa-check"></i> เปิดใช้งาน</button>`
                    }
                        </td>
                    </tr>
                `;
                i++;
            });

            $("#tbl-product-type").html(rows);
        },
        error: async function (xhr) {
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await swalWarning(`โหลดข้อมูลล้มเหลว (${xhr.status} ${msg})`);
        }
    });
}

function formatProductTypeDate(dateStr) {
    if (!dateStr) return "-";
    let d = new Date(dateStr);
    return d.toLocaleDateString("th-TH") + " " + d.toLocaleTimeString("th-TH");
}

function showAddProductTypeModal() {
    $("#txtAddProductTypeName, #txtAddBaseTime").val('');
    $('#modal-add-product-type').modal('show');
}

async function showEditProductTypeModal(productTypeId) {
    $.ajax({
        url: urlGetProductTypeById,
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(productTypeId),
        success: function (res) {
            $('#hddProductTypeID').val(res.productTypeId);
            $('#txtEditProductTypeName').val(res.name);
            $('#txtEditBaseTime').val(res.baseTime);
            $('#modal-edit-product-type').modal('show');
        },
        error: async function (xhr) {
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

async function toggleProductTypeStatus(productTypeId, isActive) {
    let action = isActive ? "เปิดใช้งาน" : "ปิดใช้งาน";

    await swalConfirm(
        `ยืนยันการ ${action} ประเภทสินค้า?`, `ยืนยันการ ${action}`, async () => {
            $.ajax({
                url: urlToggleProductTypeStatus,
                type: "PATCH",
                data: JSON.stringify(productTypeId),
                contentType: "application/json; charset=utf-8",
                success: async function (res) {
                    if (res.isSuccess) {
                        reloadProductTypeList();
                        await swalSuccess(`เปลี่ยนสถานะประเภทสินค้าเรียบร้อยแล้ว (${res.code})`);
                    } else {
                        await swalWarning(`เกิดข้อผิดพลาดในการเปลี่ยนสถานะ (${res.code}) ${res.message}`);
                    }
                },
                error: async function (xhr) {
                    let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                    await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
                }
            });
        }
    );
}
