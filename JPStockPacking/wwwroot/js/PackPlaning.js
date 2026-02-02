// Order Planning Simulator - Application Logic (jQuery Version)
// ตัวแปรสำหรับเก็บโหมดปัจจุบัน

let isSentMode = false;

// ข้อมูลจำลองออเดอร์ (โครงสร้างตาม OrderPlanModel)
let ordersData = [];

// เก็บข้อมูล segments สำหรับแสดง popup
let currentSegments = [];

// Animation timers
const animationTimers = {};

// ดึงข้อมูล orders จาก API
async function fetchOrdersData() {
    const fromDate = $('#fromDate').val();
    const toDate = $('#toDate').val();

    return await $.ajax({
        url: urlGetOrderToPlan,
        type: 'POST',
        data: {
            FromDate: fromDate,
            ToDate: toDate
        },
        beforeSend: async () => $('#loadingIndicator').show(),
        complete: async () => $('#loadingIndicator').hide(),
        success: function (response) {
            if (response && Array.isArray(response)) {
                console.log("response:", response);
                ordersData = response.map(order => ({
                    orderNo: order.orderNo,
                    custCode: order.custCode,
                    customerGroup: order.customerGroup ?? 5,
                    article: order.article,
                    qty: order.qty ?? 0,
                    sendToPackQty: order.sendToPackQty ?? 0,
                    operateDay: order.operateDay ?? 0,
                    sendToPackOperateDay: order.sendToPackOperateDay ?? 0,
                    dueDate: order.dueDate,
                    prodType: order.prodType,
                    baseTime: order.baseTime ?? 0.6
                }));
                console.log('โหลดข้อมูลสำเร็จ:', ordersData.length, 'รายการ');
                console.log('ordersData:', ordersData);
            }
        },
        error: function (xhr, status, error) {
            console.error('เกิดข้อผิดพลาดในการโหลดข้อมูล:', error);
        }
    });
}

