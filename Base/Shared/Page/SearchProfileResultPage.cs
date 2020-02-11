using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Heleus.ProfileService;

namespace Heleus.Apps.Shared
{
    public class SearchProfileResultPage : StackPage
    {
        public SearchProfileResultPage(IReadOnlyList<ProfileInfo> profiles, Func<ProfileButtonRow, Task> action) : base("SearchProfileResultPage")
        {
            AddTitleRow("Title");

            AddHeaderRow("Result");

            if (profiles.Count == 0)
            {
                AddInfoRow("NotFound");
            }
            else
            {
                foreach (var profile in profiles)
                {
                    AddIndex = AddRow(new ProfileButtonRow(profile, action));
                }
            }

            AddFooterRow();
        }
    }
}
