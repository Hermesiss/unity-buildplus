using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Trismegistus.SmartFormat.Core.Extensions;

namespace Trismegistus.SmartFormat.Extensions
{
    [Serializable]
    public class XElementFormatter : FormatterBase
    {
        public XElementFormatter()
        {
            Names = DefaultNames;
        }

        public override string[] DefaultNames => new[] {"xelement", "xml", "x", ""};

        public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;

            XElement currentXElement = null;
            if (format != null && format.HasNested) return false;
            // if we need to format list of XElements then we just take and format first
            var xElmentsAsList = current as IList<XElement>;
            if (xElmentsAsList != null && xElmentsAsList.Count > 0) currentXElement = xElmentsAsList[0];

            var currentAsXElement = currentXElement ?? current as XElement;
            if (currentAsXElement != null)
            {
                formattingInfo.Write(currentAsXElement.Value);
                return true;
            }

            return false;
        }
    }
}
