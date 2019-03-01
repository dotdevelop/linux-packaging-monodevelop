// 
// MenuBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using AppKit;
using CoreGraphics;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class MenuBackend: NSMenu, IMenuBackend
	{
		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			AutoEnablesItems = false;
		}

		public void InsertItem (int index, IMenuItemBackend menuItem)
		{
			base.InsertItem (((MenuItemBackend)menuItem).Item, index);
		}

		public void RemoveItem (IMenuItemBackend menuItem)
		{
			RemoveItem (((MenuItemBackend)menuItem).Item);
		}
		
		public void SetMainMenuMode ()
		{
			for (int n=0; n<Count; n++) {
				var it = ItemAt (n);
				if (it.Menu != null)
					it.Submenu.Title = it.Title;
			}
		}

		public void EnableEvent (object eventId)
		{
		}

		public void DisableEvent (object eventId)
		{
		}
		
		public void Popup ()
		{
			var evt = NSApplication.SharedApplication.CurrentEvent;
			if (evt != null)
				NSMenu.PopUpContextMenu (this, evt, evt.Window.ContentView);
			else if (MainWindow?.ContentView != null)
				Popup (MainWindow.ContentView, MainWindow.MouseLocationOutsideOfEventStream);

		}
		
		public void Popup (IWidgetBackend widget, double x, double y)
		{
			Popup (((ViewBackend)widget).Widget, new CGPoint (x, y));
		}

		void Popup (NSView view, CGPoint point)
		{
			this.PopUpMenu (null, point, view);
		}

		NSWindow MainWindow {
			get {
				return NSApplication.SharedApplication.MainWindow;
			}
		}

		object IMenuBackend.Font {
			get {
				return FontData.FromFont (Font);
			}
			set {
				Font = ((FontData)value).Font;
			}
		}
	}
}

