using JPStockPacking.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Cell().RowSpan(6).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));
                                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(2).Element(CellStyle).AlignCenter().Text($"{item.TtQtyToPack} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(8).Element(CellStyle).AlignLeft().Text($"{item.TdesArt}/{item.TdesFn}").FontSize(6).SemiBold();

                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignRight().Text($"Barcode").FontSize(7);
                                table.Cell().ColumnSpan(2).Element(CellStyle).AlignLeft().Text($"{item.Barcode}").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"Stสงของ").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"Stตัวอย่าง").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"St/JP").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"St/ลูกค้า").FontSize(7);

                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignRight().Text($"Finishing").FontSize(7);
                                table.Cell().ColumnSpan(2).Element(CellStyle).AlignLeft().Text($"{item.EdesFn}").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"St Article").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");

                                table.Cell().ColumnSpan(6).Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"St Barcode").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");

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
                                });

                                table.Cell().RowSpan(6).Element(CellStyle).AlignCenter().AlignMiddle().Height(60).Width(70).Element(container => container.RenderItemImage(null, item.ImagePath));
                                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.ListNo}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(2).Element(CellStyle).AlignCenter().Text($"{item.TtQtyToPack} {item.Tunit}").FontSize(8).SemiBold();
                                table.Cell().ColumnSpan(8).Element(CellStyle).AlignLeft().Text($"{item.TdesArt}/{item.TdesFn}").FontSize(6).SemiBold();

                                table.Cell().Element(CellStyle).AlignRight().Text($"ขนาด").FontSize(7);

                                for (int i = 0; i < 10; i += 1)
                                {
                                    if (item.Size.Count >= i + 1)
                                    {
                                        var a = item.Size[i];
                                        table.Cell().Element(CellStyle).AlignCenter().Text($"{a.S}").FontSize(7);
                                    }
                                    else
                                    {
                                        table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                    }
                                }

                                table.Cell().Element(CellStyle).AlignRight().Text($"จำนวน").FontSize(7);

                                for (int i = 0; i < 10; i += 1)
                                {
                                    if (item.Size.Count >= i + 1)
                                    {
                                        var a = item.Size[i];
                                        table.Cell().Element(CellStyle).AlignCenter().Text($"{a.TtQtyToPack}").FontSize(7);
                                    }
                                    else
                                    {
                                        table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                    }
                                }

                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignRight().Text($"Barcode").FontSize(7);
                                table.Cell().ColumnSpan(2).Element(CellStyle).AlignLeft().Text($"{item.Barcode}").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"Stสงของ").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"Stตัวอย่าง").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"St/JP").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"St/ลูกค้า").FontSize(7);

                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignRight().Text($"Finishing").FontSize(7);
                                table.Cell().ColumnSpan(2).Element(CellStyle).AlignLeft().Text($"{item.EdesFn}").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"St Article").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");

                                table.Cell().ColumnSpan(6).Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"St Barcode").FontSize(7);
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");
                                table.Cell().Element(CellStyle).AlignCenter().Text($"");

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
    }
}
