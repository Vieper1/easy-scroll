global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;

namespace EasyScroll
{
	[ProvideOptionPage(typeof(OptionsProvider.EasyScrollSettingsOptions), "EasyScroll", "General", 0, 0, true, SupportsProfiles = true)]
	internal sealed class EasyScrollPackage : ToolkitPackage
	{
		
	}

	[Export(typeof(IMouseProcessorProvider))]
	[Export(typeof(IKeyProcessorProvider))]
	[ContentType("text")]
	[TextViewRole(PredefinedTextViewRoles.Editable)]
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


	// Key processor
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


	// Mouse processor
	internal class EasyScrollMouseProcessor : MouseProcessorBase
	{
		private readonly IWpfTextView WpfTextView;

		private Point LastMousePoint;
		private bool IsScrolling = false;

		internal EasyScrollMouseProcessor(IWpfTextView wpfTextView) {
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
			double scrollRate = EasyScrollSettings.Instance.ScrollRate;
			double zoomLevel = WpfTextView.ZoomLevel / 100.0;


			// Vertical scroll
			double scrolLDistY = (currentMousePoint.Y - LastMousePoint.Y) / zoomLevel * scrollRate;
			WpfTextView.ViewScroller.ScrollViewportVerticallyByPixels(scrolLDistY);


			// Horizontal scroll if any
			if (EasyScrollKeyProcessor.GetIsShiftKeyPressed() || EasyScrollSettings.Instance.EasyScrollMode == EasyScrollMode.BothWays)
			{
				double scrollDistX = (LastMousePoint.X - currentMousePoint.X) / zoomLevel * scrollRate;
				WpfTextView.ViewScroller.ScrollViewportHorizontallyByPixels(scrollDistX);
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