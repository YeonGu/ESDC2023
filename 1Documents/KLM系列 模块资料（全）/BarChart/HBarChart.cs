using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;

namespace BarChart
{
    [
    ComplexBindingProperties("DataSource", "DataMember"), 
    ToolboxBitmapAttribute(typeof(BarChart.HBarChart), "BarChart.bmp")]
    public partial class HBarChart : UserControl
    {
        #region Fields
        private CDescriptionProperty description;
        private CLabelProperty label;
        private CValueProperty values;
        private CBackgroundProperty background;
        private BarSizingMode sizingMode;
        private CBorderProperty border;
        private CShadowProperty shadow;
        private int nBarWidth;
        private int nBarsGap;
        private CDataSourceManager dataSourceManager;

        // 
        private Rectangle rectBK;
        // visible area of the chart
        private Rectangle bounds;
        private RectangleF rectDesc;

        // A collection of all bars data
        //protected HItems bars;
        protected HBarItems bars;

        // Tooltip of the chart
        protected ToolTip tooltip;

        // A back buffer for double buffering to have flicker-free drawing
        private Bitmap bmpBackBuffer;

        // Used in MouseMove event to trak index of the last bar under the mouse
        [Browsable(false)]
        private int nLastVisitedBarIndex;

        // It seems that since tooltip class draws the tip over mouse cursor
        // the MouseMove event is raised constantly, if I call setTooltip to
        // change tooltip text in MouseMove event. To ignore Tooltip from being
        // repeatedly redrawing, while I don't have time to make it owner drawn
        // and prevent drawing over cursor, I'll ignor move events after first one.
        [Browsable(false)]
        private Point ptLastTooltipMouseLoction;
        
        #endregion

        #region Properties

        /// <summary>
        /// Underling collection of all bars, a list of HBarItem objects each of which correspond to a bar.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Bar Chart"), Description("A collection of chart items. A bar for each item will be drawn.")]
        public HBarItems Items
        {
            get { return bars; }
            set { bars = value;}
        }

        /// <summary>
        /// A description line of text at the bottom of the chart.
        /// </summary>
         [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Bar Chart"), Description("Look and feel of the description line at the bottom of the chart.")]
        public CDescriptionProperty Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// A boredr around chart main rectangle.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Bar Chart"), Description("Settings of the border around the chart.")]
        public CBorderProperty Border
        {
            get { return this.border; }
            set { this.border = value; }
        }

        /// <summary>
        /// Outer and inner shadows around chart border.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Bar Chart"), Description("Settings of the shadows around the chart.")]
        public CShadowProperty Shadow
        {
            get { return this.shadow; }
            set { this.shadow = value; }
        }

        /// <summary>
        /// Settings of the text drawn for each bar describing what the bar is displaying.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Bar Chart"), Description("Look and feel of the label at the bottom of each bar.")]
        public CLabelProperty Label
        {
            get { return label; }
            set { label = value; }
        }

        /// <summary>
        /// Settings of values(or %) each bar displays.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Bar Chart"), Description("Look and feel of the Value/Percent presented at the top of each bar.")]
        public CValueProperty Values
        {
            get { return values; }
            set { values = value; }
        }

        /// <summary>
        /// Background of the chart, might be a solid color, linear gradient or radial gradient.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Bar Chart"), Description("Chart background style and colors.")]
        public CBackgroundProperty Background
        {
            get { return background; }
            set { background = value; }
        }

        /// <summary>
        /// Default width of each bar. Has no effect in Autoscale sizing mode.
        /// </summary>
        [Browsable(true)]
        [Category("Bar Chart")]
        public int BarWidth
        {
            get { return nBarWidth; }
            set { nBarWidth = value; }
        }

        /// <summary>
        /// Space between bars of the bar graph, and between bars and borders of chart
        /// </summary>
        [Browsable(true)]
        [Category("Bar Chart")]
        public int BarsGap
        {
            get { return nBarsGap; }
            set { nBarsGap = value; }
        }

        /// <summary>
        /// Tooltip class of the chart
        /// </summary>
        [Browsable(true)]
        [Category("Bar Chart")]
        public ToolTip BarTooltip
        {
            get { return tooltip; }
            set { tooltip = value; }
        }

        public enum BarSizingMode
        {
            Normal,         // Use variable values for width of the bar
            AutoScale       // Automatically calculate the bounding rectangle and fit all bars inside the control
        }

        /// <summary>
        /// Enumerator defining sizing capabilities of the chart.
        /// </summary>
        [Browsable(true)]
        [Category("Bar Chart")]
        public BarSizingMode SizingMode
        {
            get { return sizingMode; }
            set { sizingMode = value; }
        }

        /// <summary>
        /// Gets number of bars of the chart
        /// </summary>
        [Browsable(false)]
        [Category("Bar Chart")]
        public int Count
        {
            get { return bars.Count; }
        }

        /// <summary>
        /// get or set data member of the connected data source. Chart reads data of this data member.
        /// </summary>
        [
         DefaultValue(""),
         Category("Bar Chart"),
		 Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design",
			 "System.Drawing.Design.UITypeEditor, System.Drawing"),
         Description("Defines data member of the connected data source. Chart reads data of this data member.")
        ]
        public string DataMember
        {
            get
            {
                if (this.dataSourceManager == null)
                {
                    return String.Empty;
                }
                else
                {
                    return this.dataSourceManager.DataMember;
                }
            }
            set
            {
                if (value != this.DataMember)
                {
                    if (this.dataSourceManager == null)
                    {
                        CreateChartForEachRow eventHandler = new CreateChartForEachRow();
                        this.dataSourceManager = new CDataSourceManager(this);
                        this.dataSourceManager.DataEventHandler = eventHandler;
                    }
                    this.dataSourceManager.ConnectTo(this.DataSource, value);
                }
            }
        }

