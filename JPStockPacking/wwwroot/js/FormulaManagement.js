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
                    ItemCount: parseInt(txtItemCount) || 0,
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
                            await showSuccess(`ลงทะเบียนผู้ใช้ใหม่เรียบร้อยแล้ว (${res.code})`);
                            $('#modal-add-user').modal('hide');
                        } else {
                            await showWarning(`เกิดข้อผิดพลาดในการลงทะเบียน (${res.code}) ${res.message})`);
                        }
                    },
                    error: async function (xhr) {
                        let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                        await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
                    }
                });
            }
        );
    });
});

function showEditFormulaModal() {
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