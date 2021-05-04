using System;
using System.Collections.Generic;
using System.IO;
using Orange.Library.Values;
using Array = Orange.Library.Values.Array;
using System.Linq;
using Core.Collections;
using Core.Enumerables;
using Core.Numbers;
using Core.Strings;

namespace Orange.Library
{
   public class Table
   {
      public class MetaData
      {
         public MetaData(PadType padType)
         {
            MaxLength = 0;
            PadType = padType;
         }

         public int MaxLength { get; set; }

         public PadType PadType { get; set; }

         public void Evaluate(string value)
         {
            var length = value.Length;
            if (length > MaxLength)
            {
               MaxLength = length;
            }
         }
      }

      public class Row
      {
         protected List<string> columns;
         protected int rowIndex;
         protected Hash<string, PadType> overriddenPadTypes;

         public Row(int rowIndex, Hash<string, PadType> overriddenPadTypes)
         {
            columns = new List<string>();
            this.rowIndex = rowIndex;
            this.overriddenPadTypes = overriddenPadTypes;
         }

         protected string getKey() => getKey(rowIndex, columns.Count);

         protected static string getKey(int rowIndex, int columnIndex) => $"{rowIndex} {columnIndex}";

         public void AddColumn(string text) => columns.Add(text);

         public void AddColumn(string text, PadType padType)
         {
            overriddenPadTypes[getKey()] = padType;
            columns.Add(text);
         }

         public string this[int columnIndex]
         {
            get => columns[columnIndex];
            set => columns[columnIndex] = value;
         }

         public bool IndexExists(int columnIndex) => columnIndex > -1 && columnIndex < columns.Count;

         public string Render(string leftDivider, string middleDivider, string rightDivider, List<MetaData> metaData)
         {
            var fields = new List<string>();
            for (var i = 0; i < metaData.Count; i++)
            {
               var data = metaData[i];
               var value = IndexExists(i) ? columns[i] : "";
               if (value == null)
               {
                  continue;
               }

               var padType = overriddenPadTypes.Find(getKey(rowIndex, i), _ => data.PadType);
               var paddedItem = value.Pad(padType, data.MaxLength);
               fields.Add(paddedItem);
            }

            var renderedRow = leftDivider + fields.ToString(middleDivider) + rightDivider;
            return renderedRow;
         }

         public string RenderAsHeader(string leftDivider, string middleDivider, string rightDivider, List<MetaData> metaData)
         {
            var headers = new List<string>();
            for (var i = 0; i < metaData.Count; i++)
            {
               var data = metaData[i];
               var value = IndexExists(i) ? columns[i] : "";
               if (value == null)
               {
                  continue;
               }

               var paddedItem = value.Pad(PadType.Center, data.MaxLength);
               headers.Add(paddedItem);
            }

            var renderedHeader = leftDivider + headers.ToString(middleDivider) + rightDivider;
            return renderedHeader;
         }

         public string Representation => "| " + columns.ToString(" | ") + " |...";
      }

      protected const string LOCATION = "Table";

      protected static PadType getPadType(Bits32<Value.OptionType> option)
      {
         if (option[Value.OptionType.RJust])
         {
            return PadType.Left;
         }
         else if (option[Value.OptionType.Center])
         {
            return PadType.Center;
         }
         else
         {
            return PadType.Right;
         }
      }

      protected static (string, PadType, bool) extractText(Value value)
      {
         var options = value.Options;
         return (value.Text, getPadType(options), options[Value.OptionType.None]);
      }

      protected static string spread(string text, int length) => text.Repeat(length).Keep(length);

      protected List<MetaData> metaData;
      protected Row headerRow;
      protected List<Row> rows;
      protected Hash<string, PadType> overriddenPadding;

      public Table(Array array, bool lines = false)
      {
         metaData = new List<MetaData>();
         rows = new List<Row>();
         overriddenPadding = new Hash<string, PadType>();
         headerRow = new Row(-1, overriddenPadding);
         foreach (var item in array)
         {
            var (text, padType, _) = extractText(item.Value);
            headerRow.AddColumn(text);
            var data = new MetaData(padType);
            data.Evaluate(text);
            metaData.Add(data);
         }

         HeaderDivider = lines ? "-" : "";
         Horizontal = "";
         Vertical = lines ? "|" : "";
      }

      public string HeaderDivider { get; set; }

      public string Horizontal { get; set; }

      public string Vertical { get; set; }

      public int Length
      {
         get
         {
            var sum = metaData.Sum(d => d.MaxLength);
            sum += Vertical.IsNotEmpty() ? 3 * (metaData.Count - 1) + 4 : metaData.Count - 1;

            return sum;
         }
      }

      public void AddRow(Array array)
      {
         var length = Math.Min(metaData.Count, array.Length);
         var row = new Row(rows.Count, overriddenPadding);
         rows.Add(row);
         for (var i = 0; i < length; i++)
         {
            var (text, padType, useDefault) = extractText(array[i]);
            if (useDefault)
            {
               row.AddColumn(text);
            }
            else
            {
               row.AddColumn(text, padType);
            }

            var data = metaData[i];
            data.Evaluate(text);
         }
      }

      public override string ToString()
      {
         var length = Length;
         var headerDivider = getHeaderDivider(length);
         var horizontal = getHorizontal(length);
         var leftDivider = getLeftDivider();
         var middleDivider = getMiddleDivider();
         var rightDivider = getRightDivider();
         var hasHorizontal = horizontal != null;
         var hasHeaderDivider = headerDivider != null;
         using var writer = new StringWriter();
         if (hasHorizontal)
         {
            writer.WriteLine(horizontal);
         }

         var header = headerRow.RenderAsHeader(leftDivider, middleDivider, rightDivider, metaData);
         writer.WriteLine(header);
         if (hasHeaderDivider)
         {
            writer.WriteLine(headerDivider);
         }

         foreach (var renderedRow in rows.Select(row => row.Render(leftDivider, middleDivider, rightDivider, metaData)))
         {
            writer.WriteLine(renderedRow);
            if (hasHorizontal)
            {
               writer.WriteLine(horizontal);
            }
         }

         return writer.ToString();
      }

      protected string getRightDivider() => Vertical.IsNotEmpty() ? " " + Vertical.Keep(1) : "";

      protected string getMiddleDivider() => Vertical.IsNotEmpty() ? " " + Vertical.Keep(1) + " " : " ";

      protected string getLeftDivider() => Vertical.IsNotEmpty() ? Vertical.Keep(1) + " " : "";

      protected string getHorizontal(int length) => Horizontal.IsNotEmpty() ? spread(Horizontal, length) : null;

      protected string getHeaderDivider(int length) => HeaderDivider.IsNotEmpty() ? spread(HeaderDivider, length) : null;

      public string Representation => headerRow.Representation;
   }
}