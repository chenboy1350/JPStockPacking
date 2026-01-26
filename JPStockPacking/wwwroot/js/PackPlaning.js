// Order Planning Simulator - Application Logic (jQuery Version)
// ตัวแปรสำหรับเก็บโหมดปัจจุบัน

let isSentMode = false;

// ข้อมูลจำลองออเดอร์ (โครงสร้างตาม OrderPlanModel)
const ordersData = [
    { orderNo: '25000001', custCode: 'C001', article: 'CCJ82-C', qty: 30000, sendToPackQty: 20000, operateDay: 1.12, sendToPackOperateDay: 0.75, dueDate: '2025-12-25', prodType: 'TypeA', baseTime: 0.6 },
    { orderNo: '25000002', custCode: 'C001', article: 'CCJ82-C', qty: 25000, sendToPackQty: 5000, operateDay: 0.93, sendToPackOperateDay: 0.19, dueDate: '2025-12-26', prodType: 'TypeA', baseTime: 0.6 },
    { orderNo: '25000003', custCode: 'C002', article: 'CCJ82-C', qty: 98000, sendToPackQty: 80000, operateDay: 3.66, sendToPackOperateDay: 2.99, dueDate: '2025-12-27', prodType: 'TypeA', baseTime: 0.6 },
    { orderNo: '25000004', custCode: 'C002', article: 'CCJ82-C', qty: 18000, sendToPackQty: 18000, operateDay: 0.67, sendToPackOperateDay: 0.67, dueDate: '2025-12-28', prodType: 'TypeA', baseTime: 0.6 },
    { orderNo: '25000005', custCode: 'C003', article: 'CCJ82-C', qty: 35000, sendToPackQty: 34999, operateDay: 1.31, sendToPackOperateDay: 1.31, dueDate: '2025-12-29', prodType: 'TypeA', baseTime: 0.6 },
    { orderNo: '25000006', custCode: 'C003', article: 'CCJ82-C', qty: 50000, sendToPackQty: 5000, operateDay: 1.87, sendToPackOperateDay: 0.19, dueDate: '2025-12-30', prodType: 'TypeB', baseTime: 0.6 },
    { orderNo: '25000007', custCode: 'C004', article: 'CCJ82-C', qty: 45000, sendToPackQty: 12999, operateDay: 1.68, sendToPackOperateDay: 0.49, dueDate: '2025-12-26', prodType: 'TypeB', baseTime: 0.6 },
    { orderNo: '25000008', custCode: 'C004', article: 'CCJ82-C', qty: 30000, sendToPackQty: 26400, operateDay: 1.12, sendToPackOperateDay: 0.99, dueDate: '2025-12-27', prodType: 'TypeB', baseTime: 0.6 },
    { orderNo: '25000009', custCode: 'C005', article: 'CCJ82-C', qty: 25000, sendToPackQty: 1200, operateDay: 0.93, sendToPackOperateDay: 0.04, dueDate: '2025-12-28', prodType: 'TypeB', baseTime: 0.6 },
    { orderNo: '25000010', custCode: 'C005', article: 'CCJ82-C', qty: 98000, sendToPackQty: 98000, operateDay: 3.66, sendToPackOperateDay: 3.66, dueDate: '2025-12-29', prodType: 'TypeB', baseTime: 0.6 },
    { orderNo: '25000011', custCode: 'C006', article: 'CCJ82-C', qty: 18000, sendToPackQty: 500, operateDay: 0.67, sendToPackOperateDay: 0.02, dueDate: '2025-12-25', prodType: 'TypeC', baseTime: 0.6 },
    { orderNo: '25000012', custCode: 'C006', article: 'CCJ82-C', qty: 35000, sendToPackQty: 20000, operateDay: 1.31, sendToPackOperateDay: 0.75, dueDate: '2025-12-26', prodType: 'TypeC', baseTime: 0.6 },
    { orderNo: '25000013', custCode: 'C007', article: 'CCJ82-C', qty: 50000, sendToPackQty: 200, operateDay: 1.87, sendToPackOperateDay: 0.01, dueDate: '2025-12-27', prodType: 'TypeC', baseTime: 0.6 },
    { orderNo: '25000014', custCode: 'C007', article: 'CCJ82-C', qty: 45000, sendToPackQty: 37000, operateDay: 1.68, sendToPackOperateDay: 1.38, dueDate: '2025-12-30', prodType: 'TypeC', baseTime: 0.6 },
    { orderNo: '25000015', custCode: 'C008', article: 'CCJ82-C', qty: 30000, sendToPackQty: 24999, operateDay: 1.12, sendToPackOperateDay: 0.93, dueDate: '2025-12-31', prodType: 'TypeD', baseTime: 0.6 },
    { orderNo: '25000016', custCode: 'C008', article: 'CCJ82-C', qty: 25000, sendToPackQty: 25000, operateDay: 0.93, sendToPackOperateDay: 0.93, dueDate: '2025-12-27', prodType: 'TypeD', baseTime: 0.6 },
    { orderNo: '25000017', custCode: 'C009', article: 'CCJ82-C', qty: 98000, sendToPackQty: 16800, operateDay: 3.66, sendToPackOperateDay: 0.63, dueDate: '2025-12-28', prodType: 'TypeD', baseTime: 0.6 },
    { orderNo: '25000018', custCode: 'C009', article: 'CCJ82-C', qty: 18000, sendToPackQty: 18000, operateDay: 0.67, sendToPackOperateDay: 0.67, dueDate: '2025-12-29', prodType: 'TypeD', baseTime: 0.6 },
    { orderNo: '25000019', custCode: 'C010', article: 'CCJ82-C', qty: 35000, sendToPackQty: 6900, operateDay: 1.31, sendToPackOperateDay: 0.26, dueDate: '2025-12-30', prodType: 'TypeE', baseTime: 0.6 },
    { orderNo: '25000020', custCode: 'C010', article: 'CCJ82-C', qty: 50000, sendToPackQty: 9800, operateDay: 1.87, sendToPackOperateDay: 0.37, dueDate: '2025-12-31', prodType: 'TypeE', baseTime: 0.6 },
    { orderNo: '25000021', custCode: 'C010', article: 'CCJ82-C', qty: 45000, sendToPackQty: 45000, operateDay: 1.68, sendToPackOperateDay: 1.68, dueDate: '2025-12-31', prodType: 'TypeE', baseTime: 0.6 }
];

