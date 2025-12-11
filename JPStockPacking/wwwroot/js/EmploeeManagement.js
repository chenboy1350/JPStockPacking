$(document).ready(function () {

});

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
            $('#hddUserId').val(res.employeeID);
            $('#txtEditFirstName').val(res.firstName);
            $('#txtEditLastName').val(res.lastName);
            $('#txtEditNickName').val(res.nickName);
            $('#ddlEditDept').val(res.departmentID).trigger('change');

            $('#modal-edit-employee').modal('show');
        },
        error: async function (xhr) {
            let msg = xhr.responseJSON?.message || xhr.responseText || 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ';
            await showWarning(`เกิดข้อผิดพลาด (${xhr.status} ${msg})`);
        }
    });
}
