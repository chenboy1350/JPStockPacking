using JPStockPacking.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace JPStockPacking.Services.Helper
{
    public static class ReportExtension
    {
        public static void CreateLotItemCard(this IContainer container, SendToPackLots item)
        {
            container.Border(1)
                            .BorderColor(Colors.Black)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Cell().RowSpan(3).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));

                                if (!string.IsNullOrEmpty(item.Approver))
                                {
                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo} ✵").FontSize(8).SemiBold();
                                }
                                else
                                {
                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8).SemiBold();
                                }

                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดสั่ง {item.TtQty} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดแจ้ง {item.TtQtyToPack} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(7).Element(CellStyle).AlignLeft().Text($"{item.TdesArt}/{item.TdesFn}").FontSize(6).SemiBold();
                                if (!string.IsNullOrEmpty(item.Approver)) table.Cell().ColumnSpan(7).Element(CellStyle).AlignLeft().Text($"ยืนยันโดย : {item.Approver}").FontSize(6).SemiBold();

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .Border(0.5f)
                                        .BorderColor(Colors.Black)
                                        .Padding(3);
                                }
                            });
        }

        public static void CreateLotSizeItemCard(this IContainer container, SendToPackLots item)
        {
            container.Border(1)
                            .BorderColor(Colors.Black)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });


                                if (item.Size.Any(x => !string.IsNullOrEmpty(x.Approver)))
                                {
                                    table.Cell().RowSpan(9).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));
                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo} ✵").FontSize(8).SemiBold();
                                }
                                else
                                {
                                    table.Cell().RowSpan(8).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));
                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8).SemiBold();
                                }

                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดสั่ง {item.TtQty} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดแจ้ง {item.TtQtyToPack} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(13).Element(CellStyle).AlignLeft().Text($"{item.TdesArt}/{item.TdesFn}").FontSize(6).SemiBold();
                                if (item.Size != null && item.Size.Count != 0)
                                {
                                    var approvers = string.Join(", ", item.Size.Where(s => !string.IsNullOrEmpty(s.Approver)).Select(s => s.Approver));

                                    if (!string.IsNullOrEmpty(approvers))
                                    {
                                        table.Cell().ColumnSpan(13).Element(CellStyle).AlignLeft().Text($"ยืนยันโดย : {approvers}").FontSize(6).SemiBold();
                                    }

                                    table.Cell().Element(CellStyle).AlignRight().Text($"ขนาด").FontSize(7).SemiBold();

                                    for (int i = 0; i < 12; i += 1)
                                    {
                                        if (item.Size.Count >= i + 1)
                                        {
                                            var a = item.Size[i];
                                            table.Cell().Element(CellStyle).AlignCenter().Text($"{a.S}").FontSize(7).SemiBold();
                                        }
                                        else
                                        {
                                            table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                        }
                                    }

                                    table.Cell().Element(CellStyle).AlignRight().Text($"จำนวนสั่ง").FontSize(7);

                                    for (int i = 0; i < 12; i += 1)
                                    {
                                        if (item.Size.Count >= i + 1)
                                        {
                                            var a = item.Size[i];
                                            table.Cell().Element(CellStyle).AlignCenter().Text($"{a.Q}").FontSize(7);
                                        }
                                        else
                                        {
                                            table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                        }
                                    }

                                    table.Cell().Element(CellStyle).AlignRight().Text($"จำนวนแจ้ง").FontSize(7).Underline();

                                    for (int i = 0; i < 12; i += 1)
                                    {
                                        if (item.Size.Count >= i + 1)
                                        {
                                            var a = item.Size[i];
                                            if (!string.IsNullOrEmpty(a.Approver))
                                            {
                                                table.Cell().Element(CellStyle).AlignCenter().Text($"{a.TtQtyToPack} ✵").FontSize(7).Underline();
                                            }
                                            else
                                            {
                                                table.Cell().Element(CellStyle).AlignCenter().Text($"{a.TtQtyToPack}").FontSize(7).Underline();
                                            }
                                        }
                                        else
                                        {
                                            table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                        }
                                    }

                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .Border(0.5f)
                                        .BorderColor(Colors.Black)
                                        .Padding(3);
                                }
                            });
        }

        public static void CreateLotItemWithPriceCard(this IContainer container, SendToPackLots item)
        {
            container.Border(1)
                            .BorderColor(Colors.Black)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                if (!string.IsNullOrEmpty(item.Approver))
                                {
                                    table.Cell().RowSpan(4).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));
                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo} ✵").FontSize(8).SemiBold();
                                }
                                else
                                {
                                    table.Cell().RowSpan(3).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));
                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8).SemiBold();
                                }
                                var Unit = item.Tunit == "คู่" ? "PR" : "PC";
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดสั่ง {item.TtQty} {Unit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดแจ้ง {item.TtQtyToPack} {Unit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(7).Element(CellStyle).AlignLeft().Text($"{item.TdesArt.Trim()}/{item.TdesFn.Trim()}").FontSize(7).SemiBold();
                                if (!string.IsNullOrEmpty(item.Approver)) table.Cell().ColumnSpan(7).Element(CellStyle).AlignLeft().Text($"ยืนยันโดย : {item.Approver}").FontSize(6).SemiBold();
                                table.Cell().RowSpan(3).ColumnSpan(7).Element(CellStyle).AlignLeft().Text($"{item.EnSendQtyPrice.ToUpper()}").FontSize(8).SemiBold();

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .Border(0.5f)
                                        .BorderColor(Colors.Black)
                                        .Padding(3);
                                }
                            });
        }

        public static void CreateLotSizeItemWithPriceCard(this IContainer container, SendToPackLots item)
        {
            container.Border(1)
                            .BorderColor(Colors.Black)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                if (item.Size.Any(x => !string.IsNullOrEmpty(x.Approver)))
                                {
                                    table.Cell().RowSpan(9).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));
                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo} ✵").FontSize(8).SemiBold();
                                }
                                else
                                {
                                    table.Cell().RowSpan(8).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));
                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8).SemiBold();
                                }
                                var Unit = item.Tunit == "คู่" ? "PR" : "PC";
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดสั่ง {item.TtQty} {Unit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดแจ้ง {item.TtQtyToPack} {Unit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(6).Element(CellStyle).AlignCenter().Text($"{item.EnSendQtyPrice.ToUpper()}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(13).Element(CellStyle).AlignLeft().Text($"{item.TdesArt.Trim()}/{item.TdesFn.Trim()}").FontSize(7).SemiBold();

                                if (item.Size != null && item.Size.Count != 0)
                                {
                                    var approvers = string.Join(", ", item.Size.Where(s => !string.IsNullOrEmpty(s.Approver)).Select(s => s.Approver));

                                    if (!string.IsNullOrEmpty(approvers))
                                    {
                                        table.Cell().ColumnSpan(13).Element(CellStyle).AlignLeft().Text($"ยืนยันโดย : {approvers}").FontSize(6).SemiBold();
                                    }

                                    table.Cell().Element(CellStyle).AlignRight().Text($"ขนาด").FontSize(7).SemiBold();

                                    for (int i = 0; i < 12; i += 1)
                                    {
                                        if (item.Size.Count >= i + 1)
                                        {
                                            var a = item.Size[i];
                                            table.Cell().Element(CellStyle).AlignCenter().Text($"{a.S}").FontSize(7).SemiBold();
                                        }
                                        else
                                        {
                                            table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                        }
                                    }

                                    table.Cell().Element(CellStyle).AlignRight().Text($"จำนวนสั่ง").FontSize(7);

                                    for (int i = 0; i < 12; i += 1)
                                    {
                                        if (item.Size.Count >= i + 1)
                                        {
                                            var a = item.Size[i];
                                            table.Cell().Element(CellStyle).AlignCenter().Text($"{a.Q}").FontSize(7);
                                        }
                                        else
                                        {
                                            table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                        }
                                    }

                                    table.Cell().Element(CellStyle).AlignRight().Text($"จำนวนแจ้ง").FontSize(7).Underline();

                                    for (int i = 0; i < 12; i += 1)
                                    {
                                        if (item.Size.Count >= i + 1)
                                        {
                                            var a = item.Size[i];
                                            if (!string.IsNullOrEmpty(a.Approver))
                                            {
                                                table.Cell().Element(CellStyle).AlignCenter().Text($"{a.TtQtyToPack} ✵").FontSize(7).Underline();
                                            }
                                            else
                                            {
                                                table.Cell().Element(CellStyle).AlignCenter().Text($"{a.TtQtyToPack}").FontSize(7).Underline();
                                            }
                                        }
                                        else
                                        {
                                            table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                        }
                                    }

                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .Border(0.5f)
                                        .BorderColor(Colors.Black)
                                        .Padding(3);
                                }
                            });
        }

        private static void RenderItemImage(this IContainer container, byte[]? ImageBytes = null, string? ImagePath = null)
        {
            if (ImageBytes != null && ImageBytes.Length > 0)
            {
                container.Image(ImageBytes)
                    .FitArea()
                    .WithCompressionQuality(ImageCompressionQuality.High);
            }
            else if (!string.IsNullOrEmpty(ImagePath))
            {
                if (File.Exists(ImagePath))
                {
                    container.Image(ImagePath)
                        .FitArea()
                        .WithCompressionQuality(ImageCompressionQuality.High);
                }
                else
                {
                    container.RenderPlaceholder($"File not found:\n{Path.GetFileName(ImagePath)}");
                }
            }
            else
            {
                container.RenderPlaceholder("No Image Available");
            }
        }

        public static void RenderHeaderImage(this IContainer container, byte[]? ImageBytes = null, string? ImagePath = null)
        {
            if (ImageBytes != null && ImageBytes.Length > 0)
            {
                container.Image(ImageBytes)
                    .FitArea()
                    .WithCompressionQuality(ImageCompressionQuality.High);
            }
            else if (!string.IsNullOrEmpty(ImagePath))
            {
                if (File.Exists(ImagePath))
                {
                    container.Image(ImagePath)
                        .FitArea()
                        .WithCompressionQuality(ImageCompressionQuality.High);
                }
                else
                {
                    container.RenderPlaceholder($"File not found:\n{Path.GetFileName(ImagePath)}");
                }
            }
            else
            {
                container.RenderPlaceholder("No Image Available");
            }
        }

        private static void RenderPlaceholder(this IContainer container, string message = "No Image")
        {
            container.Background(Colors.Grey.Lighten3)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten1)
                .AlignCenter()
                .AlignMiddle()
                .Column(col =>
                {
                    col.Item().Text("💎")
                        .FontSize(24)
                        .AlignCenter()
                        .FontColor(Colors.Blue.Medium);

                    col.Item().PaddingTop(2).Text(message)
                        .FontSize(6)
                        .AlignCenter()
                        .FontColor(Colors.Grey.Darken1);
                });
        }

        public static void BreakReportHeader(this IContainer container)
        {
            container.PaddingBottom(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(30);
                    columns.RelativeColumn(30);
                });

                table.Cell().Element(CellStyle).AlignLeft().Text("รายการส่งซ่อม").FontSize(11).SemiBold();
                table.Cell().Element(CellStyle).AlignRight()
                     .Text($"วันที่ออกเอกสาร : {DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("th-TH"))}")
                     .FontSize(7);

                static IContainer CellStyle(IContainer c) => c.Padding(3);
            });
        }

        public static void BreakReportContent(this IContainer container, List<LostAndRepairModel> model)
        {
            container.Column(col =>
            {
                // ส่วนหัว
                col.Item().PaddingTop(5).Element(c => c.BreakReportHeader());

                // ตาราง - ใช้ ShowOnce() เพื่อไม่ให้แบ่งข้าม page
                col.Item().PaddingVertical(2).ShowOnce().Element(c =>
                {
                    c.Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        // Header row
                        table.Cell().Element(CellStyle).AlignCenter().Text("#").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("เลขที่ใบรับ").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("lotNo").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("รหัสลูกค้า\nOrderNo").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("ลำดับ").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("รหัสสินค้า").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("จำนวนที่\nส่งซ่อม").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("อาการ").FontSize(8);

                        // Data rows - จำกัดจำนวนแถวตามพื้นที่ที่มี
                        var maxRows = CalculateMaxRows(); // คำนวณจำนวนแถวสูงสุดที่แสดงได้
                        var displayModel = model.Take(maxRows).ToList();

                        foreach (var item in displayModel)
                        {
                            table.Cell().Element(CellStyle).AlignCenter().Text("#").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ReceiveNo}").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.LotNo}").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.CustCode}/{item.OrderNo}").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Article}").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.BreakQty}").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.BreakDescription}").FontSize(8);
                        }

                        // Footer signature
                        table.Cell().ColumnSpan(8).Padding(2).Column(row =>
                        {
                            row.Item()
                                .PaddingTop(60) // ลด padding เพื่อประหยัดพื้นที่
                                .Element(container =>
                                {
                                    container.Row(row =>
                                    {
                                        row.RelativeItem().AlignCenter().Column(col =>
                                        {
                                            col.Item().Text("(...............................)").AlignCenter();
                                            col.Item().Text("ผู้แจ้ง").FontSize(8).AlignCenter();
                                        });
                                        row.RelativeItem().AlignCenter().Column(col =>
                                        {
                                            col.Item().Text("(...............................)").AlignCenter();
                                            col.Item().Text("ผู้อนุมัติ").FontSize(8).AlignCenter();
                                        });
                                        row.RelativeItem().AlignCenter().Column(col =>
                                        {
                                            col.Item().Text("(...............................)").AlignCenter();
                                            col.Item().Text("ผู้รับ").FontSize(8).AlignCenter();
                                        });
                                    });
                                });
                        });
                    });
                });
            });

            static IContainer CellStyle(IContainer container)
            {
                return container
                    .Border(0.5f)
                    .BorderColor(Colors.Black)
                    .Padding(3);
            }
        }

        public static void LostReportHeader(this IContainer container, LostAndRepairModel model)
        {
            container.PaddingBottom(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Cell().ColumnSpan(6).Element(CellStyle).AlignCenter().Text($"รายการสูญหาย ล็อต : {model.LotNo}").FontSize(11).SemiBold();
                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text("");
                table.Cell().ColumnSpan(3).Element(CellStyle).AlignRight().Text($"วันที่ออกเอกสาร : {DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("th-TH"))}").FontSize(7);
                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text("");
                table.Cell().ColumnSpan(3).Element(CellStyle).AlignRight().Text($"วันที่จบแพ็คกิ้ง : {model.SeldDate1}").FontSize(7);

                static IContainer CellStyle(IContainer c) => c.Padding(3);
            });
        }

        public static void LostReportContent(this IContainer container, List<LostAndRepairModel> model, UserModel userModel)
        {
            container.Column(col =>
            {
                // ส่วนหัว
                col.Item().PaddingTop(5).Element(c => c.LostReportHeader(model.FirstOrDefault()!));

                // ตาราง - ใช้ ShowOnce() เพื่อไม่ให้แบ่งข้าม page
                col.Item().PaddingVertical(2).ShowOnce().Element(c =>
                {
                    c.Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        // Header row
                        table.Cell().Element(CellStyle).AlignCenter().Text("#").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("รหัสลูกค้าOrderNo").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("ลำดับ").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("รหัสสินค้า").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("จำนวนรวม").FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text("จำนวนที่สูญหาย").FontSize(8);

                        foreach (var item in model)
                        {
                            table.Cell().Element(CellStyle).AlignCenter().Text("#").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.CustCode}/{item.OrderNo}").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Article}").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.TtQty}").FontSize(8);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.LostQty}").FontSize(8);
                        }


                        // Footer signature
                        table.Cell().ColumnSpan(6).Padding(2).Column(row =>
                        {
                            row.Item()
                                .PaddingTop(60) // ลด padding เพื่อประหยัดพื้นที่
                                .Element(container =>
                                {
                                    container.Row(row =>
                                    {
                                        row.RelativeItem().AlignCenter().Column(col =>
                                        {
                                            col.Item().Text("(...............................)").AlignCenter();
                                            col.Item().Text("ผู้แจ้ง").FontSize(8).AlignCenter();
                                        });
                                        row.RelativeItem().AlignCenter().Column(col =>
                                        {
                                            col.Item().Text($"({userModel.FirstName} {userModel.LastName} ({userModel.NickName}))").AlignCenter();
                                            col.Item().Text("ผู้อนุมัติ").FontSize(8).AlignCenter();
                                        });
                                        row.RelativeItem().AlignCenter().Column(col =>
                                        {
                                            col.Item().Text("(...............................)").AlignCenter();
                                            col.Item().Text("ผู้รับ").FontSize(8).AlignCenter();
                                        });
                                    });
                                });
                        });
                    });
                });
            });

            static IContainer CellStyle(IContainer container)
            {
                return container
                    .Border(0.5f)
                    .BorderColor(Colors.Black)
                    .Padding(3);
            }
        }

        public static void SendToHeader(this IContainer container, TempPackPage tempPackPage)
        {
            container.PaddingBottom(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(30);
                    columns.RelativeColumn(30);
                    columns.RelativeColumn(30);
                    columns.RelativeColumn(30);
                    columns.RelativeColumn(30);
                    columns.RelativeColumn(30);
                    columns.RelativeColumn(30);
                });

                table.Cell().ColumnSpan(7).Element(CellStyle).AlignCenter().Text($"{tempPackPage.Title} ({tempPackPage.OrderNoAndCusCode})").FontSize(16).SemiBold();

                table.Cell().Element(CellStyle).AlignLeft().Text($"เลขที่ใบรับ").FontSize(11);
                table.Cell().ColumnSpan(2).Element(CellStyle).AlignLeft().Text($": {tempPackPage.Doc}").FontSize(11);

                table.Cell().ColumnSpan(3).Element(CellStyle).AlignRight().Text($"วันที่พิมพ์ : ").FontSize(11);
                table.Cell().ColumnSpan(1).Element(CellStyle).AlignRight().Text($"{DateTime.Now:dd/MM/yyyy}").FontSize(11);

                table.Cell().Element(CellStyle).AlignLeft().Text($"ส่งไป").FontSize(11);
                table.Cell().ColumnSpan(2).Element(CellStyle).AlignLeft().Text($": {tempPackPage.SendTo}").FontSize(11);

                table.Cell().ColumnSpan(3).Element(CellStyle).AlignRight().Text($"วันที่ออกเอกสาร : ").FontSize(11);
                table.Cell().ColumnSpan(1).Element(CellStyle).AlignRight().Text($"{tempPackPage.ListDate:dd/MM/yyyy}").FontSize(11);

                static IContainer CellStyle(IContainer c) => c.Padding(3);
            });
        }

        public static void SendToReportContent(this IContainer container, List<TempPackPage> model)
        {
            container.Column(col =>
            {
                foreach (var tempack in model)
                {
                    // ส่วนหัว
                    col.Item().PaddingTop(5).Element(c => c.SendToHeader(tempack));

                    // ตาราง (ShowOnce เพื่อไม่ให้แตกข้ามหน้า)
                    col.Item().PaddingVertical(2).ShowOnce().Element(c =>
                    {
                        c.Table(table =>
                        {
                            // คำนวณจำนวนคอลัมน์ตาม SendType
                            int columnCount = GetColumnCount(tempack.SendType);

                            // กำหนดโครงสร้างคอลัมน์
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                if (tempack.SendType == "KS" || tempack.SendType == "KM") columns.RelativeColumn(2);
                                if (tempack.SendType == "KS" || tempack.SendType == "KM" || tempack.SendType == "KX") columns.RelativeColumn(2);
                                if (tempack.SendType == "KX") columns.RelativeColumn(2);
                                if (tempack.SendType == "KS" || tempack.SendType == "KM") columns.RelativeColumn(3);
                                if (tempack.SendType == "KS" || tempack.SendType == "KM" || tempack.SendType == "KX") columns.RelativeColumn(2);
                                if (tempack.SendType == "KS" || tempack.SendType == "KM") columns.RelativeColumn(2);
                                if (tempack.SendType == "KM") columns.RelativeColumn(2);
                            });

                            // Header
                            table.Cell().Element(CellStyle).AlignCenter().Text("ListNo").FontSize(8);
                            if (tempack.SendType == "KS" || tempack.SendType == "KM") table.Cell().Element(CellStyle).AlignCenter().Text("เลขที่งานช่าง\nครั้งที่ส่ง").FontSize(8);
                            if (tempack.SendType == "KS" || tempack.SendType == "KM" || tempack.SendType == "KX") table.Cell().Element(CellStyle).AlignCenter().Text("รหัสสินค้า").FontSize(8);
                            if (tempack.SendType == "KX") table.Cell().Element(CellStyle).AlignCenter().Text("FN").FontSize(8);
                            if (tempack.SendType == "KS" || tempack.SendType == "KM") table.Cell().Element(CellStyle).AlignCenter().Text("ชื่อช่าง").FontSize(8);
                            if (tempack.SendType == "KS" || tempack.SendType == "KM" || tempack.SendType == "KX") table.Cell().Element(CellStyle).AlignCenter().Text("จำนวน").FontSize(8);
                            if (tempack.SendType == "KS" || tempack.SendType == "KM") table.Cell().Element(CellStyle).AlignCenter().Text("น้ำหนักรวม\n(กรัม)").FontSize(8);
                            if (tempack.SendType == "KM") table.Cell().Element(CellStyle).AlignCenter().Text("สาเหตุ").FontSize(8);

                            // Data rows
                            var maxRows = CalculateMaxRows();
                            var displayModel = tempack.TempPacks.Take(maxRows).ToList();

                            foreach (var item in displayModel)
                            {
                                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8);
                                if (tempack.SendType == "KS" || tempack.SendType == "KM") table.Cell().Element(CellStyle).AlignCenter().Text($"{item.JobBarcode}\n{item.NumSend}").FontSize(8);
                                if (tempack.SendType == "KS" || tempack.SendType == "KM" || tempack.SendType == "KX") table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Article}").FontSize(8);
                                if (tempack.SendType == "KX") table.Cell().Element(CellStyle).AlignCenter().Text($"{item.FinishingEN} / {item.FinishingTH}").FontSize(8);
                                if (tempack.SendType == "KS" || tempack.SendType == "KM") table.Cell().Element(CellStyle).AlignCenter().Text($"({item.EmpCode}) {item.Name}").FontSize(8);
                                if (tempack.SendType == "KS" || tempack.SendType == "KM") table.Cell().Element(CellStyle).AlignCenter().Text($"{item.OkTtl}").FontSize(8);
                                if (tempack.SendType == "KX") table.Cell().Element(CellStyle).AlignCenter().Text($"{(int)item.OkTtl} {item.Unit}").FontSize(8);
                                if (tempack.SendType == "KS" || tempack.SendType == "KM") table.Cell().Element(CellStyle).AlignCenter().Text($"{item.OkWg}").FontSize(8);
                                if (tempack.SendType == "KM") table.Cell().Element(CellStyle).AlignCenter().Text($"{item.BreakDescription}").FontSize(8);

                            }

                            // Footer: รวมยอด

                            table.Cell().ColumnSpan((uint)columnCount).Padding(2).Column(row =>
                            {
                                row.Item()
                                    .PaddingTop(20)
                                    .Element(container =>
                                    {
                                        container.Row(row =>
                                        {
                                            if (tempack.SendType != "KX")
                                            {
                                                row.RelativeItem().AlignCenter().Column(col =>
                                                {
                                                    col.Item().Text($"รวม     {displayModel.Where(w => w.Unit.Trim() == "PC").Sum(s => s.OkTtl)}     ชิ้น").AlignCenter().FontSize(11);
                                                });
                                                row.RelativeItem().AlignCenter().Column(col =>
                                                {
                                                    col.Item().Text($"รวม     {displayModel.Where(w => w.Unit.Trim() == "PR").Sum(s => s.OkTtl)}     คู่").AlignCenter().FontSize(11);
                                                });
                                                row.RelativeItem().AlignCenter().Column(col =>
                                                {
                                                    col.Item().Text($"น้ำหนักรวม     {displayModel.Sum(s => s.OkWg)}     กรัม").AlignCenter().FontSize(11);
                                                });
                                            }
                                            else
                                            {
                                                row.RelativeItem().AlignCenter().Column(col =>
                                                {
                                                    col.Item().Text($"").AlignCenter().FontSize(11);
                                                });
                                                row.RelativeItem().AlignCenter().Column(col =>
                                                {
                                                    col.Item().Text($"รวม     {displayModel.Where(w => w.Unit.Trim() == "PC").Sum(s => s.OkTtl)}     PC").AlignCenter().FontSize(11);
                                                });
                                                row.RelativeItem().AlignCenter().Column(col =>
                                                {
                                                    col.Item().Text($"รวม     {displayModel.Where(w => w.Unit.Trim() == "PR").Sum(s => s.OkTtl)}     PR").AlignCenter().FontSize(11);
                                                });
                                                row.RelativeItem().AlignCenter().Column(col =>
                                                {
                                                    col.Item().Text($"").AlignCenter().FontSize(11);
                                                });
                                            }
                                        });
                                    });
                            });

                            // Footer: ลายเซ็น
                            table.Cell().ColumnSpan((uint)columnCount).Padding(2).Column(row =>
                            {
                                row.Item()
                                    .PaddingTop(60)
                                    .Element(container =>
                                    {
                                        container.Row(row =>
                                        {
                                            row.RelativeItem().AlignCenter().Column(col =>
                                            {
                                                col.Item().Text("(...............................)").AlignCenter().FontSize(11);
                                                col.Item().Text($"{tempack.Reporter}").FontSize(11).AlignCenter();
                                                col.Item().Text("ผู้ส่งสินค้า").FontSize(11).AlignCenter();
                                            });
                                            row.RelativeItem().AlignCenter().Column(col =>
                                            {
                                                col.Item().Text("(...............................)").AlignCenter().FontSize(11);
                                                col.Item().Text("ผู้รับสินค้า").FontSize(11).AlignCenter();
                                            });
                                            row.RelativeItem().AlignCenter().Column(col =>
                                            {
                                                col.Item().Text("(...............................)").AlignCenter().FontSize(11);
                                                col.Item().Text("QA Packing").FontSize(11).AlignCenter();
                                            });
                                        });
                                    });
                            });
                        });
                    });

                    col.Item().PageBreak();
                }
            });

            // ฟังก์ชันตกแต่งเซลล์
            static IContainer CellStyle(IContainer container)
            {
                return container
                    .Border(0.5f)
                    .BorderColor(Colors.Black)
                    .Padding(3);
            }
        }
        private static int CalculateMaxRows()
        {
            // คำนวณประมาณจากขนาดฟอนต์และ padding
            // Header ประมาณ 50 point, Footer ประมาณ 80 point
            // แต่ละแถวประมาณ 20 point (ฟอนต์ 8 + padding)
            var availableHeight = (PageSizes.A4.Height - (0.5f * 2 * 72 / 2.54f) - 20) / 2; // ครึ่งหน้า
            var usedHeight = 130; // header + footer + margins
            var rowHeight = 20;

            return Math.Max(1, (int)((availableHeight - usedHeight) / rowHeight));
        }

        // Helper: คำนวณจำนวนคอลัมน์จริง
        private static int GetColumnCount(string sendType)
        {
            int count = 1; // "ลำดับ"
            if (sendType is "KS" or "KM") count += 1; // เลขที่งานช่าง/ครั้งที่ส่ง
            if (sendType is "KS" or "KM" or "KX") count += 1; // รหัสสินค้า
            if (sendType is "KX") count += 1; // FN
            if (sendType is "KS" or "KM") count += 1; // ชื่อช่าง
            if (sendType is "KS" or "KM" or "KX") count += 1; // จำนวน
            if (sendType is "KS" or "KM") count += 1; // น้ำหนักรวม
            if (sendType is "KM") count += 1; // สาเหตุ

            return count;
        }
    }
}
