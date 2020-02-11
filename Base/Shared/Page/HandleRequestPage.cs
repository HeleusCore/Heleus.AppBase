using System;
using System.Threading.Tasks;

namespace Heleus.Apps.Shared
{
    public class HandleRequestPage : StackPage
    {
        public const char HandleRequestIcon = Icons.Cogs;
        public static string HandleRequestTranslation = "HandleRequestPage.Title";

        readonly EditorRow _request;
        readonly EntryRow _password;

        async Task ProcessRequest(ButtonRow button)
        {
            try
            {
                var schemeAction = SchemeAction.ParseSchemeAction(new Uri(_request.Edit.Text));
                if (schemeAction != null)
                {
                    if(schemeAction.RequiresPassword)
                    {
                        var pw = _password.Edit.Text;
                        if(string.IsNullOrWhiteSpace(pw))
                        {
                            await ErrorAsync("PasswordMissing");
                            return;
                        }

                        if(!await schemeAction.Decrypt(pw))
                        {
                            await ErrorAsync("PasswordWrong");
                            return;
                        }
                    }
                    await schemeAction.Run();
                }
                else
                    await ErrorAsync("Failure");
            }
            catch { }
        }

        public HandleRequestPage(string request = null) : base("HandleRequestPage")
        {
            AddTitleRow("Title");

            _request = AddEditorRow(request, "Link");

            Status.Add(_request.Edit, T("LinkStatus"), (sv, edit, newText, oldText) =>
            {
                try
                {
                    var schemeAction = SchemeAction.ParseSchemeAction(new Uri(newText));
                    _password.Edit.IsEnabled = schemeAction != null && schemeAction.RequiresPassword;

                    return schemeAction != null;
                }
                catch { }

                return false;
            });

            AddIndex = AddSubmitRow("Submit", ProcessRequest);
            AddIndexBefore = true;

            _password = AddPasswordRow(null, "Password");
            _password.Edit.IsEnabled = false;

            Status.ReValidate();
        }
    }
}
