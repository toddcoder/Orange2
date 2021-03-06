﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Core.Arrays;
using Core.Collections;
using Core.Computers;
using Core.Dates;
using Core.Enumerables;
using Core.Monads;
using Core.Numbers;
using Core.RegularExpressions;
using Core.Strings;
using Core.WinForms.Consoles;
using Core.WinForms.Documents;
using Orange.Library;
using Orange.Library.Values;
using static System.Math;
using static System.Windows.Forms.Application;
using static System.Windows.Forms.Clipboard;
using static Core.Arrays.ArrayFunctions;
using static Core.Monads.MonadFunctions;
using static Newtonsoft.Json.JsonConvert;
using static Orange.Library.Runtime;
using static Core.WinForms.Documents.Document;

namespace OrangePlayground
{
   public partial class Playground : Form
   {
      protected const string MENU_FILE = "File";
      protected const string MENU_EDIT = "Edit";
      protected const string MENU_BUILD = "Build";
      protected const string MENU_TEXT = "Text";
      protected const string MENU_INSERT = "Insert";

      protected Document document;
      protected TextBoxConsole outputConsole;
      protected TextBoxConsole errorConsole;
      protected TextWriter errorWriter;
      protected TextWriter consoleWriter;
      protected TextReader textReader;
      protected bool locked;
      protected bool manual;
      protected Stopwatch stopwatch;
      protected InteractiveFileCache fileCache;
      protected Settings settings;
      protected Hash<int, string> results;
      protected int[] tabStops;
      protected Menus menus;

      public Playground() => InitializeComponent();

      public IMaybe<FileName> PassedFileName { get; set; } = none<FileName>();

      protected static IMaybe<FileName> getSettingsFile()
      {
         FolderName folder = @"C:\Enterprise\Configurations\Orange";
         var file = folder + "orange.json";
         if (!file.Exists())
         {
            folder = @"C:\Configurations";
            file = folder + "orange.json";
         }

         return maybe(file.Exists(), () => file);
      }

      protected static FolderName getSettingsFolder()
      {
         FolderName folder = @"C:\Enterprise\Configurations\Orange";
         if (folder.Exists())
         {
            return folder;
         }

         folder = @"C:\Configurations";

         return folder.Exists() ? folder : FolderName.Current;
      }

      protected static Settings getSettings() => getSettingsFile().Map(f => DeserializeObject<Settings>(f.Text)).DefaultTo(() => new Settings());

