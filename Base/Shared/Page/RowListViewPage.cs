using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public abstract class RowCell : ViewCell
    {
        public static readonly BindableProperty UpdatedProperty = BindableProperty.Create("Updated", typeof(int), typeof(RowCell), 0);

        public static double DisabledAlphaValue = 0.2;

        public readonly PointerFrame Frame = new PointerFrame();
        public readonly Layout RowLayout;

        protected IButtonStyle _rowStyle;

        public virtual new bool IsEnabled
        {
            get => base.IsEnabled;

            set
            {
                if (base.IsEnabled && value)
                    return;
                if (!base.IsEnabled && !value)
                    return;

                base.IsEnabled = value;

                if (_rowStyle != null)
                {
                    if (value)
                    {
                        Frame.ColorStyle = _rowStyle.Color;
                        //Frame.ColorStyleChanged();
                    }
                    else
                    {
                        Frame.ColorStyle = _rowStyle.DisabledColor;
                        //Frame.ColorStyleChanged();
                    }
                }
            }
        }

        protected RowCell(Layout rowLayout)
        {
            RowLayout = rowLayout;
            var inputTransparent = (UIApp.IsMacOS);

            if (inputTransparent)
                RowLayout.InputTransparent = true;

            RowLayout.LayoutChanged += Layouted;
            RowLayout.SizeChanged += Layouted;

            Frame.Margin = new Thickness(10, 0, 10, 10);
            Frame.ColorStyle = Theme.RowColor;
            Frame.Content = rowLayout;

            View = Frame;
        }

        void Layouted(object sender, EventArgs e)
        {
            Layouted((int)Frame.Width, (int)Frame.Height);
        }

        protected virtual void Layouted(int width, int height)
        {

        }
    }

    public abstract class RowCell<T> : RowCell where T : RowItem
    {
        protected T Item => (T)BindingContext;

        protected RowCell(Layout rowLayout) : base(rowLayout)
        {
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if(BindingContext is T rowItem && propertyName == UpdatedProperty.PropertyName)
            {
                Update(rowItem);
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if(BindingContext is T rowItem)
            {
                Update(rowItem);
                ForceUpdateSize();
            }
        }

        protected virtual void Update(T item)
        {
            if (IsEnabled != item.IsEnabled)
                IsEnabled = item.IsEnabled;

            if (_rowStyle != item.RowStyle)
                _rowStyle = item.RowStyle;

            if (_rowStyle == null)
            {
                if(Frame.BackgroundColor != Color.Transparent)
                    Frame.BackgroundColor = Color.Transparent;
            }
            else
            {
                if (IsEnabled)
                {
                    if(Frame.ColorStyle != _rowStyle.Color)
                        Frame.ColorStyle = _rowStyle.Color;
                }
                else
                {
                    if(Frame.ColorStyle != _rowStyle.DisabledColor)
                        Frame.ColorStyle = _rowStyle.DisabledColor;
                }
            }
        }
    }

    public abstract class RowItem : INotifyPropertyChanged
    {
        public readonly int RowId;

        public string Identifier { get; set; }
        public object Tag { get; set; }

        public int Updated { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsEnabled { get; private set; } = true;
        public void UpdateEnabled(bool enabled, bool propagateChanges)
        {
            IsEnabled = enabled;
            if (propagateChanges)
                Update();
        }

        public IButtonStyle RowStyle { get; protected set; }
        public void UpdateRowStyle(IButtonStyle rowStyle, bool propagateUpdate)
        {
            RowStyle = rowStyle;
            if (propagateUpdate)
                Update();
        }

        protected RowItem(int rowId)
        {
            RowId = rowId;
        }

        public void Update()
        {
            Updated++;
            OnPropertyChanged("Updated");
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RowLabelItem : RowItem
    {
        public const int RowLabelId = 10;

        public int LabelPadding { get; protected set; } = 40;
        public void UpdatePadding(int padding, bool propagateUpdate)
        {
            LabelPadding = padding;
            if (propagateUpdate)
                Update();
        }

        public FontStyle FontStyle { get; protected set; } = Theme.RowFont;
        public void UpdateFontStyle(FontStyle fontStyle, bool propagateUpdate)
        {
            FontStyle = fontStyle;
            if (propagateUpdate)
                Update();
        }

        public ColorStyle ColorStyle { get; protected set; } = Theme.TextColor;
        public void UpdateColorStyle(ColorStyle colorStyle, bool propagateUpdate)
        {
            ColorStyle = colorStyle;
            if (propagateUpdate)
                Update();
        }

        public string Text { get; private set; }
        public void UpdateText(string text, bool propagateUpdate)
        {
            Text = text;
            if (propagateUpdate)
                Update();
        }

        protected RowLabelItem(string text, int rowId) : base(rowId)
        {
            Text = text;
            RowStyle = Theme.RowButton;
        }

        public RowLabelItem(string text) : base(RowLabelId)
        {
            Text = text;
            RowStyle = Theme.RowButton;
        }
    }

    public class RowLabelCell : RowLabelCell<RowLabelItem>
    {
    }

    public class RowLabelCell<T> : RowCell<T> where T : RowLabelItem
    {
        int _lastLabelHeight;
        int _padding = 40;

        public readonly ExtLabel Label = new ExtLabel();

        public new AbsoluteLayout RowLayout => (AbsoluteLayout)base.RowLayout;

        public override bool IsEnabled
        {
            get => base.IsEnabled;

            set
            {
                if (base.IsEnabled && value)
                    return;
                if (!base.IsEnabled && !value)
                    return;

                base.IsEnabled = value;

                if (value)
                {
                    Label.ColorStyleAlpha = 1.0;
                }
                else
                {
                    Label.ColorStyleAlpha = DisabledAlphaValue;
                }
            }
        }

        public RowLabelCell() : this(new AbsoluteLayout())
        {
        }

        public RowLabelCell(AbsoluteLayout rowLayout) : base(rowLayout)
        {
            Label.InputTransparent = true;
            if (!UIApp.IsGTK)
                Label.IsEnabled = false;

            Label.FontStyle = Theme.RowFont;
            Label.ColorStyle = Theme.TextColor;

            Label.Margin = new Thickness(15, 5, 0, 5);

            //Label.WidthRequest = Screen.StackPageWidth - (LabelPadding + Label.Margin.Left);

            AbsoluteLayout.SetLayoutFlags(Label, AbsoluteLayoutFlags.YProportional);
            AbsoluteLayout.SetLayoutBounds(Label, new Rectangle(0, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

            RowLayout.Children.Add(Label);
        }

        protected override void Update(T item)
        {
            base.Update(item);

            if (_padding != item.LabelPadding)
            {
                _padding = item.LabelPadding;
                RowLayout.ForceLayout();
            }

            if (Label.ColorStyle != item.ColorStyle)
                Label.ColorStyle = item.ColorStyle;
            if (Label.FontStyle != item.FontStyle)
                Label.FontStyle = item.FontStyle;

            if (Label.Text != item.Text)
                Label.Text = item.Text;
        }

        int _lastHeight;

        protected override void Layouted(int width, int height)
        {
            if (height > 0)
            {
                if(_lastHeight == 0)
                {
                    _lastHeight = height;
                }
                else
                {
                    if(height != _lastHeight)
                    {
                        _lastHeight = height;
                        ForceUpdateSize();
                    }
                }
            }

            var p = (int)(_padding + Label.Margin.Left);

            if ((int)Label.WidthRequest != (width - p))
                Label.WidthRequest = width - p;

            var labelHeight = (int)Label.Height;

            if (labelHeight > 0)
            {
                var rh = RenderHeight;
                if (_lastLabelHeight == 0)
                {
                    _lastLabelHeight = labelHeight;
                    if (height < labelHeight || height > labelHeight)
                        ForceUpdateSize();
                }
                else if (_lastLabelHeight != labelHeight)
                {
                    _lastLabelHeight = labelHeight;
                    ForceUpdateSize();
                }
            }
        }
    }

    public class RowHeaderItemBase : RowLabelItem
    {
        public readonly HeaderRowType HeaderType;

        protected RowHeaderItemBase(HeaderRowType headerRowType, int rowId, string text) : base(text, rowId)
        {
            HeaderType = headerRowType;
            LabelPadding = 0;
            RowStyle = null;

            switch(headerRowType)
            {
                case HeaderRowType.Title:
                    FontStyle = Theme.RowTitleFont;
                    break;
                case HeaderRowType.Header:
                    FontStyle = Theme.RowHeaderFont;
                    break;
                case HeaderRowType.Footer:
                    FontStyle = Theme.RowFooterFont;
                    break;
            }
        }
    }

    public class RowTitleItem : RowHeaderItemBase
    {
        public const int RowTitleId = 1;

        public RowTitleItem(string text) : base(HeaderRowType.Title, RowTitleId, text)
        {

        }
    }

    public class RowHeaderItem : RowHeaderItemBase
    {
        public const int RowHeaderId = 2;

        public RowHeaderItem(string text) : base(HeaderRowType.Header, RowHeaderId, text)
        {

        }
    }

    public class RowFooterItem : RowHeaderItemBase
    {
        public const int RowFooterId = 3;

        public RowFooterItem(string text) : base(HeaderRowType.Footer, RowFooterId, text)
        {

        }
    }

    public class RowHeaderCell : RowLabelCell<RowHeaderItemBase>
    {
        public RowHeaderCell() : base (new AbsoluteLayout())
        {
        }

        protected override void Update(RowHeaderItemBase item)
        {
            base.Update(item);

            if (item.HeaderType == HeaderRowType.Title)
            {
                Label.Margin = new Thickness(10, 5, 0, 5);
            }
            else if (item.HeaderType == HeaderRowType.Header)
            {
                Label.Margin = new Thickness(5, 5, 0, 0);
            }
            else
            {
                Label.Margin = new Thickness(5, 0, 0, 0);
            }
        }
    }

    public abstract class RowViewCell<V, R> : RowCell<R> where V : View where R : RowViewItem
    {
        protected readonly V _view;

        public new AbsoluteLayout RowLayout => base.RowLayout as AbsoluteLayout;

        protected RowViewCell(V view, bool addMargin, AbsoluteLayout rowLayout = null) : base(rowLayout ?? new AbsoluteLayout())
        {
            _view = view;

            if (addMargin)
                view.Margin = new Thickness(15, 15, 15, 15);

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.SizeProportional);
            AbsoluteLayout.SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));

            RowLayout.Children.Add(view);
        }
    }

    public class RowViewItem : RowItem
    {
        public RowViewItem(int rowId) : base(rowId)
        {
            RowStyle = Theme.RowButton;
        }
    }

    public class RowDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RowHeaderCellDataTemplate { get; private set; }

        readonly Dictionary<int, DataTemplate> _templates = new Dictionary<int, DataTemplate>();

        public RowDataTemplateSelector()
        {
            RowHeaderCellDataTemplate = new DataTemplate(typeof(RowHeaderCell));
            RowHeaderCellDataTemplate.SetBinding(RowCell.UpdatedProperty, RowCell.UpdatedProperty.PropertyName);
        }

        public void AddDataTemplate<T>(int id) where T : RowCell
        {
            if (_templates.ContainsKey(id))
                throw new ArgumentException("Id already used", nameof(id));

            var dataTemplate = new DataTemplate(typeof(T));
            dataTemplate.SetBinding(RowCell.UpdatedProperty, RowCell.UpdatedProperty.PropertyName);
            _templates[id] = dataTemplate;
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var rowItem = item as RowItem;
            //Console.WriteLine($"{rowItem.RowId}");
            _templates.TryGetValue(rowItem.RowId, out var template);

            if (template == null)
                throw new Exception($"DataTemplate with id {rowItem.RowId} not found.");

            return template;
        }
    }

    public class RowListView : ListView
    {
        public readonly ObservableCollection<RowItem> Rows = new ObservableCollection<RowItem>();
        public readonly RowDataTemplateSelector TemplateSelector = new RowDataTemplateSelector();

        public RowListView() : base(ListViewCachingStrategy.RecycleElementAndDataTemplate)
        {
            SelectionMode = ListViewSelectionMode.None;
            HasUnevenRows = true;
            SeparatorVisibility = SeparatorVisibility.None;

            ItemTemplate = TemplateSelector;
            ItemsSource = Rows;

            BackgroundColor = Color.Transparent;
        }
    }

    public class RowListViewPage : ExtContentPage
    {
        readonly RowListView _rowListView = new RowListView();

        public RowListViewPage() : base("RowListViewPage")
        {
            _rowListView.TemplateSelector.AddDataTemplate<RowHeaderCell>(RowTitleItem.RowTitleId);
            _rowListView.TemplateSelector.AddDataTemplate<RowHeaderCell>(RowHeaderItem.RowHeaderId);
            _rowListView.TemplateSelector.AddDataTemplate<RowHeaderCell>(RowFooterItem.RowFooterId);
            _rowListView.TemplateSelector.AddDataTemplate<RowLabelCell>(RowLabelItem.RowLabelId);

            for (var i = 0; i < 500; i++)
            {
                var mod = i % 10;

                if (mod == 0)
                {
                    _rowListView.Rows.Add(new RowTitleItem(i + " Title as fas dfas dfsad fsad fsad fsa dfsa dfsad fsa fdsa fsad fsad fsad f"));
                }
                else if (mod == 1)
                {
                    _rowListView.Rows.Add(new RowHeaderItem(i + " Header as fas dfas dfsad fsad fsad fsa dfsa dfsad fsa fdsa fsad fsad fsad f"));
                }
                else if (mod == 9)
                {
                    _rowListView.Rows.Add(new RowFooterItem(i + " Footer as fas dfas dfsad fsad fsad fsa dfsa dfsad fsa fdsa fsad fsad fsad f"));
                }
                else
                {
                    _rowListView.Rows.Add(new RowLabelItem(i + " RowLabelItem Title as fas dfas dfsad fsad fsad fsa dfsa dfsad fsa fdsa fsad fsad fsad f"));
                }
            }

            SetRootContent(_rowListView);
        }

        public override void Init()
        {
            base.Init();
        }
    }
}
