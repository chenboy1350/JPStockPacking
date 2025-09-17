using JPStockPacking.Models;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
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

            var document = Document.Create(container =>
            {
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
                            table.Cell().Element(CellStyle).AlignRight().Text($"วันที่ออกเอกสาร : {DateTime.Now.ToString("dd-MM-yyyy", new CultureInfo("th-TH"))}").FontSize(7);
                            table.Cell().ColumnSpan(2).Element(CellStyle).AlignLeft().Text($"Packing Instruction : {model.Special}").FontSize(7);

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.Padding(3);
                            }
                        });

                    page.Content()
                        .PaddingVertical(2)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                            });

                            List<SendToPackLots> Items = [.. model.Lots];

                            table.Cell().Padding(2).Column(row =>
                            {
                                foreach (var item in Items)
                                {
                                    if (item.Size == null || item.Size!.Count <= 0)
                                    {
                                        row.Item().PaddingBottom(2).ShowEntire().Element(e => e.CreateLotItemCard(item));
                                    }
                                    else
                                    {
                                        row.Item().PaddingBottom(2).ShowEntire().Element(e => e.CreateLotSizeItemCard(item));
                                    }
                                }

                                row.Item()
                                    .PaddingTop(30)
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

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("หน้า ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return document.GeneratePdf();

        }
    }
}
