using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace Groestlcoin_VanityGen_UI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
            Swatches = new SwatchesProvider().Swatches;
        }

        public Swatch DarkPrimary => Swatches.First(d => d.Name == "indigo");
        public Swatch LightPrimary => Swatches.First(d => d.Name == "teal");

        public void ApplyBase(bool isDark) {
            new PaletteHelper().SetLightDark(isDark);
            //SetLightDark(isDark);
        }

        public static class StringExtensions {
            public static bool ContainsAny(string input, IEnumerable<string> containsKeywords, StringComparison comparisonType) {
                return containsKeywords.Any(keyword => input.IndexOf(keyword, comparisonType) >= 0);
            }
        }

        public IEnumerable<Swatch> Swatches { get; }


        public void ApplyPrimary(Swatch swatch) {
            new PaletteHelper().ReplacePrimaryColor(swatch);
        }


        public void ApplyAccent(Swatch swatch) {
            new PaletteHelper().ReplaceAccentColor(swatch);
        }



    }
}
