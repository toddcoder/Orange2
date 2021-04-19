using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Core.Collections;
using Core.RegularExpressions;
using Orange.Library;
using Orange.Library.Parsers;
using static System.Drawing.Color;
using static System.Drawing.FontStyle;
using static Orange.Library.Parsers.IDEColor;
using static Orange.Library.Parsers.IDEColor.EntityType;

namespace OrangePlayground
{
   public class ColorParser : IColorizer
   {
      public static void StopTextBoxUpdate(RichTextBox textBox) => SendMessage(textBox.Handle, WM_SET_REDRAW, false, 0);

      public static void ResumeTextBoxUpdate(RichTextBox textBox) => SendMessage(textBox.Handle, WM_SET_REDRAW, true, 0);

      protected const int WM_SET_REDRAW = 11;

      [DllImport("user32.dll")]
      public static extern int SendMessage(IntPtr hWnd, int msg, bool wParam, int lParam);

      protected Hash<string, IDEColor> colors;
      protected RichTextBox textBox;

      public ColorParser(RichTextBox textBox)
      {
         colors = Parser.IDEColors;
         this.textBox = textBox;
      }

      public void Colorize()
      {
         var position = textBox.SelectionStart;
         var length = textBox.SelectionLength;
         var normalFont = textBox.Font;
         using var boldFont = new Font(textBox.Font, Bold);
         using var italicFont = new Font(textBox.Font, Italic);
         using var underlineFont = new Font("Monoid", textBox.Font.Size, Underline);
         using var specialFont = new Font("Monoid", textBox.Font.Size, Regular);
         StopTextBoxUpdate(textBox);

         foreach (var color in colors.Select(item => item.Value))
         {
            textBox.Select(color.Position, color.Length);
            textBox.SelectionColor = getForeColor(color);
            textBox.SelectionBackColor = getBackColor(color);
            Font selectionFont;
            if (isItalic(color.Type))
            {
               selectionFont = italicFont;
            }
            else if (isBold(color.Type))
            {
               selectionFont = boldFont;
            }
            else if (isUnderline(color.Type))
            {
               selectionFont = underlineFont;
            }
            else if (isSpecial(color.Type))
            {
               selectionFont = specialFont;
            }
            else
            {
               selectionFont = normalFont;
            }

            textBox.SelectionFont = selectionFont;
         }

         //markText("/t", GhostWhite);
         markText("/s+ (/r /n | /r | /n)", PaleVioletRed);

         textBox.SelectionStart = position;
         textBox.SelectionLength = length;

         ResumeTextBoxUpdate(textBox);
         textBox.Refresh();
      }

      protected void markText(string pattern, Color backColor)
      {
         if (textBox.Text.Matcher(pattern).If(out var matcher))
         {
            for (var i = 0; i < matcher.MatchCount; i++)
            {
               var (_, index, length) = matcher.GetMatch(i);
               textBox.Select(index, length);
               textBox.SelectionBackColor = backColor;
            }
         }
      }

      protected static bool isBold(EntityType type) => type switch
      {
         Structures => true,
         Messaging => true,
         KeyWords => true,
         _ => false
      };

      protected static bool isItalic(EntityType type) => type switch
      {
         Variables => true,
         _ => false
      };

      protected static bool isUnderline(EntityType type) => type == Types;

      protected static bool isSpecial(EntityType type) => type is Strings or Interpolated;

      protected static Color getBackColor(IDEColor color) => color.Type switch
      {
         _ => White
      };

      protected static Color getForeColor(IDEColor color) => color.Type switch
      {
         Strings => FromArgb(38, 205, 0),
         Numbers => DeepSkyBlue,
         Operators => Red,
         Variables => Blue,
         Structures => Black,
         Whitespaces => Black,
         Comments => FromArgb(128, 128, 128),
         Messaging => Teal,
         Formats => Violet,
         Dates => DarkOliveGreen,
         Arrays => LightSalmon,
         Alternators => CadetBlue,
         Symbols => CornflowerBlue,
         Booleans => Coral,
         KeyWords => BlueViolet,
         Invokeables => CadetBlue,
         Interpolated => DodgerBlue,
         Types => CadetBlue,
         _ => Black
      };
   }
}