using System;
using Heleus.Base;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class SecretKeyView : RowView
    {
        readonly ExtLabel _id;
        readonly ExtLabel _bla;
        readonly ExtLabel _chainId;
        readonly ExtLabel _ts;

        readonly ExtLabel _account;

#if DEBUG
        readonly ExtLabel _active;
#endif

        public SecretKeyView(SecretKeyInfo secretKeyInfo, bool showDetails) : base("SecretKeyView")
        {
#if DEBUG
            (_, _active) = AddRow("Active", "-");
#endif

            (_, _id) = AddRow("Id", null);
            (_, _bla) = AddRow("Type", null);

            if (showDetails)
            {
                (_, _chainId) = AddRow("ChainId", null);
                (_, _account) = AddRow("AccountId", null);
            }

            (_, _ts) = AddLastRow("Timestamp", null);

            Update(secretKeyInfo);
        }

#if DEBUG
        public void UpdateActive(bool active)
        {
            if (_active != null)
                _active.Text = active ? "active" : "inactive";
        }
#endif

        public void Update(SecretKeyInfo secretKeyInfo)
        {
            if (secretKeyInfo == null)
            {
                _id.Text = "-";
                _bla.Text = "-";
                if (_chainId != null)
                    _chainId.Text = "-";
                if (_account != null)
                    _account.Text = "-";
                _ts.Text = "-";
            }
            else
            {
                _id.Text = secretKeyInfo.SecretId.ToString();
                _bla.Text = Tr.Get($"SecretKeyInfoTypes.{secretKeyInfo.SecretKeyInfoType}");
                if (_chainId != null)
                    _chainId.Text = secretKeyInfo.ChainId.ToString();
                _ts.Text = Time.DateTimeString(secretKeyInfo.Timestamp);

                if (_account != null)
                {
                    if (secretKeyInfo is AccountSecretKeyInfo accountSecretKeyInfo)
                        _account.Text = Tr.Get("Common.AccountIdTemplate", accountSecretKeyInfo.AccountId, accountSecretKeyInfo.KeyIndex);
                    else if (secretKeyInfo is KeyExchageSecretKeyInfo keyExchageSecretKeyInfo)
                        _account.Text = $"{Tr.Get("Common.AccountIdTemplate", keyExchageSecretKeyInfo.Account1, keyExchageSecretKeyInfo.KeyIndex1)}, {Tr.Get("Common.AccountIdTemplate", keyExchageSecretKeyInfo.Account2, keyExchageSecretKeyInfo.KeyIndex2)}";
                    else
                        _account.Text = "-";
                }
            }
        }
    }

}
