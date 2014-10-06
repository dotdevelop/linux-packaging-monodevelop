//
// MDLinkMenuItem.cs
//
// Author:
//       Michael Hutchinson <m.j.hutchinson@gmail.com>
//
// Copyright (c) 2013 Xamarin Inc.
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

using MonoMac.AppKit;
using MonoDevelop.Components.Commands;
using MonoMac.Foundation;

namespace MonoDevelop.MacIntegration.MacMenu
{
	class MDLinkMenuItem : NSMenuItem, IUpdatableMenuItem
	{
		LinkCommandEntry lce;

		public MDLinkMenuItem (LinkCommandEntry lce)
		{
			this.lce = lce;
			this.Title = lce.Text;

			Target = this;
			Action = MDMenuItem.ActionSel;
		}

		[Export (MDMenuItem.ActionSelName)]
		public void Run ()
		{
			MonoDevelop.Ide.DesktopService.ShowUrl (lce.Url);
		}

		public void Update (MDMenu parent, ref NSMenuItem lastSeparator, ref int index)
		{
			MDMenu.ShowLastSeparator (ref lastSeparator);
		}
	}
}