        /// <summary>
        /// Get or Set Data Source to connected to.
        /// </summary>
        [
         DefaultValue(null),
         RefreshProperties(RefreshProperties.Repaint),
         AttributeProvider(typeof(IListSource)),
         Category("Bar Chart"),
         Description("Defines Data Source to connected to."),
		 TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")
        ]
        public object DataSource
        {
            get
            {
                if (this.dataSourceManager == null)
                {
                    return null;
                }
                else
                {
                    return this.dataSourceManager.DataSource;
                }
            }
            set
            {
                if (value != this.DataSource)
                {
                    if (this.dataSourceManager == null)
                    {
                        CreateChartForEachRow eventHandler = new CreateChartForEachRow();
                        this.dataSourceManager = new CDataSourceManager(this);

                        this.dataSourceManager.DataEventHandler = eventHandler;
                        this.dataSourceManager.ConnectTo(value, this.DataMember);
                    }
                    else
                    {
                        this.dataSourceManager.ConnectTo(value, this.DataMember);
                        if (value == null)
                        {
                            this.dataSourceManager = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Data manger is responsible for coordinating dataconnection and data event handler.
        /// </summary>
        [Browsable(false)]
        public CDataSourceManager DataSourceManager
        {
            get { return this.dataSourceManager; }
        }

        #endregion //"Properties"

        #region CustomEvents

        /// <summary>
        /// Delegate type of the barchart bar related events
        /// </summary>
        /// <param name="sender">The HBarChart who sent the event</param>
        /// <param name="e">BarEventArgs that contains event information</param>
        public delegate void OnBarEvent(object sender, BarEventArgs e);

        /// <summary>
        /// Mouse moved into territory of a bar :-)
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Bar Chart"), Description("Mouse is now over a bar rectangle starting from top of the chart, left of the bar and ending right of the bar and bottom of the chart.")]
        public event OnBarEvent BarMouseEnter;

        /// <summary>
        /// Mouse just left a bar
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Bar Chart"), Description("Mouse just hovered out a bar.")]
        public event OnBarEvent BarMouseLeave;

        /// <summary>
        /// Mouse click event occured on a bar
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Bar Chart"), Description("Mouse click event occurd while mouse is over a bar.")]
        public event OnBarEvent BarClicked;

        /// <summary>
        /// Mouse double click on a bar
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Bar Chart"), Description("Mouse double click event occurd while mouse is over a bar")]
        public event OnBarEvent BarDoubleClicked;

        // A bar clicked
        private void RaiseClickEvent(HBarItem bar, int nIndex)
        {
            if (BarClicked != null)
            {
                BarClicked(this, new BarEventArgs(bar, nIndex));
            }
        }

        // A bar double clicked
        private void RaiseDoubleClickEvent(HBarItem bar, int nIndex)
        {
            if (BarDoubleClicked != null)
            {
                BarDoubleClicked(this, new BarEventArgs(bar, nIndex));
            }
        }

        // Mouse moved over a bar
        private void RaiseHoverInEvent(HBarItem bar, int nIndex)
        {
            if (BarMouseEnter != null)
            {
                BarMouseEnter(this, new BarEventArgs(bar, nIndex));
            }
        }

        // Mouse moved out over a bar
        private void RaiseHoverOutEvent(HBarItem bar, int nIndex)
        {
            if (BarMouseLeave != null)
            {
                BarMouseLeave(this, new BarEventArgs(bar, nIndex));
            }
        }

        #endregion // Events

        #region Methods
        
        /// <summary>
        /// Causes the modifications to be trigered to GUI of the chart.
        /// </summary>
        public void RedrawChart()
        {
            if (bmpBackBuffer != null)
            {
                bmpBackBuffer.Dispose();
                bmpBackBuffer = null;
            }

            this.Refresh();
        }

        /// <summary>
        /// Add a new item(bar) to the chart
        /// </summary>
        /// <param name="dValue">Double value of the new bar</param>
        /// <param name="strLabel">Label description of the bar</param>
        /// <param name="colorBar">Color of the bar</param>
        public void Add(double dValue, string strLabel, Color colorBar)
        {
            bars.Add(new HBarItem(dValue, strLabel, colorBar));
        }

        /// <summary>
        /// Remove a bar by it's a zero based index from chart
        /// </summary>
        /// <param name="nIndex">Index of the bar to be removed</param>
        /// <returns>true if item removed or false in case of any error, most likely index out of range</returns>
        public bool RemoveAt(int nIndex)
        {
            if (nIndex < 0 || nIndex >= bars.Count) return false;

            bars.RemoveAt(nIndex);
            return true;
        }

        /// <summary>
        /// Retrieve a bar by it's a zero based index
        /// </summary>
        /// <param name="nIndex">Index of the bar</param>
        /// <param name="bar">Out parameter. Will hold the bar after retrieving it</param>
        /// <returns>true if bar exists and retrieved, otherwise false</returns>
        public bool GetAt(int nIndex, out HBarItem bar)
        {
            bar = null;
            if (nIndex < 0 || nIndex >= bars.Count) return false;

            bar = bars[nIndex];
            return true;
        }

        /// <summary>
        /// Change current value of a bar
        /// </summary>
        /// <param name="nIndex">Zero based index of the bar</param>
        /// <param name="dNewValue">New value to replace existing value of the bar</param>
        /// <returns>true if changed successfully</returns>
        public bool ModifyAt(int nIndex, double dNewValue)
        {
            if (nIndex < 0 || nIndex >= bars.Count) return false;

            bars[nIndex].Value = dNewValue;
            return true;
        }

        /// <summary>
        /// Change a bar with a new one
        /// </summary>
        /// <param name="nIndex">Zero based index of the bar</param>
        /// <param name="barNew">New properties of the bar</param>
        /// <returns></returns>
        public bool ModifyAt(int nIndex, HBarItem barNew)
        {
            if (nIndex < 0 || nIndex >= bars.Count) return false;

            bars.RemoveAt(nIndex);
            bars.Insert(nIndex, barNew);

            return true;
        }

        /// <summary>
        /// Insert a new bar at a specified zero based index
        /// </summary>
        /// <param name="nIndex">Zero based index of the bar</param>
        /// <param name="dValue">New value of the bar</param>
        /// <param name="strLabel">Label of the bar</param>
        /// <param name="colorBar">Color of the bar</param>
        /// <returns>true if bar inserted otherwise false.</returns>
        public bool InsertAt(int nIndex, double dValue, string strLabel, Color colorBar)
        {
            if (nIndex < 0 || nIndex >= bars.Count) return false;

            bars.Insert(nIndex, new HBarItem(dValue, strLabel, colorBar));

            return true;
        }

        /// <summary>
        /// Prints the chart in a WYSIWYG manner
        /// </summary>
        /// <param name="bFitToPaper">If true, chart will fill whole paper surface</param>
        /// <param name="strDocName">A name for print job document.</param>
        /// <returns></returns>
        public bool Print(bool bFitToPaper, string strDocName)
        {
            CPrinter printer = new CPrinter();

            // Ask user to select a printer and set options for it
            printer.ShowOptions();
            
            // Customize the document and sizing mode
            printer.Document.DocumentName = strDocName;
            printer.FitToPaper = bFitToPaper;

            // Create and prepare a bitmap to be printed into printer DC
            Bitmap bmpChart;
            if (bFitToPaper)
            {
                // Full screen
                bmpChart = new Bitmap(
                    printer.Document.DefaultPageSettings.Bounds.Width,
                    printer.Document.DefaultPageSettings.Bounds.Height);
            }
            else
            {
                // WYSIWYG
                bmpChart = (Bitmap)bmpBackBuffer.Clone();
            }
            // Draw On the bitmap
            DrawChart(ref bmpChart);
            
            // Set bitmap for printing
            printer.BmpBuffer = bmpChart;
            
            // Ask printer class to print its bitmap.
            bool bRet = false;
            bRet =  printer.Print();

            // Remove bitmap from memory
            bmpChart.Dispose();
            bmpChart = null;

            return bRet;
        }

        // Will be called when chart is being resized. We need to redraw the chart.
        private void OnSize(object sender, EventArgs e)
        {
            RedrawChart();
        }

        // Wanna connect this to a data source?
        protected override void OnBindingContextChanged(EventArgs e)
        {
            try
            {
                if (this.dataSourceManager != null)
                {
                    try
                    {
                        this.dataSourceManager.ConnectTo(this.DataSource, this.DataMember);
                    }
                    catch (ArgumentException)
                    {
                        if (this.DesignMode)
                        {
                            this.DataMember = String.Empty;
                            return;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    base.OnBindingContextChanged(e);
                }
            }
            finally
            {
            }
        }

        // To add a null bar
        internal void Add(object nullObject)
        {
            // UNDONE: Char must display something like a question mark here
            //         so that users know that no value is available for this bar
            this.Add(0.0, "N/A", Color.Black);
        }

        #endregion

        #region constructors
        
        // Constructor
        public HBarChart()
        {
            this.bounds = new Rectangle(0, 0, 0, 0);
            this.border = new CBorderProperty();
            this.shadow = new CShadowProperty();
            rectDesc = new RectangleF(0, 0, 0, 0);
            
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            // Designer
            InitializeComponent();

            description = new CDescriptionProperty();
            label = new CLabelProperty();
            values = new CValueProperty();
            background = new CBackgroundProperty();

            // Initialize members
            nBarWidth = 24;
            nBarsGap = 4;

            SizingMode = BarSizingMode.Normal;

            //fontTooltip = new Font("Verdana", 12);

            bars = new HBarItems();

            bmpBackBuffer = null;

            ptLastTooltipMouseLoction = new Point(0, 0);
            tooltip = new ToolTip();
            tooltip.IsBalloon = true;
            //tooltip.ShowAlways = true;
            tooltip.InitialDelay = 100;
            tooltip.ReshowDelay = 100;
            //tooltip.AutoPopDelay = Int32.MaxValue;

            nLastVisitedBarIndex = -1;
        }

        #endregion

        #region Drawings
        // Control needs repainting
        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (bmpBackBuffer==null)
            {
                // Redraw the char into back buffer
                DrawChart(ref bmpBackBuffer);
            }

            // Blot the buffer to view
            if (bmpBackBuffer != null)
            {
                /*e.Graphics.DrawImageUnscaled(bmpBackBuffer, 0, 0);*/
               
                e.Graphics.DrawImage(
                    bmpBackBuffer, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
            }
        }

        // Draws a chart on the given bitmap
        private void DrawChart(ref Bitmap bmp)
        {
           
            if (bmp == null)
            {
                bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            }

            using (Graphics gr = Graphics.FromImage(bmp))
            {
                CalculateBound(bmp.Size);

                // Draw background
                Background.Draw(gr, this.rectBK);

                //Draw graph and all texts
                DrawBars(gr, bmp.Size);
            }
        }

        // Calculates bounding rectangle of the chart, border and shadows
        private void CalculateBound(Size sizeClient)
        {
            // Calculate bounding rectangle
            this.bounds = new Rectangle(0, 0, sizeClient.Width, sizeClient.Height);

            if (this.shadow.Mode == CShadowProperty.Modes.Outer || this.shadow.Mode == CShadowProperty.Modes.Both)
            {
                this.shadow.SetRect(this.bounds, 1);

                this.bounds.X += this.shadow.WidthOuter;
                this.bounds.Y += this.shadow.WidthOuter;
                this.bounds.Width -= 2 * this.shadow.WidthOuter;
                this.bounds.Height -= 2 * this.shadow.WidthOuter;
            }
            rectBK = new Rectangle(this.bounds.X, this.bounds.Y, this.bounds.Width, this.bounds.Height);

            if (this.border != null && Border.Visible)
            {
                this.border.SetRect(this.bounds);

                this.bounds.X += this.Border.Width;
                this.bounds.Y += this.Border.Width;
                this.bounds.Width -= 2 * this.Border.Width;
                this.bounds.Height -= 2 * this.Border.Width;
            }

            if (this.shadow.Mode == CShadowProperty.Modes.Inner || this.shadow.Mode == CShadowProperty.Modes.Both)
            {
                this.shadow.SetRect(this.bounds, 0);
                /*
                this.bounds.X += this.shadow.WidthInner;
                this.bounds.Y += this.shadow.WidthInner;
                this.bounds.Width -= 2 * this.shadow.WidthInner;
                this.bounds.Height -= 2 * this.shadow.WidthInner;*/
            }

        }

        // Draws bars of the chart along with labels and values
        private void DrawBars(Graphics gr, Size sizeChart)
        {
            if (description==null) return;
            if (label==null) return;
            if (values == null) return;

            // Store some original values
            int nLastBarGaps = nBarsGap;

            // Other calculations
            if (SizingMode == BarSizingMode.AutoScale)
            {
                int nbarWidthTemp = nBarWidth;

                // Calculate gap size
                if (bars.Count > 0)
                {
                    nBarsGap = 4 + (12 * this.bounds.Width) / (int)(345 * bars.Count * 7);
                    if (nBarsGap > 50) nBarsGap = 50;

                    // Calculate maximum bar size
                    nBarWidth = (this.bounds.Width - ((bars.Count + 1) * nBarsGap)) / bars.Count;
                    if (nBarWidth <= 0) nBarWidth = 24;
                }

                // Calcuate font sizes & create fonts
                CreateLabelFont(gr, new Size(nBarWidth, 0));
                CreateValueFont(gr, new Size(nBarWidth, 0));
                CreateDescFont(gr, this.bounds.Size);
                
                CalculatePositions(gr);
                
                nBarWidth = nbarWidthTemp;
            }
            else
            {
                if (this.values.Font == null || this.values.Font.Size != values.FontDefaultSize)
                {
                    this.Values.FontReset();
                }
                if (this.Label.Font == null || this.Label.Font.Size != this.Label.FontDefaultSize)
                {
                    this.Label.FontReset();
                }
                if (this.Description.Font == null || this.Description.Font.Size != this.Description.FontDefaultSize)
                {
                    this.Description.FontReset();
                }

                CalculatePositions(gr);
            }

            this.shadow.Draw(gr, this.BackColor);

            // Draw description line
            if (Description.Visible && Description.Font!=null)
            {
                StringFormat stringFormat = StringFormat.GenericDefault;
                stringFormat.LineAlignment = StringAlignment.Center;
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.Trimming = StringTrimming.None;
                stringFormat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;

                gr.DrawString(
                    description.Text,
                    description.Font,
                    new SolidBrush(description.Color),
                    rectDesc,
                    stringFormat);
            }

            foreach (HBarItem bar in bars)
            {
                // Draw the bar itself
                bar.Draw(gr);

                // Draw label
                if (Label.Visible)
                {
                    gr.DrawString(
                        bar.Label,
                        Label.Font,
                        new SolidBrush(Label.Color),
                        bar.LabelRect);
                }

                // Draw value or %
                if (Values.Visible)
                {
                    string strValue = string.Empty;
                    if (Values.Mode == CValueProperty.ValueMode.Digit)
                    {
                        strValue = bar.Value.ToString("F1");
                    }
                    else if (Values.Mode == CValueProperty.ValueMode.Percent)
                    {
                        if (bars.ABSTotal != 0) strValue =
                            ((double)(bar.Value / bars.ABSTotal)).ToString("P1",
                            System.Globalization.CultureInfo.CurrentCulture);
                    }
                    gr.DrawString(
                        strValue,
                        Values.Font,
                        new SolidBrush(Values.Color),
                        bar.ValueRect);
                }

            }

            // Draw chart border
            this.border.Draw(gr);

            // restore values that changed during the transition to auto scale mode
            nBarsGap = nLastBarGaps;
        }

        // Calculates bounding rectangles of bars, values, labels for 
        // positive, negative or 0 values and also chart description line
        private void CalculatePositions(Graphics gr)
        {
            int         i           = 0;
            bool        bHasNegative= (bars.Maximum < 0 || bars.Minimum < 0);
            bool        bAllNegative= (bars.Maximum < 0 && bars.Minimum < 0);
            float       fBoundTH    = 0;
            float       fBarTH      = 0;
            int         nStartX;//     = (bounds.Size.Width - bars.Count * nBarWidth - (bars.Count + 1) * nBarsGap) / 2;

            // Where all bars start
            nStartX = this.bounds.X + (this.bounds.Width - bars.Count * nBarWidth - (bars.Count + 1) * nBarsGap) / 2;

            // Calculating Desc rect
            if ( Description != null && Description.Visible && Description.Font != null && gr != null)
            {
                rectDesc.X = this.bounds.X + nBarsGap;
                rectDesc.Y = this.bounds.Bottom - 2 * nBarsGap - Description.Font.GetHeight(gr);
                rectDesc.Width = this.bounds.Size.Width - 2 * nBarsGap;
                rectDesc.Height = description.Font.GetHeight(gr) + 2 * nBarsGap;
            }
            else rectDesc = RectangleF.Empty;

            foreach (HBarItem bar in bars)
            {
                if (bHasNegative)
                {
                    // Calculating Bar.BoundRect for each bar
                    bar.BoundRect.X = nStartX + i * nBarWidth + (i + 1) * nBarsGap;
                    bar.BoundRect.Width = nBarWidth;
                    if (bAllNegative)
                    {
                        bar.BoundRect.Height = this.bounds.Height - rectDesc.Height;
                        bar.BoundRect.Y = this.bounds.Y + nBarsGap;
                    }
                    else
                    {
                        bar.BoundRect.Height = (this.bounds.Height - rectDesc.Height) / 2 + Label.Font.GetHeight(gr) + nBarsGap / 2;
                        if (bar.Value > 0)
                        {
                            bar.BoundRect.Y = this.bounds.Y + nBarsGap;
                        }
                        else
                        {
                            bar.BoundRect.Y = (this.bounds.Height - rectDesc.Height) / 2 - Label.Font.GetHeight(gr) - nBarsGap/2;
                        }
                    }

                    // Calculating Bar.LabelRect for each bar
                    bar.LabelRect.X = bar.BoundRect.X;
                    bar.LabelRect.Width = bar.BoundRect.Width + nBarsGap;
                    bar.LabelRect.Height = Label.Font.GetHeight(gr);
                    if (bAllNegative) bar.LabelRect.Y = nBarsGap;
                    else if (bar.Value >= 0) bar.LabelRect.Y = bar.BoundRect.Bottom - nBarsGap / 2 - bar.LabelRect.Height;
                    else bar.LabelRect.Y = this.bounds.Y + bar.BoundRect.Top;

                    // Calculating Bar.BarRect for each bar
                    fBoundTH = bar.BoundRect.Height - 2 * nBarsGap - bar.LabelRect.Height - values.Font.GetHeight(gr);
                    fBarTH = (float)((Math.Abs(bar.Value) * fBoundTH) / bars.ABSMaximum);
                    if (!(fBarTH >= 0)) fBarTH = 0;
                    if (bAllNegative)
                    {
                        bar.BarRect = new RectangleF(
                            bar.BoundRect.X,
                            bar.LabelRect.Bottom+nBarsGap,
                            bar.BoundRect.Width,
                            fBarTH);

                        // Calculating Bar.ValueRect for each bar
                        bar.ValueRect.X = bar.BoundRect.X;
                        bar.ValueRect.Y = bar.BarRect.Bottom + nBarsGap;
                        bar.ValueRect.Width = bar.BoundRect.Width;
                        bar.ValueRect.Height = values.Font.GetHeight(gr);
                    }
                    else
                    {
                        bar.BarRect = new RectangleF(
                            bar.BoundRect.X,
                            this.bounds.Y + (bar.Value > 0 ? (this.bounds.Height - rectDesc.Height) / 2 - fBarTH : (this.bounds.Height - rectDesc.Height) / 2),
                            bar.BoundRect.Width,
                            fBarTH);

                        // Calculating Bar.ValueRect for each bar
                        bar.ValueRect.X = bar.BoundRect.X;
                        bar.ValueRect.Y = (bar.Value > 0 ? bar.BarRect.Top - values.Font.GetHeight(gr): bar.BarRect.Bottom+nBarsGap/2);
                        bar.ValueRect.Width = bar.BoundRect.Width + nBarsGap;
                        bar.ValueRect.Height = values.Font.GetHeight(gr);
                    }
                }
                else
                {
                    // Calculating Bar.BoundRect for each bar
                    bar.BoundRect.X = nStartX + i * nBarWidth + (i + 1) * nBarsGap;
                    bar.BoundRect.Y = this.bounds.Y + nBarsGap;
                    bar.BoundRect.Width = nBarWidth;
                    bar.BoundRect.Height = this.bounds.Height - rectDesc.Height;

                    // Calculating Bar.LabelRect for each bar
                    if (Label.Visible)
                    {
                        bar.LabelRect.X = bar.BoundRect.X;
                        bar.LabelRect.Y = bounds.Bottom - rectDesc.Height - Label.Font.GetHeight(gr);
                        bar.LabelRect.Width = bar.BoundRect.Width + nBarsGap;
                        bar.LabelRect.Height = Label.Font.GetHeight(gr);
                    }
                    else bar.LabelRect = RectangleF.Empty;

                    // Calculating Bar.BoundRect for each bar
                    fBoundTH = bar.BoundRect.Height - 2*nBarsGap - bar.LabelRect.Height - (values.Visible? values.Font.GetHeight(gr):0);
                    fBarTH = (float)((Math.Abs(bar.Value) * fBoundTH) / bars.ABSMaximum);
                    if (!(fBarTH >= 0)) fBarTH = 0;
                    bar.BarRect = new RectangleF(
                        bar.BoundRect.X,
                        bar.BoundRect.Y + fBoundTH - fBarTH + (values.Visible ? values.Font.GetHeight(gr) : 0),
                        bar.BoundRect.Width,
                        fBarTH);

                    // Calculating Bar.ValueRect for each bar
                    if (Values.Visible)
                    {
                        bar.ValueRect.X = bar.BoundRect.X;
                        bar.ValueRect.Y = bar.BarRect.Top-values.Font.GetHeight(gr) -nBarsGap/2;
                        bar.ValueRect.Width = bar.BoundRect.Width + 2*nBarsGap;
                        bar.ValueRect.Height = values.Font.GetHeight(gr);
                    }
                    else bar.ValueRect = RectangleF.Empty;
                }
                 
                i++;
            }
        }

        // In Autoscale sizing mode, calculates best size for values and creates the font
        private void CreateValueFont(Graphics gr, SizeF sizeBar)
        {
            float fSize1 = 100 + (sizeBar.Width / 24);

            float sizeMax = 0;
            float sizeText;
            string strValue = string.Empty;

            for (int i = 0; i < bars.Count; i++)
            {
                if (Values.Mode == CValueProperty.ValueMode.Digit)
                {
                    strValue = String.Format("{0:F1}", bars[i].Value);
                }
                else if (Values.Mode == CValueProperty.ValueMode.Percent && bars.ABSTotal > 0)
                {
                    strValue = ((double)(bars[i].Value / bars.ABSTotal)).ToString("P1", System.Globalization.CultureInfo.CurrentCulture);
                }

                sizeText = gr.MeasureString(strValue, Values.Font).Width;
                if (sizeText > sizeMax)
                {
                    sizeMax = sizeText;
                }
            }

            sizeText = (Values.Font.Size * (sizeBar.Width / sizeMax));

            if (fSize1 <= 0 && sizeText <= 0) return;
            else if (fSize1 <= 0) Values.FontSetSize(sizeText);
            else if (sizeText <= 0) Values.FontSetSize(fSize1);
            else Values.FontSetSize((fSize1 > sizeText ? sizeText : fSize1));

        }

        // In Autoscale sizing mode, calculates best size for labels and creates the font
        private void CreateLabelFont(Graphics gr, SizeF sizeBar)
        {
            float fSize1 = 100 + (sizeBar.Width / 24);
            float sizeMax = 0;
            float sizeText;
            for (int i = 0; i < bars.Count; i++)
            {
                sizeText = gr.MeasureString(bars[i].Label, Label.Font).Width;
                if (sizeText > sizeMax)
                {
                    sizeMax = sizeText;
                }
            }

            //sizeText = sizeMax;
            float fWidthRatio = sizeBar.Width / sizeMax;
            sizeText = (Label.Font.Size * fWidthRatio);

            if (fSize1 <= 0 &&  sizeText<= 0) return;
            else if (fSize1 <= 0) Label.FontSetSize(sizeText);
            else if (sizeText <= 0) Label.FontSetSize(fSize1);
            else Label.FontSetSize((fSize1 > sizeText ? sizeText : fSize1));

        }

        // In Autoscale sizing mode, calculates best size for description font and creates it. used in auto scaling mode
        private void CreateDescFont(Graphics gr, SizeF sizeBound)
        {
            float fSize1 = sizeBound.Height / 15;

            float fWidthRatio = (sizeBound.Width - 2 * nBarsGap) / gr.MeasureString(description.Text, description.Font).Width;
            float fSize2 = (description.Font.Size * fWidthRatio);

            if (fSize1 <= 0 && fSize2 <= 0) return;
            else if (fSize1 <= 0) description.FontSetSize( fSize2 );
            else if (fSize2 <= 0) description.FontSetSize( fSize1 );
            else description.FontSetSize( (fSize1>fSize2?fSize2:fSize1) );
        }

        // Draw a bar with label & value 
        private void DrawBar(Graphics gr, HBarItem bar)
        {
            // Draw the bar itself
            bar.Draw(gr);

            // Draw label
            if (Label.Visible)
            {
                float nLabelHeight = Label.Font.GetHeight(gr);
                gr.DrawString(
                    bar.Label,
                    Label.Font,
                    new SolidBrush(Label.Color),
                    new RectangleF(
                        bar.BarRect.X,
                        bar.BarRect.Bottom + nBarsGap,
                        bar.BarRect.Width,
                        nLabelHeight));
            }

            // Draw value or %
            if (Values.Visible)
            {
                string strValue = string.Empty;
                if (Values.Mode == CValueProperty.ValueMode.Digit)
                {
                    strValue = bar.Value.ToString("F1");
                }
                else if (Values.Mode == CValueProperty.ValueMode.Percent)
                {
                    if (bars.Total > 0) strValue =
                        ((double)(bar.Value / bars.Total)).ToString("P1", 
                        System.Globalization.CultureInfo.CurrentCulture);
                }

                float fValueHeight = Values.Font.GetHeight(gr);
                gr.DrawString(
                    strValue,
                    Values.Font,
                    new SolidBrush(Values.Color),
                    new RectangleF(
                        bar.BarRect.X,
                        bar.BarRect.Top - fValueHeight - 1,
                        bar.BarRect.Width + 2 * nBarsGap,
                        fValueHeight));
            }
        }
        
        // Prevent the control to draw any backgrounds
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Do nothing
        }

        #endregion  // Drawings

        #region MouseEvents

        // Resets tooltip text and display position
        private void SetCurrTooltip(HBarItem bar)
        {
            if (bar == null)
            {
                tooltip.Hide(this);
                tooltip.RemoveAll();

            }
            else
            {
                //tooltip.Active = true;
                string strCaption = string.Empty;
                string strPercent = string.Empty;

                //if (bars.Total > 0)
                //{
                //    strPercent = ((double)(bar.Value / bars.Total)).
                //        ToString("P2", System.Globalization.CultureInfo.CurrentCulture);
                //}

                //strCaption = String.Format("{0}\r\n{1}", bar.Value, strPercent);
                strCaption = String.Format("{0}dBm", bar.Value - bar.Offset);

                // This seems not to be working in Vista. I hope it will after that
                if (System.Environment.OSVersion.Version.Major!=6)
                {
                    tooltip.Hide(this);
                    tooltip.RemoveAll();
                }
                tooltip.Dispose();
                tooltip = new ToolTip();
                tooltip.IsBalloon = true;
                //tooltip.ShowAlways = true;
                tooltip.InitialDelay = 100;
                tooltip.ReshowDelay = 100;
                //tooltip.AutoPopDelay = Int32.MaxValue;
                tooltip.ToolTipTitle = bar.Label;
                tooltip.SetToolTip(this, strCaption.ToString());
                //tooltip.Show(strCaption.ToString(), this);
            }
        }

        // Is Mouse pointer inside a bar rectangle?
        private HBarItem HitTest(Point MousePoint, out int nIndex)
        {
            //HBarData bar;
            nIndex = -1;
            for (int i = 0; i < bars.Count; i++)
            {
                //if (bars.GetAt(i, out bar))
                //{
                    if (bars[i].BoundRect.Contains(MousePoint))
                    {
                        nIndex = i;
                        return bars[i];
                    }
                //}
            }

            return null;
        }

        // Why this function is called when mouse is not moving but is just over control?
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            // If mouse is actually moving!
            if (ptLastTooltipMouseLoction != e.Location)
            {
                ptLastTooltipMouseLoction = e.Location;

                int nIndex;
                HBarItem bar = HitTest(e.Location, out nIndex);

                if (bar != null)
                {
                    // So mouse is inside a bar
                    #region HoverInEvent

                    if (nLastVisitedBarIndex >= 0)
                    {
                        if (nIndex != nLastVisitedBarIndex)
                        {
                            // If moved into a new bar, from another bar
                            // How odd, it didn't notice it's over empty space, I miss SetCapture.
                            //OnBarLeave();
                            OnBarEnter(bar, nIndex);
                        }
                        else
                        {
                            // Moving along a bar
                            // Logically should be enabled, but will cause a bug in vista so no tooltip will be visible any longer
                            SetCurrTooltip(null);
                            //SetCurrTooltip(bar);
                        }
                    }
                    else
                    {
                        // If moved into a bar from empty space
                        OnBarEnter(bar, nIndex);
                    }
                    #endregion
                    
                    SetCurrTooltip(bar);
                }
                else
                {
                    // Mouse moving in empty space
                    OnBarLeave();
                }
            }
            else
            {
                // Funny! Mouse is not moving. It's just placed over this control.
            }
        }

        // Mouse entered a bar
        private void OnBarEnter(HBarItem bar, int nIndex)
        {
            nLastVisitedBarIndex = nIndex;
            RaiseHoverInEvent(bar, nIndex);

            SetCurrTooltip(bar);
        }

        // Mouse left a bar
        private void OnBarLeave()
        {
            if (nLastVisitedBarIndex >= 0)
            {
                //HBarItem barEvent;
                //if (bars.GetAt(nLastVisitedBarIndex, out barEvent))
                //{
                    SetCurrTooltip(null);
                    RaiseHoverOutEvent(bars[nLastVisitedBarIndex], nLastVisitedBarIndex);
                    nLastVisitedBarIndex = -1;

                //}
            }
        }

        // Mouse leaving control VISIBLE AREA
        private void OnMouseLeave(object sender, EventArgs e)
        {
            // Calculate 
            Rectangle rectControlWnd = RectangleToScreen(this.ClientRectangle); 
            /*new Rectangle();
            rectControlWnd.Location = PointToScreen(Location);
            rectControlWnd.Width = this.ClientSize.Width;
            rectControlWnd.Height = this.ClientSize.Height;*/

            if (!rectControlWnd.Contains(Cursor.Position))
            {
                SetCurrTooltip(null);
                OnBarLeave();
            }
        }

        // Mouse clicked on control
        private void OnClick(object sender, MouseEventArgs e)
        {
            int nIndex;

            HBarItem bar = HitTest(e.Location, out nIndex);
            if (bar != null)
            {
                RaiseClickEvent(bar, nIndex);
            }
        }

        // Mouse double click on control
        private void OnDoubleClick(object sender, MouseEventArgs e)
        {
            int nIndex;

            HBarItem bar = HitTest(e.Location, out nIndex);
            if (bar != null)
            {
                RaiseDoubleClickEvent(bar, nIndex);
            }

        }

        #endregion // MouseEvents

        #region overrides and events

        // Update outer shadow if exists
        private void HBarChart_BackColorChanged(object sender, EventArgs e)
        {
            if (this.shadow != null &&
                (this.shadow.Mode == CShadowProperty.Modes.Both ||
                this.shadow.Mode == CShadowProperty.Modes.Outer))
            {
                RedrawChart();
            }
        }
        
        #endregion

    }

    #region Items

    // Each item will present a bar of the bar chart
    public class HBarItem : ICloneable
    {
        #region Fields

        // A refrence to parent, in case of any changes that parent should know
        private HBarItems parent;

        // Actual bar rect inside the bounding rectangle
        protected RectangleF rectBar;

        // Bounding rectangle of the bar (Bar.Left, Chart.Top, Bar.Width, Chart.Height)
        public RectangleF BoundRect;
        //protected RectangleF rectBound;

        // Rectangle inside which value of this bar will be drawn
        public RectangleF ValueRect;
        
        // Rectangle inside which Label of this bar will be drawn
        public RectangleF LabelRect;
        
        // Main color of the bar. Might be used to create a gradient
        protected Color colorBar;

        // Label under the bar
        protected string strLabel;

        // A double value of the bar positive or negative
        protected double dValue;

        // A double value of the bar positive or negative
        protected double dOffset;

        bool bShowBorder;

        // A set of GDI objects to draw the bar
        private Color ColorBacklightEnd;
        private Color ColorGlowStart;
        private Color ColorGlowEnd;
        private Color ColorFillBK;
        private Color ColorBorder;

        SolidBrush brushFill;

        private RectangleF rectGradient;
        private GraphicsPath pathGradient;
        private PathGradientBrush brushGradient;
        private Color[] ColorGradientSurround;

        private LinearGradientBrush brushGlow;
        private RectangleF rectGlow;
        private PointF gradientCenterPoint;

        #endregion

        #region Properties

        public HBarItems Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public RectangleF BarRect
        {
            get { return rectBar; }
            set 
            { 
                rectBar = value;
                CreateGlowBrush();
            }
        }

        public Color Color
        {
            get { return colorBar; }
            set 
            { 
                colorBar = value;

                ColorFillBK = GetDarkerColor(Color, 85);
                ColorBorder = GetDarkerColor(Color, 100);

                if (brushFill != null)
                {
                    brushFill.Dispose();
                    brushFill = null;
                }
                brushFill = new SolidBrush(ColorFillBK);
            }
        }
        
        [Localizable(true)]
        public string Label
        {
            get { return strLabel; }
            set { strLabel = value; }
        }
        
        public double Value
        {
            get { return dValue; }
            set 
            {
                dValue = value; 
                if (Parent != null) Parent.ShouldReCalculate = true;
            }
        }

        public double Offset
        {
            get { return dOffset; }
            set
            {
                dOffset = value;
                if (Parent != null) Parent.ShouldReCalculate = true;
            }
        }

        public bool ShowBorder
        {
            get { return bShowBorder; }
            set { bShowBorder = value; }
        }
        
        #endregion

        #region Constructors

        // Constructors
        public HBarItem(double dValue, string strLabel, Color colorBar, RectangleF rectfBar, RectangleF rectfBound, HBarItems Parent)
            : this(dValue, strLabel, colorBar, rectfBar, rectfBound)
        {
            this.Parent = Parent;
        }
      
        public HBarItem(double dValue, string strLabel, Color colorBar, RectangleF rectfBar, RectangleF rectfBound)
            : this(dValue, strLabel, colorBar, rectfBar)
        {
            BarRect = rectfBound;
        }

        public HBarItem(double dValue, string strLabel, Color colorBar, RectangleF barRect)
            : this(dValue, strLabel, colorBar)
        {
            rectBar = barRect;
        }
        public HBarItem(double dValue,double dOffset, string strLabel, Color colorBar)
            : this(dValue, strLabel, colorBar)
        {
            Offset = dOffset;
        }

        public HBarItem(double dValue, string strLabel, Color colorBar)
            : this()
        {
            Value = dValue;
            Label = strLabel;
            Color = colorBar;
        }

        public HBarItem()
        {
            colorBar = Color.Empty;

            Value = 0;

            Label = string.Empty;

            Parent = null;

            ColorBacklightEnd = Color.FromArgb(80, 0, 0, 0);
            ColorGradientSurround = new Color[] { ColorBacklightEnd };

            ShowBorder = true;

            BarRect = RectangleF.Empty;
            BoundRect = new RectangleF(0, 0, 0, 0);
        }

        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return new HBarItem(Value, Label, Color, BarRect, BoundRect);
        }

        public object Clone()
        {
            return new HBarItem(Value, Label, Color, BarRect, BoundRect);
        }

        #endregion

        #region Methods

        // In case chart uses a theme that needs a gradient
        private void CreateGradientBrush()
        {
            // Reset all objects
            if (pathGradient == null)
            {
                pathGradient = new GraphicsPath();
                //pathGradient.Dispose();
                //pathGradient = null;
            }
            if (brushGradient != null)
            {
                brushGradient.Dispose();
                brushGradient = null;
            }

            // Create or reset objects
            this.rectGradient.X = rectBar.Left - rectBar.Width / 8;
            this.rectGradient.Y = rectBar.Top - rectBar.Height / 2;
            this.rectGradient.Width = rectBar.Width * 2;
            this.rectGradient.Height = rectBar.Height * 2;

            this.gradientCenterPoint.X = rectBar.Right;
            this.gradientCenterPoint.Y = rectBar.Top + rectBar.Height / 2;

            pathGradient.Reset();
            pathGradient.AddEllipse( rectGradient );

            brushGradient = new PathGradientBrush(pathGradient);
            brushGradient.CenterPoint = this.gradientCenterPoint;
            brushGradient.CenterColor = Color;
            brushGradient.SurroundColors = ColorGradientSurround;
        }

        // In case chart uses Glass theme
        void CreateGlowBrush()
        {
            if (rectBar.Height <= 0) rectBar.Height = 1;

            // Caculate Glow density
            int nAlphaStart = (int)(185 + 5 * BarRect.Width / 24),
                nAlphaEnd = (int)(10 + 4 * BarRect.Width / 24);

            if (nAlphaStart > 255) nAlphaStart = 255;
            else if (nAlphaStart < 0) nAlphaStart = 0;

            if (nAlphaEnd > 255) nAlphaEnd = 255;
            else if (nAlphaEnd < 0) nAlphaEnd = 0;
            
            ColorGlowStart = Color.FromArgb(nAlphaEnd, 255, 255, 255);
            ColorGlowEnd = Color.FromArgb(nAlphaStart, 255, 255, 255);

            if (brushGlow != null)
            {
                brushGlow.Dispose();
                brushGlow = null;
            }

            rectGlow = new RectangleF(rectBar.Left, rectBar.Top, rectBar.Width / 2, rectBar.Height);
            brushGlow = new LinearGradientBrush(
                new PointF(rectGlow.Right + 1, rectGlow.Top), 
                new PointF(rectGlow.Left - 1, rectGlow.Top),
                ColorGlowStart, ColorGlowEnd);
        }

        // Draws a bar item. This function does not draw label or value of a bar
        public void Draw(Graphics gr)
        {
            if (BarRect.Width <= 0 || BarRect.Height <= 0) return;
           
            // Draw fill color
            if (parent.DrawingMode == HBarItems.DrawingModes.Solid)
            {
                gr.FillRectangle(new SolidBrush(Color), BarRect);
            }
            else
            {
                gr.FillRectangle(brushFill, BarRect);
            }

            // Draw gradients
            if (parent.DrawingMode == HBarItems.DrawingModes.Glass ||
                parent.DrawingMode == HBarItems.DrawingModes.Rubber)
            {
                CreateGradientBrush();
                gr.FillRectangle(brushGradient, BarRect);
            }

            if (parent.DrawingMode == HBarItems.DrawingModes.Glass)
            {
                gr.FillRectangle(brushGlow, rectGlow);
            }
            
            // Draw border
            if (ShowBorder)
            {
                gr.DrawRectangle(
                    new Pen(ColorBorder, 1),
                    rectBar.Left, rectBar.Top, rectBar.Width, rectBar.Height);
            }
        }

        // Decrease all RGB values as much as 'intensity' says
        private Color GetDarkerColor(Color color, byte intensity)
        {
            int r, g, b;

            r = color.R - intensity;
            g = color.G - intensity;
            b = color.B - intensity;

            if (r > 255 || r < 0) r *= -1;
            if (g > 255 || g < 0) g *= -1;
            if (b > 255 || b < 0) b *= -1;

            return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }

        #endregion
    }

    // This collection holds all bars. To be a datasource it implements 
    // IList interface.
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class HBarItems : IList<HBarItem>
    {
        #region Fields

        // Do we need to calculate max, min and total again?
        private bool bShouldReCalculate;

        // A list of all bars
        private List<HBarItem> items;

        // Minimum, maximum & total value of all values
        private double dMaximumValue;
        private double dMinimumValue;
        private double dTotal;


        // Absolute(Ignoring positive or negative sign) Minimum, maximum & total value of all values
        private double dABSMaximumValue;
        private double dABSMinimumValue;
        private double dABSTotalValue;

        private DrawingModes drawingMode;
        private int nBarWidth;

        #endregion

        #region Properties

        //[Browsable(false)]
        public bool ShouldReCalculate
        {
            get { return bShouldReCalculate; }
            set { bShouldReCalculate = value; }
        }

        public double Maximum
        {
            get 
            { 
                if (ShouldReCalculate) ReCalculateAll();
                return dMaximumValue; 
            }
            set
            {
                dMaximumValue = value;
                dABSMaximumValue = Math.Abs(value);
            }
        }

        public double Minimum
        {
            get 
            {
                if (ShouldReCalculate) ReCalculateAll();
                return dMinimumValue;
            }
            set
            {
                dMinimumValue = value;
                dABSMinimumValue = Math.Abs(value);
            }
        }

        public double Total
        {
            get 
            {
                if (ShouldReCalculate) ReCalculateAll();
                return dTotal;
            }
        }

        public double ABSMaximum
        {
            get 
            { 
                if (ShouldReCalculate) ReCalculateAll();
                return dABSMaximumValue; 
            }
        }

        public double ABSMinimum
        {
            get 
            {
                if (ShouldReCalculate) ReCalculateAll();
                return dABSMinimumValue;
            }
        }

        public double ABSTotal
        {
            get 
            {
                if (ShouldReCalculate) ReCalculateAll();
                return dABSTotalValue;
            }
        }

        public enum DrawingModes
        {
            Glass,         // A gradient + a glow
            Rubber,        // A gradient
            Solid          // A solid background
        }
        [Browsable(true)]
        [Category("Bar Chart")]
        public DrawingModes DrawingMode
        {
            get { return drawingMode; }
            set { drawingMode = value; }
        }


        [Browsable(true)]
        [Category("Bar Chart")]
        public int DefaultWidth
        {
            get { return nBarWidth; }

            set
            {
                nBarWidth = value;
            }
        }
        #endregion

        #region Methods
        
        private void ReCalculateAll()
        {
            if (items.Count <= 0)
            {
                dMaximumValue = dMinimumValue = dTotal = 0;
                dABSMaximumValue = dABSMinimumValue = dABSTotalValue = 0;
            }
            else
            {
                dTotal = dABSTotalValue = 0;

                dMaximumValue = dMinimumValue = items[0].Value;
                dABSMaximumValue = dABSMinimumValue = Math.Abs(items[0].Value);

                foreach (HBarItem item in items)
                {
                    dTotal += item.Value;
                    dABSTotalValue += Math.Abs(item.Value);

                    if (item.Value > dMaximumValue) dMaximumValue = item.Value;
                    else if (item.Value < dMinimumValue) dMinimumValue = item.Value;

                    if (Math.Abs(item.Value) > dABSMaximumValue) dABSMaximumValue = Math.Abs(item.Value);
                    else if (Math.Abs(item.Value) < dABSMinimumValue) dABSMinimumValue = Math.Abs(item.Value);
                }
            }

            ShouldReCalculate = false;
        }
        
        #endregion

        #region Constructors
        // Constructor
        public HBarItems()
        {
            items = new List<HBarItem>();

            dTotal = dMaximumValue = dMinimumValue = 0;

            DrawingMode = DrawingModes.Glass;
        }
        #endregion


        #region IList<HBarItem> Members

        public int IndexOf(HBarItem item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, HBarItem item)
        {
            item.Parent = this;
            items.Insert(index, item);
            ShouldReCalculate = true;
        }

        public void RemoveAt(int index)
        {
            items[index].Parent = null;
            items.RemoveAt(index);
            ShouldReCalculate = true;
        }

        public HBarItem this[int index]
        {
            get
            {
                return items[index];
            }
            set
            {
                items[index].Parent = null;
                items[index] = value;
                items[index].Parent = this;
                ShouldReCalculate = true;
            }
        }

        #endregion

        #region ICollection<HBarItem> Members

        public void Add(HBarItem item)
        {
            items.Add(item);
            item.Parent = this;
            ShouldReCalculate = true;
        }

        public void Clear()
        {
            foreach (HBarItem item in items)
                item.Parent = null;

            items.Clear();
            ShouldReCalculate = true;
        }

        public bool Contains(HBarItem item)
        {
            return items.Contains(item);
        }

        public void CopyTo(HBarItem[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(HBarItem item)
        {
            item.Parent = null;
            bool bRet = items.Remove(item);
            ShouldReCalculate = true;
            return bRet;
        }

        #endregion

        #region IEnumerable<HBarItem> Members

        public IEnumerator<HBarItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion

    }

    #endregion

    #region GUIElements

    // A set of classes for storing GUI elements and properties in BarChart class
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class CDescriptionProperty
    {
        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        private bool bVisible;
        public bool Visible
        {
            get { return bVisible; }
            set { bVisible = value; }
        }

        private Font font;
        public Font Font
        {
            get { return font; }
            set 
            {
                FontSet(value);
                if (font != null) this.fFontDefaultSize = font.Size;
            }
        }

        private Color color;
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public CDescriptionProperty()
        {
            this.fFontDefaultSize = 14;
            Font = new Font("Tahoma", this.fFontDefaultSize, FontStyle.Bold);
            Color = Color.FromArgb(255, 255, 255, 255);
            Text = string.Empty;
            Visible = true;
        }

        private float fFontDefaultSize;

        [Browsable(false)]
        public float FontDefaultSize
        {
            get { return this.fFontDefaultSize; }
            set { this.fFontDefaultSize = value; }
        }
        
        // Updates current font, but does not change default size
        public void FontSet(Font font)
        {
            if (this.font != null)
            {
                this.font.Dispose();
                this.font = null;
            }

            this.font = new Font(font, font.Style);
        }

        public void FontSetSize(float fNewSize)
        {
            if (this.font == null)
            {
                this.font = new Font("Tahoma", fNewSize, FontStyle.Bold);

            }
            else
            {
                if (this.font.Size == fNewSize) return;

                Font old = this.font;
                this.font = new Font(old.FontFamily, fNewSize, old.Style);

                old.Dispose();
                old = null;
            }
        }

        public void FontReset()
        {
            string strFace = "Tahoma";
            FontStyle style = FontStyle.Bold;

            if (font != null)
            {
                if (this.font.Size == this.fFontDefaultSize) return;
                
                if (font.Name != strFace) strFace = font.Name;
                if (font.Style != style) style = font.Style;
                
                this.font.Dispose();
                this.font = null;

            }
            if (this.FontDefaultSize <= 0)
            {
                this.font = null;
                return;
            }

            this.font = new Font(strFace, this.FontDefaultSize, style);
        }
    }

    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class CLabelProperty
    {
        private bool bVisible;
        public bool Visible
        {
            get { return bVisible; }
            set { bVisible = value; }
        }

        private Font font;
        public Font Font
        {
            get { return font; }
            set 
            {
                FontSet(value);
                if (font != null) this.fFontDefaultSize = font.Size;
            }
        }

        private Color color;
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public CLabelProperty()
        {
            FontDefaultSize = 8;
            Font = new Font("Tahoma", FontDefaultSize, FontStyle.Bold);

            Color = Color.FromArgb(255, 255, 255, 255);
            Visible = true;
        }
        private float fFontDefaultSize;

        [Browsable(false)]
        public float FontDefaultSize
        {
            get { return this.fFontDefaultSize; }
            set { this.fFontDefaultSize = value; }
        }

        // Updates current font, but does not change default size
        public void FontSet(Font font)
        {
            if (this.font != null)
            {
                this.font.Dispose();
                this.font = null;
            }

            this.font = new Font(font, font.Style);
        }

        public void FontSetSize(float fNewSize)
        {
            if (this.font == null)
            {
                this.font = new Font("Tahoma", fNewSize, FontStyle.Bold);

            }
            else
            {
                if (this.font.Size == fNewSize) return;

                Font old = this.font;
                this.font = new Font(old.FontFamily, fNewSize, old.Style);

                old.Dispose();
                old = null;
            }
        }

        public void FontReset()
        {
            string strFace = "Tahoma";
            FontStyle style = FontStyle.Bold;

            if (font != null)
            {
                if (this.font.Size == this.fFontDefaultSize) return;

                if (font.Name != strFace) strFace = font.Name;
                if (font.Style != style) style = font.Style;

                this.font.Dispose();
                this.font = null;

            }
            if (this.FontDefaultSize <= 0)
            {
                this.font = null;
                return;
            }

            this.font = new Font(strFace, this.FontDefaultSize, style);
        }
    }

    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class CValueProperty
    {
        private float fFontDefaultSize;
        
        [Browsable(false)]
        public float FontDefaultSize
        {
            get { return this.fFontDefaultSize; }
            set { this.fFontDefaultSize = value; }
        }

        private bool bVisible;
        public bool Visible
        {
            get { return bVisible; }
            set { bVisible = value; }
        }

        private Font font;
        public Font Font
        {
            get { return this.font; }
            set 
            { 
                FontSet(value);
                if (font != null) this.fFontDefaultSize = font.Size;
            }
        }

        private Color color;
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public enum ValueMode
        {
            Digit,      // Display the value of each bar at the top of it
            Percent     // Display a percentage depending on the other values
        }
        private ValueMode mode;
        public ValueMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public CValueProperty()
        {
            Mode = ValueMode.Digit;
            FontDefaultSize = 7;
            Font = new Font("Tahoma", FontDefaultSize);
            Color = Color.FromArgb(255, 255, 255, 255);
            Visible = true;
        }


        // Updates current font, but does not change default size
        public void FontSet(Font font)
        {
            if (this.font != null)
            {
                this.font.Dispose();
                this.font = null;
            }

            this.font = new Font(font, font.Style);
        }

        public void FontSetSize(float fNewSize)
        {
            if (this.font == null)
            {
                this.font = new Font("Tahoma", fNewSize, FontStyle.Bold);

            }
            else
            {
                if (this.font.Size == fNewSize) return;

                Font old = this.font;
                this.font = new Font(old.FontFamily, fNewSize, old.Style);

                old.Dispose();
                old = null;
            }
        }

        public void FontReset()
        {
            string strFace = "Tahoma";
            FontStyle style = FontStyle.Bold;

            if (font != null)
            {
                if (this.font.Size == this.fFontDefaultSize) return;

                if (font.Name != strFace) strFace = font.Name;
                if (font.Style != style) style = font.Style;

                this.font.Dispose();
                this.font = null;

            }
            if (this.FontDefaultSize <= 0)
            {
                this.font = null;
                return;
            }

            this.font = new Font(strFace, this.FontDefaultSize, style);
        }
    }

    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class CBackgroundProperty
    {
        #region Fields

        // How to paint background
        public enum PaintingModes
        {
            SolidColor,
            LinearGradient,
            RadialGradient
        }

        // Firs Gradient color( at the moment top)
        private Color gradientColor1;

        // Second gradient color (at the moment, bottom)
        private Color gradientColor2;

        // Color for solid background
        private Color solidColor;

        // Defines painting mode of this background object
        private PaintingModes paintingMode;

        // Background brush
        private Brush brush;

        private RectangleF rectBound;

        [Browsable(false)]
        private RectangleF rectGradient;
        
        [Browsable(false)]
        GraphicsPath pathGradient;

        [Browsable(false)]
        PointF radialCenterPoint;
        #endregion

        #region Properties

        [Browsable(true)]
        public PaintingModes PaintingMode
        {
            get { return paintingMode; }
            set 
            {
                if (value != paintingMode)
                {
                    paintingMode = value;
                    ResetBrush();
                }
            }
        }
        
        [Browsable(true)]
        public Color SolidColor
        {
            get { return solidColor; }
            set 
            { 
                solidColor = value;
                ResetBrush();
            }
        }
        
        [Browsable(true)]
        public Color GradientColor2
        {
            get { return gradientColor2; }
            set 
            { 
                gradientColor2 = value;
                ResetBrush();
            }
        }
       
        [Browsable(true)]
        public Color GradientColor1
        {
            get { return gradientColor1; }
            set 
            { 
                gradientColor1 = value;
                ResetBrush();
            }
        }
       
        [Browsable(false)]
        public RectangleF BoundRect
        {
            get { return rectBound; }
        }

        #endregion

        public void SetBoundRect(RectangleF boundRect)
        {
            this.rectBound.X = boundRect.X;
            this.rectBound.Y = boundRect.Y;
            this.rectBound.Width = boundRect.Width;
            this.rectBound.Height = boundRect.Height;
        }

        // Constructor
        public CBackgroundProperty()
        {
            brush = null;

            paintingMode = PaintingModes.RadialGradient;
            gradientColor1 = Color.FromArgb(255, 140, 210, 245);
            gradientColor2 = Color.FromArgb(255, 0, 30, 90);
            solidColor = gradientColor2;

            rectGradient = RectangleF.Empty;
            this.pathGradient = new GraphicsPath();
        }

        #region Methods

        // Recreates BK brush
        private void ResetBrush()
        {
            // Delete last brush
            if (brush != null)
            {
                brush.Dispose();
                brush = null;
            }

            // Create backgroud brush
            if (PaintingMode == PaintingModes.LinearGradient)
            {
                if (BoundRect.Height <= 0) return;

                brush = new LinearGradientBrush(
                    new Point((int)BoundRect.X, (int)BoundRect.Y),
                    new Point((int)BoundRect.X, (int)BoundRect.Bottom),
                    GradientColor1, GradientColor2);
            }
            else if (PaintingMode == PaintingModes.RadialGradient)
            {
                CreateGradientBrush();
            }
            else
            {
                brush = new SolidBrush(SolidColor);
            }
        }

        private void CreateGradientBrush()
        {
            if (this.rectBound == null || this.rectBound.Width<1 || this.rectBound.Height<1) return;
            
            PathGradientBrush brushGradient;

            this.rectGradient.X = this.rectBound.Left - this.rectBound.Width / 2;
            this.rectGradient.Y = this.rectBound.Top - this.rectBound.Height / 3;
            this.rectGradient.Width = this.rectBound.Width * 2;
            this.rectGradient.Height = this.rectBound.Height + this.rectBound.Height / 2;

            this.radialCenterPoint.X = this.rectBound.Left + this.rectBound.Width / 2;
            this.radialCenterPoint.Y = this.rectBound.Top + this.rectBound.Height / 2;

            pathGradient.Reset();
            pathGradient.AddEllipse(rectGradient);

            brushGradient = new PathGradientBrush(pathGradient);
            brushGradient.CenterPoint = this.radialCenterPoint;
            brushGradient.CenterColor = this.gradientColor1;
            brushGradient.SurroundColors = new Color[] { this.gradientColor2 };

            this.brush = brushGradient;
            brushGradient = null;

        }

        // Draws background inside visible rectangle of the given graphics
        public void Draw(Graphics gr, RectangleF rectBound)
        {
            this.SetBoundRect(rectBound);
            
            ResetBrush();
            if (brush == null) return;

            gr.FillRectangle(brush, rectBound);
        }

        #endregion
    }

    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class CBorderProperty
    {
        #region Fields

        private int nSize;

        private Color color;

        private bool bVisible;

        private Pen pen;

        private RectangleF rectBound;

        #endregion

        #region Properties

        [Browsable(true)]
        public bool Visible
        {
            get { return this.bVisible; }
            set { this.bVisible = value; }
        }
        
        [Browsable(true)]
        public Color Color
        {
            get { return color; }
            set 
            { 
                color = value;
                ResetPen();
            }
        }

        [Browsable(true)]
        public int Width
        {
            get { return nSize; }
            set 
            {
                nSize = value;
                ResetPen();
            }
        }
       
        [Browsable(false)]
        public RectangleF BoundRect
        {
            get { return rectBound; }
            set 
            { 
                rectBound = value;
            }
        }

        #endregion

        // Constructor
        public CBorderProperty()
        {
            this.pen = null;
            this.BoundRect = new RectangleF(0, 0, 0, 0);
            this.Visible = true;
            this.Color = Color.White;
            this.Width = 1;
        }

        #region Methods

        // Recreates BK brush
        private void ResetPen()
        {
            // Delete last brush
            if (this.pen != null)
            {
                this.pen.Dispose();
                this.pen = null;
            }

            if (this.nSize <= 0) return;

            this.pen = new Pen( this.color, this.nSize );
        }

        // Draws background inside visible rectangle of the given graphics
        public void Draw(Graphics gr)
        {
            if (this.rectBound == null || this.rectBound == RectangleF.Empty ) this.rectBound = gr.VisibleClipBounds;

            if (!this.bVisible) return;

            if (this.pen == null) ResetPen();
            if (this.pen == null) return;

            gr.DrawRectangle(this.pen, this.rectBound.X+this.Width/2, this.rectBound.Y+this.Width/2, this.rectBound.Width-this.Width, this.rectBound.Height-this.Width);
        }

        #endregion

        internal void SetRect(Rectangle rect)
        {
            this.rectBound.X = rect.X;
            this.rectBound.Y = rect.Y;
            this.rectBound.Width = rect.Width;
            this.rectBound.Height = rect.Height;
        }
    }

    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class CShadowProperty
    {
        // Drawing modes of the shadow
        public enum Modes{None, Inner, Outer, Both}
       
        #region Fields

        private int nSizeOuter;
        private int nSizeInner;

        private Color colorOuter;
        private Color colorInner;

        private Pen pen;
        private Pen penBack;

        private Modes mode;

        private RectangleF rectOuter;
        private RectangleF rectInner;

        #endregion

        #region Properties

        
        [Browsable(true)]
        public Color ColorOuter
        {
            get { return colorOuter; }
            set { colorOuter = value;}
        }

        [Browsable(true)]
        public Color ColorInner
        {
            get { return colorInner; }
            set { colorInner = value; }
        }

        [Browsable(true)]
        public int WidthOuter
        {
            get { return nSizeOuter; }
            set { nSizeOuter = value; }
        }
       
        [Browsable(true)]
        public int WidthInner
        {
            get { return nSizeInner; }
            set { nSizeInner = value; }
        }
       
        [Browsable(true)]
        public Modes Mode
        {
            get { return mode; }
            set { mode = value; }
        }
       
        #endregion

        // Constructor
        public CShadowProperty()
        {
            this.pen = null;
            
            this.colorInner = Color.FromArgb(100, 0, 0, 0);
            this.colorOuter = Color.FromArgb(100, 0, 0, 0);
            
            this.nSizeInner = 5;
            this.nSizeOuter = 5;
            
            this.rectInner = RectangleF.Empty;
            this.rectOuter = RectangleF.Empty;

            this.mode = Modes.Inner;
        }

        #region Methods
 
        public void SetRect(RectangleF rect, int nIndex/* 0 = inner, 1 = outer*/)
        {
            this.SetRect(
            rect.X,
            rect.Y,
            rect.Width,
            rect.Height,
            nIndex);
        }

        public void SetRect(float x, float y, float width, float height, int nIndex/* 0 = inner, 1 = outer*/)
        {
            if (nIndex == 0)
            {
                this.rectInner.X = x;
                this.rectInner.Y = y;
                this.rectInner.Width = width;
                this.rectInner.Height = height;
            }
            else
            {
                this.rectOuter.X = x;
                this.rectOuter.Y = y;
                this.rectOuter.Width = width;
                this.rectOuter.Height = height;
            }
        }

        public void Draw(Graphics gr, Color colorBK)
        {
            if (this.mode == Modes.None) return;
            else
            {
                if (this.mode == Modes.Outer || this.mode == Modes.Both)
                {
                    if (this.nSizeOuter <= 0 || this.nSizeOuter > this.colorOuter.A) return;
                    DrawOuterShadow(gr, colorBK);
                }
                if (this.mode == Modes.Inner || this.mode == Modes.Both)
                {
                    if (this.nSizeInner <= 0 || this.nSizeInner > this.colorInner.A) return;
                    DrawInnerShadow(gr);
                }
            }
        }

        private void DrawInnerShadow(Graphics gr)
        {
            if (this.rectInner == null || this.rectInner == Rectangle.Empty) return;
            if (this.pen == null) this.pen = new Pen(colorInner);
            if (this.pen.Color != this.colorInner) this.pen.Color = colorInner;

            Rectangle rect = new Rectangle((int)(this.rectInner.X + this.pen.Width / 2), (int)(this.rectInner.Y + this.pen.Width / 2), (int)(this.rectInner.Width - this.pen.Width), (int)(this.rectInner.Height - this.pen.Width));

            int nStep = this.colorInner.A / this.nSizeInner;
            if (nStep <= 0) nStep = 1;
            for (int i = this.colorInner.A; i > 0; i -= nStep)
            {
                this.pen.Color = Color.FromArgb(i/*alpha*/, this.pen.Color);
                gr.DrawRectangle(this.pen, rect);

                rect.Inflate(-1, -1);
            }
        }

        private void DrawOuterShadow(Graphics gr, Color colorBK)
        {
            if (this.rectOuter == null || this.rectOuter == Rectangle.Empty) return;
            //if (this.pen == null) this.pen = new Pen(colorInner);

            // clear background
            if (this.penBack == null || this.penBack.Width!=this.nSizeOuter || this.penBack.Color!=colorBK) this.penBack = new Pen(colorBK, this.nSizeOuter);
            gr.DrawRectangle(penBack, new Rectangle((int)(this.rectOuter.X + this.penBack.Width / 2), (int)(this.rectOuter.Y + this.penBack.Width / 2), (int)(this.rectOuter.Width - this.penBack.Width), (int)(this.rectOuter.Height - this.penBack.Width)));
            
            // draw shadow
            if (this.pen == null) this.pen = new Pen(colorOuter, 1);
            if (this.pen.Color != this.colorOuter) this.pen.Color = colorOuter;
            Rectangle rect = new Rectangle((int)(this.rectOuter.X + this.pen.Width / 2), (int)(this.rectOuter.Y + this.pen.Width / 2), (int)(this.rectOuter.Width - this.pen.Width), (int)(this.rectOuter.Height - this.pen.Width));

            int nStep = this.colorOuter.A / this.nSizeOuter;
            if (nStep <= 0) nStep = 1;
            for (int i = 0; i < this.colorOuter.A; i += nStep)
            {
                this.pen.Color = Color.FromArgb(i/*alpha*/, this.pen.Color);
                gr.DrawRectangle(this.pen, rect);

                rect.Inflate(-1, -1);
            }
        }

        #endregion
    }

    #endregion

    #region EventArgs

    public class BarEventArgs : EventArgs
    {
        private int nIndex;
        public int BarIndex
        {
            get { return nIndex; }
            set { nIndex = value; }
        }

        private HBarItem bar;
        public HBarItem Bar
        {
            get { return bar; }
        }

        public BarEventArgs()
        {
            bar = null;
            nIndex = -1;
        }

        public BarEventArgs(HBarItem bar, int nBarIndex)
        {
            this.bar = bar;
            BarIndex = nBarIndex;
        }
    }

    #endregion

    #region Print

    // A print helper class for a bitmap buffer. Landscape or portrate, but
    // does NOT support any angles in between. It does not check for maximum printable
    // pages, which I think might cause spoolsv.exe to fail if overflows.
    public class CPrinter
    {
        private Bitmap bmpBuffer;
        public Bitmap BmpBuffer
        {
            get { return bmpBuffer; }
            set { bmpBuffer = value; }
        }

        private int nPageCount;
        public int PageCount
        {
            get { return nPageCount; }
            set { nPageCount = value; }
        }

        private int nPagesPrinted;
        public int PagesPrinted
        {
            get { return nPagesPrinted; }
            set { nPagesPrinted = value; }
        }

        private PrintDocument document;
        public PrintDocument Document
        {
            get { return document; }
            set { /*document = value;*/ }
        }

        private bool bFitToPaper;
        public bool FitToPaper
        {
            get { return bFitToPaper; }
            set { bFitToPaper = value; }
        }

        public CPrinter()
        {
            //printSettings = new PrinterSettings();
            bmpBuffer = null;

            document = new PrintDocument();
            document.PrintPage += new PrintPageEventHandler(this.OnPrintPage);
        }

        public bool ShowOptions()
        {
            bool ret = false;

            DialogResult res;
            PrintDialog pdlg;

            pdlg = new PrintDialog();
            pdlg.Document = document;
            pdlg.UseEXDialog = true;

            res = pdlg.ShowDialog();
            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                ret = true;
            }
            pdlg.Dispose();
            pdlg = null;

            return ret;
        }

        public bool Print()
        {
            if (BmpBuffer == null) return false;

            PageCount = 0;

            document.Print();

            return true;
        }

        private void OnPrintPage(object sender, PrintPageEventArgs e)
        {
            if (FitToPaper)
            {
                e.Graphics.DrawImage(
                    BmpBuffer,
                    e.PageBounds,
                    new Rectangle(
                    0,
                    0,
                    BmpBuffer.Width,
                    BmpBuffer.Height),
                    GraphicsUnit.Pixel);
            }
            else
            {
                e.Graphics.DrawImageUnscaled(
                    BmpBuffer,
                    e.PageBounds.Left,
                    e.PageBounds.Top);
            }

            e.HasMorePages = false;
            /*
            // Prepare for next pageas
            PagesPrinted++;
            if (PagesPrinted >= PageCount)
            {
                e.HasMorePages = false;
                BmpBuffer.Dispose();
                BmpBuffer = null;
            }
            else e.HasMorePages = true;
             */
        }
    }

    #endregion

    #region DataSource

    /// <summary>
    /// CDataColumnItem is holder for a data column of the data source
    /// </summary>
    public class CDataColumnItem
    {
        private string name;
        private string displayName;
        private int boundIndex;
        private TypeConverter converter;
        private Type valueType;
        bool isReadonly;

        public string Name
        {
            get{ return name; }
            set{ name = value; }
        }

        public string DisplayName
        {
            get{ return displayName; }
            set{ displayName = value; }
        }

        public bool IsReadonly
        {
            get { return isReadonly; }
            set { isReadonly = value; }
        }

        public int BoundIndex
        {
            get{ return boundIndex; }
            set{ boundIndex = value; }
        }

        public TypeConverter Converter
        {
            get{ return converter; }
            set{ converter = value; }
        }

        public Type ValueType
        {
            get{ return valueType; }
            set{ valueType = value; }
        }
    }

    /// <summary>
    /// CDataColumnCollection is a collection of columns of the data source
    /// </summary>
    public class CDataColumnCollection : IList<CDataColumnItem>
    {
        private List<CDataColumnItem> items;

        public CDataColumnCollection()
        {
            items = new List<CDataColumnItem>();
        }

        #region IList<CDataColumnItem> Members

        public int IndexOf(CDataColumnItem item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, CDataColumnItem item)
        {
            items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        public CDataColumnItem this[int index]
        {
            get
            {
                return items[index];
            }
            set
            {
                items[index] = value;
            }
        }

        #endregion

        #region ICollection<CDataColumnItem> Members

        public void Add(CDataColumnItem item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(CDataColumnItem item)
        {
            return items.Contains(item);
        }

        public void CopyTo(CDataColumnItem[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(CDataColumnItem item)
        {
            return items.Remove(item);
        }

        #endregion

        #region IEnumerable<CDataColumnItem> Members

        public IEnumerator<CDataColumnItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// A data manager should impelement these functions and respond to events by updating chart
    /// </summary>
    internal interface IDataConnectionEvents
    {
        // ListChangedType.ItemChanged. If nColIndex==-1 more than one column changed. Update all row
        void DataSource_ItemUpdated(int nRowIndex, int nColIndex);
        
        // ListChangedType.ItemDeleted 
        void DataSource_ItemDeleted(int nItemIndex);

        // ListChangedType.ItemAdded
        void DataSource_ItemAdded(int nItemIndex);

        // ListChangedType.ItemChanged
        void DataSource_SelectedRowChanged(int nPosition);

        // ListChangedType.Reset
        // ListChangedType.ItemMoved (index changed)
        // ListChangedType.PropertyDescriptorAdded
        // ListChangedType.PropertyDescriptorDeleted
        // ListChangedType.PropertyDescriptorChanged
        void DataSource_ResetItems();

        // Initialization finished successfully
        void DataSource_DataBoundCompleted();

        // Will be called by owner to get refrences to this class
        void SetData(object chart, object dataConnection);
    }

    // This class acts as an interface between chart GUI and Datasource. 
    // DataSource will be controled by the 'data' field of this class. 
    // Chart will be accessed by the 'owner' field of the class. So this
    // class relates 'owner' to 'data'. To do that it uses another class.
    // In fact there's another class in between. A class that impelements 
    // 'IDataConnectionEvents'.
    //
    // Chart(owner) <-> DataSourceManager <-> IDataConnectionEvents
    //                          ^
    //                          |
    //                          v
    //                   CDataConnection
    //
    //
    // The key reason to use 'dataEventHandler' is to be able to add another 
    // way of handling data later on. To add another data interpretor, we need to:
    // 1. Add a new scheme name to 'InterpretSchemes' enum. This class will recieve datasource events.
    // 2. Add a new class that impelements 'IDataConnectionEvents' interface.
    // 3. Instantiate a member of the newly created class(step 2) in 'SetDataInterpreterMode'
    // function of this class.
    //
    // The 'SetDataInterpreterMode' function cause this class to send events 
    // to your class if your class impelemented 'IDataConnectionEvents' interface.
    // It's your responsibility to respond to these events. The way you handle these 
    // events identifys the way the chart handles input data.
    public class CDataSourceManager
    {
        #region Fields

        // Refrence to parent for notifying it of events or asking it to reDraw
        private object owner;

        // A data connection object to relate us to chart DataSource ( and it's related stuff like CurrencyManager)
        private CDataConnection data;

        // An object that interpret data of the connected datasourceto respond to it's events and feed chart with true data
        //private object dataEventHandler;

        #endregion

        public object DataSource
        {
            get
            {
                return this.data.DataSource;
            }
            // Parent calls 'ConnectTo' rather than set
        }

        public string DataMember
        {
            get
            {
                if (this.data == null) return null;

                return this.data.DataMember;
            }
            // Parent calls 'ConnectTo' rather than set
        }

        public CDataConnection DataConnection
        {
            get { return this.data; }
        }

        public object DataEventHandler
        {
            get 
            {
                if (this.data == null)
                {
                    return null;
                }

                return this.data.DataEventHandler; 
            }

            set 
            {
                if (this.data == null)
                {
                    this.data = new CDataConnection((UserControl)this.owner, value);
                }
                else
                {
                    this.data.DataEventHandler = value;
                }
            }
        }

        internal void ConnectTo(object dataSource, string dataMember)
        {
            if (this.data == null)
            {
                this.data = new CDataConnection((UserControl)this.owner, null);
            }

            this.data.SetDataSource(dataSource, dataMember);
        }

        public CDataSourceManager(HBarChart owner)
        {
            this.owner = owner;
        }
    }

    // A class to connect to a datasource, interact with it, retrieve data
    // and recieve events. It sends events to it's member of type
    // IDataConnectionEvents.
    public class CDataConnection
    {
        #region Enums

        public enum DataSourceStates { None, Initializing, Initialized }
        public enum ConnectionStates { None, Initializing, Initialized }

        #endregion

        #region Fields
        
        //underlying data
        private CDataColumnCollection columns;
        private ArrayList rows;

        private int nLastSelectedRowIndex;


        // We need a refrence to owner of this class. Owner should has BindingContext
        System.Windows.Forms.UserControl parent = null;

        // Refrence to a DataManager class that will recieve messages of this class
        private object dataEventHandler;

        // Please note that corrency refers to 'being current' here, not $, Euro, Rials, etc.
        // Maybe they wanted to say concurrency. Anyway my English is not goodenough to decide
        protected CurrencyManager currencyManager = null;

        // Must have for supporting DataSource
        object dataSource = null;
        string dataMember = String.Empty;

        // For initializable datasources
        private DataSourceStates dataSourceState = DataSourceStates.None;

        // Current initialization state befor connecting to a datasource
        private ConnectionStates connectionState = ConnectionStates.None;
        
        #endregion

        #region Properties

        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
        }

        public string DataMember
        {
            get
            {
                return this.dataMember;
            }
        }

        public DataSourceStates DataSourceState
        {
            get { return dataSourceState; }
        }
        
        public ConnectionStates ConnectionState
        {
            get { return connectionState; }
        }

        public CurrencyManager CurrencyManager
        {
            get { return this.currencyManager; }
        }

        public CDataColumnCollection Columns
        {
            get { return columns; }
        }

        public ArrayList Rows
        {
            get { return rows; }
        }

        public int LastSelectedRowIndex
        {
            get { return this.nLastSelectedRowIndex; }
        }

        public object DataEventHandler
        {
            get { return this.dataEventHandler; }
            set { this.SetEventHandler(value);  }
        }

        #endregion

        #region Methods

        // Will be called uppon changes in datasource or datamember by parent
        // and also in DataSource_Initialized function internally
        public void SetDataSource(object dataSource, string dataMember)
        {
            // Enter only if another initialization is not in progress
            // Any one has time for adding race condition checkout here?
            if (this.connectionState == ConnectionStates.Initializing) return;
            this.connectionState = ConnectionStates.Initializing;

            // It is said that some data sources need to be initialized before being used. [OMG]
            ISupportInitializeNotification supportInitialize = this.dataSource as ISupportInitializeNotification;
            if ( supportInitialize != null && this.dataSourceState == DataSourceStates.Initializing)
            {
                // This function was called by initialize event handler of the datasource
                // so the datasource is probably initialized by now we'll find it out later
                // but befor that we'd better make sure we're not using event any longer
                supportInitialize.Initialized -= new EventHandler(DataSource_Initialized);
            } 


            // Update datasource and member
            if (dataMember == null) dataMember = String.Empty;
            this.dataSource = dataSource;
            this.dataMember = dataMember;

            // Without a BindingContext, how would us have a currencyManager?!
            if (this.parent.BindingContext == null) return;

            try
            {
                // Stop recieving events from old source if any exists.
                if (this.currencyManager != null)
                {
                    this.currencyManager.PositionChanged -= new EventHandler(CurrencyManager_PositionChanged);
                    this.currencyManager.ListChanged -= new ListChangedEventHandler(CurrencyManager_ListChanged);
                }

                // Update currencyManager if we should
                if (this.dataSource != null && this.dataSource != Convert.DBNull)
                {
                    if (supportInitialize != null && !supportInitialize.IsInitialized)
                    {
                        if (this.dataSourceState == DataSourceStates.None)
                        {
                            this.dataSourceState = DataSourceStates.Initializing;
                            supportInitialize.Initialized += new EventHandler(DataSource_Initialized);
                        }
                        // after initialization, this function will be called later and this will be set
                        this.currencyManager = null;
                    }
                    else
                    {
                        this.currencyManager = this.parent.BindingContext[this.dataSource, this.dataMember] as CurrencyManager;
                        IDataConnectionEvents events = this.dataEventHandler as IDataConnectionEvents;
                        RenewAllData();
                        if (events != null)
                        {
                            events.DataSource_DataBoundCompleted();
                        }
                    }
                }
                else
                {
                    this.currencyManager = null;
                }

                // I want to recieve all events
                if (this.currencyManager != null)
                {
                    this.currencyManager.PositionChanged += new EventHandler(CurrencyManager_PositionChanged);
                    this.currencyManager.ListChanged += new ListChangedEventHandler(CurrencyManager_ListChanged);
                }
            }
            finally
            {
                this.connectionState = ConnectionStates.Initialized;
            }
        }

        // If datasource needs to be initialized, this message callback will be added
        // in SetDataSource function and will be called after datasource is finished it's initialization process
        private void DataSource_Initialized(object sender, EventArgs e)
        {
            ISupportInitializeNotification supportInitialize = this.dataSource as ISupportInitializeNotification;

            if (supportInitialize != null)
            {
                supportInitialize.Initialized -= new EventHandler(DataSource_Initialized);
            }

            this.dataSourceState = DataSourceStates.Initialized;

            SetDataSource(this.dataSource, this.dataMember);
            // we can now inform parent of DatabindingCompleted event
        }

        // This callback is called by CurrencyManager indicating a modification in the data
        private void CurrencyManager_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch(e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    AddItem(e.NewIndex);
                    break;

                case ListChangedType.ItemDeleted: 
                    DeleteItem(e.NewIndex);
                    break;

                case ListChangedType.ItemChanged: 
                    UpdateItem(e.NewIndex);
                    break;

                default:
                    // In each of these cases I better reclculate everything
                    
                    // ListChangedType.Reset
                    // ListChangedType.ItemMoved (index changed)
                    // ListChangedType.PropertyDescriptorAdded
                    // ListChangedType.PropertyDescriptorDeleted
                    // ListChangedType.PropertyDescriptorChanged

                    ResetItems();
                    break;
            }
        }

        // This callback is called by CurrencyManager when current row is changed
        private void CurrencyManager_PositionChanged(object sender, EventArgs e)
        {
            OnSelecltedRowChanged();
        }


        /// <summary>
        /// This function gets index of a given property name
        /// </summary>
        /// <param name="dataPropertyName">property name</param>
        /// <returns>index of the given property name</returns>
        public int GetColumnIndex(string dataPropertyName)
        {
            PropertyDescriptorCollection props = this.currencyManager.GetItemProperties();
            if (props == null) return -1;

            int ret = -1;
            for (int i = 0; i < props.Count; i++)
            {
                if (String.Compare(props[i].Name, dataPropertyName, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                {
                    ret = i;
                    break;
                }
            }

            return ret;
        }
 


        private void UpdateItem(int itemIndex)
        {
            if (this.columns == null || this.columns.Count == 0) RenewAllData();
            if (this.columns == null || this.columns.Count == 0) return;
            if (this.rows == null || this.rows.Count == 0) return;

            // add a row to our rows
            PropertyDescriptorCollection props = this.currencyManager.GetItemProperties();
            if (props == null) return;

            int nUpdatedColumn = -1;
            int nChanges = 0;
            for (int i = 0; i < this.columns.Count; i++)
            {
                if (((ArrayList)rows[itemIndex])[i] != props[i].GetValue(this.currencyManager.List[itemIndex]))
                {
                    ((ArrayList)rows[itemIndex])[i] = props[i].GetValue(this.currencyManager.List[itemIndex]);
                    nUpdatedColumn = i;
                    nChanges++;
                }
            }

            IDataConnectionEvents events = this.dataEventHandler as IDataConnectionEvents;
            if (events != null)
            {
                events.DataSource_ItemUpdated(itemIndex, nChanges == 1 ? nUpdatedColumn : -1);
            }
        }

        private void DeleteItem(int itemIndex)
        {
            if (this.columns == null || this.columns.Count == 0) RenewAllData();
            if (this.columns == null || this.columns.Count == 0) return;
            if (this.rows == null || this.rows.Count == 0) return;

            PropertyDescriptorCollection props = this.currencyManager.GetItemProperties();
            if (props == null) return;
            if (itemIndex >= rows.Count) return;
            
            // Inform parent of the change before deleting the record
            IDataConnectionEvents events = this.dataEventHandler as IDataConnectionEvents;
            if (events != null)
            {
                events.DataSource_ItemDeleted(itemIndex);
            }

            // Now remove the row
            this.rows.RemoveAt(itemIndex);
        }

        private void AddItem(int itemIndex)
        {
            if (this.columns == null || this.columns.Count == 0) RenewAllData();
            if (this.columns == null || this.columns.Count == 0) return;

            // add a row to our rows
            PropertyDescriptorCollection props = this.currencyManager.GetItemProperties();
            if (props == null) return;

            ArrayList row = new ArrayList(this.columns.Count);
            for (int i=0;i<this.columns.Count;i++)
            {
                row.Add(props[i].GetValue(this.currencyManager.List[itemIndex]));
            }
            this.rows.Insert(itemIndex, row);
            row = null;

            IDataConnectionEvents events = this.dataEventHandler as IDataConnectionEvents;
            if (events != null)
            {
                events.DataSource_ItemAdded(itemIndex);
            }
        }

        private void OnSelecltedRowChanged()
        {
            if (this.nLastSelectedRowIndex != this.currencyManager.Position)
            {
                ResetColumns();
                ResetRows();
            }

            IDataConnectionEvents events = this.dataEventHandler as IDataConnectionEvents;
            if (events != null)
            {
                events.DataSource_SelectedRowChanged(this.currencyManager.Position);
            }

            this.nLastSelectedRowIndex = this.currencyManager.Position;
        }

        private void ResetItems()
        {
            RenewAllData();

            IDataConnectionEvents events = this.dataEventHandler as IDataConnectionEvents;
            if (events != null)
            {
                if (events != null) events.DataSource_ResetItems();
            }
        }

        private void RenewAllData()
        {
            // Update internal data
            ResetColumns();
            ResetRows();
        }

        /// <summary>
        /// WARNING: THIS IS SLOW. Use for few amound of data only
        /// Using current bound data, populates Rows field of this class by rows of the datasource
        /// </summary>
        private void ResetRows()
        {
            this.rows.Clear();
            //this.props[boundColumnIndex].GetValue(this.currencyManager[rowIndex]);

            if (this.columns == null || this.columns.Count == 0) return;
            if (this.currencyManager == null) return;
            
            PropertyDescriptorCollection props = this.currencyManager.GetItemProperties();
            if (props == null) return;

            ArrayList row;
            for (int i = 0; i < currencyManager.List.Count; i++)
            {
                row = new ArrayList(columns.Count);

                for (int j = 0; j < columns.Count; j++)
                {
                    row.Add(props[j].GetValue(this.currencyManager.List[i]));
                }
                this.rows.Add(row);
            }
        }

        /// <summary>
        /// Using current bound data, populates Columns field of this class by columns of the datasource
        /// </summary>
        private void ResetColumns()
        {
            this.Columns.Clear();

            CDataColumnItem item = null;

            if (this.currencyManager == null) return;
            
            PropertyDescriptorCollection props = this.currencyManager.GetItemProperties();
            if (props == null) return;

            for (int i=0; i<props.Count; i++)
            {
                item = new CDataColumnItem();
                item.BoundIndex = i;
                item.Converter = props[i].Converter;
                item.DisplayName = props[i].DisplayName;
                item.IsReadonly = props[i].IsReadOnly;
                item.Name = props[i].Name;
                item.ValueType = props[i].PropertyType;

                this.Columns.Add(item);
                item = null;
            }

        }

        #endregion

        #region Constructors/Distructors

        public CDataConnection(UserControl parent, object dataEventHandler) :this()
        {
            this.parent = parent;

            SetEventHandler(dataEventHandler);
        }

        private void SetEventHandler(object dataEventHandler)
        {
            if (this.dataEventHandler != dataEventHandler)
            {
                if (this.dataEventHandler != null)
                {
                    this.dataEventHandler = null;
                }

                this.dataEventHandler = dataEventHandler;
                IDataConnectionEvents eh = dataEventHandler as IDataConnectionEvents;
                if (eh != null)
                {
                    eh.SetData(this.parent, this);
                }
            }
        }

        public CDataConnection()
        {
            this.dataEventHandler = null;
            this.parent = null;
            this.columns = new CDataColumnCollection();
            this.rows = new ArrayList();
        }

        public void Dispose()
        {
            if (this.currencyManager != null)
            {
                this.currencyManager.PositionChanged -= new EventHandler(CurrencyManager_PositionChanged);
                this.currencyManager.ListChanged -= new ListChangedEventHandler(CurrencyManager_ListChanged);
            }
            this.currencyManager = null;

            if (this.rows != null)
            {
                for (int i=0;i<this.rows.Count;i++)
                {
                    ((ArrayList)this.rows[i]).Clear();
                }
                this.rows.Clear();
            }

            if (this.columns != null && this.columns.Count > 0) this.columns.Clear();
        }

        #endregion
    }

    // This class defines behavior of the chart in response to data events
    public class CreateChartForEachRow : IDataConnectionEvents
    {
        private CDataConnection data;
        private HBarChart chart;
        private Color[] colors;

        public CreateChartForEachRow()
        {
            this.data = null;
            this.chart = null;

            // A set of colors to select a random one from these.
            this.colors = new Color[] {
                Color.FromArgb(255, 200, 255, 255), 
                Color.FromArgb(255, 150, 200, 255), 
                Color.FromArgb(255, 100, 100, 200),
                Color.FromArgb(255, 255, 60, 130),
                Color.FromArgb(255, 250, 200, 255),
                Color.FromArgb(255, 255, 255, 0),
                Color.FromArgb(255, 255, 155, 55),
                Color.FromArgb(255, 150, 200, 155),
                Color.FromArgb(255, 255, 255, 200),
                Color.FromArgb(255, 100, 150, 200),
                Color.FromArgb(255, 130, 235, 250),
                Color.FromArgb(255, 150, 240, 80)};
        }

        #region IDataConnectionEvents Members

        public void SetData(object chart, object dataConnection)
        {
            this.chart = chart as HBarChart;
            this.data = dataConnection as CDataConnection;
        }

        public void DataSource_ItemUpdated(int nRowIndex, int nColIndex)
        {
            if (nRowIndex < 0) return;
            if (this.chart == null) return;
            if (nRowIndex == this.data.LastSelectedRowIndex)
            {
                // which column changed?
                if (nColIndex < 0)
                {
                    for (int i = 0; i < this.data.Columns.Count; i++)
                    {
                        ArrayList row = (ArrayList)this.data.Rows[nRowIndex];
                        if (row != null && row[i] != null && row[i] != Convert.DBNull)
                        {
                            chart.ModifyAt(i, Convert.ToDouble(row[i]));
                        }
                        else
                        {
                            chart.ModifyAt(i, 0);
                        }
                    }
                }
                else
                {
                    double dValue = Convert.ToDouble(((ArrayList)this.data.Rows[nRowIndex])[nColIndex]);
                    chart.ModifyAt(nColIndex, dValue);
                }
                chart.RedrawChart();
            }
        }

        public void DataSource_ItemDeleted(int nItemIndex)
        {
            if (nItemIndex < 0) return;
            if (this.chart == null) return;
            if (nItemIndex == this.data.LastSelectedRowIndex)
            {
                chart.Items.Clear();
                chart.RedrawChart();
            }
        }

        public void DataSource_ItemAdded(int nItemIndex)
        {
            // Do nothing, unless it changes current row
            if (this.data.LastSelectedRowIndex == nItemIndex)
            {
                DataSource_ResetItems();
            }
        }

        public void DataSource_SelectedRowChanged(int nPosition)
        {
            if (nPosition < 0) return;
            if (this.chart == null) return;

            chart.Items.Clear();

            Random r = new Random(1);
            for (int i = 0; i < this.data.Columns.Count; i++)
            {
                ArrayList row = (ArrayList)this.data.Rows[nPosition];
                if (row != null && row[i] != null && row[i] != Convert.DBNull)
                {
                    chart.Add(
                        Convert.ToDouble(row[i]),
                        String.IsNullOrEmpty(this.data.Columns[i].DisplayName) ? this.data.Columns[i].Name : this.data.Columns[i].DisplayName,
                        this.colors[r.Next(0, colors.Length-1)]);
                }
                else
                {
                    chart.Add(null);
                }
            }

            chart.RedrawChart();
        }

        public void DataSource_ResetItems()
        {
            if (this.data.LastSelectedRowIndex < 0) return;
            if (this.chart == null) return;

            chart.Items.Clear();

            Random r = new Random(1);
            for (int i = 0; i < this.data.Columns.Count; i++)
            {
                ArrayList row = (ArrayList)this.data.Rows[this.data.LastSelectedRowIndex];
                if (row != null && row[i] != null && row[i] != Convert.DBNull)
                {
                    chart.Add(
                        Convert.ToDouble(row[i]),
                        String.IsNullOrEmpty(this.data.Columns[i].DisplayName) ? this.data.Columns[i].Name : this.data.Columns[i].DisplayName,
                        this.colors[r.Next(0, colors.Length - 1)]);
                }
                else
                {
                    chart.Add(null);
                }
            }

            chart.RedrawChart();
        }

        public void DataSource_DataBoundCompleted()
        {
            if (this.chart == null) return;
            
            // Create chart
            DataSource_ResetItems();
        }

        #endregion
    }

    #endregion
}
