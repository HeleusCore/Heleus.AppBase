using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Text;
using Android.Text.Style;
using Heleus.Apps.Shared.Android;
using Xamarin.Forms.Platform.Android;

namespace Heleus.Apps.Shared
{
    public static class FirebaseNotification
    {
        internal static void HandlePushMessage(Context context, Firebase.Messaging.RemoteMessage remoteMessage)
        {
            NativeSettings.Init(context);
            AndroidStorage.Init(context);

            try
            {
                remoteMessage.Data.TryGetValue("scheme", out string scheme);
                remoteMessage.Data.TryGetValue("silent", out var silent);

                try
                {
                    if (!string.IsNullOrEmpty(scheme) && UIApp.Current != null)
                    {
                        var uri = new Uri(scheme);
                        UIApp.Run(() => UIApp.PubSub.PublishAsync(new PushNotificationEvent(uri, silent == "1" ? PushNotificationEventType.Silent : PushNotificationEventType.NoneUserInteraction)));
                    }
                }
                catch { }

                if (MainActivity.Active)
                    return;

                remoteMessage.Data.TryGetValue("logo", out string logo);
                remoteMessage.Data.TryGetValue("title", out string title);
                remoteMessage.Data.TryGetValue("message", out string message);

                if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(message))
                {
                    var settings = new FirebaseNotificationSettings();
                    settings.Load(context);

                    int defaults = 0;
                    if (settings.Vibrate)
                        defaults |= (int)NotificationDefaults.Vibrate;
                    if (settings.PlaySound && string.IsNullOrEmpty(settings.SoundUri))
                        defaults |= (int)NotificationDefaults.Sound;

                    Bitmap largeIcon = null;
                    if (!string.IsNullOrEmpty(logo))
                    {
                        try
                        {
                            var hash = logo.GetHashCode();
                            string logoPath = System.IO.Path.Combine(context.CacheDir.AbsolutePath, "logocache_" + hash);
                            if (File.Exists(logoPath))
                            {
                                largeIcon = BitmapFactory.DecodeFile(logoPath);
                            }
                            else
                            {
                                try
                                {
                                    var logoUrl = new Java.Net.URL(logo);
                                    var connection = logoUrl.OpenConnection();
                                    connection.ReadTimeout = connection.ConnectTimeout = 5000;
                                    using (var stream = connection.InputStream)
                                    {
                                        using (var memoryStream = new MemoryStream())
                                        {
                                            stream.CopyTo(memoryStream);

                                            var imageData = memoryStream.ToArray();
                                            largeIcon = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
                                            if (largeIcon != null)
                                            {
                                                File.WriteAllBytes(logoPath, imageData);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    global::Heleus.Base.Log.IgnoreException(ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            global::Heleus.Base.Log.IgnoreException(ex);
                        }
                    }

                    var nm = (NotificationManager)context.GetSystemService(Context.NotificationService);

                    const string NOTIFICATION_CHANNEL_ID = "channel";
                    /*
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        var notificationChannel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, "Notifications", NotificationImportance.Default);

                        // Configure the notification channel.
                        notificationChannel.Group = "group";
                        notificationChannel.Description = "Notifications";
                        if (settings.UseLed)
                        {
                            notificationChannel.EnableLights(true);
                            notificationChannel.LightColor = settings.LedColor.ToAndroid();
                        }
                        else
                        {
                            notificationChannel.EnableLights(false);
                        }

                        if(settings.Vibrate)
                        {
                            notificationChannel.EnableVibration(true);
                            notificationChannel.SetVibrationPattern(new long[] { 0, 1000, 500, 1000 });
                        }
                        else
                        {
                            notificationChannel.EnableVibration(false);
                        }

                        if(settings.PlaySound && !string.IsNullOrEmpty(settings.SoundUri))
                        {
                            notificationChannel.SetSound(global::Android.Net.Uri.Parse(settings.SoundUri), null);
                        }

                        nm.CreateNotificationChannel(notificationChannel);
                    }
                    */

                    var notification = new NotificationCompat.Builder(context, NOTIFICATION_CHANNEL_ID).
                         SetColor(Shared.Theme.SecondaryColor.Color.ToAndroid()).
                         SetSmallIcon(global::Heleus.Apps.Shared.Android.Resource.Drawable.notification).
                         SetDefaults(defaults).
                         SetAutoCancel(true);

                    if (largeIcon != null)
                        notification.SetLargeIcon(largeIcon);
                    else
                        notification.SetLargeIcon(BitmapFactory.DecodeResource(context.Resources, global::Heleus.Apps.Shared.Android.Resource.Mipmap.icon));

                    try
                    {
                        if (settings.PlaySound && !string.IsNullOrEmpty(settings.SoundUri))
                        {
                            notification.SetSound(global::Android.Net.Uri.Parse(settings.SoundUri));
                        }
                    }
                    catch (Exception ex)
                    {
                        global::Heleus.Base.Log.IgnoreException(ex);
                    }

                    if (settings.UseLed)
                        notification.SetLights(settings.LedColor.ToAndroid(), 300, 2000);

                    var notificationId = (title + message + DateTime.UtcNow.Ticks).GetHashCode();

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        var groupNotification = new NotificationCompat.Builder(context, NOTIFICATION_CHANNEL_ID).
                              SetColor(Shared.Theme.SecondaryColor.Color.ToAndroid()).
                              SetSmallIcon(global::Heleus.Apps.Shared.Android.Resource.Drawable.notification).
                              SetLargeIcon(BitmapFactory.DecodeResource(context.Resources, global::Heleus.Apps.Shared.Android.Resource.Mipmap.icon)).
                              SetAutoCancel(true);

                        groupNotification.SetGroupSummary(true);
                        groupNotification.SetGroup("group");
                        nm.Notify(1234, groupNotification.Build());
                    }

                    var sb = new SpannableString(title);
                    sb.SetSpan(new StyleSpan(TypefaceStyle.Bold), 0, title.Length, SpanTypes.ExclusiveExclusive);

                    notification.SetContentTitle(sb);
                    notification.SetContentText(message);
                    notification.SetGroup("group");

                    var openIntent = new Intent(context, typeof(MainActivity));
                    openIntent.SetAction(LocalFirebaseMessagingService.NotificationOpenSingleAction);
                    openIntent.PutExtra("scheme", scheme);
                    openIntent.PutExtra("notificationid", notificationId);

                    var pendingOpenIntent = PendingIntent.GetActivity(context, notificationId, openIntent, PendingIntentFlags.UpdateCurrent);
                    notification.SetContentIntent(pendingOpenIntent);

                    nm.Notify(notificationId, notification.Build());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                global::Heleus.Base.Log.IgnoreException(ex);
            }
        }
    }
}
