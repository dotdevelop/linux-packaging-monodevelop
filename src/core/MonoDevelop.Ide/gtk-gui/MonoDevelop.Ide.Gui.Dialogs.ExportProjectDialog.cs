// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace MonoDevelop.Ide.Gui.Dialogs {
    
    public partial class ExportProjectDialog {
        
        private Gtk.VBox vbox2;
        
        private Gtk.Table table1;
        
        private Gtk.ComboBox comboFormat;
        
        private MonoDevelop.Components.FolderEntry folderEntry;
        
        private Gtk.Label label2;
        
        private Gtk.Label label3;
        
        private Gtk.Label label4;
        
        private Gtk.Label labelNewFormat;
        
        private Gtk.Button button51;
        
        private Gtk.Button buttonOk;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget MonoDevelop.Ide.Gui.Dialogs.ExportProjectDialog
            this.Events = ((Gdk.EventMask)(256));
            this.Name = "MonoDevelop.Ide.Gui.Dialogs.ExportProjectDialog";
            this.Title = Mono.Unix.Catalog.GetString("Export Project");
            this.WindowPosition = ((Gtk.WindowPosition)(4));
            this.BorderWidth = ((uint)(6));
            // Internal child MonoDevelop.Ide.Gui.Dialogs.ExportProjectDialog.VBox
            Gtk.VBox w1 = this.VBox;
            w1.Events = ((Gdk.EventMask)(256));
            w1.Name = "dialog_VBox";
            w1.Spacing = 6;
            w1.BorderWidth = ((uint)(2));
            // Container child dialog_VBox.Gtk.Box+BoxChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            this.vbox2.Spacing = 12;
            this.vbox2.BorderWidth = ((uint)(6));
            // Container child vbox2.Gtk.Box+BoxChild
            this.table1 = new Gtk.Table(((uint)(3)), ((uint)(2)), false);
            this.table1.Name = "table1";
            this.table1.RowSpacing = ((uint)(6));
            this.table1.ColumnSpacing = ((uint)(6));
            // Container child table1.Gtk.Table+TableChild
            this.comboFormat = Gtk.ComboBox.NewText();
            this.comboFormat.Name = "comboFormat";
            this.table1.Add(this.comboFormat);
            Gtk.Table.TableChild w2 = ((Gtk.Table.TableChild)(this.table1[this.comboFormat]));
            w2.TopAttach = ((uint)(1));
            w2.BottomAttach = ((uint)(2));
            w2.LeftAttach = ((uint)(1));
            w2.RightAttach = ((uint)(2));
            w2.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.folderEntry = new MonoDevelop.Components.FolderEntry();
            this.folderEntry.Name = "folderEntry";
            this.table1.Add(this.folderEntry);
            Gtk.Table.TableChild w3 = ((Gtk.Table.TableChild)(this.table1[this.folderEntry]));
            w3.TopAttach = ((uint)(2));
            w3.BottomAttach = ((uint)(3));
            w3.LeftAttach = ((uint)(1));
            w3.RightAttach = ((uint)(2));
            w3.XOptions = ((Gtk.AttachOptions)(4));
            w3.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label2 = new Gtk.Label();
            this.label2.Name = "label2";
            this.label2.Xalign = 0F;
            this.label2.LabelProp = Mono.Unix.Catalog.GetString("Target folder:");
            this.table1.Add(this.label2);
            Gtk.Table.TableChild w4 = ((Gtk.Table.TableChild)(this.table1[this.label2]));
            w4.TopAttach = ((uint)(2));
            w4.BottomAttach = ((uint)(3));
            w4.XOptions = ((Gtk.AttachOptions)(4));
            w4.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label3 = new Gtk.Label();
            this.label3.Name = "label3";
            this.label3.Xalign = 0F;
            this.label3.LabelProp = Mono.Unix.Catalog.GetString("New format:");
            this.table1.Add(this.label3);
            Gtk.Table.TableChild w5 = ((Gtk.Table.TableChild)(this.table1[this.label3]));
            w5.TopAttach = ((uint)(1));
            w5.BottomAttach = ((uint)(2));
            w5.XOptions = ((Gtk.AttachOptions)(4));
            w5.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label4 = new Gtk.Label();
            this.label4.Name = "label4";
            this.label4.Xalign = 0F;
            this.label4.LabelProp = Mono.Unix.Catalog.GetString("Current format:");
            this.table1.Add(this.label4);
            Gtk.Table.TableChild w6 = ((Gtk.Table.TableChild)(this.table1[this.label4]));
            w6.XOptions = ((Gtk.AttachOptions)(4));
            w6.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.labelNewFormat = new Gtk.Label();
            this.labelNewFormat.Name = "labelNewFormat";
            this.labelNewFormat.Xalign = 0F;
            this.table1.Add(this.labelNewFormat);
            Gtk.Table.TableChild w7 = ((Gtk.Table.TableChild)(this.table1[this.labelNewFormat]));
            w7.LeftAttach = ((uint)(1));
            w7.RightAttach = ((uint)(2));
            w7.XOptions = ((Gtk.AttachOptions)(4));
            w7.YOptions = ((Gtk.AttachOptions)(4));
            this.vbox2.Add(this.table1);
            Gtk.Box.BoxChild w8 = ((Gtk.Box.BoxChild)(this.vbox2[this.table1]));
            w8.Position = 0;
            w8.Expand = false;
            w8.Fill = false;
            w1.Add(this.vbox2);
            Gtk.Box.BoxChild w9 = ((Gtk.Box.BoxChild)(w1[this.vbox2]));
            w9.Position = 0;
            w9.Expand = false;
            w9.Fill = false;
            // Internal child MonoDevelop.Ide.Gui.Dialogs.ExportProjectDialog.ActionArea
            Gtk.HButtonBox w10 = this.ActionArea;
            w10.Name = "MonoDevelop.Ide.ExportProjectDialog_ActionArea";
            w10.Spacing = 6;
            w10.BorderWidth = ((uint)(5));
            w10.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
            // Container child MonoDevelop.Ide.ExportProjectDialog_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.button51 = new Gtk.Button();
            this.button51.CanDefault = true;
            this.button51.CanFocus = true;
            this.button51.Name = "button51";
            this.button51.UseStock = true;
            this.button51.UseUnderline = true;
            this.button51.Label = "gtk-cancel";
            this.AddActionWidget(this.button51, -6);
            Gtk.ButtonBox.ButtonBoxChild w11 = ((Gtk.ButtonBox.ButtonBoxChild)(w10[this.button51]));
            w11.Expand = false;
            w11.Fill = false;
            // Container child MonoDevelop.Ide.ExportProjectDialog_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.buttonOk = new Gtk.Button();
            this.buttonOk.CanDefault = true;
            this.buttonOk.CanFocus = true;
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.UseStock = true;
            this.buttonOk.UseUnderline = true;
            this.buttonOk.Label = "gtk-ok";
            this.AddActionWidget(this.buttonOk, -5);
            Gtk.ButtonBox.ButtonBoxChild w12 = ((Gtk.ButtonBox.ButtonBoxChild)(w10[this.buttonOk]));
            w12.Position = 1;
            w12.Expand = false;
            w12.Fill = false;
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.DefaultWidth = 509;
            this.DefaultHeight = 195;
            this.Show();
            this.folderEntry.PathChanged += new System.EventHandler(this.OnFolderEntryPathChanged);
        }
    }
}