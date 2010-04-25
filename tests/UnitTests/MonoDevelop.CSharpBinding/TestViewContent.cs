//
// TestViewContent.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Mono.TextEditor;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;

namespace MonoDevelop.CSharpBinding.Tests
{
	public class TestViewContent : AbstractViewContent, IEditableTextBuffer, Mono.TextEditor.ITextEditorDataProvider
	{
		TextEditorData data;
		
		public override Gtk.Widget Control {
			get {
				return null;
			}
		}
		

		{
			document = new Mono.TextEditor.Document ();
			data = new TextEditorData (document);
			Name = "";
		}
		
		public override void Load(string fileName)
		{
		}
		
		public FilePath Name { 
			get;
			set;
		}
		
		public int LineCount {
			get {
				return document.LineCount;
			}
		}
		
		Mono.TextEditor.Document document;
		public string Text {
			get {
				return document.Text;
			}
			set {
				document.Text = value;
			}
		}
		
		public int InsertText (int position, string text)
		{
			((IBuffer)document).Insert (position, text);
			return text.Length;
		}
		
		public void DeleteText (int position, int length)
		{
			((IBuffer)document).Replace (position, length, "");
		}
		
		public int Length {
			get {
				return document.Length;
			}
		}
		
		public string GetText (int startPosition, int endPosition)
		{
			return document.GetTextBetween (startPosition, endPosition);
		}
		public char GetCharAt (int position)
		{
			return document.GetCharAt (position);
		}
		
		public int GetPositionFromLineColumn (int line, int column)
		{
			return document.LocationToOffset (line - 1, column - 1);
		}
		
		public void GetLineColumnFromPosition (int position, out int line, out int column)
		{
			DocumentLocation loc = document.OffsetToLocation (position);
			line = loc.Line + 1;
			column = loc.Column + 1;
		}
		
		public string SelectedText { get { return ""; } set { } }
		
		public int CursorPosition {
			get {
				return data.Caret.Offset;
			}
			set {
				data.Caret.Offset = value;
			}
		}

		public int SelectionStartPosition { 
			get {
				if (!data.IsSomethingSelected)
					return data.Caret.Offset;
				return data.SelectionRange.Offset;
			}
		}
		
		public int SelectionEndPosition { 
			get {
				if (!data.IsSomethingSelected)
					return data.Caret.Offset;
				return data.SelectionRange.EndOffset;
			}
		}
		
		public void Select (int startPosition, int endPosition)
		{
			data.SelectionRange = new Segment (startPosition, endPosition - startPosition);
		}
		
		public void ShowPosition (int position)
		{
		}
		
		public bool EnableUndo {
			get {
				return false;
			}
		}
		public bool EnableRedo {
			get {
				return false;
			}
		}
		
		public void SetCaretTo (int line, int column)
		{
		}
		public void SetCaretTo (int line, int column, bool highlightCaretLine)
		{
		}
		
		public void Undo()
		{
		}
		public void Redo()
		{
		}
		
		public void BeginAtomicUndo ()
		{
		}
		public void EndAtomicUndo ()
		{
		}
		
		public TextEditorData GetTextEditorData ()
		{
			return data;
		}
		public event EventHandler CaretPositionSet;
		public event EventHandler<TextChangedEventArgs> TextChanged;
	}
}