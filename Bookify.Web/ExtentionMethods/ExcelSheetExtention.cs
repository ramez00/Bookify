using ClosedXML.Excel;

namespace Bookify.Web.ExtentionMethods
{
    public static class ExcelSheetExtention
    {
        public static void AddHeader(this IXLWorksheet sheet, string[] headerCells)
        {
            for (int i = 0; i < headerCells.Length; i++)
                sheet.Cell(1, i + 1).SetValue(headerCells[i]);

            var header = sheet.Range(1, 1, 1, headerCells.Length);

            header.Style.Fill.BackgroundColor = XLColor.Black;
            header.Style.Font.FontColor = XLColor.White;
            header.Style.Font.SetBold();
        }

        public static void AddFormate(this IXLWorksheet sheet)
        {
            sheet.ColumnsUsed().AdjustToContents();
            sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            sheet.CellsUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.CellsUsed().Style.Border.OutsideBorderColor = XLColor.CoolBlack;
        }
    }
}
