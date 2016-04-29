using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Orange.Library;
using Standard.Computer;
using Standard.Configurations;
using Standard.ObjectGraphs;
using Standard.Types.Arrays;
using Standard.Types.Collections;
using Standard.Types.Dates;
using Standard.Types.Maybe;
using Standard.Types.Numbers;
using Standard.Types.RegularExpressions;
using Standard.Types.Strings;
using Standard.Types.Tuples;
using Standard.WinForms.Consoles;
using Standard.WinForms.Documents;
using static System.Windows.Forms.Application;
using static System.Windows.Forms.Clipboard;
using static Orange.Library.Runtime;
using static Standard.Types.Arrays.ArrayFunctions;
using static Standard.WinForms.Consoles.TextBoxConsole.ConsoleColorType;
using static Standard.WinForms.Documents.Document;
using static Standard.Types.Tuples.TupleFunctions;

namespace OrangePlayground
{
   public partial class Playground : Form
   {
      const string MENU_FILE = "File";
      const string MENU_EDIT = "Edit";
      const string MENU_BUILD = "Build";
      const string MENU_TEXT = "Text";
      const string MENU_INSERT = "Insert";
      const string PLAYGROUND_FONT_NAME = "Source Code Pro";

      Document document;
      TextBoxConsole outputConsole;
      TextBoxConsole errorConsole;
      TextWriter errorWriter;
      TextWriter consoleWriter;
      TextReader textReader;
      bool locked;
      bool manual;
      Stopwatch stopwatch;
      InteractiveFileCache fileCache;
      Configuration configuration;
      Settings settings;
      Hash<int, string> results;

      public Playground()
      {
         InitializeComponent();
      }

      void Playground_Load(object sender, EventArgs e)
      {
         results = new AutoHash<int, string>("");

         errorConsole = new TextBoxConsole(this, textErrors, PLAYGROUND_FONT_NAME, 12, Cathode);
         errorWriter = errorConsole.Writer();
         try
         {
            try
            {
               textEditor.SelectionTabs = array(32, 64, 96, 128);
               configuration = new Configuration("orange");
               settings = configuration.Root.Object<Settings>().Required();
            }
            catch (Exception exception)
            {
               displayException(exception);
            }

            outputConsole = new TextBoxConsole(this, textConsole, PLAYGROUND_FONT_NAME, 12, Quick);
            consoleWriter = outputConsole.Writer();
            textReader = outputConsole.Reader();

            document = new Document(this, textEditor, ".orange", "Orange", PLAYGROUND_FONT_NAME, 14);
            document.StandardMenus();
            var menus = document.Menus;
            menus.Menu(MENU_FILE, "Set Current Folder", (s, evt) => setCurrentFolder());

            menus.Menu(MENU_EDIT, "Duplicate", (s, evt) => duplicate(), "^D");
            menus.Menu(MENU_EDIT, "Indent", (s, evt) => indent(), "^I");
            menus.Menu(MENU_EDIT, "Unindent", (s, evt) => unindent(), "^%I");

            menus.Menu($"&{MENU_BUILD}");
            menus.Menu(MENU_BUILD, "Run", (s, evt) => run(), "F5");
            menus.Menu(MENU_BUILD, "Manual", (s, evt) =>
            {
               manual = !manual;
               ((ToolStripMenuItem)s).Checked = manual;
            }, "^F5");

            menus.Menu($"&{MENU_TEXT}");
            menus.Menu(MENU_TEXT, "Paste from clipboard", (s, evt) =>
            {
               var text = ClipboardText();
               if (text.IsSome)
                  textText.Text = GetWindowsText(text.Value);
               tabs.SelectedTab = tabText;
            }, "^|V");
            menus.Menu(MENU_TEXT, "Copy to clipboard", (s, evt) =>
            {
               var text = textConsole.SelectionLength == 0 ? textConsole.Text : textConsole.SelectedText;
               text = GetWindowsText(text);
               SetText(text);
            }, "^|C");

            menus.Menu($"&{MENU_INSERT}");
            menus.Menu(MENU_INSERT, "|> println", (s, evt) => insertText(" |> println", 0, 0), "^P");
            menus.Menu(MENU_INSERT, "|> print", (s, evt) => insertText(" |> print", 0, 0));
            menus.Menu(MENU_INSERT, "|> put", (s, evt) => insertText(" |> put", 0, 0));
            menus.MenuSeparator(MENU_INSERT);
            menus.Menu(MENU_INSERT, "if template", (s, evt) => insertTemplate("if true", "true"), "^|I");
            menus.Menu(MENU_INSERT, "while template", (s, evt) => insertTemplate("while true", "true"), "^|W");
            menus.Menu(MENU_INSERT, "for template", (s, evt) => insertTemplate("for i in ^0", "^0"), "^|F");
            menus.Menu(MENU_INSERT, "func template", (s, evt) => insertTemplate("func proc()", "proc"), "^|P");

            menus.CreateMainMenu(this);

            menus.StandardContextEdit(document);

            locked = false;
            manual = false;
            stopwatch = new Stopwatch();
            fileCache = new InteractiveFileCache();

            if (settings.DefaultFolder != null)
               FolderName.Current = settings.DefaultFolder;
            textText.Text = settings.Text ?? "";
            if (settings.LastFile != null)
               document.Open(settings.LastFile);
         }
         catch (Exception exception)
         {
            displayException(exception);
         }
      }

