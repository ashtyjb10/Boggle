﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Controller is allowed to use these to modify the view.
    /// </summary>
    public interface IAnalysisView
    {
        event Action<string> NewFileChosen;

        event Action<string> SaveFileChosen;

        event Action<string> GetCellInfo;

        event Action<string> ContentsChanged;

        event Action<string> SelectionChanged;

        event Action<int> ColChanged;

        event Action<int> RowChanged;

        event Action CloseEvent;

        void CellNameText(string CellName);

        void ContentsBox(string contents);

        bool isChanged { get; }

        string Title { set; get; }

        string Content { set; }

        string Value {   set; }

        string Cell { set; }

        void DoClose();
    }
}
