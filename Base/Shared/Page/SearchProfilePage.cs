using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Heleus.ProfileService;

namespace Heleus.Apps.Shared
{
    public class SearchProfilePage : StackPage
    {
        protected readonly EntryRow _search;
        readonly ProfileSearch _profileSearch;

        async Task Search(ButtonRow button)
        {
            if (IsBusy)
                return;
            IsBusy = true;

            var profiles = await _profileSearch.Search(_search.Edit.Text);

            RemoveHeaderSection("Result");

            AddHeaderRow("Result");

            if (profiles.Count == 0)
            {
                AddInfoRow("NotFound");
            }
            else
            {
                foreach (var profile in profiles)
                {
                    AddIndex = AddRow(new ProfileButtonRow(profile, ProfileButton));
                }
            }

            AddFooterRow();
            IsBusy = false;

            UpdateSuspendedLayout();
        }

        protected virtual async Task ProfileButton(ProfileButtonRow arg)
        {
            await Navigation.PushAsync(new ViewProfilePage(arg.AccountId));
        }

        public SearchProfilePage(ProfileSearch profileSearch) : base("SearchProfilePage")
        {
            IsSuspendedLayout = true;
            _profileSearch = profileSearch;

            AddTitleRow("Title");

            AddHeaderRow("Search");

            _search = AddEntryRow("", "AccountId");
            _search.SetDetailViewIcon(Icons.Coins);

            var submit = AddSubmitRow("SearchButton", Search, false);
            submit.IsEnabled = false;

            _search.Edit.TextChanged += (sender, e) =>
            {
                submit.IsEnabled = _profileSearch.IsValidSearchText(e.NewTextValue);
            };

            AddFooterRow();
        }
    }
}
