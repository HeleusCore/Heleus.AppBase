using System;
using System.IO;
using System.Threading.Tasks;
using Heleus.Base;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    class ImageSelectionPage : StackPage
    {
        readonly AbsoluteLayout _imageLayout = new AbsoluteLayout();
        readonly PointerFrame _imageFrame = new PointerFrame();
        readonly ExtImage _imageView = new ExtImage();
        readonly ExtBoxView[] _boxes = new ExtBoxView[4];
        readonly Func<ImageHandler, Task> _callback;

        ImageHandler _image;

        double _boxSize = 0.9;
        double _boxPositionX = 0.5;
        double _boxPositionY = 0.5;
        double _boxPointerDownX;
        double _boxPointerDownY;

        public async static Task OpenImagePicker(Func<ImageHandler, Task> callback, Xamarin.Forms.Page page = null)
        {
            await UIApp.OpenImagePicker(async (image) =>
            {
                await (page ?? UIApp.Current.CurrentPage).Navigation.PushAsync(new ImageSelectionPage(image, callback));
            });
        }

        public ImageSelectionPage(ImageHandler image, Func<ImageHandler, Task> callback) : base("ImageSelectionPage")
        {
            _image = image;
            _callback = callback;

            AddTitleRow("Title");
            ScrollView.DisableTouchCanel = true;

            AddHeaderRow();

            _imageView.InputTransparent = true;
            _imageView.Aspect = Aspect.AspectFill;

            AbsoluteLayout.SetLayoutFlags(_imageView, AbsoluteLayoutFlags.SizeProportional);
            AbsoluteLayout.SetLayoutBounds(_imageView, new Rectangle(0, 0, 1, 1));

            _imageLayout.Children.Add(_imageView);
            _imageLayout.InputTransparent = true;
            for (var i = 0; i < _boxes.Length; i++)
            {
                var box = _boxes[i] = new ExtBoxView();

                box.WidthRequest = box.HeightRequest = 0;
                box.BackgroundColor = Color.Black.MultiplyAlpha(0.75);
                box.InputTransparent = true;

                if (i == 4)
                    box.BackgroundColor = Color.Black.MultiplyAlpha(0.5);

                _imageLayout.Children.Add(box);
            }

            _imageFrame.SetPointerFrameType(PointerFrameType.Movement);
            _imageFrame.Content = _imageLayout;
            _imageFrame.SizeChanged += ImageFrame_SizeChanged;
            _imageFrame.MovementHandler.OnPointerDown = PointerDown;
            _imageFrame.MovementHandler.OnPointerMoved = PointerMoved;
            _imageFrame.BackgroundColor = Color.Transparent;

            AddView(_imageFrame);

            var slider = AddSimpleSliderRow((int)(255 * _boxSize), 0, 255);
            slider.NewValue = (newValue) =>
            {
                _boxSize = 0.1 + (newValue / 255.0) * 0.9;
                UpdateBox();
            };

            AddButtonRow("SelectImage", SelectImage);
            AddFooterRow();

            AddSubmitRow("Select", Select);
        }

        public override async Task InitAsync()
        {
            var img = _image;
            _image = null;
            await ImageHandlerChanged(img);
        }

        async Task ImageHandlerChanged(ImageHandler image)
        {
            _image?.Dispose();
            _image = null;

            if (image == null)
            {
                _imageView.Source = null;
                ImageFrame_SizeChanged(null, null);
                return;
            }

            try
            {
                // use small preview image
                if (image.Width > 750 || image.Height > 750)
                {
                    using (var img = await image.Resize(750))
                    {
                        var data = await img.Save();
                        _imageView.Source = ImageSource.FromStream(() => new MemoryStream(data));
                    }
                }
                else
                {
                    var data = await image.Save();
                    _imageView.Source = ImageSource.FromStream(() => new MemoryStream(data));
                }

                _image = image;
                ImageFrame_SizeChanged(null, null);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

        async Task SelectImage(ButtonRow button)
        {
            await UIApp.OpenImagePicker(async (image) =>
            {
                await ImageHandlerChanged(image);
            });
        }

        async Task Select(ButtonRow button)
        {
            if (_image == null)
                return;

            var croppedSize = (int)(Math.Min(_image.Width, _image.Height) * _boxSize);

            var x = (int)(_image.Width * _boxPositionX) - croppedSize / 2;
            var y = (int)(_image.Height * _boxPositionY) - croppedSize / 2;

            using (var cutImage = await _image.CopySection(x, y, croppedSize, croppedSize))
            {
#if DEBUG
                File.WriteAllBytes(Path.Combine(StorageInfo.DocumentStorage.RootPath, "ImageSelectionPage." + (cutImage.IsOpaque ? "jpg" : "png")), await cutImage.Save(80));
#endif

                if(_callback != null)
                    await _callback?.Invoke(cutImage);
            }

            await Navigation.PopAsync();
        }

        void UpdateBox()
        {
            if (_imageFrame.Width <= 0 || _imageFrame.Height <= 0)
                return;

            var boxSizeInPX = (int)(Math.Min(_imageFrame.Height, _imageFrame.Width) * _boxSize);
            var boxHalfSize = boxSizeInPX / 2;

            var centerX = Math.Max(boxHalfSize, Math.Min((int)(_imageFrame.Width * _boxPositionX), (int)(_imageFrame.Width - boxHalfSize)));
            var centerY = Math.Max(boxHalfSize, Math.Min((int)(_imageFrame.Height * _boxPositionY), (int)(_imageFrame.Height - boxHalfSize)));

            _boxPositionX = centerX / _imageFrame.Width;
            _boxPositionY = centerY / _imageFrame.Height;

            // -------------------------------
            // |                             | box0
            // -------------------------------
            // |---|                     |---|
            // |   | box1     x          |---| box2
            // |---|                     |---|
            // ------------------------------- box3

            var width = (int)_imageFrame.Width;
            var height = (int)_imageFrame.Height;

            //AbsoluteLayout.SetLayoutBounds(boxes[4], new Rectangle(centerX - boxSizeInPX / 2, centerY - boxSizeInPX / 2, boxSizeInPX, boxSizeInPX));

            var pad = 0;

            var size0 = centerY - boxHalfSize;
            _boxes[0].IsVisible = (size0 > 0);
            AbsoluteLayout.SetLayoutBounds(_boxes[0], new Rectangle(0, 0, width, centerY - boxHalfSize));

            var size1 = centerX - boxHalfSize;
            _boxes[1].IsVisible = (size1 > 0);
            AbsoluteLayout.SetLayoutBounds(_boxes[1], new Rectangle(0, centerY - boxHalfSize + pad, centerX - boxHalfSize, boxSizeInPX - 2 * pad));

            var size2 = width - centerX - boxHalfSize;
            _boxes[2].IsVisible = (size2 > 0);
            AbsoluteLayout.SetLayoutBounds(_boxes[2], new Rectangle(centerX + boxHalfSize, centerY - boxHalfSize + pad, width - centerX - boxHalfSize, boxSizeInPX - 2 * pad));

            var size3 = height - centerY - boxHalfSize;
            _boxes[3].IsVisible = (size3 > 0);
            AbsoluteLayout.SetLayoutBounds(_boxes[3], new Rectangle(0, size0 + boxSizeInPX, width, height - centerY - boxHalfSize));
        }

        void PointerDown(PointerPositionEvent e)
        {
            _boxPointerDownX = _boxPositionX;
            _boxPointerDownY = _boxPositionY;
        }

        void PointerMoved(PointerPositionEvent e)
        {
            if (_imageFrame.Width <= 0 || _imageFrame.Height <= 0)
                return;

            _boxPositionX = _boxPointerDownX + (e.LocalX - e.LocalStartX) / _imageFrame.Width;
            _boxPositionY = _boxPointerDownY + (e.LocalY - e.LocalStartY) / _imageFrame.Height;

            UpdateBox();
        }

        void ImageFrame_SizeChanged(object sender, EventArgs e)
        {
            var aspect = 1.0;
            if (_image != null)
                aspect = _image.Aspect;

            if (_imageFrame.Width <= 0)
                return;

            _imageFrame.HeightRequest = _imageFrame.Width / aspect;
            UpdateBox();
        }

        public override void OnPopped()
        {
            base.OnPopped();

            try
            {
                _image?.Dispose();
                _image = null;
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }
    }
}
