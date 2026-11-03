using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using MDRNJfitness.Helpers;
using MDRNJfitness.Helpers.Converters;

namespace MDRNJfitness.Views;

public partial class WorkoutTypePickerPage : ContentPage
{
    private static readonly Dictionary<string, string> Hints = new()
    {
        [WorkoutTypes.Cardio] = "Mostly trains END.",
        [WorkoutTypes.Flexibility] = "Mostly trains FLX.",
        [WorkoutTypes.Strength] = "Mostly trains STR.",
        [WorkoutTypes.Sports] = "A mix of all three stats.",
    };

    private readonly WorkoutType _current;
    private WorkoutCategory _shown;
    private WorkoutType? _picked;
    private bool _closing;

    private WorkoutTypePickerPage(WorkoutType current)
    {
        InitializeComponent();
        _current = current;
        _shown = WorkoutTypes.CategoryOf(current) ?? WorkoutTypes.Categories[0];
        ShowCategory(_shown);
    }

    /// <summary>Opens the picker; returns the chosen type, or null if it was closed without one.</summary>
    public static async Task<WorkoutType?> PickAsync(string? current)
    {
        var start = Enum.TryParse<WorkoutType>(current, true, out var parsed) ? parsed : WorkoutType.Running;
        var page = new WorkoutTypePickerPage(start);
        await ModalPages.PushAndWaitAsync(page);
        return page._picked;
    }

    private void ShowCategory(WorkoutCategory category)
    {
        _shown = category;
        CategoryHint.Text = Hints.GetValueOrDefault(category.Name, "");
        CategoryHint.TextColor = WorkoutCategoryStyle.Color(category.Name);

        CategoryGrid.Children.Clear();
        for (var i = 0; i < WorkoutTypes.Categories.Count; i++)
        {
            var tile = CategoryTile(WorkoutTypes.Categories[i]);
            CategoryGrid.Add(tile, i % 2, i / 2);
        }

        TypeGrid.Children.Clear();
        TypeGrid.RowDefinitions.Clear();
        for (var i = 0; i < category.Types.Count; i++)
        {
            if (i % 2 == 0) TypeGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            TypeGrid.Add(TypeOption(category, category.Types[i]), i % 2, i / 2);
        }
    }

    private View CategoryTile(WorkoutCategory category)
    {
        var color = WorkoutCategoryStyle.Color(category.Name);
        var selected = category == _shown;

        var tile = new Border
        {
            Style = Resource<Style>("PxSunken"),
            BackgroundColor = selected ? color : Resource<Color>("PxPanelSunken"),
            Stroke = selected ? Resource<Color>("PxInk") : color,
            StrokeThickness = selected ? 3 : 2,
            Padding = new Thickness(8, 14),
            Content = new Label
            {
                Style = Resource<Style>("PxCaption"),
                FontSize = 9,
                Text = category.Name.ToUpperInvariant(),
                TextColor = selected ? Resource<Color>("PxInk") : color,
                HorizontalOptions = LayoutOptions.Center
            }
        };
        Tap(tile, () => ShowCategory(category));
        return tile;
    }

    private View TypeOption(WorkoutCategory category, WorkoutType type)
    {
        var color = WorkoutCategoryStyle.Color(category.Name);
        var current = type == _current;

        var option = new Border
        {
            Style = Resource<Style>("PxSunken"),
            BackgroundColor = current ? Resource<Color>("PxPanelRaised") : Resource<Color>("PxPanelSunken"),
            Stroke = current ? color : Resource<Color>("PxInk"),
            StrokeThickness = current ? 3 : 2,
            Padding = new Thickness(10, 10),
            MinimumHeightRequest = 52,
            Content = new Label
            {
                Style = Resource<Style>("PxBody"),
                FontSize = 21,
                Text = (current ? "✔ " : "") + WorkoutTypes.DisplayName(type),
                TextColor = current ? color : Resource<Color>("PxText"),
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap
            }
        };
        Tap(option, async () =>
        {
            _picked = type;
            await CloseAsync();
        });
        return option;
    }

    private static void Tap(View view, Action onTap)
    {
        var gesture = new TapGestureRecognizer();
        gesture.Tapped += (_, _) => onTap();
        view.GestureRecognizers.Add(gesture);
    }

    private async Task CloseAsync()
    {
        if (_closing) return; // a double tap would pop the page underneath too
        _closing = true;
        await Navigation.PopModalAsync();
    }

    private async void OnCloseClicked(object? sender, EventArgs e) => await CloseAsync();

    private static T Resource<T>(string key) =>
        Application.Current!.Resources.TryGetValue(key, out var value) && value is T typed ? typed : default!;
}
