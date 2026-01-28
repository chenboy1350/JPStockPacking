$(document).ready(function () {
    $(document).on("click", "#btnConfirmAddEmployee", async function () {
        await swalConfirm(
            "ยืนยันการเพิ่มพนักงาน?", "ยืนยันการเพิ่มพนักงาน", async () => {
                let ddlAddDept = $("#ddlAddDept").val();
                let txtAddFirstName = $("#txtAddFirstName").val();
                let txtAddLastName = $("#txtAddLastName").val();
                let txtAddNickName = $("#txtAddNickName").val();

                if (!txtAddFirstName || !txtAddLastName || !ddlAddDept) {
                    return;
                    await swalWarning("กรุณากรอกข้อมูลให้ครบถ้วน");
                }

                let model = {
                    FirstName: txtAddFirstName,
                    LastName: txtAddLastName,
                    NickName: txtAddNickName,
                    DepartmentID: ddlAddDept
                };

                $.ajax({
                    url: urlAddNewEmployee,
                    type: "POST",
                    data: JSON.stringify(model),
                    contentType: "application/json; charset=utf-8",
                    success: async function (res) {
                        if (res.isSuccess) {
                            $('#modal-add-user').modal('hide');
                            reloadEmployeeList();
                            await swalSuccess(`เพิ่มพนักงานเรียบร้อยแล้ว (${res.code})`);
                        } else {
                            $('#modal-add-user').modal('hide');
                            await swalWarning(`เกิดข้อผิดพลาดในการเพิ่มพนักงาน (${res.code}) ${res.message})`);
                        }
                    },
                    error: async function (xhr) {
                        $('#modal-add-user').modal('hide');
                        let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                        await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
                    }
                });
            }
        );
    });

    $(document).on("click", "#btnConfirmEditEmployee", async function () {
        await swalConfirm(
            "ยืนยันการแก้ไขข้อมูลพนักงาน?", "ยืนยันการแก้ไขข้อมูลพนักงาน", async () => {
                let hddEmpID = $("#hddEmpID").val();
                let ddlEditDept = $("#ddlEditDept").val();
                let txtEditFirstName = $("#txtEditFirstName").val();
                let txtEditLastName = $("#txtEditLastName").val();
                let txtEditNickName = $("#txtEditNickName").val();

                if (!txtEditFirstName || !txtEditLastName || !ddlEditDept) {
                    return;
                    await swalWarning("กรุณากรอกข้อมูลให้ครบถ้วน");
                }

                let model = {
                    EmployeeID: hddEmpID,
                    FirstName: txtEditFirstName,
                    LastName: txtEditLastName,
                    NickName: txtEditNickName,
                    DepartmentID: ddlEditDept
                };

                $.ajax({
                    url: urlEditEmployee,
                    type: "PATCH",
                    data: JSON.stringify(model),
                    contentType: "application/json; charset=utf-8",
                    success: async function (res) {
                        if (res.isSuccess) {
                            $('#modal-edit-user').modal('hide');
                            reloadEmployeeList();
                            await swalSuccess(`แก้ไขข้อมูลพนักงานเรียบร้อยแล้ว (${res.code})`);
                        } else {
                            $('#modal-edit-user').modal('hide');
                            await swalWarning(`เกิดข้อผิดพลาดในการแก้ไขข้อมูลพนักงาน (${res.code}) ${res.message})`);
                        }
                    },
                    error: async function (xhr) {
                        $('#modal-edit-user').modal('hide');
                        let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
                        await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
                    }
                });
            }
        );
    });
});

function reloadEmployeeList() {
    $("#tbl-employee").empty();

    $.ajax({
        url: urlGetEmployee,
        type: "GET",
        success: function (res) {
            let rows = "";
            let i = 1;

            res.forEach(emp => {
                rows += `
                    <tr>
                        <td>${i}</td>
                        <td>${emp.firstName}</td>
                        <td>${emp.lastName}</td>
                        <td>${emp.nickName ?? ''}</td>
                        <td>${emp.departmentName}</td>
                        <td>${formatDate(emp.createDate)}</td>
                        <td>${formatDate(emp.updateDate)}</td>
                        <td>
                            ${emp.isActive
                        ? `<span class="badge bg-success">เปิดใช้งาน</span>`
                        : `<span class="badge bg-danger">ปิดใช้งาน</span>`}
                        </td>
                        <td>
                            <button class="btn btn-info btn-sm" onclick="showEditEmployeeModal(${emp.employeeID})">
                                <i class="fas fa-edit"></i> แก้ไข
                            </button>
                            ${emp.isActive
                        ? `<button class="btn btn-danger btn-sm" onclick="toggleEmployeeStatus(${emp.employeeID})"><i class="fas fa-ban"></i> ปิดใช้งาน</button>`
                        : `<button class="btn btn-success btn-sm" onclick="toggleEmployeeStatus(${emp.employeeID})"><i class="fas fa-check"></i> เปิดใช้งาน</button>`
                    }
                        </td>
                    </tr>
                `;
                i++;
            });

            $("#tbl-employee").html(rows);
        },
        error: async function (xhr) {
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await swalWarning(`โหลดข้อมูลล้มเหลว (${xhr.status} ${msg})`);
        }
    });
}

function formatDate(dateStr) {
    if (!dateStr) return "-";
    let d = new Date(dateStr);
    return d.toLocaleDateString("th-TH") + " " + d.toLocaleTimeString("th-TH");
}


function showAddEmployeeModal() {
    $("#txtAddFirstName, #txtAddLastName, #txtAddNickName").val('');

    $('#ddlAddDept').select2({
        dropdownParent: $('#modal-add-employee'),
    });

    $('#modal-add-employee').modal('show');
}

async function showEditEmployeeModal(employeeID) {
    $('#ddlEditDept').select2({
        dropdownParent: $('#modal-edit-employee'),
    });

    $.ajax({
        url: urlGetEmployeeByID,
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ EmployeeID: employeeID }),
        success: function (res) {
            $('#hddEmpID').val(res.employeeID);
            $('#txtEditFirstName').val(res.firstName);
            $('#txtEditLastName').val(res.lastName);
            $('#txtEditNickName').val(res.nickName);
            $('#ddlEditDept').val(res.departmentID).trigger('change');

            $('#modal-edit-employee').modal('show');
        },
        error: async function (xhr) {
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await swalWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}

async function toggleEmployeeStatus(empID, isActive) {
    let action = isActive ? "เปิดใช้งาน" : "ปิดใช้งาน";

    await swalConfirm(
        `ยืนยันการ ${action} พนักงาน?`, `ยืนยันการ ${action}`, async () => {
            let model = {
                EmployeeID: empID,
            };

            $.ajax({
                url: urlToggleEmployeelaStatus,
                type: "PATCH",
                data: JSON.stringify(model),
                contentType: "application/json; charset=utf-8",
                success: async function (res) {
                    if (res.isSuccess) {
                        reloadEmployeeList()
                        await swalSuccess(`เปลี่ยนสถานะพนักงานเรียบร้อยแล้ว (${res.code})`);
                    } else {
                        await swalWarning(`เกิดข้อผิดพลาดในการเปลี่ยนสถานะพนักงาน (${res.code}) ${res.message})`);
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
