using System.ComponentModel;
using System.Runtime.InteropServices;

namespace EasyScroll
{
	internal partial class OptionsProvider
	{
		// Register the options with this attribute on your package class:
		// [ProvideOptionPage(typeof(OptionsProvider.EasyScrollSettingsOptions), "EasyScroll", "EasyScrollSettings", 0, 0, true, SupportsProfiles = true)]
		[ComVisible(true)]
		public class EasyScrollSettingsOptions : BaseOptionPage<EasyScrollSettings> { }
	}

	public enum EasyScrollMode
    {
        VerticalOnly,
        BothWays
    }

	public class EasyScrollSettings : BaseOptionModel<EasyScrollSettings>
	{
		[Category("General")]
        [DisplayName("Scroll Mode")]
        [Description("Choose between scrolling vertical or both horizontal and vertical.")]
        [DefaultValue(EasyScrollMode.VerticalOnly)]
        public EasyScrollMode EasyScrollMode { get; set; } = EasyScrollMode.VerticalOnly;

		private double scrollRate = 1.0;
		[Category("General")]
        [DisplayName("Scroll Rate")]
        [Description("Choose how much to scroll")]
        [DefaultValue(1.0)]
        public double ScrollRate
		{
			get
			{
				return scrollRate;
			}
			set
			{
				scrollRate = value < 0.001 ? 0.001 : (value > 1000.0 ? 1000.0 : value);
			}
		}
	}
}
