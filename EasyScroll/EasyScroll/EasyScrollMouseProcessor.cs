using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace EasyScroll
{
    [Export(typeof(IMouseProcessorProvider))]
	[Export(typeof(IKeyProcessorProvider))]
	[ContentType("text")]
	[TextViewRole(PredefinedTextViewRoles.Interactive)]
	[Name("EasyScroll")]
	internal class EasyScrollProcessorProvider : IMouseProcessorProvider, IKeyProcessorProvider
	{
        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            return new EasyScrollKeyProcessor(wpfTextView);
        }

        IMouseProcessor IMouseProcessorProvider.GetAssociatedProcessor(IWpfTextView wpfTextView)
		{
			return new EasyScrollMouseProcessor(wpfTextView);
		}
	}

    internal class EasyScrollKeyProcessor : KeyProcessor
    {
		private static KeyboardDevice Device;

        public EasyScrollKeyProcessor(IWpfTextView wpfTextView) {}

        public override void KeyDown(KeyEventArgs args)
        {
			Device = args.KeyboardDevice;
			base.KeyDown(args);
        }

		public static bool GetIsShiftKeyPressed()
        {
			if (Device == null) return false;
			return Device.IsKeyDown(Key.LeftShift) || Device.IsKeyDown(Key.RightShift);
        }
    }

    internal class EasyScrollMouseProcessor : MouseProcessorBase
	{
		public static bool IsShiftPressed = false;

		private readonly IWpfTextView WpfTextView;

		private Point LastMousePoint;
		private bool IsScrolling = false;

		internal EasyScrollMouseProcessor(IWpfTextView wpfTextView)
		{
			WpfTextView = wpfTextView;
		}

		public override void PreprocessMouseDown(MouseButtonEventArgs args)
        {
            if (args.ChangedButton == MouseButton.Middle)
			{
				LastMousePoint = args.GetPosition(null);
				IsScrolling = true;
			}
        }

        public override void PreprocessMouseUp(MouseButtonEventArgs args)
        {
            if (args.ChangedButton == MouseButton.Middle)
			{
				IsScrolling = false;
			}
        }

        public override void PreprocessMouseEnter(MouseEventArgs args)
        {
			LastMousePoint = args.GetPosition(null);
            base.PreprocessMouseEnter(args);
        }

        public override void PreprocessMouseMove(MouseEventArgs args)
        {
			if (args.MiddleButton != MouseButtonState.Pressed)
			{
				IsScrolling = false;
				return;
			}
			IsScrolling = true;

            Point currentMousePoint = args.GetPosition(null);

			double verticalDist = currentMousePoint.Y - LastMousePoint.Y;
			double verticalDistScaled = verticalDist / WpfTextView.ZoomLevel * 100.0;
			WpfTextView.ViewScroller.ScrollViewportVerticallyByPixels(verticalDistScaled);

			if (EasyScrollKeyProcessor.GetIsShiftKeyPressed())
            {
				double horizontalDist = LastMousePoint.X - currentMousePoint.X;
				double horizontalDistScaled = horizontalDist / WpfTextView.ZoomLevel * 100.0;
				WpfTextView.ViewScroller.ScrollViewportHorizontallyByPixels(horizontalDistScaled);
            }

			LastMousePoint = currentMousePoint;
			args.Handled = true;
        }

		public override void PreprocessMouseWheel(MouseWheelEventArgs args)
		{
			if (!IsScrolling)
			{
				base.PreprocessMouseWheel(args);
				return;
			}

			args.Handled = true;
		}
	}
}
