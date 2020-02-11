using System;
using System.Threading.Tasks;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class ExportSecretKeyPage : StackPage
    {
        readonly SecretKey _secretKey;
        readonly EntryRow _password;
        readonly EditorRow _key;

        async Task Export(ButtonRow button)
        {
            var pw = _password.Edit.Text;
            if (pw == null)
                pw = string.Empty;

            if(string.IsNullOrEmpty(pw))
            {
                if(!await ConfirmAsync("ConfirmEmptyExport"))
                {
                    return;
                }
            }

            var exportedSecret = _secretKey.ExportSecretKey(pw);

            var text = $"{Tr.Get("App.FullName")} {Tr.Get("Common.SecretKey")} {_secretKey.KeyInfo.SecretId}|{exportedSecret.HexString}";

            _key.Edit.Text = text;
            UIApp.CopyToClipboard(text);

            await MessageAsync("Success");
        }

        public ExportSecretKeyPage(SecretKey secretKey) : base("ExportSecretKeyPage")
        {
            _secretKey = secretKey;

            AddTitleRow("Title");

            AddHeaderRow();

            var view = new SecretKeyView(secretKey.KeyInfo, true);
            AddViewRow(view);

            _key = AddEditorRow(null, null);
            _key.SetDetailViewIcon(Icons.Key);
            AddFooterRow();

            AddIndex = AddSubmitRow("ExportButton", Export);
            AddIndexBefore = true;
            _password = AddPasswordRow("", "Password");
        }
    }
}
