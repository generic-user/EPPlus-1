/*************************************************************************************************
  Required Notice: Copyright (C) EPPlus Software AB. 
  This software is licensed under PolyForm Noncommercial License 1.0.0 
  and may only be used for noncommercial purposes 
  https://polyformproject.org/licenses/noncommercial/1.0.0/

  A commercial license to use this software can be purchased at https://epplussoftware.com
 *************************************************************************************************
  Date               Author                       Change
 *************************************************************************************************
  01/27/2020         EPPlus Software AB       Initial release EPPlus 5
 *************************************************************************************************/
using System;
using System.Globalization;
using System.Xml;
using OfficeOpenXml.Utils;

namespace OfficeOpenXml.Table
{

    /// <summary>
    /// A table column
    /// </summary>
    public class ExcelTableColumn : XmlHelper
    {
        internal ExcelTable _tbl;
        internal ExcelTableColumn(XmlNamespaceManager ns, XmlNode topNode, ExcelTable tbl, int pos) :
            base(ns, topNode)
        {
            _tbl = tbl;
            Position = pos;
        }
        /// <summary>
        /// The column id
        /// </summary>
        public int Id 
        {
            get
            {
                return GetXmlNodeInt("@id");
            }
            set
            {
                SetXmlNodeString("@id", value.ToString());
            }
        }
        /// <summary>
        /// The position of the column
        /// </summary>
        public int Position
        {
            get;
            internal set;
        }
        /// <summary>
        /// The name of the column
        /// </summary>
        public string Name
        {
            get
            {
                var n=GetXmlNodeString("@name");
                if (string.IsNullOrEmpty(n))
                {
                    if (_tbl.ShowHeader)
                    {
                        n = ConvertUtil.ExcelDecodeString(_tbl.WorkSheet.GetValue<string>(_tbl.Address._fromRow, _tbl.Address._fromCol + this.Position));
                    }
                    else
                    {
                        n = "Column" + (this.Position+1).ToString();
                    }
                }
                return n;
            }
            set
            {
                var v = ConvertUtil.ExcelEncodeString(value);
                SetXmlNodeString("@name", v);
                if (_tbl.ShowHeader)
                {
                    _tbl.WorkSheet.SetValue(_tbl.Address._fromRow, _tbl.Address._fromCol + this.Position, value);
                }
                _tbl.WorkSheet.SetTableTotalFunction(_tbl, this);
            }
        }
        /// <summary>
        /// A string text in the total row
        /// </summary>
        public string TotalsRowLabel
        {
            get
            {
                return GetXmlNodeString("@totalsRowLabel");
            }
            set
            {
                SetXmlNodeString("@totalsRowLabel", value);
            }
        }
        /// <summary>
        /// Build-in total row functions.
        /// To set a custom Total row formula use the TotalsRowFormula property
        /// <seealso cref="TotalsRowFormula"/>
        /// </summary>
        public RowFunctions TotalsRowFunction
        {
            get
            {
                if (GetXmlNodeString("@totalsRowFunction") == "")
                {
                    return RowFunctions.None;
                }
                else
                {
                    return (RowFunctions)Enum.Parse(typeof(RowFunctions), GetXmlNodeString("@totalsRowFunction"), true);
                }
            }
            set
            {
                if (value == RowFunctions.Custom)
                {
                    throw(new Exception("Use the TotalsRowFormula-property to set a custom table formula"));
                }
                string s = value.ToString();
                s = s.Substring(0, 1).ToLower(CultureInfo.InvariantCulture) + s.Substring(1, s.Length - 1);
                SetXmlNodeString("@totalsRowFunction", s);
                _tbl.WorkSheet.SetTableTotalFunction(_tbl, this);
            }
        }
        const string TOTALSROWFORMULA_PATH = "d:totalsRowFormula";
        /// <summary>
        /// Sets a custom Totals row Formula.
        /// Be carefull with this property since it is not validated. 
        /// <example>
        /// tbl.Columns[9].TotalsRowFormula = string.Format("SUM([{0}])",tbl.Columns[9].Name);
        /// </example>
        /// </summary>
        public string TotalsRowFormula
        {
            get
            {
                return GetXmlNodeString(TOTALSROWFORMULA_PATH);
            }
            set
            {
                if (value.StartsWith("=")) value = value.Substring(1, value.Length - 1);
                SetXmlNodeString("@totalsRowFunction", "custom");                
                SetXmlNodeString(TOTALSROWFORMULA_PATH, value);
                _tbl.WorkSheet.SetTableTotalFunction(_tbl, this);
            }
        }
        const string DATACELLSTYLE_PATH = "@dataCellStyle";
        /// <summary>
        /// The named style for datacells in the column
        /// </summary>
        public string DataCellStyleName
        {
            get
            {
                return GetXmlNodeString(DATACELLSTYLE_PATH);
            }
            set
            {
                if(_tbl.WorkSheet.Workbook.Styles.NamedStyles.FindIndexByID(value)<0)
                {
                    throw(new Exception(string.Format("Named style {0} does not exist.",value)));
                }
                SetXmlNodeString(TopNode, DATACELLSTYLE_PATH, value,true);
               
                int fromRow=_tbl.Address._fromRow + (_tbl.ShowHeader?1:0),
                    toRow=_tbl.Address._toRow - (_tbl.ShowTotal?1:0),
                    col=_tbl.Address._fromCol+Position;

                if (fromRow <= toRow)
                {
                    _tbl.WorkSheet.Cells[fromRow, col, toRow, col].StyleName = value;
                }
            }
        }
  		const string CALCULATEDCOLUMNFORMULA_PATH = "d:calculatedColumnFormula";
 		/// <summary>
 		/// Sets a calculated column Formula.
 		/// Be carefull with this property since it is not validated. 
 		/// <example>
 		/// tbl.Columns[9].CalculatedColumnFormula = string.Format("SUM(MyDataTable[[#This Row],[{0}]])",tbl.Columns[9].Name);
 		/// </example>
 		/// </summary>
 		public string CalculatedColumnFormula
 		{
 			get
 			{
 				return GetXmlNodeString(CALCULATEDCOLUMNFORMULA_PATH);
 			}
 			set
 			{
 				if (value.StartsWith("=")) value = value.Substring(1, value.Length - 1);
 				SetXmlNodeString(CALCULATEDCOLUMNFORMULA_PATH, value);
 			}
 		}

    }
}