      protected void Playground_Load(object sender, EventArgs e)
      {
         results = new AutoHash<int, string>(_ => "");
         settings = getSettings();

         errorConsole = new TextBoxConsole(this, textErrors, settings.ErrorFont, settings.ErrorFontSize, TextBoxConsole.ConsoleColorType.Cathode);
         errorWriter = errorConsole.Writer();
         try
         {
            try
            {
               tabStops = array(32, 64, 96, 128);
               textEditor.SelectionTabs = tabStops;
            }
            catch (Exception exception)
            {
               displayException(exception);
            }

            outputConsole = new TextBoxConsole(this, textConsole, settings.ConsoleFont, settings.ConsoleFontSize,
               TextBoxConsole.ConsoleColorType.Quick);
            consoleWriter = outputConsole.Writer();
            textReader = outputConsole.Reader();

            document = new Document(this, textEditor, ".orange", "Orange", settings.EditorFont, settings.EditorFontSize);
            document.StandardMenus();
            menus = document.Menus;
            menus.Menu(MENU_FILE, "Set Current Folder", (_, _) => setCurrentFolder());

            menus.Menu(MENU_EDIT, "Duplicate", (_, _) => duplicate(), "^D");
            menus.Menu(MENU_EDIT, "Indent", (_, _) => indent(), "^I");
            menus.Menu(MENU_EDIT, "Unindent", (_, _) => unindent(), "^%I");

            menus.Menu($"&{MENU_BUILD}");
            menus.Menu(MENU_BUILD, "Run", (_, _) => run(), "F5");
            menus.Menu(MENU_BUILD, "Manual", (s, _) =>
            {
               manual = !manual;
               ((ToolStripMenuItem)s).Checked = manual;
            }, "^F5");

            menus.Menu($"&{MENU_TEXT}");
            menus.Menu(MENU_TEXT, "Clipboard -> Text", (_, _) =>
            {
               if (ClipboardText().If(out var clipboardText))
               {
                  var windowsText = GetWindowsText(clipboardText);
                  if (textText.SelectionLength == 0)
                  {
                     textText.Text = windowsText;
                  }
                  else
                  {
                     textText.SelectedText = windowsText;
                  }

                  tabs.SelectedTab = tabText;
               }
            }, "^|V");
            menus.Menu(MENU_TEXT, "Clipboard -> Append to Text", (_, _) =>
            {
               if (ClipboardText().If(out var clipboardText))
               {
                  var windowsText = GetWindowsText(clipboardText);
                  textText.AppendText(windowsText);
                  tabs.SelectedTab = tabText;
               }
            });
            menus.Menu(MENU_TEXT, "Clipboard -> Text & Run", (_, _) =>
            {
               if (ClipboardText().If(out var clipboardText))
               {
                  textText.Text = GetWindowsText(clipboardText);
               }

               DoEvents();
               run();
               var selectedText = textConsole.SelectionLength == 0 ? textConsole.Text : textConsole.SelectedText;
               selectedText = GetWindowsText(selectedText);
               SetText(selectedText);
            }, "F6");
            menus.Menu(MENU_TEXT, "Text -> Clipboard", (_, _) =>
            {
               var text = textConsole.SelectionLength == 0 ? textConsole.Text : textConsole.SelectedText;
               text = GetWindowsText(text);
               SetText(text);
            }, "^|C");
            menus.Menu(MENU_TEXT, "Save text to file", (_, _) =>
            {
               var text = textConsole.SelectionLength == 0 ? textConsole.Text : textConsole.SelectedText;
               text = GetWindowsText(text);
               if (dialogSave.ShowDialog() == DialogResult.OK)
               {
                  FileName file = dialogSave.FileName;
                  try
                  {
                     file.Text = text;
                  }
                  catch (Exception exception)
                  {
                     displayException(exception);
                  }
               }
            });

            menus.Menu($"&{MENU_INSERT}");
            menus.Menu(MENU_INSERT, "println", (_, _) => insertText("println ", 0, 0), "^P");
            menus.Menu(MENU_INSERT, "println interpolated", (_, _) => insertText("println $\"\"", -1, 0), "^%P");
            menus.Menu(MENU_INSERT, "print", (_, _) => insertText("print ", 0, 0));
            menus.Menu(MENU_INSERT, "manifln", (_, _) => insertText("manifln ", 0, 0), "^M");
            menus.Menu(MENU_INSERT, "manif", (_, _) => insertText("manif", 0, 0));
            menus.Menu(MENU_INSERT, "put", (_, _) => insertText("put ", 0, 0));
            menus.Menu(MENU_INSERT, "peek", (_, _) => surround("peek(", ")"), "^K");
            menus.MenuSeparator(MENU_INSERT);
            menus.Menu(MENU_INSERT, "if template", (_, _) => insertTemplate("if true", "true"), "^|I");
            menus.Menu(MENU_INSERT, "while template", (_, _) => insertTemplate("while true", "true"), "^|W");
            menus.Menu(MENU_INSERT, "for template", (_, _) => insertTemplate("for i in ^0", "^0"), "^|F");
            menus.Menu(MENU_INSERT, "func template", (_, _) => insertTemplate("func proc()", "proc"), "^|P");
            menus.Menu(MENU_INSERT, "func template", (_, _) => insertTemplate("func proc()", "proc"), "^|P");

            menus.CreateMainMenu(this);

            menus.StandardContextEdit(document);

            textEditor.ReassignHandle();

            locked = false;
            manual = false;
            stopwatch = new Stopwatch();
            fileCache = new InteractiveFileCache();

            textEditor.Paint += (_, evt) => paintResults(evt);

            setManual(settings.Manual);
            if (settings.DefaultFolder != null)
            {
               FolderName.Current = settings.DefaultFolder;
            }

            textText.Text = settings.Text.FromBase64(Encoding.UTF8) ?? "";
            if (PassedFileName.If(out var passedFileName))
            {
               document.Open(passedFileName);
            }
            else if (settings.LastFile != null && ((FileName)settings.LastFile).Exists())
            {
               document.Open(settings.LastFile);
            }
         }
         catch (Exception exception)
         {
            displayException(exception);
         }
      }