// Animation timers
const animationTimers = {};

// Document ready
$(document).ready(function () {
    // Initialize - ใช้ jQuery event delegation
    $(document).on('change', 'input', function () {
        filterAndCalculate();
    });

    // ตัวอย่างเพิ่มเติมสำหรับ click events (ถ้ามีปุ่มในอนาคต)
    $(document).on('click', '.calculate-btn', function () {
        filterAndCalculate();
    });

    $(document).on('click', '#modeToggle', function () {
        toggleMode();
    });
});




// คำนวณแผนการผลิต (ใช้ baseTime จาก orders แทน workStepsTemplate)
function calculateProductionPlan(totalQty, workersCount, baseTime) {
    // baseTime = เวลาเฉลี่ยต่อชิ้น (นาที) ที่หลังบ้านคำนวณมาแล้ว
    const totalAverageTimePerPieceMinutes = baseTime;
    const totalMinutes = totalQty * totalAverageTimePerPieceMinutes;
    const totalHours = totalMinutes / 60;
    const totalDays = totalHours / 8.5;
    const actualDays = totalDays / workersCount;
    return { totalAverageTimePerPieceMinutes, totalMinutes, totalHours, totalDays, actualDays };
}

// คำนวณจำนวนคนที่ต้องเพิ่ม
function calculateRequiredWorkers(totalQty, availableDays, currentWorkers, baseTime) {
    let requiredWorkers = currentWorkers;
    let plan = calculateProductionPlan(totalQty, requiredWorkers, baseTime);
    while (plan.actualDays > availableDays) {
        requiredWorkers++;
        plan = calculateProductionPlan(totalQty, requiredWorkers, baseTime);
    }
    return { requiredWorkers, additionalWorkers: requiredWorkers - currentWorkers, actualDays: plan.actualDays };
}

