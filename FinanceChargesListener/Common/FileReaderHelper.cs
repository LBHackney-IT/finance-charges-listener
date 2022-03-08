using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener.Common
{
    public static class FileReaderHelper
    {
        public static decimal GetChargeAmount(object excelColumnValue)
        {
            decimal result;
            if (excelColumnValue == null || excelColumnValue.ToString() == " ")
                result = 0;
            else
                result = Convert.ToDecimal(excelColumnValue);
            return result;
        }
    }
}