      protected void setManual(bool value)
      {
         manual = value;
         ((ToolStripMenuItem)menus[MENU_BUILD].DropDownItems[1]).Checked = manual;
      }

      protected void paintResults(PaintEventArgs e)
      {
         try
         {
            var firstVisibleChar = textEditor.GetCharIndexFromPosition(new Point(0, 0));
            var firstVisibleLine = textEditor.GetLineFromCharIndex(firstVisibleChar);
            var resultLines = Enumerable.Range(0, textEditor.Lines.Length).Select(_ => "").ToArray();
            var tops = Enumerable.Range(0, textEditor.Lines.Length).Select(_ => 0f).ToArray();
            foreach (var (key, value) in results)
            {
               var lineIndex = textEditor.GetLineFromCharIndex(key);
               resultLines[lineIndex] = lineIndex >= firstVisibleLine ? value.VisibleWhitespace(false).Truncate(128) : "";
               tops[lineIndex] = textEditor.GetPositionFromCharIndex(key).Y;
            }

            var font = textEditor.Font;

            for (var i = firstVisibleLine; i < textEditor.Lines.Length; i++)
            {
               var result = resultLines[i];
               var line = textEditor.Lines[i];
               var lineSize = e.Graphics.MeasureString(line, font);
               var top = tops[i];
               if (result.IsNotEmpty())
               {
                  var resultSize = e.Graphics.MeasureString(result, font);
                  var remainingWidth = textEditor.ClientRectangle.Width - lineSize.Width;
                  resultSize.Width = Min(remainingWidth, resultSize.Width);
                  var location = new PointF(e.Graphics.ClipBounds.Width - resultSize.Width, top);
                  var fillRectangle = new RectangleF(location.X, location.Y + 1, resultSize.Width, resultSize.Height - 2);
                  e.Graphics.FillRectangle(Brushes.LightGreen, fillRectangle);
                  using (var pen = new Pen(SystemColors.ButtonShadow, 1))
                  {
                     e.Graphics.DrawRectangle(pen, getRectangle(fillRectangle));
                  }

                  var format = new StringFormat(StringFormatFlags.LineLimit);
                  e.Graphics.DrawString(result, font, Brushes.Black, location, format);
               }

               var tabSize = getSize(e.Graphics.MeasureString("\t", font));
               if (line.Matcher("^ /(/t1%4)").If(out var matcher))
               {
                  var x = 8;
                  var charIndex = textEditor.GetFirstCharIndexFromLine(i);
                  var y = textEditor.GetPositionFromCharIndex(charIndex).Y;
                  var height = y + tabSize.Height / 2;
                  var tabStopIndex = matcher.FirstGroup.Length - 1;
                  var width = tabStops[tabStopIndex];
                  using (var pen = new Pen(Color.LightGray, 3))
                  {
                     pen.CustomEndCap = new AdjustableArrowCap(3, 3, true);
                     e.Graphics.DrawLine(pen, x, height, x + width, height);
                  }

                  using (var pen = new Pen(Color.LightGray, 1))
                  {
                     for (var j = 0; j < tabStopIndex; j++)
                     {
                        var tabStop = tabStops[j];
                        var left = x + tabStop;
                        e.Graphics.DrawLine(pen, left, height - 8, left, height + 8);
                     }
                  }
               }
            }
         }
         catch
         {
         }
      }

