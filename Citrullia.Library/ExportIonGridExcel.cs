using System;
using System.Collections.Generic;
using System.Text;

namespace Citrullia.Library
{
    /// <summary>
    /// Utility class for exporting ion grids to Excel
    /// </summary>
    // TODO: Fix problem with DataGridView and Office-dependencies
    public static class ExportIonGridExcel
    {
        /// <summary>
        /// Export ion data grid for paired spectra.
        /// </summary>
        /// <param name="dgvCit">The citrullination ion data grid.</param>
        /// <param name="dgvArg">The arginine ion data grid.</param>
        /// <param name="idCit">The ID of the citrullinated spectrum.</param>
        /// <param name="idArg">The ID of the arginine spectrum.</param>
        public static void ExportDataGrid(DataGridView dgvCit, DataGridView dgvArg, int idCit, int idArg)
        {
            // Creating new Excel app
            _Application app = new Microsoft.Office.Interop.Excel.Application();
            // Creating new ExcelWorkbook within app
            _Workbook workbook = app.Workbooks.Add(Type.Missing);
            // Add another worksheet
            workbook.Sheets.Add(Type.Missing, Type.Missing, 1, Type.Missing);
            // Make excell visible to the user
            app.Visible = true;


            // Get the reference to the first sheet and renaming it
            workbook.Sheets[1].Name = string.Format("Cit_IonGrid_{0}", idCit);
            // Get the reference to the first sheet and renaming it
            _Worksheet worksheet2 = workbook.Sheets[2];
            worksheet2.Name = string.Format("Arg_IonGrid_{0}", idArg);

            CreateWorksheet(dgvCit, workbook.Sheets[1]);
            CreateWorksheet(dgvArg, worksheet2);
        }

        /// <summary>
        /// Export the ion data grid for lone citrullinated spectra.
        /// </summary>
        /// <param name="dgv">The data grid view.</param>
        /// <param name="id">The ID of the lone citrullinated spectra.</param>
        public static void ExportDataGridLonely(DataGridView dgv, int id)
        {
            // Creating new Excel app
            _Application app = new Microsoft.Office.Interop.Excel.Application();
            // Creating new ExcelWorkbook within app
            _Workbook workbook = app.Workbooks.Add(Type.Missing);
            // Creating new ExcelSheet in workbook
            // Make excell visible to the user
            app.Visible = true;


            // Get the reference to the first sheet and renaming it
            workbook.Sheets[1].Name = string.Format("IonGrid{0}", id);

            CreateWorksheet(dgv, workbook.Sheets[1]);
        }

        /// <summary>
        /// Create worksheet.
        /// </summary>
        /// <param name="dgv">The datagridview to be written.</param>
        /// <param name="workSheet">The worksheet to be written to.</param>
        private static void CreateWorksheet(DataGridView dgv, _Worksheet workSheet)
        {
            //Input all the data with correct colors to worksheet
            for (int i = 1; i < dgv.Columns.Count + 1; i++)
            {
                workSheet.Cells[1, i] = dgv.Columns[i - 1].HeaderText;
                workSheet.Cells.Cells[1, i].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Orange);
            }
            for (int row = 0; row <= dgv.Rows.Count - 1; row++)
            {
                for (int column = 0; column <= dgv.Columns.Count - 1; column++)
                {
                    if (dgv.Rows[row].Cells[column].Value != null)
                    {

                        // If the value is a digit, parse it
                        if (char.IsDigit(dgv.Rows[row].Cells[column].Value.ToString()[0]))
                        {
                            double mzValue = double.Parse(dgv.Rows[row].Cells[column].Value.ToString());

                            workSheet.Cells[row + 2, column + 1] = mzValue.ToString(FileReader.NumberFormat);
                            workSheet.Cells[row + 2, column + 1].NumberFormat = "##.000";
                        }
                        else
                        {
                            workSheet.Cells[row + 2, column + 1] = dgv.Rows[row].Cells[column].Value.ToString();
                        }

                        workSheet.Cells[row + 2, column + 1].Font.Color = dgv.Rows[row].Cells[column].Style.ForeColor;
                    }
                    else
                    {
                        workSheet.Cells[row + 2, column + 1] = "";
                    }
                }
            }
        }
    }
}
