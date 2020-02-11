using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public interface IStatusViewItem
    {
        void ReValidate();
        bool Valid { get; }
    }

    public class StatusView : StackLayout
	{
        class GenericItem : IStatusViewItem
        {
            public readonly ExtLabel Message;
            protected readonly Func<bool> _validate;

            readonly StatusView _errorView;
            bool _valid;

            public bool Valid
            {
                get => _valid;

                set
                {
                    _valid = value;
                    if (!_valid)
                    {
                        Message.TextColor = _errorView.InvalidColor;
                    }
                    else
                    {
                        Message.TextColor = _errorView.ValidColor;
                    }

                    _errorView.ValidHandler?.Invoke(_errorView, _errorView.Valid);
                }
            }

            public void ReValidate()
            {
                if (_validate != null)
                    Valid = _validate.Invoke();
            }

            public GenericItem(ExtLabel message, StatusView errorView, Func<bool> validate)
            {
                Message = message;
                _errorView = errorView;

                _validate = validate;
                if(validate != null)
                    Valid = validate.Invoke();
            }
        }

        class EntryItem : GenericItem
		{
			public readonly ExtEntry Entry;

			public EntryItem(ExtEntry entry, ExtLabel message, StatusView errorView, Func<bool> validate) : base(message, errorView, validate)
			{
				Entry = entry;
			}
        }

        class EditorItem : GenericItem
        {
            public readonly ExtEditor Entry;

            public EditorItem(ExtEditor entry, ExtLabel message, StatusView errorView, Func<bool> validate) : base(message, errorView, validate)
            {
                Entry = entry;
            }
        }

        class SwitchItem : GenericItem
        {
            public readonly ExtSwitch Switch;

            public SwitchItem(ExtSwitch swt, ExtLabel message, StatusView errorView, Func<bool> validate) : base(message, errorView, validate)
            {
                Switch = swt;
            }
        }

        readonly List<GenericItem> _statusItems = new List<GenericItem>();
        readonly List<View> _busyViews = new List<View>();

        Color _validColor = Theme.TextColor.Color;
        Color _invalidColor = Color.Red;

        Action<StatusView, bool> _validHandler;
        public Action<StatusView, bool> ValidHandler
        {
            get => _validHandler;
            set
            {
                _validHandler = value;
                _validHandler?.Invoke(this, Valid);
            }
        }

        public Color ValidColor
        {
            get => _validColor;

            set
            {
                _validColor = value;
                foreach (var item in _statusItems)
                {
                    if (item.Valid)
                        item.Message.TextColor = value;
                }
            }
        }

        public Color InvalidColor
        {
            get => _invalidColor;

            set
            {
                _invalidColor = value;
                foreach (var item in _statusItems)
                {
                    if (!item.Valid)
                        item.Message.TextColor = value;
                }
            }
        }

        public StatusView()
		{
			//CompressedLayout.SetIsHeadless(this, true);
			InputTransparent = true;
			Spacing = 4;

            SizeChanged += StatusView_SizeChanged;
        }

        void StatusView_SizeChanged(object sender, EventArgs e)
        {
            var w = (int)Width;
            if (Width <= 0)
                return;

            foreach(var item in _statusItems)
            {
                if (w != (int)item.Message.WidthRequest)
                    item.Message.WidthRequest = w;
            }
        }

        ExtLabel AddMessage(string text)
        {
            var message = new ExtLabel { Text = text, FontStyle = Theme.DetailFont, InputTransparent = true };

            Children.Add(message);

            return message;
        }

        public bool Valid
		{
			get
			{
				foreach(var item in _statusItems)
				{
					if (!item.Valid)
						return false;
				}

				return true;
			}
		}

        bool _busy;
        public bool IsBusy
        {
            get => _busy;

            set
            {
                _busy = value;
                foreach(var view in _busyViews)
                {
                    if (view is StackRow row)
                        row.IsEnabled = !value;
                    else
                        view.IsEnabled = !value;
                }

                ReValidate();
            }
        }

        public void ReValidate()
        {
            foreach(var item in _statusItems)
            {
                item.ReValidate();
            }
        }

        public StatusView ClearBusyViews()
        {
            _busyViews.Clear();
            return this;
        }

        public StatusView AddBusyView(View view)
        {
            _busyViews.Add(view);
            return this;
        }

        public StatusView RemoveBusyView(View view)
        {
            _busyViews.Remove(view);
            return this;
        }

        public StatusView Add(ExtEntry entry, string text, Func<StatusView, ExtEntry, string, string, bool> validator)
		{
            var message = AddMessage(text);

            var item = new EntryItem(entry, message, this, () =>
            {
                return validator.Invoke(this, entry, entry.Text, entry.Text);
            });
			_statusItems.Add(item);

			entry.TextChanged += (sender, e) =>
			{
				item.Valid = validator.Invoke(this, entry, e.NewTextValue, e.OldTextValue);
			};
            AddBusyView(entry);
            return this;
		}

        public StatusView Add(ExtEditor editor, string text, Func<StatusView, ExtEditor, string, string, bool> validator)
        {
            var message = AddMessage(text);

            var item = new EditorItem(editor, message, this, () =>
            {
                return validator.Invoke(this, editor, editor.Text, editor.Text);
            });

            _statusItems.Add(item);

            editor.TextChanged += (sender, e) =>
            {
                item.Valid = validator.Invoke(this, editor, e.NewTextValue, e.OldTextValue);
            };

            AddBusyView(editor);
            return this;
        }

        public StatusView Add(ExtSwitch swt, string text, Func<StatusView, ExtSwitch, bool, bool> validator)
        {
            var message = AddMessage(text);
            var item = new SwitchItem(swt, message, this, () =>
            {
                return validator.Invoke(this, swt, swt.IsToggled);
            });

            _statusItems.Add(item);

            swt.Toggled = (sw) =>
            {
                item.Valid = validator.Invoke(this, swt, swt.IsToggled);
            };

            AddBusyView(swt);

            return this;
        }

        public IStatusViewItem Add(string text, Func<StatusView, bool> validator)
        {
            var message = AddMessage(text);
            var item = new GenericItem(message, this, () =>
            {
                return validator.Invoke(this);
            });
            _statusItems.Add(item);

            return item;
        }
    }
}