// คำนวณจำนวนวันที่เหลือจนถึง duedate
function calculateDaysUntil(duedate) {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const targetDate = new Date(duedate);
    targetDate.setHours(0, 0, 0, 0);
    const diffTime = targetDate - today;
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays >= 0 ? diffDays + 1 : 1;
}

// หา duedate ที่เร็วที่สุด
function getEarliestDueDate(filteredOrders) {
    if (filteredOrders.length === 0) return null;
    const duedates = filteredOrders.map(order => new Date(order.dueDate));
    const earliestDate = new Date(Math.min(...duedates));
    return earliestDate.toISOString().split('T')[0];
}

// ตรวจสอบว่าออเดอร์อยู่ในช่วงวันที่หรือไม่
function isOrderInRange(duedate, fromDate, toDate) {
    if (!fromDate || !toDate) return true;
    return duedate >= fromDate && duedate <= toDate;
}

// แปลงวันที่เป็นรูปแบบไทย
function formatThaiDate(dateStr) {
    const date = new Date(dateStr);
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return date.toLocaleDateString('th-TH', options);
}

// อัพเดทสถานะการทำงาน
function updateWorkStatus(totalQty, earliestDueDate, currentWorkers, daysCount, baseTime) {
    const $statusCard = $('#statusCard');
    const $statusIconWrapper = $('#statusIconWrapper');
    const $statusIcon = $('#statusIcon');
    const $statusLabel = $('#statusLabel');
    const $statusDays = $('#statusDays');
    const $statusUnit = $('#statusUnit');
    const $statusDescription = $('#statusDescription');
    const $workerInfo = $('#workerInfo');

    const availableDays = calculateDaysUntil(earliestDueDate);
    $statusCard.removeClass('ops-hidden');

    if (daysCount > availableDays) {
        $statusCard.attr('class', 'ops-stat-card ops-stat-card--warning ops-stat-card--full');
        $statusIconWrapper.attr('class', 'ops-stat-icon ops-stat-icon--red');
        $statusIcon.attr('class', 'fas fa-exclamation-triangle');
        $statusLabel.html('<i class="fas fa-exclamation-triangle"></i> งานไม่ทัน!');

        const result = calculateRequiredWorkers(totalQty, availableDays, currentWorkers, baseTime);
        $statusDays.text(daysCount.toFixed(2));
        $statusUnit.text('วัน');

        $statusDescription.html(`
            <div class="ops-status-row"><span>เวลาที่ต้องใช้:</span><strong>${daysCount.toFixed(2)} วัน</strong></div>
            <div class="ops-status-row"><span>วันที่เหลือถึง due date:</span><strong>${availableDays} วัน</strong></div>
            <div class="ops-status-row"><span>Due date ที่เร็วที่สุด:</span><strong>${formatThaiDate(earliestDueDate)}</strong></div>
            <div class="ops-status-highlight ops-status-highlight--danger"><i class="fas fa-clock"></i> ช้ากว่ากำหนด ${(daysCount - availableDays).toFixed(2)} วัน</div>
        `);

        $workerInfo.html(`
            <div class="ops-status-highlight ops-status-highlight--danger"><i class="fas fa-user-plus"></i> +${result.additionalWorkers} คน</div>
            <div class="ops-text-muted" style="margin-top: 0.5rem; font-size: 0.85rem;">ต้องเพิ่มพนักงาน</div>
            <div class="ops-status-row" style="margin-top: 1rem;"><span>พนักงานปัจจุบัน:</span><strong>${currentWorkers} คน</strong></div>
            <div class="ops-status-row"><span>พนักงานที่ต้องการ:</span><strong class="ops-text-danger">${result.requiredWorkers} คน</strong></div>
            <div class="ops-status-row"><span>เวลาที่ใช้ (ถ้าเพิ่มคน):</span><strong>${result.actualDays.toFixed(2)} วัน</strong></div>
        `);
    } else {
        $statusCard.attr('class', 'ops-stat-card ops-stat-card--success ops-stat-card--full');
        $statusIconWrapper.attr('class', 'ops-stat-icon ops-stat-icon--green');
        $statusIcon.attr('class', 'fas fa-check-circle');
        $statusLabel.html('<i class="fas fa-check-circle"></i> งานทัน!');

        const reserveDays = availableDays - daysCount;
        $statusDays.text('+' + reserveDays.toFixed(2));
        $statusUnit.text('วัน');

        $statusDescription.html(`
            <div class="ops-status-row"><span>เวลาที่ต้องใช้:</span><strong>${daysCount.toFixed(2)} วัน</strong></div>
            <div class="ops-status-row"><span>วันที่เหลือถึง due date:</span><strong>${availableDays} วัน</strong></div>
            <div class="ops-status-row"><span>Due date ที่เร็วที่สุด:</span><strong>${formatThaiDate(earliestDueDate)}</strong></div>
            <div class="ops-status-highlight ops-status-highlight--success"><i class="fas fa-check"></i> <strong>เร็วกว่ากำหนด +${reserveDays.toFixed(2)} วัน</strong></div>
        `);

        $workerInfo.html(`
            <div class="ops-status-highlight ops-text-success" style="font-size: 1.8rem;"><i class="fas fa-check-circle"></i> ${currentWorkers} คน</div>
            <div class="ops-text-success ops-font-semibold" style="margin-top: 0.5rem; font-size: 0.95rem;">✓ ${currentWorkers} คน สามารถทำทันได้แล้ว</div>
            <div class="ops-status-row" style="margin-top: 1rem; padding-top: 1rem; border-top: 1px solid rgba(16, 185, 129, 0.2);"><span>พนักงานที่มี:</span><strong class="ops-text-success">${currentWorkers} คน</strong></div>
            <div class="ops-status-row"><span>สถานะ:</span><strong class="ops-text-success">✓ เพียงพอและพร้อมทำงาน</strong></div>
            <div class="ops-status-row"><span>เวลาสำรอง:</span><strong class="ops-text-success" style="font-size: 1.1rem;">+${reserveDays.toFixed(2)} วัน</strong></div>
        `);
    }
}

