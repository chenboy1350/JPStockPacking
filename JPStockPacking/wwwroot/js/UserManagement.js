﻿$(document).ready(function () {
    $(document).on("change", "#ddlEmp", function () {
        let selectedVal = $(this).val();

        if (selectedVal) {
            $("#txtAddUsername, #txtAddPassword, #txtAddConfirmPassword").prop("disabled", false);
        } else {
            $("#txtAddUsername, #txtAddPassword, #txtAddConfirmPassword").prop("disabled", true);
        }
    });

    $(document).on("input", "#txtAddConfirmPassword, #txtAddPassword", function () {
        let pass = $("#txtAddPassword").val();
        let confirmPass = $("#txtAddConfirmPassword").val();

        if (confirmPass.length > 0) {
            if (pass === confirmPass) {
                $("#txtAddConfirmPassword")
                    .removeClass("is-invalid")
                    .addClass("is-valid");
            } else {
                $("#txtAddConfirmPassword")
                    .removeClass("is-valid")
                    .addClass("is-invalid");
            }
        } else {
            $("#txtAddConfirmPassword").removeClass("is-valid is-invalid");
        }
    });

    $(document).on("input", "#txtEditPassword, #txtEditConfirmPassword", function () {
        let pass = $("#txtEditPassword").val();
        let confirmPass = $("#txtEditConfirmPassword").val();

        if (confirmPass.length > 0) {
            if (pass === confirmPass) {
                $("#txtEditConfirmPassword")
                    .removeClass("is-invalid")
                    .addClass("is-valid");
            } else {
                $("#txtEditConfirmPassword")
                    .removeClass("is-valid")
                    .addClass("is-invalid");
            }
        } else {
            $("#txtEditConfirmPassword").removeClass("is-valid is-invalid");
        }
    });

    $(document).on("click", "#btnConfirmAddUser", async function () {
        await showSaveConfirm(
            "ยืนยันการลงทะเบียน?", "ยืนยันการลงทะเบียน", async () => {
                let empId = $("#ddlEmp").val();
                let username = $("#txtAddUsername").val();
                let password = $("#txtAddPassword").val();
                let confirmPassword = $("#txtAddConfirmPassword").val();

                if (password !== confirmPassword) {
                    await showWarning("รหัสผ่านไม่ตรงกัน");
                    return;
                }

                let model = {
                    EmployeeID: parseInt(empId) || 0,
                    Username: username,
                    Password: confirmPassword,
                };

                $.ajax({
                    url: urlAddNewUser,
                    type: "POST",
                    data: JSON.stringify(model),
                    contentType: "application/json; charset=utf-8",
                    success: async function (res) {
                        if (res.isSuccess) {
                            await showSuccess(`ลงทะเบียนผู้ใช้ใหม่เรียบร้อยแล้ว (${res.code})`);
                            $('#modal-add-user').modal('hide');
                            reloadUserList();
                        } else {
                            await showWarning(`เกิดข้อผิดพลาดในการลงทะเบียน (${res.code}) ${res.message})`);
                        }
                    },
                    error: async function (xhr) {
                        await showWarning(`เกิดข้อผิดพลาดในการลงทะเบียน (${xhr.status})`);
                    }
                });
            }
        );
    });

    $(document).on("click", "#btnConfirmEditUser", async function () {
        await showSaveConfirm(
            "ยืนยันการแก้ไขบัญชีผู้ใช้?", "ยืนยันการแก้ไข", async () => {
                let userId = $("#hddUserId").val();
                let empId = $("#ddlEditEmp").val();
                let username = $("#txtEditUsername").val();
                let password = $("#txtEditPassword").val();
                let confirmPassword = $("#txtEditConfirmPassword").val();



                if (password !== confirmPassword)
                {
                    await showWarning("รหัสผ่านไม่ตรงกัน");
                    return;
                }

                let model = {
                    UserID: parseInt(userId) || 0,
                    EmployeeID: parseInt(empId) || 0,
                    Username: username,
                    Password: confirmPassword,
                };

                $.ajax({
                    url: urlEditUser,
                    type: "PATCH",
                    data: JSON.stringify(model),
                    contentType: "application/json; charset=utf-8",
                    success: async function (res) {
                        if (res.isSuccess) {
                            await showSuccess(`แก้ไขข้อมูลผู้ใช้เรียบร้อยแล้ว (${res.code})`);
                            $('#modal-add-user').modal('hide');
                            reloadUserList();
                        } else {
                            await showWarning(`เกิดข้อผิดพลาดในการแก้ไข(${res.code}) ${res.message})`);
                        }
                    },
                    error: async function (xhr) {
                        await showWarning(`เกิดข้อผิดพลาดในการแก้ไข (${xhr.status})`);
                    }
                });
            }
        );
    });
});

