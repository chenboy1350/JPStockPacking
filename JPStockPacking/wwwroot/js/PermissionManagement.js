$(document).ready(function () {
    $(document).on("click", "#btnConfirmEditPermission", async function () {

        const userId = parseInt($("#hddUserId").val());

        let selected = [];

        $(".rdo-permission:checked").each(function () {
            if ($(this).val() === "enable") {
                selected.push(parseInt($(this).data("id")));
            }
        });

        const payload = {
            userId: userId,
            permissionIds: selected
        };

        $.ajax({
            url: urlUpdateUserPermission,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: async function () {
                $("#modal-edit-user-permission").modal("hide");
                await swalSuccess("บันทึกสำเร็จ");
            },
            error: async function (xhr) {
                await swalWarning(`บันทึกไม่สำเร็จ (${xhr.status})`);
            }
        });
    });

});

function showEditPermissionModal(userId, username) {
    $("#hddUserId").val(userId);
    $("#tbl-user-permission").html("");
    $("#txtperUsername").val(username);

    let html = "";
    let index = 1;

    $.ajax({
        url: urlGetUserPermission,
        type: 'GET',
        data: { userId: userId },
        success: function (res) {

            res.forEach(p => {

                const pidEnable = `perm_enable_${p.permissionId}`;
                const pidDisable = `perm_disable_${p.permissionId}`;

                html += `
                <tr>
                    <td>${index+=1}</td>
                    <td>${p.name}</td>
                    <td class="text-center">
                        <div class="icheck-success d-inline">
                            <input type="radio" 
                                   name="perm_${p.permissionId}" 
                                   id="${pidEnable}" 
                                   class="rdo-permission" 
                                   data-id="${p.permissionId}"
                                   value="enable"
                                   ${p.enabled ? "checked" : ""}>
                            <label for="${pidEnable}"></label>
                        </div>
                    </td>

                    <td class="text-center">
                        <div class="icheck-danger d-inline">
                            <input type="radio" 
                                   name="perm_${p.permissionId}" 
                                   id="${pidDisable}" 
                                   class="rdo-permission" 
                                   data-id="${p.permissionId}"
                                   value="disable"
                                   ${!p.enabled ? "checked" : ""}>
                            <label for="${pidDisable}"></label>
                        </div>
                    </td>
                </tr>
            `;
            });


            $("#tbl-user-permission").html(html);

            $("#modal-edit-user-permission").modal("show");
        },
        error: async function (xhr) {
            $("#tbl-user-permission").html(`
                <tr><td colspan="4" class="text-center text-danger">
                    โหลดข้อมูลไม่สำเร็จ (${xhr.status})
                </td></tr>
            `);

            await swalWarning(`เกิดข้อผิดพลาด (${xhr.status})`);
        },
    });
}