// แสดงข้อมูลในตาราง
function displayOrders() {
    const $tbody = $('#ordersTableBody');
    const fromDate = $('#fromDate').val();
    const toDate = $('#toDate').val();

    $tbody.empty();

    $.each(ordersData, function (index, order) {
        const isFiltered = isOrderInRange(order.dueDate, fromDate, toDate);
        const filteredClass = !isFiltered ? 'ops-row--filtered' : '';

        const sentPercentage = ((order.sendToPackQty / order.qty) * 100).toFixed(1);
        let sentColorClass = 'ops-text-warning';
        if (sentPercentage >= 100) sentColorClass = 'ops-text-success';
        else if (sentPercentage >= 50) sentColorClass = 'ops-text-info';

        const badgeClass = isFiltered ? 'ops-badge--success' : 'ops-badge--muted';
        const badgeText = isFiltered ? 'เลือก' : 'ไม่เลือก';

        const rowHtml = `
            <tr class="${filteredClass}">
                <td><strong>${order.orderNo}</strong></td>
                <td>${order.qty.toLocaleString()}</td>
                <td><div class="ops-flex ops-items-center ops-gap-sm"><strong class="${sentColorClass}">${order.sendToPackQty.toLocaleString()}</strong><span class="ops-text-muted" style="font-size: 0.85rem;">(${sentPercentage}%)</span></div></td>
                <td>${order.operateDay.toFixed(2)}</td>
                <td>${formatThaiDate(order.dueDate)}</td>
                <td><span class="ops-badge ${badgeClass}">${badgeText}</span></td>
            </tr>
        `;

        $tbody.append(rowHtml);
    });
}

