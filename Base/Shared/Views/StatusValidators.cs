using System;
using Heleus.Base;

namespace Heleus.Apps.Shared
{
    public static partial class StatusValidators
    {
        public static bool PasswordValidator(int passwordLength, StatusView statusView, ExtEntry edit, string newText, string oldText)
        {
            return !newText.IsNullOrWhiteSpace() && newText.Length >= passwordLength;
        }

        public static bool HexValidator(int hexLength, StatusView statusView, ExtEntry edit, string newText, string oldText)
        {
            try
            {
                if (newText == null)
                    return false;

                foreach (var c in newText)
                {
                    var isHex = ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
                    if (!isHex)
                    {
                        edit.Text = oldText;
                        return false;
                    }
                }

                if (newText.Length != hexLength)
                    return false;

                var data = Hex.FromString(newText);
                return data.Length == (hexLength / 2);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
            return false;
        }

        public static bool HexValidator(int hexLength, StatusView statusView, ExtEditor edit, string newText, string oldText)
        {
            try
            {
                if (newText == null)
                    return false;

                foreach (var c in newText)
                {
                    var isHex = ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
                    if (!isHex)
                    {
                        edit.Text = oldText;
                        return false;
                    }
                }

                if (newText.Length != hexLength)
                    return false;

                var data = Hex.FromString(newText);
                return data.Length == (hexLength / 2);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
            return false;
        }

        public static bool ValidUri(StatusView statusView, ExtEntry edit, string newText, string oldText)
        {
            return newText.IsValdiUrl(false);
        }

        public static bool ValidUriOrEmpty(StatusView statusView, ExtEntry edit, string newText, string oldText)
        {
            return newText.IsValdiUrl(true);
        }

        public static bool PositiveNumberValidator(StatusView statusView, ExtEntry edit, string newText, string oldText)
        {
            if (long.TryParse(newText, out var rec))
            {
                if (rec <= 0)
                {
                    edit.Text = oldText;
                    return false;
                }

                return true;
            }

            if (!newText.IsNullOrEmpty())
                edit.Text = oldText;

            return false;
        }

        public static bool PositiveNumberValidatorWithZero(StatusView statusView, ExtEntry edit, string newText, string oldText)
        {
            if (long.TryParse(newText, out var rec))
            {
                if (rec < 0)
                {
                    edit.Text = oldText;
                    return false;
                }

                return true;
            }

            if (!newText.IsNullOrEmpty())
                edit.Text = oldText;

            return false;
        }
        public static bool NoEmptyString(StatusView statusView, ExtEntry edit, string newText, string oldText)
        {
            return !string.IsNullOrWhiteSpace(newText);
        }

        public static bool IsToggled(StatusView statusView, ExtSwitch extSwitch, bool toggled)
        {
            return toggled;
        }
    }
}