function showAddUserModal() {
    $("#txtAddUsername, #txtAddPassword, #txtAddConfirmPassword").val('');
    $("#txtAddUsername, #txtAddPassword, #txtAddConfirmPassword").prop("disabled", true);

    $("#txtAddConfirmPassword").removeClass("is-valid is-invalid");

    $.ajax({
        url: urlAvailableEmployee,
        type: 'GET',
        success: function (res) {
            let $ddl = $('#ddlEmp');
            $ddl.empty();
            $ddl.append('<option value="" selected>-- เลือกพนักงาน --</option>');

            $.each(res, function (i, item) {
                $ddl.append(`<option value="${item.id}">${item.firstName} ${item.lastName} (${item.nickName})</option>`);
            });
        },
        error: function () {
            alert('Error retrieving user data.');
        }
    });

    $('#modal-add-user').modal('show');
}

function showEditUserModal(userId) {
    $('#modal-edit-user').modal('show');

    $.ajax({
        url: urlGetUser,
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ userID: userId }),
        success: function (users) {
            let user = users[0];
            $('#hddUserId').val(userId);
            $('#txtEditUsername').val(user.username);
            $('#modal-edit-user').modal('show');
        },
        error: function () {
            alert('Error retrieving user data.');
        }
    });
}

async function reloadUserList() {
    $("#userTableBody").empty();

    try {
        const res = await $.ajax({
            url: urlGetUser,
            type: "POST",
            data: JSON.stringify({}),
            contentType: "application/json; charset=utf-8"
        });

        let rows = '';
        res.forEach((u, i) => {
            rows += `
                <tr>
                    <td class="text-start">#</td>
                    <td class="text-start">${u.username}</td>
                    <td class="text-start">${u.password || ''}</td>
                    <td class="text-start">${u.firstName || ''} ${u.lastName || ''} (${u.nickName || ''})</td>
                    <td class="text-start">${u.departmentName || ''}</td>
                    <td class="text-start">${u.createDate || ''}</td>
                    <td class="text-start">${u.updateDate || ''}</td>
                    <td class="col-status">${u.isActive ? '<span class="badge badge-success">เปิดใช้งาน</span>' : '<span class="badge badge-danger">ปิดใช้งาน</span>'}
                    </td>
                    <td class="col-action">
                        <button class="btn btn-info btn-sm" onclick="showEditUserModal(${u.userID})">
                            <i class="fas fa-pen-square"></i> แก้ไข
                        </button>
                        ${u.isActive
                    ? `<button class="btn btn-danger btn-sm" onclick="toggleUserStatus(${u.userID}, false)">
                                    <i class="fas fa-window-close"></i> ปิดใช้งาน
                               </button>`
                    : `<button class="btn btn-danger btn-sm" onclick="toggleUserStatus(${u.userID}, true)">
                                    <i class="fas fa-check-square"></i> เปิดใช้งาน
                               </button>`
                }
                    </td>
                </tr>
            `;
        });

        $("#userTableBody").html(rows);

    } catch (err) {
        await showWarning("โหลดข้อมูลผู้ใช้ล้มเหลว");
        console.error(err);
    }
}

async function toggleUserStatus(userId, isActive) {
    let action = isActive ? "เปิดใช้งาน" : "ปิดใช้งาน";
    await showSaveConfirm(
        `ยืนยันการ ${action} บัญชีผู้ใช้?`, `ยืนยันการ ${action}`, async () => {
            try {
                const res = await $.ajax({
                    url: urlToggleUserStatus,
                    type: "PATCH",
                    data: JSON.stringify({ UserID: userId, IsActive: isActive }),
                    contentType: "application/json; charset=utf-8"
                });
                if (res.isSuccess) {
                    await showSuccess(`เปลี่ยนสถานะผู้ใช้เรียบร้อยแล้ว (${res.code})`);
                    reloadUserList();
                } else {
                    await showWarning(`เกิดข้อผิดพลาดในการเปลี่ยนสถานะผู้ใช้ (${res.code}) ${res.message})`);
                }
            } catch (err) {
                await showWarning(`เกิดข้อผิดพลาดในการเปลี่ยนสถานะผู้ใช้ (${err.status})`);
                console.error(err);
            }
        }
    );
}