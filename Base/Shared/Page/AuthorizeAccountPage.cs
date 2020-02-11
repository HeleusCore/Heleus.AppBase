using System;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class AuthorizeAccountPage : StackPage
    {
        readonly int _minPassphraseLength;
        readonly ServiceNode _serviceNode;
        readonly int _popCount;
        readonly EditorRow _keyRow;
        readonly KeyView _keyView;

        readonly SelectionRow<AuthorizeType> _selectionRow;

        public AuthorizeAccountPage(ServiceNode serviceNode, int minPassphraseLength, int popCount) : base("AuthorizeAccountPage")
        {
            Subscribe<ServiceAccountImportEvent>(AccountImport);
            Subscribe<ServiceAccountAuthorizedEvent>(AccountAuth);

            Subscribe<ResumeEvent>(Resume);

            _minPassphraseLength = minPassphraseLength;
            _serviceNode = serviceNode;
            _popCount = popCount;

            AddTitleRow("Title");

            EnableStatus();

            AddHeaderRow("Options");

            AddFooterRow();

            var sm = AddSubmitRow("Authorize", Authorize);
            //AddIndex = sm;
            //AddInfoRow("RequestInfo", Tr.Get("App.FullName"));

            AddIndex = null;

            AddHeaderRow("AuthorizeType");

            _selectionRow = AddSelectionRows(new SelectionItemList<AuthorizeType>
            {
                new SelectionItem<AuthorizeType>(AuthorizeType.Derived, Tr.Get("AuthorizeType.Derived")),
                new SelectionItem<AuthorizeType>(AuthorizeType.Passphrase, Tr.Get("AuthorizeType.Passphrase")),
                new SelectionItem<AuthorizeType>(AuthorizeType.Random, Tr.Get("AuthorizeType.Random"))
            }, AuthorizeType.Derived);
            _selectionRow.SelectionChanged = SecretTypeChanged;

            AddInfoRow("SignatureKeyTypeInfo");

            AddFooterRow();


            AddHeaderRow("SignatureKey");
            _keyView = new KeyView(null, true);
            AddViewRow(_keyView);

            _keyRow = AddEditorRow("", "SignatureKey");
            _keyRow.SetDetailViewIcon(Icons.Key);

            Status.Add(_keyRow.Edit, T("KeyStatus"), (sv, edit, newText, oldText) =>
            {
                if (StatusValidators.HexValidator(64, sv, edit, newText, oldText))
                {
                    var seed = Hex.FromString(newText);
                    var key = Key.GenerateEd25519(seed);
                    _keyView.Update(key);

                    return true;
                }

                _keyView.Update(null);
                return false;
            });

            AddInfoRow("SignatureKeyInfo");

            AddFooterRow();

            Update();
        }

        async Task Resume(ResumeEvent arg)
        {
            var cp = await UIApp.CopyFromClipboard();
            if(cp != null)
            {
                var data = Hex.ByteArrayFromCrcString(cp);
                if (data == null)
                    return;

                var derivedKey = GetDerivedKey(cp);
                if (derivedKey.ChainId == _serviceNode.ChainId)
                {
                    await Paste(null);
                    if(await ConfirmTextAsync(T("AuthDerivedConfirm", Tr.Get("App.FullName"))))
                    {
                        await Authorize(null);
                    }
                }
            }
        }

        async Task Done()
        {
            await MessageTextAsync(T("Success", Tr.Get("App.FullName")));
            await PopAsync(_popCount);
        }

        async Task AccountAuth(ServiceAccountAuthorizedEvent arg)
        {
            await Done();
        }

        async Task AccountImport(ServiceAccountImportEvent arg)
        {
            await Done();
        }

        async Task Authorize(ButtonRow arg)
        {
            var seed = Hex.FromString(_keyRow.Edit.Text);
            var key = Key.GenerateEd25519(seed);

            if (_selectionRow.Selection == AuthorizeType.Derived)
            {
                var derivedKey = GetDerivedKey(GetRow<EditorRow>("DerivedKeyInfo").Edit.Text);
                if(derivedKey != null)
                {
                    var result = await _serviceNode.RequestRestore(derivedKey.AccountId, key);
                    if (result != RestoreResult.Ok) // okay is handled by Done()
                    {
                        if (result == RestoreResult.AlreadyAvailable)
                        {
                            (var sn, var ac) = ServiceNodeManager.Current.GetServiceAccount(key);
                            await ErrorTextAsync(Tr.Get("RestoreResult.AlreadyAvailable", sn.GetName(), ac.Name));
                        }
                        else
                        {
                            await ErrorTextAsync(Tr.Get("RestoreResult." + result));
                        }
                    }
                }
                else
                {
                    await ErrorAsync("InvalidDerivedKey");
                }
            }
            else
            {
                if (!await ConfirmAsync("ConfirmAuth"))
                    return;

                IsBusy = true;

                var result = await _serviceNode.RequestAuthorization(key);
                if (result == AuthorizationResult.NotConnected)
                {
                    await ErrorAsync(".AuthorizationResult.NotConnected");
                }
                else if (result == AuthorizationResult.AlreadyAvailable)
                {
                    (var sn, var ac) = ServiceNodeManager.Current.GetServiceAccount(key);
                    await ErrorTextAsync(Tr.Get("AuthorizationResult.AlreadyAvailable", sn.GetName(), ac.Name));
                }

                IsBusy = false;
            }
        }

        void Update()
        {
            var options = GetRow<HeaderRow>("Options");
            ClearHeaderSection(options);

            _keyRow.Edit.Text = string.Empty;
            Status.ReValidate();

            var type = _selectionRow.Selection;
            if (type == AuthorizeType.Random)
            {
                _keyRow.Edit.IsReadOnly = false;

                AddIndex = options;
                AddInfoRow("RandomInfo");
                var r = AddButtonRow("NewRandom", NewRandom);
                r.SetDetailViewIcon(Icons.Random);

                _ = NewRandom(r);
            }
            else if (type == AuthorizeType.Passphrase)
            {
                _keyRow.Edit.IsReadOnly = false;

                AddIndex = options;
                AddInfoRow("PassphraseInfo", _minPassphraseLength);

                var button = AddButtonRow("NewPassphrase", NewPassphrase);
                button.RowStyle = Theme.SubmitButton;
                button.SetDetailViewIcon(Icons.Exchange);
                button.IsEnabled = false;

                var entry = AddEntryRow(null, "Passphrase", _minPassphraseLength);
                entry.SetDetailViewIcon(Icons.Unlock);
                entry.Edit.TextChanged += (a, evt) =>
                {
                    button.IsEnabled = evt.NewTextValue.Length >= _minPassphraseLength;
                };
            }
            else if (type == AuthorizeType.Derived)
            {
                _keyRow.Edit.IsReadOnly = true;

                AddIndex = options;

                AddInfoRow("DerivedInfo");

                var derivedView = new DerivedKeyView(null);
                AddViewRow(derivedView);

                var editor = AddEditorRow(null, "DerivedKeyInfo");
                editor.SetDetailViewIcon(Icons.Info);

                editor.Edit.TextChanged += async (a, evt) =>
                {
                    try
                    {
                        var data = Hex.ByteArrayFromCrcString(evt.NewTextValue);
                        if (data == null)
                            return;

                        var derivedKey = GetDerivedKey(evt.NewTextValue);
                        if(derivedKey.ChainId == _serviceNode.ChainId)
                        {
                            var key = await derivedKey.DecryptKey(_serviceNode.DerivedPassword);
                            if (key != null)
                            {
                                derivedView.Update(derivedKey);
                                _keyRow.Edit.Text = Hex.ToString(key.RawData).Substring(0, 64);

                                return;
                            }

                            await ErrorAsync("OutdatedDerivedKey");
                        }
                        else
                        {
                            //derivedView.Update(derivedKey);
                            await ErrorAsync("InvalidChainId");
                        }
                    }
                    catch
                    {
                        await ErrorAsync("InvalidDerivedKey");
                    }

                    _keyRow.Edit.Text = null;
                    derivedView.Update(null);
                };

                var p = AddButtonRow("Paste", Paste);
                p.SetDetailViewIcon(Icons.Clipboard);

                AddInfoRow("NewDerivedInfo");

                var n = AddButtonRow("NewDerived", NewDerived);
                n.RowStyle = Theme.SubmitButton;
                n.SetDetailViewIcon(Icons.RowLink);
            }
        }

        DerivedKey GetDerivedKey(string text)
        {
            try
            {
                var data = Hex.ByteArrayFromCrcString(text);
                if (data != null)
                {
                    using (var unpacker = new Unpacker(data))
                    {
                        return new DerivedKey(unpacker);
                    }
                }
            }
            catch { }

            return null;
        }

        async Task Paste(ButtonRow arg)
        {
            var row = GetRow<EditorRow>("DerivedKeyInfo");
            if(row != null)
            {
                var cb = await UIApp.CopyFromClipboard();
                if (cb != null)
                    row.Edit.Text = cb;
            }
        }

        Task NewDerived(ButtonRow button)
        {
            //if (!await ConfirmAsync("ConfirmDerived"))
                //return;

            _serviceNode.RequestDerived();
            return Task.CompletedTask;
        }

        async Task NewRandom(ButtonRow button)
        {
            IsBusy = true;
            button.IsEnabled = false;

            var secretKey = await RandomSecretKeyInfo.NewRandomSecretKey(_serviceNode.ChainId);
            _keyRow.Edit.Text = Hex.ToString(secretKey.SecretHash);
            Status.ReValidate();

            button.IsEnabled = true;
            IsBusy = false;
        }

        async Task NewPassphrase(ButtonRow button)
        {
            IsBusy = true;
            button.IsEnabled = false;

            var newEdit = GetRow<EntryRow>("Passphrase");

            var secretKey = await PassphraseSecretKeyInfo.NewPassphraseSecretKey(_serviceNode.ChainId, newEdit.Edit.Text);
            _keyRow.Edit.Text = Hex.ToString(secretKey.SecretHash);
            Status.ReValidate();

            button.IsEnabled = true;
            IsBusy = false;
        }

        Task SecretTypeChanged(AuthorizeType obj)
        {
            Update();
            return Task.CompletedTask;
        }
    }
}
