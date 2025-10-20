using JPStockPacking.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using static JPStockPacking.Services.Implement.AuthService;
using static JPStockPacking.Services.Implement.OrderManagementService;

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

                                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดสั่ง {item.TtQty} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดแจ้ง {item.TtQtyToPack} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(7).Element(CellStyle).AlignLeft().Text($"{item.TdesArt}/{item.TdesFn}").FontSize(6).SemiBold();

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

                                table.Cell().RowSpan(8).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));

                                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดสั่ง {item.TtQty} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดแจ้ง {item.TtQtyToPack} {item.Tunit}").FontSize(8).SemiBold();

                                table.Cell().ColumnSpan(13).Element(CellStyle).AlignLeft().Text($"{item.TdesArt}/{item.TdesFn}").FontSize(6).SemiBold();

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
                                        table.Cell().Element(CellStyle).AlignCenter().Text($"{a.TtQtyToPack}").FontSize(7).Underline();
                                    }
                                    else
                                    {
                                        table.Cell().Element(CellStyle).AlignCenter().Text($"");
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

                                table.Cell().RowSpan(3).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));

                                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดสั่ง {item.TtQty} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดแจ้ง {item.TtQtyToPack} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(7).Element(CellStyle).AlignLeft().Text($"{item.TdesArt.Trim()}/{item.TdesFn.Trim()}").FontSize(7).SemiBold();
                                table.Cell().RowSpan(3).ColumnSpan(7).Element(CellStyle).AlignCenter().Text($"{item.EnSendQtyPrice.ToUpper()}").FontSize(8).SemiBold();

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

                                table.Cell().RowSpan(8).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));

                                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดสั่ง {item.TtQty} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"ยอดแจ้ง {item.TtQtyToPack} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().RowSpan(2).ColumnSpan(6).Element(CellStyle).AlignCenter().Text($"{item.EnSendQtyPrice.ToUpper()}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(7).Element(CellStyle).AlignLeft().Text($"{item.TdesArt.Trim()}/{item.TdesFn.Trim()}").FontSize(7).SemiBold();

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
                                        table.Cell().Element(CellStyle).AlignCenter().Text($"{a.TtQtyToPack}").FontSize(7).Underline();
                                    }
                                    else
                                    {
                                        table.Cell().Element(CellStyle).AlignCenter().Text($"");
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
    }
}