// กรองและคำนวณ
function filterAndCalculate() {
    const fromDate = $('#fromDate').val();
    const toDate = $('#toDate').val();
    const workers = parseInt($('#inputWorkers').val()) || 1;
    const hours = 8.5;

    const filteredOrders = ordersData.filter(order => isOrderInRange(order.dueDate, fromDate, toDate));

    let totalQty;
    if (isSentMode) {
        totalQty = filteredOrders.reduce((sum, order) => sum + order.sendToPackQty, 0);
    } else {
        totalQty = filteredOrders.reduce((sum, order) => sum + order.qty, 0);
    }

    // คำนวณ baseTime เฉลี่ยจาก orders ที่กรอง
    const avgBaseTime = filteredOrders.length > 0
        ? filteredOrders.reduce((sum, order) => sum + order.baseTime, 0) / filteredOrders.length
        : 0.6;

    const selectedCount = filteredOrders.length;
    const currentPlan = calculateProductionPlan(totalQty, workers, avgBaseTime);
    const daysCount = currentPlan.actualDays;

    const totalHours = daysCount * workers * hours;
    const targetPerWorker = daysCount > 0 ? (totalQty / (workers * daysCount)).toFixed(2) : 0;
    const targetPerHour = totalHours > 0 ? (totalQty / totalHours).toFixed(2) : 0;
    const efficiency = totalHours > 0 ? (totalQty / (workers * totalHours)).toFixed(2) : 0;

    animateValue('productCount', 0, totalQty, 1000);
    animateValue('daysCount', 0, parseFloat(daysCount.toFixed(2)), 1000);
    animateValue('workersCount', 0, workers, 1000);
    animateValue('targetPerWorker', 0, parseFloat(targetPerWorker), 1000);

    $('#selectedOrders').text(selectedCount + ' รายการ');
    $('#totalHours').text(totalHours.toFixed(2) + ' ชั่วโมง');
    $('#targetPerHour').text(targetPerHour + ' ชิ้น/ชม.');
    $('#targetPerWorkerDay').text(targetPerWorker + ' ชิ้น/วัน');
    $('#efficiency').text(efficiency + ' ชิ้น/คน/ชม.');

    const $statusBadge = $('#statusBadge');
    if (parseFloat(targetPerWorker) < 50) {
        $statusBadge.text('เป้าหมายต่ำ').attr('class', 'ops-badge ops-badge--success');
    } else if (parseFloat(targetPerWorker) < 100) {
        $statusBadge.text('เป้าหมายปกติ').attr('class', 'ops-badge ops-badge--info');
    } else {
        $statusBadge.text('เป้าหมายสูง').attr('class', 'ops-badge ops-badge--warning');
    }

    $('#summaryDetails').removeClass('ops-hidden');
    displayOrders();

    if (filteredOrders.length > 0 && totalQty > 0) {
        const earliestDueDate = getEarliestDueDate(filteredOrders);
        updateWorkStatus(totalQty, earliestDueDate, workers, daysCount, avgBaseTime);
    } else {
        $('#statusCard').addClass('ops-hidden');
    }
}

// ฟังก์ชันสลับโหมดการแสดงผล
function toggleMode() {
    isSentMode = !isSentMode;

    const $button = $('#modeToggle');
    const $icon = $button.find('.ops-mode-icon');
    const $text = $button.find('.ops-mode-text');

    if (isSentMode) {
        $button.addClass('ops-mode-btn--sent');
        $icon.text('✅');
        $text.text('สินค้าที่ส่งมาแล้ว');
    } else {
        $button.removeClass('ops-mode-btn--sent');
        $icon.text('📦');
        $text.text('สินค้าทั้งหมด');
    }

    filterAndCalculate();
}

// Animation function
function animateValue(id, start, end, duration) {
    const $element = $('#' + id);
    if (animationTimers[id]) clearInterval(animationTimers[id]);

    const range = end - start;
    const isDecimal = end % 1 !== 0;

    if (Math.abs(range) < 0.01) {
        $element.text(isDecimal ? end.toFixed(2) : Math.floor(end));
        return;
    }

    const increment = range / (duration / 16);
    let current = start;

    animationTimers[id] = setInterval(function () {
        current += increment;
        if ((increment > 0 && current >= end) || (increment < 0 && current <= end)) {
            current = end;
            clearInterval(animationTimers[id]);
            delete animationTimers[id];
        }
        $element.text(isDecimal ? current.toFixed(2) : Math.floor(current));
    }, 16);
}
