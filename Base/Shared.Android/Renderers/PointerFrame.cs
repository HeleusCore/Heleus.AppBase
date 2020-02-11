using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using static Android.Views.View;

using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(PointerFrame), typeof(Heleus.Apps.Shared.Android.Renderers.PointerFrameRenderer))]

namespace Heleus.Apps.Shared.Android.Renderers
{
	public class PointerFrameRenderer : ViewGroup, IVisualElementRenderer, IEffectControlProvider
	{
		readonly PointerEventHandlerAndroid touchEvent = new PointerEventHandlerAndroid();

		int? _defaultLabelFor;

		bool _disposed;
		ContentView _element;

		VisualElementPackager _visualElementPackager;
		VisualElementTracker _visualElementTracker;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		GradientDrawable _backgroundDrawable;

		public PointerFrameRenderer(Context context) : base(context)
		{
		}

		protected ViewGroup Control => this;

		protected ContentView Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				ContentView oldElement = _element;
				_element = value;

				OnElementChanged(new ElementChangedEventArgs<ContentView>(oldElement, _element));

				//_element?.SendViewInitialized(Control);
			}
		}

		VisualElement IVisualElementRenderer.Element => Element;
		ViewGroup IVisualElementRenderer.ViewGroup => this;
		AView IVisualElementRenderer.View => this;

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Context context = Context;
			return new SizeRequest(new Size(context.ToPixels(20), context.ToPixels(20)));
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			var frame = element as ContentView;
			if (frame == null)
				throw new ArgumentException("Element must be of type Frame");
			Element = frame;

			if (!string.IsNullOrEmpty(Element.AutomationId))
				ContentDescription = Element.AutomationId;
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = LabelFor;

			LabelFor = (int)(id ?? _defaultLabelFor);
		}

		VisualElementTracker IVisualElementRenderer.Tracker => _visualElementTracker;

		void IVisualElementRenderer.UpdateLayout()
		{
			VisualElementTracker tracker = _visualElementTracker;
			tracker?.UpdateLayout();
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (_visualElementTracker != null)
				{
					_visualElementTracker.Dispose();
					_visualElementTracker = null;
				}

				if (_visualElementPackager != null)
				{
					_visualElementPackager.Dispose();
					_visualElementPackager = null;
				}

				if (_backgroundDrawable != null)
				{
					_backgroundDrawable.Dispose();
					_backgroundDrawable = null;
				}

				var count = ChildCount;
				for (var i = 0; i < count; i++)
				{
					AView child = GetChildAt(i);
					child.Dispose();
				}

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.GetRenderer(Element) == this)
						Element.ClearValue(UIApp.RenderProperty);
				}
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<ContentView> e)
		{
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));

			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				this.EnsureId();

				_backgroundDrawable = new GradientDrawable();
				_backgroundDrawable.SetShape(ShapeType.Rectangle);

				var cornerRadius = Context.ToPixels(6);
				_backgroundDrawable.SetCornerRadius(cornerRadius);

				this.SetBackground(_backgroundDrawable);

				if (_visualElementTracker == null)
				{
					_visualElementTracker = new VisualElementTracker(this);
					_visualElementPackager = new VisualElementPackager(this);
					_visualElementPackager.Load();
				}

				touchEvent.Receiver = (e.NewElement as IPointerEventReceiver);
				e.NewElement.PropertyChanged += OnElementPropertyChanged;
				UpdateBackgroundColor();
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (Element == null)
				return;

			var children = ((IElementController)Element).LogicalChildren;
			for (var i = 0; i < children.Count; i++)
			{
				var visualElement = children[i] as VisualElement;
				if (visualElement == null)
					continue;
				IVisualElementRenderer renderer = Platform.GetRenderer(visualElement);
				renderer?.UpdateLayout();
			}
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (_disposed)
				return false;
			
			if (touchEvent.OnTouchEvent(e))
				return true;

			return false;
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);

			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}

		void UpdateBackgroundColor()
		{
			if (_disposed)
				return;

			Color bgColor = Element.BackgroundColor;
			_backgroundDrawable.SetColor(bgColor.IsDefault ? AColor.White : bgColor.ToAndroid());
		}
	}
}
