$(document).ready(function () {
    $(document).on("click", "#btnConfirmAddFormula", async function () {
        await showSaveConfirm(
            "Confirm to save?", "Save Formula", async () => {
                let txtFormulaName = $("#txtFormulaName").val();
                let ddlCustomerGroup = $("#ddlCustomerGroup").val();
                let ddlProductType = $("#ddlProductType").val();
                let ddlPackMethod = $("#ddlPackMethod").val();
                let txtItemCount = $("#txtItemCount").val();
                let txtP1 = $("#txtP1").val();
                let txtP2 = $("#txtP2").val();

                if (!txtFormulaName) {
                    await showWarning("pls fill name");
                    return;
                }

                if (!ddlCustomerGroup || !ddlProductType || !ddlPackMethod) {
                    await showWarning("pls select required dropdowns");
                    return;
                }

                if (!txtItemCount || !txtP1 || !txtP2) {
                    await showWarning("pls fill all numeric fields");
                    return;
                }
                
                let model = {
                    Name: txtFormulaName,
                    CustomerGroupId: parseInt(ddlCustomerGroup) || 0,
                    PackMethodId: parseInt(ddlPackMethod) || 0,
                    ProductTypeId: parseInt(ddlProductType) || 0,
                    Items: parseInt(txtItemCount) || 0,
                    P1: parseFloat(txtP1) || 0,
                    P2: parseFloat(txtP2) || 0
                };

                $.ajax({
                    url: urlAddNewFormula,
                    type: "POST",
                    data: JSON.stringify(model),
                    contentType: "application/json; charset=utf-8",
                    success: async function (res) {
                        if (res.isSuccess) {
                            $('#modal-add-formula').modal('hide');
                            loadFormulaTable();
                            await showSuccess(`ลงทะเบียนสูตรคำนวณใหม่เรียบร้อยแล้ว`);
                        } else {
                            $('#modal-add-formula').modal('hide');
                            await showWarning(`เกิดข้อผิดพลาดในการลงทะเบียน (${res.code}) ${res.message})`);
                        }
                    },
                    error: async function (xhr) {
                        $('#modal-add-formula').modal('hide');
                        let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                        await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
                    }
                });
            }
        );
    });

    $(document).on("click", "#btnConfirmEditFormula", async function () {
        await showSaveConfirm(
            "Confirm to save?", "Save Formula", async () => {
                let editFormulaId = $("#editFormulaId").val();
                let txtFormulaName = $("#txtEditFormulaName").val();
                let ddlCustomerGroup = $("#ddlEditCustomerGroup").val();
                let ddlProductType = $("#ddlEditProductType").val();
                let ddlPackMethod = $("#ddlEditPackMethod").val();
                let txtItemCount = $("#txtEditItemCount").val();
                let txtP1 = $("#txtEditP1").val();
                let txtP2 = $("#txtEditP2").val();

                if (!txtFormulaName) {
                    await showWarning("pls fill name");
                    return;
                }

                if (!ddlCustomerGroup || !ddlProductType || !ddlPackMethod) {
                    await showWarning("pls select required dropdowns");
                    return;
                }

                if (!txtItemCount || !txtP1 || !txtP2) {
                    await showWarning("pls fill all numeric fields");
                    return;
                }

                let model = {
                    FormulaId: parseInt(editFormulaId) || 0,
                    Name: txtFormulaName,
                    CustomerGroupId: parseInt(ddlCustomerGroup) || 0,
                    PackMethodId: parseInt(ddlPackMethod) || 0,
                    ProductTypeId: parseInt(ddlProductType) || 0,
                    Items: parseInt(txtItemCount) || 0,
                    P1: parseFloat(txtP1) || 0,
                    P2: parseFloat(txtP2) || 0
                };

                $.ajax({
                    url: urlEditFormula,
                    type: "PATCH",
                    data: JSON.stringify(model),
                    contentType: "application/json; charset=utf-8",
                    success: async function (res) {
                        if (res.isSuccess) {
                            $('#modal-edit-formula').modal('hide');
                            loadFormulaTable();
                            await showSuccess(`แก้ไขสูตรคำนวณเรียบร้อยแล้ว`);
                        } else {
                            $('#modal-edit-formula').modal('hide');
                            await showWarning(`เกิดข้อผิดพลาดในการแก้ไข(${res.code}) ${res.message})`);
                        }
                    },
                    error: async function (xhr) {
                        $('#modal-edit-formula').modal('hide');
                        let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                        await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
                    }
                });
            }
        );
    });

});

