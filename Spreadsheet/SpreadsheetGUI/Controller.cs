﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Formulas;
using SS;

namespace SpreadsheetGUI
{
    public class Controller
    {
        //Window being controlled
        private IAnalysisView window;
        private AbstractSpreadsheet spreadsheet;
        private int row;
        private int col;
        private String CellName = "A1";
        private String[] cellLett = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
        "N","O","P","Q","R","S","T","U","V","W","X","Y","Z"};


        /// <summary>
        /// Control of the window.
        /// </summary>
        /// <param name="window"></param>
        public Controller(IAnalysisView window)
        {
            this.window = window;
            this.spreadsheet = new Spreadsheet();
            window.Title = "";
            window.NewFileChosen += HandleNewFileChosen;
            window.SaveFileChosen += HandleSaveFileChosen;
            window.CloseEvent += HandleCloseEvent;
            window.ContentsChanged += HandleContentsChanged;
            window.SelectionChanged += HandleSelectionChanged;
            window.RowChanged += HandleRowChanged;
            window.ColChanged += HandleColChanged;
            
    }

        public Controller(IAnalysisView window, string fileName)
        {
            this.window = window;
            StreamReader reader = new StreamReader(fileName);
            this.spreadsheet = new Spreadsheet(reader, new Regex(@"^([a-zA-Z]+)([1-9])(\d+)?$"));
            reader.Close();
            window.Title = "";
            window.NewFileChosen += HandleNewFileChosen;
            window.SaveFileChosen += HandleSaveFileChosen;
            window.CloseEvent += HandleCloseEvent;
            window.ContentsChanged += HandleContentsChanged;
            window.SelectionChanged += HandleSelectionChanged;
            window.RowChanged += HandleRowChanged;
            window.ColChanged += HandleColChanged;

            int itterator = 0;
            foreach(string cell in spreadsheet.GetNamesOfAllNonemptyCells())
            {
                string firstLet = cell.Substring(0, 1);
                string rest = cell.Substring(1, cell.Length - 1);
                int row = Convert.ToInt32(rest) - 1;

                int col = GetColumn(firstLet);

                window.UpdatedValue(col, row, spreadsheet.GetCellValue(cell));

                //if we are on the first one we need to update the current boxes!
                if (itterator == 0)
                {
                    window.CellNameText(cell);
                    window.ContentsBox(spreadsheet.GetCellContents(cell));
                    window.ValueBox(spreadsheet.GetCellValue(cell));
                    //update the current contents box and value box
                    itterator++;
                }
            }
            HandleSaveFileChosen(fileName);
        }

        private void HandleSaveFileChosen(string fileName)
        {
            try
            {
                window.Title = fileName;
                StreamWriter writer = new StreamWriter(fileName);
                spreadsheet.Save(writer);
                writer.Close();
            }
            catch (IOException e)
            {
                window.CouldNotSaveFileMessage();
            }
            catch (XmlException)
            {
                window.CouldNotSaveFileMessage();
            }
        }

        /// <summary>
        ///  hangle for :getting the new row for getting the cell name.
        /// </summary>
        /// <param name="newRow"></param>
        private void HandleRowChanged(int newRow)
        {
            row = newRow;
        }

        /// <summary>
        /// handle for: getting the new col for getting the cell name. then we get the cell name because we
        /// have updated both the row and col at this point, and send that back to the window.
        /// </summary>
        /// <param name="newCol"></param>
        private void HandleColChanged(int newCol)
        {
            col = newCol;
            CellName = GetCellName();

            window.CellNameText(CellName);
        }

        /// <summary>
        /// Handles a request for the selection change. it gets the value and contents from spreadsheet and 
        /// sends it back to the window.
        /// </summary>
        private void HandleSelectionChanged()
        {
            object value = spreadsheet.GetCellValue(CellName);
            object contents = spreadsheet.GetCellContents(CellName);

            window.ValueBox(value);
            window.ContentsBox(contents);
        }

        private void HandleSave()
        {
            
        }

        /// <summary>
        /// handles the contents changed. we change it in the spreadsheet, and then we change all of the values 
        /// of the cells that are returned from the spreadsheet. and pass that back to the window.
        /// </summary>
        /// <param name="newContents"></param>
        private void HandleContentsChanged(string newContents)
        {
            try
            {
                //cells not changed yet.
                ISet<string> needToChangeCells = spreadsheet.SetContentsOfCell(CellName, newContents);

                int itterator = 0;
                foreach (string cell in needToChangeCells)
                {
                    string firstLet = cell.Substring(0, 1);
                    string rest = cell.Substring(1, cell.Length - 1);
                    int row = Convert.ToInt32(rest) - 1;

                    int col = GetColumn(firstLet);

                    window.UpdatedValue(col, row, spreadsheet.GetCellValue(cell));

                    //if we are on the first one we need to update the current boxes!
                    if (itterator == 0)
                    {
                        window.ValueBox(spreadsheet.GetCellValue(cell));
                        //update the current contents box and value box
                        itterator++;
                    }

                    //get value and contents pass back to the window to reset.
                }

                window.ContentsBox(newContents);
            }
            //If there is a circular equation. show message box.
            catch (CircularException)
            {
                window.CircularExceptionWarinig();
            }
            catch (FormulaFormatException)
            {
                window.FormulaExceptionWarning();
            }
        }


        /// <summary>
        /// handles a close request.
        /// </summary>
        /// <param name="e"></param>
        private void HandleCloseEvent(FormClosingEventArgs e)
        {
            //If the spreadsheet is changed, send to display save warning window.
            if (spreadsheet.Changed)
            {
                window.QuitWarning(e);
            }
            //Otherwise close the spreadsheet.
            else
            {
                window.DoClose();
            }
        }


        /// <summary>
        /// handles a new File chosen and runs a new one.
        /// </summary>
        /// <param name="fileName"></param>
        private void HandleNewFileChosen(string fileName)
        {
            try
            {
                SpreadsheetApplicationContext.GetContext().RunNew(fileName);
            }
            catch(IOException)
            {
                window.CouldNotLoadFileMessage();
            }
            catch (XmlException)
            {
                window.CouldNotLoadFileMessage();
            }
        }

        /*public bool isChanged()
        {
            return spreadsheet.Changed;
        }*/
        /// <summary>
        /// gets the cell name of the current row and column.
        /// </summary>
        /// <returns></returns>
        private String GetCellName()
        {
            //have column number need the letter a =0
            int tempRow = row + 1;
            return cellLett[col] + tempRow;
        }

        /// <summary>
        /// get the column with input being the letter of the cell column.
        /// </summary>
        /// <param name="let"></param>
        /// <returns></returns>
        private int GetColumn(string let)
        {
            //spreadsheet starts at A1 (row = -1)
            int itterator = 0;
            foreach (string letter in cellLett)
            {
                if(letter.Equals(let))
                {
                    break;
                }
                itterator++;
            }
            return itterator;
        }
    }
}
