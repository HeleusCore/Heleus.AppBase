using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public class SubmitAccountRow : LabelRow
    {
        public static void UpdateRow(LabelRow row, SubmitAccount submitAccount)
        {
            row.RowLayout.SetAccentColor(submitAccount != null ? submitAccount.ServiceNode.AccentColor : Color.Transparent);

            var name = new Span
            {
                Text = submitAccount != null ? submitAccount.Name : "-"
            };
            name.SetStyle(Theme.TextFont, Theme.TextColor);

            var detail = new Span
            {
                Text = submitAccount != null ? $"\n{submitAccount.Detail}" : ""
            };
            detail.SetStyle(Theme.MicroFont, Theme.TextColor);

            row.Label.Margin = new Thickness(row.Label.Margin.Left, 2, row.Label.Margin.Right, 2);

            row.Label.FormattedText = new FormattedString
            {
                Spans =
                {
                    name, detail
                }
            };
        }

        public SubmitAccountRow(SubmitAccount submitAccount)
        {
            UpdateRow(this, submitAccount);
        }
    }

    public class SubmitAccountButtonRow<T> : ButtonRow where T : SubmitAccount
    {
        readonly ExtContentPage _page;
        readonly string _lastUsedSubmitAccountKey;
        readonly Func<List<T>> _select;

        public Func<SubmitAccountButtonRow<T>, Task> SelectionChanged;

        T _submitAccount;
        public T SubmitAccount
        {
            get => _submitAccount;

            set
            {
                var changed = value != _submitAccount;
                _submitAccount = value;
                if (changed)
                {
                    UpdateButton();
                    UIApp.Run(async () =>
                    {
                        if (SelectionChanged != null)
                            await SelectionChanged.Invoke(this);
                    });
                }
            }
        }

        void UpdateButton()
        {
            SubmitAccountRow.UpdateRow(this, _submitAccount);
        }

        async Task SelectSubmitAccount(ButtonRow button)
        {
            await _page.Navigation.PushAsync(new SubmitAccountsPage<T>(_select.Invoke(), (submitAccount) =>
            {
                SubmitAccount = submitAccount;
                AppBase.Current.SetLastUsedSubmitAccount(submitAccount, _lastUsedSubmitAccountKey);
            }));
        }

        SubmitAccountButtonRow(T submitAccount, ExtContentPage page) : base(Icons.RowMore, null)
        {
            SetDetailViewIcon(Icons.Server);
            Label.Margin = new Thickness(Label.Margin.Left, 5, Label.Margin.Right, 5);

            _submitAccount = submitAccount;
            UpdateButton();

            page.Subscribe<ServiceNodeChangedEvent>(ServiceNodeChanged);
            page.Subscribe<AccountRenamedEvent>(AccountRenamed);
            _page = page;
        }

        public SubmitAccountButtonRow(T submitAccount, ExtContentPage page, Func<SubmitAccountButtonRow<T>, Task> action) : this(submitAccount, page)
        {
            ButtonAction = async (button) =>
            {
                if (action != null)
                    await action.Invoke(this);
            };
        }

        public SubmitAccountButtonRow(ExtContentPage page, Func<List<T>> select, string lastUsedSubmitAccountKey = "default") : this(AppBase.Current.GetLastUsedSubmitAccount<T>(lastUsedSubmitAccountKey), page)
        {
            ButtonAction = SelectSubmitAccount;
            _select = select;
            _lastUsedSubmitAccountKey = lastUsedSubmitAccountKey;

            if(_submitAccount == null)
            {
                _submitAccount = select.Invoke().FirstOrDefault();
                UpdateButton();
            }
        }

        Task AccountRenamed(AccountRenamedEvent arg)
        {
            UpdateButton();
            return Task.CompletedTask;
        }

        Task ServiceNodeChanged(ServiceNodeChangedEvent arg)
        {
            UpdateButton();
            return Task.CompletedTask;
        }
    }

    public class SubmitAccountButtonRow : SubmitAccountButtonRow<SubmitAccount>
    {
        public SubmitAccountButtonRow(SubmitAccount submitAccount, ExtContentPage page, Func<SubmitAccountButtonRow<SubmitAccount>, Task> action) : base(submitAccount, page, action)
        {
        }

        public SubmitAccountButtonRow(ExtContentPage page, Func<List<SubmitAccount>> select, string lastUsedSubmitAccountKey = "default") : base(page, select, lastUsedSubmitAccountKey)
        {
        }
    }
}
