// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace MonoDevelop.Deployment.Linux {
    
    public partial class DesktopPanelWidget {
        
        private Gtk.Notebook notebook2;
        
        private Gtk.Label label6;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget MonoDevelop.Deployment.Linux.DesktopPanelWidget
            Stetic.BinContainer.Attach(this);
            this.CanFocus = true;
            this.Name = "MonoDevelop.Deployment.Linux.DesktopPanelWidget";
            // Container child MonoDevelop.Deployment.Linux.DesktopPanelWidget.Gtk.Container+ContainerChild
            this.notebook2 = new Gtk.Notebook();
            this.notebook2.CanFocus = true;
            this.notebook2.Name = "notebook2";
            this.notebook2.CurrentPage = 0;
            // Notebook tab
            Gtk.Label w1 = new Gtk.Label();
            w1.Visible = true;
            this.notebook2.Add(w1);
            this.label6 = new Gtk.Label();
            this.label6.CanFocus = true;
            this.label6.Name = "label6";
            this.label6.LabelProp = "page1";
            this.notebook2.SetTabLabel(w1, this.label6);
            this.label6.ShowAll();
            this.Add(this.notebook2);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.Show();
        }
    }
}