async function loadFormulaTable() {

    let model = {};

    $.ajax({
        url: urlGetFormula,
        type: "POST",
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",

        success: function (res) {

            let tbody = $("#tbl-body-formula");
            tbody.empty();

            if (!res || res.length === 0) {
                tbody.append(`<tr><td colspan="13" class="text-center">ไม่พบข้อมูล</td></tr>`);
                return;
            }

            let rows = "";
            let index = 1;

            res.forEach(item => {

                let statusBadge = item.isActive
                    ? `<span class="badge badge-success">Active</span>`
                    : `<span class="badge badge-danger">Disabled</span>`;

                let toggleButton = item.isActive
                    ? `<button class="btn btn-danger btn-sm" onclick="toggleFormulaStatus(${item.formulaID})"><i class="fas fa-window-close"></i> Disable</button>`
                    : `<button class="btn btn-success btn-sm" onclick="toggleFormulaStatus(${item.formulaID})"><i class="fas fa-check-square"></i> Active</button>`;

                rows += `
                    <tr>
                        <td class="text-center">${index}</td>
                        <td class="text-left">${item.name}</td>
                        <td class="text-center">${item.customerGroup}</td>
                        <td class="text-center">${item.packMethod}</td>
                        <td class="text-center">${item.productType}</td>

                        <td class="text-right">${item.items}</td>
                        <td class="text-right">${item.p1}</td>
                        <td class="text-right">${item.p2}</td>
                        <td class="text-right">${item.avg}</td>
                        <td class="text-right">${item.itemPerSec}</td>

                        <td class="text-center">${statusBadge}</td>

                        <td class="text-center">
                            <button class="btn btn-info btn-sm" onclick="showEditFormulaModal(${item.formulaID})">
                                <i class="fas fa-pen-square"></i> Edit
                            </button>

                            ${toggleButton}
                        </td>
                    </tr>
                `;

                index++;
            });

            tbody.append(rows);
        },

        error: function () {
            showWarning("โหลดข้อมูลล้มเหลว");
        }
    });
}


function showAddFormulaModal() {
    $('#modal-add-formula').modal('show');

    $('#ddlCustomerGroup').select2({
        dropdownParent: $('#modal-add-formula'),
    });

    $('#ddlProductType').select2({
        dropdownParent: $('#modal-add-formula'),
    });

    $('#ddlPackMethod').select2({
        dropdownParent: $('#modal-add-formula'),
    });
}

function showEditFormulaModal(formulaId) {
    $('#ddlCustomerGroup').select2({
        dropdownParent: $('#modal-add-formula'),
    });

    $('#ddlProductType').select2({
        dropdownParent: $('#modal-add-formula'),
    });

    $('#ddlPackMethod').select2({
        dropdownParent: $('#modal-add-formula'),
    });

    let model = {
        FormulaId: formulaId,
    };

    $.ajax({
        url: urlGetFormula,
        type: "POST",
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        success: async function (res) {
            if (!res || res.length === 0) {
                await showWarning("ไม่พบข้อมูลสูตรคำนวณ");
                return;
            }

            let f = res[0];

            $("#editFormulaId").val(f.formulaID);
            $("#txtEditFormulaName").val(f.name);

            $("#ddlEditCustomerGroup").val(f.customerGroupID).trigger("change");
            $("#ddlEditProductType").val(f.productTypeID).trigger("change");
            $("#ddlEditPackMethod").val(f.packMethodID).trigger("change");

            $("#txtEditItemCount").val(f.items);
            $("#txtEditP1").val(f.p1);
            $("#txtEditP2").val(f.p2);

            $("#modal-edit-formula").modal("show");
        },
        error: async function () {
            await showWarning("โหลดข้อมูลสูตรล้มเหลว");
        }
    });
}

async function toggleFormulaStatus(formulaId) {
    let model = {
        FormulaId: formulaId,
    };

    $.ajax({
        url: urlToggleFormulaStatus,
        type: "PATCH",
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        success: async function (res) {
            if (res.isSuccess) {
                loadFormulaTable()
            } else {
                await showWarning(res.message);
            }
        },
        error: async function (xhr) {
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}
