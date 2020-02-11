using System;
using Heleus.Base;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class ServiceAccountView : RowView
    {
        readonly ExtLabel _name;
        readonly ExtLabel _accountId;
        readonly ExtLabel _publicKey;

        public ServiceAccountView() : base("ClientAccountView")
        {
            (_, _name) = AddRow("Name", "-");
            (_, _accountId) = AddRow("AccountId", "-");
            (_, _publicKey) = AddLastRow("PublicKey", "-");
            _publicKey.FontStyle = Theme.DetailFont;
        }

        public ServiceAccountView(KeyStore account) : this()
        {
            Update(account);
        }

        public void Update(KeyStore account)
        {
            if (account == null)
            {
                Reset();
            }
            else
            {
                _publicKey.Text = Hex.ToString(account.PublicKey.RawData);
                _name.Text = account.Name;
                _accountId.Text = Tr.Get("Common.AccountIdTemplate", account.AccountId, account.KeyIndex);
            }
        }

        public void Reset()
        {
            _publicKey.Text = "-";
            _name.Text = "-";
            _accountId.Text = "-";
        }
    }

    public class ClientAccountView : RowView
	{
		readonly ExtLabel _name;
		readonly ExtLabel _accountId;
		readonly ExtLabel _type;
		readonly ExtLabel _chainId;
        readonly ExtLabel _expires;

		public ClientAccountView(bool showChainInfo = true, bool showExpires = true) : base("ClientAccountView")
		{
			(_, _name) = AddRow("Name", "-");
			(_, _type) = AddRow("Type", "-");

            if(!showChainInfo && !showExpires)
                (_, _accountId) = AddLastRow("AccountId", "-");
            else
                (_, _accountId) = AddRow("AccountId", "-");

			if (showChainInfo)
			{
                if(showExpires)
				    (_, _chainId) = AddRow("ChainId", "-");
                else
                    (_, _chainId) = AddLastRow("ChainId", "-");
            }

            if (showExpires)
                (_, _expires) = AddLastRow("Expires", "-");
		}

		public ClientAccountView(KeyStore account) : this(account.KeyStoreType != KeyStoreTypes.CoreAccount, false)
		{
			Update(account);
		}

		public void Update(KeyStore account)
		{
			if (account == null)
			{
				Reset();
			}
			else
			{
				_name.Text = account.Name;
				_accountId.Text = Tr.Get("Common.AccountIdTemplate", account.AccountId, account.KeyIndex);
                _type.Text = Tr.Get("KeyStoreTypes." + account.KeyStoreType.ToString());

                /*
                if (_expires != null)
                {
                    _expires.Text = account.Expires == 0 ? "-" : Time.DateTimeString(account.Expires);
                    _expires.ColorStyle = account.IsExpired() ? Theme.ErrorColor : Theme.TextColor;
                }
                */

				if(_chainId != null)
					_chainId.Text = account.ChainId.ToString();
			}
		}

		public void Reset()
		{
			_name.Text = "-";
			_accountId.Text = "-";
			_type.Text = "-";

            if (_expires != null)
            {
                _expires.Text = "-";
                _expires.ColorStyle = Theme.TextColor;
            }

			if(_chainId != null)
				_chainId.Text = "-";
		}
	}
}
