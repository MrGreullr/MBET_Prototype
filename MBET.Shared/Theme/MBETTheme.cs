using MudBlazor;

namespace MBET.Shared.Theme
{
    public static class MBETTheme
    {
        public static MudTheme DefaultTheme = new MudTheme()
        {
            // --- EXISTING DARK THEME (Future Tech) ---
            PaletteDark = new PaletteDark()
            {
                // Brand Colors: High-Vis Orange & Electric Blue
                Primary = "#ff6b00",
                PrimaryDarken = "#cc5500",
                Secondary = "#0082ff",
                Tertiary = "#22c55e",

                // Deep Space Backgrounds
                Background = "#05060b",     // Pure Black/Navy
                Surface = "#0b0e14",        // Obsidian
                AppbarBackground = "rgba(5, 6, 11, 0.8)",
                DrawerBackground = "#0b0e14",

                // Text & Actions
                TextPrimary = "#ffffff",
                TextSecondary = "rgba(255,255,255, 0.5)",
                ActionDefault = "#ffffff",
                Divider = "rgba(255,255,255, 0.05)",
                LinesDefault = "rgba(255,255,255, 0.05)",
            },

            // --- RICH "DESERT TECH" LIGHT THEME ---
            PaletteLight = new PaletteLight()
            {
                // Brand: High-Vis Orange popping against warm earthy tones
                Primary = "#ff6b00",
                PrimaryDarken = "#cc5500",
                Secondary = "#1a237e",      // Midnight Blue (High contrast anchor)
                Tertiary = "#2e7d32",       // Tactical Green

                // Foundation: Rich Beige & Smoked Depth
                Background = "#f5f2eb",     // Bone / Sand (Warm, rich base)
                Surface = "#ebe5da",        // Smoked Beige (Distinct depth for cards)

                // Navigation & Structure
                AppbarBackground = "#e3dac9", // Almond/Champagne (Clearly separates header)
                DrawerBackground = "#ebe5da", // Matches Surface for cohesive sidebar

                // Text & Contrast (Charcoal/Coffee tones instead of pure black)
                TextPrimary = "#2d2a26",    // Deep Charcoal (Softer than black, readable)
                TextSecondary = "#5c554f",  // Earthy Grey
                ActionDefault = "#2d2a26",  // Dark icons for visibility

                // Accents
                Divider = "rgba(45, 42, 38, 0.12)", // Subtle earthy divider
                LinesDefault = "rgba(45, 42, 38, 0.12)",
                TableLines = "rgba(45, 42, 38, 0.12)",

                // States
                HoverOpacity = 0.10,
                RippleOpacity = 0.10
            },

            // Shared Typography
            Typography = new Typography()
            {
                Default = new DefaultTypography()
                {
                    FontFamily = new[] { "Plus Jakarta Sans", "Manrope", "sans-serif" },
                    FontSize = "0.9rem",
                    LineHeight = "1.5"
                },
                H1 = new H1Typography() { FontSize = "4.5rem", FontWeight = "800", LineHeight = "1.1", LetterSpacing = "-0.03em" },
                H2 = new H2Typography() { FontSize = "3rem", FontWeight = "800", LineHeight = "1.2", LetterSpacing = "-0.02em" },
                H3 = new H3Typography() { FontSize = "2rem", FontWeight = "700" },
                Button = new ButtonTypography() { FontWeight = "700", LetterSpacing = "0.05em", TextTransform = "uppercase" },
                Caption = new CaptionTypography() { FontWeight = "700", LetterSpacing = "0.1em", TextTransform = "uppercase" }
            },
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "24px",
                AppbarHeight = "80px"
            }
        };
    }
}