      protected static Rectangle getRectangle(RectangleF rect) => new((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);

      protected static Size getSize(SizeF size) => new((int)size.Width, (int)size.Height);

      protected void duplicate()
      {
         var dupLine = "";

         if (textEditor.SelectionLength == 0)
         {
            var currentLineIndex = textEditor.CurrentLineIndex();
            if (textEditor.AtEnd() && textEditor.Text.IsMatch("/n $"))
            {
               currentLineIndex--;
            }

            var lines = textEditor.Lines;
            if (currentLineIndex.Between(0).Until(lines.Length))
            {
               dupLine = lines[currentLineIndex].Copy();
            }
         }
         else
         {
            dupLine = textEditor.SelectedText;
         }

         textEditor.AppendAtEnd(dupLine, "/n");
      }

      protected static void setCurrentFolder()
      {
         using var dialog = new FolderBrowserDialog { SelectedPath = FolderName.Current.ToString() };
         if (dialog.ShowDialog() == DialogResult.OK)
         {
            FolderName.Current = dialog.SelectedPath;
         }
      }

      protected void run() => update(true, true);

      protected void update(bool execute, bool fromMenu)
      {
         if (locked || textEditor.TextLength == 0)
         {
            return;
         }

         locked = true;

         if (manual)
         {
            labelResult.Text = "running...";
            DoEvents();
         }
         else if (fromMenu)
         {
            document.Save();
         }

         try
         {
            textConsole.Clear();
            textErrors.Clear();

            results.Clear();

            var orange = new Orange.Library.Orange(textEditor.Text, new ColorParser(textEditor), fileCache,
                  new PlaygroundConsole(consoleWriter, textReader))
               { Text = textText.Text, ModuleFolders = array(@"C:\Enterprise\Modules", @"C:\Enterprise\Orange") };
            stopwatch.Reset();
            stopwatch.Start();
            if (execute)
            {
               orange.Execute();
            }
            else
            {
               orange.ColorizeOnly();
            }

            stopwatch.Stop();
            orange.Colorize();
            document.Clean();

            var lastValue = orange.LastValue;
            var lastType = orange.LastType;
            var resultText = lastValue.IsNotEmpty() ? $"{lastValue} | {lastType}" : "";
            results = Block.Results;
            var elapsed = stopwatch.Elapsed.ToLongString(true);
            labelResult.Text = resultText;
            labalElapsed.Text = elapsed;
            tabs.SelectedTab = tabConsole;
            textEditor.Select();
            textEditor.Refresh();
         }
         catch (Exception exception)
         {
            displayException(exception);
         }
         finally
         {
            locked = false;
         }
      }

      protected void displayException(Exception exception)
      {
         errorWriter.WriteLine(exception.Message);
         consoleWriter.WriteLine("Dumping print buffer:");
         consoleWriter.WriteLine(State?.PrintBuffer ?? "");
         tabs.SelectedTab = tabErrors;
         textEditor.Select();
      }

      protected void textEditor_TextChanged(object sender, EventArgs e)
      {
         if (document == null)
         {
            return;
         }

         update(!manual, false);
         document.Dirty();
      }

      protected void insertText(string text, int selectionOffset, int length = -1)
      {
         textEditor.SelectedText = text;
         textEditor.SelectionStart += selectionOffset;
         if (length > -1)
         {
            textEditor.SelectionLength = length;
         }
      }

      protected void surround(string before, string after)
      {
         var selected = textEditor.SelectedText;
         var replacement = $"{before}{selected}{after}";
         textEditor.SelectedText = replacement;
      }

      protected void insertDelimiterText(string delimiter, int selectionOffset, int length, int halfLength = -1)
      {
         if (textEditor.SelectionLength == 0)
         {
            insertText(delimiter, selectionOffset, length);
         }
         else
         {
            var selection = textEditor.SelectedText;
            if (halfLength == -1)
            {
               halfLength = delimiter.Length / 2;
            }

            textEditor.SelectedText = delimiter.Keep(halfLength) + selection + delimiter.Drop(halfLength);
         }
      }

      protected string textAtInsert(int take, int skip = 0) => textEditor.Text.Drop(textEditor.SelectionStart + skip).Keep(take);

      protected void setTextAtInsert(int take, int skip = 0, string text = "")
      {
         textEditor.Select(textEditor.SelectionStart + skip, take);
         textEditor.SelectedText = text;
         textEditor.Select(textEditor.SelectionStart + skip, 0);
      }

      protected void moveSelectionRelative(int amount = 1) => textEditor.SelectionStart += amount;

      protected void textEditor_KeyPress(object sender, KeyPressEventArgs e)
      {
         switch (e.KeyChar)
         {
            case '(':
               insertDelimiterText("()", -1, 0);
               e.Handled = true;
               break;
            case ')':
               if (textAtInsert(1) == ")")
               {
                  moveSelectionRelative();
                  e.Handled = true;
               }

               break;
            case '{':
               insertDelimiterText("{}", -1, 0);
               e.Handled = true;
               break;
            case '}':
               if (textAtInsert(1) == "}")
               {
                  moveSelectionRelative();
                  e.Handled = true;
               }

               break;
            case '[':
               insertDelimiterText("[]", -1, 0);
               e.Handled = true;
               break;
            case ']':
               if (textAtInsert(1) == "]")
               {
                  moveSelectionRelative();
                  e.Handled = true;
               }

               break;
            case '\'':
               if (textAtInsert(1) == "'")
               {
                  moveSelectionRelative();
                  e.Handled = true;
                  break;
               }

               insertDelimiterText("''", -1, 0);
               e.Handled = true;
               break;
            case '"':
               if (textAtInsert(1) == "\"")
               {
                  moveSelectionRelative();
                  e.Handled = true;
                  break;
               }

               insertDelimiterText("\"\"", -1, 0);
               e.Handled = true;
               break;
            case ',':
               if (textAtInsert(1) == ",")
               {
                  moveSelectionRelative();
                  e.Handled = true;
               }

               break;
            case ';':
               if (textAtInsert(1) == ";")
               {
                  if (textAtInsert(1, -1) == "\n")
                  {
                     insertPass();
                  }
                  else
                  {
                     moveSelectionRelative();
                  }

                  e.Handled = true;
               }

               break;
         }
      }

      protected void Playground_FormClosing(object sender, FormClosingEventArgs e)
      {
         if (document.FileName.If(out var fileName))
         {
            try
            {
               settings.LastFile = fileName;
               settings.DefaultFolder = FolderName.Current.FullPath;
               settings.Text = textText.Text.ToBase64(Encoding.UTF8);
               settings.Manual = manual;
               var file = getSettingsFolder() + "orange.json";
               file.Text = SerializeObject(settings);
            }
            catch (Exception exception)
            {
               MessageBox.Show(exception.Message);
            }
         }
      }

      protected void textEditor_KeyUp(object sender, KeyEventArgs e)
      {
         switch (e.KeyCode)
         {
            case Keys.Escape:
               if (textAtInsert(1) == "'" && textAtInsert(1, -1) == "'")
               {
                  e.Handled = true;
                  setTextAtInsert(1);
               }
               else if (textAtInsert(1) == "\"" && textAtInsert(1, -1) == "\"")
               {
                  e.Handled = true;
                  setTextAtInsert(1);
               }
               else if (textAtInsert(1) == ")" && textAtInsert(1, -1) == "(")
               {
                  e.Handled = true;
                  setTextAtInsert(1);
               }
               else if (textAtInsert(1) == "]" && textAtInsert(1, -1) == "[")
               {
                  e.Handled = true;
                  setTextAtInsert(1);
               }
               else if (textAtInsert(1) == "}" && textAtInsert(1, -1) == "{")
               {
                  e.Handled = true;
                  setTextAtInsert(1);
               }

               break;
            case Keys.F12:
               insertPass();
               e.Handled = true;
               break;
            case Keys.Right:
               if (e.Alt)
               {
                  matchPass(true);
                  e.Handled = true;
               }

               break;
            case Keys.Left:
               if (e.Alt)
               {
                  matchPass(false);
                  e.Handled = true;
               }

               break;
            case Keys.F1:
               if (getWord().If(out var word) && findWord(word).If(out var found))
               {
                  insertText(found.Drop(word.Length), 0);
               }

               e.Handled = true;
               break;
         }
      }

      protected IMaybe<string> getWord()
      {
         var startIndex = textEditor.SelectionStart;
         var begin = textEditor.Text.Keep(startIndex);
         return begin.Matcher("/(/w+) $").Map(m => m.FirstGroup);
      }

      protected IMaybe<string> findWord(string begin)
      {
         var matches = textEditor.Text.Split("-/w+").Distinct().Where(word => word.StartsWith(begin));
         return matches.FirstOrNone();
      }

      protected void matchPass(bool first)
      {
         if (textEditor.Text.Matcher("/b 'pass' /b").If(out var matcher))
         {
            var (_, index, length) = matcher.GetMatch(first ? 0 : matcher.MatchCount - 1);
            textEditor.Select(index, length);
         }
      }

      protected string getIndent() => textEditor.CurrentLine().Matcher("^ /t*").Map(m => m[0]).DefaultTo(() => "");

      protected void insertTemplate(string template, string replacement)
      {
         var indent = getIndent();
         var expandedTemplate = $"{indent}{template}\n{indent}\tpass";
         var index = expandedTemplate.IndexOf(replacement, StringComparison.Ordinal);
         if (index == -1)
         {
            insertText(expandedTemplate, 0, 0);
         }
         else
         {
            index = -(expandedTemplate.Length - index);
            insertText(expandedTemplate, index, replacement.Length);
         }
      }

      protected void insertPass()
      {
         var indent = getIndent();
         var text = $"\n{indent}\tpass";
         insertText(text, -text.Length, 0);
      }

      protected IMaybe<(string[], int)> linesFromSelection()
      {
         var startIndex = textEditor.GetLineFromCharIndex(textEditor.SelectionStart);
         var stopIndex = textEditor.GetLineFromCharIndex(textEditor.SelectionStart + textEditor.SelectionLength);
         var lines = textEditor.Lines;
         if (startIndex.Between(0).Until(lines.Length) && stopIndex.Between(0).Until(lines.Length))
         {
            return (lines.RangeOf(startIndex, stopIndex), startIndex).Some();
         }

         return none<(string[], int)>();
      }

      protected void indent()
      {
         try
         {
            var lines = textEditor.Lines;
            if (linesFromSelection().If(out var selectedLines, out var offset))
            {
               var saved = saveSelection();
               for (var i = 0; i < selectedLines.Length; i++)
               {
                  textEditor.Lines[i + offset] = $"\t{selectedLines[i]}";
               }

               textEditor.Lines = lines;
               restoreSelection(saved);
            }
         }
         catch (Exception exception)
         {
            errorWriter.WriteLine(exception.Message);
         }
      }

      protected void unindent()
      {
         try
         {
            var lines = textEditor.Lines;
            if (linesFromSelection().If(out var selectedLines, out var offset))
            {
               var saved = saveSelection();
               for (var i = 0; i < selectedLines.Length; i++)
               {
                  if (selectedLines[i].Matcher("^ /t /@").If(out var matcher))
                  {
                     lines[i + offset] = matcher.FirstGroup;
                  }
               }

               textEditor.Lines = lines;
               restoreSelection(saved);
            }
         }
         catch (Exception exception)
         {
            errorWriter.WriteLine(exception.Message);
         }
      }

      protected (int, int) saveSelection() => (textEditor.SelectionStart, textEditor.SelectionLength);

      protected void restoreSelection((int start, int length) saved) => textEditor.Select(saved.start, saved.length);

      protected void textEditor_VScroll(object sender, EventArgs e)
      {
         try
         {
         }
         catch
         {
         }
      }

      protected void textResults_SelectionChanged(object sender, EventArgs e)
      {
         try
         {
         }
         catch
         {
         }
      }
   }
}