      void duplicate()
      {
         var dupLine = "";

         if (textEditor.SelectionLength == 0)
         {
            var currentLineIndex = textEditor.CurrentLineIndex();
            if (textEditor.AtEnd() && textEditor.Text.IsMatch("/n $"))
               currentLineIndex--;
            var lines = textEditor.Lines;
            if (currentLineIndex.Between(0).Until(lines.Length))
               dupLine = lines[currentLineIndex].Copy();
         }
         else
            dupLine = textEditor.SelectedText;

         textEditor.AppendAtEnd(dupLine, "/n");
      }

      static void setCurrentFolder()
      {
         using (var dialog = new FolderBrowserDialog())
         {
            dialog.SelectedPath = FolderName.Current.ToString();
            if (dialog.ShowDialog() == DialogResult.OK)
               FolderName.Current = dialog.SelectedPath;
         }
      }

      void run() => update(true, true);

      void update(bool execute, bool fromMenu)
      {
         if (locked || textEditor.TextLength == 0)
            return;

         locked = true;

         if (manual)
         {
            labelResult.Text = "running...";
            DoEvents();
         }
         else if (fromMenu)
            document.Save();

         try
         {
            textConsole.Clear();
            textErrors.Clear();

            results.Clear();

            var orange = new Orange.Library.Orange(textEditor.Text, new ColorParser(textEditor), fileCache,
               new PlaygroundConsole(consoleWriter, textReader))
            {
               Text = textText.Text
            };
            orange.Statement += (sender, e) => results[e.Index] = e.Result;
            stopwatch.Reset();
            stopwatch.Start();
            if (execute)
               orange.Execute();
            else
               orange.ColorizeOnly();
            stopwatch.Stop();
            orange.Colorize();
            document.Clean();

            var lastValue = orange.LastValue;
            var lastType = orange.LastType;
            var resultText = lastValue.IsNotEmpty() ? $"{lastValue} | {lastType}" : "";
            var elapsed = stopwatch.Elapsed.ToLongString(true);
            labelResult.Text = resultText;
            labalElapsed.Text = elapsed;
            tabs.SelectedTab = tabConsole;
            textEditor.Select();

            var resultLines = textEditor.Lines.Select(line => "").ToArray();

            foreach (var item in results)
            {
               var lineIndex = textEditor.GetLineFromCharIndex(item.Key);
               resultLines[lineIndex] = item.Value;
            }
            textResults.Lines = resultLines;
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

      void displayException(Exception exception)
      {
         errorWriter.WriteLine(exception.Message);
         if (State != null)
         {
            consoleWriter.WriteLine("Dumping print buffer:");
            consoleWriter.WriteLine(State.PrintBuffer);
         }
         tabs.SelectedTab = tabErrors;
         textEditor.Select();
      }

      void textEditor_TextChanged(object sender, EventArgs e)
      {
         if (document == null)
            return;

         update(!manual, false);
         document.Dirty();
      }

      void insertText(string text, int selectionOffset, int length = -1)
      {
         textEditor.SelectedText = text;
         textEditor.SelectionStart += selectionOffset;
         if (length > -1)
            textEditor.SelectionLength = length;
      }

      void insertDelimiterText(string delimiter, int selectionOffset, int length, int halfLength = -1)
      {
         if (textEditor.SelectionLength == 0)
            insertText(delimiter, selectionOffset, length);
         else
         {
            var selection = textEditor.SelectedText;
            if (halfLength == -1)
               halfLength = delimiter.Length / 2;
            textEditor.SelectedText = delimiter.Take(halfLength) + selection + delimiter.Skip(halfLength);
         }
      }

      string textAtInsert(int take, int skip = 0) => textEditor.Text.Skip(textEditor.SelectionStart + skip).Take(take);

      void setTextAtInsert(int take, int skip = 0, string text = "")
      {
         textEditor.Select(textEditor.SelectionStart + skip, take);
         textEditor.SelectedText = text;
         textEditor.Select(textEditor.SelectionStart + skip, 0);
      }

      void moveSelectionRelative(int amount = 1) => textEditor.SelectionStart += amount;

      void textEditor_KeyPress(object sender, KeyPressEventArgs e)
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
                     insertPass();
                  else
                     moveSelectionRelative();
                  e.Handled = true;
               }
               break;
         }
      }

