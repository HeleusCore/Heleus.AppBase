using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Heleus.Base;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public class SuspendableStackLayout : StackLayout
    {
        //bool _hasRenderer;

        public bool Suspended;

        protected override bool ShouldInvalidateOnChildAdded(View child)
        {
            if (!Suspended)
                return base.ShouldInvalidateOnChildAdded(child);

            return false;
        }

        protected override bool ShouldInvalidateOnChildRemoved(View child)
        {
            if (!Suspended)
                return base.ShouldInvalidateOnChildRemoved(child);

            return false;
        }

        protected override void OnChildMeasureInvalidated()
        {
            if (!Suspended)
                base.OnChildMeasureInvalidated();
        }

        protected override void InvalidateLayout()
        {
            if (!Suspended)
                base.InvalidateLayout();
        }

        protected override void InvalidateMeasure()
        {
            if (!Suspended)
                base.InvalidateMeasure();
        }

        public void UpdateSuspendableLayout(bool invalidateLayout = false)
        {
            if (!Suspended)
                return;

            base.InvalidateLayout();

            /*
            var newRenderer = false;
            if (!_hasRenderer)
            {
                _hasRenderer = this.GetPropertyIfSet<object>(UIApp.RenderProperty, null) != null;
                newRenderer = _hasRenderer;
            }

            var p = Parent?.Parent?.Parent as Page;

            //Console.WriteLine($"_hasRenderer {_hasRenderer}, newRenderer {newRenderer}, {p?.GetType()}");
            Console.WriteLine($"{Height}, {p.Height}");

            base.InvalidateLayout();
            return;
            /*
            if (_hasRenderer)
            {
                if (!_hasRenderer || invalidateLayout)
                {
                    base.InvalidateLayout();
                    base.InvalidateMeasure();
                }
                else
                {
                    base.InvalidateMeasure();
                }
            }
            */
        }
    }

    public class StackPage : ExtContentPage
    {
        public SuspendableStackLayout StackLayout
        {
            get;
            private set;
        } = new SuspendableStackLayout();

        public bool IsSuspendedLayout
        {
            get => StackLayout.Suspended;
            set => StackLayout.Suspended = value;
        }

        public void UpdateSuspendedLayout(bool invalidateLayout = false)
        {
            StackLayout.UpdateSuspendableLayout(invalidateLayout);
        }

        protected ExtScrollView ScrollView
        {
            get;
            private set;
        } = new ExtScrollView();

        public View AddIndex;
        public bool AddIndexBefore;

        StatusView _status;
        protected StatusView Status
        {
            get
            {
                if (_status == null)
                    _status = new StatusView();
                return _status;
            }
        }

        protected StatusView EnableStatus()
        {
            return Status;
        }

        public StackPage(string name, bool lazyInit = false) : base(name)
        {
			var compressLayout = (UIApp.IsAndroid || UIApp.IsMacOS);
            var inputTransparent = (UIApp.IsMacOS);

            //if (compressLayout)
            //	CompressedLayout.SetIsHeadless(StackLayout, true);
            StackLayout.InputTransparent = inputTransparent;

            if (UIApp.IsMacOS)
                ScrollView.InputTransparent = true;

            StackLayout.Spacing = UIApp.IsDesktop ? 5 : 10;
            StackLayout.Padding = new Thickness(0);
            ScrollView.Padding = new Thickness(0);

            ScrollView.Content = StackLayout;

#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor
            SetupPadding();
#pragma warning restore RECS0021 // Warns about calls to virtual member functions occuring in the constructor

            StackLayout.SizeChanged += StackLayout_SizeChanged;

            if (!lazyInit || UIApp.IsMacOS)
                SetRootContent();
        }

        public new bool IsBusy
        {
            set
            {
                base.IsBusy = value;
                Loading = value;
                if (_status != null)
                    _status.IsBusy = value;
            }

            get
            {
                return base.IsBusy;
            }
        }

        public async Task ScrollToTop()
        {
            await ScrollView.ScrollToAsync(0, 0, true);
        }

        protected override void UpdateColors()
        {
            base.UpdateColors();

            if (UIApp.IsGTK)
                ScrollView.BackgroundColor = Theme.PrimaryColor.Color;
        }

        protected virtual void SetupPadding()
        {
            var desiredWith = Screen.StackPageWidth;
            var screenWidth = Screen.CurrentScreenWidth;

            var desiredPadding = (int)((screenWidth - desiredWith) / 2.0);
            var padding = (int)StackLayout.Padding.Left;

            if (desiredPadding != padding)
                StackLayout.Padding = new Thickness(desiredPadding, 0, desiredPadding, 0);
        }

        void StackLayout_SizeChanged(object sender, EventArgs e)
        {
            SetupPadding();
        }

        protected void SetRootContent()
        {
            SetRootContent(ScrollView);
        }

        public void RemoveView(View view)
        {
            if (view == null)
                return;

            StackLayout.Children.Remove(view);
        }

        public void RemoveHeaderSection(string name)
        {
            RemoveHeaderSection(GetRow<HeaderRow>(name));
        }

        public void RemoveHeaderSection(HeaderRow headerRow)
        {
            if (headerRow == null)
                return;

            var idx = StackLayout.Children.IndexOf(headerRow);
            if (idx >= 0)
            {
                int headerCount = 0;
                while (true)
                {
                    if (idx >= StackLayout.Children.Count)
                        break;

                    var row = StackLayout.Children[idx];
                    if (row is HeaderRow)
                        headerCount++;
                    StackLayout.Children.RemoveAt(idx);

                    if (headerCount >= 2)
                        break;
                }
            }
        }

        public List<T> GetHeaderSectionRows<T>(string name) where T : StackRow
        {
            return GetHeaderSectionRows<T>(GetRow<HeaderRow>(name));
        }

        public List<StackRow> GetHeaderSectionRows(string name)
        {
            return GetHeaderSectionRows(GetRow<HeaderRow>(name));
        }

        public List<T> GetHeaderSectionRows<T>(HeaderRow headerRow) where T : StackRow
        {
            var result = new List<T>();

            if (headerRow != null)
            {
                var idx = StackLayout.Children.IndexOf(headerRow);
                if (idx >= 0)
                {
                    while (true)
                    {
                        idx += 1;

                        if (idx >= StackLayout.Children.Count)
                            break;

                        if (StackLayout.Children[idx] is HeaderRow)
                        {
                            break;
                        }

                        var stackRow = StackLayout.Children[idx] as T;

                        if (stackRow != null)
                        {
                            result.Add(stackRow);
                        }
                    }
                }
            }

            return result;
        }

        public List<StackRow> GetHeaderSectionRows(HeaderRow headerRow) 
        {
            var result = new List<StackRow>();

            if (headerRow != null)
            {
                var idx = StackLayout.Children.IndexOf(headerRow);
                if (idx >= 0)
                {
                    while (true)
                    {
                        idx += 1;

                        if (idx >= StackLayout.Children.Count)
                            break;

                        if (StackLayout.Children[idx] is HeaderRow)
                        {
                            break;
                        }

                        if (StackLayout.Children[idx] is StackRow stackRow)
                        {
                            result.Add(stackRow);
                        }
                    }
                }
            }

            return result;
        }

        public void ClearHeaderSection(string name)
        {
            ClearHeaderSection(GetRow<HeaderRow>(name), null);
        }

        public void ClearHeaderSection(string name, Func<StackRow, bool> rem)
        {
            ClearHeaderSection(GetRow<HeaderRow>(name), rem);
        }

        public void ClearHeaderSection(HeaderRow headerRow)
        {
            ClearHeaderSection(headerRow, null);
        }

        public void ClearHeaderSection(HeaderRow headerRow, Func<StackRow, bool> rem)
        {
            if (headerRow == null)
                return;

            var idx = StackLayout.Children.IndexOf(headerRow);
            if (idx >= 0)
            {
                idx += 1;
                while (true)
                {
                    if (idx >= StackLayout.Children.Count)
                        break;

                    if (StackLayout.Children[idx] is HeaderRow)
                    {
                        break;
                    }

                    if (rem != null && StackLayout.Children[idx] is StackRow stackRow)
                    {
                        if (rem.Invoke(stackRow))
                        {
                            StackLayout.Children.RemoveAt(idx);
                        }
                        else
                        {
                            idx++;
                        }
                    }
                    else
                    {
                        StackLayout.Children.RemoveAt(idx);
                    }
                }
            }
        }

        public void AddView(View view)
        {
            if (AddIndex != null)
            {
                var idx = StackLayout.Children.IndexOf(AddIndex);
                if (idx >= 0)
                {
                    if (AddIndexBefore)
                        StackLayout.Children.Insert(idx, view);
                    else
                        StackLayout.Children.Insert(idx + 1, view);
                }
                else
                {
                    StackLayout.Children.Add(view);
                }
            }
            else
            {
                StackLayout.Children.Add(view);
            }

            //StackLayout.Children.Add( new BoxView { HeightRequest = 10, Color = Color.Transparent});
        }

        public StackRow GetRow(string identifier)
        {
            foreach (var item in StackLayout.Children)
            {
                if (item is StackRow row && row.Identifier == identifier)
                    return row;
            }
            return null;
        }

        public T GetRow<T>(string identifier) where T : StackRow
        {
            foreach (var item in StackLayout.Children)
            {
                if (item is T && item is StackRow row && row.Identifier == identifier)
                    return (T)row;
            }
            return default(T);
        }

        public SeparatorRow AddSeparatorRow(bool translucent = false)
        {
            var row = new SeparatorRow(null, translucent);

            AddView(row);

            return row;
        }

        public SeparatorRow AddSeparatorRow(int minHeight, bool translucent = false)
        {
            var row = new SeparatorRow(null, translucent, minHeight);

            AddView(row);

            return row;
        }

        public TextRow AddInfoRow(string translation = null, params object[] parameters)
        {
            var row = new TextRow
            {
                Identifier = translation
            };
            var label = row.Label;
            if (!string.IsNullOrEmpty(translation))
                label.Text = T(translation, parameters);

            AddView(row);
            return row;
        }

        public LabelRow AddTextRow(string translation, params object[] parameters)
        {
            var row = new LabelRow()
            {
                Identifier = translation,
                LabelPadding = 15
            };

            row.Label.Margin = new Thickness(15, 5, 15, 5);
            //row.LabelPadding = 30;

            var label = row.Label;
            if (!string.IsNullOrEmpty(translation))
                label.Text = T(translation, parameters);

            label.FontStyle = Theme.TextFont;
            AddView(row);

            return row;
        }

        HeaderRow AddHeaderFooter(HeaderRowType headerRowType, string translation = null, params object[] parameters)
        {
            var row = new HeaderRow(headerRowType)
            {
                Identifier = translation
            };

            if (headerRowType != HeaderRowType.Invisible)
            {
                var label = row.Label;
                if (!string.IsNullOrEmpty(translation))
                    label.Text = T(translation, parameters);
            }

            AddView(row);
            return row;
        }

        public HeaderRow AddTitleRow(string translation = null, params object[] parameters)
        {
            var row = AddHeaderFooter(HeaderRowType.Title, translation, parameters);
            var label = row.Label;

            label.Text = label.Text?.ToUpper();

            return row;
        }

        public HeaderRow AddInvisibleHeaderRow(string identifier)
        {

            var row = AddHeaderFooter(HeaderRowType.Invisible, identifier);
            return row;
        }

        public HeaderRow AddHeaderRow(string translation = null, bool upper = false, params object[] parameters)
        {
            var row = AddHeaderFooter(HeaderRowType.Header, translation, parameters);
            var label = row.Label;

            if (upper)
                label.Text = label.Text?.ToUpper();

            return row;
        }

        public HeaderRow AddFooterRow(string translation = null, params object[] parameters)
        {
            var row = AddHeaderFooter(HeaderRowType.Footer, translation, parameters);
            row.Identifier = translation;

            return row;
        }

        public IconLabelRow AddIconRow(string translation, char icon, params object[] parameters)
        {
            var row = new IconLabelRow(Icons.RowMore)
            {
                Identifier = translation
            };
            var label = row.Label;
            if (!string.IsNullOrEmpty(translation))
                label.Text = T(translation, parameters);

            row.FontIcon.Icon = icon;

            AddView(row);
            return row;
        }

        public ImageRow AddImageRow(double aspect, string identifier = null)
        {
            var row = new ImageRow(aspect)
            {
                Identifier = identifier
            };

            AddView(row);
            return row;
        }

        public ButtonRow AddButtonRow(string translation, Func<ButtonRow, Task> action, params object[] parameters)
        {
            var row = new ButtonRow(Icons.RowMore, action)
            {
                Identifier = translation
            };

            var label = row.Label;
            if (!string.IsNullOrEmpty(translation))
                label.Text = T(translation, parameters);

            AddView(row);
            return row;
        }

        public T AddRow<T>(T stackRow, string identifier = null) where T : StackRow
        {
            stackRow.Identifier = identifier;
            AddView(stackRow);

            return stackRow;
        }

        public ButtonViewRow<T> AddButtonViewRow<T>(T view, Func<ButtonViewRow<T>, Task> action, bool addMargin = true, string identifier = null) where T : View
        {
            var row = new ButtonViewRow<T>(Icons.RowMore, action, view, addMargin)
            {
                Identifier = identifier
            };

            //row.RowLayout.Children.Clear();

            AddView(row);
            return row;
        }

        public ButtonRow AddSubmitButtonRow(string translation, Func<ButtonRow, Task> action, params object[] parameters)
        {
            var row = AddButtonRow(translation, action, parameters);
            row.FontIcon.IsVisible = false;
            row.RowStyle = Theme.SubmitButton;
            row.SetDetailViewIcon(Icons.RowSubmit);

            return row;
        }

        public ButtonRow AddSubmitRow(string translation, Func<ButtonRow, Task> action, bool addHeaderAndFooter = true, params object[] parameters)
        {
            if (_status != null)
                AddViewRow(_status);

            if (addHeaderAndFooter)
				AddHeaderRow();

            var row = AddSubmitButtonRow(translation, action, parameters);

			if (addHeaderAndFooter)
				AddFooterRow();
			
            if(_status != null)
                _status.ValidHandler = (sv, valid) => row.IsEnabled = valid && !sv.IsBusy;

            return row;
        }

		Task LinkTapped(ButtonRow row)
		{
			var url = row.Tag as string;
			if (!string.IsNullOrEmpty(url))
			{
				try
				{
                    UIApp.OpenUrl(new Uri(url));
				}
				catch (Exception ex)
				{
                    Log.IgnoreException(ex);
				}
			}
			return Task.FromResult(true);
		}

		public ButtonRow AddLinkRow(string translation, string url, params object[] parameters)
		{
			var row = AddButtonRow(translation, LinkTapped, parameters);
			row.Tag = url;
			row.FontIcon.Icon = Icons.RowLink;
			return row;
		}

        public SwitchRow<SW> AddSwitchRow<SW>(string translation, params object[] parameters) where SW : ExtSwitch, new()
        {
            var row = new SwitchRow<SW>(Icons.RowMore)
            {
                Identifier = translation
            };
            var label = row.Label;
			if (!string.IsNullOrEmpty(translation))
				label.Text = T(translation, parameters);

            if (_status != null)
            {
                _status.AddBusyView(row);
            }

            AddView(row);
            return row;
        }

        public SwitchRow AddSwitchRow(string translation, params object[] parameters)
        {
            var row = new SwitchRow(Icons.RowMore)
            {
                Identifier = translation
            };
            var label = row.Label;
			if (!string.IsNullOrEmpty (translation))
				label.Text = T (translation, parameters);

            if (_status != null)
            {
                _status.AddBusyView(row);
            }

            AddView(row);
			return row;
		}

		public EntryRow AddEntryRow(string text, string translation, params object[] parameters)
		{
			var row = new EntryRow(Icons.RowMore)
			{
				Identifier = translation
			};

			var entry = row.Edit;
			//entry.Keyboard = Keyboard.Email;
			entry.Text = text;
			if (!string.IsNullOrEmpty(translation))
				entry.Placeholder = T(translation, parameters);

            if (_status != null)
            {
                _status.AddBusyView(row);
            }

            AddView(row);
			return row;
		}

        public EntryRow AddPasswordRow(string text, string translation, params object[] parameters)
        {
            var row = AddEntryRow(text, translation, parameters);

            row.SetDetailViewIcon(Icons.Unlock);
            row.Edit.IsPassword = true;

            return row;
        }

        public EditorRow AddEditorRow(string text, string translation, params object[] parameters)
		{
            var row = new EditorRow
            {
                Identifier = translation
            };

            var editor = row.Edit;
			editor.Text = text;

            if (!string.IsNullOrEmpty(translation))
                editor.Placeholder = T(translation, parameters);

            //editor.WidthRequest = editor.HeightRequest = 10;

            if(_status != null)
            {
                _status.AddBusyView(row);
            }

            AddView(row);
			return row;
		}

		public SliderRow AddSliderRow(int value, int minValue, int maxValue, string identifier = null)
        {
            var row = new SliderRow(value, minValue, maxValue)
            {
                Identifier = identifier
            };

            if (_status != null)
                _status.AddBusyView(row);

            AddView(row);

            return row;
        }

        public SimpleSliderRow AddSimpleSliderRow(int value, int minValue, int maxValue, string identifier = null)
        {
            var row = new SimpleSliderRow(value, minValue, maxValue)
            {
                Identifier = identifier
            };

            if (_status != null)
                _status.AddBusyView(row);

            AddView(row);

			return row;
		}

        public ViewRow AddViewRow(View view, bool addMargin = true, string identifier = null)
		{
            var row = new ViewRow(view, addMargin)
            {
                Identifier = identifier
            };

			AddView(row);

			return row;
		}

        public SelectionRow<T> AddSelectionRows<T>(IEnumerable<SelectionItem<T>> items, T selection, string identifier = null)
        {
            var row = new SelectionRow<T>(Icons.RowSelected)
            {
                Identifier = identifier
            };

            row.SetupSelectionRow(this, items);
            row.Selection = selection;

            if (_status != null)
            {
                foreach (var button in row.Buttons)
                {
                    _status.AddBusyView(button);
                }
            }

            return row;
        }

        public PickerRow<T> AddPickerRow<T>(SelectionItemList<T> items, T selection, string identifier = null)
		{
			var row = new PickerRow<T>(Icons.RowMore);
			row.FontIcon.Icon = Icons.RowPicker;

			row.Identifier = identifier;

			var picker = row.Picker;
			picker.Items = items;
			picker.Selection = selection;

			AddView(row);

			return row;
		}
    }
}
