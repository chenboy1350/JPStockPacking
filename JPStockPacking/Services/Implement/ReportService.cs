using JPStockPacking.Models;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Data.SqlTypes;
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

                                        col.Item().PaddingTop(30).Element(container =>
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
                                "KL" => "รายการสินค้าสูญหาย",
                                _ => "ไม่ทราบประเภท"
                            },
                            SendTo = g.First().SendType switch
                            {
                                "KS" => "Stock Body",
                                "KM" => "Spare Part Stock",
                                "KX" => "Export",
                                "KL" => "Center",
                                _ => "Unknow"
                            },
                            SendType = g.First().SendType switch
                            {
                                "KS" => "KS",
                                "KM" => "KM",
                                "KX" => "KX",
                                "KL" => "KL",
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

        public byte[] GenerateComparedInvoiceReport(ComparedInvoiceFilterModel comparedInvoiceFilterModel, List<ComparedInvoiceModel> model, InvoiceType invoiceType)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            if (invoiceType == InvoiceType.CorrectOnly)
            {
                model = [.. model.Where(w => w.IsMatched)];
            }
            else if (invoiceType == InvoiceType.IncorectOnly)
            {
                model = [.. model.Where(w => !w.IsMatched)];
            }

            DateTime FromDate = comparedInvoiceFilterModel.FromDate ?? DateTime.Now;
            DateTime ToDate = comparedInvoiceFilterModel.ToDate ?? DateTime.Now;

            if (comparedInvoiceFilterModel.OrderNo != null || comparedInvoiceFilterModel.InvoiceNo != null)
            {
                FromDate = SqlDateTime.MinValue.Value;
                ToDate = SqlDateTime.MaxValue.Value;
            }

            if (comparedInvoiceFilterModel.FromDate != null)
            {
                FromDate = comparedInvoiceFilterModel.FromDate.Value;
            }

            if (comparedInvoiceFilterModel.ToDate != null)
            {
                ToDate = comparedInvoiceFilterModel.ToDate.Value;
            }

            const int rowsPerPage = 46;
            int totalPages = (int)Math.Ceiling(model.Count / (double)rowsPerPage);

            var document = Document.Create(container =>
            {
                for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
                {
                    var pageItems = model
                        .Skip(pageIndex * rowsPerPage)
                        .Take(rowsPerPage)
                        .ToList();

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
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text($"รายงานตรวจสอบการส่งออก").FontSize(11).SemiBold();
                                table.Cell().Element(CellStyle).AlignRight().Text($"Print Date: {DateTime.Now:dd/MM/yyyy}").FontSize(7);

                                if (comparedInvoiceFilterModel.FromDate != null && comparedInvoiceFilterModel.ToDate != null) table.Cell().Element(CellStyle).AlignLeft().Text($"Date : {comparedInvoiceFilterModel.FromDate:dd/MM/yyyy} - {comparedInvoiceFilterModel.ToDate:dd/MM/yyyy}").FontSize(8);
                                if (comparedInvoiceFilterModel.FromDate != null && comparedInvoiceFilterModel.ToDate == null) table.Cell().Element(CellStyle).AlignLeft().Text($"Date : {comparedInvoiceFilterModel.FromDate:dd/MM/yyyy}").FontSize(8);
                                if (comparedInvoiceFilterModel.FromDate == null && comparedInvoiceFilterModel.ToDate != null) table.Cell().Element(CellStyle).AlignLeft().Text($"Date : {comparedInvoiceFilterModel.ToDate:dd/MM/yyyy}").FontSize(8);


                                if (!string.IsNullOrEmpty(comparedInvoiceFilterModel.InvoiceNo))
                                    table.Cell().Element(CellStyle).AlignLeft().Text($"InvoiceNo : {comparedInvoiceFilterModel.InvoiceNo.ToUpper()}").FontSize(8);
                                if (!string.IsNullOrEmpty(comparedInvoiceFilterModel.OrderNo))
                                    table.Cell().Element(CellStyle).AlignLeft().Text($"OrderNo : {comparedInvoiceFilterModel.OrderNo}").FontSize(8);
                                table.Cell().Element(CellStyle).AlignLeft().Text($"Invoice Type : {invoiceType}").FontSize(8);


                                static IContainer CellStyle(IContainer container) => container.Padding(3);
                            });

                        page.Content()
                            .PaddingVertical(2)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Cell().RowSpan(2).Element(CellStyle).AlignCenter().AlignMiddle().Text("No").FontSize(8).SemiBold();
                                table.Cell().RowSpan(2).Element(CellStyle).AlignCenter().AlignMiddle().Text("InvoiceNo").FontSize(8).SemiBold();
                                table.Cell().RowSpan(2).Element(CellStyle).AlignCenter().AlignMiddle().Text("OrderNo").FontSize(8).SemiBold();
                                table.Cell().RowSpan(2).Element(CellStyle).AlignCenter().AlignMiddle().Text("Article").FontSize(8).SemiBold();

                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text("EXPORT").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(3).Element(CellStyle).AlignCenter().Text("PACKING").FontSize(8).SemiBold();
                                table.Cell().RowSpan(2).Element(CellStyle).AlignCenter().AlignMiddle().Text("RESULT").FontSize(8).SemiBold();

                                table.Cell().Element(CellStyle).AlignCenter().Text("QTY").FontSize(8).SemiBold();
                                table.Cell().Element(CellStyle).AlignCenter().Text("PRICE").FontSize(8).SemiBold();
                                table.Cell().Element(CellStyle).AlignCenter().Text("TOTAL").FontSize(8).SemiBold();

                                table.Cell().Element(CellStyle).AlignCenter().Text("QTY").FontSize(8).SemiBold();
                                table.Cell().Element(CellStyle).AlignCenter().Text("PRICE").FontSize(8).SemiBold();
                                table.Cell().Element(CellStyle).AlignCenter().Text("TOTAL").FontSize(8).SemiBold();

                                int i = pageIndex * rowsPerPage + 1;

                                foreach (var item in pageItems)
                                {
                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{i}").FontSize(8);
                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{item.JPInvoiceNo}").FontSize(8);
                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{item.CustCode}/{item.JPOrderNo}").FontSize(8);
                                    table.Cell().Element(CellStyle).AlignLeft().Text($"{item.JPArticle}").FontSize(8);

                                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.JPTtQty}").FontSize(8);
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.JPPrice}").FontSize(8);
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.JPTotalPrice}").FontSize(8);

                                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.SPTtQty}").FontSize(8);
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.SPPrice}").FontSize(8);
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.SPTotalPrice}").FontSize(8);

                                    table.Cell().Element(CellStyle).AlignCenter().Text($"{(!item.IsMatched ? "X" : "")}").FontSize(8);

                                    i++;
                                }

                                if (pageIndex == totalPages - 1)
                                {
                                    table.Cell().ColumnSpan(7).Padding(2).Column(col =>
                                    {
                                        col.Item().AlignRight().Padding(2).Text($"EXPORT TOTAL :     {model.Where(w => w.MakeUnit == "PC").Sum(s => s.JPTtQty):N0}    PC").FontSize(10).Bold();
                                        col.Item().AlignRight().Padding(2).Text($"{model.Where(w => w.MakeUnit == "PR").Sum(s => s.JPTtQty):N0}    PR").FontSize(10).Bold();
                                        col.Item().AlignRight().Padding(2).Text($"{model.Where(w => w.MakeUnit == "SET").Sum(s => s.JPTtQty):N0}  SET").FontSize(10).Bold();
                                        col.Item().AlignRight().Padding(2).Text($"{(decimal)model.Sum(s => s.JPTotalPrice):N0}    US").FontSize(10).Bold();
                                    });
                                    table.Cell().ColumnSpan(4).Padding(2).Column(col =>
                                    {
                                        col.Item().AlignRight().Padding(2).Text($"PACKING TOTAL :     {model.Where(w => w.MakeUnit == "PC").Sum(s => s.SPTtQty):N0}    PC").FontSize(10).Bold();
                                        col.Item().AlignRight().Padding(2).Text($"{model.Where(w => w.MakeUnit == "PR").Sum(s => s.SPTtQty):N0}    PR").FontSize(10).Bold();
                                        col.Item().AlignRight().Padding(2).Text($"{model.Where(w => w.MakeUnit == "SET").Sum(s => s.SPTtQty):N0}  SET").FontSize(10).Bold();
                                        col.Item().AlignRight().Padding(2).Text($"{(decimal)model.Sum(s => s.SPTotalPrice):N0}    US").FontSize(10).Bold();
                                    });
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span($" of {totalPages}");
                            });

                        static IContainer CellStyle(IContainer container) =>
                            container.Border(0.5f).BorderColor(Colors.Black).Padding(3);
                    });
                }
            });

            return document.GeneratePdf();
        }
    }
}