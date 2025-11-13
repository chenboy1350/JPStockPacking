$(document).ready(function () {
    $(document).on('click', '#btnSaveAppSettings', async function () {
        let txtChxQtyPercentage = $("#txtChxQtyPercentage").val();
        let txtMinWgPercentage = $("#txtMinWgPercentage").val();

        let model = {
            ChxQtyPersentage: parseInt(txtChxQtyPercentage) || 0,
            MinWgPersentage: parseInt(txtMinWgPercentage) || 0,
        };

        $.ajax({
            url: urlUpdateAppSettings,
            type: "POST",
            data: JSON.stringify(model),
            contentType: "application/json; charset=utf-8",
            success: async function (res) {
                if (res.isSuccess) {
                    await showSuccess(`บันทึกการตั้งค่าแล้ว`);
                    $('#modal-add-user').modal('hide');
                } else {
                    await showWarning(`เกิดข้อผิดพลาดในการบันทึก (${res.code}) ${res.message})`);
                }
            },
            error: async function (xhr) {
                await showWarning(`เกิดข้อผิดพลาดในการบันทึก (${xhr.status})`);
            }
        });
    });
});