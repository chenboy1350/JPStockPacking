using JPStockPacking.Models;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Services.Implement
{
    public class ReportService(IWebHostEnvironment webHostEnvironment) : IReportService
    {
        private readonly IWebHostEnvironment _env = webHostEnvironment;

        public byte[] GenerateSendQtyToPackReport(SendToPackModel model, PrintTo printTo)
        {
            var imgPath = Path.Combine(_env.WebRootPath, "img", "logo.png");
            string ReportTitle = printTo.ToString();

            QuestPDF.Settings.License = LicenseType.Community;

            const int rowsPerColumn = 10;
            const int rowsPerPage = rowsPerColumn * 2;

            List<SendToPackLots> Items = [.. model.Lots.Where(x => x.Size.Count == 0)];
            List<SendToPackLots> ItemSizes = [.. model.Lots.Where(x => x.Size.Count > 0)];

            int totalPages = (int)Math.Ceiling(Items.Count / (double)rowsPerPage);

            var document = Document.Create(container =>
            {
                for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
                {
                    var pageItems = Items
                        .Skip(pageIndex * rowsPerPage)
                        .Take(rowsPerPage)
                        .ToList();

                    List<SendToPackLots> leftItems;
                    List<SendToPackLots> rightItems;

                    if (pageItems.Count < rowsPerPage)
                    {
                        int half = (int)Math.Ceiling(pageItems.Count / 2.0);
                        leftItems = [.. pageItems.Take(half)];
                        rightItems = [.. pageItems.Skip(half)];
                    }
                    else
                    {
                        leftItems = [.. pageItems.Take(rowsPerColumn)];
                        rightItems = [.. pageItems.Skip(rowsPerColumn).Take(rowsPerColumn)];
                    }

                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Portrait());
                        page.Margin(0.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Tahoma"));

                        page.Header()
                            .PaddingBottom(5)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.RelativeColumn(30);
                                    columns.RelativeColumn(10);
                                });

                                table.Cell().RowSpan(3).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderHeaderImage(null, imgPath));
                                table.Cell().Element(CellStyle).AlignCenter().Text($"รายการแจ้งยอด {ReportTitle} : {model.CustCode}/{model.OrderNo}  คุณภาพ {model.Grade}").FontSize(11).SemiBold();
                                table.Cell().Element(CellStyle).AlignRight().Text($"วันที่ออกเอกสาร : {DateTime.Now:dd-MM-yyyy HH:mm}").FontSize(7);
                                table.Cell().ColumnSpan(2).Element(CellStyle).AlignLeft().Text($"Packing Instruction : {model.Special}").FontSize(7);

                                static IContainer CellStyle(IContainer container) => container.Padding(3);
                            });

                        page.Content()
                            .PaddingVertical(2)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Cell().Padding(2).Column(col =>
                                {
                                    foreach (var item in leftItems)
                                    {
                                        col.Item().PaddingBottom(2).ShowEntire().Element(e =>
                                        {
                                            if (printTo == PrintTo.Export)
                                                e.CreateLotItemWithPriceCard(item);
                                            else
                                                e.CreateLotItemCard(item);
                                        });
                                    }

                                    int fillCount = rowsPerColumn - leftItems.Count;
                                    for (int i = 0; i < fillCount; i++)
                                        col.Item().Height(2);
                                });

                                table.Cell().Padding(2).Column(col =>
                                {
                                    foreach (var item in rightItems)
                                    {
                                        col.Item().PaddingBottom(2).ShowEntire().Element(e =>
                                        {
                                            if (printTo == PrintTo.Export)
                                                e.CreateLotItemWithPriceCard(item);
                                            else
                                                e.CreateLotItemCard(item);
                                        });
                                    }

                                    int fillCount = rowsPerColumn - rightItems.Count;
                                    for (int i = 0; i < fillCount; i++)
                                        col.Item().Height(2);
                                });

                                if (pageIndex == totalPages - 1)
                                {
                                    table.Cell().ColumnSpan(2).Padding(2).Column(col =>
                                    {
                                        foreach (var item in ItemSizes)
                                        {
                                            col.Item().PaddingBottom(2).ShowEntire().Element(e =>
                                            {
                                                if (printTo == PrintTo.Export)
                                                    e.CreateLotSizeItemWithPriceCard(item);
                                                else
                                                    e.CreateLotSizeItemCard(item);
                                            });
                                        }

                                        if (printTo == PrintTo.Export)
                                        {
                                            col.Item().AlignRight().Padding(2).Text($"รวมยอดแจ้งทั้งหมด : {model.SumPCSendTtQty} PC / {model.SumPRSendTtQty} PR / {model.SumSendTtPrice}").FontSize(10).Bold();
                                        }

                                        col.Item().PaddingTop(20).Element(container =>
                                        {
                                            container.Row(row =>
                                            {
                                                row.RelativeItem().AlignCenter().Column(c =>
                                                {
                                                    c.Item().Text("(...............................)").AlignCenter();
                                                    c.Item().Text($"{model.Sender}").AlignCenter();
                                                    c.Item().Text("ผู้ส่ง").FontSize(8).AlignCenter();
                                                });
                                                row.RelativeItem().AlignCenter().Column(c =>
                                                {
                                                    c.Item().Text("(...............................)").AlignCenter();
                                                    c.Item().Text($"{model.Approver}").AlignCenter();
                                                    c.Item().Text("ผู้อนุมัติ").FontSize(8).AlignCenter();
                                                });
                                                row.RelativeItem().AlignCenter().Column(c =>
                                                {
                                                    c.Item().Text("(...............................)").AlignCenter();
                                                    c.Item().Text("...............................").AlignCenter();
                                                    c.Item().Text("ผู้รับ").FontSize(8).AlignCenter();
                                                });
                                            });
                                        });
                                    });
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("หน้า ");
                                x.CurrentPageNumber();
                                x.Span($" / {totalPages}");
                            });
                    });
                }
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateBreakReport(List<LostAndRepairModel> model)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Portrait());
                    page.Margin(0.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Tahoma"));

                    page.Content().Column(col =>
                    {
                        col.Item().Element(content =>
                        {
                            content.BreakReportContent(model);
                        });
                    });

                    //page.Content().Column(col =>
                    //{
                    //    // คำนวณความสูงที่เหลือหลังจากหัก margin และ footer
                    //    var availableHeight = PageSizes.A4.Height - (0.5f * 2 * 72 / 2.54f) - 60; // หัก margin และพื้นที่สำหรับ footer
                    //    var halfHeight = availableHeight / 2;

                    //    // ครึ่งบน - กำหนดความสูงแน่นอน
                    //    col.Item().Height(halfHeight).Element(content =>
                    //    {
                    //        content.BreakReportContent(model);
                    //    });

                    //    // เส้นคั่นกลาง
                    //    col.Item().Height(2).Background(Colors.Grey.Lighten2);

                    //    // ครึ่งล่าง - กำหนดความสูงแน่นอนและไม่ให้เกิน
                    //    col.Item().Height(halfHeight).Element(content =>
                    //    {
                    //        content.Column(innerCol =>
                    //        {
                    //            // ใช้ MaxHeight เพื่อไม่ให้เกินขอบเขต
                    //            innerCol.Item().MaxHeight(halfHeight).Element(c =>
                    //                c.BreakReportContent(model));
                    //        });
                    //    });
                    //});
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateLostReport(List<LostAndRepairModel> model, UserModel userModel)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Portrait());
                    page.Margin(0.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Tahoma"));

                    page.Content().Column(col =>
                    {
                        col.Item().Element(content =>
                        {
                            content.LostReportContent(model, userModel);
                        });
                    });

                    //page.Content().Column(col =>
                    //{
                    //    // คำนวณความสูงที่เหลือหลังจากหัก margin และ footer
                    //    var availableHeight = PageSizes.A4.Height - (0.5f * 2 * 72 / 2.54f) - 60; // หัก margin และพื้นที่สำหรับ footer
                    //    var halfHeight = availableHeight / 2;

                    //    // ครึ่งบน - กำหนดความสูงแน่นอน
                    //    col.Item().Height(halfHeight).Element(content =>
                    //    {
                    //        content.LostReportContent(model, userModel);
                    //    });

                    //    // เส้นคั่นกลาง
                    //    col.Item().Height(2).Background(Colors.Grey.Lighten2);

                    //    // ครึ่งล่าง - กำหนดความสูงแน่นอนและไม่ให้เกิน
                    //    col.Item().Height(halfHeight).Element(content =>
                    //    {
                    //        content.Column(innerCol =>
                    //        {
                    //            // ใช้ MaxHeight เพื่อไม่ให้เกินขอบเขต
                    //            innerCol.Item().MaxHeight(halfHeight).Element(c =>
                    //                c.LostReportContent(model, userModel));
                    //        });
                    //    });
                    //});
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateSenToReport(List<TempPack> model)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                _ = container.Page(page =>
                {
                    page.Size(PageSizes.A4.Portrait());
                    page.Margin(0.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Tahoma"));

                    var result = model
                        .GroupBy(tp => tp.Doc)
                        .Select(g => new TempPackPage
                        {
                            Doc = g.Key,
                            ListDate = g.Max(x => x.MdateSend),
                            Title = g.First().SendType switch
                            {
                                "KS" => "ใบส่งสินค้าเก็บ stock",
                                "KM" => "ใบส่งสินค้าหลอม 925",
                                "KX" => "ใบส่งสินค้าส่งออก",
                                _ => "ไม่ทราบประเภท"
                            },
                            SendTo = g.First().SendType switch
                            {
                                "KS" => "Stock Body",
                                "KM" => "Spare Part Stock",
                                "KX" => "Export",
                                _ => "Unknow"
                            },
                            SendType = g.First().SendType switch
                            {
                                "KS" => "KS",
                                "KM" => "KM",
                                "KX" => "KX",
                                _ => "N/A"
                            },
                            OrderNoAndCusCode = $"{g.First().CustCode.Trim()}/{g.First().OrderNo}",
                            Reporter = g.First().Username,
                            TempPacks = [.. g.OrderBy(x => x.LotNo)]
                        })
                        .OrderByDescending(x => x.ListDate)
                        .ToList();

                    page.Content().Column(col =>
                    {
                        col.Item().Element(content =>
                        {
                            content.SendToReportContent(result);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

    }
}