      void Playground_FormClosing(object sender, FormClosingEventArgs e)
      {
         if (document.FileName.IsSome)
         {
            try
            {
               settings.LastFile = document.FileName.Value;
               settings.DefaultFolder = FolderName.Current;
               settings.Text = textText.Text;
               var settingsGraph = ObjectGraph.Serialize(settings);
               FileName configurationFile = @"C:\Enterprise\Configurations\Orange\development.configuration";
               configurationFile.Text = settingsGraph.ToString();
            }
            catch (Exception exception)
            {
               MessageBox.Show(exception.Message);
            }
         }
      }

      void textEditor_KeyUp(object sender, KeyEventArgs e)
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
               getWord().If(word => findWord(word).If(found => insertText(found.Skip(word.Length), 0)));
               e.Handled = true;
               break;
         }
      }

      IMaybe<string> getWord()
      {
         var startIndex = textEditor.SelectionStart;
         var begin = textEditor.Text.Take(startIndex);
         return begin.Matches("/(/w+) $").Map(m => m.FirstGroup);
      }

      IMaybe<string> findWord(string begin)
      {
         var matches = textEditor.Text.Split("-/w+").Distinct().Where(word => word.StartsWith(begin));
         return matches.FirstOrNone();
      }

      void matchPass(bool first)
      {
         var matches = textEditor.Text.Matches("/b 'pass' /b");
         matches.If(matcher =>
         {
            var passMatch = matcher.GetMatch(first ? 0 : matcher.MatchCount - 1);
            textEditor.Select(passMatch.Index, passMatch.Length);
         });
      }

      string getIndent() => textEditor.CurrentLine().Matches("^ /t*").Map(m => m[0], () => "");

      void insertTemplate(string template, string replacement)
      {
         var indent = getIndent();
         var expandedTemplate = $"{indent}{template}\n{indent}\tpass";
         var index = expandedTemplate.IndexOf(replacement);
         if (index == -1)
            insertText(expandedTemplate, 0, 0);
         else
         {
            index = -(expandedTemplate.Length - index);
            insertText(expandedTemplate, index, replacement.Length);
         }
      }

      void insertPass()
      {
         var indent = getIndent();
         var text = $"\n{indent}\tpass";
         insertText(text, -text.Length, 0);
      }

      IMaybe<Tuple<string[], int>> linesFromSelection()
      {
         var startIndex = textEditor.GetLineFromCharIndex(textEditor.SelectionStart);
         var stopIndex = textEditor.GetLineFromCharIndex(textEditor.SelectionStart + textEditor.SelectionLength);
         var lines = textEditor.Lines;
         if (startIndex.Between(0).Until(lines.Length) && stopIndex.Between(0).Until(lines.Length))
            return tuple(lines.RangeOf(startIndex, stopIndex), startIndex).Some();
         return new None<Tuple<string[], int>>();
      }

      void indent()
      {
         try
         {
            var lines = textEditor.Lines;
            string[] selectedLines;
            int offset;
            if (linesFromSelection().Assign(out selectedLines, out offset))
            {
               var saved = saveSelection();
               for (var i = 0; i < selectedLines.Length; i++)
                  textEditor.Lines[i + offset] = $"\t{selectedLines[i]}";
               textEditor.Lines = lines;
               restoreSelection(saved);
            }
         }
         catch (Exception exception)
         {
            errorWriter.WriteLine(exception.Message);
         }
      }

      void unindent()
      {
         try
         {
            var lines = textEditor.Lines;
            string[] selectedLines;
            int offset;
            if (linesFromSelection().Assign(out selectedLines, out offset))
            {
               var saved = saveSelection();
               for (var i = 0; i < selectedLines.Length; i++)
                  selectedLines[i].Matches("^ /t /@").If(matcher => lines[i + offset] = matcher.FirstGroup);
               textEditor.Lines = lines;
               restoreSelection(saved);
            }
         }
         catch (Exception exception)
         {
            errorWriter.WriteLine(exception.Message);
         }
      }

      Tuple<int, int> saveSelection() => tuple(textEditor.SelectionStart, textEditor.SelectionLength);

      void restoreSelection(Tuple<int, int> saved) => textEditor.Select(saved.Item1, saved.Item2);

      void textEditor_VScroll(object sender, EventArgs e)
      {
         try
         {
            var currentLine = textEditor.CurrentLineIndex();
            var position = textResults.GetFirstCharIndexFromLine(currentLine);
            if (position < 0)
               return;
            textResults.Select(position, 0);
         }
         catch
         {
         }
      }

      void textResults_SelectionChanged(object sender, EventArgs e)
      {
         try
         {
            labelResult.Text = textResults.CurrentLine();
         }
         catch
         {
         }
      }
   }
}