// Document ready
$(document).ready(function () {
    // Initialize - ใช้ jQuery event delegation
    $(document).on('change', 'input', function () {
        //filterAndCalculate();
    });

    // ตัวอย่างเพิ่มเติมสำหรับ click events (ถ้ามีปุ่มในอนาคต)
    $(document).on('click', '#calculateBtn', async function () {
        await fetchOrdersData()
        filterAndCalculate();
    });

    $(document).on('click', '#modeToggle', async function () {
        await fetchOrdersData()
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
    // ถ้า availableDays = 0 (due วันนี้) ให้ถือว่ามี 1 วันเป็นอย่างน้อย
    const effectiveDays = Math.max(availableDays, 1);
    let requiredWorkers = currentWorkers;
    let plan = calculateProductionPlan(totalQty, requiredWorkers, baseTime);
    while (plan.actualDays > effectiveDays) {
        requiredWorkers++;
        plan = calculateProductionPlan(totalQty, requiredWorkers, baseTime);
    }
    return { requiredWorkers, additionalWorkers: requiredWorkers - currentWorkers, actualDays: plan.actualDays };
}

// คำนวณจำนวนวันที่เหลือจนถึง duedate (จาก fromDate)
// คืนค่า 0 = วันนี้, 1 = พรุ่งนี้, etc.
function calculateDaysUntil(duedate, fromDate) {
    const startDate = new Date(fromDate);
    startDate.setHours(0, 0, 0, 0);
    const targetDate = new Date(duedate);
    targetDate.setHours(0, 0, 0, 0);
    const diffTime = targetDate - startDate;
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return Math.max(diffDays, 0);
}

// หา duedate ที่เร็วที่สุด
function getEarliestDueDate(filteredOrders) {
    if (filteredOrders.length === 0) return null;
    const duedates = filteredOrders.map(order => new Date(order.dueDate));
    const earliestDate = new Date(Math.min(...duedates));
    // ใช้ local date แทน toISOString เพื่อหลีกเลี่ยงปัญหา timezone
    const year = earliestDate.getFullYear();
    const month = String(earliestDate.getMonth() + 1).padStart(2, '0');
    const day = String(earliestDate.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
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

    const fromDate = $('#fromDate').val();
    const availableDays = calculateDaysUntil(earliestDueDate, fromDate);
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
    $tbody.empty();

    // เรียงตาม customerGroup -> dueDate
    const sortedOrders = [...ordersData].sort((a, b) => {
        // เรียงตาม customerGroup ก่อน (1 = สำคัญสุด)
        if (a.customerGroup !== b.customerGroup) {
            return a.customerGroup - b.customerGroup;
        }
        // ถ้า customerGroup เท่ากัน เรียงตาม dueDate
        return new Date(a.dueDate) - new Date(b.dueDate);
    });

    $.each(sortedOrders, function (index, order) {
        const sentPercentage = ((order.sendToPackQty / order.qty) * 100).toFixed(1);
        let sentColorClass = 'ops-text-warning';
        if (sentPercentage >= 100) sentColorClass = 'ops-text-success';
        else if (sentPercentage >= 50) sentColorClass = 'ops-text-info';

        // แสดง badge VIP เฉพาะกลุ่ม 1
        const vipBadge = order.customerGroup === 1
            ? '<span class="ops-badge ops-badge--warning" style="margin-left: 0.5rem;"><i class="fas fa-star"></i></span>'
            : '';

        const rowHtml = `
            <tr>
                <td><strong>${order.orderNo}</strong>${vipBadge}</td>
                <td>${order.custCode || '-'}</td>
                <td>${order.qty.toLocaleString()}</td>
                <td><div class="ops-flex ops-items-center ops-gap-sm"><strong class="${sentColorClass}">${order.sendToPackQty.toLocaleString()}</strong><span class="ops-text-muted" style="font-size: 0.85rem;">(${sentPercentage}%)</span></div></td>
                <td>${order.operateDay.toFixed(2)}</td>
                <td>${formatThaiDate(order.dueDate)}</td>
                <td><span class="ops-badge ops-badge--success">เลือก</span></td>
            </tr>
        `;

        $tbody.append(rowHtml);
    });
}

// กรองและคำนวณ
function filterAndCalculate() {
    const workers = parseInt($('#inputWorkers').val()) || 1;
    const hours = 8.5;

    // ใช้ ordersData ตรงๆ เพราะ backend กรองมาให้แล้ว
    let totalQty;
    if (isSentMode) {
        totalQty = ordersData.reduce((sum, order) => sum + order.sendToPackQty, 0);
    } else {
        totalQty = ordersData.reduce((sum, order) => sum + order.qty, 0);
    }

    // คำนวณ baseTime เฉลี่ยจาก orders
    const avgBaseTime = ordersData.length > 0
        ? ordersData.reduce((sum, order) => sum + order.baseTime, 0) / ordersData.length
        : 0.6;

    const currentPlan = calculateProductionPlan(totalQty, workers, avgBaseTime);
    const daysCount = currentPlan.actualDays;
    const targetPerWorker = daysCount > 0 ? (totalQty / (workers * daysCount)).toFixed(2) : 0;

    animateValue('productCount', 0, totalQty, 1000);
    animateValue('daysCount', 0, parseFloat(daysCount.toFixed(2)), 1000);
    animateValue('workersCount', 0, workers, 1000);
    animateValue('targetPerWorker', 0, parseFloat(targetPerWorker), 1000);

    const $statusBadge = $('#statusBadge');
    if (parseFloat(targetPerWorker) < 50) {
        $statusBadge.text('ชิลๆ สบายๆ').attr('class', 'ops-badge ops-badge--success');
    } else if (parseFloat(targetPerWorker) < 100) {
        $statusBadge.text('ปกติ').attr('class', 'ops-badge ops-badge--info');
    } else {
        $statusBadge.text('งานล้นมือ').attr('class', 'ops-badge ops-badge--warning');
    }

    displayOrders();

    if (ordersData.length > 0 && totalQty > 0) {
        const earliestDueDate = getEarliestDueDate(ordersData);
        updateWorkStatus(totalQty, earliestDueDate, workers, daysCount, avgBaseTime);

        // Timeline Breakdown
        const segments = createTimelineSegments(ordersData, $('#fromDate').val(), workers, isSentMode);
        displayTimelineBreakdown(segments, workers);
    } else {
        $('#statusCard').addClass('ops-hidden');
        $('#timelineSection').addClass('ops-hidden');
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

// ====== TIMELINE BREAKDOWN FUNCTIONS ======

// หา due dates ที่ไม่ซ้ำและเรียงลำดับ
function getUniqueDueDates(orders) {
    const dueDatesSet = new Set();
    orders.forEach(order => {
        const date = new Date(order.dueDate);
        date.setHours(0, 0, 0, 0);
        dueDatesSet.add(date.getTime());
    });
    return Array.from(dueDatesSet)
        .sort((a, b) => a - b)
        .map(timestamp => new Date(timestamp));
}

// เช็คว่าวันที่เท่ากันหรือไม่
function isSameDate(date1, date2) {
    const d1 = new Date(date1);
    const d2 = new Date(date2);
    return d1.getFullYear() === d2.getFullYear() &&
        d1.getMonth() === d2.getMonth() &&
        d1.getDate() === d2.getDate();
}

// สร้าง Timeline Segments แบ่งตาม Due Date (ใช้วันนี้เป็นจุดอ้างอิง)
function createTimelineSegments(orders, fromDate, workers, isSentMode) {
    const uniqueDueDates = getUniqueDueDates(orders);
    const segments = [];

    // ใช้วันนี้เป็นจุดอ้างอิงหลัก
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    let carryOverDays = 0;

    for (let i = 0; i < uniqueDueDates.length; i++) {
        const dueDate = uniqueDueDates[i];

        // กรอง orders ที่มี dueDate ตรงกัน
        const segmentOrders = orders.filter(order => isSameDate(order.dueDate, dueDate));

        // คำนวณ qty ตาม mode
        const segmentQty = isSentMode
            ? segmentOrders.reduce((sum, o) => sum + o.sendToPackQty, 0)
            : segmentOrders.reduce((sum, o) => sum + o.qty, 0);

        // คำนวณ baseTime เฉลี่ย
        const avgBaseTime = segmentOrders.length > 0
            ? segmentOrders.reduce((sum, o) => sum + o.baseTime, 0) / segmentOrders.length
            : 0.6;

        // เช็คว่า overdue หรือไม่
        const isOverdue = dueDate < today;

        // คำนวณวันที่เลยมา หรือ วันที่เหลือ
        let daysOverdue = 0;
        let daysUntilDue = 0;
        let availableDays = 1; // default สำหรับ overdue (ต้องเสร็จวันนี้)

        if (isOverdue) {
            // คำนวณว่าเลยมากี่วัน
            daysOverdue = Math.ceil((today - dueDate) / (1000 * 60 * 60 * 24));
            availableDays = 1; // ต้องเสร็จภายในวันนี้
        } else {
            // คำนวณว่าอีกกี่วันถึง due date (จากวันนี้)
            daysUntilDue = calculateDaysUntil(dueDate, today);
            // ถ้า due วันนี้ (daysUntilDue = 0) ยังมีเวลา 1 วันในการทำงาน
            availableDays = daysUntilDue === 0 ? 1 : daysUntilDue;
        }

        // effective available days รวม carry-over
        const effectiveAvailableDays = Math.max(availableDays + carryOverDays, 0);

        // คำนวณ days needed
        const plan = calculateProductionPlan(segmentQty, workers, avgBaseTime);
        const daysNeeded = plan.actualDays;

        // เช็คว่าทันหรือไม่
        const isOnTime = daysNeeded <= effectiveAvailableDays;
        const timeDiff = effectiveAvailableDays - daysNeeded;

        // คำนวณจำนวนคนที่ต้องเพิ่มถ้าไม่ทัน
        let requiredWorkers = workers;
        let additionalWorkers = 0;
        if (!isOnTime) {
            const targetDays = isOverdue ? 1 : availableDays; // overdue ต้องเสร็จใน 1 วัน
            const result = calculateRequiredWorkers(segmentQty, Math.max(targetDays, 0.5), workers, avgBaseTime);
            requiredWorkers = result.requiredWorkers;
            additionalWorkers = result.additionalWorkers;
        }

        // เก็บข้อมูล segment
        segments.push({
            index: i + 1,
            startDate: today,
            endDate: dueDate,
            orders: segmentOrders,
            orderCount: segmentOrders.length,
            totalQty: segmentQty,
            avgBaseTime: avgBaseTime,
            availableDays: availableDays,
            effectiveAvailableDays: effectiveAvailableDays,
            daysNeeded: daysNeeded,
            isOnTime: isOnTime,
            timeDiff: timeDiff,
            carryOverFromPrevious: carryOverDays,
            carryOverToNext: timeDiff,
            // ข้อมูลใหม่
            isOverdue: isOverdue,
            daysOverdue: daysOverdue,
            daysUntilDue: daysUntilDue,
            requiredWorkers: requiredWorkers,
            additionalWorkers: additionalWorkers
        });

        // ส่งต่อ carry-over ไป segment ถัดไป (รวมถึง overdue ที่ทำไม่เสร็จ)
        carryOverDays = timeDiff;
    }

    return segments;
}

// แสดง Timeline Breakdown (แนวนอน: OVERDUE ← | TODAY | → UPCOMING)
function displayTimelineBreakdown(segments, workers) {
    const $container = $('#timelineContainer');
    const $summary = $('#timelineSummary');
    const $section = $('#timelineSection');

    if (segments.length === 0) {
        $section.addClass('ops-hidden');
        return;
    }

    $section.removeClass('ops-hidden');
    $container.empty();

    // เก็บ segments ไว้ใน global variable สำหรับ popup
    currentSegments = segments;

    // แยก segments เป็น 3 กลุ่ม: OVERDUE, TODAY, UPCOMING
    const overdueSegments = segments.filter(s => s.isOverdue);
    const todaySegments = segments.filter(s => !s.isOverdue && s.daysUntilDue === 0);
    const upcomingSegments = segments.filter(s => !s.isOverdue && s.daysUntilDue > 0);

    // สร้าง Timeline แนวนอน
    const timelineHtml = `
        <div class="ops-horizontal-timeline">
            <!-- Timeline Content -->
            <div class="ops-timeline-track">
                <!-- Timeline Line (อยู่ใน track เพื่อให้ยาวตาม content) -->
                <div class="ops-timeline-line"></div>

                <!-- OVERDUE Section -->
                ${overdueSegments.length > 0 ? `
                    <div class="ops-timeline-zone ops-timeline-zone--overdue">
                        <div class="ops-zone-label">
                            <i class="fas fa-exclamation-triangle"></i>
                            <span>เลยกำหนด</span>
                        </div>
                        <div class="ops-zone-segments">
                            ${overdueSegments.map(seg => createHorizontalSegmentHtml(seg, true)).join('')}
                        </div>
                    </div>
                ` : ''}

                <!-- TODAY Section (marker + cards ที่ due วันนี้) -->
                <div class="ops-timeline-today">
                    <div class="ops-today-label">
                        <i class="fas fa-map-marker-alt"></i>
                        <span>วันนี้</span>
                    </div>
                    ${todaySegments.length > 0 ? `
                        <div class="ops-today-segments">
                            ${todaySegments.map(seg => createHorizontalSegmentHtml(seg, false)).join('')}
                        </div>
                    ` : ''}
                </div>

                <!-- UPCOMING Section -->
                ${upcomingSegments.length > 0 ? `
                    <div class="ops-timeline-zone ops-timeline-zone--upcoming">
                        <div class="ops-zone-label">
                            <i class="fas fa-calendar-alt"></i>
                            <span>กำลังจะถึง</span>
                        </div>
                        <div class="ops-zone-segments">
                            ${upcomingSegments.map(seg => createHorizontalSegmentHtml(seg, false)).join('')}
                        </div>
                    </div>
                ` : `
                    <div class="ops-timeline-zone ops-timeline-zone--empty">
                        <div class="ops-zone-empty">
                            <i class="fas fa-check-circle"></i>
                            <span>ไม่มีงานรอ</span>
                        </div>
                    </div>
                `}
            </div>
        </div>
    `;

    $container.html(timelineHtml);

    // เพิ่ม drag-to-scroll สำหรับ timeline
    initTimelineDragScroll();

    // Scroll ไปที่ TODAY ให้อยู่ตรงกลาง
    scrollToTodayCenter();

    // Summary
    const totalSegments = segments.length;
    const overdueCount = overdueSegments.length;
    const todayCount = todaySegments.length;
    const allOnTime = segments.every(s => s.isOnTime);

    // === คำนวณงานที่ต้องเสร็จวันนี้ (Overdue + Today) ===
    const mustFinishTodaySegments = [...overdueSegments, ...todaySegments];
    const mustFinishTodayQty = mustFinishTodaySegments.reduce((sum, s) => sum + s.totalQty, 0);

    // คำนวณ avgBaseTime รวมของงานที่ต้องเสร็จวันนี้
    let todayAvgBaseTime = 0;
    if (mustFinishTodaySegments.length > 0) {
        const totalWeightedTime = mustFinishTodaySegments.reduce((sum, s) => sum + (s.avgBaseTime * s.totalQty), 0);
        todayAvgBaseTime = totalWeightedTime / mustFinishTodayQty;
    }

    // คำนวณว่าต้องใช้กี่คนเพื่อเสร็จใน 1 วัน
    let todayRequiredWorkers = workers;
    let todayAdditionalWorkers = 0;
    let todayIsOnTime = true;

    if (mustFinishTodayQty > 0) {
        const todayPlan = calculateProductionPlan(mustFinishTodayQty, workers, todayAvgBaseTime);
        todayIsOnTime = todayPlan.actualDays <= 1;

        if (!todayIsOnTime) {
            const result = calculateRequiredWorkers(mustFinishTodayQty, 1, workers, todayAvgBaseTime);
            todayRequiredWorkers = result.requiredWorkers;
            todayAdditionalWorkers = result.additionalWorkers;
        }
    }

    // สถานะโดยรวม
    const overallStatusText = overdueCount > 0
        ? `<span class="ops-text-danger">🔴 ${overdueCount} overdue</span>`
        : allOnTime
            ? '<span class="ops-text-success">✓ ผ่านทั้งหมด</span>'
            : '<span class="ops-text-warning">⚠ มี segment ไม่ทัน</span>';

    // คำแนะนำสำหรับงานที่ต้องเสร็จวันนี้
    let workersRecHtml = '';
    if (mustFinishTodayQty > 0) {
        if (todayIsOnTime) {
            workersRecHtml = `
                <div class="ops-timeline-workers-rec ops-timeline-workers-rec--success">
                    <strong class="ops-text-success"><i class="fas fa-check-circle"></i> ${workers} คน เพียงพอ</strong>
                    <div class="ops-text-muted">งานที่ต้องเสร็จวันนี้ ${mustFinishTodayQty.toLocaleString()} ชิ้น ทำได้ทัน</div>
                </div>`;
        } else {
            workersRecHtml = `
                <div class="ops-timeline-workers-rec ops-timeline-workers-rec--warning">
                    <strong class="ops-text-danger"><i class="fas fa-user-plus"></i> ต้องเพิ่ม +${todayAdditionalWorkers} คน (รวม ${todayRequiredWorkers} คน)</strong>
                    <div class="ops-text-muted">เพื่อทำงาน ${mustFinishTodayQty.toLocaleString()} ชิ้น (Overdue + วันนี้) ให้เสร็จใน 1 วัน</div>
                </div>`;
        }
    } else {
        workersRecHtml = `
            <div class="ops-timeline-workers-rec ops-timeline-workers-rec--success">
                <strong class="ops-text-success"><i class="fas fa-check-circle"></i> ไม่มีงานที่ต้องเสร็จวันนี้</strong>
            </div>`;
    }

    $summary.html(`
        <div class="ops-timeline-summary-item">
            <div class="ops-timeline-summary-value">${totalSegments}</div>
            <div class="ops-timeline-summary-label">จำนวน Segments</div>
        </div>
        <div class="ops-timeline-summary-item">
            <div class="ops-timeline-summary-value ${overdueCount > 0 ? 'ops-text-danger' : ''}">${overdueCount}</div>
            <div class="ops-timeline-summary-label">Overdue</div>
        </div>
        <div class="ops-timeline-summary-item">
            <div class="ops-timeline-summary-value ops-text-warning">${todayCount}</div>
            <div class="ops-timeline-summary-label">วันนี้</div>
        </div>
        <div class="ops-timeline-summary-item">
            <div class="ops-timeline-summary-value ${mustFinishTodayQty > 0 ? 'ops-text-danger' : ''}">${mustFinishTodayQty.toLocaleString()}</div>
            <div class="ops-timeline-summary-label">ต้องเสร็จวันนี้ (ชิ้น)</div>
        </div>
        <div class="ops-timeline-summary-item">
            <div class="ops-timeline-summary-value">${overallStatusText}</div>
            <div class="ops-timeline-summary-label">สถานะโดยรวม</div>
        </div>
        ${workersRecHtml}
    `);
}

// Drag-to-scroll สำหรับ timeline
function initTimelineDragScroll() {
    const timeline = document.querySelector('.ops-horizontal-timeline');
    if (!timeline) return;

    let isDown = false;
    let startX;
    let scrollLeft;

    timeline.addEventListener('mousedown', (e) => {
        // ไม่ทำงานถ้าคลิกที่ปุ่มหรือ link
        if (e.target.closest('button, a, .ops-hseg-card')) return;

        isDown = true;
        timeline.classList.add('ops-dragging');
        startX = e.pageX - timeline.offsetLeft;
        scrollLeft = timeline.scrollLeft;
    });

    timeline.addEventListener('mouseleave', () => {
        isDown = false;
        timeline.classList.remove('ops-dragging');
    });

    timeline.addEventListener('mouseup', () => {
        isDown = false;
        timeline.classList.remove('ops-dragging');
    });

    timeline.addEventListener('mousemove', (e) => {
        if (!isDown) return;
        e.preventDefault();
        const x = e.pageX - timeline.offsetLeft;
        const walk = (x - startX) * 1.5; // ความเร็วในการ scroll
        timeline.scrollLeft = scrollLeft - walk;
    });
}

// Scroll timeline ไปที่ TODAY ให้อยู่ตรงกลาง
function scrollToTodayCenter() {
    const timeline = document.querySelector('.ops-horizontal-timeline');
    const todayElement = document.querySelector('.ops-timeline-today');

    if (!timeline || !todayElement) return;

    // รอให้ DOM render เสร็จก่อน
    setTimeout(() => {
        const timelineWidth = timeline.clientWidth;

        // คำนวณตำแหน่งที่ต้อง scroll เพื่อให้ today อยู่ตรงกลาง
        const todayCenter = todayElement.offsetLeft + (todayElement.offsetWidth / 2);
        const scrollPosition = todayCenter - (timelineWidth / 2);

        // Scroll ไปที่ตำแหน่งที่คำนวณ (smooth)
        timeline.scrollTo({
            left: Math.max(0, scrollPosition),
            behavior: 'smooth'
        });
    }, 100);
}

// แสดง popup รายละเอียด orders ใน segment
function showSegmentOrders(segmentIndex) {
    const segment = currentSegments.find(s => s.index === segmentIndex);
    if (!segment) return;

    // สร้างตาราง orders
    const ordersTableHtml = segment.orders.map((order, idx) => {
        const qty = order.qty || 0;
        const packedQty = order.packedQty || 0;
        const remaining = qty - packedQty;
        const baseTime = order.baseTime || 0;
        const vipBadge = order.customerGroup === 1
            ? '<span class="ops-badge ops-badge--warning" style="margin-left: 0.5rem;"><i class="fas fa-star"></i></span>'
            : '';

        return `
            <tr>
                <td>${idx + 1}</td>
                <td><strong>${order.orderNo || '-'}</strong>${vipBadge}</td>
                <td>${order.custCode || '-'}</td>
                <td class="text-end">${qty.toLocaleString()}</td>
                <td class="text-end">${packedQty.toLocaleString()}</td>
                <td class="text-end">${remaining.toLocaleString()}</td>
                <td class="text-center">${baseTime} นาที</td>
            </tr>
        `;
    }).join('');

    // สถานะ
    let statusBadge = '';
    if (segment.isOverdue) {
        statusBadge = '<span class="badge bg-danger">เลยกำหนด</span>';
    } else if (segment.daysUntilDue === 0) {
        statusBadge = '<span class="badge bg-warning text-dark">วันนี้</span>';
    } else {
        statusBadge = '<span class="badge bg-primary">กำลังจะถึง</span>';
    }

    // สร้าง modal content
    const modalHtml = `
        <div class="modal fade" id="segmentOrdersModal" tabindex="-1">
            <div class="modal-dialog modal-lg modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            <i class="fas fa-box"></i>
                            รายละเอียด Segment - Due: ${formatThaiDate(segment.endDate)}
                            ${statusBadge}
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <!-- Summary -->
                        <div class="row mb-3">
                            <div class="col-md-3">
                                <div class="card bg-light">
                                    <div class="card-body text-center py-2">
                                        <div class="fs-4 fw-bold">${segment.orderCount}</div>
                                        <small class="text-muted">ออเดอร์</small>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="card bg-light">
                                    <div class="card-body text-center py-2">
                                        <div class="fs-4 fw-bold">${segment.totalQty.toLocaleString()}</div>
                                        <small class="text-muted">ชิ้น</small>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="card bg-light">
                                    <div class="card-body text-center py-2">
                                        <div class="fs-4 fw-bold">${segment.daysNeeded.toFixed(1)}</div>
                                        <small class="text-muted">วันที่ใช้</small>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="card bg-light">
                                    <div class="card-body text-center py-2">
                                        <div class="fs-4 fw-bold ${segment.isOnTime ? 'text-success' : 'text-danger'}">
                                            ${segment.isOnTime ? '✓ ทัน' : `+${segment.additionalWorkers} คน`}
                                        </div>
                                        <small class="text-muted">${segment.isOnTime ? 'สถานะ' : 'ต้องเพิ่ม'}</small>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Orders Table -->
                        <div class="table-responsive">
                            <table class="table table-sm table-hover ops-table">
                                <thead>
                                    <tr>
                                        <th>#</th>
                                        <th>Order No</th>
                                        <th>ลูกค้า</th>
                                        <th class="text-end">จำนวน</th>
                                        <th class="text-end">แพ็คแล้ว</th>
                                        <th class="text-end">คงเหลือ</th>
                                        <th class="text-center">เวลา/ชิ้น</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${ordersTableHtml}
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">ปิด</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // ลบ modal เก่าถ้ามี
    $('#segmentOrdersModal').remove();

    // เพิ่ม modal ใหม่
    $('body').append(modalHtml);

    // แสดง modal
    const modal = new bootstrap.Modal(document.getElementById('segmentOrdersModal'));
    modal.show();
}

// สร้าง HTML สำหรับ Segment แนวนอน
function createHorizontalSegmentHtml(segment, isOverdue) {
    const isToday = !isOverdue && segment.daysUntilDue === 0;

    // กำหนด class ตามสถานะ: overdue=แดง, today=เหลือง, upcoming=ฟ้า
    let segmentClass = '';
    if (isOverdue) {
        segmentClass = 'ops-hseg--overdue';
    } else if (isToday) {
        segmentClass = 'ops-hseg--today';
    } else {
        segmentClass = 'ops-hseg--upcoming';
    }

    // เพิ่ม class สำหรับสถานะทันหรือไม่ทัน
    const statusClass = segment.isOnTime ? 'ops-hseg--success' : 'ops-hseg--warning';

    // Calendar icon color based on segment type
    let calendarIconClass = '';
    if (isOverdue) {
        calendarIconClass = 'ops-icon--red';
    } else if (isToday) {
        calendarIconClass = 'ops-icon--yellow';
    } else {
        calendarIconClass = 'ops-icon--blue';
    }

    // Time info
    const timeInfo = isOverdue
        ? `เลยมา ${segment.daysOverdue} วัน`
        : isToday ? 'วันนี้' : `อีก ${segment.daysUntilDue} วัน`;

    // Recommendation
    const recommendation = !segment.isOnTime
        ? `<div class="ops-hseg-rec">+${segment.additionalWorkers} คน</div>`
        : '';

    return `
        <div class="ops-hseg ${segmentClass} ${statusClass}" title="${formatThaiDate(segment.endDate)}">
            <div class="ops-hseg-connector"></div>
            <div class="ops-hseg-node"></div>
            <div class="ops-hseg-card" data-segment-index="${segment.index}" onclick="showSegmentOrders(${segment.index})">
                <div class="ops-hseg-header">
                    <span class="ops-hseg-date"><i class="fas fa-calendar-alt ${calendarIconClass}"></i> ${formatThaiDate(segment.endDate)}</span>
                </div>
                <div class="ops-hseg-time">${timeInfo}</div>
                <div class="ops-hseg-stats">
                    <div class="ops-hseg-stat">
                        <span class="ops-hseg-stat-value">${segment.totalQty.toLocaleString()}</span>
                        <span class="ops-hseg-stat-label">ชิ้น</span>
                    </div>
                    <div class="ops-hseg-stat">
                        <span class="ops-hseg-stat-value">${segment.orderCount}</span>
                        <span class="ops-hseg-stat-label">ออเดอร์</span>
                    </div>
                </div>
                <div class="ops-hseg-days">
                    <span>ใช้ ${segment.daysNeeded.toFixed(1)} วัน</span>
                    <span class="${segment.timeDiff >= 0 ? 'ops-text-success' : 'ops-text-danger'}">
                        ${segment.timeDiff >= 0 ? '+' : ''}${segment.timeDiff.toFixed(1)}
                    </span>
                </div>
                ${recommendation}
            </div>
        </div>
    `;
}

// สร้าง HTML สำหรับแต่ละ segment
function createSegmentHtml(segment, _workers, isOverdue) {
    const statusClass = segment.isOnTime ? 'ops-timeline-segment--success' : 'ops-timeline-segment--warning';
    const overdueClass = isOverdue ? 'ops-timeline-segment--overdue' : '';

    // Status text
    let statusText = '';
    let statusBadgeClass = '';
    if (isOverdue) {
        statusText = `🔴 เลยมา ${segment.daysOverdue} วัน`;
        statusBadgeClass = 'ops-segment-status--overdue';
    } else if (segment.isOnTime) {
        statusText = '✓ ทันกำหนด';
        statusBadgeClass = 'ops-segment-status--success';
    } else {
        statusText = '⚠ ไม่ทัน';
        statusBadgeClass = 'ops-segment-status--warning';
    }

    const timeDiffClass = segment.timeDiff >= 0 ? 'ops-text-success' : 'ops-text-danger';
    const timeDiffPrefix = segment.timeDiff > 0 ? '+' : '';

    // Progress bar calculation
    const progressPercent = segment.effectiveAvailableDays > 0
        ? Math.min((segment.daysNeeded / segment.effectiveAvailableDays) * 100, 100)
        : 100;

    // Date info
    let dateInfoHtml = '';
    if (isOverdue) {
        dateInfoHtml = `
            <div class="ops-segment-dates ops-segment-dates--overdue">
                <i class="fas fa-exclamation-circle"></i>
                Due: ${formatThaiDate(segment.endDate)}
                <span class="ops-text-danger" style="font-weight: 600;">(เลยมา ${segment.daysOverdue} วัน)</span>
                <span class="ops-text-muted">• ต้องเสร็จวันนี้</span>
            </div>
        `;
    } else {
        dateInfoHtml = `
            <div class="ops-segment-dates">
                <i class="fas fa-calendar-alt"></i>
                Due: ${formatThaiDate(segment.endDate)}
                <span class="ops-text-muted">(${segment.daysUntilDue === 0 ? 'วันนี้' : `อีก ${segment.daysUntilDue} วัน`})</span>
                ${segment.carryOverFromPrevious !== 0 ? `
                    <span class="${segment.carryOverFromPrevious > 0 ? 'ops-text-success' : 'ops-text-danger'}" style="font-size: 0.75rem; margin-left: 0.5rem;">
                        (${segment.carryOverFromPrevious > 0 ? '+' : ''}${segment.carryOverFromPrevious.toFixed(2)} จากช่วงก่อน)
                    </span>
                ` : ''}
            </div>
        `;
    }

    // Recommendation HTML (ถ้าไม่ทัน)
    let recommendationHtml = '';
    if (!segment.isOnTime) {
        const targetText = isOverdue ? 'เสร็จวันนี้' : 'ทันกำหนด';
        recommendationHtml = `
            <div class="ops-segment-recommendation">
                <i class="fas fa-user-plus"></i>
                <span>ต้องเพิ่ม <strong>+${segment.additionalWorkers} คน</strong> (รวม ${segment.requiredWorkers} คน) ถึงจะ${targetText}</span>
            </div>
        `;
    }

    return `
        <div class="ops-timeline-segment ${statusClass} ${overdueClass}">
            <div class="ops-segment-header">
                <div class="ops-segment-title">
                    <span class="ops-segment-number">${segment.index}</span>
                    Due: ${formatThaiDate(segment.endDate)}
                </div>
                <span class="ops-segment-status ${statusBadgeClass}">${statusText}</span>
            </div>

            ${dateInfoHtml}

            <div class="ops-segment-body">
                <div class="ops-segment-stat">
                    <div class="ops-segment-stat-value">${segment.totalQty.toLocaleString()}</div>
                    <div class="ops-segment-stat-label">ชิ้น</div>
                </div>
                <div class="ops-segment-stat">
                    <div class="ops-segment-stat-value">${segment.orderCount}</div>
                    <div class="ops-segment-stat-label">ออเดอร์</div>
                </div>
                <div class="ops-segment-stat">
                    <div class="ops-segment-stat-value">${segment.daysNeeded.toFixed(2)}</div>
                    <div class="ops-segment-stat-label">วันที่ใช้</div>
                </div>
                <div class="ops-segment-stat">
                    <div class="ops-segment-stat-value">${segment.effectiveAvailableDays.toFixed(2)}</div>
                    <div class="ops-segment-stat-label">วันที่มี</div>
                </div>
                <div class="ops-segment-stat">
                    <div class="ops-segment-stat-value ${timeDiffClass}">${timeDiffPrefix}${segment.timeDiff.toFixed(2)}</div>
                    <div class="ops-segment-stat-label">ส่วนต่าง</div>
                </div>
            </div>

            <div class="ops-segment-progress">
                <div class="ops-segment-progress-bar ${!segment.isOnTime ? 'ops-segment-progress-bar--warning' : ''}" style="width: ${progressPercent}%;"></div>
            </div>

            ${recommendationHtml}

            ${segment.carryOverToNext !== 0 ? `
                <div class="ops-segment-carry ${segment.carryOverToNext > 0 ? 'ops-segment-carry--positive' : 'ops-segment-carry--negative'}">
                    <i class="fas ${segment.carryOverToNext > 0 ? 'fa-arrow-right' : 'fa-exclamation-circle'}"></i>
                    <span>→ ${segment.carryOverToNext > 0 ? '+' : ''}${segment.carryOverToNext.toFixed(2)} วัน${segment.carryOverToNext > 0 ? ' เหลือส่งต่อ' : ' ค้างไป segment ถัดไป'}</span>
                </div>
            ` : ''}
        </div>
    `;
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

// ====== TEST FUNCTION ======
// ฟังก์ชันทดสอบ Timeline ด้วยข้อมูลจำลอง
function testTimeline() {
    // ข้อมูลทดสอบ
    const testData = [
        {"orderNo":"25001023","custCode":"BG8","customerGroup":5,"article":"124670074","qty":1200,"sendToPackQty":444,"operateDay":0.04705882352941176,"sendToPackOperateDay":0.017411764705882352,"dueDate":"2026-01-24T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001091","custCode":"DDJ64","customerGroup":4,"article":"230600013.5","qty":2750,"sendToPackQty":1129,"operateDay":0.10784313725490188,"sendToPackOperateDay":0.044274509803921565,"dueDate":"2026-01-31T00:00:00","prodType":"PENDANT","baseTime":0.6},
        {"orderNo":"25001094","custCode":"AAJ57","customerGroup":4,"article":"224630012.1","qty":15,"sendToPackQty":0,"operateDay":0.000588235294117647,"sendToPackOperateDay":0,"dueDate":"2026-01-28T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001095","custCode":"AAJ57","customerGroup":4,"article":"061640005.10","qty":305,"sendToPackQty":102,"operateDay":0.011960784313725489,"sendToPackOperateDay":0.004,"dueDate":"2026-01-28T00:00:00","prodType":"NOSE STUD","baseTime":0.6},
        {"orderNo":"25001104","custCode":"AAS3","customerGroup":4,"article":"124630230","qty":320,"sendToPackQty":63,"operateDay":0.012549019607843144,"sendToPackOperateDay":0.0024705882352941176,"dueDate":"2026-01-24T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001114","custCode":"JP1","customerGroup":1,"article":"114680007","qty":2218,"sendToPackQty":303,"operateDay":0.08698039215686272,"sendToPackOperateDay":0.01188235294117647,"dueDate":"2026-01-31T00:00:00","prodType":"RING","baseTime":0.6},
        {"orderNo":"25001117","custCode":"AAS3","customerGroup":4,"article":"120600277.5","qty":280,"sendToPackQty":50,"operateDay":0.010980392156862745,"sendToPackOperateDay":0.00196078431372549,"dueDate":"2026-01-31T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001118","custCode":"SAC21","customerGroup":1,"article":"062540010","qty":6960,"sendToPackQty":343,"operateDay":0.27294117647058824,"sendToPackOperateDay":0.013450980392156862,"dueDate":"2026-01-31T00:00:00","prodType":"NOSE STUD","baseTime":0.6},
        {"orderNo":"25001120","custCode":"CCJ82","customerGroup":1,"article":"110670007.1","qty":1350,"sendToPackQty":0,"operateDay":0.052941176470588235,"sendToPackOperateDay":0,"dueDate":"2026-02-05T00:00:00","prodType":"RING","baseTime":0.6},
        {"orderNo":"25001121","custCode":"CCJ82","customerGroup":1,"article":"110670007.1","qty":1280,"sendToPackQty":0,"operateDay":0.05019607843137255,"sendToPackOperateDay":0,"dueDate":"2026-02-05T00:00:00","prodType":"RING","baseTime":0.6},
        {"orderNo":"25001122","custCode":"CCJ82","customerGroup":1,"article":"110670007.1","qty":2360,"sendToPackQty":0,"operateDay":0.09254901960784313,"sendToPackOperateDay":0,"dueDate":"2026-02-05T00:00:00","prodType":"RING","baseTime":0.6},
        {"orderNo":"25001123","custCode":"CCJ82","customerGroup":1,"article":"124680011.1.L","qty":730,"sendToPackQty":0,"operateDay":0.028627450980392155,"sendToPackOperateDay":0,"dueDate":"2026-02-05T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001125","custCode":"CCJ82","customerGroup":1,"article":"1200008.6.L","qty":620,"sendToPackQty":620,"operateDay":0.02431372549019608,"sendToPackOperateDay":0.02431372549019608,"dueDate":"2026-02-05T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001126","custCode":"CCJ82","customerGroup":1,"article":"1200008.6.L","qty":550,"sendToPackQty":549,"operateDay":0.021568627450980392,"sendToPackOperateDay":0.021529411764705877,"dueDate":"2026-02-05T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001127","custCode":"CCJ82","customerGroup":1,"article":"1200008.6.L","qty":1070,"sendToPackQty":828.5,"operateDay":0.04196078431372549,"sendToPackOperateDay":0.03247058823529411,"dueDate":"2026-02-05T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001128","custCode":"CCJ82","customerGroup":1,"article":"1200008.6.L","qty":750,"sendToPackQty":750,"operateDay":0.029411764705882353,"sendToPackOperateDay":0.029411764705882353,"dueDate":"2026-02-05T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001130","custCode":"CCJ82","customerGroup":1,"article":"1200006.60.L","qty":4650,"sendToPackQty":4606.5,"operateDay":0.18235294117647063,"sendToPackOperateDay":0.18062745098039212,"dueDate":"2026-01-22T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001133","custCode":"CCJ82","customerGroup":1,"article":"1200006.50.L","qty":11630,"sendToPackQty":11654,"operateDay":0.4560784313725489,"sendToPackOperateDay":0.4570196078431372,"dueDate":"2026-01-22T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001134","custCode":"CCJ82","customerGroup":1,"article":"114620005.1.L","qty":3750,"sendToPackQty":3751,"operateDay":0.1470588235294118,"sendToPackOperateDay":0.1470980392156863,"dueDate":"2026-01-22T00:00:00","prodType":"RING","baseTime":0.6},
        {"orderNo":"25001139","custCode":"CCJ152","customerGroup":4,"article":"124640031","qty":510,"sendToPackQty":49.5,"operateDay":0.019999999999999987,"sendToPackOperateDay":0.0019215686274509805,"dueDate":"2026-01-29T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001142","custCode":"CCJ148","customerGroup":1,"article":"1200006.12DMH","qty":2350,"sendToPackQty":1428.5,"operateDay":0.09215686274509804,"sendToPackOperateDay":0.056,"dueDate":"2026-02-06T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001144","custCode":"CCJ82","customerGroup":1,"article":"220550111.95L","qty":500,"sendToPackQty":0,"operateDay":0.0196078431372549,"sendToPackOperateDay":0,"dueDate":"2026-01-29T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001145","custCode":"CCJ82","customerGroup":1,"article":"2200436.8.L","qty":15000,"sendToPackQty":0,"operateDay":0.5882352941176471,"sendToPackOperateDay":0,"dueDate":"2026-01-29T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001146","custCode":"CCJ82","customerGroup":1,"article":"220550111.3.L","qty":6400,"sendToPackQty":0,"operateDay":0.25098039215686274,"sendToPackOperateDay":0,"dueDate":"2026-01-29T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001147","custCode":"JP32","customerGroup":3,"article":"S130610024","qty":30,"sendToPackQty":0,"operateDay":0.001176470588235294,"sendToPackOperateDay":0,"dueDate":"2026-01-31T00:00:00","prodType":"PENDANT","baseTime":0.6},
        {"orderNo":"25001148","custCode":"BBJ60","customerGroup":5,"article":"131610027","qty":40,"sendToPackQty":0,"operateDay":0.001568627450980392,"sendToPackOperateDay":0,"dueDate":"2026-02-04T00:00:00","prodType":"PENDANT","baseTime":0.6},
        {"orderNo":"25001152","custCode":"CCJ82","customerGroup":1,"article":"1200006.40.L","qty":2480,"sendToPackQty":1170.5,"operateDay":0.09725490196078436,"sendToPackOperateDay":0.04588235294117647,"dueDate":"2026-01-29T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001153","custCode":"CCJ82","customerGroup":1,"article":"1200007.14.L","qty":3300,"sendToPackQty":2667,"operateDay":0.1294117647058824,"sendToPackOperateDay":0.10458823529411765,"dueDate":"2026-01-29T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001154","custCode":"CCJ82","customerGroup":1,"article":"1200006.12.L","qty":2320,"sendToPackQty":1199,"operateDay":0.09098039215686274,"sendToPackOperateDay":0.04701960784313726,"dueDate":"2026-01-29T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"25001155","custCode":"CCJ82","customerGroup":1,"article":"1200006.50.L","qty":1600,"sendToPackQty":1362,"operateDay":0.06274509803921569,"sendToPackOperateDay":0.05341176470588235,"dueDate":"2026-01-29T00:00:00","prodType":"EARRING","baseTime":0.6},
        {"orderNo":"26000010","custCode":"CCJ82","customerGroup":1,"article":"Z260002","qty":6,"sendToPackQty":0,"operateDay":0.00023529411764705886,"sendToPackOperateDay":0,"dueDate":"2026-01-30T00:00:00","prodType":"NECKLACE","baseTime":0.6},
        {"orderNo":"26000048","custCode":"E10","customerGroup":2,"article":"03HC0122","qty":389,"sendToPackQty":0,"operateDay":0.015254901960784311,"sendToPackOperateDay":0,"dueDate":"2026-01-31T00:00:00","prodType":"PENDANT","baseTime":0.6}
    ];

    // กำหนด ordersData
    ordersData = testData;

    console.log('=== TEST DATA LOADED ===');
    console.log('Total orders:', ordersData.length);

    // วิเคราะห์ข้อมูลตาม Due Date
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    console.log('Today:', today.toISOString().split('T')[0]);

    const dueDateCounts = {};
    ordersData.forEach(order => {
        const dueDate = order.dueDate.split('T')[0];
        const dueDateObj = new Date(order.dueDate);
        dueDateObj.setHours(0, 0, 0, 0);
        const isOverdue = dueDateObj < today;

        if (!dueDateCounts[dueDate]) {
            dueDateCounts[dueDate] = { count: 0, qty: 0, isOverdue };
        }
        dueDateCounts[dueDate].count++;
        dueDateCounts[dueDate].qty += order.qty;
    });

    console.log('\n=== DUE DATE BREAKDOWN ===');
    Object.keys(dueDateCounts).sort().forEach(date => {
        const info = dueDateCounts[date];
        const status = info.isOverdue ? '🔴 OVERDUE' : '📅 UPCOMING';
        console.log(`${date}: ${info.count} orders, ${info.qty.toLocaleString()} pcs ${status}`);
    });

    // เรียก filterAndCalculate เพื่อแสดง Timeline
    filterAndCalculate();

    console.log('\n=== TIMELINE RENDERED ===');
}

