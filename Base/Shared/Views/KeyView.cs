using Heleus.Base;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class KeyView : RowView
    {
        readonly ExtLabel _private;
        readonly ExtLabel _public;

        public KeyView(Key key, bool forcePrivate) : base("KeyView")
        {
            if (forcePrivate || (key != null && key.IsPrivate))
            {
                (_, _private) = AddRow("Private", null);
                _private.FontStyle = Theme.MicroFont;
            }
            (_, _public) = AddLastRow("Public", null);
            _public.FontStyle = Theme.MicroFont;

            Update(key);
        }

        public void Update(Key key)
        {
            if (key == null)
            {
                if (_private != null)
                    _private.Text = "-";

                _public.Text = "-";
                return;
            }

            if (_private != null)
            {
                if (key.IsPrivate)
                    _private.Text = Hex.ToString(key.RawData).Substring(0, 64);
                else
                    _private.Text = "-";
            }

            _public.Text = Hex.ToString(key.PublicKey.RawData);
        }
    }
}
